using UnityEngine;

namespace Zlipacket.Core.Tools.Utilities
{
    public class CoroutineWrapper
    {
        private MonoBehaviour owner;
        private Coroutine coroutine;
        
        public bool isDone = false;
        
        public CoroutineWrapper(MonoBehaviour owner, Coroutine coroutine)
        {
            this.owner = owner;
            this.coroutine = coroutine;
        }

        public void Stop()
        {
            owner.StopCoroutine(coroutine);
            isDone = true;
        }
    }
}