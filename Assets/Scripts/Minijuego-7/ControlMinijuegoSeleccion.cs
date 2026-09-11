using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControlMinijuegoSeleccion : MonoBehaviour
{
    [Header("Testimonio a Mostrar")]
    [SerializeField] private TextMeshProUGUI textoTestimonioUI;
    [TextArea(2, 4)]
    [SerializeField] private string testimonioDelCaso = "Vi a alguien que llevaba algo muy pesado y caminaba lento. ¡Tenía las manos manchadas de tiza azul!";

    [Header("Configuración de Sospechosos")]
    [SerializeField] private SuspectData[] sospechosos; // Configura 3 en el inspector
    [SerializeField] private int indiceSospechosoCorrecto = 2; // El índice (0, 1 o 2) que es la respuesta correcta

    [Header("Referencias de UI")]
    [SerializeField] private Button botonFinalizar; // Tu botón verde
    [SerializeField] private TextMeshProUGUI textoBotonFinalizar;

    [Header("Referencias de Feedback")]
    [SerializeField] private PanelFeedbackReutilizable panelFeedback;
    [SerializeField] private int intentosRestantes = 3;
    [SerializeField] private bool volverAlMenuPrincipalAlResolver = true;
    [SerializeField] private string nombreEscenaMenuPrincipal = "MenuPrincipal";

    [Header("Indicador Visual de Vidas (Corazones)")]
    [SerializeField] private GameObject[] iconosCorazones;
    [SerializeField] private TextMeshProUGUI textoAlertaRapida;
    [SerializeField] private CanvasGroup panelAlertaCanvasGroup;

        [Header("Textos de Alerta Flotante (Toast)")]
    [SerializeField] private string alertaSinSeleccion = "Por favor, selecciona a un sospechoso antes de finalizar.";
    [SerializeField] private string alertaIncorrecto = "¡Sospechoso incorrecto! Te quedan {0} intentos. Relee los testimonios en tu libreta.";

    [Header("Textos Personalizables de Feedback (Victoria)")]
    [SerializeField] private string tituloVictoria = "¡Excelente Trabajo, Detective!";
    [SerializeField] private string mensajeVictoria = "Has hallado al sospechoso correcto.";
    [SerializeField] [TextArea(3, 6)] private string resumenPedagogicoVictoria = "1. Analizaste el testimonio.\n2. Verificaste las pistas de la tiza.\n3. Identificaste al culpable con éxito.";

    [Header("Textos Personalizables de Feedback (Derrota)")]
    [SerializeField] private string tituloDerrota = "¡Se acabaron los intentos!";
    [SerializeField] [TextArea(2, 4)] private string mensajeDerrota = "Has agotado tus 3 oportunidades en este caso. Relee atentamente las pistas e inténtalo de nuevo.";

    private int indexSeleccionadoActual = -1;
    private int maxIntentos;
    private Coroutine alertaCoroutine;

    private void Start()
    {
        maxIntentos = intentosRestantes;

        // Configurar los botones de los sospechosos de forma dinámica
        for (int i = 0; i < sospechosos.Length; i++)
        {
            int indexLocal = i;
            if (sospechosos[i].botonContenedor != null)
            {
                if (sospechosos[i].imagenRetrato != null && sospechosos[i].spriteRetrato != null)
                    sospechosos[i].imagenRetrato.sprite = sospechosos[i].spriteRetrato;

                if (sospechosos[i].textoAccion != null)
                    sospechosos[i].textoAccion.text = sospechosos[i].descripcionAccion;

                sospechosos[i].botonContenedor.onClick.RemoveAllListeners();
                sospechosos[i].botonContenedor.onClick.AddListener(() => SeleccionarSospechoso(indexLocal));
            }

            // Ocultar indicadores visuales al inicio
            if (sospechosos[i].panelSeleccionIndicador != null)
            {
                sospechosos[i].panelSeleccionIndicador.SetActive(false);
            }
        }

        // Cargar texto del testimonio inicial
        if (textoTestimonioUI != null)
        {
            textoTestimonioUI.text = testimonioDelCaso;
        }

        // Configurar botón finalizar
        if (botonFinalizar != null)
        {
            botonFinalizar.onClick.RemoveAllListeners();
            botonFinalizar.onClick.AddListener(ValidarSeleccion);
        }

        // Suscribirse a eventos del panel reutilizable
        if (panelFeedback != null)
        {
            panelFeedback.AlContinuar.RemoveAllListeners();
            panelFeedback.AlContinuar.AddListener(OnContinuar);

            panelFeedback.AlReintentar.RemoveAllListeners();
            panelFeedback.AlReintentar.AddListener(OnReintentar);
        }

        ActualizarUIIntentos();
    }

    private void SeleccionarSospechoso(int index)
    {
        indexSeleccionadoActual = index;

        // Recorremos los sospechosos: activamos el indicador del seleccionado
        for (int i = 0; i < sospechosos.Length; i++)
        {
            bool esEsteSeleccionado = (i == index);
            if (sospechosos[i].panelSeleccionIndicador != null)
            {
                sospechosos[i].panelSeleccionIndicador.SetActive(esEsteSeleccionado);
            }
        }
    }

    private void ActualizarUIIntentos()
    {
        if (iconosCorazones != null && iconosCorazones.Length > 0)
        {
            if (textoBotonFinalizar != null)
            {
                textoBotonFinalizar.text = "Finalizar caso";
            }

            for (int i = 0; i < iconosCorazones.Length; i++)
            {
                if (iconosCorazones[i] != null)
                {
                    iconosCorazones[i].SetActive(i < intentosRestantes);
                }
            }
        }
        else
        {
            if (textoBotonFinalizar != null)
            {
                textoBotonFinalizar.text = $"Finalizar caso (intentos restantes: {intentosRestantes}/{maxIntentos})";
            }
        }
    }

    private void MostrarAlertaRapida(string texto)
    {
        if (textoAlertaRapida != null)
        {
            textoAlertaRapida.text = texto;

            var parent = textoAlertaRapida.transform.parent;
            if (parent != null)
            {
                var group = parent.GetComponent<CanvasGroup>();
                if (group == null) group = parent.gameObject.AddComponent<CanvasGroup>();

                if (alertaCoroutine != null) StopCoroutine(alertaCoroutine);
                alertaCoroutine = StartCoroutine(AnimarAlerta(group));
            }
        }
    }

    private System.Collections.IEnumerator AnimarAlerta(CanvasGroup group)
    {
        group.gameObject.SetActive(true);

        // Fade In
        float t = 0f;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(0f, 1f, t / 0.2f);
            yield return null;
        }
        group.alpha = 1f;

        // Wait
        yield return new WaitForSeconds(3.0f);

        // Fade Out
        t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(1f, 0f, t / 0.3f);
            yield return null;
        }
        group.alpha = 0f;
        group.gameObject.SetActive(false);
    }

    public void ValidarSeleccion()
    {
        // Si no se ha elegido ninguno
        if (indexSeleccionadoActual == -1)
        {
            MostrarAlertaRapida(alertaSinSeleccion);
            return;
        }

        // Si la selección es correcta
        if (indexSeleccionadoActual == indiceSospechosoCorrecto)
        {
            if (panelFeedback != null)
            {
                panelFeedback.MostrarExito(
                    tituloVictoria,
                    mensajeVictoria,
                    resumenPedagogicoVictoria
                );
            }
        }
        else
        {
            intentosRestantes--;
            ActualizarUIIntentos();

            if (intentosRestantes > 0)
            {
                MostrarAlertaRapida(string.Format(alertaIncorrecto, intentosRestantes));
            }
            else
            {
                if (panelFeedback != null)
                {
                    panelFeedback.MostrarDerrota(
                        tituloDerrota,
                        mensajeDerrota
                    );
                }
            }
        }
    }

    private void OnContinuar()
    {
        if (volverAlMenuPrincipalAlResolver)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nombreEscenaMenuPrincipal);
        }
        else
        {
            Debug.Log("Avanzando a la siguiente ronda o fase de preguntas...");
        }
    }

    private void OnReintentar()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
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
    public GameObject panelSeleccionIndicador;
}
