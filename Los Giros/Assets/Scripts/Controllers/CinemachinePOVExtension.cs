using System;
using Cinemachine;
using UnityEngine;

public class CinemachinePOVExtension : CinemachineExtension
{
    public event Action OnRotationCompleteY, OnRotationCompleteX, OnTurnComplete; // Eventos para las rotaciones
    private Vector3 currentRotation; // Almacena la rotacion actual
    [SerializeField] private float rotationSpeed = 5f; // Velocidad de rotacion
    private bool isRotatingY = false, isRotatingX = false; // Banderas para saber si esta girando en Y o X
    private bool needAtack = true; // Control del flujo de ataque
    private float targetYRotation, targetXRotation; // angulos objetivo de rotacion en los ejes Y y X

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (vcam.Follow && stage == CinemachineCore.Stage.Aim)
        {
            // Rotacion en el eje Y
            if (isRotatingY)
            {
                currentRotation.y = Mathf.Lerp(currentRotation.y, targetYRotation, rotationSpeed * deltaTime);

                if (Mathf.Abs(currentRotation.y - targetYRotation) < 0.1f)
                {
                    currentRotation.y = targetYRotation;
                    isRotatingY = false;

                    if (needAtack)
                    {
                        needAtack = !needAtack;
                        OnRotationCompleteY?.Invoke(); // Notificar que la rotacion en Y ha terminado
                    }
                    else
                    {
                        needAtack = !needAtack;
                        OnTurnComplete?.Invoke(); // Notificar que el turno ha terminado
                    }
                }
            }

            // Rotacion en el eje X
            if (isRotatingX)
            {
                currentRotation.x = Mathf.Lerp(currentRotation.x, targetXRotation, rotationSpeed * deltaTime);

                if (Mathf.Abs(currentRotation.x - targetXRotation) < 0.1f)
                {
                    currentRotation.x = targetXRotation;
                    isRotatingX = false;
                    OnRotationCompleteX?.Invoke(); // Notificar que la rotacion en X ha terminado
                }
            }

            // Aplicar la rotacion actual al estado de la camara
            state.RawOrientation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0f);
        }
    }

    // Funcion para iniciar el giro de 180 grados en el eje Y
    public void Rotate180DegreesY()
    {
        if (!isRotatingY && !isRotatingX)
        {
            isRotatingY = true;
            targetYRotation = (currentRotation.y + 180f) % 360f; // Calcular el angulo objetivo
        }
    }

    // Funcion para iniciar el giro en grados en el eje X
    public void Rotate45DegreesX(int angle)
    {
        if (!isRotatingX && !isRotatingY)
        {
            isRotatingX = true;
            targetXRotation = (currentRotation.x - angle) % 360f; // Calcular el angulo objetivo
        }
    }
}