using UnityEngine;

namespace Zlipacket.Core.Tools.Utilities
{
    public abstract class Singleton<T> : MonoBehaviour where T : Component
    {
        public static T Instance {get; private set;}

        public virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;
            }
            else
            {
                DestroyImmediate(gameObject);
                return;
            }
            
            Initialize();
        }
        
        [HideInInspector] public bool initialized = false;
        public virtual void Initialize()
        {
            if (initialized) return;
            
            initialized = true;
        }
    }

    public abstract class PersistantSingleton<T> : Singleton<T> where T : Component
    {
        public override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }
    }
}