using UnityEngine;
using UnityEngine.Events;

namespace Zlipacket.Core.Tools.Utilities
{
    public class PauseManager : Singleton<PauseManager>
    {
        [Header("Events")]
        public UnityEvent onPause;
        public UnityEvent onResume;
        
        public bool IsPaused { get; private set; }

        public void TogglePause()
        {
            if (IsPaused) Resume();
            else Pause();
        }

        public void Pause()
        {
            Debug.Log("Pause");
            IsPaused = true;
            Time.timeScale = 0f;
            onPause?.Invoke();
        }

        public void Resume()
        {
            Debug.Log("Resume");
            IsPaused = false;
            Time.timeScale = 1f;
            onResume?.Invoke();
        }
    }
}