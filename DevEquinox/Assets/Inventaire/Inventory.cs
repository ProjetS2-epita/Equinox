using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    public List<ItemsManager> itemsList;



    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        itemsList = new List<ItemsManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void AddItems(ItemsManager item)
    {
        if (item.isStackable)
        {

            bool itemAlreadyInInventory = false;

            foreach(ItemsManager itemInInventory in itemsList)
            {
                if(itemInInventory.itemName == item.itemName)
                {
                    itemInInventory.amount += item.amount;
                    itemAlreadyInInventory = true;
                }
            }

            if (!itemAlreadyInInventory)
            {
                itemsList.Add(item);
            }
        }
        else 
        {
            itemsList.Add(item);
        }
        


    }

  

    public List<ItemsManager> GetItemList()
    {
        return itemsList;
    }
}
