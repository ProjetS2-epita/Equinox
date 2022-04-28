using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gyrotator : MonoBehaviour
{

    [SerializeField] Transform toRotate;
    [SerializeField] float rotationSpeed;


    // Update is called once per frame
    void Update() {
        toRotate.Rotate(Vector3.up, rotationSpeed);
    }
}
