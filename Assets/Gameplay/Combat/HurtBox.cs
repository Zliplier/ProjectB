using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zlipacket.Core.Tools.Extension;

namespace Gameplay.Combat
{
    public class HurtBox : MonoBehaviour
    {
        [Header("Configs")]
        public string hitTag;
        public float damage;
        public float score;
        public float knockbackForce;
        public float hitStopTime;

        [Header("Components")]
        public GameObject owner;
        
        [Header("Events")]
        public UnityEvent<HitData> onHitSuccess;
        
        private List<GameObject> hitList = new List<GameObject>();

        private void OnDisable()
        {
            hitList.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (string.IsNullOrWhiteSpace(hitTag) || !other.CompareTag(hitTag))
                return;
            
            //Debug.Log("Hitbox Enter: " + other.name);
            if (other.TryGetComponent(out Hitbox hitbox))
            {
                if (!hitList.Contains(hitbox.owner))
                {
                    hitList.Add(hitbox.owner);
                    Hit(hitbox);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (string.IsNullOrWhiteSpace(hitTag) || !other.CompareTag(hitTag))
                return;
            
            if (other.TryGetComponent(out Hitbox hitbox))
            {
                if (hitList.Contains(hitbox.owner))
                {
                    hitList.Remove(hitbox.owner);
                }
            }
        }

        private void Hit(Hitbox hitbox)
        {
            //Debug.Log("Hit");
            HitData hitData = new HitData(damage, score, hitStopTime, knockbackForce, this, hitbox);
            onHitSuccess.Invoke(hitData);
            hitbox.Hit(hitData);
        }
    }
    
    [Serializable]
    public class HitData
    {
        public float damage;
        public float score;
        public float hitStopTime;
        public float knockbackForce;
        
        public HurtBox dealer;
        public GameObject dealerOwner => dealer?.owner;
        public Hitbox target;
        public GameObject targetOwner => target?.owner;

        public HitData(float damage, float score, float hitStopTime, float knockbackForce, HurtBox dealer, Hitbox target)
        {
            this.damage = damage;
            this.score = score;
            this.hitStopTime = hitStopTime;
            this.knockbackForce = knockbackForce;
            
            this.dealer = dealer;
            this.target = target;
        }
    }
}