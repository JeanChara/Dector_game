using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Controlador8 : MonoBehaviour
{
    [Header("Referencias de Slots y Paneles")]
    [Tooltip("Los slots de orden (deben estar indexados en sus propiedades).")]
    [SerializeField] private DropSlot[] slotsOrden;

    [Tooltip("El panel inferior donde inician las tarjetas.")]
    [SerializeField] private DropSlot panelHechosOriginales;

    [Header("Lista de Tarjetas de Hechos")]
    [Tooltip("Las tarjetas que contienen la lógica TarjetaHecho.")]
    [SerializeField] private TarjetaHecho[] tarjetasHechos;

    [Header("Referencias de UI Principal")]
    [SerializeField] private Button botonFinalizarCaso;
    [SerializeField] private TextMeshProUGUI textoBotonFinalizar;
    [SerializeField] private TextMeshProUGUI textoAlertaRapida; // Mensajes rápidos en pantalla

    [Header("Pantalla de Feedback Reutilizable")]
    [SerializeField] private PanelFeedbackReutilizable panelFeedback;

    [Header("Navegación")]
    [SerializeField] private NavegacionPrincipal navegacionPrincipal;

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
    /// Restablece el minijuego: limpia slots y reinicia intentos.
    /// </summary>
    public void ResetMinijuego()
    {
        intentosRestantes = intentosMaximos;
        ActualizarTextoIntentos();
        if (textoAlertaRapida != null) textoAlertaRapida.text = "";

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
                
                // Restaurar color original de la tarjeta
                var img = tarjeta.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                {
                    img.color = new Color(32f/255f, 138f/255f, 151f/255f, 1f);
                }
                
                tarjeta.transform.localPosition = Vector3.zero;
            }
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
            MostrarAlertaRapida("<color=red>¡Aún quedan espacios vacíos! Coloca todas las palabras para finalizar.</color>");
            return;
        }

        // 2. Verificar la correspondencia correcta de los IDs de cada slot
        bool ordenCorrecto = true;
        List<TarjetaHecho> tarjetasIncorrectas = new List<TarjetaHecho>();

        for (int i = 0; i < slotsOrden.Length; i++)
        {
            TarjetaHecho tarjeta = colocadas[i];
            int indiceSlotEsperado = slotsOrden[i].indiceSlot; // El ID que exige este slot en específico

            // Comparamos el ID de la tarjeta con el ID requerido por el slot
            if (tarjeta.idCronologico == indiceSlotEsperado)
            {
                // Bloquear y oscurecer tarjeta correcta
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

        // 3. Procesar resultados
        if (ordenCorrecto)
        {
            // ÉXITO PEDAGÓGICO
            string titulo = "¡Excelente Trabajo, Detective!";
            string mensaje = "Lograste completar los espacios en blanco correctamente.";
            
            string resumenPedagogico = 
                "<b>¿Cómo ocurrió el caso?</b>\n\n" +
                "1. Alguien se acercó sigilosamente durante el <b>recreo</b> cuando el aula estaba vacía.\n" +
                "2. Encontraron el <b>celular</b> escondido al fondo del casillero del pasillo.\n\n" +
                "<i>¡Completar las palabras clave nos ayuda a esclarecer los hechos del misterio!</i>";

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
            // FALLO: Devolver únicamente las tarjetas incorrectas al panel original
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
                MostrarAlertaRapida("<color=orange>¡Algunas palabras no son correctas! Fueron devueltas abajo. Inténtalo de nuevo.</color>");
            }
            else
            {
                // SIN INTENTOS
                string tituloDerrota = "¡Oh no, se acabaron los intentos!";
                string mensajeDerrota = "Lee con mucha atención los detalles del caso. ¡Tómate tu tiempo y vuelve a intentarlo!";
                
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
        if (textoBotonFinalizar != null)
        {
            textoBotonFinalizar.text = $"Finalizar caso (intentos restantes: {intentosRestantes}/{intentosMaximos})";
        }
    }

    private void MostrarAlertaRapida(string texto)
    {
        if (textoAlertaRapida != null)
        {
            textoAlertaRapida.text = texto;
        }
    }

    private void AlResolverExitosamente()
    {
        if (navegacionPrincipal != null)
        {
            navegacionPrincipal.VolverATestimonios();
        }
    }
}