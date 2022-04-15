using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Mother class for all time-changing quantities system (battery, starving, thirst)
/// </summary>
public class TimeChangingQ : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI barValue;
    [SerializeField] public Image QuantityDisplay;
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
        UpdateDisplayValue();
    }

    private void UpdateDisplayValue()
    {
        if (QuantityDisplay != null) QuantityDisplay.fillAmount = currentQuantity / 100;
        if (barValue != null) barValue.text = $"{Mathf.Ceil(currentQuantity)} %";
    }
}
