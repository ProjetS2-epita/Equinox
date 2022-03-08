using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameManager : MonoBehaviour
{
    public static NameManager instance;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    public void Apply(string name)
    {
        gameObject.GetComponent<Text>().text = name;
    }
}
