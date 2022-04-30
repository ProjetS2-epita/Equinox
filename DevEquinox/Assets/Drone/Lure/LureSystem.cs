using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

public class LureSystem : MonoBehaviour
{
    private SphereCollider _noiseSystem;
    private AudioSource _lureSource;
    public AudioClip _lurer;
    private List<Collider> _collided;
    private float _noiseRadius = 50f;
    private float _lifeSpan = 20f;

    private void Awake()
    {
        _collided = new List<Collider>();
        _noiseSystem = GetComponent<SphereCollider>();
        _lureSource = GetComponent<AudioSource>();
        _lureSource.loop = true;
        _lureSource.minDistance = 1f;
        _lureSource.maxDistance = _noiseRadius;
        _lureSource.spread = 360f;
        Addressables.LoadAssetAsync<AudioClip>(GlobalAccess._lureSound).Completed += (asyncOp) => {
            _lurer = asyncOp.Result;
            _lureSource.clip = _lurer;
            _lureSource.Play();
        };
    }

    void Start()
    {
        _noiseSystem.enabled = true;
        _noiseSystem.radius = _noiseRadius;
        StartCoroutine(ReleaseAfterDelay());
    }

    private IEnumerator ReleaseAfterDelay()
    {
        yield return new WaitForSeconds(_lifeSpan);
        Addressables.Release(_lurer);
        Addressables.ReleaseInstance(transform.parent.gameObject);
    }

    protected IEnumerator AlertEnemies(Transform transform)
    {
        Debug.Log($"AlertEnemies2 from {gameObject.name}");
        foreach (Collider collider in _collided) {
            if (collider != null) {
                if (collider.TryGetComponent(out EnemyAI AI)) AI.OnAware(transform);
                yield return null;
            }
            _collided.Remove(collider);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(GlobalAccess._Enemy)) return;
        Debug.Log(other);
        _collided.Append(other);
        StartCoroutine(AlertEnemies(transform));
    }
}
