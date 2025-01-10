using UnityEngine;

public class Player : MonoBehaviour
{
    public int maxHealth;
    private int currentHealth;

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
