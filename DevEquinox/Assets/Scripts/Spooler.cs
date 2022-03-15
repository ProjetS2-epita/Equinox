using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Spooler : MonoBehaviour
{

    #region Pools

    [System.Serializable]
    public class ItemPool {
        public string type;
        public GameObject poolObject;
        public uint amount;
        public bool expandable;
        public List<GameObject> itemsPooled;

        public ItemPool (string type, AsyncOperationHandle<GameObject> asyncOp, bool expandable, uint poolAmount = 0)
        {
            this.type = type;
            this.expandable = expandable;
            itemsPooled = new List<GameObject>();
            asyncOp.Completed += (completedOp) => {
                poolObject = completedOp.Result;
            };
        }
        public void Generate()
        {
            for (uint j = 0; j < amount; j++) {
                GameObject obj = Instantiate(poolObject);
                obj.SetActive(false);
                itemsPooled.Add(obj);
            }
        }
        public void DestroyPooled() {
            foreach (GameObject obj in itemsPooled) Destroy(obj);
        }

    }
    public void AddPool(ItemPool pool)
    {
        pools.Add(pool);
    }
    public void ReducePool(string type, int amount)
    {
        ItemPool pool = pools.Where(p => p.type == type) as ItemPool;
        if (pool == null) return;
        uint i = 0;
        foreach (GameObject obj in pool.itemsPooled.Where(o => !o.activeInHierarchy))
        {
            if (i == amount) return;
            pool.itemsPooled.Remove(obj);
            Destroy(obj);
            i++;
        }
    }
    public void RemovePool(string type)
    {
        ItemPool pool = pools.Where(p => p.type == type) as ItemPool;
        if (pool == null) return;
        foreach (GameObject obj in pool.itemsPooled)
        {
            pool.itemsPooled.Remove(obj);
            Destroy(obj);
        }
        pools.Remove(pool);
    }
    public ItemPool GetPool(string type)
    {
        ItemPool pool = null;
        bool found = false;
        for (int i = 0; i < pools.Count && !found; i++) {
            if (pools[i].type == type) {
                pool = pools[i];
                found = true;
            }
        }
        return pool;
    }
    public List<GameObject> GetFromPool(string type, int amount = 1)
    {
        List<GameObject> rt = new List<GameObject>();
        ItemPool pool = GetPool(type);
        if (pool == null) return rt;
        for (int i = 0; i < pool.itemsPooled.Count && amount > 0; i++) {
            Debug.Log("in gfp loop");
            if (!pool.itemsPooled[i].activeInHierarchy) {
                rt.Append(pool.itemsPooled[i]);
                amount--;
            }
        }
        return rt;
    }

    #endregion
    [SerializeField] private List<ItemPool> pools;

    #region spawnPoints

    [System.Serializable]
    public class SpawnPoint
    {
        public Vector3 position;
        public float radius;
        public int amount;
        public int priority;
        public string type;

        public SpawnPoint(Vector3 position, float radius, int amount, int priority, string type)
        {
            this.position = position;
            this.radius = radius;
            this.amount = amount;
            this.priority = priority;
            this.type = type;
        }
    }
    public void addSpawnPoint(SpawnPoint point)
    {
        if(spawnPoints.Contains(point)) return;
        spawnPoints.Add(point);
    }
    public void removeSpawnPoint(SpawnPoint point)
    {
        spawnPoints.Remove(point);
    }
    public void spawnAt(SpawnPoint point)
    {
        NavMeshHit hit;
        bool spawnPositionFound;
        List<GameObject> spawnables = GetFromPool(point.type, point.amount);
        Debug.Log($"before spawnAt loop {spawnables.Count}");
        for (int i = 0; i < spawnables.Count; i++) {
            Debug.Log($"spawnAt {i}");
            do {
                spawnPositionFound = NavMesh.SamplePosition(Random.insideUnitSphere * point.radius + point.position, out hit, point.radius, -1);
            } while(!spawnPositionFound);
            spawnables[i].transform.position = hit.position;
            spawnables[i].transform.Rotate(0f, Random.Range(0f, 360f), 0f);
            spawnables[i].SetActive(true);
        }
    }

    #endregion
    [SerializeField] private List<SpawnPoint> spawnPoints;

    
    
    void Start()
    {
        for (int i = 0; i < pools.Count; i++) {
            Debug.Log($"pool {i} : {pools[i].type} | {pools[i].itemsPooled.Count}");
            pools[i].Generate();
        }
        /*
        for (int i = 0; i <= spawnPoints.Count; i++) {
            spawnAt(spawnPoints[i]);
        }
        */
        spawnAt(spawnPoints[0]);


    }
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.35f);
        for (int i = 0; i < spawnPoints.Count; i++) {
            Gizmos.DrawSphere(spawnPoints[i].position, spawnPoints[i].radius);
            
        }
    }

    private void OnDestroy()
    {
        foreach(ItemPool pool in pools) pool.DestroyPooled();
    }
}
