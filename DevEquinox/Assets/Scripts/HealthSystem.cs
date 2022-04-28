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
    [SerializeField] private AudioClip _HurtSound;
    private AudioSource _audioSource;
    private float health;
    public bool IsDead => health <= 0f;

    void Awake()
    {
        health = maxHealth;
        UpdateHealthBarValue();
        _audioSource = GetComponent<AudioSource>();
        _audioSource.spread = 360f;
        _audioSource.loop = false;
        _audioSource.spatialBlend = 1f;
    }

    public void TakeDamage(float amount)
    {
        if (_HurtSound != null) _audioSource.PlayOneShot(_HurtSound);
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
        healthBar.fillAmount = health / maxHealth;
        healthDisplay.text = $"{Mathf.Ceil(health)} %";
    }
}
