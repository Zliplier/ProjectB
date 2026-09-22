using Zlipacket.Core.Tools.Utilities;
using Zlipacket.Core.UI.Canvas;

namespace Gameplay.Player
{
    public class PlayerManager : PersistantSingleton<PlayerManager>
    {
        public PanelLayerManager playerCanvas;

        public PlayerController playerPawn;
    }
}