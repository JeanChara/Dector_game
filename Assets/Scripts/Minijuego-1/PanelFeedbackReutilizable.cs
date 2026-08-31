using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class PanelFeedbackReutilizable : MonoBehaviour
{
    [Header("Sub-Paneles")]
    [SerializeField] private GameObject contenedorFeedback;
    [SerializeField] private GameObject subPanelExito;
    [SerializeField] private GameObject subPanelDerrota;

    [Header("Referencias de UI - Éxito")]
    [SerializeField] private TextMeshProUGUI textoTituloExito;
    [SerializeField] private TextMeshProUGUI textoMensajeExito;
    [SerializeField] private TextMeshProUGUI textoExplicacionExito;
    [SerializeField] private Button botonContinuar;

    [Header("Referencias de UI - Derrota (Sin Intentos)")]
    [SerializeField] private TextMeshProUGUI textoTituloDerrota;
    [SerializeField] private TextMeshProUGUI textoMensajeDerrota;
    [SerializeField] private Button botonReintentar;

    [Header("Eventos de Botón")]
    public UnityEvent AlContinuar = new UnityEvent();
    public UnityEvent AlReintentar = new UnityEvent();

    private void Awake()
    {
        // Asignar listeners a los botones
        if (botonContinuar != null)
        {
            botonContinuar.onClick.RemoveAllListeners();
            botonContinuar.onClick.AddListener(ClickContinuar);
        }

        if (botonReintentar != null)
        {
            botonReintentar.onClick.RemoveAllListeners();
            botonReintentar.onClick.AddListener(ClickReintentar);
        }
    }

    /// <summary>
    /// Muestra la pantalla de éxito con retroalimentación pedagógica detallada.
    /// </summary>
    /// <param name="titulo">Título llamativo (ej: ¡Excelente Trabajo, Detective!)</param>
    /// <param name="mensaje">Mensaje de confirmación principal.</param>
    /// <param name="explicacionPedagogica">Texto ordenado cronológicamente que explica la secuencia para reforzar comprensión lectora.</param>
    public void MostrarExito(string titulo, string mensaje, string explicacionPedagogica)
    {
        if (contenedorFeedback != null) contenedorFeedback.SetActive(true);
        if (subPanelExito != null) subPanelExito.SetActive(true);
        if (subPanelDerrota != null) subPanelDerrota.SetActive(false);

        if (textoTituloExito != null) textoTituloExito.text = titulo;
        if (textoMensajeExito != null) textoMensajeExito.text = mensaje;
        if (textoExplicacionExito != null) textoExplicacionExito.text = explicacionPedagogica;
    }

    /// <summary>
    /// Muestra la pantalla de derrota cuando se han agotado los intentos.
    /// </summary>
    /// <param name="titulo">Título de derrota (ej: ¡Oh no, se acabaron los intentos!)</param>
    /// <param name="mensaje">Mensaje alentador para volver a intentarlo y releer.</param>
    public void MostrarDerrota(string titulo, string mensaje)
    {
        if (contenedorFeedback != null) contenedorFeedback.SetActive(true);
        if (subPanelExito != null) subPanelExito.SetActive(false);
        if (subPanelDerrota != null) subPanelDerrota.SetActive(true);

        if (textoTituloDerrota != null) textoTituloDerrota.text = titulo;
        if (textoMensajeDerrota != null) textoMensajeDerrota.text = mensaje;
    }

    /// <summary>
    /// Oculta todo el panel de feedback.
    /// </summary>
    public void Ocultar()
    {
        if (contenedorFeedback != null) contenedorFeedback.SetActive(false);
    }

    private void ClickContinuar()
    {
        Ocultar();
        AlContinuar.Invoke();
    }

    private void ClickReintentar()
    {
        Ocultar();
        AlReintentar.Invoke();
    }
}
