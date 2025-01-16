using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class SeleccionCarta : MonoBehaviour, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, ISelectHandler, IDeselectHandler
{

    EventSystem system;
    GameObject lastSelectedGameObject;
    GameObject currentSelectedGameObject_Recent;

    public Carta cartaInfo;

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
        cartaInfo = GetComponentInChildren<Carta>();

        GetLastGameObjectSelected();
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
        Debug.Log(cartaInfo.infoES + " es seleccionado.");

        //Debug.Log(lastSelectedGameObject.name)

        //Reconocer el prefab y su script particular

    }

    private void GetLastGameObjectSelected()
    {
        //Intento de registrar

        if (system.currentSelectedGameObject != currentSelectedGameObject_Recent)
        {
            lastSelectedGameObject = currentSelectedGameObject_Recent;

            currentSelectedGameObject_Recent = system.currentSelectedGameObject;
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
       //Animación al seleccionar la carta
        StartCoroutine(AnimarCarta(true));
        Debug.Log("En Seleccion.");
        
    }

    public void OnDeselect(BaseEventData eventData)
    {
        //Animación al deseleccionar la carta
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