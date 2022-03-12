using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pooler : MonoBehaviour
{

    [System.Serializable]
    public class ItemPool {
        public string type;
        public int poolAmount;
        public GameObject poolObject;
        public bool expandable;
        public List<GameObject> itemsPooled;

        ItemPool (string type, int poolAmount, GameObject poolObject, bool expandable)
        {
            this.type = type;
            this.poolAmount = poolAmount;
            this.poolObject = poolObject;
            this.expandable = expandable;
            itemsPooled = new List<GameObject>();
        }
    }

    public List<ItemPool> pools;

    void Start()
    {
        for(int i = 0; i < pools.Count; i++) {
            for(uint j = 0; j < pools[i].poolAmount; j++) {
                GameObject obj = Instantiate(pools[i].poolObject);
                obj.SetActive(false);
                pools[i].itemsPooled.Add(obj);
            }
        }
    }



}
