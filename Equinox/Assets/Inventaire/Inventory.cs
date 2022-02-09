using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    private List<ItemsManager> itemsList;


    [SerializeField] GameObject itemSlotContainer;
    [SerializeField] Transform itemSlotContainerParent;

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
        itemsList.Add(item);
    }

    public void UpdateItemsInventory()
    {
        foreach(Transform itemSlot in itemSlotContainerParent)
        {
            Destroy(itemSlot.gameObject);
        }
        
        foreach(ItemsManager item in itemsList)
        {
            RectTransform itemSlot = Instantiate(itemSlotContainer, itemSlotContainerParent).GetComponent<RectTransform>();
            Sprite itemImage = itemSlot.Find("Case Image").GetComponent<Sprite>();
            itemImage = item.itemsImage;
        }

    }
}
