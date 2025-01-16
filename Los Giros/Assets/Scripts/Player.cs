using UnityEngine;

public class Player : MonoBehaviour
{
    public int maxHealth, maxAmmo;
    [Range(0f, 1f)] public float accuracy;
    [HideInInspector] public int currentHealth, currentAmmo;

    void Start()
    {
        currentHealth = maxHealth;
        currentAmmo = maxAmmo;
    }

    void Update()
    {
        
    }

    public void ReceiveDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Current player health: " + currentHealth);
    }
}
