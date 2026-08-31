using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControladorTestigos : MonoBehaviour
{
    [Header("Caso de Juego")]
    [Tooltip("El caso actual del cual se mostrarán los testimonios.")]
    public DatosCaso casoActual;

    [Header("Referencias de UI del Testigo")]
    [SerializeField] private TextMeshProUGUI textoNombre;
    [SerializeField] private TextMeshProUGUI textoRol;
    [SerializeField] private Image imagenFoto;
    [SerializeField] private TextMeshProUGUI textoTestimonio;

    [Header("Referencias de Botones de Navegación")]
    [SerializeField] private Button botonAnterior;
    [SerializeField] private Button botonSiguiente;
    
    [Header("Componentes Visuales de los Botones (para cambio de color)")]
    [SerializeField] private Image fondoBotonAnterior;
    [SerializeField] private TextMeshProUGUI textoBotonAnterior;
    [SerializeField] private Image fondoBotonSiguiente;
    [SerializeField] private TextMeshProUGUI textoBotonSiguiente;

    [Header("Configuración Visual de los Botones (Sprites)")]
    [Tooltip("Sprite para el botón cuando está activo (ej. boton-activado.png)")]
    [SerializeField] private Sprite spriteBotonActivo;
    
    [Tooltip("Sprite para el botón cuando está inactivo/deshabilitado (ej. boton-desactivado.png)")]
    [SerializeField] private Sprite spriteBotonInactivo;

    [Header("Configuración de Colores de Texto")]
    [Tooltip("Color de texto para el botón activo (Negro #000000)")]
    public Color colorTextoActivo = Color.black;
    
    [Tooltip("Color de texto para el botón inactivo/deshabilitado (Blanco #FFFFFF)")]
    public Color colorTextoInactivo = Color.white;

    private int indiceTestigoActual = 0;

    private void Start()
    {
        if (casoActual == null)
        {
            Debug.LogError("ControladorTestigos: No se ha asignado un DatosCaso.");
            return;
        }

        if (casoActual.testigos == null || casoActual.testigos.Count == 0)
        {
            Debug.LogWarning("ControladorTestigos: El caso actual no tiene testigos configurados.");
            return;
        }

        indiceTestigoActual = 0;
        MostrarTestigo(indiceTestigoActual);
    }

    /// <summary>
    /// Muestra la información del testigo en la posición especificada.
    /// </summary>
    public void MostrarTestigo(int indice)
    {
        if (casoActual == null || indice < 0 || indice >= casoActual.testigos.Count) return;

        DatosTestigo testigo = casoActual.testigos[indice];

        // Actualizar textos
        if (textoNombre != null) textoNombre.text = testigo.nombreTestigo;
        if (textoRol != null) textoRol.text = testigo.rolTestigo;
        if (textoTestimonio != null) textoTestimonio.text = testigo.textoTestimonio;

        // Actualizar foto
        if (imagenFoto != null)
        {
            if (testigo.fotoTarjeta != null)
            {
                imagenFoto.sprite = testigo.fotoTarjeta;
                imagenFoto.gameObject.SetActive(true);
            }
            else
            {
                imagenFoto.gameObject.SetActive(false);
            }
        }

        // Actualizar estado e interactividad de los botones de navegación
        ActualizarEstadoBotones();
    }

    /// <summary>
    /// Avanza al siguiente testigo si está disponible.
    /// </summary>
    public void SiguienteTestigo()
    {
        if (casoActual == null) return;
        
        if (indiceTestigoActual < casoActual.testigos.Count - 1)
        {
            indiceTestigoActual++;
            MostrarTestigo(indiceTestigoActual);
        }
    }

    /// <summary>
    /// Retrocede al testigo anterior si está disponible.
    /// </summary>
    public void AnteriorTestigo()
    {
        if (indiceTestigoActual > 0)
        {
            indiceTestigoActual--;
            MostrarTestigo(indiceTestigoActual);
        }
    }

    /// <summary>
    /// Actualiza el estado visual (sprites y colores) e interactivo (interactable) de los botones.
    /// </summary>
    private void ActualizarEstadoBotones()
    {
        if (casoActual == null) return;

        bool tieneAnterior = indiceTestigoActual > 0;
        bool tieneSiguiente = indiceTestigoActual < casoActual.testigos.Count - 1;

        // Botón Anterior
        if (botonAnterior != null)
        {
            botonAnterior.interactable = tieneAnterior;
        }
        
        if (fondoBotonAnterior != null)
        {
            fondoBotonAnterior.sprite = tieneAnterior ? spriteBotonActivo : spriteBotonInactivo;
        }

        if (textoBotonAnterior != null)
        {
            textoBotonAnterior.color = tieneAnterior ? colorTextoActivo : colorTextoInactivo;
        }

        // Botón Siguiente
        if (botonSiguiente != null)
        {
            botonSiguiente.interactable = tieneSiguiente;
        }

        if (fondoBotonSiguiente != null)
        {
            fondoBotonSiguiente.sprite = tieneSiguiente ? spriteBotonActivo : spriteBotonInactivo;
        }

        if (textoBotonSiguiente != null)
        {
            textoBotonSiguiente.color = tieneSiguiente ? colorTextoActivo : colorTextoInactivo;
        }
    }
}
