using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TurretController : MonoBehaviour
{
    [SerializeField] private float VerticalUpperClamp;
    [SerializeField] private float VerticalBottomClamp;
    [SerializeField] private float HorizontalRotationSpeed = 2f;
    [SerializeField] private float detectionRange = 50f;
    [SerializeField] private float cooldown = 0.5f;
    [SerializeField] private float lightSpan = 0.05f;
    [SerializeField] private float treshold = 0.1f;
    [SerializeField] private float damage = 75f;
    [SerializeField] private float soundPropagation = 75f;
    [SerializeField] private Vector3 impactForce = new Vector3(500f, 5f, 0f);
    private Transform target;
    private bool ready = true;
    private bool targetLocked = false;

    [SerializeField] private AudioClip FireSound;
    [SerializeField] private GameObject BloodTrace;
    [SerializeField] private Transform FireSpot;
    [SerializeField] private Transform HorizontalPivot;
    [SerializeField] private Transform VerticalPivot;
    [SerializeField] private LayerMask attackableLayer;
    private Collider[] detected = new Collider[100];
    private Collider[] toAlert = new Collider[200];
    private ParticleSystem[] particles;
    private AudioSource audioSource;
    private Light FlashEffect;
    private float baseIntensity;

    private void Start() {
        audioSource = GetComponentInChildren<AudioSource>();
        FlashEffect = GetComponentInChildren<Light>();
        particles = GetComponentsInChildren<ParticleSystem>();
        baseIntensity = FlashEffect.intensity;
        FlashEffect.intensity = 0f;
        audioSource.clip = FireSound;
        audioSource.loop = false;
        audioSource.spread = 360f;
        audioSource.minDistance = 0f;
        audioSource.maxDistance = soundPropagation;
    }


    void Update() {
        if (targetLocked) Aim(target);
        else {
            target = Nearest();
            targetLocked = target != null && target.gameObject.activeInHierarchy;
        }
    }


    private void Aim(Transform target) {
        if (target == null || !target.gameObject.activeInHierarchy) {
            targetLocked = false;
            return;
        }

        Quaternion lookY = Quaternion.LookRotation(target.position - HorizontalPivot.position);
        lookY.x = 0f;
        lookY.z = 0f;
        HorizontalPivot.rotation = Quaternion.Slerp(HorizontalPivot.rotation, lookY * Quaternion.Euler(0f, 180f, 0f), HorizontalRotationSpeed * Time.deltaTime);

        float vert = VerticalPivot.position.y - target.position.y;
        if (vert == 0f) return;
        float hor = Vector3.Distance(VerticalPivot.position, new Vector3(target.position.x, VerticalPivot.position.y, target.position.z));
        float angle = (vert > 0 ? -180f : -90) + Mathf.Rad2Deg * Mathf.Atan(vert < 0 ? Mathf.Abs(vert) / hor : hor / Mathf.Abs(vert));
        if (angle > VerticalUpperClamp || angle < VerticalBottomClamp) return;
        VerticalPivot.localEulerAngles = new Vector3(angle, 0f, 0f);

        Vector3 dir = -(target.position - HorizontalPivot.position).normalized;
        if (ready && Mathf.Abs(Clamped180(VerticalPivot.localEulerAngles.x) - angle) < treshold && (Vector3.Dot(dir, HorizontalPivot.forward) > 1 - treshold)) Fire();
    }

    private void Fire() {
        if (target == null || !target.gameObject.activeInHierarchy) {
            targetLocked = false;
            return;
        }
        if (Physics.Linecast(FireSpot.position, target.position, out RaycastHit hit, attackableLayer, QueryTriggerInteraction.Ignore)) {
            HealthSystem health = hit.transform.gameObject.GetComponent<HealthSystem>();
            if (health != null) {
                NetworkServer.Spawn(Instantiate(BloodTrace, hit.point, Quaternion.identity));
                health.TakeDamage(damage);
                if (health.IsDead) {
                    hit.transform.gameObject.GetComponent<EnemyAI>().Die(hit.point, hit.normal + impactForce);
                }
            }
        }
        ActivateEffects();
        targetLocked = false;
        StartCoroutine(CoolDown());
        StartCoroutine(AlertEnemies());
    }
    
    
    private Transform Nearest()
    {
        Transform nearestTransform = null;
        float nearestDistance = detectionRange;
        Physics.OverlapSphereNonAlloc(HorizontalPivot.position, detectionRange, detected, attackableLayer);
        for (uint i = 0; i < detected.Length; i++) {
            if (detected[i] == null || !detected[i].gameObject.activeInHierarchy) continue;
            float distance = Vector3.Distance(HorizontalPivot.position, detected[i].gameObject.transform.position);
            if (distance < nearestDistance) {
                nearestDistance = distance;
                nearestTransform = detected[i].gameObject.transform;
            }
        }
        return nearestTransform;
    }

    private void ActivateEffects() {
        audioSource.Play();
        for (int i = 0; i < particles.Length; i++) particles[i].Play();
        StartCoroutine(Blink());
    }

    private IEnumerator Blink() {
        FlashEffect.intensity = baseIntensity;
        yield return new WaitForSeconds(lightSpan);
        FlashEffect.intensity = 0;
    }

    private IEnumerator CoolDown() {
        ready = false;
        yield return new WaitForSeconds(cooldown);
        ready = true;
    }

    private IEnumerator AlertEnemies()
    {
        for (uint i = 0; i < Physics.OverlapSphereNonAlloc(VerticalPivot.position, soundPropagation, toAlert, attackableLayer); i++) {
            if (toAlert[i] != null && toAlert[i].gameObject.activeInHierarchy) {
                if (toAlert[i].TryGetComponent(out EnemyAI AI)) AI.OnAware(VerticalPivot);
                if (i % 25 == 0) yield return null;
            }
        }
        yield return null;
    }

    private float Clamped180(float angle) => angle > 180 ? -angle + 180 : angle;

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.35f);
        Gizmos.DrawSphere(HorizontalPivot.position, detectionRange);
        Gizmos.color = new Color(1.0f, 0.99f, 0.22f, 0.35f);
        Gizmos.DrawSphere(HorizontalPivot.position, soundPropagation);
    }
}
