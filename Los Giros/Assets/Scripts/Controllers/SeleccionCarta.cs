using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SeleccionCarta : MonoBehaviour, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, ISelectHandler, IDeselectHandler
{

    EventSystem system;
    // GameObject lastSelectedGameObject;
    // GameObject currentSelectedGameObject_Recent;

    private AudioSource audioSource;

    // Animacion cursor por encima
    private readonly float moveTime = 0.1f;
    [Range(0f, 2f)] private readonly float scaleAmount = 1.15f;
    private Vector3 startScale;

    void Start()
    {
        system = EventSystem.current;

        startScale = transform.localScale;
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // GetLastGameObjectSelected();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // El puntero entra en el objeto
        Debug.Log("Mirando carta.");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // El puntero sale del objeto
        Debug.Log("No mirando carta");

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //El puntero cliquea sobre el objeto

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        List<Carta> cardsInHand = FindObjectsOfType<Carta>().ToList();

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.TryGetComponent<Carta>(out var cartaHit))
            {
                Debug.Log(cartaHit.id + " es Seleccionada");
                foreach (Carta carta in cardsInHand) // Resetear seleccion en caso de no ser la primera carta escogida
                {
                    carta.isSelected = false;
                }
                cartaHit.isSelected = true;
            }
        }
    }

    /*private void GetLastGameObjectSelected()
    {
        //Intento de registrar

        if (system.currentSelectedGameObject != currentSelectedGameObject_Recent)
        {
            lastSelectedGameObject = currentSelectedGameObject_Recent;

            currentSelectedGameObject_Recent = system.currentSelectedGameObject;
        }
    }*/

    public void OnSelect(BaseEventData eventData)
    {
        //Animacion al seleccionar la carta
        StartCoroutine(AnimarCarta(true));
        Debug.Log("En Seleccion.");

    }

    public void OnDeselect(BaseEventData eventData)
    {
        //Animacion al deseleccionar la carta
        StartCoroutine(AnimarCarta(false));
        Debug.Log("En Deseleccion.");

    }

    private IEnumerator AnimarCarta(bool startingAnim)
    {
        Vector3 endScale;
        float elapsedTime = 0f;
        while (elapsedTime < moveTime)
        {
            elapsedTime += Time.time;
            if (startingAnim)
                endScale = startScale * scaleAmount;
            else
                endScale = startScale;

            // Calculate the lerped amounts
            Vector3 lerpedScale = Vector3.Lerp(transform.localScale, endScale, elapsedTime / moveTime);

            transform.localScale = lerpedScale;
            yield return null;
        }
    }
}