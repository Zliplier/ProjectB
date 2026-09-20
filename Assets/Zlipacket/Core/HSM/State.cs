using System;

namespace Zlipacket.Core.HSM
{
    /// <summary>
    /// Base class for every state. TOwner is whatever the machine is driving
    /// (a player controller, an enemy AI, a UI screen, a turret, a dialogue
    /// system... anything). Inherit from this and override only what you need.
    ///
    /// Hierarchy: set a state's Parent when you register it, and optionally
    /// give a state an InitialSubState so entering it automatically drills
    /// down into a default child (e.g. entering "Grounded" auto-enters "Idle").
    /// </summary>
    public abstract class State<TOwner>
    {
        public TOwner Owner { get; private set; }
        public StateMachine<TOwner> Machine { get; private set; }
        public State<TOwner> Parent { get; private set; }

        /// If set, entering this state also enters this child by default.
        public State<TOwner> InitialSubState { get; set; }

        /// Seconds since this specific state was entered (resets on OnEnter).
        public float TimeSinceEnter => Machine == null ? 0f : Machine.Time - enterTime;
        private float enterTime;

        /// True while this state is anywhere in the currently active chain
        /// (root...leaf) - not just when it's the leaf itself.
        public bool IsActive => Machine != null && Machine.IsInActiveChain(this);

        internal void Init(TOwner owner, StateMachine<TOwner> machine, State<TOwner> parent)
        {
            Owner = owner;
            Machine = machine;
            Parent = parent;
        }

        internal void MarkEnterTime(float t) => enterTime = t;

        // ---- Lifecycle: override whichever you need ----

        /// Called once when this state becomes active (root-to-leaf order).
        public virtual void OnEnter() { }

        /// Called once when this state stops being active (leaf-to-root order).
        public virtual void OnExit() { }

        /// Called every frame while active, leaf-to-root (children run first).
        public virtual void OnUpdate() { }

        /// Called every physics step while active, leaf-to-root.
        public virtual void OnFixedUpdate() { }

        /// Called every late-update while active, leaf-to-root.
        public virtual void OnLateUpdate() { }

        /// Cheap declarative alternative to calling Machine.ChangeState() by
        /// hand: return the type of the state you want to move to, or null to
        /// stay. Checked leaf-to-root once per Tick, after OnUpdate.
        /// Example: return (Input.GetButtonDown("Jump")) ? typeof(JumpState) : null;
        public virtual Type GetTransition() => null;

        /// Generic message/event bubbling: leaf handles first, then parents,
        /// until someone returns true. Handy for things like "Hit", "Interact".
        public virtual bool HandleEvent(string eventName, object payload = null) => false;

        public override string ToString() => GetType().Name;
    }
}
