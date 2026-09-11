using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ControladorMenuPrincipal : MonoBehaviour
{
    [Header("Estilos UI Toolkit")]
    [SerializeField] private StyleSheet estiloMenu;

    [Header("Datos de Casos")]
    [Tooltip("Asigna el ScriptableObject con la lista de casos configurados.")]
    [SerializeField] private DatosListaCasos datosListaCasos;

    private UIDocument uiDocument;
    private VisualElement root;

    // Elementos UXML del Carrusel Central
    private Button btnAnteriorCaso;
    private Button btnSiguienteCaso;
    private VisualElement visorCasoActivo;
    private VisualElement badgeCasoActivo;
    private Label txtBadgeCaso;
    private Label tituloCasoActivo;
    private Label descCasoActivo;
    private Button btnIniciarCasoActivo;
    private Label txtContadorCaso;
    private VisualElement indicadoresPuntos;
    private Button btnSalirJuego;

    // Previsualización Lateral Izquierda (Caso Anterior)
    private VisualElement cardCasoPrev;
    private VisualElement badgeCasoPrev;
    private Label txtBadgePrev;
    private Label tituloCasoPrev;

    // Previsualización Lateral Derecha (Caso Siguiente)
    private VisualElement cardCasoNext;
    private VisualElement badgeCasoNext;
    private Label txtBadgeNext;
    private Label tituloCasoNext;

    // Estado interno del carrusel
    private int indiceCasoActual = 0;
    private List<ItemCasoMenu> listaCasos = new List<ItemCasoMenu>();

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        if (uiDocument != null)
        {
            root = uiDocument.rootVisualElement;
            InicializarUI();
        }
    }

    public void InicializarUI()
    {
        if (root == null) return;

        // Configurar Botón de Salir (Arriba a la Izquierda)
        btnSalirJuego = root.Q<Button>("btnSalirJuego");
        if (btnSalirJuego == null)
        {
            btnSalirJuego = new Button();
            btnSalirJuego.name = "btnSalirJuego";
            btnSalirJuego.text = "✕  SALIR";
            btnSalirJuego.style.position = Position.Absolute;
            btnSalirJuego.style.top = 25;
            btnSalirJuego.style.left = 25;
            btnSalirJuego.style.backgroundColor = new Color(0.141f, 0.094f, 0.059f, 0.95f);
            btnSalirJuego.style.color = Color.white;
            btnSalirJuego.style.fontSize = 15;
            btnSalirJuego.style.unityFontStyleAndWeight = FontStyle.Bold;
            btnSalirJuego.style.borderTopLeftRadius = 10;
            btnSalirJuego.style.borderTopRightRadius = 10;
            btnSalirJuego.style.borderBottomLeftRadius = 10;
            btnSalirJuego.style.borderBottomRightRadius = 10;
            btnSalirJuego.style.paddingLeft = 18;
            btnSalirJuego.style.paddingRight = 18;
            btnSalirJuego.style.paddingTop = 10;
            btnSalirJuego.style.paddingBottom = 10;
            btnSalirJuego.style.borderLeftColor = new Color(0.243f, 0.169f, 0.118f, 1f);
            btnSalirJuego.style.borderRightColor = new Color(0.243f, 0.169f, 0.118f, 1f);
            btnSalirJuego.style.borderTopColor = new Color(0.243f, 0.169f, 0.118f, 1f);
            btnSalirJuego.style.borderBottomColor = new Color(0.243f, 0.169f, 0.118f, 1f);
            btnSalirJuego.style.borderLeftWidth = 2;
            btnSalirJuego.style.borderRightWidth = 2;
            btnSalirJuego.style.borderTopWidth = 2;
            btnSalirJuego.style.borderBottomWidth = 2;
            root.Add(btnSalirJuego);
        }

        if (btnSalirJuego != null)
        {
            btnSalirJuego.clicked -= SalirDelJuego;
            btnSalirJuego.clicked += SalirDelJuego;
            btnSalirJuego.BringToFront();
            btnSalirJuego.RegisterCallback<PointerDownEvent>(evt => {
                btnSalirJuego.style.scale = new Scale(new Vector3(0.92f, 0.92f, 1f));
            });
            btnSalirJuego.RegisterCallback<PointerUpEvent>(evt => {
                btnSalirJuego.style.scale = new Scale(new Vector3(1f, 1f, 1f));
            });
        }

        if (estiloMenu != null && !root.styleSheets.Contains(estiloMenu))
        {
            root.styleSheets.Add(estiloMenu);
        }

        // 1. Obtener referencias UXML
        btnAnteriorCaso = root.Q<Button>("btnAnteriorCaso");
        btnSiguienteCaso = root.Q<Button>("btnSiguienteCaso");
        visorCasoActivo = root.Q<VisualElement>("visorCasoActivo");
        badgeCasoActivo = root.Q<VisualElement>("badgeCasoActivo");
        txtBadgeCaso = root.Q<Label>("txtBadgeCaso");
        tituloCasoActivo = root.Q<Label>("tituloCasoActivo");
        descCasoActivo = root.Q<Label>("descCasoActivo");
        btnIniciarCasoActivo = root.Q<Button>("btnIniciarCasoActivo");
        txtContadorCaso = root.Q<Label>("txtContadorCaso");
        indicadoresPuntos = root.Q<VisualElement>("indicadoresPuntos");

        // Previsualizaciones laterales
        cardCasoPrev = root.Q<VisualElement>("cardCasoPrev");
        badgeCasoPrev = root.Q<VisualElement>("badgeCasoPrev");
        txtBadgePrev = root.Q<Label>("txtBadgePrev");
        tituloCasoPrev = root.Q<Label>("tituloCasoPrev");

        cardCasoNext = root.Q<VisualElement>("cardCasoNext");
        badgeCasoNext = root.Q<VisualElement>("badgeCasoNext");
        txtBadgeNext = root.Q<Label>("txtBadgeNext");
        tituloCasoNext = root.Q<Label>("tituloCasoNext");

        // 2. Registrar Eventos de Navegación del Carrusel
        if (btnAnteriorCaso != null)
        {
            btnAnteriorCaso.clicked += NavegarAnterior;
            btnAnteriorCaso.RegisterCallback<PointerDownEvent>(evt => {
                if (indiceCasoActual > 0) btnAnteriorCaso.style.scale = new Scale(new Vector3(0.92f, 0.92f, 1f));
            });
            btnAnteriorCaso.RegisterCallback<PointerUpEvent>(evt => {
                btnAnteriorCaso.style.scale = new Scale(new Vector3(1f, 1f, 1f));
            });
        }

        if (btnSiguienteCaso != null)
        {
            btnSiguienteCaso.clicked += NavegarSiguiente;
            btnSiguienteCaso.RegisterCallback<PointerDownEvent>(evt => {
                if (indiceCasoActual < listaCasos.Count - 1) btnSiguienteCaso.style.scale = new Scale(new Vector3(0.92f, 0.92f, 1f));
            });
            btnSiguienteCaso.RegisterCallback<PointerUpEvent>(evt => {
                btnSiguienteCaso.style.scale = new Scale(new Vector3(1f, 1f, 1f));
            });
        }

        // Clic directo en las tarjetas laterales para navegar
        if (cardCasoPrev != null)
        {
            cardCasoPrev.RegisterCallback<PointerDownEvent>(evt => NavegarAnterior());
        }

        if (cardCasoNext != null)
        {
            cardCasoNext.RegisterCallback<PointerDownEvent>(evt => NavegarSiguiente());
        }

        // 3. Cargar la lista de casos
        CargarListaCasos();

        // 4. Mostrar el caso inicial
        MostrarCasoActual();
    }

    private void CargarListaCasos()
    {
        listaCasos.Clear();

        if (datosListaCasos != null && datosListaCasos.listaCasos != null && datosListaCasos.listaCasos.Count > 0)
        {
            listaCasos.AddRange(datosListaCasos.listaCasos);
        }
        else
        {
            // Fallback por defecto si no hay ScriptableObject asignado aún
            listaCasos.Add(new ItemCasoMenu
            {
                numeroCaso = "CASO 01",
                nombreEscena = "EscenaCaso_01",
                colorTema = new Color(0.9f, 0.47f, 0.0f),
                colorBoton = new Color(1.0f, 0.57f, 0.17f)
            });
            listaCasos.Add(new ItemCasoMenu
            {
                numeroCaso = "CASO 02",
                nombreEscena = "EscenaCaso_02",
                colorTema = new Color(0.09f, 0.39f, 0.67f),
                colorBoton = new Color(0.3f, 0.67f, 0.97f)
            });
            listaCasos.Add(new ItemCasoMenu
            {
                numeroCaso = "CASO 03",
                nombreEscena = "EscenaCaso_03",
                colorTema = new Color(0.17f, 0.54f, 0.24f),
                colorBoton = new Color(0.32f, 0.81f, 0.4f)
            });
        }
    }

    public void MostrarCasoActual()
    {
        if (listaCasos == null || listaCasos.Count == 0) return;

        // Asegurar que el índice esté dentro del rango
        indiceCasoActual = Mathf.Clamp(indiceCasoActual, 0, listaCasos.Count - 1);
        ItemCasoMenu item = listaCasos[indiceCasoActual];

        // 1. Actualizar Tarjeta Central Activa
        if (txtBadgeCaso != null) txtBadgeCaso.text = string.IsNullOrEmpty(item.numeroCaso) ? $"CASO {indiceCasoActual + 1:D2}" : item.numeroCaso;

        if (item.datosCaso != null)
        {
            if (tituloCasoActivo != null) tituloCasoActivo.text = item.datosCaso.tituloCaso;
            if (descCasoActivo != null) descCasoActivo.text = item.datosCaso.descripcionCaso;
        }
        else
        {
            if (tituloCasoActivo != null) tituloCasoActivo.text = ObtenerTituloFallback(indiceCasoActual);
            if (descCasoActivo != null) descCasoActivo.text = ObtenerDescFallback(indiceCasoActual);
        }

        // Aplicar Colores del Tema Dinámico en la Tarjeta Activa
        if (badgeCasoActivo != null)
        {
            badgeCasoActivo.style.backgroundColor = item.colorTema;
            badgeCasoActivo.style.borderLeftColor = MultiplyColor(item.colorTema, 0.75f);
            badgeCasoActivo.style.borderRightColor = MultiplyColor(item.colorTema, 0.75f);
            badgeCasoActivo.style.borderTopColor = MultiplyColor(item.colorTema, 0.75f);
            badgeCasoActivo.style.borderBottomColor = MultiplyColor(item.colorTema, 0.75f);
        }

        if (btnIniciarCasoActivo != null)
        {
            btnIniciarCasoActivo.style.backgroundColor = item.colorBoton;
            btnIniciarCasoActivo.style.borderLeftColor = MultiplyColor(item.colorBoton, 0.7f);
            btnIniciarCasoActivo.style.borderRightColor = MultiplyColor(item.colorBoton, 0.7f);
            btnIniciarCasoActivo.style.borderTopColor = MultiplyColor(item.colorBoton, 0.7f);
            btnIniciarCasoActivo.style.borderBottomColor = MultiplyColor(item.colorBoton, 0.5f);

            btnIniciarCasoActivo.clickable = new Clickable(() => CargarEscenaCaso(item.nombreEscena));
        }

        // 2. Actualizar Previsualización Izquierda (Caso Anterior)
        if (cardCasoPrev != null)
        {
            if (indiceCasoActual > 0)
            {
                cardCasoPrev.RemoveFromClassList("oculta");
                ItemCasoMenu prevItem = listaCasos[indiceCasoActual - 1];
                if (txtBadgePrev != null) txtBadgePrev.text = prevItem.numeroCaso;
                if (tituloCasoPrev != null) tituloCasoPrev.text = prevItem.datosCaso != null ? prevItem.datosCaso.tituloCaso : ObtenerTituloFallback(indiceCasoActual - 1);
                if (badgeCasoPrev != null) badgeCasoPrev.style.backgroundColor = prevItem.colorTema;
            }
            else
            {
                cardCasoPrev.AddToClassList("oculta");
            }
        }

        // 3. Actualizar Previsualización Derecha (Caso Siguiente)
        if (cardCasoNext != null)
        {
            if (indiceCasoActual < listaCasos.Count - 1)
            {
                cardCasoNext.RemoveFromClassList("oculta");
                ItemCasoMenu nextItem = listaCasos[indiceCasoActual + 1];
                if (txtBadgeNext != null) txtBadgeNext.text = nextItem.numeroCaso;
                if (tituloCasoNext != null) tituloCasoNext.text = nextItem.datosCaso != null ? nextItem.datosCaso.tituloCaso : ObtenerTituloFallback(indiceCasoActual + 1);
                if (badgeCasoNext != null) badgeCasoNext.style.backgroundColor = nextItem.colorTema;
            }
            else
            {
                cardCasoNext.AddToClassList("oculta");
            }
        }

        // 4. Actualizar Contador y Puntos Indicadores
        if (txtContadorCaso != null)
        {
            txtContadorCaso.text = $"Caso {indiceCasoActual + 1} de {listaCasos.Count}";
        }

        GenerarIndicadoresPuntos();
        ActualizarEstadoFlechas();

        // Animación C# de apertura suave al cambiar de caso
        if (visorCasoActivo != null && gameObject.activeInHierarchy)
        {
            StopAllCoroutines();
            StartCoroutine(AnimarCambioCaso());
        }
    }

    private string ObtenerTituloFallback(int index)
    {
        switch (index)
        {
            case 0: return "El celular de Diego";
            case 1: return "El libro de historia";
            case 2: return "El balón de básquetbol";
            default: return $"Misterio del Caso #{index + 1}";
        }
    }

    private string ObtenerDescFallback(int index)
    {
        switch (index)
        {
            case 0: return "Durante el recreo, el celular de Diego desapareció del salón de química. Revisa los testimonios y analiza las pistas.";
            case 1: return "El valioso libro antiguo de la biblioteca ha sido extraviado. Interroga a los sospechosos y encuentra la verdad.";
            case 2: return "El balón oficial desapareció del depósito deportivo. Encuentra las pruebas clave antes de que termine el día.";
            default: return "Investiga los testimonios y recopila evidencias para resolver este nuevo misterio.";
        }
    }

    private System.Collections.IEnumerator AnimarCambioCaso()
    {
        if (visorCasoActivo == null) yield break;

        visorCasoActivo.style.scale = new Scale(new Vector3(0.95f, 0.95f, 1f));
        float t = 0f;
        while (t < 0.12f)
        {
            t += Time.deltaTime;
            float factor = Mathf.Lerp(0.95f, 1f, t / 0.12f);
            visorCasoActivo.style.scale = new Scale(new Vector3(factor, factor, 1f));
            yield return null;
        }
        visorCasoActivo.style.scale = new Scale(new Vector3(1f, 1f, 1f));
    }

    private void GenerarIndicadoresPuntos()
    {
        if (indicadoresPuntos == null) return;
        indicadoresPuntos.Clear();

        for (int i = 0; i < listaCasos.Count; i++)
        {
            VisualElement punto = new VisualElement();
            punto.AddToClassList("punto-indicador");
            if (i == indiceCasoActual)
            {
                punto.AddToClassList("activo");
                punto.style.backgroundColor = listaCasos[indiceCasoActual].colorBoton;
            }
            indicadoresPuntos.Add(punto);
        }
    }

    private void ActualizarEstadoFlechas()
    {
        if (btnAnteriorCaso != null)
        {
            if (indiceCasoActual == 0)
            {
                btnAnteriorCaso.AddToClassList("deshabilitado");
            }
            else
            {
                btnAnteriorCaso.RemoveFromClassList("deshabilitado");
            }
        }

        if (btnSiguienteCaso != null)
        {
            if (indiceCasoActual >= listaCasos.Count - 1)
            {
                btnSiguienteCaso.AddToClassList("deshabilitado");
            }
            else
            {
                btnSiguienteCaso.RemoveFromClassList("deshabilitado");
            }
        }
    }

    public void NavegarAnterior()
    {
        if (indiceCasoActual > 0)
        {
            indiceCasoActual--;
            MostrarCasoActual();
        }
    }

    public void NavegarSiguiente()
    {
        if (indiceCasoActual < listaCasos.Count - 1)
        {
            indiceCasoActual++;
            MostrarCasoActual();
        }
    }

    public void CargarEscenaCaso(string nombreEscena)
    {
        Debug.Log($"[ControladorMenuPrincipal] Cargando escena: {nombreEscena}");
        SceneManager.LoadScene(nombreEscena);
    }

    public void SalirDelJuego()
    {
        Debug.Log("[ControladorMenuPrincipal] Saliendo del juego...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private Color MultiplyColor(Color c, float factor)
    {
        return new Color(
            Mathf.Clamp01(c.r * factor),
            Mathf.Clamp01(c.g * factor),
            Mathf.Clamp01(c.b * factor),
            c.a
        );
    }
}
