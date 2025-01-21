using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private List<EnemyAction> allowedActions = new() { EnemyAction.Attack, EnemyAction.Reload, EnemyAction.Heal }; // Acciones permitidas para este enemigo
    [SerializeField] private EnemySpecialAttack specialAttack;
    [SerializeField] private int healAmount, damage, maxHealth, maxAmmo, reloadAmount;
    [Range(0f, 1f)] public float accuracy;
    [SerializeField] private AudioClip audioClipDamaged;
    private Player player;
    private EnemyAction actionChosen;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    [HideInInspector] public int currentHealth, currentAmmo;
    private TurnController turnController;

    private void Start()
    {
        turnController = FindObjectOfType<TurnController>();
        player = FindObjectOfType<Player>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;
        currentAmmo = maxAmmo;
    }

    private void Update()
    {

    }

    public void ChooseAction()
    {
        // Verificar que haya suficientes acciones permitidas
        if (allowedActions.Count < 3)
            return;

        // Crear una lista temporal para las 3 opciones seleccionadas
        List<EnemyAction> selectedActions = new();

        while (selectedActions.Count < 3)
        {
            int randomIndex = Random.Range(0, allowedActions.Count);
            if (!selectedActions.Contains(allowedActions[randomIndex]))
                selectedActions.Add(allowedActions[randomIndex]);
        }

        // Evaluar cada accion segun una formula personalizada
        EnemyAction bestAction = EnemyAction.Attack; // Valor inicial por defecto
        float highestScore = float.MinValue;

        foreach (var action in selectedActions)
        {
            float score = EvaluateAction(action);
            if (score > highestScore)
            {
                highestScore = score;
                bestAction = action;
            }
        }

        // Asignar la mejor accion
        actionChosen = bestAction;
    }

    // Metodo para evaluar la puntuacion de una accion
    private float EvaluateAction(EnemyAction action)
    {
        float score = 0f;

        // Fórmula personalizada para puntuar acciones
        switch (action)
        {
            case EnemyAction.Attack:
                if (currentAmmo <= 0)
                    score = 0; // No puede atacar si no tiene municion
                else
                    score += 15f * (1.5f - (FindObjectOfType<Player>().currentHealth / FindObjectOfType<Player>().maxHealth)); // Prioriza atacar cuando el jugador tiene menos vida
                break;

            case EnemyAction.Reload:
                if (currentAmmo >= maxAmmo)
                    score = 0; // No puede recargar si ya tiene municion completa
                else
                    score += 10f * (1.5f - (currentAmmo / maxAmmo)); // Prioriza recargar si el cargador tiene pocas balas
                break;

            case EnemyAction.Heal:
                if (currentHealth >= maxHealth)
                    score = 0; // No puede curarse si ya tiene salud completa
                else
                    score += 10f * (1.5f - (currentHealth / maxHealth)); // Prioriza curarse cuando el enemigo tiene poca vida
                break;

            case EnemyAction.Dodge:
                score += 8f; // Prioridad fija por evasion
                break;

            case EnemyAction.Protect:
                score += 5f * (1.5f - (currentHealth / maxHealth)); // Prioriza protegerse cuando tiene poca vida
                break;

            case EnemyAction.SpecialAttack:
                score += 20f; // Maxima prioridad si esta disponible
                break;

            default:
                score += Random.Range(1f, 5f); // Aleatoriedad para acciones no especificadas
                break;
        }

        return score;
    }

    public void DoAction()
    {
        switch (actionChosen)
        {
            case EnemyAction.Attack:
                Attack();
                break;
            case EnemyAction.Heal:
                Heal(healAmount);
                break;
            case EnemyAction.Reload:
                Reload();
                break;
            case EnemyAction.SpecialAttack:
                SpecialAttack();
                break;
            default:
                break;
        }
    }

    #region ATACAR
    private void Attack()
    {
        // Generar un valor aleatorio para determinar si el disparo acierta
        float hitChance = Random.Range(0f, 1f);

        if (hitChance <= accuracy)
        {
            // Si acierta, realiza el ataque
            StartCoroutine(AnimAttack(() => player.ReceiveDamage(damage)));
        }
        else
        {
            // Si falla, muestra un mensaje de fallo
            StartCoroutine(AnimAttack(() => Debug.Log("El ataque enemigo falló.")));
        }

        // Reducir la municion independientemente de si acierta o falla
        currentAmmo -= 1;
    }

    private void SpecialAttack()
    {
        switch (specialAttack)
        {
            case EnemySpecialAttack.DoubleShot:
                DoubleShot();
                break;
            case EnemySpecialAttack.RifleShot:
                RifleShot();
                break;
            case EnemySpecialAttack.Dynamite:
                Dynamite();
                break;
            default:
                break;
        }
    }

    private void DoubleShot()
    {
        for (int i = 0; i < 2; i++)
        {
            // Generar un valor aleatorio para determinar si el disparo acierta
            float hitChance = Random.Range(0f, 1f);

            if (hitChance <= accuracy)
            {
                // Si acierta, realiza el ataque
                StartCoroutine(AnimAttack(() => player.ReceiveDamage(damage)));
            }
            else
            {
                // Si falla, muestra un mensaje de fallo
                StartCoroutine(AnimAttack(() => Debug.Log("El ataque enemigo fallo.")));
            }

            // Reducir la municion independientemente de si acierta o falla
            currentAmmo -= 1;
        }
    }

    private void RifleShot()
    {
        // Generar un valor aleatorio para determinar si el disparo acierta
        float hitChance = Random.Range(0f, 1f);

        if (hitChance <= 0.90f)
        {
            // Si acierta, realiza el ataque
            StartCoroutine(AnimAttack(() => player.ReceiveDamage(damage)));
            Debug.Log("¡Ataque rifle exitoso! El jugador recibio daño.");
        }
        else
        {
            // Si falla, muestra un mensaje de fallo
            StartCoroutine(AnimAttack(() => Debug.Log("El ataque fallo.")));
        }

        // Reducir la municion independientemente de si acierta o falla
        currentAmmo -= 1;
    }

    private void Dynamite()
    {
        // Generar un valor aleatorio para determinar si la dinamita acierta
        float hitChance = Random.Range(0f, 1f);

        if (hitChance <= accuracy)
        {
            // Si acierta, realiza el ataque
            StartCoroutine(AnimAttack(() => player.ReceiveDamage(damage)));
            Debug.Log("¡Ataque dinamita exitoso! El jugador recibio daño.");
        }
        else
        {
            // Si falla, muestra un mensaje de fallo
            StartCoroutine(AnimAttack(() => Debug.Log("El ataque del enemigo falló.")));
        }

        // Reducir la municion independientemente de si acierta o falla
        currentAmmo -= 1;
    }

    private IEnumerator AnimAttack(System.Action onAnimationComplete)
    {
        // Tamaño original
        Vector3 escalaOriginal = transform.localScale;

        // Tamaño deformado (puedes ajustar los valores para mas o menos deformacion)
        Vector3 escalaDeformada = new(escalaOriginal.x * 1.65f, escalaOriginal.y * 0.65f, escalaOriginal.z);

        // Duracion total de la animacion
        float duracion = 0.2f;

        // Tiempo para deformarse
        float tiempoDeformacion = duracion / 2;

        // Fase 1: Escalar hacia la deformacion
        float tiempo = 0;
        while (tiempo < tiempoDeformacion)
        {
            transform.localScale = Vector3.Lerp(escalaOriginal, escalaDeformada, tiempo / tiempoDeformacion);
            tiempo += Time.deltaTime;
            yield return null;
        }

        // Asegurarnos de alcanzar exactamente la escala deformada
        transform.localScale = escalaDeformada;

        // Ejecutar accion despues de la animacion de ataque
        onAnimationComplete?.Invoke();

        // Fase 2: Volver a la escala original
        tiempo = 0;
        while (tiempo < tiempoDeformacion)
        {
            transform.localScale = Vector3.Lerp(escalaDeformada, escalaOriginal, tiempo / tiempoDeformacion);
            tiempo += Time.deltaTime;
            yield return null;
        }

        // Asegurarnos de restaurar la escala original
        transform.localScale = escalaOriginal;
    }
    #endregion

    #region CURAR
    private void Heal(int healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }
    #endregion

    #region RECIBIR DAÑO
    public void ReceiveDamage(int damage)
    {
        // audioSource.PlayOneShot(audioClipDamaged);
        currentHealth -= damage;
        Debug.LogWarning("Vida actual del enemigo: " + currentHealth);
        StartCoroutine(Damaged());
        if (currentHealth <= 0)
            Death();
    }

    private void Death()
    {
        turnController.DetectOutcome(); // Detectar el resultado del duelo
        // Hacerlo IEnumerator, meterle animacion, desactivar collider y destruir
        Destroy(gameObject);
    }

    private IEnumerator Damaged()
    {
        spriteRenderer.material = GetComponent<Flicker>().flickerMaterial;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.material = GetComponent<Flicker>().originalMaterial;
    }
    #endregion

    #region RECARGAR
    private void Reload()
    {
        currentAmmo += reloadAmount;
        if (currentAmmo > maxAmmo)
            currentAmmo = maxAmmo;
    }
    #endregion
}

public enum EnemyAction
{
    Attack,
    Heal,
    Dodge,
    Reload,
    Protect,
    SpecialAttack
}

public enum EnemySpecialAttack
{
    DoubleShot,
    RifleShot,
    Dynamite
}