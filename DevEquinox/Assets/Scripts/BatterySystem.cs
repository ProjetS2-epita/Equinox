using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatterySystem : TimeChangingQ
{
    protected override void Start()
    {
        initQuantity = 100f;
        decreaseOverTime = 2;
        increaseOverTime = 1;
        base.Start();
    }

    protected override void Update()
    {
        if (Failed) return;
        //Debug.Log($"Battery : {Mathf.Round(currentQuantity*1000)/1000}%");
        base.Update();
        if (Failed) Debug.Log("System Failure: " + this.GetType().Name + " on " + gameObject.name);
    }
}
