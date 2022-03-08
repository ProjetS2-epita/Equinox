using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DescManager : MonoBehaviour
{
    public static DescManager instance;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

   public void Apply(string desc)
    {
        gameObject.GetComponent<Text>().text = desc;
    }
}
