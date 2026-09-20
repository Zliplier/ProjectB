using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Zlipacket.Core.Tools.Extension;

namespace Gameplay.Enemy
{
    public class EnemyAnimator : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Animator animator;
        [SerializeField] private TextMeshPro faceText;
        [SerializeField] private List<GameObject> flipTargets;
        
        public AnimatorClipInfo GetCurrentClip(int layerIndex = 0)
            => animator.GetCurrentAnimatorClipInfo(layerIndex)[0];
        
        public void Flip(bool isRight)
        {
            foreach (var flipTarget in flipTargets)
                flipTarget.transform.localScale = flipTarget.transform.localScale.Insert(x: Mathf.Abs(flipTarget.transform.localScale.x) * (isRight ? 1 : -1));
        }

        public AnimatorClipInfo Play(string animationName, float speed = 1f, int layerIndex = 0, bool allowedRetrigger = true)
        {
            animator.speed = speed;

            if (animator.HasState(layerIndex, Animator.StringToHash(animationName)))
            {
                if (allowedRetrigger)
                    animator.Play(animationName, layerIndex, 0f);
                else
                    animator.Play(animationName, layerIndex);
                
                animator.Update(0f); 
            }
            else
            {
                Debug.LogError($"Animation {animationName} not found.");
                return default;
            }
            
            AnimatorClipInfo clipInfo = animator.GetCurrentAnimatorClipInfo(layerIndex).FirstOrDefault(a => a.clip.name == animationName);
            return clipInfo;
        }
        
        public AnimatorClipInfo PlayByDuration(string animationName, float duration = 1f, int layerIndex = 0)
        {
            AnimatorClipInfo clipInfo = Play(animationName, layerIndex: layerIndex);
            
            animator.speed = clipInfo.clip.length / duration;
            
            return clipInfo;
        }

        public void SetFaceText(string text, float size = 4f)
            => faceText.SetText(text);
    }
}