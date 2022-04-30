using System.Collections;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.AddressableAssets;
using Mirror;

public class DroneController : Controllers
{
	[SerializeField] public Transform _owner;
	public GameObject _lure;
	
	public float _maxTravelDistance = 100f;
	public float _baseNoiseDistance = 20f;
    public float _dampingCoefficient = 5f;

	//Inputs
	private InputAction _lureDrop;

	private bool _activeCam;
	private Transform _DroneCamTransform;

    public override void OnStartClient()
    {
		Debug.Log($"start is local : {_owner.GetComponent<ThirdPersonController>().isLocalPlayer}");
		if (!_owner.GetComponent<ThirdPersonController>().isLocalPlayer)
		{
			gameObject.SetActive(false);
			return;
		}
		_MapName = GlobalAccess._Drone;
		_TopClamp = 180f;
		_BottomClamp = 90f;
		_SpeedChangeRate = 3f;
		_Sensitivity = 1.5f;
		_MoveSpeed = 14f;
		base.OnStartClient();
		_sphereCollider.radius = _baseNoiseDistance;
		_audioSource.maxDistance = _baseNoiseDistance;
		_audioSource.minDistance = 1f;
		_owner = null;

		_DroneCamTransform = GameObject.FindGameObjectWithTag(GlobalAccess._DroneCamera).transform;
		_Camera = GetComponentInChildren<CinemachineVirtualCamera>();
		_lureDrop = _Map.FindAction("LureDrop");
		_switchAction = _Map.FindAction("SwitchAvatar");
		OnEnable();
		_activeCam = _Map.enabled;
		_audioSource.loop = true;
		Addressables.LoadAssetAsync<AudioClip>(GlobalAccess._droneFlyingSound).Completed += (asyncOp) => {
			_Sound = asyncOp.Result;
			_audioSource.clip = _Sound;
			StartCoroutine(FadeIn(5));
		};
    }


	private void OnEnable()
	{
		if (!isLocalPlayer) return;
		_lureDrop.started += LureDrop;
		_switchAction.performed += SwitchControl;
	}

	private void OnDisable()
	{
		if (!isLocalPlayer) return;
		_lureDrop.started -= LureDrop;
		_switchAction.performed -= SwitchControl;
	}

    protected override void Update()
    {
		//Debug.Log($"update is local: {_owner.GetComponent<ThirdPersonController>().isLocalPlayer}");
		//if (!_owner.GetComponent<ThirdPersonController>().isLocalPlayer) return;
		if (_Map.enabled != _activeCam) {
			FocusCam(_Map.enabled);
			_activeCam = _Map.enabled;
		}

		if (!_Map.enabled) return;
		base.Update();
		Move();
	}

	protected override void LateUpdate()
	{
		if (!isLocalPlayer || !_Map.enabled) return;
		base.LateUpdate();
	}

	protected override void CameraRotation()
    {
		base.CameraRotation();
		selfTransform.rotation = Quaternion.Euler(0f, _cinemachineTargetYaw, 0f);
		_DroneCamTransform.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0f);
	}

	protected override void Move()
	{
		Vector3 move = _moveAction.ReadValue<Vector3>().normalized;
		float targetSpeed = _MoveSpeed;
		if (move == Vector3.zero) targetSpeed = 0.0f;
		float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
		float speedOffset = 0.1f;
		float inputMagnitude = move.magnitude;

		if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			_speed = Mathf.Round(Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, _actualTime * _SpeedChangeRate) * 1000f) / 1000f;
		else _speed = targetSpeed;
		Vector3 inputDirection = new Vector3(move.x, 0.0f, move.z).normalized;
		if (move != Vector3.zero) {
			_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.eulerAngles.y;
			float rotation = Mathf.SmoothDampAngle(selfTransform.eulerAngles.y, _targetRotation, ref _rotationVelocity, _RotationSmoothTime);
			selfTransform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
		}
		Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * new Vector3(0.0f, move.y, 1f);

		targetDirection *= (_speed * _actualTime);

		// Distance Control (signal strength)
		if (_owner == null || Vector3.Distance(_owner.position, selfTransform.position + targetDirection) > _maxTravelDistance) return;

		_controller.Move(targetDirection);
		_sphereCollider.radius = _baseNoiseDistance + _baseNoiseDistance * _controller.velocity.magnitude / 10f;
	}

	public override void SwitchControl(InputAction.CallbackContext ctx)
    {
		_playerInput.SwitchCurrentActionMap(GlobalAccess._Player);
		if (Vector3.Distance(selfTransform.position, _owner.position) < _rapatriationRange) {
			NetworkServer.UnSpawn(gameObject);
			Addressables.Release(_Sound);
			Addressables.ReleaseInstance(gameObject);
		}
	}

	public void LureDrop(InputAction.CallbackContext ctx)
    {
		Addressables.InstantiateAsync(GlobalAccess._lurePrefab, selfTransform.position, selfTransform.rotation);
	}

	private void FocusCam(bool focus)
    {
		_Camera.enabled = focus;
    }

	private IEnumerator FadeIn(float FadeTime)
	{
		_audioSource.Play();
		_audioSource.volume = 0f;
		while (_audioSource.volume < 1) {
			_audioSource.volume += _actualTime / FadeTime;
			yield return null;
		}
	}

}
