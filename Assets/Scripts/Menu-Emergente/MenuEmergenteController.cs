using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuEmergenteController : MonoBehaviour
{
    [Header("Caso y Testigos")]
    [Tooltip("El caso actual que contiene la información de los testigos.")]
    public DatosCaso casoActual;

    [Header("Referencias de UI (Panel Deslizable)")]
    [SerializeField] private RectTransform panelContenedor;
    [SerializeField] private Button botonCerrar;

    [Header("Referencias de UI (Detalle del Testimonio)")]
    [SerializeField] private TextMeshProUGUI textoTestimonioDetalle;
    [SerializeField] private Image imagenDetalle;

    [Header("Configuración de Lista de Testigos")]
    [Tooltip("El GameObject contenedor que tiene la lista de testigos en la UI.")]
    [SerializeField] private RectTransform contenedorLista;

    [Tooltip("Prefab para instanciar dinámicamente los elementos de la lista.")]
    [SerializeField] private GameObject prefabElementoLista;

    [Header("Configuración de Desplazamiento Lateral")]
    [Tooltip("Posición X en RectTransform cuando el menú está cerrado.")]
    [SerializeField] private float posicionCerradoX = -453f;

    [Tooltip("Posición X en RectTransform cuando el menú está abierto.")]
    [SerializeField] private float posicionAbiertoX = 0f;

    [Tooltip("Duración en segundos del efecto de deslizamiento.")]
    [SerializeField] private float duracionDesplazamiento = 0.25f;

    private List<ElementoListaTestigo> elementosLista = new List<ElementoListaTestigo>();
    private int indiceSeleccionado = -1;
    private Coroutine corrutinaDesplazamiento;
    private bool estaAbierto = false;

    private void Start()
    {
        // Configurar el botón de cerrar si está asignado
        if (botonCerrar != null)
        {
            botonCerrar.onClick.RemoveAllListeners();
            botonCerrar.onClick.AddListener(() => AlternarMenu(false));
        }

        // Colocar el menú en posición inicial cerrada
        if (panelContenedor != null)
        {
            Vector2 pos = panelContenedor.anchoredPosition;
            pos.x = posicionCerradoX;
            panelContenedor.anchoredPosition = pos;
        }

        InicializarLista();
    }

    /// <summary>
    /// Puebla e inicializa dinámicamente los elementos en la lista del menú emergente.
    /// </summary>
    private void InicializarLista()
    {
        if (casoActual == null)
        {
            Debug.LogError("MenuEmergenteController: No se ha asignado un DatosCaso.");
            return;
        }

        elementosLista.Clear();

        if (prefabElementoLista == null || contenedorLista == null)
        {
            Debug.LogError("MenuEmergenteController: Asegúrate de asignar el Prefab y el Contenedor de la lista en el inspector.");
            return;
        }

        // Limpiar hijos existentes por si acaso
        foreach (Transform hijo in contenedorLista)
        {
            Destroy(hijo.gameObject);
        }

        // Instanciar dinámicamente cada testigo del caso
        for (int i = 0; i < casoActual.testigos.Count; i++)
        {
            GameObject nuevoObj = Instantiate(prefabElementoLista, contenedorLista);
            ElementoListaTestigo scriptElemento = nuevoObj.GetComponent<ElementoListaTestigo>();
            if (scriptElemento != null)
            {
                scriptElemento.Configurar(i, casoActual.testigos[i], this);
                elementosLista.Add(scriptElemento);
            }
        }

        // Seleccionar el primer testigo por defecto si existe alguno
        if (casoActual.testigos.Count > 0)
        {
            SeleccionarTestigo(0);
        }
    }

    /// <summary>
    /// Selecciona visualmente un testigo y actualiza el panel de información lateral.
    /// </summary>
    public void SeleccionarTestigo(int indice)
    {
        if (casoActual == null || indice < 0 || indice >= casoActual.testigos.Count) return;

        indiceSeleccionado = indice;
        DatosTestigo testigo = casoActual.testigos[indice];

        // Actualizar textos y fotos del panel de detalle
        if (textoTestimonioDetalle != null)
        {
            // Usamos el testimonio detallado si está disponible; de lo contrario, usamos el normal.
            string textoAMostrar = !string.IsNullOrEmpty(testigo.textoTestimonioDetalle) 
                ? testigo.textoTestimonioDetalle 
                : testigo.textoTestimonio;
            textoTestimonioDetalle.text = textoAMostrar;
        }

        if (imagenDetalle != null)
        {
            // Usamos la fotoDetalle (evidencia/pista). Si no existe, usamos la fotoTarjeta (original).
            Sprite spriteAMostrar = testigo.fotoDetalle != null ? testigo.fotoDetalle : testigo.fotoTarjeta;
            if (spriteAMostrar != null)
            {
                imagenDetalle.sprite = spriteAMostrar;
                imagenDetalle.gameObject.SetActive(true);
            }
            else
            {
                imagenDetalle.gameObject.SetActive(false);
            }
        }

        // Actualizar el estado visual seleccionado en la lista
        for (int i = 0; i < elementosLista.Count; i++)
        {
            if (elementosLista[i] != null)
            {
                elementosLista[i].EstablecerSeleccionado(i == indiceSeleccionado);
            }
        }
    }

    /// <summary>
    /// Abre o cierra de manera animada (deslizamiento lateral) el panel.
    /// </summary>
    public void AlternarMenu(bool abrir)
    {
        if (estaAbierto == abrir) return;

        estaAbierto = abrir;

        if (corrutinaDesplazamiento != null)
        {
            StopCoroutine(corrutinaDesplazamiento);
        }

        float destinoX = abrir ? posicionAbiertoX : posicionCerradoX;
        corrutinaDesplazamiento = StartCoroutine(AnimarDesplazamiento(destinoX));
    }

    /// <summary>
    /// Alterna el estado del menú entre abierto y cerrado (método de conveniencia).
    /// </summary>
    public void CambiarEstadoMenu()
    {
        AlternarMenu(!estaAbierto);
    }

    /// <summary>
    /// Corrutina para interpolar linealmente (Lerp) la posición lateral del panel.
    /// </summary>
    private IEnumerator AnimarDesplazamiento(float destinoX)
    {
        if (panelContenedor == null) yield break;

        Vector2 posInicial = panelContenedor.anchoredPosition;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracionDesplazamiento)
        {
            tiempoTranscurrido += Time.deltaTime;
            float t = Mathf.Clamp01(tiempoTranscurrido / duracionDesplazamiento);
            
            // Suavizado de la curva de animación (SmoothStep)
            t = t * t * (3f - 2f * t);

            Vector2 nuevaPos = panelContenedor.anchoredPosition;
            nuevaPos.x = Mathf.Lerp(posInicial.x, destinoX, t);
            panelContenedor.anchoredPosition = nuevaPos;

            yield return null;
        }

        Vector2 posFinal = panelContenedor.anchoredPosition;
        posFinal.x = destinoX;
        panelContenedor.anchoredPosition = posFinal;
    }
}
