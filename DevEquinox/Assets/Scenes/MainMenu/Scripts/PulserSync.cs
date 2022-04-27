using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulserSync : MonoBehaviour
{
    public float Frequency = 1f;
    public float BaseAlpha = 0.15f;

    private void Awake()
    {
        AlphaPulser[] pulsers = GetComponentsInChildren<AlphaPulser>();
        float delay = Frequency * (pulsers.Length - 1) + Frequency;
        for (int i = 0; i < pulsers.Length; i++) {
            if (!pulsers[i].Synchronize) continue;
            pulsers[i].Frequency = Frequency;
            pulsers[i].BaseAlpha = BaseAlpha;
            pulsers[i].Shift = Frequency * i;
            pulsers[i].Delay = delay;
        }
    }

}
