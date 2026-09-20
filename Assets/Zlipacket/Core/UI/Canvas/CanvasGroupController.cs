using System;
using System.Collections;
using UnityEngine;

namespace Zlipacket.Core.UI.Canvas
{
    public class CanvasGroupController
    {
        public const float DEFAULT_FADE_SPEED = 3f;
        
        private MonoBehaviour owner;
        private CanvasGroup rootCg;

        private Coroutine co_Showing = null;
        private Coroutine co_Hiding = null;
        public bool isShowing => co_Showing != null;
        public bool isHiding => co_Hiding != null;
        public bool isFading => isShowing || isHiding;
        public bool isVisible => co_Showing != null || rootCg.alpha > 0f;
        public float alpha { get => rootCg.alpha;
            set => rootCg.alpha = value;
        }
        
        public event Action onFadingInStarted;
        public event Action onFadingInCompleted;
        public event Action onFadingOutStarted;
        public event Action onFadingOutCompleted;
        
        public CanvasGroupController(MonoBehaviour owner, CanvasGroup rootCg)
        {
            this.owner = owner;
            this.rootCg = rootCg;
        }
        
        public Coroutine Show(float speed = 1f, bool immediate = false, Action callback = null)
        {
            if (isShowing)
                return co_Showing;
            else if (isHiding)
            {
                owner.StopCoroutine(co_Hiding);
                co_Hiding = null;
            }
            
            if (!owner.isActiveAndEnabled)
                return null;
            
            co_Showing = owner.StartCoroutine(Fading(1f, speed, immediate, callback));
            return co_Showing;
        }

        public Coroutine Hide(float speed = 1f, bool immediate = false, Action callback = null)
        {
            if (isHiding)
                return co_Hiding;
            else if (isShowing)
            {
                owner.StopCoroutine(co_Showing);
                co_Showing = null;
            }

            if (!owner.isActiveAndEnabled)
                return null;
                
            co_Hiding = owner.StartCoroutine(Fading(0f, speed, immediate, callback));
            return co_Hiding;
        }

        private IEnumerator Fading(float targetAlpha, float speed = 1f, bool immediate = false, Action callback = null)
        {
            if (targetAlpha > 0f)
            {
                SetInteractableState(true);
                onFadingInStarted?.Invoke();
            }
            else
            {
                onFadingOutStarted?.Invoke();
            }
            
            CanvasGroup cg = rootCg;

            if (immediate)
                cg.alpha = targetAlpha;
            
            while (cg.alpha != targetAlpha)
            {
                cg.alpha = Mathf.MoveTowards(cg.alpha, targetAlpha, Time.deltaTime * DEFAULT_FADE_SPEED * speed);
                yield return null;
            }

            if (targetAlpha <= 0f)
            {
                SetInteractableState(false);
                onFadingOutCompleted?.Invoke();
            }
            else
            {
                onFadingInCompleted?.Invoke();
            }
            
            co_Showing = null;
            co_Hiding = null;
            callback?.Invoke();
        }

        public void SetInteractableState(bool active)
        {
            rootCg.interactable = active;
            rootCg.blocksRaycasts = active;
        }
    }
}