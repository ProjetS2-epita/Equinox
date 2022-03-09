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
	[SerializeField] private Transform owner;
	public void SetOwner(Transform parent) => owner = parent;
	private float rapatriationRange = 10f;
	public GameObject lure;
	public float maxDistance = 100f;
    private CinemachineVirtualCamera droneCam;
	private CharacterController _controller;
    [SerializeField] private float droneSensitivity;
    [SerializeField] private AudioClip bladeSound;
	[SerializeField] private float MoveSpeed = 2.0f;
	public float Gravity = -15.0f;
	private float _targetRotation = 0.0f;
	private float _rotationVelocity;
    private AudioSource audioSource;
	private SphereCollider noisePropagation;
	public float baseNoiseDistance = 20f;
	private float _speed;
    public float dampingCoefficient = 5f;

	//Inputs
	private PlayerInput _playerInput;
	private InputActionMap droneMap;
	private InputAction moveAction;
	private InputAction lookAction;
	private InputAction lureDrop;
	private InputAction switchAvatar;
	private bool activeCam;

	private const float _threshold = 0.01f;
	private Transform _mainCamera;
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
			_mainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
		}
		_controller = GetComponent<CharacterController>();
		audioSource = GetComponent<AudioSource>();
		noisePropagation = GetComponent<SphereCollider>();
		noisePropagation.radius = baseNoiseDistance;
		audioSource.maxDistance = baseNoiseDistance;
		audioSource.minDistance = 1f;
		owner = null;

		_playerInput = GameObject.FindGameObjectWithTag("GameManager").GetComponent<PlayerInput>();
		DroneCamTransform = GameObject.FindGameObjectWithTag("DroneCamera").transform;
		droneCam = GetComponentInChildren<CinemachineVirtualCamera>();
		droneMap = _playerInput.actions.FindActionMap("Drone", true);
		moveAction = droneMap.FindAction("DroneMove");
		lookAction = droneMap.FindAction("DroneLook");
		lureDrop = droneMap.FindAction("LureDrop");
		switchAvatar = droneMap.FindAction("SwitchAvatar");

		activeCam = droneMap.enabled;
	}

	private void OnEnable()
	{
		lureDrop.started += LureDrop;
		switchAvatar.performed += SwitchAvatar;
	}

	private void OnDisable()
	{
		lureDrop.started -= LureDrop;
		switchAvatar.performed -= SwitchAvatar;
	}

	void Start()
    {
		audioSource.loop = true;
		audioSource.clip = bladeSound;
		StartCoroutine(FadeIn(audioSource, 5));
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

	private void CameraRotation()
    {
		Vector2 look = lookAction.ReadValue<Vector2>();

        if (look.sqrMagnitude >= _threshold) {
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
			_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.eulerAngles.y;
			float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
			transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
		}
		Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * new Vector3(0.0f, move.y, 1f);
		if (move.x == 0 && move.z == 0) {
			targetDirection.x = 0;
			targetDirection.y *= 10;
			targetDirection.z = 0;
        }
		targetDirection *= (_speed * Time.deltaTime);

		// Distance Control (signal strength)
		if (owner == null || Vector3.Distance(owner.position, gameObject.transform.position + targetDirection) > maxDistance) return;

		_controller.Move(targetDirection);
		noisePropagation.radius = baseNoiseDistance + baseNoiseDistance * _controller.velocity.magnitude / 10f;
	}

	public void SwitchAvatar(InputAction.CallbackContext ctx)
    {
		_playerInput.SwitchCurrentActionMap("Player");
		if (Vector3.Distance(transform.position, owner.position) < rapatriationRange)
			Destroy(gameObject);
	}

	public void LureDrop(InputAction.CallbackContext ctx)
    {
		Instantiate(lure, gameObject.transform.position, transform.rotation).GetComponentInChildren<LureSystem>();
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

	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.CompareTag("Enemy"))
		{
			other.gameObject.GetComponent<EnemyAI>().OnAware(this.gameObject.transform);
		}
	}

	public static IEnumerator FadeIn(AudioSource audioSource, float FadeTime)
	{
		audioSource.Play();
		audioSource.volume = 0f;
		while (audioSource.volume < 1)
		{
			audioSource.volume += Time.deltaTime / FadeTime;
			yield return null;
		}
	}

}
