using UnityEngine;

public class Player : MonoBehaviour
{
    public int maxHealth, maxAmmo;
    [Range(0f, 1f)] public float accuracy;
    [HideInInspector] public int currentHealth, currentAmmo;
    private TurnController turnController;

    void Start()
    {
        turnController = FindObjectOfType<TurnController>();
        currentHealth = maxHealth;
        currentAmmo = maxAmmo;
    }

    void Update()
    {

    }

    public void ReceiveDamage(int damage)
    {
        currentHealth -= damage;
        turnController.UpdateHealthText();
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        turnController.UpdateHealthText();
    }
}
