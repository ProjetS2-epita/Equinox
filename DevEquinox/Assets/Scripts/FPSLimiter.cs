using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FPSLimiter : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = Mathf.Min(Screen.currentResolution.refreshRate, 75);
    }
}
