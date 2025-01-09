using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class SeleccionCarta : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private AudioSource audioSource;

    // Animacion cursor por encima
    private readonly float moveTime = 0.1f;
    [Range(0f, 2f)] private readonly float scaleAmount = 1.15f;
    private Vector3 startScale;

    void Start()
    {
        startScale = transform.localScale;
        audioSource = GetComponent<AudioSource>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Selecciona la carta
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Deselecciona la carta
    }

    public void OnSelect(BaseEventData eventData)
    {
       //Animación al seleccionar la carta
        StartCoroutine(AnimarCarta(true));
    }

    public void OnDeselect(BaseEventData eventData)
    {
        //Animación al deseleccionar la carta
        StartCoroutine(AnimarCarta(false));
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