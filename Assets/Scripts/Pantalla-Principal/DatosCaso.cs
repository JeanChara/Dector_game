using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NuevoDatosCaso", menuName = "ScriptableObjects/DatosCaso", order = 2)]
public class DatosCaso : ScriptableObject
{
    [Header("Detalles del Caso")]
    public string tituloCaso;
    [TextArea(3, 6)]
    public string descripcionCaso;

    [Header("Fondo del Escenario")]
    [Tooltip("Imagen de fondo blur/escenario para la escena y minijuegos de este caso")]
    public Sprite imagenFondo;

    [Header("Testigos del Caso")]
    public List<DatosTestigo> testigos = new List<DatosTestigo>();

    [Header("Datos del Minijuego de Resolución")]
    public string tituloMinijuego;
    public string nombreMinijuego;
    [TextArea(5, 10)]
    public string descripcionMinijuego;

    [Header("Configuración Minijuego 3 (Pistómetro)")]
    public List<DatosPista> pistasMinijuego3 = new List<DatosPista>();

    [Header("Configuración Minijuego 4 (Descarte de Inconsistencias)")]
    public List<NivelContradiccionData> nivelesMinijuego4 = new List<NivelContradiccionData>();

    [Header("Resumen de Victoria")]
    [TextArea(4, 8)]
    public string textoVictoriaResumen;
}
