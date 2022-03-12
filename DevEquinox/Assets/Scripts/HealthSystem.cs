using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
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
    }

    public void RestoreHealth(float amount)
    {
        health += Mathf.Min(maxHealth - health, amount);
    }
}
