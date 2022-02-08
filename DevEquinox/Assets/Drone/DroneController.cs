using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SphereCollider))]
public class DroneController : MonoBehaviour
{

    private CinemachineVirtualCamera droneCam;
	private CharacterController _controller;
	private SphereCollider sphereCollider;
    [SerializeField] private float droneSensitivity;
    [SerializeField] private AudioClip bladeSound;
    [SerializeField] private bool LockCameraPosition = false;
	[SerializeField] private float MoveSpeed = 2.0f;
	private bool _rotateOnMove = true;
	private float _targetRotation = 0.0f;
	private float _rotationVelocity;
	private float _verticalVelocity;
	private AudioSource audioSource;
	private float _speed;

	//Inputs
	private PlayerInput _playerInput;
	private InputAction moveAction;
	private InputAction lookAction;
	private InputAction avatarAction;
	private InputActionMap droneMap;

	private const float _threshold = 0.01f;
	private GameObject _mainCamera;
	private float _cinemachineTargetYaw;
	private float _cinemachineTargetPitch;
	public float TopClamp = 70.0f;
	public float BottomClamp = -30.0f;
	public float CameraAngleOverride = 0.0f;
	public float SpeedChangeRate = 10.0f;
	public float RotationSmoothTime = 0.12f;



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
		droneMap = _playerInput.actions.FindActionMap("Drone", true);	//////////////////////////////////////////
		moveAction = droneMap.FindAction("DroneMove");
		lookAction = droneMap.FindAction("DroneLook");
		avatarAction = droneMap.FindAction("SwitchAvatar");
	}

	private void OnEnable()
	{
		avatarAction.performed += AvatarSwitch;
	}

	private void OnDisable()
	{
		avatarAction.performed -= AvatarSwitch;							//////////////////////////////////////////
	}

	void Start()
    {
		/*
		_hasAnimator = TryGetComponent(out _animator);
		AssignAnimationIDs();
		*/
	}

    void Update()
    {
		//_hasAnimator = TryGetComponent(out _animator);
		Move();

		//walk & sprint noise radius
		//sphereCollider.radius = GetPlayerStealthProfile() == 0 ? walkEnemyPerceptionRadius : sprintEnemyPerceptionRadius;
	}
	private void LateUpdate()
	{
		CameraRotation();
	}

	/*
	private void AssignAnimationIDs()
	{
		_animIDSpeed = Animator.StringToHash("Speed");
		_animIDGrounded = Animator.StringToHash("Grounded");
		_animIDJump = Animator.StringToHash("Jump");
		_animIDFreeFall = Animator.StringToHash("FreeFall");
		_animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
	}
	*/


	private void CameraRotation()
    {
		Debug.Log("Drone camera rotation");
		Vector2 look = lookAction.ReadValue<Vector2>();

        // if there is an input and camera position is not fixed
        if (look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            _cinemachineTargetYaw += look.x * Time.deltaTime * droneSensitivity;
            _cinemachineTargetPitch += look.y * Time.deltaTime * droneSensitivity;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }

	private void Move()
	{
		Debug.Log("Drone move");
		Vector2 move = moveAction.ReadValue<Vector2>();

		// set target speed based on move speed, sprint speed and if sprint is pressed

		// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

		// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
		// if there is no input, set the target speed to 0
		if (move == Vector2.zero) MoveSpeed = 0.0f;

		// a reference to the players current horizontal velocity
		float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

		float speedOffset = 0.1f;
		float inputMagnitude = move.magnitude;// : 1f;

		// accelerate or decelerate to target speed
		if (currentHorizontalSpeed < MoveSpeed - speedOffset || currentHorizontalSpeed > MoveSpeed + speedOffset)
		{
			// creates curved result rather than a linear one giving a more organic speed change
			// note T in Lerp is clamped, so we don't need to clamp our speed
			_speed = Mathf.Lerp(currentHorizontalSpeed, MoveSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

			// round speed to 3 decimal places
			_speed = Mathf.Round(_speed * 1000f) / 1000f;
		}
		else
		{
			_speed = MoveSpeed;
		}
		//_animationBlend = Mathf.Lerp(_animationBlend, MoveSpeed, Time.deltaTime * SpeedChangeRate);

		// normalise input direction
		Vector3 inputDirection = new Vector3(move.x, 0.0f, move.y).normalized;

		// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
		// if there is a move input rotate player when the player is moving
		if (move != Vector2.zero)
		{
			_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
			float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

			// rotate to face input direction relative to camera position
			if (_rotateOnMove)
			{
				transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
			}
		}


		Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

		// move the player
		_controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

		/*
		// update animator if using character
		if (_hasAnimator)
		{
			_animator.SetFloat(_animIDSpeed, _animationBlend);
			_animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
		}
		*/
	}

	private void AvatarSwitch(InputAction.CallbackContext ctx)
    {
        Debug.Log("Query switch from Drone to Avatar");
		//focusCam(false);
		_playerInput.SwitchCurrentActionMap("Player");
	}

	private void SetRotateOnMove(bool newRotateOnMove)
	{
		_rotateOnMove = newRotateOnMove;
	}

	private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
	{
		if (lfAngle < -360f) lfAngle += 360f;
		if (lfAngle > 360f) lfAngle -= 360f;
		return Mathf.Clamp(lfAngle, lfMin, lfMax);
	}

	public void focusCam(bool focus)
    {
		droneCam.enabled = focus;
    }

}
