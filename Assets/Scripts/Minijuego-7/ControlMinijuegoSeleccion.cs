using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControlMinijuegoSeleccion : MonoBehaviour
{
    [Header("Testimonio a Mostrar")]
    [SerializeField] private TextMeshProUGUI textoTestimonioUI;
    [TextArea]
    [SerializeField] private string testimonioDelCaso = "Vi a alguien que llevaba algo muy pesado y caminaba lento. ¡Tenía las manos manchadas de tiza azul!";

    [Header("Configuración de Sospechosos")]
    [SerializeField] private SuspectData[] sospechosos; // Configura 3 en el inspector
    [SerializeField] private int indiceSospechosoCorrecto = 2; // El índice (0, 1 o 2) que es la respuesta correcta

    [Header("Referencias de UI")]
    [SerializeField] private Button botonFinalizar; // Tu botón verde

    [Header("Referencias de Feedback")]
    [SerializeField] private PanelFeedbackReutilizable panelFeedback; // Arrastra el objeto que tiene el script de tu compañero
    [SerializeField] private int intentosRestantes = 3;

    private int indiceSeleccionado = -1;

    void Start()
    {
        // Mostrar el testimonio inicial
        if (textoTestimonioUI != null)
        {
            textoTestimonioUI.text = testimonioDelCaso;
        }

        // Configurar los botones de los sospechosos de forma dinámica
        for (int i = 0; i < sospechosos.Length; i++)
        {
            int indexLocal = i; // Necesario para la closure de C#
            if (sospechosos[i].botonContenedor != null)
            {
                // Rellenar datos visuales
                if (sospechosos[i].imagenRetrato != null && sospechosos[i].spriteRetrato != null)
                    sospechosos[i].imagenRetrato.sprite = sospechosos[i].spriteRetrato;

                if (sospechosos[i].textoAccion != null)
                    sospechosos[i].textoAccion.text = sospechosos[i].descripcionAccion;

                // Escuchar el clic en cada contenedor/botón
                sospechosos[i].botonContenedor.onClick.AddListener(() => SeleccionarSospechoso(indexLocal));
            }

            // Apagar los indicadores visuales al iniciar
            if (sospechosos[i].panelSeleccionIndicador != null)
            {
                sospechosos[i].panelSeleccionIndicador.SetActive(false);
            }
        }

        // Configurar el botón verde de validar
        if (botonFinalizar != null)
        {
            botonFinalizar.onClick.AddListener(VerificarRespuesta);
        }
    }

    public void SeleccionarSospechoso(int indice)
    {
        indiceSeleccionado = indice;
        Debug.Log("Sospechoso seleccionado: " + indice);

        // Recorremos los sospechosos: activamos el indicador del seleccionado y apagamos el resto
        for (int i = 0; i < sospechosos.Length; i++)
        {
            if (sospechosos[i].panelSeleccionIndicador != null)
            {
                bool esEsteSeleccionado = (i == indiceSeleccionado);
                sospechosos[i].panelSeleccionIndicador.SetActive(esEsteSeleccionado);
            }
        }
    }

    public void VerificarRespuesta()
    {
        if (indiceSeleccionado == -1)
        {
            Debug.LogWarning("Primero debes seleccionar un sospechoso.");
            return;
        }

        if (indiceSeleccionado == indiceSospechosoCorrecto)
        {
            if (panelFeedback != null)
            {
                panelFeedback.MostrarExito(
                    "¡Excelente Trabajo, Detective!", 
                    "Has hallado al sospechoso correcto.", 
                    "1. Analizaste el testimonio.\n2. Verificaste las pistas de la tiza.\n3. Identificaste al culpable con éxito."
                );
            }
        }
        else
        {
            intentosRestantes--;
            
            if (intentosRestantes > 0)
            {
                if (panelFeedback != null)
                {
                    panelFeedback.MostrarDerrota(
                        "¡Incorrecto!", 
                        $"Te quedan {intentosRestantes} intentos. Relee bien el testimonio."
                    );
                }
            }
            else
            {
                if (panelFeedback != null)
                {
                    panelFeedback.MostrarDerrota(
                        "¡Se acabaron los intentos!", 
                        "Has agotado tus oportunidades en este caso."
                    );
                }
            }
        }
    }
}

[System.Serializable]
public struct SuspectData
{
    public Button botonContenedor;
    public Image imagenRetrato;
    public Sprite spriteRetrato;
    public TextMeshProUGUI textoAccion;
    [TextArea] public string descripcionAccion;
    
    [Header("Indicador Visual")]
    public GameObject panelSeleccionIndicador; // El panel o marco de color que se encenderá al hacer clic
}