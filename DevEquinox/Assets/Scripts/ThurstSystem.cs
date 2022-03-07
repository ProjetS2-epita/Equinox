using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThurstSystem : TimeChangingQ
{
    public float ThurstPercent { get { return currentQuantity; } set { currentQuantity = value; } }
    public float ThurstDecrease { get { return decreaseOverTime; } set { decreaseOverTime = value; } }
    public float ThurstIncrease { get { return increaseOverTime; } set { increaseOverTime = value; } }

    protected override void Start()
    {
        base.Start();
        ThurstDecrease = 2f;
        ThurstIncrease = 1f;
    }

    protected override void Update()
    {
        if (Failed) return;
        //Debug.Log($"Thurst : {Mathf.Round(currentQuantity*1000)/1000}%");
        base.Update();
        if (Failed) Debug.Log("System Failure: " + this.GetType().Name + " on " + gameObject.name);
    }
}
