using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int maxHealth, maxAmmo, initialAmmo;
    [Range(0f, 1f)] public float accuracy, enemyDodgeProbability, rifleAccuracy;
    [SerializeField] private float dodgeDistance = 1f; // Distancia que se movera hacia abajo
    [SerializeField] private float dodgeDuration = 0.2f; // Duracion del movimiento hacia abajo
    [SerializeField] private float dodgeWaitTime = 0.1f; // Tiempo que esperara antes de regresar
    [SerializeField] private float returnDuration = 0.2f; // Duracion del movimiento de regreso
    [HideInInspector] public int currentHealth, currentAmmo;
    [HideInInspector] public bool isDodging;
    private TurnController turnController;
    private PlayerSounds playerSounds;

    void Start()
    {
        turnController = FindObjectOfType<TurnController>();
        playerSounds = FindObjectOfType<PlayerSounds>();
        currentHealth = maxHealth;
        currentAmmo = initialAmmo;
    }

    // Recibir daño
    public void ReceiveDamage(int damage)
    {
        // Reducir la salud del player
        currentHealth -= damage;
        FindObjectOfType<Enemy>().damageMultiplier = 1;

        // Comprobar la salud y muerte del player
        if (currentHealth < 0)
            Death();

        // Actualizar el texto de la vida
        turnController.UpdatePlayerHealthUI();
    }

    // Curacion del player
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        turnController.UpdatePlayerHealthUI();
    }

    // Método para iniciar la esquiva
    public void Dodge()
    {
        StartCoroutine(DodgeCoroutine());
    }

    // Corrutina para manejar la animacion de esquiva
    private IEnumerator DodgeCoroutine()
    {
        Vector3 originalPosition = transform.position; // Posicion original del jugador
        Vector3 dodgePosition = new(originalPosition.x, originalPosition.y - dodgeDistance, originalPosition.z);

        // Mover hacia abajo
        float elapsedTime = 0f;

        playerSounds.PlayDodgeSound();

        while (elapsedTime < dodgeDuration)
        {
            transform.position = Vector3.Lerp(originalPosition, dodgePosition, elapsedTime / dodgeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = dodgePosition;

        // Esperar un breve momento
        yield return new WaitForSeconds(dodgeWaitTime);

        // Regresar a la posicion original
        elapsedTime = 0f;
        while (elapsedTime < returnDuration)
        {
            transform.position = Vector3.Lerp(dodgePosition, originalPosition, elapsedTime / returnDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPosition;
    }

    // Controlar la muerte del player, animaciones, efectos, banderas...
    private void Death()
    {
        currentHealth = 0;
        turnController.DetectOutcome();
    }
}
