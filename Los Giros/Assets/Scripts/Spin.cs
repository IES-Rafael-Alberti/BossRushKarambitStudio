using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    public List<SpinBoost> spinBoosts;
    public float spinDuration = 2f; // Duracion del giro
    [SerializeField] private int minRotations = 3;    // Minimo de vueltas completas
    [SerializeField] private int maxRotations = 6;    // Maximo de vueltas completas
    [SerializeField] private GameObject boostIcon;
    [SerializeField] private List<string> textSpinBoost;
    private bool isSpinning = false;
    [HideInInspector] public string textBoost;
    [HideInInspector] public SpinBoost spinBoost;

    public void ChoseBoost()
    {
        int random = Random.Range(0, spinBoosts.Count);
        spinBoost = spinBoosts[random];
        switch (spinBoost)
        {
            case SpinBoost.X2Damage:
                textBoost = textSpinBoost[0];
                break;
            case SpinBoost.X3Damage:
                textBoost = textSpinBoost[1];
                break;
            case SpinBoost.X10Damage:
                textBoost = textSpinBoost[2];
                break;
            case SpinBoost.HealingBullets:
                textBoost = textSpinBoost[3];
                break;
            /*case SpinBoost.X2Damage:
                break;
            case SpinBoost.X2Damage:
                break;
            case SpinBoost.X2Damage:
                break;*/
            default:
                break;
        }
    }

    // Metodo para iniciar el giro
    public void MakeSpin()
    {
        if (!isSpinning)
        {
            StartCoroutine(SpinWheel());
        }
    }

    private IEnumerator SpinWheel()
    {
        isSpinning = true;

        // Calcula un numero de giros aleatorio
        int rotations = Random.Range(minRotations, maxRotations);
        // Calcula un angulo aleatorio entre 0 y 360 grados
        float finalAngle = Random.Range(0f, 360f);

        // Calcula el angulo total, vueltas completas mas el angulo final
        float totalAngle = 360f * rotations + finalAngle;

        // Guarda la rotacion inicial
        float startAngle = transform.eulerAngles.z;

        // Tiempo transcurrido
        float elapsedTime = 0f;

        // Realiza el giro interpolando suavemente
        while (elapsedTime < spinDuration)
        {
            elapsedTime += Time.deltaTime;

            // Interpola el angulo utilizando un easing para un efecto mas realista
            float currentAngle = Mathf.Lerp(startAngle, startAngle + totalAngle, EaseOutCubic(elapsedTime / spinDuration));

            // Aplica la rotacion
            transform.eulerAngles = new Vector3(0f, 0f, currentAngle);

            yield return null;
        }

        // Ajusta la rotacion final para evitar pequeños desajustes
        transform.eulerAngles = new Vector3(0f, 0f, startAngle + totalAngle);

        isSpinning = false;
    }

    // Funcion de suavizado para una animacion mas realista
    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3);
    }
}

// Potenciadores de la ruleta
public enum SpinBoost
{
    X2Damage,
    X3Damage,
    X10Damage,
    HealingBullets,

}