using System;
using System.Collections.Generic;
using Gameplay.Combat;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Enemy
{
    public class EnemyCombat : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private List<Hitbox> hitboxes;
        [SerializeField] private List<HurtBox> hurtboxes;
        
        [Header("Events")]
        public UnityEvent<HitData> onHitSuccess;
        public UnityEvent<HitData> onHitRecieved;

        private void Start()
        {
            foreach (HurtBox hurtbox in hurtboxes)
                hurtbox?.onHitSuccess.AddListener(hitData => onHitSuccess.Invoke(hitData));
            foreach (Hitbox hitbox in hitboxes)
                hitbox?.onHitRecieved.AddListener(hitData => onHitRecieved.Invoke(hitData));
        }

        public void SetActiveAllHitboxes(bool active = true)
        {
            foreach (Hitbox hitbox in hitboxes)
                hitbox?.gameObject.SetActive(active);
        }
        
        public void SetActiveAllHurtboxes(bool active = false)
        {
            foreach (HurtBox hurtbox in hurtboxes)
                hurtbox?.gameObject.SetActive(active);
        }
    }
}