using UnityEngine;

public class BatterySystem : TimeChangingQ
{
    public float ElectricityPercent { get { return currentQuantity; } set { currentQuantity = value; } }
    public float ElectricityDecrease { get { return decreaseOverTime; } set { decreaseOverTime = value; } }
    public float ElectricityIncrease { get { return increaseOverTime; } set { increaseOverTime = value; } }

    protected override void Start()
    {
        base.Start();
        ElectricityDecrease = 2f;
        ElectricityIncrease = 1f;
    }

    protected override void Update()
    {
        if (Failed) return;
        //Debug.Log($"Battery : {Mathf.Round(currentQuantity*1000)/1000}%");
        base.Update();
        if (Failed) Debug.Log("System Failure: " + this.GetType().Name + " on " + gameObject.name);
    }
}
