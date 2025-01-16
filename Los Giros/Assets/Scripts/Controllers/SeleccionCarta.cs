using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class SeleccionCarta : MonoBehaviour, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, ISelectHandler, IDeselectHandler
{

    EventSystem system;
    GameObject lastSelectedGameObject;
    GameObject currentSelectedGameObject_Recent;

    //public GameObject cartaSeleccionada;

    //string tagName = "TagCarta";

    //public Carta cartaInfo;

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

        //Raycast Hit funciona perfecto!

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Carta cartaHit = hit.collider.GetComponent<Carta>();
            if (cartaHit != null)
            {
                Debug.Log(cartaHit.id + " es Seleccionada");

                if (cartaHit.id == 0)
                {
                    Debug.Log("Disparo.");
                }

                else if (cartaHit.id == 1)
                {
                    Debug.Log("Cura.");
                }

                else if (cartaHit.id == 2)
                {
                    Debug.Log("Esquiva.");
                }

                else
                {
                    Debug.Log("Carta desconocida");
                }
            }


        }



        /*cartaSeleccionada = GameObject.FindGameObjectWithTag(tagName);

        cartaInfo = cartaSeleccionada.GetComponent<Carta>();

        Debug.Log(cartaInfo.id + " es seleccionado.");

        if (cartaInfo.id == 0) 
        {
            Debug.Log("Disparo.");
        }

        else if (cartaInfo.id == 1) 
        {
            Debug.Log("Cura.");
        }

        else if (cartaInfo.id == 2)
        {
            Debug.Log("Esquiva.");
        }

        else
        {
            Debug.Log("Carta desconocida");
        }*/

        //Debug.Log(lastSelectedGameObject.name)

        //Reconocer el prefab y su script particular

    }

    /*public void ClickSeleccion()
    {
        if (Input.GetMouseButtonDown(0))
        {
            
        }
    }*/

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