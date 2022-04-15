using System.Collections;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Mirror;

public abstract class Controllers : NetworkBehaviour
{
    [SerializeField] protected float _SpeedChangeRate;
    [SerializeField] protected float _Sensitivity;
    [SerializeField] protected float _MoveSpeed;
    [SerializeField] public float _rapatriationRange;
    [SerializeField] protected float _Gravity;
    protected float _speed;
    protected string _Instantiable;
    protected string _MapName;
    protected float _TopClamp;
    protected float _BottomClamp;
    protected float _CameraAngleOverride;
    protected float _targetRotation;
    protected float _rotationVelocity;
    protected float _RotationSmoothTime;
    protected float _cinemachineTargetYaw;
    protected float _cinemachineTargetPitch;
    protected float _threshold;
    protected float _actualTime;
    protected float _triggetStayTimeout = 0f;
    

    protected Transform _mainCamera;
    protected Transform selfTransform;
    protected CinemachineVirtualCamera _Camera;
    protected LayerMask _enemiesLayer;
    protected AudioClip _Sound;
    protected AudioSource _audioSource;
    protected SphereCollider _sphereCollider;
    protected Animator _animator;
    protected CharacterController _controller;
    protected Collider[] _colliders;
    public Image _droneIcon;
    public Image _dronePower;

    protected PlayerInput _playerInput;
    protected InputActionMap _Map;
    protected InputAction _moveAction;
    protected InputAction _lookAction;
    protected InputAction _switchAction;


    public override void OnStartClient()
    {
        base.OnStartClient();
        selfTransform = transform;
        _colliders = new Collider[GlobalAccess._maxEnemiesInSight];
        _Gravity = -25.0f;
        _CameraAngleOverride = 0.0f;
        _targetRotation = 0.0f;
        _RotationSmoothTime = 0.12f;
        _threshold = 0.01f;
        _enemiesLayer = 1 << LayerMask.NameToLayer(GlobalAccess._Enemy);
        _mainCamera = GameObject.FindGameObjectWithTag(GlobalAccess._MainCamera).transform;
        _controller = GetComponent<CharacterController>();
        _audioSource = GetComponent<AudioSource>();
        _sphereCollider = GetComponent<SphereCollider>();
        _animator = GetComponent<Animator>();

        _playerInput = GameObject.FindGameObjectWithTag(GlobalAccess._GameManager).GetComponentInChildren<PlayerInput>();
        _Map = _playerInput.actions.FindActionMap(_MapName, true);
        _moveAction = _Map.FindAction("Move");
        _lookAction = _Map.FindAction("Look");
        _audioSource.clip = _Sound;
    }


    protected virtual void Update()
    {
        _actualTime = Time.deltaTime;
    }

    protected virtual void LateUpdate()
    {
        CameraRotation();
    }

    protected virtual void CameraRotation()
    {
        Vector2 look = _lookAction.ReadValue<Vector2>();

        if (look.sqrMagnitude >= _threshold) {
            _cinemachineTargetYaw += look.x * _actualTime * _Sensitivity;
            _cinemachineTargetPitch += look.y * _actualTime * _Sensitivity;
        }
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, _BottomClamp, _TopClamp);
    }

    protected abstract void Move();

    public abstract void SwitchControl(InputAction.CallbackContext ctx);

    protected static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    protected IEnumerator AlertEnemies(float radius)
    {
        for (uint i = 0; i < Physics.OverlapSphereNonAlloc(selfTransform.position, radius, _colliders, _enemiesLayer); i++) {
            if (_colliders[i] != null) {
                if (_colliders[i].TryGetComponent(out EnemyAI AI)) AI.OnAware(selfTransform);
                if (i % 25 == 0) yield return null;
            }
        }
        yield return null;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(GlobalAccess._Enemy)) return;

        _triggetStayTimeout += _actualTime;
        if(_triggetStayTimeout > GlobalAccess._triggerUpdateRate) {
            Debug.Log("Enemy alerted");
            if (other.TryGetComponent(out EnemyAI AI)) AI.OnAware(selfTransform);
            _triggetStayTimeout = 0;
        }
    }
}
