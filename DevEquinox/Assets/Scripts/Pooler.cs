using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Pooler : MonoBehaviour
{
    [System.Serializable]
    public class ItemPool
    {
        public string type;
        public GameObject poolObject;
        public uint amount;
        public bool expandable;
    }
    [System.Serializable]
    public class SpawnPoint
    {
        public Vector3 position;
        public float radius;
    }

    public static Pooler Instance;
    [SerializeField] public List<ItemPool> pools;
    [SerializeField] public Dictionary<string, List<GameObject>> poolDict;
    [SerializeField] public List<SpawnPoint> spawnPoints;

    private void Awake() {
        Instance = this;
    }

    private void Start()
    {
        poolDict = new Dictionary<string, List<GameObject>>();
        for(int i = 0; i < pools.Count; i++) {
            List<GameObject> pooled = new List<GameObject>();
            for(int j = 0; j < pools[i].amount; j++) {
                GameObject obj = Instantiate(pools[i].poolObject);
                obj.SetActive(false);
                pooled.Add(obj);
            }
            poolDict.Add(pools[i].type, pooled);
        }

        foreach (SpawnPoint point in spawnPoints) {
            SpawnAt(GlobalAccess._E_zombie1, point);
        }

    }

    public void SpawnAt(string type, SpawnPoint point) 
    {
        if (!poolDict.ContainsKey(type)) throw new KeyNotFoundException();

        NavMeshHit hit;
        bool spawnPositionFound;
        List<GameObject> pool = poolDict[type], spawnables = new List<GameObject>();
        bool search = true;
        for (int i = 0; i < pool.Count && search; i++) {
            if (!pool[i].activeInHierarchy) {
                spawnables.Add(pool[i]);
                //search = false;
            }
        }
        Debug.Log($"{spawnables.Count} zombie");

        for (int i = 0; i < spawnables.Count; i++) {
            Debug.Log($"spawnAt {i}");
            do {
                spawnPositionFound = NavMesh.SamplePosition(Random.insideUnitSphere * point.radius + point.position, out hit, point.radius, -1);
            } while (!spawnPositionFound);
            spawnables[i].transform.position = hit.position;
            spawnables[i].transform.Rotate(0f, Random.Range(0f, 360f), 0f);
            spawnables[i].SetActive(true);
        }



    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.35f);
        for (int i = 0; i < spawnPoints.Count; i++) {
            Gizmos.DrawSphere(spawnPoints[i].position, spawnPoints[i].radius);
        }
    }

}
