using System.Collections;
using UnityEngine;

public class Carta : MonoBehaviour
{
    public int id, damage, healAmount;
    public bool isPlayable = false;
    public ActionType actionType;
    public SpecialAttackType specialAttackType;
    [SerializeField] private AudioClip audioClip, audioClipSonidoCarta;
    /*[HideInInspector]*/ public bool isSelected = false;

    private void Start()
    {

    }

    public void DoAction()
    {
        switch (actionType)
        {
            case ActionType.Shot:
                Shot();
                Debug.Log("DISPARO");
                break;
            case ActionType.Heal:
                Heal(healAmount);
                Debug.Log("CURACION");
                break;
            case ActionType.Reload:
                Reload();
                Debug.Log("RECARGA");
                break;
            case ActionType.SpecialAttack:
                SpecialAttack();
                Debug.Log("DISPARO ESPECIAL");
                break;
            case ActionType.Dodge:
                Debug.Log("ESQUIVA");
                break;
            default:
                Debug.LogWarning($"{name} intento ejecutar una accion no permitida: {actionType}");
                break;
        }
    }

    private void Shot()
    {
        Debug.Log("SE DISPARA AL ENEMIGO");
        Enemy enemy = FindObjectOfType<Enemy>();
        Player player = FindObjectOfType<Player>();
        // Generar un valor aleatorio para determinar si el disparo acierta
        float hitChance = Random.Range(0f, 1f);

        if (hitChance <= player.accuracy)
        {
            // Si acierta, realiza el ataque
            StartCoroutine(AnimAttack(() => enemy.ReceiveDamage(damage)));
            Debug.Log("¡Ataque exitoso! El enemigo recibio daño.");
        }
        else
        {
            // Si falla, muestra un mensaje de fallo
            StartCoroutine(AnimAttack(() => Debug.Log("El ataque del jugador falló.")));
        }

        // Reducir la municion independientemente de si acierta o falla
        player.currentAmmo -= 1;
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

    private void Heal(int healAmount)
    {
        Player player = FindObjectOfType<Player>();
        player.currentHealth += healAmount;
        if (player.currentHealth > player.maxHealth)
            player.currentHealth = player.maxHealth;
    }

    private void Reload()
    {
        Player player = FindObjectOfType<Player>();
        player.currentAmmo += 1;
        if (player.currentAmmo > player.maxAmmo)
            player.currentAmmo = player.maxAmmo;
    }

    private void SpecialAttack()
    {
        switch (specialAttackType)
        {
            case SpecialAttackType.DoubleShot:
                DoubleShot();
                break;
            case SpecialAttackType.RifleShot:
                RifleShot();
                break;
            case SpecialAttackType.Dynamite:
                Dynamite();
                break;
        }
    }

    private void DoubleShot()
    {
        Enemy enemy = FindObjectOfType<Enemy>();
        Player player = FindObjectOfType<Player>();
        for (int i = 0; i < 2; i++)
        {
            // Generar un valor aleatorio para determinar si el disparo acierta
            float hitChance = Random.Range(0f, 1f);

            if (hitChance <= player.accuracy)
            {
                // Si acierta, realiza el ataque
                StartCoroutine(AnimAttack(() => enemy.ReceiveDamage(damage)));
                Debug.Log("¡Doble Ataque exitoso! El enemigo recibio daño.");
            }
            else
            {
                // Si falla, muestra un mensaje de fallo
                StartCoroutine(AnimAttack(() => Debug.Log("El ataque fallo.")));
            }

            // Reducir la municion independientemente de si acierta o falla
            player.currentAmmo -= 1;
        }
    }

    private void RifleShot()
    {
        Enemy enemy = FindObjectOfType<Enemy>();
        Player player = FindObjectOfType<Player>();
        // Generar un valor aleatorio para determinar si el disparo acierta
        float hitChance = Random.Range(0f, 1f);

        if (hitChance <= 0.90f)
        {
            // Si acierta, realiza el ataque
            StartCoroutine(AnimAttack(() => enemy.ReceiveDamage(damage)));
            Debug.Log("¡Ataque rifle exitoso! El enemigo recibio daño.");
        }
        else
        {
            // Si falla, muestra un mensaje de fallo
            StartCoroutine(AnimAttack(() => Debug.Log("El ataque fallo.")));
        }

        // Reducir la municion independientemente de si acierta o falla
        player.currentAmmo -= 1;
    }

    private void Dynamite()
    {
        Enemy enemy = FindObjectOfType<Enemy>();
        Player player = FindObjectOfType<Player>();
        // Generar un valor aleatorio para determinar si la dinamita acierta
        float hitChance = Random.Range(0f, 1f);

        if (hitChance <= player.accuracy)
        {
            // Si acierta, realiza el ataque
            StartCoroutine(AnimAttack(() => enemy.ReceiveDamage(damage)));
            Debug.Log("¡Ataque dinamita exitoso! El enemigo recibio daño.");
        }
        else
        {
            // Si falla, muestra un mensaje de fallo
            StartCoroutine(AnimAttack(() => Debug.Log("El ataque fallo.")));
        }

        // Reducir la municion independientemente de si acierta o falla
        player.currentAmmo -= 1;
    }
}

[System.Serializable]
public class DatosCarta
{
    public int id, daño, curacion, proteccion;
    public bool esEsquiva;
}

public enum ActionType
{
    Shot,
    Heal,
    Dodge,
    Reload,
    SpecialAttack
}

public enum SpecialAttackType
{
    DoubleShot,
    RifleShot,
    Dynamite
}