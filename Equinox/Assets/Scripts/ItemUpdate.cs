using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ItemUpdate : MonoBehaviour
{
    // Start is called before the first frame update
    public static ItemUpdate instance;

    void Start()
    {
        instance = this;
    }

    public void Display(string name,string desc)
    {
        Debug.Log("im in!");
        // Get the gameObject
        GameObject itemName = GameObject.Find("Item Name Text");
        GameObject itemDesc = GameObject.Find("Item Description Text");
        // Set the value 
        itemName.GetComponent<Text>().text = name;
        itemDesc.GetComponent<Text>().text = desc;
        
    }
}
