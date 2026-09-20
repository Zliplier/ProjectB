using System;
using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.Enemy
{
    public class EnemyMovement : MonoBehaviour
    {
        [Header("Configs")]
        public float deceleration = 10f;
        
        [Header("Components")]
        [field: SerializeField] public NavMeshAgent agent { get; private set; }
        [SerializeField] private Rigidbody rb;
        
        [Header("Enable")]
        public bool agentControlEnabled = false;
        
        public bool IsFacingRight { get; private set; } = true;
        
        public Vector3 ForwardVector => IsFacingRight ? Vector3.right : Vector3.left;

        public Vector3 velocity => (agentControlEnabled ? agent.desiredVelocity : Vector3.zero) + additionalVelocity;
        
        public Vector3 additionalVelocity { get; private set; } = Vector3.zero;
        
        private void Start()
        {
            agent.updatePosition = false;
            agent.updateRotation = false;
        }

        private void Update()
        {
            TurnCheck();
        }

        private void FixedUpdate()
        {
            HandleImpulse();
            
            ApplyMovement();
            //Debug.Log(agent.desiredVelocity);
        }
        
        private void LateUpdate()
        {
            agent.nextPosition = rb.position;
        }

        private void ApplyMovement()
        {
            rb.linearVelocity = velocity;
        }

        public void MoveTo(Vector3 destination)
        {
            agent.SetDestination(destination);
        }

        private void TurnCheck()
        {
            if (agent.desiredVelocity.x > 0)
                IsFacingRight = true;
            else if (agent.desiredVelocity.x < 0)
                IsFacingRight = false;
        }

        private void HandleImpulse()
        {
            additionalVelocity = Vector3.MoveTowards(
                additionalVelocity, 
                Vector3.zero, 
                deceleration * Time.fixedDeltaTime);
            //agent.velocity += additionalVelocity;
        }

        public void AddImpulse(Vector3 impulse)
        {
            additionalVelocity += impulse;
        }
    }
}