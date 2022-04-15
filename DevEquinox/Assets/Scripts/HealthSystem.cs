using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Image healthBar; 
    [SerializeField] private TextMeshProUGUI healthDisplay;
    private float health;
    public bool IsDead => health <= 0f;

    void Awake()
    {
        health = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (health <= 0) return;
        health -= amount;
        UpdateHealthBarValue();
    }

    public void RestoreHealth(float amount)
    {
        health += Mathf.Min(maxHealth - health, amount);
        UpdateHealthBarValue();
    }

    private void UpdateHealthBarValue()
    {
        if (healthBar == null || healthDisplay == null) return;
        healthBar.fillAmount = health / 100;
        healthDisplay.text = $"{Mathf.Ceil(health)} %";
    }
}
