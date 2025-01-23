using UnityEngine;

public class Player : MonoBehaviour
{
    public int maxHealth, maxAmmo;
    [Range(0f, 1f)] public float accuracy, enemyDodgeProbability, rifleAccuracy;
    [HideInInspector] public int currentHealth, currentAmmo;
    [HideInInspector] public bool isDodging;
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

    // Recibir daño
    public void ReceiveDamage(int damage)
    {
        // Reducir la salud del player
        currentHealth -= damage;

        // Comprobar la salud y muerte del player
        if (currentHealth < 0)
            Death();

        // Actualizar el texto de la vida
        turnController.UpdateHealthText();
    }

    // Curacion del player
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        turnController.UpdateHealthText();
    }

    // Controlar la muerte del player, animaciones, efectos, banderas...
    private void Death()
    {
        currentHealth = 0;
        turnController.DetectOutcome();
    }
}
