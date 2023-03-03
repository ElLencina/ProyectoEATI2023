using UnityEngine;
using System.Collections;

namespace VRStep
{

	public enum ControllerType
	{
		RigidBody,
		CharacterController,
		Manual
	}

	public class WIPController : MonoBehaviour
	{
		private Rigidbody _rigidbody;
		private CharacterController _characterController;

		private ControllerType controllerType = ControllerType.RigidBody;

		[Header("Movement options")]

		[Tooltip("The fastest speed that the character can move (Default value 6)")]
		public float maxForwardVelocity = 6;

		[Tooltip("The slowest speed that the character can move, and the initial speed when you take a step (Default value 2)")]
		public float minForwardVelocity = 2;

		[Tooltip("Whether or not to slow down movement every fixedupdate frame")]
		public bool dampingEnabled = true;

		[Tooltip("The amount of damping applied every FixedUpdate frame (Default value .95) ")]
		public float dampingValue = .95f;


		[Header("Step Response Times")]

		[Tooltip("If the time between steps is this or lower, velocity is set to max forward velocity (Default value .2)")]
		public float minStepTime = .2f;

		[Tooltip("If the time between steps is this or higher, velocity is set to the min forward velocity (Default Value 1.3)")]
		public float maxStepTime = 1.3f;

		[Tooltip("If the time between steps is this or higher, velocity is set to 0 (Default value 1.3)")]
		public float stoppingStepTime = 1.3f;

		[Tooltip("If this is enabled, the rotation of this object will be aligned with the forward vector of the forwardTransform object")]
		public bool rotateTowardsGaze = true;

		[Header("Object References")]

		[Tooltip("The reference to the Transform that is pointing forward, usually you want this to be the GVR head object.  The up/down tilt orientation of this object is ignored")]
		public Transform forwardTransform;

		[Header("Jump options")]
		
		[Tooltip("Whether or not jumping is enabled or not")]
		public bool jumpEnabled = true;

		[Tooltip("Threshold on the accelerometer in the up direction of when to detect a jump")]
		public float jumpDetectionThreshold = 1;

		[Tooltip("Whether or not the character is currently on the ground")]
		public bool onGround = false;

		[Tooltip("The layers that count as ground, assign this layer to the objects that are on the ground")]
		public LayerMask groundLayerMask;

		[Tooltip("The amount of time after leaving the ground that the user can still jump, to avoid frustration")]
		public float jumpGracePeriod = .3f;

		private float _jumpGracePeriodTimer = 0;
		private float _jumpCooldownTimer = 0;
		private float _jumpCooldown = .5f;


		[HideInInspector]
		public float targetForwardVelocity;

		[HideInInspector]
		public float timeSinceLastStep = 5f;

		[HideInInspector]
		public Vector3 velocityVector;

		void Start()
		{

			_rigidbody = GetComponent<Rigidbody>();
			_characterController = GetComponent<CharacterController>();

			if (controllerType == ControllerType.RigidBody && _rigidbody == null)
			{
				Debug.Log("No Rigidbody component found on the WIP Controller object, setting ControllerType to manual");
				controllerType = ControllerType.Manual;
			}

			if (controllerType == ControllerType.CharacterController && _characterController == null)
			{
				Debug.Log("No CharacterController component found on the WIP Controller object, setting ControllerType to manual");
				controllerType = ControllerType.Manual;
			}

			StepDetector.AddStepAction(OnStep);

		}

		void Update()
		{
			CheckIfOnGround();

			if (onGround)
			{
				timeSinceLastStep += Time.deltaTime;
			}
			if (_jumpCooldownTimer > 0)
			{
				_jumpCooldownTimer -= Time.deltaTime;
			}
		}

		void FixedUpdate()
		{
			if(rotateTowardsGaze)
				PointTowardsGaze();

			if (timeSinceLastStep > stoppingStepTime)
				targetForwardVelocity = 0;

			//set velocity based on our target
			velocityVector = transform.forward * targetForwardVelocity;

			if (controllerType == ControllerType.RigidBody)
			{
				velocityVector.y = _rigidbody.velocity.y;
				_rigidbody.velocity = velocityVector;
			}
			else if (controllerType == ControllerType.CharacterController)
			{
				velocityVector.y = _characterController.velocity.y;
				velocityVector.y += Physics.gravity.y * Time.fixedDeltaTime;
				_characterController.Move(velocityVector * Time.fixedDeltaTime);
			}
			

			//apply damping
			if(dampingEnabled)
				targetForwardVelocity *= dampingValue;

			if(controllerType == ControllerType.RigidBody)
				CheckJump();
		}

		void PointTowardsGaze()
		{
			Vector3 currentRotation = transform.eulerAngles;
			currentRotation.y = forwardTransform.rotation.eulerAngles.y;
			transform.eulerAngles = currentRotation;
		}

		void OnStep()
		{
			if (onGround)
			{
				if (timeSinceLastStep > maxStepTime)
				{
					targetForwardVelocity = minForwardVelocity;
				}
				else if (timeSinceLastStep <= minStepTime)
				{
					targetForwardVelocity = maxForwardVelocity;
				}
				else
				{
					float t = (timeSinceLastStep - minStepTime) / (maxStepTime - minStepTime);
					float potentialVelocity = Mathf.Lerp(maxForwardVelocity, minForwardVelocity, t);

					if (potentialVelocity >= targetForwardVelocity)
						targetForwardVelocity = potentialVelocity;
				}

				timeSinceLastStep = 0;
			}
		}

		void CheckIfOnGround()
		{
			if (Physics.Raycast(transform.position, Vector3.down, .95f, groundLayerMask))
			{
				_jumpGracePeriodTimer = jumpGracePeriod;
				onGround = true;
				dampingEnabled = true;
			}
			else
			{
				_jumpGracePeriodTimer -= Time.deltaTime;
				onGround = false;
				dampingEnabled = false;
			}
		}

		void CheckJump()
		{
			Vector3 up = -Input.gyro.gravity;
			float upwardsAccel = StepDetector.GetUpwardsAcceleration();

			bool jumpInputted = (upwardsAccel > jumpDetectionThreshold) || Input.GetKey(KeyCode.Space);

			if ((onGround || _jumpGracePeriodTimer > 0) && _jumpCooldownTimer <= 0 && jumpInputted) 
			{
				_jumpCooldownTimer = _jumpCooldown;
				Vector3 curVel = _rigidbody.velocity;
				curVel.y = 5;
				_rigidbody.velocity = curVel;

				onGround = false;
				dampingEnabled = false;
			}
		}
		

	}
}