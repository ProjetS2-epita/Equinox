using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float health;

    void Awake()
    {
        health = maxHealth;
    }

    void Update()
    {
        
    }

    public void TakeDamage(float amount)
    {
        if (health <= 0) return;
        health -= amount;
        //if (health <= 0f) Die();
    }

    public void RestoreHealth(float amount)
    {
        health += Mathf.Min(maxHealth - health, amount);
    }

    public bool IsDead => health <= 0f;

    void Die()
    {
        Destroy(gameObject);
    }

}
