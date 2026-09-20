using UnityEngine;
using Zlipacket.Core.HSM;

namespace Gameplay.Player.PlayerState
{
    public class PlayerIdle : State<PlayerController>
    {
        public override void OnEnter()
        {
            base.OnEnter();
            
            Owner.playerAnimator.Play(nameof(PlayerAnimationName.Idle));
            
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (!Owner.playerMovement.movementInput.Equals(Vector3.zero))
            {
                Machine.ChangeState<PlayerMove>();
            }
        }
    }
}