using Gameplay.Player.PlayerState;
using UnityEngine;
using Zlipacket.Core.HSM;
using Zlipacket.Core.Input;
using Zlipacket.Core.Tools.Utilities;

namespace Gameplay.Player
{
    public class PlayerController : Singleton<PlayerController>
    {
        [Header("Components")]
        [field: SerializeField] public GameObject root { get; private set; }
        [field: SerializeField] public PlayerMovement playerMovement { get; private set; }
        [field: SerializeField] public PlayerAnimator playerAnimator { get; private set; }
        
        private StateMachine<PlayerController> stateMachine;
        public InputBuffer inputBuffer { get; } = new InputBuffer();

        public override void Awake()
        {
            base.Awake();

            stateMachine = new StateMachine<PlayerController>(this);
            
            //States
            stateMachine.AddState<PlayerIdle>();
            stateMachine.AddState<PlayerMove>();
            
            //Transition
            
            //Any Transition
            
            
            stateMachine.Start<PlayerIdle>();
        }

        private void Start()
        {
            SetControlEnabled(false);
        }

        private void Update()
        {
            stateMachine.Tick(Time.deltaTime);
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
    }
}