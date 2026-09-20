using System;
using UnityEngine.Events;

namespace Zlipacket.Core.Tools.Utilities
{
    public class Timer
    {
        public float Duration { get; private set; }
        public float TimeRemaining { get; private set; }
        public float NormalizedTime => TimeRemaining / Duration; // 1 -> 0
        public bool IsRunning { get; private set; }
        public bool IsLooping { get; set; }

        public event UnityAction OnTimerComplete;

        public Timer(float duration, bool isLooping = false)
        {
            Duration = duration;
            IsLooping = isLooping;
        }

        public void Start()
        {
            TimeRemaining = Duration;
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
        }

        // Must be called from a MonoBehaviour's Update() method
        public void Tick(float deltaTime)
        {
            if (!IsRunning) return;

            TimeRemaining -= deltaTime;

            if (TimeRemaining <= 0)
            {
                OnTimerComplete?.Invoke();

                if (IsLooping)
                {
                    TimeRemaining = Duration; // Reset for loop
                }
                else
                {
                    IsRunning = false;
                }
            }
        }
    }
}