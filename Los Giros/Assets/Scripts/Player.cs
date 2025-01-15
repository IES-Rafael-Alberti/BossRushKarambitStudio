using UnityEngine;

public class Player : MonoBehaviour
{
    public int maxHealth, maxAmmo;
    [HideInInspector] public int currentHealth, currentAmmo;

    void Start()
    {
        currentHealth = maxHealth;
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
