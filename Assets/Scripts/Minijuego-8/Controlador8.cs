using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Controlador8 : MonoBehaviour
{
    [Header("Referencias de Slots y Paneles")]
    [Tooltip("Los slots de orden (Slot1 y Slot2).")]
    [SerializeField] private DropSlot[] slotsOrden;

    [Tooltip("El panel inferior donde inician las tarjetas (Contenedorpalabras).")]
    [SerializeField] private DropSlot panelHechosOriginales;

    [Header("Lista de Tarjetas de Hechos")]
    [Tooltip("Las 5 tarjetas de palabras.")]
    [SerializeField] private TarjetaHecho[] tarjetasHechos;

    [Header("Referencias de UI Principal")]
    [SerializeField] private Button botonFinalizarCaso;
    [SerializeField] private TextMeshProUGUI textoBotonFinalizar;
    [SerializeField] private TextMeshProUGUI textoAlertaRapida;

    [Header("Configuración de Intentos / Vidas")]
    [SerializeField] private int intentosMaximos = 2;

    [Header("Indicador Visual de Vidas (Corazones)")]
    [SerializeField] private GameObject[] iconosCorazones;

    [Header("Pantalla de Feedback Reutilizable")]
    [SerializeField] private PanelFeedbackReutilizable panelFeedback;

    [Header("Textos Personalizables de Feedback (Victoria)")]
    [SerializeField] private string tituloVictoria = "¡Excelente Trabajo, Detective!";
    [SerializeField] private string mensajeVictoria = "Lograste completar los espacios en blanco correctamente.";
    [SerializeField] [TextArea(3, 6)] private string resumenPedagogicoVictoria = "<b>¿Cómo ocurrió el caso?</b>\n\n1. Alguien se acercó sigilosamente durante el <b>recreo</b> cuando el aula estaba vacía.\n2. Encontraron el <b>celular</b> escondido al fondo del casillero del pasillo.\n\n<i>¡Completar las palabras clave nos ayuda a esclarecer los hechos del misterio!</i>";

    [Header("Textos Personalizables de Feedback (Derrota)")]
    [SerializeField] private string tituloDerrota = "¡Oh no, se acabaron los intentos!";
    [SerializeField] [TextArea(2, 4)] private string mensajeDerrota = "Lee con mucha atención los detalles del caso. ¡Tómate tu tiempo y vuelve a intentarlo!";

    [Header("Textos de Alerta Flotante (Toast)")]
    [SerializeField] private string alertaEspaciosVacios = "¡Aún quedan espacios vacíos! Coloca las 2 palabras clave para finalizar.";
    [SerializeField] private string alertaIncorrecto = "¡Orden o palabras incorrectas! Fueron devueltas abajo. Inténtalo de nuevo.";

    [Header("Navegación")]
    [SerializeField] private NavegacionPrincipal navegacionPrincipal;
    [SerializeField] private bool volverAlMenuPrincipalAlResolver = true;

    private int intentosRestantes;
    private Coroutine alertaCoroutine;

    private void Start()
    {
        if (botonFinalizarCaso != null)
        {
            botonFinalizarCaso.onClick.RemoveAllListeners();
            botonFinalizarCaso.onClick.AddListener(ValidarOrden);
        }

        if (panelFeedback != null)
        {
            panelFeedback.AlContinuar.RemoveAllListeners();
            panelFeedback.AlContinuar.AddListener(AlResolverExitosamente);

            panelFeedback.AlReintentar.RemoveAllListeners();
            panelFeedback.AlReintentar.AddListener(ResetMinijuego);
        }

        ResetMinijuego();
    }

    public void ResetMinijuego()
    {
        intentosRestantes = intentosMaximos;
        ActualizarTextoIntentos();
        
        if (textoAlertaRapida != null)
        {
            textoAlertaRapida.text = "";
            var parent = textoAlertaRapida.transform.parent;
            if (parent != null) parent.gameObject.SetActive(false);
        }

        foreach (var tarjeta in tarjetasHechos)
        {
            if (tarjeta != null)
            {
                DraggableCard dragCard = tarjeta.GetComponent<DraggableCard>();
                if (dragCard != null)
                {
                    dragCard.bloqueada = false;
                    dragCard.parentAfterDrag = panelHechosOriginales.transform;
                    tarjeta.transform.SetParent(panelHechosOriginales.transform);
                }

                var img = tarjeta.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                {
                    img.color = new Color(32f/255f, 138f/255f, 151f/255f, 1f);
                }

                tarjeta.transform.localPosition = Vector3.zero;
            }
        }
    }

    public void ValidarOrden()
    {
        List<TarjetaHecho> colocadas = new List<TarjetaHecho>();
        bool todosLlenos = true;

        for (int i = 0; i < slotsOrden.Length; i++)
        {
            TarjetaHecho tarjetaEnSlot = null;
            if (slotsOrden[i] != null)
            {
                foreach (Transform child in slotsOrden[i].transform)
                {
                    TarjetaHecho th = child.GetComponent<TarjetaHecho>();
                    if (th != null && child.gameObject.activeSelf)
                    {
                        tarjetaEnSlot = th;
                        break;
                    }
                }
            }

            if (tarjetaEnSlot == null)
            {
                todosLlenos = false;
                break;
            }
            colocadas.Add(tarjetaEnSlot);
        }

        if (!todosLlenos)
        {
            MostrarAlertaRapida(alertaEspaciosVacios);
            return;
        }

        bool ordenCorrecto = true;
        List<TarjetaHecho> tarjetasIncorrectas = new List<TarjetaHecho>();

        for (int i = 0; i < slotsOrden.Length; i++)
        {
            TarjetaHecho tarjeta = colocadas[i];
            int indiceSlotEsperado = slotsOrden[i].indiceSlot;

            if (tarjeta.idCronologico == indiceSlotEsperado)
            {
                DraggableCard dragCard = tarjeta.GetComponent<DraggableCard>();
                if (dragCard != null)
                {
                    dragCard.bloqueada = true;
                }

                var img = tarjeta.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                {
                    img.color = new Color(15f/255f, 68f/255f, 75f/255f, 1f);
                }
            }
            else
            {
                ordenCorrecto = false;
                tarjetasIncorrectas.Add(tarjeta);
            }
        }

        if (ordenCorrecto)
        {
            if (panelFeedback != null)
            {
                panelFeedback.MostrarExito(tituloVictoria, mensajeVictoria, resumenPedagogicoVictoria);
            }
            else
            {
                Debug.Log("¡Victoria! " + mensajeVictoria);
            }
        }
        else
        {
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
                MostrarAlertaRapida(alertaIncorrecto);
            }
            else
            {
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
