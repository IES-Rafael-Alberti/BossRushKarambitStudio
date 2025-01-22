using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SeleccionCarta : MonoBehaviour, IEventSystemHandler
{
    private AudioSource audioSource;
    public Camera myCamera;
    // Animacion cursor por encima
    private readonly float moveTime = 0.1f;
    [Range(0f, 2f)] private readonly float scaleAmount = 1.15f;
    private Vector3 startScale;

    void Start()
    {
        myCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        startScale = transform.localScale;
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        SelectCard();
        Ray ray = myCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Card")))
        {
            // Comprueba si el collider golpeado pertenece a este objeto
            if (hit.collider.gameObject == gameObject)
            {
                StartCoroutine(AnimCard(true));
            }
        }
        else
        {
            if(!GetComponent<Carta>().isSelected)
                StartCoroutine(AnimCard(false));
        }
    }

    private void SelectCard()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Genera un rayo desde la camara hacia la posicion del mouse en el mundo 3D
            Ray ray = myCamera.ScreenPointToRay(Input.mousePosition);
            // Lanza el rayo
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Card")))
            {
                // Comprueba si el collider golpeado pertenece a este objeto
                if (hit.collider.gameObject == gameObject)
                {
                    Debug.Log(hit.collider.gameObject.GetComponent<Carta>().id + " es Seleccionada");

                    // Encuentra todas las cartas en la mano y resetea su seleccion
                    List<Carta> cardsInHand = FindObjectsOfType<Carta>().ToList();
                    foreach (Carta carta in cardsInHand)
                    {
                        carta.isSelected = false;
                    }

                    // Marca la carta golpeada como seleccionada
                    hit.collider.gameObject.GetComponent<Carta>().isSelected = true;
                    EventSystem.current.SetSelectedGameObject(gameObject);
                }
            }
        }
    }

    private IEnumerator AnimCard(bool startingAnim)
    {
        Vector3 endScale = startingAnim ? startScale * scaleAmount : startScale;
        float elapsedTime = 0f;

        while (elapsedTime < moveTime)
        {
            elapsedTime += Time.time;
            transform.localScale = Vector3.Lerp(transform.localScale, endScale, elapsedTime / moveTime);
            yield return null;
        }
    }
}