using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnController : MonoBehaviour
{
    #region VARIABLES
    // Constantes
    private readonly int limitTimerTime = 0;

    // Serializables
    [SerializeField] private List<BasicPlayerAction> basicsPlayerActions;
    [SerializeField] private int drawCardAmount, timerTime;
    [SerializeField] private TMP_Text txtTimer, txtPlayerHealth;
    [SerializeField] private GameObject prefabCard, victoryPanel, defeatPanel;
    [SerializeField] private Button btnContinue, btnRetry;
    [SerializeField] private BaseDatosCartas baseDatosCartas;
    [SerializeField] private CinemachinePOVExtension cameraScript;
    [SerializeField] private Transform cardLocation;
    [SerializeField] float moveDuration = 0.75f, rotateDuration = 0.9f;

    // Privadas
    private bool isBattleActive = false;
    private Player player;

    // Publicas
    [HideInInspector] public readonly List<DatosCarta> playerDeck = new();
    #endregion

    #region UNITY METHODS
    private void Start()
    {
        InitData();
        UpdateHealthText();
        StartBattle();
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
        cameraScript.OnRotationCompleteY += DoPlayerAction; // Suscribirse a eventos
        cameraScript.OnRotationCompleteY += DoEnemyAction;
        cameraScript.OnTurnComplete += InitTurn;
        // btnContinue.onClick.AddListener(NextEnemy); DESCOMENTAR EN EL MOMENTO NECESARIO
        // btnRetry.onClick.AddListener(Retry);

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
        StartCoroutine(DrawCard()); // Robar cartas
        ChooseEnemyAction();
        StartCoroutine(InitTimer());
    }

    public void EndTurn()
    {
        cameraScript.Rotate180DegreesY();
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

    #endregion

    #region PLAYER ACTIONS

    private IEnumerator PlayerAction()
    {
        PlayCard(); // Juega la carta seleccionada
        yield return new WaitForSeconds(1f); // Tiempo antes de girar
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

    public void UpdateHealthText()
    {
        txtPlayerHealth.text = "Health: " + player.currentHealth;
    }

    #endregion

    #region GAME ENDINGS

    public void DetectWin()
    {
        isBattleActive = false;
        Debug.Log("¡Jugador, has derrotado al enemigo!");
        StopAllCoroutines();
        cameraScript.Rotate45DegreesX();
        StartCoroutine(ShowVictoryPanel());
    }

    public void DetectLose()
    {
        isBattleActive = false;
        Debug.Log("¡Jugador, has sido derrotado!");
        StopAllCoroutines();
        cameraScript.Rotate45DegreesX();
        StartCoroutine(ShowDefeatPanel());
    }

    private IEnumerator ShowVictoryPanel() // Se llama al final de la rotacion de la camara
    {
        cameraScript.Rotate45DegreesX();
        yield return new WaitForSeconds(2f);
        // Logica para mostrar pantalla de victoria
        victoryPanel.SetActive(true);
    }

    private IEnumerator ShowDefeatPanel()
    {
        cameraScript.Rotate45DegreesX();
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

    private void Retry()
    {
        // Logica para reiniciar la partida
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