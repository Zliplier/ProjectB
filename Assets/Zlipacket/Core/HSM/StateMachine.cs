using System;
using System.Collections;
using System.Collections.Generic;

namespace Zlipacket.Core.HSM
{
    /// <summary>
    /// A generic hierarchical state machine that can drive anything (TOwner).
    /// States are identified by their Type, so usage looks like:
    ///
    ///     FSM.AddState&lt;IdleState&gt;();
    ///     FSM.ChangeState&lt;MoveState&gt;();
    ///     if (FSM.IsInState&lt;AttackState&gt;()) ...
    ///     yield return FSM.WaitForState&lt;AttackState&gt;();
    ///
    /// Features:
    ///  - True hierarchy: parent/child states, shared Enter/Exit only fires
    ///    for the branch that actually changes (like Unity's Animator sub-state
    ///    machines, but code-driven).
    ///  - Three ways to transition, mix and match:
    ///      1) Manual:      FSM.ChangeState&lt;TState&gt;()
    ///      2) Declarative: override State.GetTransition() and return a type
    ///      3) Triggers:    FSM.AddTransition&lt;TFrom,TTo&gt;("trigger"); FSM.Fire("trigger");
    ///  - History stack of previous leaf states, with GoToPreviousState().
    ///  - Coroutine helpers to await a state, wait while in a state, or wait
    ///    for the next transition of any kind.
    /// </summary>
    public class StateMachine<TOwner>
    {
        private readonly TOwner owner;
        private readonly Dictionary<Type, State<TOwner>> states = new Dictionary<Type, State<TOwner>>();
        private readonly List<State<TOwner>> activeChain = new List<State<TOwner>>(); // root -> leaf
        private readonly Stack<Type> history = new Stack<Type>();
 
        private readonly List<(Func<bool> condition, Type target)> anyConditionTransitions =
            new List<(Func<bool>, Type)>();
        private readonly Dictionary<(Type from, string trigger), Type> triggerTransitions =
            new Dictionary<(Type, string), Type>();
        private readonly Dictionary<string, Type> anyTriggerTransitions = new Dictionary<string, Type>();
 
        private event Action transitionOccurred;
 
        /// Cap on how many previous states are remembered. 0 = unlimited.
        public int MaxHistory = 32;
 
        /// Machine-local clock, advanced by calling Tick(deltaTime).
        public float Time { get; private set; }
 
        public State<TOwner> CurrentState => activeChain.Count > 0 ? activeChain[activeChain.Count - 1] : null;
        public State<TOwner> PreviousState { get; private set; }
        public bool IsTransitioning { get; private set; }
 
        /// Fired whenever the leaf state changes: (from, to). "from" is null on the very first Start().
        public event Action<State<TOwner>, State<TOwner>> StateChanged;
 
        public StateMachine(TOwner owner)
        {
            this.owner = owner;
        }
 
        // ==================== Registration ====================
 
        public TState AddState<TState>() where TState : State<TOwner>, new()
            => AddState<TState>((State<TOwner>)null);
 
        public TState AddState<TState, TParent>()
            where TState : State<TOwner>, new()
            where TParent : State<TOwner>
            => AddState<TState>(GetState<TParent>());
 
        public TState AddState<TState>(State<TOwner> parent) where TState : State<TOwner>, new()
        {
            var type = typeof(TState);
            if (states.ContainsKey(type))
                throw new InvalidOperationException($"State '{type.Name}' is already registered.");
 
            var state = new TState();
            state.Init(owner, this, parent);
            states[type] = state;
            return state;
        }
 
        public TState GetState<TState>() where TState : State<TOwner>
        {
            states.TryGetValue(typeof(TState), out var s);
            return s as TState;
        }
 
        public bool HasState<TState>() where TState : State<TOwner> => states.ContainsKey(typeof(TState));
 
        // ==================== Optional declarative transitions ====================
 
        /// Global rule checked every Tick regardless of current state: if condition()
        /// is true, jump to TTarget. Checked before per-state GetTransition().
        public void AddAnyTransition<TTarget>(Func<bool> condition) where TTarget : State<TOwner>
            => anyConditionTransitions.Add((condition, typeof(TTarget)));
 
        /// Register a trigger-based transition scoped to one state (like an Animator trigger).
        public void AddTransition<TFrom, TTo>(string trigger)
            where TFrom : State<TOwner>
            where TTo : State<TOwner>
            => triggerTransitions[(typeof(TFrom), trigger)] = typeof(TTo);
 
        /// Register a trigger that works no matter which state is currently active.
        public void AddAnyTrigger<TTo>(string trigger) where TTo : State<TOwner>
            => anyTriggerTransitions[trigger] = typeof(TTo);
 
        /// Shared lookup used by both Fire and CanFire: checks the active
        /// chain leaf-to-root first (so a child's mapping wins over its
        /// parent's), then falls back to any-state triggers.
        private bool TryGetTriggerTarget(string trigger, out Type target)
        {
            for (int i = activeChain.Count - 1; i >= 0; i--)
            {
                if (triggerTransitions.TryGetValue((activeChain[i].GetType(), trigger), out target))
                    return true;
            }
            return anyTriggerTransitions.TryGetValue(trigger, out target);
        }
 
        /// True if Fire(trigger) would find a matching transition right now,
        /// without actually firing it. Use this to gate things like input
        /// buffers instead of checking CurrentState's type from outside.
        public bool CanFire(string trigger) => TryGetTriggerTarget(trigger, out _);
 
        /// Fires a trigger. Checks the active chain leaf-to-root first (so a child's
        /// mapping wins over its parent's), then falls back to any-state triggers.
        /// Returns true if a matching transition was found. By default, firing a
        /// trigger that targets the state you're already in is a no-op (same as
        /// ChangeState) - pass force: true to re-enter it anyway (re-runs
        /// OnExit -> OnEnter), which is what you want for things like a hit
        /// reaction that should reset its timer on every hit, even repeated ones.
        public bool Fire(string trigger, bool force = false)
        {
            if (TryGetTriggerTarget(trigger, out var target))
            {
                ChangeStateInternal(target, force: force);
                return true;
            }
            return false;
        }
 
        // ==================== Starting / driving ====================
 
        public void Start<TState>() where TState : State<TOwner>
            => ChangeStateInternal(typeof(TState), recordHistory: false);
 
        /// Call from MonoBehaviour.Update(). Advances the clock, updates the
        /// active chain leaf-to-root, then checks for automatic transitions.
        public void Tick(float deltaTime)
        {
            Time += deltaTime;
            if (activeChain.Count == 0) return;
 
            for (int i = activeChain.Count - 1; i >= 0; i--)
                activeChain[i].OnUpdate();
 
            CheckAutomaticTransitions();
        }
 
        /// Call from MonoBehaviour.FixedUpdate().
        public void FixedTick()
        {
            for (int i = activeChain.Count - 1; i >= 0; i--)
                activeChain[i].OnFixedUpdate();
        }
 
        /// Call from MonoBehaviour.LateUpdate().
        public void LateTick()
        {
            for (int i = activeChain.Count - 1; i >= 0; i--)
                activeChain[i].OnLateUpdate();
        }
 
        private void CheckAutomaticTransitions()
        {
            foreach (var (condition, target) in anyConditionTransitions)
            {
                if (condition())
                {
                    ChangeStateInternal(target);
                    return; // one transition per Tick keeps things predictable
                }
            }
 
            for (int i = activeChain.Count - 1; i >= 0; i--)
            {
                var next = activeChain[i].GetTransition();
                if (next != null)
                {
                    ChangeStateInternal(next);
                    return;
                }
            }
        }
 
        // ==================== Manual transitions ====================
 
        public void ChangeState<TState>() where TState : State<TOwner>
            => ChangeStateInternal(typeof(TState));
 
        /// Same as ChangeState, but re-enters even if it's already the current leaf.
        public void ForceChangeState<TState>() where TState : State<TOwner>
            => ChangeStateInternal(typeof(TState), force: true);
 
        private void ChangeStateInternal(Type targetType, bool recordHistory = true, bool force = false)
        {
            if (!states.TryGetValue(targetType, out var target))
                throw new InvalidOperationException(
                    $"State '{targetType.Name}' was never registered. Call AddState<{targetType.Name}>() first.");
 
            if (!force && CurrentState != null && CurrentState.GetType() == targetType)
                return;
 
            var newChain = BuildChain(target);
            var oldLeaf = CurrentState;
            int common = CommonAncestorIndex(activeChain, newChain);
 
            IsTransitioning = true;
 
            // Exit leaf -> just above the shared ancestor.
            for (int i = activeChain.Count - 1; i > common; i--)
                activeChain[i].OnExit();
 
            // Don't record a state re-entering itself (force:true self-trigger)
            // as "history" - it isn't a different previous state, and pushing
            // it would make a later GoToPreviousState() pop back to the same
            // type, hit the same-leaf guard, and silently do nothing.
            if (oldLeaf != null && recordHistory && oldLeaf.GetType() != targetType)
            {
                history.Push(oldLeaf.GetType());
                TrimHistory();
            }
 
            activeChain.RemoveRange(common + 1, activeChain.Count - common - 1);
 
            // Enter shared ancestor's child -> new leaf.
            for (int i = common + 1; i < newChain.Count; i++)
            {
                newChain[i].MarkEnterTime(Time);
                activeChain.Add(newChain[i]);
                newChain[i].OnEnter();
            }
 
            PreviousState = oldLeaf;
            IsTransitioning = false;
 
            StateChanged?.Invoke(oldLeaf, CurrentState);
            transitionOccurred?.Invoke();
        }
 
        // ==================== History ====================
 
        public bool CanGoBack => history.Count > 0;
 
        /// Pops the last remembered leaf state and transitions to it, without
        /// polluting the history with the state you're leaving.
        public void GoToPreviousState()
        {
            if (history.Count == 0) return;
            var previousType = history.Pop();
            ChangeStateInternal(previousType, recordHistory: false);
        }
 
        public IReadOnlyCollection<Type> GetHistory() => history;
 
        public void ClearHistory() => history.Clear();
 
        private void TrimHistory()
        {
            if (MaxHistory <= 0 || history.Count <= MaxHistory) return;
            var arr = history.ToArray(); // top-first (index 0 = most recent)
            history.Clear();
            for (int i = arr.Length - 2; i >= 0; i--) // drop the oldest entry, rebuild
                history.Push(arr[i]);
        }
 
        // ==================== Chain helpers ====================
 
        // Walks up from leaf to root, reverses it, then drills down through
        // any InitialSubState chain so containers auto-resolve to a real leaf.
        private List<State<TOwner>> BuildChain(State<TOwner> leaf)
        {
            var chain = new List<State<TOwner>>();
            for (var cursor = leaf; cursor != null; cursor = cursor.Parent)
                chain.Add(cursor);
            chain.Reverse(); // root -> leaf
 
            var tail = chain[chain.Count - 1];
            while (tail.InitialSubState != null)
            {
                tail = tail.InitialSubState;
                chain.Add(tail);
            }
            return chain;
        }
 
        private int CommonAncestorIndex(List<State<TOwner>> a, List<State<TOwner>> b)
        {
            int i = -1;
            int max = Math.Min(a.Count, b.Count);
            while (i + 1 < max && a[i + 1] == b[i + 1]) i++;
            return i;
        }
 
        // ==================== Queries ====================
 
        /// True if TState is anywhere in the active chain (leaf or an ancestor).
        public bool IsInState<TState>() where TState : State<TOwner>
        {
            for (int i = 0; i < activeChain.Count; i++)
                if (activeChain[i] is TState) return true;
            return false;
        }
 
        public bool IsInActiveChain(State<TOwner> state) => activeChain.Contains(state);
 
        /// Root -> leaf. Don't mutate the returned list.
        public IReadOnlyList<State<TOwner>> ActiveChain => activeChain;
 
        // ==================== Event bubbling ====================
 
        /// Sends an event starting at the leaf, bubbling up through parents
        /// until a state handles it (returns true) or the root is reached.
        public bool SendEvent(string eventName, object payload = null)
        {
            for (int i = activeChain.Count - 1; i >= 0; i--)
                if (activeChain[i].HandleEvent(eventName, payload))
                    return true;
            return false;
        }
 
        // ==================== Coroutine helpers ====================
        // Use with StartCoroutine, e.g.: yield return FSM.WaitForState<AttackState>();
 
        public IEnumerator WaitForState<TState>() where TState : State<TOwner>
        {
            while (!(CurrentState is TState))
                yield return null;
        }
 
        public IEnumerator WaitWhileState<TState>() where TState : State<TOwner>
        {
            while (CurrentState is TState)
                yield return null;
        }
 
        /// Waits until any transition at all fires, regardless of which states are involved.
        public IEnumerator WaitForNextTransition()
        {
            bool fired = false;
            void Handler() => fired = true;
            transitionOccurred += Handler;
            while (!fired) yield return null;
            transitionOccurred -= Handler;
        }
 
        public IEnumerator WaitForAnyState(params Type[] stateTypes)
        {
            while (CurrentState == null || Array.IndexOf(stateTypes, CurrentState.GetType()) < 0)
                yield return null;
        }
    }
}
