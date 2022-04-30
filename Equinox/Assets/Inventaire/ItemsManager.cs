using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsManager : MonoBehaviour
{
    public enum ItemType { Bouffe,Arme,Projectile,Tenu,Technologie}

    public ItemType itemtype;
    public string itemName, itemDescription;
    public Sprite itemsImage;

    public enum AffectType { HP, Soif, Faim }
    public AffectType itemAffect;
    public int amountOfAffect;
    public GameObject Model;

    public bool isStackable;
    public int amount;
    // Start is called before the first frame update
    void Start()
    { // Gravity set
        gameObject.GetComponent<Rigidbody>().useGravity = true;
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Inventory.instance.AddItems(this);
            gameObject.SetActive(false);
            //SelfDestroy();
            MenuManager.instance.UpdateItemsInventory();
        }
        else if (other.CompareTag("Ground"))  // Le terrain
        {
            Debug.Log("Stop");
            gameObject.GetComponent<Rigidbody>().useGravity = false;
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
        } 
    }
    

  
    


    public void SelfDestroy()
    {
        Destroy(gameObject);
    }

}
