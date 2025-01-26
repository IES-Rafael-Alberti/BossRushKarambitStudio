using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private List<EnemyAction> allowedActions = new() { EnemyAction.Attack, EnemyAction.Reload, EnemyAction.Heal }; // Acciones permitidas para este enemigo
    [SerializeField] private EnemySpecialAttack specialAttack;
    [SerializeField] private int healAmount, maxAmmo, initialAmmo, reloadAmount;
    public int maxHealth, damage;
    [Range(0f, 1f)] public float accuracy, playerDodgeProbability, rifleAccuracy;
    [SerializeField] private AudioClip audioClipDamaged;
    private Player player;
    [HideInInspector] public EnemyAction actionChosen;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    [HideInInspector] public int currentHealth, currentAmmo, damageMultiplier;
    private TurnController turnController;

    private void Start()
    {
        turnController = FindObjectOfType<TurnController>();
        player = FindObjectOfType<Player>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;
        currentAmmo = initialAmmo;
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

        // Reiniciar los pesos al inicio del turno
        List<float> cardWeights = ResetCardWeights();

        while (selectedActions.Count < 3)
        {
            // Seleccionar una carta basada en pesos
            int randomIndex = GetWeightedRandomIndex(cardWeights);
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

    // Metodo para seleccionar un indice basado en pesos
    private int GetWeightedRandomIndex(List<float> weights)
    {
        float totalWeight = weights.Sum(); // Suma de todos los pesos
        float randomValue = Random.Range(0, totalWeight); // Generar un numero aleatorio en el rango de los pesos totales

        float cumulativeWeight = 0f;
        for (int i = 0; i < weights.Count; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue < cumulativeWeight)
                return i; // Devolver el indice seleccionado
        }

        return weights.Count - 1; // Devolver el ultimo indice como respaldo
    }

    // Metodo para reiniciar los pesos al inicio de cada turno
    private List<float> ResetCardWeights()
    {
        List<float> weights = new(new float[allowedActions.Count]);
        for (int i = 0; i < weights.Count; i++)
        {
            weights[i] = 1f; // Reiniciar todos los pesos al valor inicial
        }
        return weights;
    }

    // Metodo para evaluar la puntuacion de una accion
    private float EvaluateAction(EnemyAction action)
    {
        float score = 0f;

        // Formula personalizada para puntuar acciones
        switch (action)
        {
            case EnemyAction.Attack:
                if (currentAmmo <= 0)
                    score = 0; // No puede atacar si no tiene municion
                else
                    score += 10f * (1.5f - (FindObjectOfType<Player>().currentHealth / FindObjectOfType<Player>().maxHealth)); // Prioriza atacar cuando el jugador tiene menos vida
                break;

            case EnemyAction.Reload:
                if (currentAmmo >= maxAmmo)
                    score = 0; // No puede recargar si ya tiene municion completa
                else
                    score += 10f * (1.1f - (currentAmmo / maxAmmo)); // Prioriza recargar si el cargador tiene pocas balas
                break;

            case EnemyAction.Heal:
                if (currentHealth >= maxHealth)
                    score = 0; // No puede curarse si ya tiene salud completa
                else
                    score += 10f * (1.1f - (currentHealth / maxHealth)); // Prioriza curarse cuando el enemigo tiene poca vida
                break;

            case EnemyAction.Dodge:
                score += 8f; // Prioridad fija por evasion
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

        if (hitChance <= accuracy) // Si acierta, realiza el ataque
        {
            if (player.isDodging && hitChance <= accuracy - playerDodgeProbability) // Si esta esquivando el player, pero la precision esta dentro del rango, le da la bala
                StartCoroutine(AnimAttack(() => player.ReceiveDamage(damage * damageMultiplier)));
            else if (player.isDodging && hitChance > accuracy - playerDodgeProbability) // Si esta esquivando el player, pero no esta a rango de precision, no le da
                StartCoroutine(AnimAttack(() => Debug.Log("El ataque enemigo fallo.")));
            else // No esta el player esquivando y le da
                StartCoroutine(AnimAttack(() => player.ReceiveDamage(damage * damageMultiplier)));
        }
        else // Si falla, muestra un mensaje de fallo
            StartCoroutine(AnimAttack(() => Debug.Log("El ataque enemigo fallo.")));

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

            if (hitChance <= accuracy) // Si acierta, realiza el ataque
            {
                if (player.isDodging && hitChance <= accuracy - playerDodgeProbability) // Si esta esquivando el player, pero la precision esta dentro del rango, le da la bala
                    StartCoroutine(AnimAttack(() => player.ReceiveDamage(damage * damageMultiplier)));
                else if (player.isDodging && hitChance > accuracy - playerDodgeProbability) // Si esta esquivando el player, pero no esta a rango de precision, no le da
                    StartCoroutine(AnimAttack(() => Debug.Log("El ataque enemigo fallo.")));
                else // No esta el player esquivando y le da
                    StartCoroutine(AnimAttack(() => player.ReceiveDamage(damage * damageMultiplier)));
            }
            else // Si falla, muestra un mensaje de fallo
                StartCoroutine(AnimAttack(() => Debug.Log("El ataque enemigo fallo.")));
        }
    }

    private void RifleShot()
    {
        // Generar un valor aleatorio para determinar si el disparo acierta
        float hitChance = Random.Range(0f, 1f);

        if (hitChance <= rifleAccuracy)
        {
            if (player.isDodging && hitChance <= rifleAccuracy - playerDodgeProbability) // Si esta esquivando el player, pero la precision esta dentro del rango, le da la bala
                StartCoroutine(AnimAttack(() => player.ReceiveDamage(damage * damageMultiplier)));
            else if (player.isDodging && hitChance > rifleAccuracy - playerDodgeProbability) // Si esta esquivando el player, pero no esta a rango de precision, no le da
                StartCoroutine(AnimAttack(() => Debug.Log("El ataque enemigo fallo.")));
            else // No esta el player esquivando y le da
                StartCoroutine(AnimAttack(() => player.ReceiveDamage(damage * damageMultiplier)));
        }
        else // Si falla, muestra un mensaje de fallo
            StartCoroutine(AnimAttack(() => Debug.Log("El ataque fallo.")));
    }

    private void Dynamite()
    {
        // Generar un valor aleatorio para determinar si la dinamita acierta
        float hitChance = Random.Range(0f, 1f);

        if (hitChance <= accuracy)
        {
            if (player.isDodging && hitChance <= accuracy - playerDodgeProbability) // Si esta esquivando el player, pero la precision esta dentro del rango, le da la dinamita
                StartCoroutine(AnimAttack(() => player.ReceiveDamage(damage * damageMultiplier)));
            else if (player.isDodging && hitChance > accuracy - playerDodgeProbability) // Si esta esquivando el player, pero no esta a rango de precision, la dinamita le hace menos daño
                StartCoroutine(AnimAttack(() => player.ReceiveDamage(damage / 2 * damageMultiplier)));
            else // No esta el player esquivando y le da
                StartCoroutine(AnimAttack(() => player.ReceiveDamage(damage * damageMultiplier)));
        }
        else // Si falla, muestra un mensaje de fallo
            StartCoroutine(AnimAttack(() => Debug.Log("El ataque del enemigo fallo.")));
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
    public void Heal(int healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        turnController.UpdateEnemyHealthUI();
    }
    #endregion

    #region RECIBIR DAÑO
    public void ReceiveDamage(int damage)
    {
        // audioSource.PlayOneShot(audioClipDamaged);
        currentHealth -= damage;
        StartCoroutine(Damaged());
        turnController.UpdateEnemyHealthUI();
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

        Debug.LogWarning("Munición actual del enemigo: " + currentAmmo);
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