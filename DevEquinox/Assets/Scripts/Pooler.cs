using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Mirror;

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
        //public Vector3 position;
        public Transform transform;
        public float radius;
        public uint amount;
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

    public void SpawnAt(string type, SpawnPoint point, uint amount = 0) 
    {
        if (!poolDict.ContainsKey(type)) throw new KeyNotFoundException();
        if (amount == 0) amount = point.amount;
        List<GameObject> pool = poolDict[type], spawnables = new List<GameObject>();
        for (int i = 0; i < pool.Count && amount > 0; i++) {
            if (!pool[i].activeInHierarchy) {
                spawnables.Add(pool[i]);
                amount--;
            }
        }
        NavMeshHit hit;
        bool spawnPositionFound;
        for (int i = 0; i < spawnables.Count; i++) {
            do {
                spawnPositionFound = NavMesh.SamplePosition(Random.insideUnitSphere * point.radius + point.transform.position, out hit, point.radius, -1);
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
            Gizmos.DrawSphere(spawnPoints[i].transform.position, spawnPoints[i].radius);
        }
    }

}
