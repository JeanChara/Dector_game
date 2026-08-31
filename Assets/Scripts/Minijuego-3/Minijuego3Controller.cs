using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class Minijuego3Controller : MonoBehaviour
{
    [Header("Datos del Caso")]
    public DatosCaso casoActual;

    [Header("Referencias a Paneles Externos para Navegación")]
    [SerializeField] private GameObject panelPantallaPrincipal;

    [Header("Estilos UI Toolkit")]
    [SerializeField] private StyleSheet estiloMinijuego;

    private UIDocument uiDocument;
    private VisualElement root;

    // Elementos UXML
    private Label textoCategoria;
    private Label textoTitulo;
    private Label textoDescripcion;

    private VisualElement pistometroFill;
    private Label pistometroCount;

    private VisualElement carpetaDropzone;
    private List<VisualElement> tarjetasPistas = new List<VisualElement>();

    private Button btnRegresar;
    private Button btnFinalizarCaso;
    private Label txtBtnFinalizar;
    private List<VisualElement> placasVida = new List<VisualElement>();

    private VisualElement toastNotification;
    private Label txtToast;

    private VisualElement modalResultado;
    private Label modalTitulo;
    private Label modalBody;
    private Button btnModalAccion;
    private Button btnCerrarModal;

    // Estado interno
    private const int TOTAL_PISTAS_REQUERIDAS = 3;
    private const int MAX_VIDAS = 2;
    private int vidas = 2;
    private int pistasEncontradas = 0;
    private bool juegoTerminado = false;
    private HashSet<int> tarjetasProcesadas = new HashSet<int>();
    private List<DatosPista> pistasMinijuego = new List<DatosPista>();
    private Coroutine corrutinaToast;

    // Estado de Drag & Drop
    private bool isDragging = false;
    private int activeCardIndex = -1;
    private Vector3 pointerStartPosition;

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

        if (estiloMinijuego != null && !root.styleSheets.Contains(estiloMinijuego))
        {
            root.styleSheets.Add(estiloMinijuego);
        }

        // 1. Obtener Elementos UXML
        textoCategoria = root.Q<Label>("textoCategoria");
        textoTitulo = root.Q<Label>("textoTitulo");
        textoDescripcion = root.Q<Label>("textoDescripcion");

        pistometroFill = root.Q<VisualElement>("pistometroFill");
        pistometroCount = root.Q<Label>("pistometroCount");

        carpetaDropzone = root.Q<VisualElement>("carpetaDropzone");

        tarjetasPistas.Clear();
        for (int i = 1; i <= 5; i++)
        {
            VisualElement t = root.Q<VisualElement>($"tarjeta-{i}");
            if (t != null)
            {
                tarjetasPistas.Add(t);
                int indexCard = i - 1; // 0..4

                // Configurar Eventos Pointer para Drag & Drop y Clic Nativos
                t.RegisterCallback<PointerDownEvent>(evt => OnPointerDownCard(evt, indexCard, t));
                t.RegisterCallback<PointerMoveEvent>(evt => OnPointerMoveCard(evt, indexCard, t));
                t.RegisterCallback<PointerUpEvent>(evt => OnPointerUpCard(evt, indexCard, t));
                t.RegisterCallback<PointerCaptureOutEvent>(evt => OnPointerCaptureOutCard(evt, t));
            }
        }

        Button btnLibro = root.Q<Button>("btnLibro");
        if (btnLibro != null)
        {
            btnLibro.clicked += () => {
                NavegacionPrincipal nav = FindAnyObjectByType<NavegacionPrincipal>();
                if (nav != null) nav.AlternarMenuEmergente();
            };
        }

        Button btnConfiguracion = root.Q<Button>("btnConfiguracion");
        if (btnConfiguracion != null)
        {
            btnConfiguracion.clicked += () => {
                NavegacionPrincipal nav = FindAnyObjectByType<NavegacionPrincipal>();
                if (nav != null) nav.AbrirConfiguracion();
            };
        }

        btnRegresar = root.Q<Button>("btnRegresar");
        if (btnRegresar != null)
        {
            btnRegresar.clicked += OnClicVolverATestimonios;
        }

        btnFinalizarCaso = root.Q<Button>("btnFinalizarCaso");
        if (btnFinalizarCaso != null)
        {
            btnFinalizarCaso.clicked += OnClicFinalizarCaso;
        }

        txtBtnFinalizar = root.Q<Label>("txtBtnFinalizar");

        placasVida.Clear();
        for (int i = 1; i <= 3; i++)
        {
            VisualElement v = root.Q<VisualElement>($"vida-{i}");
            if (v != null) placasVida.Add(v);
        }

        toastNotification = root.Q<VisualElement>("toastNotification");
        txtToast = root.Q<Label>("txtToast");

        modalResultado = root.Q<VisualElement>("modalResultado");
        modalTitulo = root.Q<Label>("modalTitulo");
        modalBody = root.Q<Label>("modalBody");

        btnModalAccion = root.Q<Button>("btnModalAccion");
        if (btnModalAccion != null)
        {
            btnModalAccion.clicked += OnClicModalAccion;
        }

        btnCerrarModal = root.Q<Button>("btnCerrarModal");
        if (btnCerrarModal != null)
        {
            btnCerrarModal.clicked += CerrarModal;
        }

        // 2. Cargar Textos de Reglas y Pistas de Minijuego #3
        if (casoActual != null)
        {
            if (textoCategoria != null && !string.IsNullOrEmpty(casoActual.tituloMinijuego)) textoCategoria.text = casoActual.tituloMinijuego;
            if (textoTitulo != null && !string.IsNullOrEmpty(casoActual.nombreMinijuego)) textoTitulo.text = casoActual.nombreMinijuego;
            if (textoDescripcion != null && !string.IsNullOrEmpty(casoActual.descripcionMinijuego)) textoDescripcion.text = casoActual.descripcionMinijuego;

            VisualElement fondoEscuela = root.Q<VisualElement>("fondoEscuela");
            if (fondoEscuela != null && casoActual.imagenFondo != null)
            {
                fondoEscuela.style.backgroundImage = new StyleBackground(casoActual.imagenFondo);
            }
        }

        // Cargar pistas directamente en el archivo del minijuego
        pistasMinijuego = ObtenerPistasDelCaso();
        for (int i = 0; i < tarjetasPistas.Count && i < pistasMinijuego.Count; i++)
        {
            Label lbl = tarjetasPistas[i].Q<Label>($"txt-tarjeta-{i + 1}");
            if (lbl != null)
            {
                lbl.text = pistasMinijuego[i].textoPista;
            }
        }

        ReiniciarJuego();
    }

    // -------------------------------------------------------------------
    // Lógica de Drag & Drop y Efectos C#
    // -------------------------------------------------------------------
    private void OnPointerDownCard(PointerDownEvent evt, int indexCard, VisualElement card)
    {
        if (juegoTerminado || tarjetasProcesadas.Contains(indexCard)) return;

        isDragging = true;
        activeCardIndex = indexCard;
        pointerStartPosition = evt.position;

        card.style.scale = new Scale(new Vector3(1.07f, 1.07f, 1f));
        card.style.opacity = 0.88f;

        card.CapturePointer(evt.pointerId);
    }

    private void OnPointerMoveCard(PointerMoveEvent evt, int indexCard, VisualElement card)
    {
        if (!isDragging || activeCardIndex != indexCard || !card.HasPointerCapture(evt.pointerId)) return;

        Vector3 delta = evt.position - pointerStartPosition;
        card.style.translate = new Translate(delta.x, delta.y, 0);

        // Resaltar Carpeta de Evidencias en Verde al sobrevolarla (Detección por centro de tarjeta o puntero)
        VisualElement areaImpacto = (carpetaDropzone != null) ? (carpetaDropzone.Q<VisualElement>(className: "icono-carpeta") ?? carpetaDropzone) : null;
        if (areaImpacto != null)
        {
            Rect carpetaBounds = areaImpacto.worldBound;
            Vector2 cardCenter = card.worldBound.center;
            if (carpetaBounds.Contains(evt.position) || carpetaBounds.Contains(cardCenter))
            {
                carpetaDropzone.AddToClassList("dropzone-activa");
            }
            else
            {
                RestablecerEstiloCarpeta();
            }
        }
    }

    private void OnPointerUpCard(PointerUpEvent evt, int indexCard, VisualElement card)
    {
        if (!isDragging || activeCardIndex != indexCard) return;

        isDragging = false;
        activeCardIndex = -1;
        if (card.HasPointerCapture(evt.pointerId))
        {
            card.ReleasePointer(evt.pointerId);
        }

        RestablecerEstiloCarpeta();

        card.style.scale = new Scale(new Vector3(1f, 1f, 1f));
        card.style.opacity = 1f;

        Vector3 delta = evt.position - pointerStartPosition;
        bool droppedOnCarpeta = false;

        VisualElement areaImpacto = (carpetaDropzone != null) ? (carpetaDropzone.Q<VisualElement>(className: "icono-carpeta") ?? carpetaDropzone) : null;
        if (areaImpacto != null)
        {
            Rect carpetaBounds = areaImpacto.worldBound;
            Vector2 cardCenter = card.worldBound.center;
            if (carpetaBounds.Contains(evt.position) || carpetaBounds.Contains(cardCenter))
            {
                droppedOnCarpeta = true;
            }
        }

        // Restablecer posición visual de la tarjeta
        card.style.translate = new Translate(0, 0, 0);

        // Evaluar ÚNICAMENTE si fue soltada dentro de la Carpeta de Evidencias
        if (droppedOnCarpeta)
        {
            ProcesarSeleccionPista(indexCard);
        }
        else if (delta.magnitude < 12f)
        {
            MostrarToast("Arrastra la tarjeta hacia la Carpeta de Evidencias", false);
        }
    }

    private void OnPointerCaptureOutCard(PointerCaptureOutEvent evt, VisualElement card)
    {
        if (isDragging)
        {
            isDragging = false;
            activeCardIndex = -1;
            RestablecerEstiloCarpeta();
            card.style.scale = new Scale(new Vector3(1f, 1f, 1f));
            card.style.opacity = 1f;
            card.style.translate = new Translate(0, 0, 0);
        }
    }

    private void RestablecerEstiloCarpeta()
    {
        if (carpetaDropzone == null) return;
        carpetaDropzone.RemoveFromClassList("dropzone-activa");
    }

    // -------------------------------------------------------------------
    // Lógica Principal de Selección y Animación C# de Error Shake
    // -------------------------------------------------------------------
    private void ProcesarSeleccionPista(int indexCard)
    {
        if (juegoTerminado || tarjetasProcesadas.Contains(indexCard)) return;

        bool esCorrecta = false;
        if (pistasMinijuego != null && indexCard < pistasMinijuego.Count)
        {
            esCorrecta = pistasMinijuego[indexCard].esCorrecta;
        }

        VisualElement tarjeta = tarjetasPistas[indexCard];

        if (esCorrecta)
        {
            // ACIERTO
            tarjetasProcesadas.Add(indexCard);
            tarjeta.AddToClassList("correcta-marcada");

            VisualElement sello = tarjeta.Q<VisualElement>($"sello-{indexCard + 1}");
            if (sello != null) sello.RemoveFromClassList("oculta");

            pistasEncontradas++;
            ActualizarPistometro();
            MostrarToast("¡Pista agregada a la investigación!", true);

            if (pistasEncontradas >= TOTAL_PISTAS_REQUERIDAS)
            {
                juegoTerminado = true;
                BloquearTarjetasRestantes();
                ActivarBotonFinalizar();
            }
        }
        else
        {
            // ERROR
            tarjetasProcesadas.Add(indexCard);
            tarjeta.AddToClassList("incorrecta-marcada");

            // Animación C# de Temblor (Shake)
            StartCoroutine(AnimarShakeTarjeta(tarjeta));

            vidas--;
            ActualizarVidas();
            MostrarToast("Esta información no ayuda al caso", false);

            if (vidas <= 0)
            {
                juegoTerminado = true;
                BloquearTarjetasRestantes();
                Invoke(nameof(MostrarModalDerrota), 0.6f);
            }
        }
    }

    private IEnumerator AnimarShakeTarjeta(VisualElement tarjeta)
    {
        float[] offsets = new float[] { -10f, 10f, -8f, 8f, -4f, 4f, 0f };
        foreach (float offset in offsets)
        {
            tarjeta.style.translate = new Translate(offset, 0, 0);
            yield return new WaitForSeconds(0.04f);
        }
        tarjeta.style.translate = new Translate(0, 0, 0);
    }

    private void ActualizarPistometro()
    {
        float porcentaje = ((float)pistasEncontradas / TOTAL_PISTAS_REQUERIDAS) * 100f;
        if (pistometroFill != null) pistometroFill.style.width = Length.Percent(porcentaje);
        if (pistometroCount != null) pistometroCount.text = $"{pistasEncontradas}/{TOTAL_PISTAS_REQUERIDAS}";
    }

    private void ActualizarVidas()
    {
        for (int i = 0; i < placasVida.Count; i++)
        {
            if (placasVida[i] != null)
            {
                if (i >= MAX_VIDAS)
                {
                    placasVida[i].style.display = DisplayStyle.None;
                }
                else if (i < vidas)
                {
                    placasVida[i].style.display = DisplayStyle.Flex;
                    placasVida[i].RemoveFromClassList("rota");
                    placasVida[i].AddToClassList("activa");
                }
                else
                {
                    placasVida[i].style.display = DisplayStyle.Flex;
                    placasVida[i].RemoveFromClassList("activa");
                    placasVida[i].AddToClassList("rota");
                }
            }
        }
    }

    private void BloquearTarjetasRestantes()
    {
        for (int i = 0; i < tarjetasPistas.Count; i++)
        {
            if (!tarjetasProcesadas.Contains(i))
            {
                tarjetasPistas[i].AddToClassList("desactivada");
            }
        }
    }

    private void ActivarBotonFinalizar()
    {
        if (btnFinalizarCaso != null)
        {
            btnFinalizarCaso.RemoveFromClassList("deshabilitado");
        }
        if (txtBtnFinalizar != null)
        {
            txtBtnFinalizar.text = "Ver Resolución del Caso";
        }
        MostrarToast("Caso completado. Haz clic abajo para ver el resumen.", true);
    }

    private void OnClicFinalizarCaso()
    {
        if (pistasEncontradas >= TOTAL_PISTAS_REQUERIDAS)
        {
            MostrarModalVictoria();
        }
    }

    private void OnClicVolverATestimonios()
    {
        gameObject.SetActive(false);
        if (panelPantallaPrincipal != null)
        {
            panelPantallaPrincipal.SetActive(true);
        }
    }

    public void MostrarToast(string mensaje, bool esExito)
    {
        if (toastNotification == null || txtToast == null) return;

        txtToast.text = mensaje;
        toastNotification.RemoveFromClassList("oculta");

        if (corrutinaToast != null) StopCoroutine(corrutinaToast);
        corrutinaToast = StartCoroutine(OcultarToastDespuesDeTiempo(2.5f));
    }

    private IEnumerator OcultarToastDespuesDeTiempo(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        if (toastNotification != null) toastNotification.AddToClassList("oculta");
    }

    private void MostrarModalVictoria()
    {
        if (modalResultado == null) return;

        if (btnCerrarModal != null) btnCerrarModal.RemoveFromClassList("oculta");
        if (modalTitulo != null) modalTitulo.text = "Pistas Seleccionadas del Caso";
        if (modalBody != null)
        {
            if (casoActual != null && !string.IsNullOrEmpty(casoActual.textoVictoriaResumen))
            {
                modalBody.text = casoActual.textoVictoriaResumen;
            }
            else
            {
                string titulo = (casoActual != null && !string.IsNullOrEmpty(casoActual.tituloCaso)) ? casoActual.tituloCaso : "";
                if (titulo.Contains("2"))
                {
                    modalBody.text = "¡Excelente trabajo de investigación!\n\n" +
                                     "Estas 3 pistas permitieron resolver el caso:\n" +
                                     "• El broche de la jaula fue levantado manualmente.\n" +
                                     "• Camila tenía pelos cafés de hámster en su sudadera.\n" +
                                     "• Camila llevaba semillas de girasol en sus bolsillos.";
                }
                else
                {
                    modalBody.text = "¡Excelente trabajo de investigación!\n\n" +
                                     "Estas 3 pistas permitieron resolver el caso:\n" +
                                     "• La puerta del laboratorio estaba abierta cuando Diego regresó.\n" +
                                     "• El profesor encontró la ventana abierta.\n" +
                                     "• Había una mochila verde sobre una silla.";
                }
            }
        }

        if (btnModalAccion != null) btnModalAccion.text = "Entendido";
        modalResultado.RemoveFromClassList("oculta");
    }

    private void MostrarModalDerrota()
    {
        if (modalResultado == null) return;

        if (btnCerrarModal != null) btnCerrarModal.AddToClassList("oculta");
        if (modalTitulo != null) modalTitulo.text = "Has agotado tus vidas";
        if (modalBody != null)
        {
            modalBody.text = "Inténtalo de nuevo\n\n" +
                             $"Seleccionaste información que no ayudaba al caso y perdiste tus {MAX_VIDAS} insignias.\n\n" +
                             "Analiza con cuidado los testimonios antes de volver a seleccionar.";
        }

        if (btnModalAccion != null) btnModalAccion.text = "Reintentar Caso";
        modalResultado.RemoveFromClassList("oculta");
    }

    private void OnClicModalAccion()
    {
        if (modalResultado != null) modalResultado.AddToClassList("oculta");

        if (vidas <= 0)
        {
            ReiniciarJuego();
        }
    }

    public void CerrarModal()
    {
        if (modalResultado != null && pistasEncontradas >= TOTAL_PISTAS_REQUERIDAS)
        {
            modalResultado.AddToClassList("oculta");
        }
    }

    public void ReiniciarJuego()
    {
        vidas = MAX_VIDAS;
        pistasEncontradas = 0;
        juegoTerminado = false;
        tarjetasProcesadas.Clear();

        ActualizarVidas();
        ActualizarPistometro();

        for (int i = 0; i < tarjetasPistas.Count; i++)
        {
            VisualElement t = tarjetasPistas[i];
            if (t != null)
            {
                t.RemoveFromClassList("correcta-marcada");
                t.RemoveFromClassList("incorrecta-marcada");
                t.RemoveFromClassList("desactivada");
                t.style.scale = new Scale(new Vector3(1f, 1f, 1f));
                t.style.opacity = 1f;
                t.style.translate = new Translate(0, 0, 0);

                VisualElement sello = t.Q<VisualElement>($"sello-{i + 1}");
                if (sello != null) sello.AddToClassList("oculta");
            }
        }

        if (btnFinalizarCaso != null) btnFinalizarCaso.AddToClassList("deshabilitado");
        if (txtBtnFinalizar != null) txtBtnFinalizar.text = "Encuentra las 3 pistas";

        if (toastNotification != null) toastNotification.AddToClassList("oculta");
        if (modalResultado != null) modalResultado.AddToClassList("oculta");
    }

    private List<DatosPista> ObtenerPistasDelCaso()
    {
        if (casoActual != null && casoActual.pistasMinijuego3 != null && casoActual.pistasMinijuego3.Count > 0)
        {
            return casoActual.pistasMinijuego3;
        }

        string titulo = (casoActual != null && !string.IsNullOrEmpty(casoActual.tituloCaso)) ? casoActual.tituloCaso : "";

        if (titulo.Contains("2"))
        {
            return new List<DatosPista>
            {
                new DatosPista { idPista = "1", textoPista = "El broche de metal de la jaula estaba levantado hacia arriba", esCorrecta = true },
                new DatosPista { idPista = "2", textoPista = "Camila tenía pelos cafés de hámster en la manga derecha de su sudadera", esCorrecta = true },
                new DatosPista { idPista = "3", textoPista = "Había una maqueta de un volcán de plastilina en la mesa principal", esCorrecta = false },
                new DatosPista { idPista = "4", textoPista = "Camila llevaba semillas de girasol en los bolsillos de su sudadera oversize", esCorrecta = true },
                new DatosPista { idPista = "5", textoPista = "Los audífonos de casco de Tomás son de color negro", esCorrecta = false }
            };
        }
        else if (titulo.Contains("3"))
        {
            return new List<DatosPista>
            {
                new DatosPista { idPista = "1", textoPista = "El candado del deposito de deportes estaba forzado", esCorrecta = true },
                new DatosPista { idPista = "2", textoPista = "Se escucharon botes de balon cerca de los vestidores", esCorrecta = true },
                new DatosPista { idPista = "3", textoPista = "Habia un silbato olvidado sobre las bancas", esCorrecta = false },
                new DatosPista { idPista = "4", textoPista = "Se encontro una huella de zapatilla con barro cerca del deposito", esCorrecta = true },
                new DatosPista { idPista = "5", textoPista = "El reloj de la cancha marcaba las 10:30 AM", esCorrecta = false }
            };
        }
        else
        {
            // Por defecto Caso 1
            return new List<DatosPista>
            {
                new DatosPista { idPista = "1", textoPista = "La puerta del laboratorio estaba abierta cuando Diego regresó", esCorrecta = true },
                new DatosPista { idPista = "2", textoPista = "El profesor encontró la ventana abierta", esCorrecta = true },
                new DatosPista { idPista = "3", textoPista = "Había una tarea sobre la mesa", esCorrecta = false },
                new DatosPista { idPista = "4", textoPista = "Había una mochila verde sobre una silla", esCorrecta = true },
                new DatosPista { idPista = "5", textoPista = "Diego jugó fútbol durante el recreo", esCorrecta = false }
            };
        }
    }
}
