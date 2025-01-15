using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public List<EnemyAction> allowedActions = new() { EnemyAction.Attack, EnemyAction.Reload, EnemyAction.Heal }; // Acciones permitidas para este enemigo
    public int healAmount, damage, maxHealth;
    public AudioClip audioClipDamaged;
    [HideInInspector] public Player player;
    [HideInInspector] public EnemyAction actionChosen;
    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public AudioSource audioSource;
    [HideInInspector] public int currentHealth;

    void Start()
    {
        player = FindObjectOfType<Player>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;
    }

    void Update()
    {

    }

    public virtual void ChooseAction()
    {
        // Asegurarse de que haya suficientes acciones permitidas
        if (allowedActions.Count < 3)
        {
            Debug.LogWarning("No hay suficientes acciones permitidas para elegir 3.");
            return;
        }

        // Crear una lista temporal para las 3 opciones seleccionadas
        List<EnemyAction> selectedActions = new();

        // Elegir 3 acciones únicas aleatoriamente
        while (selectedActions.Count < 3)
        {
            int randomIndex = Random.Range(0, allowedActions.Count);
            if (!selectedActions.Contains(allowedActions[randomIndex]))
            {
                selectedActions.Add(allowedActions[randomIndex]);
            }
        }

        // Elegir una acción aleatoria de las 3 seleccionadas
        int finalChoiceIndex = Random.Range(0, selectedActions.Count);
        actionChosen = selectedActions[finalChoiceIndex];

        Debug.Log($"Accion elegida: {actionChosen}");
    }

    public virtual void DoAction()
    {
        switch (actionChosen)
        {
            case EnemyAction.Attack:
                Attack();
                break;
            case EnemyAction.Heal:
                Heal(healAmount);
                break;
            default:
                Debug.LogWarning($"{name} intento ejecutar una accion no permitida: {actionChosen}");
                break;
        }
    }

    #region ATACAR
    public virtual void Attack()
    {
        StartCoroutine(AnimAttack(() => player.ReceiveDamage(damage)));
    }

    public virtual IEnumerator AnimAttack(System.Action onAnimationComplete)
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

    #region Curar
    private void Heal(int healAmount)
    {
        currentHealth += healAmount;
    }
    #endregion

    #region RECIBIR DAÑO
    public virtual void ReceiveDamage(int damage)
    {
        audioSource.PlayOneShot(audioClipDamaged);
        currentHealth -= damage;
        StartCoroutine(Damaged());
        if (currentHealth <= 0)
            Death();
    }

    public virtual void Death()
    {
        // Hacerlo IEnumerator, meterle animacion, desactivar collider y destruir
        Destroy(gameObject);
    }

    public IEnumerator Damaged()
    {
        spriteRenderer.material = GetComponent<Flicker>().flickerMaterial;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.material = GetComponent<Flicker>().originalMaterial;
    }
    #endregion
}

public enum EnemyAction
{
    Attack,
    Heal,
    Dodge,
    Reload,
    Protect
}