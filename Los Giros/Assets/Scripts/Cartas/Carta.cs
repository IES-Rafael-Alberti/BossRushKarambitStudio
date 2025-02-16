using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class Carta : MonoBehaviour
{
    [HideInInspector] public int id, damage, healAmount;
    [HideInInspector] public float moveDistanceX, moveDuration, rotateDuration;
    // public bool isPlayable = false;
    [SerializeField] private TMP_Text txtNombre;
    [HideInInspector] public string nombreCartaES, nombreCartaEN;
    public ActionType actionType;
    public SpecialAttackType specialAttackType;
    [SerializeField] private AudioClip audioClip, audioClipSonidoCarta;
    [SerializeField] private Material normalMaterial, doubleShotMaterial, rifleShotMaterial, dynamiteMaterial;
    [HideInInspector] public bool isSelected = false, isDodge;
    private ShowAnimation showAnimation;
    private TurnController turnController;
    private Locale locale;

    private void Start()
    {
        showAnimation = FindObjectOfType<ShowAnimation>();
        turnController = FindObjectOfType<TurnController>();
        StartCoroutine(AnimPlacement(moveDistanceX, moveDuration, rotateDuration));
        ApplySpecialEffect();
    }

    private void Update()
    {
        locale = LocalizationSettings.SelectedLocale;
        if (locale != null)
            if (locale.Identifier == "en")
                txtNombre.text = nombreCartaEN;
            else if (locale.Identifier == "es")
                txtNombre.text = nombreCartaES;
    }

    public void DoAction()
    {
        switch (actionType)
        {
            case ActionType.Shot:
                Shot();
                break;
            case ActionType.Heal:
                Heal(healAmount);
                break;
            case ActionType.Reload:
                Reload();
                break;
            case ActionType.SpecialAttack:
                SpecialAttack();
                break;
            case ActionType.Dodge:
                Dodge();
                break;
            default:
                break;
        }
    }

    private void Shot()
    {
        Enemy enemy = FindObjectOfType<Enemy>();
        Player player = FindObjectOfType<Player>();
        showAnimation.InitMove("Shot");
        if (player.currentAmmo > 0)
        {
            // Generar un valor aleatorio para determinar si el disparo acierta
            float hitChance = Random.Range(0f, 1f);

            if (hitChance <= player.accuracy)
            {
                // Si acierta, realiza el ataque
                StartCoroutine(AnimAttack(() => enemy.ReceiveDamage(damage)));
                player.Heal(healAmount);
                turnController.UpdatePlayerHealthUI();
            }

            // Reducir la municion independientemente de si acierta o falla
            player.currentAmmo -= 1;
            turnController.UpdateUIBullets();
        }
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

    private void Dodge()
    {
        FindObjectOfType<Player>().Dodge();
    }

    private void Heal(int healAmount)
    {
        Player player = FindObjectOfType<Player>();
        showAnimation.InitMove("Heal");
        player.Heal(healAmount);
    }

    private void Reload()
    {
        Player player = FindObjectOfType<Player>();
        showAnimation.InitMove("Reload");
        player.currentAmmo += 1;
        if (player.currentAmmo > player.maxAmmo)
            player.currentAmmo = player.maxAmmo;

        turnController.UpdateUIBullets();
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

        showAnimation.InitMove("DoubleShot");

        for (int i = 0; i < 2; i++)
        {
            // Generar un valor aleatorio para determinar si el disparo acierta
            float hitChance = Random.Range(0f, 1f);

            if (hitChance <= player.accuracy) // Si acierta, realiza el ataque
            {
                if (player.isDodging && hitChance <= player.accuracy - player.enemyDodgeProbability) // Si esta esquivando el enemigo, pero la precision esta dentro del rango, le da la bala
                    StartCoroutine(AnimAttack(() => enemy.ReceiveDamage(damage)));
                else if (player.isDodging && hitChance > player.accuracy - player.enemyDodgeProbability) // Si esta esquivando el enemigo, pero no esta a rango de precision, no le da
                    StartCoroutine(AnimAttack(() => Debug.Log("El ataque del jugador fallo.")));
                else // No esta el player esquivando y le da
                    StartCoroutine(AnimAttack(() => enemy.ReceiveDamage(damage)));
            }
            else // Si falla, muestra un mensaje de fallo
                StartCoroutine(AnimAttack(() => Debug.Log("El ataque del jugador fallo.")));
        }
    }

    private void RifleShot()
    {
        Enemy enemy = FindObjectOfType<Enemy>();
        Player player = FindObjectOfType<Player>();

        showAnimation.InitMove("RifleShot");

        // Generar un valor aleatorio para determinar si el disparo acierta
        float hitChance = Random.Range(0f, 1f);

        if (hitChance <= player.rifleAccuracy)
        {
            if (player.isDodging && hitChance <= player.rifleAccuracy - player.enemyDodgeProbability) // Si esta esquivando el player, pero la precision esta dentro del rango, le da la bala
                StartCoroutine(AnimAttack(() => enemy.ReceiveDamage(damage)));
            else if (player.isDodging && hitChance > player.rifleAccuracy - player.enemyDodgeProbability) // Si esta esquivando el player, pero no esta a rango de precision, no le da
                StartCoroutine(AnimAttack(() => Debug.Log("El ataque del jugador fallo.")));
            else // No esta el player esquivando y le da
                StartCoroutine(AnimAttack(() => enemy.ReceiveDamage(damage)));
        }
        else // Si falla, muestra un mensaje de fallo
            StartCoroutine(AnimAttack(() => Debug.Log("El ataque del jugador fallo.")));
    }

    private void Dynamite()
    {

        Enemy enemy = FindObjectOfType<Enemy>();
        Player player = FindObjectOfType<Player>();

        showAnimation.InitMove("ThrowDynamite");

        // Generar un valor aleatorio para determinar si la dinamita acierta
        float hitChance = Random.Range(0f, 1f);

        if (hitChance <= player.accuracy)
        {
            if (player.isDodging && hitChance <= player.accuracy - player.enemyDodgeProbability) // Si esta esquivando el enemigo, pero la precision esta dentro del rango, le da la dinamita
                StartCoroutine(AnimAttack(() => enemy.ReceiveDamage(damage)));
            else if (player.isDodging && hitChance > player.accuracy - player.enemyDodgeProbability) // Si esta esquivando el enemigo, pero no esta a rango de precision, la dinamita le hace menos daño
                StartCoroutine(AnimAttack(() => enemy.ReceiveDamage(damage / 2)));
            else // No esta el enemigo esquivando y le da
                StartCoroutine(AnimAttack(() => enemy.ReceiveDamage(damage)));
        }
        else // Si falla, muestra un mensaje de fallo
            StartCoroutine(AnimAttack(() => Debug.Log("El ataque del jugador fallo.")));
    }

    private IEnumerator AnimPlacement(float moveDistanceX, float moveDuration, float rotateDuration)
    {
        // Animar el movimiento en el eje X
        Vector3 startPosition = transform.localPosition;
        Vector3 endPosition = startPosition + new Vector3(moveDistanceX, 0, 0); // Mover en el eje X
        float elapsedTime = 0;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / moveDuration);
            transform.localPosition = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        // Asegurarse de que la posicion final sea exacta
        transform.localPosition = endPosition;

        // Esperar un fotograma antes de iniciar la rotacion
        yield return null;

        // Animar la rotacion en el eje Y
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0, 0, 0);
        elapsedTime = 0;

        while (elapsedTime < rotateDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / rotateDuration);
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, t);
            yield return null;
        }

        // Asegurarse de que la rotacion final sea exacta
        transform.rotation = endRotation;
    }

    private void ApplySpecialEffect()
    {
        if (actionType == ActionType.SpecialAttack)
        {
            if (specialAttackType == SpecialAttackType.DoubleShot)
                GetComponent<SpriteRenderer>().material = doubleShotMaterial;
            else if (specialAttackType == SpecialAttackType.RifleShot)
                GetComponent<SpriteRenderer>().material = rifleShotMaterial;
            else
                GetComponent<SpriteRenderer>().material = dynamiteMaterial;
        }
        else
            GetComponent<SpriteRenderer>().material = normalMaterial;
    }

}

public class DatosCarta
{
    public int id, daño, curacion, proteccion;
    public bool esEsquiva;

    public DatosCarta(int id)
    {
        this.id = id;
    }
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