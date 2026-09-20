using System;
using System.Collections;
using Gameplay.Combat;
using Gameplay.Player.PlayerState;
using Gameplay.Player.PlayerState.Action;
using UnityEngine;
using UnityEngine.Events;
using Zlipacket.Core.HSM;
using Zlipacket.Core.Input;
using Zlipacket.Core.Tools.Utilities;

namespace Gameplay.Player
{
    public class PlayerController : Singleton<PlayerController>
    {
        [Header("Components")]
        [field: SerializeField] public GameObject root { get; private set; }
        [field: SerializeField] public PlayerHealth playerHealth { get; private set; }
        [field: SerializeField] public PlayerScore playerScore { get; private set; }
        [field: SerializeField] public PlayerMovement playerMovement { get; private set; }
        [field: SerializeField] public PlayerAnimator playerAnimator { get; private set; }
        [field: SerializeField] public PlayerCombat playerCombat { get; private set; }
        [field: SerializeField] public PlayerStealth playerStealth { get; private set; }

        [Header("Events")]
        public UnityEvent onDying;
        
        private StateMachine<PlayerController> stateMachine;
        public InputBuffer inputBuffer { get; } = new InputBuffer();

        public override void Awake()
        {
            base.Awake();

            stateMachine = new StateMachine<PlayerController>(this);
            
            //States
            stateMachine.AddState<PlayerIdle>();
            stateMachine.AddState<PlayerMove>();
            stateMachine.AddState<PlayerHurt>();
            stateMachine.AddState<PlayerDead>();
            
            //Transition
            stateMachine.AddState<PlayerKill>();
            stateMachine.AddState<PlayerKill>();
            stateMachine.AddTransition<PlayerIdle, PlayerKill>("Primary");
            stateMachine.AddTransition<PlayerMove, PlayerKill>("Primary");
            
            //Any Transition
            stateMachine.AddAnyTrigger<PlayerHurt>("Hurt");
            stateMachine.AddAnyTrigger<PlayerDead>("Dead");
            
            stateMachine.Start<PlayerIdle>();
        }

        public void NormalAction()
        {
            inputBuffer.Buffer("Primary", playerCombat.primaryBufferWindow, () => stateMachine.Fire("Primary"));
        }

        private void OnEnable()
        {
            playerHealth.onDead.AddListener(OnDead);
            
            playerCombat.onHitSuccess.AddListener(OnHitSuccess);
            playerCombat.onHitRecieved.AddListener(OnHitRecieved);
            playerCombat.onPrimary.AddListener(NormalAction);
        }

        private void OnDisable()
        {
            playerHealth.onDead.RemoveListener(OnDead);
            
            playerCombat.onHitSuccess.RemoveListener(OnHitSuccess);
            playerCombat.onHitRecieved.RemoveListener(OnHitRecieved);
            playerCombat.onPrimary.RemoveListener(NormalAction);
        }

        private void Start()
        {
            SetControlEnabled(false);
        }

        private void Update()
        {
            stateMachine.Tick(Time.deltaTime);
            
            inputBuffer.TryConsume("Primary", () => stateMachine.CanFire("Primary"));
        }

        private void FixedUpdate()
        {
            stateMachine.FixedTick();
        }

        private void LateUpdate()
        {
            stateMachine.LateTick();
        }

        public void SetControlEnabled(bool enabled)
        {
            if (enabled)
                playerMovement.playerInputMap.EnableMap();
            else
                playerMovement.playerInputMap.DisableMap();
        }

        private void OnHitSuccess(HitData hitData)
        {
            playerScore.AddScore(hitData.score);
            
            playerCombat.StartHitStop(hitData.hitStopTime);
        }
        
        private void OnHitRecieved(HitData hitData)
        {
            playerHealth.AddHealth(-hitData.damage);
            if (!playerHealth.IsDead)
                stateMachine.Fire("Hurt");
            
            playerMovement.AddImpulse(
                (root.transform.position.x > hitData.dealerOwner.transform.position.x
                    ? Vector3.right
                    : Vector3.left)
                * hitData.knockbackForce);
            
            playerCombat.StartHitStop(hitData.hitStopTime);
        }

        private void OnDead()
        {
            stateMachine.Fire("Dead");
            StartCoroutine(Dying());
        }

        private IEnumerator Dying()
        {
            yield return new WaitForSecondsRealtime(2f);
            
            PauseManager.Instance.Pause();
            onDying?.Invoke();
        }
    }
}