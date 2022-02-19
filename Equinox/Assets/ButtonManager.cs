using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ButtonManager : MonoBehaviour
{
    public static ButtonManager instance;
    void Start()
    {
    }

    public void Send_Data(){
        Debug.Log("click");
        // On va chercher le modèle 3D du jeu
        GameObject obj = GameObject.Find("3D");
        
        // On cherche le nom et la description de l'item
        string name = obj.GetComponent<ItemsManager>().itemName;
        string desc = obj.GetComponent<ItemsManager>().itemDescription;
        ItemUpdate.instance.Display(name,desc);
    }
}
