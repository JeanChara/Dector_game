using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OpcionNotaPostit
{
    public int id;
    [TextArea(2, 4)]
    public string texto;
    public bool esContradiccion;
    [TextArea(2, 4)]
    public string explicacion;
}

[Serializable]
public class NivelContradiccionData
{
    public int nivel;
    public string persona;
    public string pregunta;
    public List<OpcionNotaPostit> opciones = new List<OpcionNotaPostit>();
}
