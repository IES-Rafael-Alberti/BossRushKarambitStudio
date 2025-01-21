using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TurnController : MonoBehaviour
{
    private readonly int limitTimerTime = 0;
    [SerializeField] private List<BasicPlayerAction> basicsPlayerActions;
    [SerializeField] private int drawCardAmount, timerTime;
    [SerializeField] private TMP_Text txtTimer, txtPlayerHealth;
    [SerializeField] private GameObject prefabCard, victoryPanel, defeatPanel;
    [SerializeField] private BaseDatosCartas baseDatosCartas;
    [SerializeField] private CinemachinePOVExtension cameraScript;
    [SerializeField] private Transform cardLocation;
    private bool isBattleActive = false;
    private Player player;
    [HideInInspector] public readonly List<DatosCarta> playerDeck = new();

    private void Start()
    {
        player = FindObjectOfType<Player>();
        UpdateHealthText();
        cameraScript.OnRotationComplete += DoPlayerAction; // Suscribirse al evento
        cameraScript.OnRotationComplete += DoEnemyAction;
        cameraScript.OnTurnComplete += InitTurn;
        InitData();
        StartBattle();
    }

    private void InitData()
    {
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

    #region PELEA
    public void StartBattle()
    {
        isBattleActive = true;
        InitTurn();
    }

    private void InitTurn()
    {
        StartCoroutine(DrawCard()); // Robar Cartas (inicializar, instanciar...)
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
                go.GetComponent<Carta>().id = baseDatosCartas.baseDatos[random].id;
                go.GetComponent<SpriteRenderer>().sprite = baseDatosCartas.baseDatos[go.GetComponent<Carta>().id].spriteCarta;
                go.GetComponent<Carta>().damage = baseDatosCartas.baseDatos[go.GetComponent<Carta>().id].daño;
                go.GetComponent<Carta>().healAmount = baseDatosCartas.baseDatos[go.GetComponent<Carta>().id].curacion;
                go.GetComponent<Carta>().isDodge = baseDatosCartas.baseDatos[go.GetComponent<Carta>().id].esEsquiva;
                go.GetComponent<Carta>().nombreCarta = baseDatosCartas.baseDatos[go.GetComponent<Carta>().id].infoES;
                go.GetComponent<Carta>().actionType = baseDatosCartas.baseDatos[go.GetComponent<Carta>().id].actionType;
                if (go.GetComponent<Carta>().actionType == ActionType.SpecialAttack)
                    go.GetComponent<Carta>().specialAttackType = baseDatosCartas.baseDatos[go.GetComponent<Carta>().id].specialAttackType;

                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    private IEnumerator InitTimer()
    {
        int auxTime = timerTime;
        while (auxTime >= limitTimerTime)
        {
            txtTimer.text = auxTime.ToString(); // Actualizar el texto con el valor actual
            yield return new WaitForSeconds(1f);    // Esperar un segundo
            auxTime -= 1;                         // Reducir el contador
        }
        txtTimer.text = limitTimerTime.ToString();
        EndTurn();
    }

    public void UpdateHealthText()
    {
        txtPlayerHealth.text = "Health: " + player.currentHealth;
    }

    private void DestroyCards()
    {
        List<Carta> cartas = FindObjectsOfType<Carta>().ToList();
        foreach (Carta carta in cartas)
        {
            Destroy(carta.gameObject);
        }
    }

    private IEnumerator PlayerAction()
    {
        PlayCard(); // Juega la carta seleccionada
        // Tras ejecutar su accion se vuelve a girar
        yield return new WaitForSeconds(1f);
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

    private void DoEnemyAction()
    {
        StartCoroutine(EnemyAction());
    }

    private IEnumerator EnemyAction()
    {
        Enemy enemy = FindObjectOfType<Enemy>();
        enemy.DoAction();
        yield return new WaitForSeconds(1f); // Verificar el estado de la partida despues de la accion del enemigo
        DetectOutcome();
    }

    public void DetectOutcome()
    {
        Player player = FindObjectOfType<Player>();
        Enemy enemy = FindObjectOfType<Enemy>();

        bool isPlayerDead = player.currentHealth <= 0;
        bool isEnemyDead = enemy.currentHealth <= 0;

        if (isPlayerDead && isEnemyDead) // Ambos estan muertos, priorizamos la derrota del player
        {
            Debug.Log("¡Empate tecnico! Prioridad: derrota del jugador.");
            DetectLose();
        }
        else if (isPlayerDead) // Solo el jugador esta muerto
            DetectLose();
        else if (isEnemyDead) // Solo el enemigo esta muerto
            DetectWin();
    }

    public void DetectWin()
    {
        // Detener la batalla
        isBattleActive = false;

        // Mostrar mensaje de victoria
        Debug.Log("¡Jugador, has derrotado al enemigo!");

        // Detener cualquier accion o logica en curso
        StopAllCoroutines();
        ShowVictoryPanel();
    }

    public void DetectLose()
    {
        // Detener la batalla
        isBattleActive = false;

        // Mostrar mensaje de derrota (puedes cambiar esto por una animacion o transicion)
        Debug.Log("¡Jugador, has sido derrotado!");

        // Detener cualquier accion o logica en curso
        StopAllCoroutines();
        ShowDefeatPanel();
    }

    private void ShowDefeatPanel()
    {
        // Logica para mostrar una pantalla de derrota o reiniciar el nivel
        Debug.Log("Mostrar pantalla de derrota...");
    }

    private void ShowVictoryPanel()
    {

    }
    #endregion

    private void OnDestroy()
    {
        cameraScript.OnRotationComplete -= DoPlayerAction; // Desuscribirse para evitar errores
        cameraScript.OnRotationComplete -= DoEnemyAction;
    }
}

public enum BasicPlayerAction
{
    Shot,
    Heal,
    Dodge,
    Reload,
    SpecialAttack
}