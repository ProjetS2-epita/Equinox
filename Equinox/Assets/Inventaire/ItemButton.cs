using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ItemButton : MonoBehaviour
{
    public static ItemButton instance;

    private void Start()
    {
        instance = this;
     }

    public void Display(Transform item)
    {
        GameObject model = item.Find("3DObject").gameObject;
        MenuManager.instance.itemName.text = model.GetComponent<ItemsManager>().itemName;
        MenuManager.instance.itemDescription.text = model.GetComponent<ItemsManager>().itemDescription;
    }
    

}
