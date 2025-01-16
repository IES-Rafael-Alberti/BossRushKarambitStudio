using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TurnController : MonoBehaviour
{
    private int cantidadCartasBaraja, tiempoLimite = 0;
    public static int contResultado = 0;
    [SerializeField] private int cantidadCartasARobar = 3, tiempoContador = 10;
    public List<DatosCarta> listaCartasJugador = new();
    private List<DatosCarta> listaTemp = new();
    [SerializeField] private TMP_Text txtContador;
    [SerializeField] private GameObject prefabCarta;
    [SerializeField] private List<GameObject> prefabsEnemigos;
    [SerializeField] private BaseDatosCartas baseDatosCartas;
    [SerializeField] private CinemachinePOVExtension cameraScript;
    [SerializeField] private Transform huecoCartas;
    private bool combateActivo = false;

    private void Start()
    {
        cameraScript.OnRotationComplete += EjecutarAccionJugador; // Suscribirse al evento
        cameraScript.OnRotationComplete += EjecutarAccionEnemigo;
        cameraScript.onTurnComplete += IniciarTurno;
        IniciarDatos();
        ResetearBaraja();
        EmpezarPelea();
    }

    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            cameraScript.Rotate180Degrees();
    }*/

    private void IniciarDatos()
    {
        // listaCartasJugador = ControladorDatos.listaCartasPartida;
        cantidadCartasBaraja = listaCartasJugador.Count;
    }

    #region PELEA
    public void EmpezarPelea()
    {
        combateActivo = true;
        IniciarTurno();
    }

    private void IniciarTurno()
    {
        StartCoroutine(RobarCarta()); // Robar Cartas (inicializar, instanciar...)
        ElegirAccionEnemigo();
        StartCoroutine(IniciarContador());
    }

    public void FinalizarTurno()
    {
        DestruirCartas(); // Descartar las cartas no usadas
        cameraScript.Rotate180Degrees();
        // txtCantidadCartasBaraja.text = listaTemp.Count.ToString();
    }

    private void ElegirAccionEnemigo()
    {
        Enemy enemy = FindObjectOfType<Enemy>();
        enemy.ChooseAction();
    }

    private IEnumerator RobarCarta()
    {
        int ajustePosicion = -340;
        // yield return new WaitForSeconds(2f);
        if (combateActivo)
        {
            Debug.LogWarning("Antes de condicion");
            if (cantidadCartasARobar <= listaTemp.Count)
            {
                Debug.LogWarning("Entro en condicion");
                for (int i = 0; i < cantidadCartasARobar; i++)
                {
                    int random = Random.Range(0, listaTemp.Count);
                    GameObject go = Instantiate(prefabCarta, huecoCartas);
                    go.transform.position = new Vector3(huecoCartas.transform.position.x + ajustePosicion, huecoCartas.transform.position.y + 30, 0);
                    ajustePosicion += 70;
                    go.GetComponent<Carta>().id = listaCartasJugador[random].id;
                    go.GetComponent<SpriteRenderer>().sprite = baseDatosCartas.baseDatos[go.GetComponent<Carta>().id].spriteCarta;
                    go.GetComponent<Carta>().damage = baseDatosCartas.baseDatos[go.GetComponent<Carta>().id].daño;
                    // go.GetComponent<Carta>().infoES = baseDatosCartas.baseDatos[go.GetComponent<Carta>().id].infoES; // **TRADUCCION**
                    go.GetComponent<Carta>().actionType = baseDatosCartas.baseDatos[go.GetComponent<Carta>().id].actionType;
                    if (go.GetComponent<Carta>().actionType == ActionType.SpecialAttack)
                        go.GetComponent<Carta>().specialAttackType = baseDatosCartas.baseDatos[go.GetComponent<Carta>().id].specialAttackType;

                    listaTemp.RemoveAt(random);
                    yield return new WaitForSeconds(0.2f);
                }
                cantidadCartasBaraja -= cantidadCartasARobar;
                // txtCantidadCartasBaraja.text = listaTemp.Count.ToString();
            }
            else
            {
                Debug.LogWarning("AAAA");
                if (cantidadCartasBaraja == 0)
                {
                    // Resetear la lista si no hay cartas que robar
                    ResetearBaraja();
                    cantidadCartasBaraja = listaCartasJugador.Count;
                    StartCoroutine(RobarCarta());
                }
                else
                {
                    int index = 0;
                    int iterations = listaTemp.Count; // Guardar el número inicial de iteraciones
                    for (int i = 0; i < iterations; i++) // Iteramos basado en el tamaño inicial
                    {
                        int random = Random.Range(0, listaTemp.Count);
                        GameObject go = Instantiate(prefabCarta, huecoCartas);
                        go.transform.position = new Vector3(huecoCartas.transform.position.x + ajustePosicion, huecoCartas.transform.position.y + 30, 0);
                        ajustePosicion += 70;
                        go.GetComponent<Carta>().id = listaCartasJugador[random].id;
                        go.GetComponent<SpriteRenderer>().sprite = baseDatosCartas.baseDatos[go.GetComponent<Carta>().id].spriteCarta;
                        go.GetComponent<Carta>().damage = baseDatosCartas.baseDatos[go.GetComponent<Carta>().id].daño;
                        go.GetComponent<Carta>().infoES = baseDatosCartas.baseDatos[go.GetComponent<Carta>().id].infoES;
                        go.GetComponent<Carta>().actionType = baseDatosCartas.baseDatos[go.GetComponent<Carta>().id].actionType;
                        if (go.GetComponent<Carta>().actionType == ActionType.SpecialAttack)
                            go.GetComponent<Carta>().specialAttackType = baseDatosCartas.baseDatos[go.GetComponent<Carta>().id].specialAttackType;

                        listaTemp.RemoveAt(random);
                        yield return new WaitForSeconds(0.2f);
                        index++;
                    }
                    cantidadCartasBaraja -= index;
                    // txtCantidadCartasBaraja.text = listaTemp.Count.ToString();
                    ResetearBaraja();
                    cantidadCartasBaraja = listaCartasJugador.Count;
                }
            }
        }
    }

    private IEnumerator IniciarContador()
    {
        int tiempoAux = tiempoContador;
        while (tiempoAux >= tiempoLimite)
        {
            txtContador.text = tiempoAux.ToString(); // Actualizar el texto con el valor actual
            yield return new WaitForSeconds(1f);    // Esperar un segundo
            tiempoAux -= 1;                         // Reducir el contador
        }
        txtContador.text = tiempoLimite.ToString();
        FinalizarTurno();
    }

    private void DestruirCartas()
    {
        List<Carta> cartas = FindObjectsOfType<Carta>().ToList();
        foreach (Carta carta in cartas)
        {
            Destroy(carta.gameObject);
        }
    }

    private IEnumerator AccionJugador()
    {
        Debug.Log("El jugador ataca al enemigo.");
        PlayCard(); // Juega la carta seleccionada
        // Tras ejecutar su accion se vuelve a girar
        yield return new WaitForSeconds(1f);
        cameraScript.Rotate180Degrees();
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

    private void EjecutarAccionJugador()
    {
        StartCoroutine(AccionJugador());
    }

    private void EjecutarAccionEnemigo()
    {
        StartCoroutine(AccionEnemigo());
    }

    private IEnumerator AccionEnemigo()
    {
        Enemy enemy = FindObjectOfType<Enemy>();
        enemy.DoAction();
        Debug.Log("El enemigo realiza la accion.");
        yield return new WaitForSeconds(1f);
    }

    private void ResetearBaraja()
    {
        listaTemp.Clear();
        foreach (var carta in listaCartasJugador)
        {
            listaTemp.Add(carta);
        }
    }
    #endregion

    private void OnDestroy()
    {
        cameraScript.OnRotationComplete -= EjecutarAccionJugador; // Desuscribirse para evitar errores
    }
}