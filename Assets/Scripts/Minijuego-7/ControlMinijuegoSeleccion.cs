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
    [SerializeField] private TextMeshProUGUI textoBotonFinalizar;

    [Header("Referencias de Feedback")]
    [SerializeField] private PanelFeedbackReutilizable panelFeedback; // Arrastra el objeto que tiene el script de tu compañero
    [SerializeField] private int intentosRestantes = 3;
    [SerializeField] private bool volverAlMenuPrincipalAlResolver = true;
    [SerializeField] private string nombreEscenaMenuPrincipal = "MenuPrincipal";

    [Header("Indicador Visual de Vidas (Corazones)")]
    [SerializeField] private GameObject[] iconosCorazones; // 3 objetos de corazones
    [SerializeField] private TextMeshProUGUI textoAlertaRapida;
    [SerializeField] private CanvasGroup panelAlertaCanvasGroup;

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
            if (textoBotonFinalizar == null)
            {
                textoBotonFinalizar = botonFinalizar.GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        // Configurar eventos de continuación o reintento en el panel de feedback
        if (panelFeedback != null)
        {
            panelFeedback.AlContinuar.RemoveAllListeners();
            panelFeedback.AlContinuar.AddListener(OnContinuar);

            panelFeedback.AlReintentar.RemoveAllListeners();
            panelFeedback.AlReintentar.AddListener(OnReintentar);
        }

        ActualizarUIIntentos();
    }

    public void SeleccionarSospechoso(int indice)
    {
        indiceSeleccionado = indice;
        Debug.Log("Sospechoso seleccionado: " + indice);

        // Recorremos los sospechosos: activamos el indicador del seleccionado y mantenemos el estilo 2.5D intacto
        for (int i = 0; i < sospechosos.Length; i++)
        {
            bool esEsteSeleccionado = (i == indiceSeleccionado);
            
            if (sospechosos[i].panelSeleccionIndicador != null)
            {
                sospechosos[i].panelSeleccionIndicador.SetActive(esEsteSeleccionado);
            }

            if (sospechosos[i].botonContenedor != null)
            {
                var img = sospechosos[i].botonContenedor.GetComponent<Image>();
                if (img != null)
                {
                    // Tono cálido/dorado suave para el seleccionado sin quitar los bordes
                    img.color = esEsteSeleccionado ? new Color(1.0f, 0.96f, 0.82f, 1.0f) : Color.white;
                }
            }
        }
    }

    private void ActualizarUIIntentos()
    {
        // 1. Sincronizar texto del botón "Finalizar caso (intentos restantes: X/3)"
        if (textoBotonFinalizar == null && botonFinalizar != null)
        {
            textoBotonFinalizar = botonFinalizar.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (textoBotonFinalizar != null)
        {
            textoBotonFinalizar.text = $"Finalizar caso (intentos restantes: {intentosRestantes}/3)";
        }

        // 2. Sincronizar iconos de corazones
        if (iconosCorazones != null)
        {
            for (int i = 0; i < iconosCorazones.Length; i++)
            {
                if (iconosCorazones[i] != null)
                {
                    iconosCorazones[i].SetActive(i < intentosRestantes);
                }
            }
        }
    }

    private Coroutine alertaCoroutine;

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
        group.alpha = 0f;
        
        // Fade In
        float t = 0f;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(0f, 1f, t / 0.2f);
            yield return null;
        }
        group.alpha = 1f;

        // Mantener visible
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

    public void VerificarRespuesta()
    {
        if (indiceSeleccionado == -1)
        {
            MostrarAlertaRapida("Por favor, selecciona a un sospechoso antes de finalizar.");
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
            ActualizarUIIntentos();
            
            if (intentosRestantes > 0)
            {
                // Muestra alerta emergente Toast sin cerrar el minijuego ni reiniciar el caso
                MostrarAlertaRapida($"¡Sospechoso incorrecto! Te quedan {intentosRestantes} intentos. Relee los testimonios en tu libreta.");
            }
            else
            {
                // Agotó los 3 intentos: muestra pantalla de derrota final para reintentar
                if (panelFeedback != null)
                {
                    panelFeedback.MostrarDerrota(
                        "¡Se acabaron los intentos!", 
                        "Has agotado tus 3 oportunidades en este caso. Relee atentamente las pistas e inténtalo de nuevo."
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
    public GameObject panelSeleccionIndicador; // El panel o marco de color que se encenderá al hacer clic
}