using UnityEngine;
using Zlipacket.Core.HSM;
using Zlipacket.Core.Tools.Utilities;

namespace Gameplay.Player.PlayerState.Action
{
    public class PlayerKill : State<PlayerController>
    {
        private Timer timer;
        
        public override void OnEnter()
        {
            base.OnEnter();
            
            AnimatorClipInfo clipInfo = Owner.playerAnimator.Play(nameof(PlayerAnimationName.Kill));

            if (Owner.playerMovement.ForwardVector == Vector3.right && Owner.playerMovement.movementInput.x < 0)
                Owner.playerMovement.IsFacingRight = false;
            else if (Owner.playerMovement.ForwardVector == Vector3.left && Owner.playerMovement.movementInput.x > 0)
                Owner.playerMovement.IsFacingRight = true;
            
            Owner.playerAnimator.Flip(Owner.playerMovement.IsFacingRight);
            
            Owner.playerMovement.moveEnabled = false;
            Owner.playerMovement.jumpEnabled = false;
            Owner.playerMovement.AddImpulse(Owner.playerMovement.ForwardVector * 5);
            
            timer = new Timer(clipInfo.clip.length);
            timer.OnTimerComplete += () =>
            {
                Machine.ChangeState<PlayerIdle>();
            };
            timer.Start();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            timer?.Tick(Time.deltaTime);
        }

        public override void OnExit()
        {
            base.OnExit();
            
            Owner.playerMovement.moveEnabled = true;
            Owner.playerMovement.jumpEnabled = true;

            Owner.playerCombat.SetActiveAllHurtboxes(false);
            
            timer?.Stop();
        }
    }
}