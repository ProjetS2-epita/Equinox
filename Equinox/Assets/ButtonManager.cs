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
        string name = gameObject.GetComponentInChildren<ItemsManager>().itemName;
        string desc = gameObject.GetComponentInChildren<ItemsManager>().itemDescription;
        DescManager.instance.Apply(desc);
        NameManager.instance.Apply(name);
    }
}
