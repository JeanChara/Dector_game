using UnityEngine;
using TMPro;

public class ControladorCaso : MonoBehaviour
{
    [Header("Caso de Juego")]
    [Tooltip("El caso actual del cual se mostrarán los datos generales.")]
    public DatosCaso casoActual;

    [Header("Referencias de UI del Caso")]
    [SerializeField] private TextMeshProUGUI textoTituloCaso;
    [SerializeField] private TextMeshProUGUI textoDescripcionCaso;

    private void Start()
    {
        InicializarUI();
    }

    /// <summary>
    /// Carga e inicializa la información general del caso en la UI.
    /// </summary>
    public void InicializarUI()
    {
        if (casoActual == null)
        {
            Debug.LogError("ControladorCaso: No se ha asignado un DatosCaso.");
            return;
        }

        if (textoTituloCaso != null)
        {
            textoTituloCaso.text = casoActual.tituloCaso;
        }

        if (textoDescripcionCaso != null)
        {
            textoDescripcionCaso.text = casoActual.descripcionCaso;
        }
    }
}
