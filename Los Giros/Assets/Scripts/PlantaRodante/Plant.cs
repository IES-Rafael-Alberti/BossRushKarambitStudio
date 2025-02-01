using UnityEngine;

public class Plant : MonoBehaviour
{
    [SerializeField] private float moveSpeed; // Velocidad de movimiento en X
    [SerializeField] private float rotationSpeed; // Velocidad de rotacion en Z
    [HideInInspector] public bool moveRight; // Controla la direccion de movimiento

    private void Update()
    {
        // Direccion del movimiento en X
        float direction = moveRight ? 1f : -1f;

        // Movimiento en X
        transform.position += direction * moveSpeed * Time.deltaTime * Vector3.right;

        // Rotacion en Z
        transform.Rotate(0, 0, -direction * rotationSpeed * Time.deltaTime);
    }
}
