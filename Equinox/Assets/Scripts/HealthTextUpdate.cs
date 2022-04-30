using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthTextUpdate : MonoBehaviour
{
    
    void Update()
    {
        gameObject.GetComponent<Text>().text = PlayerStats.instance.currentHP.ToString();
    }
}
