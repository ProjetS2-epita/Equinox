using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using UnityEngine.AddressableAssets;
using Mirror;

public class FP_Controller : Controllers
{
	private HealthSystem _healthSystem;
	private float _scopeRange = 100;
	public float _SprintSpeed = 7f;
	public float _JumpHeight = 1.2f;
	public float _JumpTimeout = 0.50f;
	public float _FallTimeout = 0.20f;
	private float _jumpTimeoutDelta;
	private float _fallTimeoutDelta;

	[SerializeField] private bool _Grounded = true;
	public float _GroundedOffset = -0.14f;
	public float _GroundedRadius = 0.28f;
	public LayerMask _GroundLayers;

	private bool _rotateOnMove = true;
	[SerializeField] private float _StdFOV = 55f;
	[SerializeField] private float _normalSensitivity = 1f;
	[SerializeField] private float _AimFOV = 20f;
	[SerializeField] private float _aimSensitivity = 0.3f;
	[SerializeField] private GameObject _AimCamera;

	[SerializeField] private float _shootDamage;
	[SerializeField] private GameObject BloodTrace;
	[SerializeField] private Vector3 _impactDamage = new Vector3(400f, 0.01f, 0f);
	[SerializeField] private GameObject _ghostEffect;
	public float _gunshotSoundPropagation = 100f;
	public float _walkEnemyPerceptionRadius = 1.5f;
	public float _sprintEnemyPerceptionRadius = 4f;

	private float _verticalVelocity;
	private float _terminalVelocity = 53.0f;


	private InputAction _jumpAction;
	private InputAction _sprintAction;
	private InputAction _aimAction;
	private InputAction _shootAction;

	private GameObject _droneInstance;


	private void OnEnable()
	{
		if (!isLocalPlayer) return;
		_shootAction.performed += Shoot;
		_aimAction.performed += Aim;
		_switchAction.performed += SwitchControl;
	}

	private void OnDisable()
	{
		if (!isLocalPlayer) return;
		_shootAction.performed -= Shoot;
		_aimAction.performed -= Aim;
		_switchAction.performed -= SwitchControl;
	}


	public override void OnStartClient()
	{
		if (!isLocalPlayer) return;
		gameObject.AddComponent<AudioListener>();
		_Camera.enabled = true;
		_AimCamera.SetActive(false);
		_ghostEffect.SetActive(false);

		_healthSystem = GetComponent<HealthSystem>();
		_AimCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = _AimFOV;
		_Camera.m_Lens.FieldOfView = _StdFOV;
		_scopeRange = _Camera.m_Lens.FarClipPlane;
		_MapName = GlobalAccess._Player;
		_TopClamp = 90.0f;
		_BottomClamp = -90.0f;
		_SpeedChangeRate = 8f;
		_Sensitivity = 1f;
		_MoveSpeed = 8f;
		_rapatriationRange = 5f;
		base.OnStartClient();
		_sphereCollider.radius = _walkEnemyPerceptionRadius;
		_audioSource.maxDistance = _walkEnemyPerceptionRadius;
		_audioSource.minDistance = 1f;
		_droneInstance = null;
		_jumpAction = _Map.FindAction("Jump");
		_sprintAction = _Map.FindAction("Sprint");
		_aimAction = _Map.FindAction("Aim");
		_shootAction = _Map.FindAction("Shoot");
		_switchAction = _Map.FindAction("SwitchDrone");
		OnEnable();
		_jumpTimeoutDelta = _JumpTimeout;
		_fallTimeoutDelta = _FallTimeout;
	}


	protected override void Update()
	{
		if (!isLocalPlayer) return;

		if (_healthSystem.IsDead) {
			OnDisable();
			GameStateManager.Instance.DeathScreen();
			gameObject.SetActive(false);
			return;
		}
		if (!_Map.enabled) return;
		base.Update();
		JumpAndGravity();
		GroundedCheck();
		Move();


		//walk & sprint noise radius
		_sphereCollider.radius = GetPlayerStealthProfile() == 0 ? _walkEnemyPerceptionRadius : _sprintEnemyPerceptionRadius;
	}

	protected override void LateUpdate()
	{
		if (!isLocalPlayer || !_Map.enabled) return;
		base.LateUpdate();
	}


	private void GroundedCheck()
	{
		// set sphere position, with offset
		Vector3 spherePosition = new Vector3(selfTransform.position.x, selfTransform.position.y - _GroundedOffset, selfTransform.position.z);
		_Grounded = Physics.CheckSphere(spherePosition, _GroundedRadius, _GroundLayers, QueryTriggerInteraction.Ignore);
	}

	protected override void CameraRotation()
	{
		base.CameraRotation();
		if (_AimCamera.activeSelf) _AimCamera.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + _CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
		else _Camera.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + _CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
		
	}

	protected override void Move()
	{
		Vector2 move = _moveAction.ReadValue<Vector2>().normalized;

		float targetSpeed = _sprintAction.IsPressed() ? _SprintSpeed : _MoveSpeed;

		if (move == Vector2.zero) targetSpeed = 0.0f;

		float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

		float speedOffset = 0.1f;
		float inputMagnitude = move.magnitude;

		// accelerate or decelerate to target speed
		if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
		{
			// creates curved result rather than a linear one giving a more organic speed change
			// note T in Lerp is clamped, so we don't need to clamp our speed
			_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, _actualTime * _SpeedChangeRate);
			_speed = Mathf.Round(_speed * 1000f) / 1000f;
		}
		else
		{
			_speed = targetSpeed;
		}

		Vector3 inputDirection = new Vector3(move.x, 0.0f, move.y).normalized;

		if (move != Vector2.zero)
		{
			_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
			float rotation = Mathf.SmoothDampAngle(selfTransform.eulerAngles.y, _targetRotation, ref _rotationVelocity, _RotationSmoothTime);

			// rotate to face input direction relative to camera position
			if (_rotateOnMove)
			{
				selfTransform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
			}
		}

		Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

		_controller.Move(targetDirection.normalized * (_speed * _actualTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * _actualTime);
	}

	private void JumpAndGravity()
	{
		if (_Grounded)
		{
			// reset the fall timeout timer
			_fallTimeoutDelta = _FallTimeout;

			// stop our velocity dropping infinitely when grounded
			if (_verticalVelocity < 0.0f)
			{
				_verticalVelocity = -2f;
			}

			// Jump
			if (_jumpAction.triggered && _jumpTimeoutDelta <= 0.0f)
			{
				// the square root of H * -2 * G = how much velocity needed to reach desired height
				_verticalVelocity = Mathf.Sqrt(_JumpHeight * -2f * _Gravity);
			}

			// jump timeout
			if (_jumpTimeoutDelta >= 0.0f)
			{
				_jumpTimeoutDelta -= _actualTime;
			}
		}
		else
		{
			// reset the jump timeout timer
			_jumpTimeoutDelta = _JumpTimeout;

			// fall timeout
			if (_fallTimeoutDelta >= 0.0f)
			{
				_fallTimeoutDelta -= _actualTime;
			}

			// if we are not grounded, do not jump
			//_input.jump = false;
		}

		// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
		if (_verticalVelocity < _terminalVelocity)
		{
			_verticalVelocity += _Gravity * _actualTime;
		}
	}

	public void Aim(InputAction.CallbackContext ctx)
	{
		if (_aimAction.IsPressed())
		{
			_AimCamera.SetActive(true);
			SetSensitivity(_aimSensitivity);
			SetRotateOnMove(false);
		}
		else
		{
			_AimCamera.SetActive(false);
			SetSensitivity(_normalSensitivity);
			SetRotateOnMove(true);
		}
	}

	public void Shoot(InputAction.CallbackContext ctx)
	{
		if (_shootAction.triggered)
		{
			Addressables.LoadAssetAsync<AudioClip>(GlobalAccess._gunshotSound).Completed += (asyncOp) => {
				_audioSource.PlayOneShot(asyncOp.Result);
				Addressables.Release(asyncOp);
			};

			Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
			Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
			if (Physics.Raycast(ray, out RaycastHit Hit, _scopeRange, -1, QueryTriggerInteraction.Ignore)) {
				CmdShoot(netIdentity, Hit.transform.gameObject.GetComponent<NetworkIdentity>(), Hit.point, _shootDamage, _impactDamage);
			}
		}
	}


	[Command]
	private void CmdShoot(NetworkIdentity identity, NetworkIdentity lastHit, Vector3 hitPoint, float damage, Vector3 impactForce)
	{
		if (lastHit != null)
		{
			HealthSystem health = lastHit.gameObject.GetComponent<HealthSystem>();
			if (health != null)
			{
				NetworkServer.Spawn(Instantiate(BloodTrace, hitPoint, Quaternion.identity));
				health.TakeDamage(damage);
				if (health.IsDead) lastHit.gameObject.GetComponent<EnemyAI>().Die(hitPoint, impactForce);
			}
		}
		StartCoroutine(AlertEnemies(identity.gameObject.transform, _gunshotSoundPropagation));
	}



	public override void SwitchControl(InputAction.CallbackContext ctx)
	{
		if (_droneInstance == null) {
			NavMeshHit navHit;
			if (!NavMesh.SamplePosition(selfTransform.position + Vector3.forward, out navHit, _rapatriationRange, -1))
			{
				Debug.Log("Unable to launch the drone at this location.");
				return;
			}
			Addressables.InstantiateAsync(GlobalAccess._dronePrefab, navHit.position, selfTransform.rotation).Completed += (asyncOp) => {
				DroneController dc = (_droneInstance = asyncOp.Result).GetComponent<DroneController>();
				dc._owner = selfTransform;
				dc._rapatriationRange = _rapatriationRange;
				NetworkServer.Spawn(dc.gameObject);
				_playerInput.SwitchCurrentActionMap("Drone");
			};
		}
		else _playerInput.SwitchCurrentActionMap("Drone");
	}

#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
		Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

		if (_Grounded) Gizmos.color = transparentGreen;
		else Gizmos.color = transparentRed;

		// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
		Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - _GroundedOffset, transform.position.z), _GroundedRadius);
	}
#endif

	private void SetSensitivity(float newSensitivity)
	{
		_Sensitivity = newSensitivity;
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
		return _speed <= _MoveSpeed ? 0 : 1;
	}

}