using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mother class for all time-changing quantities system (battery, starving, thirst)
/// </summary>
public class TimeChangingQ : MonoBehaviour
{
    protected float initQuantity;
    protected float currentQuantity;
    protected float decreaseOverTime;
    protected float increaseOverTime;
    protected float qMult = 10E-4f;
    protected bool isActive;
    protected bool Failed => currentQuantity <= 0f;
    protected bool Maxed => currentQuantity >= initQuantity;


    protected virtual void Start()
    {
        initQuantity = 100f;
        currentQuantity = initQuantity;
        isActive = true;
    }

    protected virtual void Update()
    {
        currentQuantity += isActive ? qMult * -decreaseOverTime : qMult * increaseOverTime;
        currentQuantity = Mathf.Clamp(currentQuantity, 0f, initQuantity);
    }
}
