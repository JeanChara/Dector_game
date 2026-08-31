using System;
using UnityEngine;

[System.Serializable]
public class DatosPista
{
    [Tooltip("Identificador único de la pista")]
    public string idPista;

    [TextArea(2, 4)]
    [Tooltip("Texto descriptivo de la pista")]
    public string textoPista;

    [Tooltip("Indica si la pista es relevante y correcta para resolver el caso")]
    public bool esCorrecta;
}
