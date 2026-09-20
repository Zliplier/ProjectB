using UnityEngine;
using Zlipacket.Core.HSM;

namespace Gameplay.Player.PlayerState
{
    public class PlayerMove : State<PlayerController>
    {
        public override void OnEnter()
        {
            base.OnEnter();
            
            Owner.playerAnimator.Play(nameof(PlayerAnimationName.Move));
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            Owner.playerAnimator.Flip(Owner.playerMovement.IsFacingRight);

            if (Owner.playerMovement.movementInput.Equals(Vector3.zero))
            {
                Machine.ChangeState<PlayerIdle>();
            }
        }
    }
}