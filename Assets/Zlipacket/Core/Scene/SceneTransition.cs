using UnityEngine;

namespace Zlipacket.Core.Scene
{
    [CreateAssetMenu(fileName = "SceneTransition", menuName = "Zlipacket/SceneTransition")]
    public class SceneTransition : ScriptableObject
    {
        public Transition transitionIn;
        public Transition transitionOut;

        public float TransitionIn(GameObject root, float duration)
            => StartTransition(root, transitionIn, duration);

        public float TransitionOut(GameObject root, float duration)
            => StartTransition(root, transitionOut, duration);
        
        private float StartTransition(GameObject root, Transition transition = null, float duration = 0)
        {
            if (duration <= 0)
                return 0;
            
            Transition newTransition = Instantiate<Transition>(transition, root.transform);
            float defaultDuration = newTransition.animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
            newTransition.animator.speed =  defaultDuration / duration;
            GameObject.Destroy(newTransition.gameObject, duration + 0.1f);
            return duration;
        }
    }
}