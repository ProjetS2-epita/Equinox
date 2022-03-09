using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.AI;


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SphereCollider))]
public class ThirdPersonController : MonoBehaviour
{

	[Header("Player")]
	[Tooltip("Move speed of the character in m/s")]
	public float MoveSpeed = 2.0f;
	[Tooltip("Sprint speed of the character in m/s")]
	public float SprintSpeed = 5.335f;
	[Tooltip("How fast the character turns to face movement direction")]
	[Range(0.0f, 0.3f)]
	public float RotationSmoothTime = 0.12f;
	[Tooltip("Acceleration and deceleration")]
	public float SpeedChangeRate = 10.0f;
	public float Sensitivity = 1f;

	[Space(10)]
	[Tooltip("The height the player can jump")]
	public float JumpHeight = 1.2f;
	[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
	public float Gravity = -15.0f;

	[Space(10)]
	[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
	public float JumpTimeout = 0.50f;
	[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
	public float FallTimeout = 0.15f;

	[Header("Player Grounded")]
	[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
	[SerializeField] private bool Grounded = true;
	[SerializeField] private bool canJump = true;
	
	[Tooltip("Useful for rough ground")]
	public float GroundedOffset = -0.14f;
	[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
	public float GroundedRadius = 0.28f;
	[Tooltip("What layers the character uses as ground")]
	public LayerMask GroundLayers;
	[SerializeField] private float GroundSlopeAngle = 0f;
	public float angleRayLength = 0.75f;

	[Header("Cinemachine")]
	[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
	public GameObject CinemachineCameraTarget;
	[Tooltip("How far in degrees can you move the camera up")]
	public float TopClamp = 70.0f;
	[Tooltip("How far in degrees can you move the camera down")]
	public float BottomClamp = -30.0f;
	[Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
	public float CameraAngleOverride = 0.0f;
	[Tooltip("For locking the camera position on all axis")]
	public bool LockCameraPosition = false;

	// Shooter Controller
	[Header("Shooter Controller")]
	[Tooltip("Camera to switch when aiming")]
	[SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
	[Tooltip("Normal camera sensitivity")]
	[SerializeField] private float normalSensitivity = 1f;
	[Tooltip("Aiming camera sensitivity")]
	[SerializeField] private float aimSensitivity = 0.3f;
	[Tooltip("Layer(s) to collide with cursor aim")]
	[SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
	[Tooltip("Layer(s) where enemies belong")]
	[SerializeField] private LayerMask ennemiesLayer;
	[Tooltip("GameObject aim debugger")]
	[SerializeField] private Transform aimTrackerTransform;
	[Tooltip("Damage per shoot")]
	[SerializeField] private float shootDamage;
	[Tooltip("Sound played when shoot")]
	public AudioClip shootSound;
	Transform lastHitTransform;
	RaycastHit lastRaycastHit;
	Vector3 mouseWorldPosition;
	[Tooltip("Gunshot sound propagation distance for enemies detection")]
	public float soundIntensity = 100f;
	[Tooltip("Detectable by enemies perimeter when walking")]
	public float walkEnemyPerceptionRadius = 1.5f;
	[Tooltip("Detectable by enemies perimeter when sprinting")]
	public float sprintEnemyPerceptionRadius = 4f;
	private AudioSource audioSource;
	private SphereCollider sphereCollider;
	public GameObject DroneOriginal;
	private GameObject DroneInstance = null;
	public float rapatriationRange = 2f;

	// cinemachine
	private float _cinemachineTargetYaw;
	private float _cinemachineTargetPitch;

	// player
	private float _speed;
	private float _animationBlend;
	private float _targetRotation = 0.0f;
	private float _rotationVelocity;
	private float _verticalVelocity;
	private float _terminalVelocity = 53.0f;

	// timeout deltatime
	private float _jumpTimeoutDelta;
	private float _fallTimeoutDelta;

	// animation IDs
	private int _animIDSpeed;
	private int _animIDGrounded;
	private int _animIDJump;
	private int _animIDFreeFall;
	private int _animIDMotionSpeed;

	private Animator _animator;
	private CharacterController _controller;

	//Inputs
	private PlayerInput _playerInput;
	private InputActionMap playerMap;
	private InputAction moveAction;
	private InputAction lookAction;
	private InputAction jumpAction;
	private InputAction sprintAction;
	public InputAction aimAction;
	public InputAction shootAction;
	private InputAction droneAction;

	private GameObject _mainCamera;
	private bool _rotateOnMove = true;
	private const float _threshold = 0.01f;
	private bool _hasAnimator;

    private void Awake()
	{
		// get a reference to our main camera
		if (_mainCamera == null)
		{
			_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
		}
		_controller = GetComponent<CharacterController>();
		audioSource = GetComponent<AudioSource>();
		sphereCollider = GetComponent<SphereCollider>();

		_playerInput = GameObject.FindGameObjectWithTag("GameManager").GetComponent<PlayerInput>();
		playerMap = _playerInput.actions.FindActionMap("Player", true);
		moveAction = playerMap.FindAction("Move");
		lookAction = playerMap.FindAction("Look");
		jumpAction = playerMap.FindAction("Jump");
		sprintAction = playerMap.FindAction("Sprint");
		aimAction = playerMap.FindAction("Aim");
		shootAction = playerMap.FindAction("Shoot");
		droneAction = playerMap.FindAction("SwitchDrone");

	}

	private void OnEnable()
	{
		shootAction.performed += Shoot;
		aimAction.performed += Aim;
		droneAction.performed += DroneSwitch;
	}

    private void OnDisable()
    {
		shootAction.performed -= Shoot;
		aimAction.performed -= Aim;
		droneAction.performed -= DroneSwitch;
	}

    private void Start()
	{
		_hasAnimator = TryGetComponent(out _animator);
		AssignAnimationIDs();
		// reset our timeouts on start
		_jumpTimeoutDelta = JumpTimeout;
		_fallTimeoutDelta = FallTimeout;
		mouseWorldPosition = Vector3.zero;
	}

	private void Update()
	{
		if (!playerMap.enabled) return;

		_hasAnimator = TryGetComponent(out _animator);

		JumpAndGravity();
		GroundedCheck();
		Move();

		mouseWorldPosition = Vector3.zero;
		Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
		Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
		lastHitTransform = null;
		if (Physics.Raycast(ray, out lastRaycastHit, 999f, aimColliderLayerMask)) {
			aimTrackerTransform.position = lastRaycastHit.point;
			mouseWorldPosition = lastRaycastHit.point;
			lastHitTransform = lastRaycastHit.transform;
		}

		//walk & sprint noise radius
		sphereCollider.radius = GetPlayerStealthProfile() == 0 ? walkEnemyPerceptionRadius : sprintEnemyPerceptionRadius;
	}

	private void LateUpdate()
	{
		CameraRotation();
	}

	private void AssignAnimationIDs()
	{
		_animIDSpeed = Animator.StringToHash("Speed");
		_animIDGrounded = Animator.StringToHash("Grounded");
		_animIDJump = Animator.StringToHash("Jump");
		_animIDFreeFall = Animator.StringToHash("FreeFall");
		_animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
	}

	private void GroundedCheck()
	{
		// set sphere position, with offset
		Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
		Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		RaycastHit hit;
		Physics.Raycast(spherePosition, Vector3.down, out hit, angleRayLength);
		GroundSlopeAngle = Vector3.Angle(hit.normal, Vector3.up);
		Debug.DrawLine(spherePosition, hit.point, Color.red);
		canJump = GroundSlopeAngle < _controller.slopeLimit;

		// update animator if using character
		if (_hasAnimator)
		{
			_animator.SetBool(_animIDGrounded, Grounded);
		}
	}

	private void CameraRotation()
	{
		Vector2 look = lookAction.ReadValue<Vector2>();

		// if there is an input and camera position is not fixed
		if (look.sqrMagnitude >= _threshold && !LockCameraPosition)
		{
			_cinemachineTargetYaw += look.x * Time.deltaTime * Sensitivity;
			_cinemachineTargetPitch += look.y * Time.deltaTime * Sensitivity;
		}

		// clamp our rotations so our values are limited 360 degrees
		_cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
		_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

		// Cinemachine will follow this target
		CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
	}

	private void Move()
	{
		Vector2 move = moveAction.ReadValue<Vector2>().normalized;

		float targetSpeed = sprintAction.IsPressed() ? SprintSpeed : MoveSpeed;

		if (move == Vector2.zero) targetSpeed = 0.0f;

		float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

		float speedOffset = 0.1f;
		float inputMagnitude = move.magnitude;

		// accelerate or decelerate to target speed
		if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
		{
			// creates curved result rather than a linear one giving a more organic speed change
			// note T in Lerp is clamped, so we don't need to clamp our speed
			_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
			_speed = Mathf.Round(_speed * 1000f) / 1000f;
		}
		else
		{
			_speed = targetSpeed;
		}
		_animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);

		Vector3 inputDirection = new Vector3(move.x, 0.0f, move.y).normalized;

		if (move != Vector2.zero)
		{
			_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
			float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

			// rotate to face input direction relative to camera position
			if (_rotateOnMove) {
				transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
			}
		}

		Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

		_controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

		if (_hasAnimator)
		{
			_animator.SetFloat(_animIDSpeed, _animationBlend);
			_animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
		}
	}

	private void JumpAndGravity()
	{
		if (Grounded)
		{
			// reset the fall timeout timer
			_fallTimeoutDelta = FallTimeout;

			// update animator if using character
			if (_hasAnimator)
			{
				_animator.SetBool(_animIDJump, false);
				_animator.SetBool(_animIDFreeFall, false);
			}

			// stop our velocity dropping infinitely when grounded
			if (_verticalVelocity < 0.0f)
			{
				_verticalVelocity = -2f;
			}

			// Jump
			if (canJump && jumpAction.triggered && _jumpTimeoutDelta <= 0.0f)
			{
				// the square root of H * -2 * G = how much velocity needed to reach desired height
				_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

				// update animator if using character
				if (_hasAnimator)
				{
					_animator.SetBool(_animIDJump, true);
				}
			}

			// jump timeout
			if (_jumpTimeoutDelta >= 0.0f)
			{
				_jumpTimeoutDelta -= Time.deltaTime;
			}
		}
		else
		{
			// reset the jump timeout timer
			_jumpTimeoutDelta = JumpTimeout;

			// fall timeout
			if (_fallTimeoutDelta >= 0.0f)
			{
				_fallTimeoutDelta -= Time.deltaTime;
			}
			else
			{
				// update animator if using character
				if (_hasAnimator)
				{
					_animator.SetBool(_animIDFreeFall, true);
				}
			}

			// if we are not grounded, do not jump
			//_input.jump = false;
		}

		// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
		if (_verticalVelocity < _terminalVelocity)
		{
			_verticalVelocity += Gravity * Time.deltaTime;
		}
	}

	public void Aim(InputAction.CallbackContext ctx)
	{
		if (aimAction.IsPressed()) {
			aimVirtualCamera.gameObject.SetActive(true);
			SetSensitivity(aimSensitivity);
			SetRotateOnMove(false);
			Vector3 worldAimTarget = mouseWorldPosition;
			worldAimTarget.y = transform.position.y;
			Vector3 aimDirection = (worldAimTarget - transform.position).normalized;
			transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
		}
		else {
			aimVirtualCamera.gameObject.SetActive(false);
			SetSensitivity(normalSensitivity);
			SetRotateOnMove(true);
		}
	}

	public void Shoot(InputAction.CallbackContext ctx)
	{
		if (shootAction.triggered) {
			audioSource.PlayOneShot(shootSound);
			if (lastHitTransform != null) {
				HealthSystem health = lastHitTransform.GetComponent<HealthSystem>();
				if (health != null) {
					//hit target
					Debug.Log("Target Hit :" + lastHitTransform.name);
					health.TakeDamage(shootDamage);
					//Instantiate(impactEffect, raycastHit.point, Quaternion.LookRotation(raycastHit.normal));
					//hitTransform.gameObject.GetComponent<Rigidbody>().AddForceAtPosition(new Vector3(10, 10, 10), raycastHit.point);
				}
				else {
					//hit something else
					Debug.Log("Target Miss :" + lastHitTransform.name);
				}
			}
			//gunshot sound propagation for enemies perception
			Collider[] enemies = Physics.OverlapSphere(transform.position, soundIntensity, ennemiesLayer);
			foreach (Collider enemy in enemies) {
				EnemyAI e = enemy.gameObject.GetComponent<EnemyAI>();
				if (e != null) {
					e.OnAware(this.gameObject.transform);
				}
			}
		}
	}

	private void DroneSwitch(InputAction.CallbackContext ctx)
    {
		if (DroneInstance == null) {
			NavMeshHit navHit;
			if (!NavMesh.SamplePosition(transform.position + Vector3.forward, out navHit, rapatriationRange, -1))
			{
				Debug.Log("Unable to launch the drone at this location.");
				return;
			}

			DroneInstance = Instantiate(DroneOriginal, navHit.position, transform.rotation);
			DroneInstance.GetComponent<DroneController>().SetOwner(gameObject.transform);
		}
		_playerInput.SwitchCurrentActionMap("Drone");
	}

	private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
	{
		if (lfAngle < -360f) lfAngle += 360f;
		if (lfAngle > 360f) lfAngle -= 360f;
		return Mathf.Clamp(lfAngle, lfMin, lfMax);
	}

	private void OnDrawGizmosSelected()
	{
		Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
		Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

		if (Grounded) Gizmos.color = transparentGreen;
		else Gizmos.color = transparentRed;
			
		// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
		Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
	}

	private void SetSensitivity(float newSensitivity)
    {
		Sensitivity = newSensitivity;
    }

	private void SetRotateOnMove(bool newRotateOnMove)
    {
		_rotateOnMove = newRotateOnMove;
    }

    private void OnApplicationFocus(bool focus)
    {
		Cursor.lockState = focus ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private int GetPlayerStealthProfile()
    {
		return _speed <= MoveSpeed ? 0 : 1;
    }

	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.CompareTag("Enemy"))
		{
			other.gameObject.GetComponent<EnemyAI>()?.OnAware(this.gameObject.transform);
		}
	}
}
