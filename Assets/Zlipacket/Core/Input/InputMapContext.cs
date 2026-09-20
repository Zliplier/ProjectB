using UnityEngine;
using UnityEngine.InputSystem;

namespace Zlipacket.Core.Input
{
    public abstract class InputMapContext : ScriptableObject
    {
        protected InputActionAsset inputActionAsset;
        protected InputActionMap actionMap;
        
        public bool IsInitialized { get; private set; }
        
        /// <summary>Name of the Action Map this SO represents. Set by the generated subclass.</summary>
        public abstract string ActionMapName { get; }
        public InputActionMap GetActionMap() => actionMap;
        
        /// <summary>
        /// Call once with the runtime Input Action Asset (e.g. from a PlayerInput
        /// component or your own asset reference) before subscribing to events.
        /// </summary>
        public virtual void Initialize(InputActionAsset asset)
        {
            if (IsInitialized) return;
            if (asset == null)
            {
                Debug.LogError($"{name}: Initialize called with a null InputActionAsset.");
                return;
            }
 
            inputActionAsset = asset;
            actionMap = asset.FindActionMap(ActionMapName, throwIfNotFound: true);
            BindActions();
            IsInitialized = true;
        }
        
        /// <summary>
        /// Generated subclasses override this and call Subscribe(...) once per
        /// action, forwarding into their own public event.
        /// </summary>
        protected abstract void BindActions();
 
        /// <summary>
        /// Hooks started/performed/canceled for the named action to a single callback.
        /// </summary>
        protected void Subscribe(string actionName, System.Action<InputAction.CallbackContext> callback)
        {
            var action = actionMap.FindAction(actionName, throwIfNotFound: true);
            action.started += callback;
            action.performed += callback;
            action.canceled += callback;
        }
 
        public void EnableMap() => actionMap?.Enable();
        public void DisableMap() => actionMap?.Disable();
        
        /// <summary>
        /// Clears cached state so the next Initialize() call re-resolves the
        /// action map from scratch. Without this, a ScriptableObject asset
        /// that survives across Play Mode sessions (e.g. "Reload Domain"
        /// disabled in Enter Play Mode Settings) can end up holding an
        /// InputActionMap reference from a previous, already-torn-down
        /// InputActionState — which triggers Unity's internal
        /// "Map must be contained in state" / "index out of range" asserts
        /// the next time EnableMap() is called.
        /// </summary>
        protected virtual void OnDisable()
        {
            IsInitialized = false;
            actionMap = null;
            inputActionAsset = null;
        }
    }
}