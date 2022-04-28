using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeRotation : MonoBehaviour
{
    private Transform selfTransform;
    void Start()
    {
        selfTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles = Vector3.zero;
    }
}
