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
	[SerializeField] private bool _rotateOnMove = false;
	public float Gravity = -15.0f;
	private float _targetRotation = 0.0f;
	private float _rotationVelocity;
	private float _verticalVelocity;
	private AudioSource audioSource;
	private float _speed;
	Vector3 _velocity;
	public float dampingCoefficient = 5;

	//Inputs
	private PlayerInput _playerInput;
	private InputActionMap droneMap;
	private InputAction moveAction;
	private InputAction lookAction;
	private InputAction avatarAction;
	private bool activeCam;

	private const float _threshold = 0.01f;
	private GameObject _mainCamera;
	private float _cinemachineTargetYaw;
	private float _cinemachineTargetPitch;
	private Transform DroneCamTransform;
	public float TopClamp = 270f;
	public float BottomClamp = 90f;
	public float CameraAngleOverride = 0.0f;
	public float SpeedChangeRate = 10.0f;
	public float RotationSmoothTime = 0.12f;



	private void Awake()
	{
		if (_mainCamera == null)
		{
			_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
		}
		_controller = GetComponent<CharacterController>();
		audioSource = GetComponent<AudioSource>();
		sphereCollider = GetComponent<SphereCollider>();

		_playerInput = GameObject.FindGameObjectWithTag("GameManager").GetComponent<PlayerInput>();
		DroneCamTransform = GameObject.FindGameObjectWithTag("DroneCamera").transform;
		droneCam = GetComponentInChildren<CinemachineVirtualCamera>();
		droneMap = _playerInput.actions.FindActionMap("Drone", true);
		moveAction = droneMap.FindAction("DroneMove");
		lookAction = droneMap.FindAction("DroneLook");
		avatarAction = droneMap.FindAction("SwitchAvatar");
		activeCam = droneMap.enabled;
	}

	private void OnEnable()
	{

	}

	private void OnDisable()
	{

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
		if (droneMap.enabled != activeCam) {
			focusCam(droneMap.enabled);
			activeCam = droneMap.enabled;
		}

		if (!droneMap.enabled) return;
		Move();

		//walk & sprint noise radius
		//sphereCollider.radius = GetPlayerStealthProfile() == 0 ? walkEnemyPerceptionRadius : sprintEnemyPerceptionRadius;
	}
	private void LateUpdate()
	{
		if (!droneMap.enabled) return;
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
		Vector2 look = lookAction.ReadValue<Vector2>();

        if (look.sqrMagnitude >= _threshold && !LockCameraPosition) {
            _cinemachineTargetYaw += look.x * Time.deltaTime * droneSensitivity;
            _cinemachineTargetPitch += look.y * Time.deltaTime * droneSensitivity;
        }
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        transform.rotation = Quaternion.Euler(0, _cinemachineTargetYaw, 0.0f);
		DroneCamTransform.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);
	}

	private void Move()
	{
		Vector3 move = moveAction.ReadValue<Vector3>();

		float targetSpeed = MoveSpeed;
		if (move == Vector3.zero) targetSpeed = 0.0f;
		float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
		float speedOffset = 0.1f;
		float inputMagnitude = move.magnitude;
		if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			_speed = Mathf.Round(Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate) * 1000f) / 1000f;
		else _speed = targetSpeed;
		Vector3 inputDirection = new Vector3(move.x, 0.0f, move.z).normalized;
		if (move != Vector3.zero) {
			_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
			float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
			transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
		}

		Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * new Vector3(0.0f, move.y, 1f);
		Vector3 moveVector = targetDirection * (_speed * Time.deltaTime);
		_controller.Move(moveVector);
	}

	public void AvatarSwitch(InputAction.CallbackContext ctx)
    {
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
