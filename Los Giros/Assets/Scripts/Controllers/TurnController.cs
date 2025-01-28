using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TurnController : MonoBehaviour
{
    #region VARIABLES
    // Constantes
    private readonly int limitTimerTime = 0;

    // Serializables
    [SerializeField] private List<GameObject> listBulletsUI, listEnemies;
    [SerializeField] private List<BasicPlayerAction> basicsPlayerActions;
    [SerializeField] private int drawCardAmount, timerTime;
    [SerializeField] private TMP_Text txtTimer, txtPlayerHealth, txtEnemyHealth;
    [SerializeField] private GameObject prefabCard, victoryPanel, defeatPanel, spinGO, iconBoost;
    [SerializeField] private Button btnContinue, btnRetry;
    [SerializeField] private BaseDatosCartas baseDatosCartas;
    [SerializeField] private CinemachinePOVExtension cameraScript;
    [SerializeField] private Transform cardLocation, world;
    [SerializeField] float moveDuration = 0.75f, rotateDuration = 0.9f;
    [SerializeField] Slider sliderPlayerHealth, sliderEnemyHealth;

    // Privadas
    private bool isBattleActive = false;
    private Player player;
    private Enemy enemy;

    // Publicas
    [HideInInspector] public readonly List<DatosCarta> playerDeck = new();
    #endregion

    #region UNITY METHODS
    private void Start()
    {
        InitData();
        UpdateUIBullets();
        UpdatePlayerHealthUI();
        UpdateEnemyHealthUI();
        StartBattle();
    }

    private void Update()
    {
        DetectOutcome();
    }

    private void OnDestroy()
    {
        cameraScript.OnRotationCompleteY -= DoPlayerAction; // Desuscribirse para evitar errores
        cameraScript.OnRotationCompleteY -= DoEnemyAction;
    }

    #endregion

    #region INITIALIZATION

    private void InitData()
    {
        player = FindObjectOfType<Player>();
        enemy = FindObjectOfType<Enemy>();
        cameraScript.OnRotationCompleteY += DoPlayerAction; // Suscribirse a eventos
        cameraScript.OnRotationCompleteY += DoEnemyAction;
        cameraScript.OnTurnComplete += InitTurn;
        // btnContinue.onClick.AddListener(NextEnemy); DESCOMENTAR EN EL MOMENTO NECESARIO
        btnRetry.onClick.AddListener(() => StartCoroutine(Retry()));

        foreach (var action in basicsPlayerActions)
        {
            if (BasicPlayerAction.Shot == action)
                playerDeck.Add(new(0));
            else if (BasicPlayerAction.Heal == action)
                playerDeck.Add(new(1));
            else if (BasicPlayerAction.Dodge == action)
                playerDeck.Add(new(2));
            else if (BasicPlayerAction.Reload == action)
                playerDeck.Add(new(3));
            else if (BasicPlayerAction.SpecialAttack == action)
                playerDeck.Add(new(4));
        }

        sliderPlayerHealth.minValue = 0;
        sliderPlayerHealth.maxValue = player.maxHealth;

        sliderEnemyHealth.minValue = 0;
        sliderEnemyHealth.maxValue = enemy.maxHealth;
    }

    #endregion

    #region TURN MANAGEMENT

    public void StartBattle()
    {
        isBattleActive = true;
        InitTurn();
    }

    private void InitTurn()
    {
        ShowSpin();
    }

    public void EndTurn()
    {
        cameraScript.Rotate180DegreesY();
        iconBoost.SetActive(false);
    }

    private void ChooseEnemyAction()
    {
        Enemy enemy = FindObjectOfType<Enemy>();
        enemy.ChooseAction();
    }

    private IEnumerator InitTimer()
    {
        int auxTime = timerTime;
        while (auxTime >= limitTimerTime)
        {
            txtTimer.text = auxTime.ToString(); // Actualizar el texto con el valor actual
            yield return new WaitForSeconds(1f);
            auxTime -= 1; // Reducir el contador
        }
        txtTimer.text = limitTimerTime.ToString();
        EndTurn();
    }

    public void DetectOutcome()
    {
        if (isBattleActive)
        {
            Player player = FindObjectOfType<Player>();
            Enemy enemy = FindObjectOfType<Enemy>();

            bool isPlayerDead = player.currentHealth <= 0;
            bool isEnemyDead = enemy.currentHealth <= 0;

            if (isPlayerDead && isEnemyDead)
                DetectLose();
            else if (isPlayerDead)
                DetectLose();
            else if (isEnemyDead)
                DetectWin();
        }
    }

    private void ShowSpin()
    {
        spinGO.SetActive(true);
        Spin spin = spinGO.GetComponentInChildren<Spin>();
        spin.MakeSpin();

        // Asegurar que el siguiente paso del turno ocurra tras el giro
        StartCoroutine(WaitForSpin());
    }

    private IEnumerator WaitForSpin()
    {
        List<Carta> cardsOnHand;
        // Esperar a que finalice la duracion del giro de la ruleta
        Spin spin = spinGO.GetComponentInChildren<Spin>();
        yield return new WaitForSeconds(spin.spinDuration + 0.1f);
        iconBoost.GetComponent<Image>().sprite = spin.spriteIcon;
        iconBoost.SetActive(true);
        switch (spin.spinBoost)
        {
            case SpinBoost.X2Damage:
                cardsOnHand = FindObjectsOfType<Carta>().ToList();
                foreach (Carta card in cardsOnHand)
                {
                    card.damage *= 2;
                }
                enemy.damageMultiplier *= 2;
                break;
            case SpinBoost.X3Damage:
                cardsOnHand = FindObjectsOfType<Carta>().ToList();
                foreach (Carta card in cardsOnHand)
                {
                    card.damage *= 3;
                }
                enemy.damageMultiplier *= 3;
                break;
            case SpinBoost.X2Action:
                // ¡¡¡ Puede producir errores inesperados, hay que probarlo !!!
                /*DoPlayerAction();
                DoEnemyAction();*/
                break;
            case SpinBoost.HealingBullets:
                cardsOnHand = FindObjectsOfType<Carta>().ToList();
                foreach (Carta card in cardsOnHand)
                {
                    if (card.damage > 0)
                        card.healAmount += 1;
                }
                if (enemy.actionChosen == global::EnemyAction.Attack || enemy.actionChosen == global::EnemyAction.SpecialAttack)
                    enemy.Heal(enemy.healAmount);
                break;
            /*case SpinBoost.X2Damage:
                break;
            case SpinBoost.X2Damage:
                break;
            case SpinBoost.X2Damage:
                break;*/
            default:
                break;
        }

        yield return new WaitForSeconds(0.15f);

        // Ocultar la ruleta tras el giro
        spinGO.SetActive(false);

        // Continuar el flujo del turno
        StartCoroutine(DrawCard()); // Robar cartas
        ChooseEnemyAction();
        StartCoroutine(InitTimer());
    }

    #endregion

    #region PLAYER ACTIONS

    private IEnumerator PlayerAction()
    {
        PlayCard(); // Juega la carta seleccionada
        DetectOutcome();
        yield return new WaitForSeconds(2f); // Tiempo antes de girar para elegir nuevamente carta
        cameraScript.Rotate180DegreesY();
        DestroyCards();
    }

    private void PlayCard()
    {
        List<Carta> cartasEnMano = FindObjectsOfType<Carta>().ToList();
        foreach (Carta carta in cartasEnMano)
        {
            if (carta.isSelected)
                carta.DoAction();
        }
    }

    private void DoPlayerAction()
    {
        StartCoroutine(PlayerAction());
    }

    #endregion

    #region ENEMY ACTIONS

    private void DoEnemyAction()
    {
        StartCoroutine(EnemyAction());
    }

    private IEnumerator EnemyAction()
    {
        Enemy enemy = FindObjectOfType<Enemy>();
        enemy.DoAction();
        DetectOutcome();
        yield return new WaitForSeconds(1f);
        enemy.damageMultiplier = 1;
    }
    #endregion

    #region BATTLE UTILITIES

    private IEnumerator DrawCard()
    {
        float ajustePosicionZ = -0.1f; // Ajuste para que se vean las cartas por encima de la baraja al robarlas
        float moveDistanceX = 1.5f;

        // Reiniciar los pesos al inicio del turno
        List<float> cardWeights = ResetCardWeights();

        if (isBattleActive)
        {
            for (int i = 0; i < drawCardAmount; i++)
            {
                // Seleccionar una carta basada en pesos
                int random = GetWeightedRandomIndex(cardWeights);

                GameObject go = Instantiate(prefabCard, cardLocation);
                go.transform.position = new Vector3(cardLocation.transform.position.x, cardLocation.transform.position.y, cardLocation.transform.position.z + ajustePosicionZ);

                // Configurar propiedades de la carta
                var cartaData = baseDatosCartas.baseDatos[random];
                Carta carta = go.GetComponent<Carta>();
                carta.id = cartaData.id;
                carta.GetComponent<SpriteRenderer>().sprite = cartaData.spriteCarta;
                carta.moveDistanceX = moveDistanceX;
                moveDistanceX += 1.3f;
                carta.moveDuration = moveDuration;
                carta.rotateDuration = rotateDuration;
                carta.damage = cartaData.daño;
                carta.healAmount = cartaData.curacion;
                carta.isDodge = cartaData.esEsquiva;
                carta.nombreCarta = cartaData.infoES;
                carta.actionType = cartaData.actionType;
                if (carta.actionType == ActionType.SpecialAttack)
                    carta.specialAttackType = cartaData.specialAttackType;

                // Reducir peso de la carta seleccionada
                cardWeights[random] *= 0.5f; // Reducir su probabilidad de aparecer de nuevo

                yield return new WaitForSeconds(0.25f);
            }
        }
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
            {
                return i; // Devolver el indice seleccionado
            }
        }

        return weights.Count - 1; // Devolver el ultimo indice como respaldo
    }

    // Metodo para reiniciar los pesos al inicio de cada turno
    private List<float> ResetCardWeights()
    {
        List<float> weights = new(new float[playerDeck.Count]);
        for (int i = 0; i < weights.Count; i++)
        {
            weights[i] = 1f; // Reiniciar todos los pesos al valor inicial
        }
        return weights;
    }

    private void DestroyCards()
    {
        List<Carta> cartas = FindObjectsOfType<Carta>().ToList();
        foreach (Carta carta in cartas)
        {
            Destroy(carta.gameObject);
        }
    }

    public void UpdatePlayerHealthUI()
    {
        txtPlayerHealth.text = "Health: " + player.currentHealth;
        sliderPlayerHealth.value = player.currentHealth;
    }

    public void UpdateEnemyHealthUI()
    {
        Enemy e = FindObjectOfType<Enemy>();
        txtEnemyHealth.text = "Health: " + e.currentHealth;
        sliderEnemyHealth.value = e.currentHealth;
    }

    #endregion

    #region GAME ENDINGS

    public void DetectWin()
    {
        isBattleActive = false;
        Debug.Log("¡Jugador, has derrotado al enemigo!");
        StopAllCoroutines();
        cameraScript.Rotate45DegreesX(45);
        StartCoroutine(ShowVictoryPanel());
    }

    public void DetectLose()
    {
        isBattleActive = false;
        Debug.Log("¡Jugador, has sido derrotado!");
        StopAllCoroutines();
        cameraScript.Rotate45DegreesX(45);
        StartCoroutine(ShowDefeatPanel());
    }

    private IEnumerator ShowVictoryPanel() // Se llama al final de la rotacion de la camara
    {
        cameraScript.Rotate45DegreesX(45);
        yield return new WaitForSeconds(2f);
        // Logica para mostrar pantalla de victoria
        victoryPanel.SetActive(true);
    }

    private IEnumerator ShowDefeatPanel()
    {
        cameraScript.Rotate45DegreesX(45);
        yield return new WaitForSeconds(2f);
        // Logica para mostrar pantalla de derrota
        defeatPanel.SetActive(true);
    }

    #endregion

    #region UI ACTIONS

    private void NextEnemy()
    {
        // Logica para avanzar al siguiente enemigo
    }

    private IEnumerator Retry()
    {
        // Logica para reiniciar la partida
        Enemy enemy = FindObjectOfType<Enemy>();
        foreach (GameObject e in listEnemies)
        {
            if (e.GetComponent<Enemy>().ID == enemy.ID)
            {
                GameObject go = Instantiate(e, world);
                go.transform.localPosition = enemy.transform.localPosition;
                Destroy(enemy.gameObject, 0.1f);
            }
        }

        // Tras instanciar al enemigo y reemplazar al anterior, recolocar la camara y reiniciar los turnos
        player.currentHealth = player.maxHealth;
        player.currentAmmo = player.initialAmmo;
        UpdatePlayerHealthUI();
        UpdateUIBullets();
        DestroyCards();
        defeatPanel.SetActive(false);
        cameraScript.Rotate45DegreesX(-45);
        yield return new WaitForSeconds(2f);
        cameraScript.Rotate180DegreesY();
        UpdateEnemyHealthUI();
        isBattleActive = true;
    }

    public void UpdateUIBullets()
    {
        // Recorrer cada indice en la lista
        for (int i = 0; i < listBulletsUI.Count; i++)
        {
            // Activar los elementos segun la municion
            listBulletsUI[i].SetActive(i < player.currentAmmo);
        }
    }
    #endregion
}

public enum BasicPlayerAction
{
    Shot,
    Heal,
    Dodge,
    Reload,
    SpecialAttack
}