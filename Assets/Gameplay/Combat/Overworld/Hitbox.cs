using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Combat
{
    public class Hitbox : MonoBehaviour
    {
        [Header("Components")]
        public GameObject owner;
        
        [Header("Events")]
        public UnityEvent<HitData> onHitRecieved;

        public void Hit(HitData hitData)
        {
            onHitRecieved?.Invoke(hitData);
        }
    }
}