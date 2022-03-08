using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageManager : MonoBehaviour
{
    public static ImageManager instance;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    public void Apply(Sprite img)
    {
        gameObject.GetComponent<Image>().sprite = img;
    }
}
