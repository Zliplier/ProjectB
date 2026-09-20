using Zlipacket.Core.HSM;

namespace Gameplay.Player.PlayerState
{
    public class PlayerDead : State<PlayerController>
    {
        public override void OnEnter()
        {
            base.OnEnter();
            
            Owner.playerAnimator.Play(nameof(PlayerAnimationName.Dead));
            
            Owner.playerMovement.moveEnabled = false;
            Owner.playerMovement.jumpEnabled = false;
            
            Owner.playerCombat.SetActiveAllHitboxes(false);
            Owner.playerCombat.SetActiveAllHurtboxes(false);
        }
    }
}