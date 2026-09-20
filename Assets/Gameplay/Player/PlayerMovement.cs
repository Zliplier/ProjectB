using System;
using System.Collections;
using InputSO;
using UnityEngine;
using UnityEngine.InputSystem;
using Zlipacket.Core.Tools.Utilities;

namespace Gameplay.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Configs")]
        public float groundAccel;
        public float airAccel;
        
        public float groundDecel;
        public float airDecel;
        
        public float walkSpeed;

        public Vector3 gravity;
        public float gravityAccel;
        
        public float jumpHeight;
        public float timeTillApexJump;
        public float hangTimeApexJump;
        public int jumpAllowed;
        public float jumpBuffer;
        public float jumpCoyote;
        
        [Range(0, 1)]
        public float turnCompensation;
        
        public LayerMask groundLayer;
	    public Vector3 groundBoxSize;
	    public float groundRayDistance;
        
        [Header("Input")]
        [field: SerializeField] public PlayerMapContext playerInputMap { get; private set; }

        [Header("Components")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Transform feetPos;

        [Header("Enable")]
        public bool moveEnabled = true;
        public bool jumpEnabled = true;
        public bool gravityEnabled = true;
    
        public bool IsGrounded { get; private set; } = true;
        public bool IsFacingRight { get; set; } = true;
        
        public Vector3 ForwardVector => IsFacingRight ? Vector3.right : Vector3.left;
        
        public float acceleration => IsGrounded? groundAccel : airAccel;
        public float deceleration => IsGrounded? groundDecel : airDecel;
        public Vector3 movementInput { get; private set; } = Vector3.zero;

        private int jumpUsed = 0;
        private float jumpBufferTime = 0f;
        private float jumpCoyoteTime = 0f;
        private Coroutine co_Jumping = null; //Coroutine will run until the apex time is reached or landing first.
        public bool IsJumping => co_Jumping != null;
        private bool jumpCutFlag = false;
        
        //Velocity
        public Vector3 velocity => moveVelocity + jumpVelocity + gravityVelocity + additionalVelocity;
        private Vector3 moveVelocity;
        private Vector3 jumpVelocity;
        private Vector3 gravityVelocity = Vector3.zero;
        private Vector3 additionalVelocity = Vector3.zero;
        
        private void OnEnable()
        {
            playerInputMap.OnMove += MovementInput;
            playerInputMap.OnSpace += JumpInput;
        }

        private void OnDisable()
        {
            playerInputMap.OnMove -= MovementInput;
            playerInputMap.OnSpace -= JumpInput;
        }

        private void MovementInput(InputAction.CallbackContext context)
        {
            Vector2 input = context.ReadValue<Vector2>();
            movementInput = input != Vector2.zero ? new Vector3(input.x, 0f, input.y).normalized : Vector3.zero;
        }
        
        private void JumpInput(InputAction.CallbackContext context)
        {
	        //Jump Start
	        if (context.started)
	        {
		        jumpBufferTime = jumpBuffer;
	        }
	        //Jump Cut
	        else if (context.canceled && IsJumping)
	        {
		        JumpCut();
	        }
        }
    
        private void Update()
        {
	        UpdateTimer(Time.deltaTime);
	        TurnCheck();
        }

        private void FixedUpdate()
        {
	        CollisionCheck();
	        DebugGroundCheck();
            
	        if (gravityEnabled)
				HandleGravity();
	        
	        HandleHorizontal();
	        
	        if (jumpEnabled)
				HandleVertical();
	        
	        HandleImpulse();
	        
	        ApplyMovement();
        }

        private void TurnCheck()
        {
	        if (moveVelocity.x > 0)
		        IsFacingRight = true;
	        else if (moveVelocity.x < 0)
		        IsFacingRight = false;
	        
            // Turn Left
            if (moveVelocity.x > 0 && movementInput.x < 0)
            {
                moveVelocity.x -= moveVelocity.x * turnCompensation;
            }
            // Turn Right
            else if (moveVelocity.x < 0 && movementInput.x > 0)
            {
	            moveVelocity.x -= moveVelocity.x * turnCompensation;
            }

            if ((moveVelocity.z < 0 && movementInput.z > 0) || (moveVelocity.z < 0 && movementInput.z > 0))
            {
                moveVelocity.z -= moveVelocity.z * turnCompensation;
            }
        }

        private void ApplyMovement()
        {
	        rb.linearVelocity = velocity;
	        //rb.MovePosition(rb.position + (velocity * Time.fixedDeltaTime));
        }

        public void HandleImpulse()
        {
	        additionalVelocity = Vector3.MoveTowards(
		        additionalVelocity, 
		        Vector3.zero, 
		        deceleration * Time.fixedDeltaTime);
        }
        
        public void AddImpulse(Vector3 impulse)
        {
	        additionalVelocity += impulse;
        }

        private void HandleHorizontal()
        {
            float targetSpeed;
            float accelRate;

            if (movementInput.sqrMagnitude > 0f && moveEnabled)
            {
                targetSpeed = walkSpeed;
                accelRate = acceleration;
            }
            else
            {
                targetSpeed = 0f;
                accelRate = deceleration;
            }
            
            moveVelocity = Vector3.Lerp(
                moveVelocity, 
                movementInput * targetSpeed, 
                accelRate * Time.fixedDeltaTime);
        }

        private void HandleVertical()
        {
	        JumpCheck();
        }
        
        private void HandleGravity()
        {
            if (IsJumping || IsGrounded)
                gravityVelocity = Vector3.zero;
            else
                gravityVelocity = Vector3.Lerp(gravityVelocity, gravity, gravityAccel * Time.fixedDeltaTime);
        }

        #region Jump
        private void JumpCheck()
        {
        	if (jumpBufferTime > 0)
        	{
        		InitiateJump();
        	}
        }

        private void InitiateJump()
        {
        	//Jump on ground or during coyote time — costs 1.
            if (!IsJumping && (IsGrounded || jumpCoyoteTime > 0) && jumpUsed < jumpAllowed)
                Jump(1);
            //Already used at least one jump in the air — normal air jump, costs 1.
            else if (!IsJumping && !IsGrounded && jumpUsed < jumpAllowed)
                Jump(1);
            //Falling without ever having jumped (walked off ledge, coyote expired) — costs 2.
            else if (!IsGrounded && jumpUsed == 0 && jumpUsed + 2 <= jumpAllowed)
                Jump(2);
        }
        
        private void Jump(int jumpUsage)
        {
	        //Debug.Log("Jump Usage: " + jumpUsage);
        	jumpUsed += jumpUsage;
        	
        	if (IsJumping)
        		StopCoroutine(co_Jumping);

	        jumpCutFlag = false;
        	co_Jumping = StartCoroutine(Jumping());

        	jumpBufferTime = 0f;
        }

        private IEnumerator Jumping()
        {
        	float jumpTime = 0f;
        	float jumpPercentage = 0f;
        	float initialJumpVelocity = Mathf.Abs((2f * jumpHeight) / Mathf.Pow(timeTillApexJump, 2f)) * timeTillApexJump;
        	
        	//Debug.Log("Start Jumping with Force: " + initialJumpVelocity);
        	
        	while (jumpTime < timeTillApexJump)
        	{
		        if (jumpCutFlag)
			        break;
		        
        		yield return new WaitForFixedUpdate();
        		jumpPercentage = Mathf.Clamp(jumpTime / timeTillApexJump, 0f, 1f);
        		
        		jumpVelocity.y = Mathf.Lerp(initialJumpVelocity, 0f, jumpPercentage);
        		
        		jumpTime += Time.fixedDeltaTime;
        	}
        	
        	//Debug.Log("Jump Apex Reached");
        	gravityVelocity = jumpVelocity * 0.5f;
        	jumpVelocity = Vector3.zero;
        	jumpCutFlag = false;
        	yield return new WaitForSeconds(hangTimeApexJump);
        	co_Jumping = null;
        	//Debug.Log("Stop Jumping");
        }

        private void JumpCut()
        {
        	jumpCutFlag = true;
        }

        private void CancelJump()
        {
        	if (IsJumping)
        	{
        		StopCoroutine(co_Jumping);
        		co_Jumping = null;
        	}
        	
        	jumpCutFlag = false;
        	jumpVelocity = Vector3.zero;
        	gravityVelocity = Vector3.zero;
        }
        #endregion
        
        private void CollisionCheck()
        {
	        GroundCheck();
        }

        private void GroundCheck()
        {
	        Vector3 boxSize = groundBoxSize;
	        Vector3 boxOrigin = feetPos.position;

	        bool hit = Physics.BoxCast(boxOrigin, boxSize / 2, Vector3.down, out var groundHit, Quaternion.identity,
		        groundRayDistance, groundLayer);
	        if (hit)
	        {
		        if (!IsGrounded)
		        {
			        IsGrounded = true;
			        OnLanding();
		        }
	        }
	        else
	        {
		        if (IsGrounded)
		        {
			        IsGrounded = false;
			        TakeOff();
		        }
	        }
        }

        private void OnLanding()
        {
	        //Debug.Log("OnLanding");
	        CancelJump();
            
	        jumpUsed = 0;
	        jumpCoyoteTime = 0f;
            
	        gravityVelocity = Vector3.zero;
        }

        private void TakeOff()
        {
	        //Debug.Log("TakeOff");
	        jumpCoyoteTime = jumpCoyote;
        }
        
        private void UpdateTimer(float deltaTime)
        {
	        JumpTimer(deltaTime);
        }

        private void JumpTimer(float deltaTime)
        {
	        if (jumpBufferTime > 0)
		        jumpBufferTime -= deltaTime;
	        if (jumpCoyoteTime > 0)
		        jumpCoyoteTime -= deltaTime;
        }
        
        private void DebugGroundCheck()
        {
	        Vector3 boxSize = groundBoxSize;
	        Vector3 boxOrigin = feetPos.position;
	        Color color = IsGrounded ? Color.green : Color.red;
            
	        ExtDebug.DrawBoxCastBox(boxOrigin, boxSize / 2, Quaternion.identity, Vector3.down, groundRayDistance, color);
        }
    }
}
