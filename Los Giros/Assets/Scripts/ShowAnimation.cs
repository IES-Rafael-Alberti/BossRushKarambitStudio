using UnityEngine;
using System.Collections;
using Cinemachine;

public class ShowAnimation : MonoBehaviour
{
    private Animator animator;
    private bool isMovingY = false, isHiding = false;
    private float elapsedTimeY; // Tiempo transcurrido para el movimiento
    private string triggerName = "";
    [SerializeField] private float initialY, targetY, moveDuration;
    [SerializeField] private float delayBeforeHide = 1f; // Tiempo de retraso antes de esconder
    [SerializeField] private CinemachineImpulseSource impulseSource;
    private float transformY;

    private void Start()
    {
        animator = GetComponent<Animator>();
        transformY = transform.localPosition.y;
    }

    private void Update()
    {
        if (isMovingY)
        {
            if (isHiding)
                HideAnim(targetY, initialY); // Llama al metodo para esconder
            else
                ShowAnim(initialY, targetY); // Llama al metodo para mostrar
        }
    }

    public void InitMove(string triggerName)
    {
        if (!isMovingY)
        {
            isMovingY = true;
            elapsedTimeY = 0f; // Reinicia el tiempo transcurrido
            this.triggerName = triggerName;
            isHiding = false; // Define que el movimiento es para mostrar
        }
    }

    public void InitHide(string triggerName)
    {
        if (!isMovingY)
        {
            isMovingY = true;
            elapsedTimeY = 0f; // Reinicia el tiempo transcurrido
            this.triggerName = triggerName;
            isHiding = true; // Define que el movimiento es para esconder
        }
    }

    // Metodo para mostrar
    private void ShowAnim(float initialY, float targetY)
    {
        if (!string.IsNullOrEmpty(triggerName))
        {
            animator.SetTrigger(triggerName); // Activa el trigger
            MoveY(initialY, targetY);
            StartCoroutine(ResetTrigger(triggerName)); // Reinicia el trigger despues de un frame
        }

        // Inicia la animacion de esconder despues del retraso
        StartCoroutine(DelayAndHide());
    }

    // Metodo para esconder
    private void HideAnim(float targetY, float initialY)
    {
        MoveY(targetY, initialY);

        if (!isMovingY)
        {
            if (!string.IsNullOrEmpty(triggerName))
            {
                animator.SetTrigger(triggerName); // Activa el trigger
                StartCoroutine(ResetTrigger(triggerName)); // Reinicia el trigger despues de un frame
            }
        }
    }

    // Metodo para mover el objeto en el eje Y
    private void MoveY(float startY, float endY)
    {
        // Incrementa el tiempo transcurrido
        elapsedTimeY += Time.deltaTime;

        // Calcula el progreso del movimiento
        float progress = Mathf.Clamp01(elapsedTimeY / moveDuration);

        // Interpola entre startY y endY
        float currentY = Mathf.Lerp(startY, endY, progress);

        // Movimiento al transform del objeto
        Vector3 currentPosition = transform.localPosition;
        transform.localPosition = new Vector3(currentPosition.x, currentY, currentPosition.z);

        // Si el movimiento ha alcanzado el objetivo, detener el proceso
        if (progress >= 1f)
            isMovingY = false;
    }

    // Corrutina para esperar antes de esconder
    private IEnumerator DelayAndHide()
    {
        yield return new WaitForSeconds(delayBeforeHide);
        InitHide(triggerName); // Inicia la animacion de esconder
    }

    // Corrutina para reiniciar el trigger
    private IEnumerator ResetTrigger(string triggerName)
    {
        yield return null; // Espera un frame
        animator.ResetTrigger(triggerName); // Reinicia el trigger
    }
}