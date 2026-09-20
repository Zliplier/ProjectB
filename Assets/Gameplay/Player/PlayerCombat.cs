using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay.Combat;
using InputSO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Zlipacket.Core.Input;
using Zlipacket.Core.Tools.Utilities;

namespace Gameplay.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Configs")]
        public float primaryBufferWindow = 0.2f;
        public float damageIFrameWindow = 1f;
        
        [Header("Input")]
        [field: SerializeField] public PlayerMapContext playerInputMap { get; private set; }

        [Header("Components")]
        [SerializeField] private List<Hitbox> hitboxes;
        [SerializeField] private List<HurtBox> hurtBoxes;

        [Header("Events")]
        public UnityEvent<HitData> onHitRecieved;
        public UnityEvent<HitData> onHitSuccess;
        public UnityEvent onPrimary;
        
        [Header("Enable")]
        public bool combatEnabled = true;
        
        private Timer iFrameTimer;
        
        private void OnEnable()
        {
            playerInputMap.OnPrimary += Primary;
        }
        
        private void OnDisable()
        {
            playerInputMap.OnPrimary -= Primary;
        }

        private void Start()
        {
            foreach (Hitbox hitbox in hitboxes)
                hitbox?.onHitRecieved.AddListener(hitData => onHitRecieved.Invoke(hitData));
            foreach (HurtBox hurtBox in hurtBoxes)
                hurtBox?.onHitSuccess.AddListener(hitData => onHitSuccess.Invoke(hitData));
        }

        private void Primary(InputAction.CallbackContext context)
        {
            if (combatEnabled && context.started)
                onPrimary?.Invoke();
        }

        private void Update()
        {
            iFrameTimer?.Tick(Time.deltaTime);
        }

        public void SetActiveAllHitboxes(bool active = true)
        {
            foreach (Hitbox hitbox in hitboxes)
                hitbox?.gameObject.SetActive(active);
        }
        
        public void SetActiveAllHurtboxes(bool active = false)
        {
            foreach (HurtBox hurtbox in hurtBoxes)
                hurtbox?.gameObject.SetActive(active);
        }
        
        public void StartIFrame(float time)
        {
            if (iFrameTimer != null && iFrameTimer.IsRunning && iFrameTimer.TimeRemaining > time)
                return;
            
            SetActiveAllHitboxes(false);
            iFrameTimer = new Timer(time);
            iFrameTimer.OnTimerComplete += () => SetActiveAllHitboxes(true);
            iFrameTimer.Start();
        }

        private Coroutine co_HitStop = null;
        public bool IsHitStopping => co_HitStop != null;
        
        public void StartHitStop(float duration)
        {
            if (IsHitStopping)
                StopCoroutine(co_HitStop);

            co_HitStop = StartCoroutine(HitStopping(duration));
        }

        private IEnumerator HitStopping(float duration)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            if (!PauseManager.Instance.IsPaused)
                Time.timeScale = 1f;
            co_HitStop = null;
        }
    }
}