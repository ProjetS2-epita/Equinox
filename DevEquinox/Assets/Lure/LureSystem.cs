using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LureSystem : MonoBehaviour
{
    private SphereCollider noiseSystem;
    private AudioSource lureSource;
    public AudioClip lurer;
    public float noiseRadius = 500f;
    public float lifeSpan = 5f;

    private void Awake()
    {
        noiseSystem = GetComponent<SphereCollider>();
        lureSource = GetComponent<AudioSource>();
        lureSource.clip = lurer;
        lureSource.loop = true;
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
            Debug.Log("collided");
            other.gameObject.GetComponent<EnemyAI>().OnAware(this.gameObject.transform);
        }
    }
}
