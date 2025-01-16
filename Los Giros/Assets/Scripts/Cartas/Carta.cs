using System;
using TMPro;
using UnityEngine;

public class Carta : MonoBehaviour
{
    public int id, daño;
    public string infoES, infoEN;
    [SerializeField] private TMP_Text txtDaño, txtInfo;
    [SerializeField] private AudioClip audioClip, audioClipSonidoCarta;
    public bool esJugable = false;

    private void Start()
    {
        txtDaño.text = daño.ToString();
        txtInfo.text = infoES; // Sistema de idioma
    }

}

[Serializable]
public class DatosCarta
{
    public int id, daño, curacion, proteccion;
    public bool esEsquiva;
}