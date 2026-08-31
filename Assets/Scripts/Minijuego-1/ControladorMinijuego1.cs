using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControladorMinijuego1 : MonoBehaviour
{
    [Header("Referencias de Slots y Paneles")]
    [Tooltip("Los 5 slots de orden cronológico (deben estar indexados del 1 al 5 en sus propiedades).")]
    [SerializeField] private DropSlot[] slotsOrden;

    [Tooltip("El panel inferior donde inician las tarjetas.")]
    [SerializeField] private DropSlot panelHechosOriginales;

    [Header("Lista de Tarjetas de Hechos")]
    [Tooltip("Las 5 tarjetas que contienen la lógica TarjetaHecho.")]
    [SerializeField] private TarjetaHecho[] tarjetasHechos;

    [Header("Referencias de UI Principal")]
    [SerializeField] private Button botonFinalizarCaso;
    [SerializeField] private TextMeshProUGUI textoBotonFinalizar;
    [SerializeField] private TextMeshProUGUI textoAlertaRapida; // Mensajes rápidos en pantalla

    [Header("Pantalla de Feedback Reutilizable")]
    [SerializeField] private PanelFeedbackReutilizable panelFeedback;

    [Header("Navegación")]
    [SerializeField] private NavegacionPrincipal navegacionPrincipal;

    [Header("Retroalimentación de Éxito")]
    [TextArea(8, 12)]
    [SerializeField] private string resumenPedagogicoPersonalizado;

    [Header("Referencias de Vidas (Corazones)")]
    [SerializeField] private GameObject[] iconosCorazones;

    [Header("Configuración de Fin de Juego")]
    [Tooltip("Si está activo, al resolver el caso se cargará el Menú Principal directamente.")]
    [SerializeField] private bool volverAlMenuPrincipalAlResolver = false;

    private int intentosMaximos = 3;
    private int intentosRestantes;

    private void Start()
    {
        if (botonFinalizarCaso != null)
        {
            botonFinalizarCaso.onClick.RemoveAllListeners();
            botonFinalizarCaso.onClick.AddListener(ValidarOrden);
        }

        // Suscribirse a los eventos del panel de feedback
        if (panelFeedback != null)
        {
            panelFeedback.AlContinuar.AddListener(AlResolverExitosamente);
            panelFeedback.AlReintentar.AddListener(ResetMinijuego);
        }

        ResetMinijuego();
    }

    /// <summary>
    /// Restablece el minijuego: mezcla tarjetas, limpia slots y reinicia intentos.
    /// </summary>
    public void ResetMinijuego()
    {
        intentosRestantes = intentosMaximos;
        ActualizarTextoIntentos();
        if (textoAlertaRapida != null)
        {
            textoAlertaRapida.text = "";
            var parent = textoAlertaRapida.transform.parent;
            if (parent != null)
            {
                var group = parent.GetComponent<CanvasGroup>();
                if (group != null) group.alpha = 0f;
                parent.gameObject.SetActive(false);
            }
        }

        // Regresar todas las tarjetas al contenedor original y limpiarlas de los slots
        foreach (var tarjeta in tarjetasHechos)
        {
            if (tarjeta != null)
            {
                DraggableCard dragCard = tarjeta.GetComponent<DraggableCard>();
                if (dragCard != null)
                {
                    dragCard.bloqueada = false; // Reset lock state
                    dragCard.parentAfterDrag = panelHechosOriginales.transform;
                    tarjeta.transform.SetParent(panelHechosOriginales.transform);
                }
                
                // Restore original teal color
                var img = tarjeta.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                {
                    img.color = new Color(32f/255f, 138f/255f, 151f/255f, 1f);
                }
                
                tarjeta.transform.localPosition = Vector3.zero;
            }
        }

        // Mezclar las tarjetas en el panel original
        MezclarTarjetas();
    }

    /// <summary>
    /// Mezcla de forma aleatoria el orden visual de las tarjetas en el panel inferior.
    /// </summary>
    private void MezclarTarjetas()
    {
        if (panelHechosOriginales == null) return;

        int childCount = panelHechosOriginales.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            int randomIndex = Random.Range(i, childCount);
            panelHechosOriginales.transform.GetChild(i).SetSiblingIndex(randomIndex);
        }
    }

    /// <summary>
    /// Valida el orden actual colocado por el estudiante.
    /// </summary>
    public void ValidarOrden()
    {
        if (textoAlertaRapida != null) textoAlertaRapida.text = "";

        // 1. Verificar si todos los slots están llenos
        List<TarjetaHecho> colocadas = new List<TarjetaHecho>();
        bool todosLlenos = true;

        for (int i = 0; i < slotsOrden.Length; i++)
        {
            TarjetaHecho tarjetaEnSlot = slotsOrden[i].GetComponentInChildren<TarjetaHecho>();
            if (tarjetaEnSlot == null)
            {
                todosLlenos = false;
                break;
            }
            colocadas.Add(tarjetaEnSlot);
        }

        if (!todosLlenos)
        {
            MostrarAlertaRapida("¡Aún quedan espacios vacíos! Coloca las 5 tarjetas para finalizar.");
            return;
        }

        // 2. Verificar el orden cronológico
        bool ordenCorrecto = true;
        List<TarjetaHecho> tarjetasIncorrectas = new List<TarjetaHecho>();

        for (int i = 0; i < slotsOrden.Length; i++)
        {
            TarjetaHecho tarjeta = colocadas[i];
            int indiceSlotEsperado = slotsOrden[i].indiceSlot; // Debe corresponder al idCronologico (1 a 5)

            if (tarjeta.idCronologico == indiceSlotEsperado)
            {
                // Lock and darken correct cards
                DraggableCard dragCard = tarjeta.GetComponent<DraggableCard>();
                if (dragCard != null)
                {
                    dragCard.bloqueada = true;
                }

                var img = tarjeta.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                {
                    img.color = new Color(15f/255f, 68f/255f, 75f/255f, 1f); // Darkened teal
                }
            }
            else
            {
                ordenCorrecto = false;
                tarjetasIncorrectas.Add(tarjeta);
            }
        }

        // 3. Procesar resultados
        if (ordenCorrecto)
        {
            // ÉXITO PEDAGÓGICO
            string titulo = "¡Excelente Trabajo, Detective!";
            string mensaje = "Lograste ordenar cronológicamente todos los acontecimientos de forma perfecta.";
            
            string resumenPedagogico = resumenPedagogicoPersonalizado;
            if (string.IsNullOrEmpty(resumenPedagogico))
            {
                resumenPedagogico = 
                    "<b>¿Cómo ocurrieron los hechos?</b>\n\n" +
                    "1. <b>Primero:</b> Diego dejó su celular en la mesa antes de salir al recreo.\n" +
                    "2. <b>Luego:</b> Mientras todos estaban en el patio, un estudiante entró rápidamente al salón.\n" +
                    "3. <b>Después:</b> Diego regresó y se dio cuenta de que su celular ya no estaba.\n" +
                    "4. <b>A continuación:</b> El profesor llegó y vio a Diego buscando preocupado.\n" +
                    "5. <b>Finalmente:</b> Sus compañeros se acercaron a ayudarlo a buscar.\n\n" +
                    "<i>¡Reconocer el orden temporal nos ayuda a comprender mejor las historias que leemos!</i>";
            }

            if (panelFeedback != null)
            {
                panelFeedback.MostrarExito(titulo, mensaje, resumenPedagogico);
            }
            else
            {
                Debug.Log("¡Victoria! " + mensaje);
            }
        }
        else
        {
            // FALLO: Devolver las incorrectas al panel original
            foreach (var tarjetaInc in tarjetasIncorrectas)
            {
                DraggableCard dragCard = tarjetaInc.GetComponent<DraggableCard>();
                if (dragCard != null)
                {
                    dragCard.parentAfterDrag = panelHechosOriginales.transform;
                    tarjetaInc.transform.SetParent(panelHechosOriginales.transform);
                }
                tarjetaInc.transform.localPosition = Vector3.zero;
            }

            intentosRestantes--;
            ActualizarTextoIntentos();

            if (intentosRestantes > 0)
            {
                MostrarAlertaRapida("¡Algunas tarjetas no estaban en el orden correcto! Fueron devueltas abajo. Inténtalo de nuevo.");
            }
            else
            {
                // SIN INTENTOS
                string tituloDerrota = "¡Oh no, se acabaron los intentos!";
                string mensajeDerrota = "Lee con mucha atención los testimonios de los personajes. ¡Tómate tu tiempo y vuelve a intentarlo!";
                
                if (panelFeedback != null)
                {
                    panelFeedback.MostrarDerrota(tituloDerrota, mensajeDerrota);
                }
                else
                {
                    Debug.Log("Derrota. Intentos agotados.");
                }
            }
        }
    }

    private void ActualizarTextoIntentos()
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
                textoBotonFinalizar.text = $"Finalizar caso (intentos restantes: {intentosRestantes}/{intentosMaximos})";
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

    private void AlResolverExitosamente()
    {
        if (volverAlMenuPrincipalAlResolver)
        {
            if (navegacionPrincipal != null)
            {
                navegacionPrincipal.VolverAlMenuPrincipal();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MenuPrincipal");
            }
        }
        else
        {
            if (navegacionPrincipal != null)
            {
                navegacionPrincipal.VolverATestimonios();
            }
        }
    }
}
