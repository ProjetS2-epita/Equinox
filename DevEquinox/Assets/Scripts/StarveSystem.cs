using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarveSystem : TimeChangingQ
{
    public float SatietyPercent { get { return currentQuantity; } set { currentQuantity = value; } }
    public float SatietyDecrease { get { return decreaseOverTime; } set { decreaseOverTime = value; } }
    public float SatietyIncrease { get { return increaseOverTime; } set { increaseOverTime = value; } }

    protected override void Start()
    {
        base.Start();
        SatietyDecrease = 1f;
        SatietyIncrease = 1f;
    }

    protected override void Update()
    {
        if (Failed) return;
        //Debug.Log($"Satiety : {Mathf.Round(currentQuantity*1000)/1000}%");
        base.Update();
        if (Failed) Debug.Log("System Failure: " + this.GetType().Name + " on " + gameObject.name);
    }
}
