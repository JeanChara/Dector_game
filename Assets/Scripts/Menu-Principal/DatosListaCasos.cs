using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemCasoMenu
{
    [Tooltip("Identificador o número de caso (ej: CASO 01)")]
    public string numeroCaso = "CASO 01";

    [Tooltip("Referencia al ScriptableObject de DatosCaso")]
    public DatosCaso datosCaso;

    [Tooltip("Nombre exacto de la escena a cargar en Build Settings")]
    public string nombreEscena = "EscenaCaso_01";

    [Tooltip("Color del tema/badge para la tarjeta del caso")]
    public Color colorTema = new Color(0.9f, 0.5f, 0.1f);

    [Tooltip("Color de fondo para el botón principal de iniciar")]
    public Color colorBoton = new Color(0.95f, 0.55f, 0.15f);
}

[CreateAssetMenu(fileName = "ListaCasosData", menuName = "Juego/Lista de Casos del Menu")]
public class DatosListaCasos : ScriptableObject
{
    [Header("Lista de Casos del Juego")]
    public List<ItemCasoMenu> listaCasos = new List<ItemCasoMenu>();
}
