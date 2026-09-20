using System;
using System.Linq;
using UnityEngine;

namespace Zlipacket.Core.UI.Canvas
{
    public class PanelLayerManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup cg;
        [SerializeField] private PanelLayer[] layers;
        
        public CanvasGroupController cgController { get; private set; }

        private void Awake()
        {
            cgController = new CanvasGroupController(this, cg);
        }

        public PanelLayer GetLayer(string layerName)
        {
            PanelLayer layer = layers.FirstOrDefault(l => string.Equals(l.layerName.ToLower(), layerName.ToLower(),
                StringComparison.InvariantCulture));
            if (layer == null)
            {
                Debug.LogError($"Canvas layer {layerName} not found.");
            }
            
            return layer;
        }

        public void ShowAll(bool immediate = false)
        {
            cgController.Show(immediate: immediate);
            cgController.SetInteractableState(true);
        }

        public void HideAll(bool immediate = false)
        {
            cgController.Hide(immediate: immediate);
            cgController.SetInteractableState(false);
        }
        
        public void ShowLayer(string layerName) => GetLayer(layerName).Show(false);
        public void HideLayer(string layerName) => GetLayer(layerName).Hide(false);
    }
}