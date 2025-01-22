using System;
using Cinemachine;
using UnityEngine;

public class CinemachinePOVExtension : CinemachineExtension
{
    public event Action OnRotationComplete, OnTurnComplete; // Evento que se dispara al completar la rotacion
    private Vector3 currentRotation; // Almacena la rotacion actual
    [SerializeField] private float rotationSpeed = 5f; // Velocidad de rotacion para el giro
    private bool isRotating = false, needAtack = true; // Bandera para saber si esta girando
    private float targetYRotation; // Angulo objetivo de rotacion en el eje Y

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (vcam.Follow && stage == CinemachineCore.Stage.Aim)
        {
            if (isRotating)
            {
                // Interpola suavemente hacia el angulo objetivo
                currentRotation.y = Mathf.Lerp(currentRotation.y, targetYRotation, rotationSpeed * deltaTime);

                // Si la rotacion esta suficientemente cerca del objetivo, deten el giro
                if (Mathf.Abs(currentRotation.y - targetYRotation) < 0.1f)
                {
                    currentRotation.y = targetYRotation;
                    isRotating = false;
                    if (needAtack)
                    {
                        needAtack = !needAtack;
                        OnRotationComplete?.Invoke(); // Notificar que la rotacion ha terminado
                    }
                    else
                    {
                        needAtack = !needAtack;
                        OnTurnComplete?.Invoke();
                    }
                }
            }

            // Aplica la rotacion actual al estado de la camara
            state.RawOrientation = Quaternion.Euler(0f, currentRotation.y, 0f);
        }
    }

    // Funcion para iniciar el giro de 180 grados
    public void Rotate180Degrees()
    {
        if (!isRotating)
        {
            isRotating = true;
            targetYRotation = (currentRotation.y + 180f) % 360f; // Calcula el angulo objetivo
        }
    }
}
