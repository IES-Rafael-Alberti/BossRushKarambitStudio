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
        cameraScript.OnRotationComplete -= DoPlayerAction; // Desuscribirse para evitar errores
        cameraScript.OnRotationComplete -= DoEnemyAction;
    }

    #endregion

    #region INITIALIZATION

    private void InitData()
    {
        player = FindObjectOfType<Player>();
        cameraScript.OnRotationComplete += DoPlayerAction; // Suscribirse al evento
        cameraScript.OnRotationComplete += DoEnemyAction;
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
        cameraScript.Rotate180Degrees();
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
        {
            Debug.Log("¡Empate tecnico! Prioridad: derrota del jugador.");
            DetectLose();
        }
        else if (isPlayerDead)
        {
            DetectLose();
        }
        else if (isEnemyDead)
        {
            DetectWin();
        }
    }

    #endregion

    #region PLAYER ACTIONS

    private IEnumerator PlayerAction()
    {
        PlayCard(); // Juega la carta seleccionada
        yield return new WaitForSeconds(1f); // Tiempo antes de girar
        cameraScript.Rotate180Degrees();
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
        yield return new WaitForSeconds(1f);
        DetectOutcome();
    }

    #endregion

    #region BATTLE UTILITIES

    private IEnumerator DrawCard()
    {
        float ajustePosicionX = -1f, ajustePosicionY = 0.6f, ajustePosicionZ = -0.1f;

        if (isBattleActive)
        {
            for (int i = 0; i < drawCardAmount; i++)
            {
                int random = Random.Range(0, playerDeck.Count);
                GameObject go = Instantiate(prefabCard, cardLocation);
                go.transform.position = new Vector3(cardLocation.transform.position.x + ajustePosicionX, cardLocation.transform.position.y + ajustePosicionY, cardLocation.transform.position.z + ajustePosicionZ);
                ajustePosicionX += 1f;

                // Configurar propiedades de la carta
                var cartaData = baseDatosCartas.baseDatos[random];
                var carta = go.GetComponent<Carta>();
                carta.id = cartaData.id;
                carta.GetComponent<SpriteRenderer>().sprite = cartaData.spriteCarta;
                carta.damage = cartaData.daño;
                carta.healAmount = cartaData.curacion;
                carta.isDodge = cartaData.esEsquiva;
                carta.nombreCarta = cartaData.infoES;
                carta.actionType = cartaData.actionType;
                if (carta.actionType == ActionType.SpecialAttack)
                    carta.specialAttackType = cartaData.specialAttackType;

                yield return new WaitForSeconds(0.2f);
            }
        }
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
        ShowVictoryPanel();
    }

    public void DetectLose()
    {
        isBattleActive = false;
        Debug.Log("¡Jugador, has sido derrotado!");
        StopAllCoroutines();
        ShowDefeatPanel();
    }

    private void ShowVictoryPanel()
    {
        // Logica para mostrar pantalla de victoria
    }

    private void ShowDefeatPanel()
    {
        // Logica para mostrar pantalla de derrota
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