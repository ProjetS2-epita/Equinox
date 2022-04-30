using UnityEngine;
using Cinemachine;
using StarterAssets;
//[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(ThirdPersonController))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(StarterAssetsInputs))]
public class ThirdPerson_ShooterController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField] private float normalSensitivity = 1f;
    [SerializeField] private float aimSensitivity = 0.3f;
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
    [SerializeField] private LayerMask ennemiesLayer;
    [SerializeField] private Transform aimTrackerTransform;
    [SerializeField] private float shootDamage;
    public AudioClip shootSound;
    public float soundIntensity = 100f;
    public float walkEnemyPerceptionRadius = 1.5f;
    public float sprintEnemyPerceptionRadius = 4f;
    //public GameObject impactEffect;

    private AudioSource audioSource;
    private CapsuleCollider sphereCollider;
    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;

    private void Awake()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        audioSource = GetComponent<AudioSource>();
        sphereCollider = GetComponent<CapsuleCollider>();
    }
    
    private void Update()
    {
        Vector3 mouseWorldPosition = Vector3.zero;
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        Transform hitTransform = null;
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask)) {
            aimTrackerTransform.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
            hitTransform = raycastHit.transform;
        }

        Aim(mouseWorldPosition);
        
        Shoot(hitTransform, raycastHit);

        //walk & sprint noise radius
        //sphereCollider.radius = thirdPersonController.GetPlayerStealthProfile() == 0 ? walkEnemyPerceptionRadius : sprintEnemyPerceptionRadius;
    }
    
    void Aim(Vector3 mouseWorldPosition)
    {
        if (starterAssetsInputs.aim)
        {
            aimVirtualCamera.gameObject.SetActive(true);
            thirdPersonController.SetSensitivity(aimSensitivity);
            thirdPersonController.SetRotateOnMove(false);
            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;
            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
        }
        else
        {
            aimVirtualCamera.gameObject.SetActive(false);
            thirdPersonController.SetSensitivity(normalSensitivity);
            thirdPersonController.SetRotateOnMove(true);
        }
    }

    void Shoot(Transform hitTransform, RaycastHit raycastHit)
    {
        if (starterAssetsInputs.shoot)
        {
            audioSource.PlayOneShot(shootSound);
            if (hitTransform != null)
            {
                HealthSystem health = hitTransform.GetComponent<HealthSystem>();
                if (health != null)
                {
                    //hit target
                    Debug.Log("Target Hit :" + hitTransform.name);
                    health.TakeDamage(shootDamage);
                    //Instantiate(impactEffect, raycastHit.point, Quaternion.LookRotation(raycastHit.normal));
                    //hitTransform.gameObject.GetComponent<Rigidbody>().AddForceAtPosition(new Vector3(10, 10, 10), raycastHit.point);
                }
                else
                {
                    //hit something else
                    Debug.Log("Target Miss :" + hitTransform.name);
                }
            }
            //gunshot sound propagation for enemies perception
            Collider[] enemies = Physics.OverlapSphere(transform.position, soundIntensity, ennemiesLayer);
            foreach (Collider enemy in enemies)
                enemy.gameObject.GetComponent<EnemyAI>()?.OnAware();

            starterAssetsInputs.shoot = false;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("COLLIDED : " + other.gameObject.name);
        if (other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<EnemyAI>().OnAware();
        }
    }

}
