using UnityEngine;

public class LureSystem : MonoBehaviour
{
    private SphereCollider noiseSystem;
    private AudioSource lureSource;
    public AudioClip lurer;
    private float noiseRadius = 50f;
    private float lifeSpan = 20f;

    private void Awake()
    {
        noiseSystem = GetComponent<SphereCollider>();
        lureSource = GetComponent<AudioSource>();
        lureSource.clip = lurer;
        lureSource.loop = true;
        lureSource.minDistance = 1f;
        lureSource.maxDistance = noiseRadius;
        lureSource.spread = 360f;
    }

    void Start()
    {
        noiseSystem.enabled = true;
        noiseSystem.radius = noiseRadius;
        lureSource.Play();
        Destroy(transform.parent.gameObject, lifeSpan);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<EnemyAI>().OnAware(gameObject.transform);
        }
    }
}
