using System;
using UnityEngine;

namespace Zlipacket.Core.UI.Canvas
{
    public class PanelLayer : MonoBehaviour
    {
        public string layerName;
        public GameObject layerRoot => gameObject;
        [SerializeField] private CanvasGroup canvasGroup;
        public CanvasGroupController cgController {get; private set;}

        private void Awake()
        {
            if (string.IsNullOrEmpty(layerName))
            {
                layerName = gameObject.name;
            }
            
            cgController = new CanvasGroupController(this, canvasGroup);
        }
        
        public void Show(float speed = 1f, bool immediate = false, Action callback = null) => cgController.Show(speed, immediate, callback);
        public void Hide(float speed = 1f, bool immediate = false, Action callback = null) => cgController.Hide(speed, immediate, callback);

        public void Show(bool immediate = false) => Show(1f, immediate: immediate);
        public void Hide(bool immediate = false) => Hide(1f, immediate: immediate);
        
        public void SetInteractableState(bool state) => cgController.SetInteractableState(state);
    }
}