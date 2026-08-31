using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class Minijuego4Controller : MonoBehaviour
{
    [Header("Datos del Caso")]
    public DatosCaso casoActual;

    [Header("Estilos UI Toolkit")]
    [SerializeField] private StyleSheet estiloMinijuego;

    private UIDocument uiDocument;
    private VisualElement root;

    // Elementos UXML
    private Label textoCategoria;
    private Label textoTitulo;
    private Label textoDescripcion;

    private Label badgeNivel;
    private Label personaNombre;
    private List<Label> pasoDots = new List<Label>();
    private Label tituloPregunta;

    private VisualElement papeleraDropzone;
    private List<VisualElement> postitsList = new List<VisualElement>();

    private Button btnRegresar;
    private Label txtBtnFinalizar;
    private List<VisualElement> placasVida = new List<VisualElement>();

    private VisualElement toastNotification;
    private Label txtToast;

    private VisualElement modalResultado;
    private Label modalTitulo;
    private Label modalBody;
    private Button btnModalAccion;
    private Button btnCerrarModal;

    // Estado del Juego
    private const int MAX_VIDAS = 2;
    private int vidas = 2;
    private int nivelActualIndex = 0;
    private bool juegoTerminado = false;
    private bool procesandoSeleccion = false;
    private HashSet<int> postitsProcesados = new HashSet<int>();
    private Coroutine corrutinaToast;

    // Estado de Drag & Drop
    private bool isDragging = false;
    private int activePostitIndex = -1;
    private Vector3 pointerStartPosition;

    // Lista de Niveles
    private List<NivelContradiccionData> niveles = new List<NivelContradiccionData>();

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

        // 1. Obtener Referencias UXML
        textoCategoria = root.Q<Label>("textoCategoria");
        textoTitulo = root.Q<Label>("textoTitulo");
        textoDescripcion = root.Q<Label>("textoDescripcion");

        badgeNivel = root.Q<Label>("badgeNivel");
        personaNombre = root.Q<Label>("personaNombre");
        tituloPregunta = root.Q<Label>("tituloPregunta");

        pasoDots.Clear();
        for (int i = 1; i <= 3; i++)
        {
            Label dot = root.Q<Label>($"paso-{i}");
            if (dot != null) pasoDots.Add(dot);
        }

        papeleraDropzone = root.Q<VisualElement>("papeleraDropzone");

        postitsList.Clear();
        for (int i = 1; i <= 4; i++)
        {
            VisualElement p = root.Q<VisualElement>($"postit-{i}");
            if (p != null)
            {
                postitsList.Add(p);
                int indexCard = i - 1;

                p.RegisterCallback<PointerDownEvent>(evt => OnPointerDownPostit(evt, indexCard, p));
                p.RegisterCallback<PointerMoveEvent>(evt => OnPointerMovePostit(evt, indexCard, p));
                p.RegisterCallback<PointerUpEvent>(evt => OnPointerUpPostit(evt, indexCard, p));
                p.RegisterCallback<PointerCaptureOutEvent>(evt => OnPointerCaptureOutPostit(evt, p));
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

        // Cargar Textos de Reglas y Datos del Minijuego #4
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

        // Cargar Lista de Niveles
        CargarNiveles();

        // Reiniciar Juego
        ReiniciarJuego();
    }

    private void CargarNiveles()
    {
        niveles.Clear();
        if (casoActual != null && casoActual.nivelesMinijuego4 != null && casoActual.nivelesMinijuego4.Count > 0)
        {
            niveles.AddRange(casoActual.nivelesMinijuego4);
            return;
        }

        string titulo = (casoActual != null && !string.IsNullOrEmpty(casoActual.tituloCaso)) ? casoActual.tituloCaso : "";

        if (titulo.Contains("2"))
        {
            // NIVELES DEL CASO 2 (El libro de historia)
            niveles.Add(new NivelContradiccionData
            {
                nivel = 1, persona = "Diego Ramírez",
                pregunta = "Arrastra la nota con la contradicción de Diego Ramírez a la Papelera",
                opciones = new List<OpcionNotaPostit>
                {
                    new OpcionNotaPostit { id = 1, texto = "Diego estaba investigando para el examen en la biblioteca.", esContradiccion = false },
                    new OpcionNotaPostit { id = 2, texto = "Diego firmó la tarjeta de préstamo oficial antes de irse.", esContradiccion = true, explicacion = "El registro de préstamos no tenía firma requerida." },
                    new OpcionNotaPostit { id = 3, texto = "Dejó el libro sobre el escritorio un momento al buscar unas hojas.", esContradiccion = false },
                    new OpcionNotaPostit { id = 4, texto = "Al regresar a la mesa, el libro ya no estaba.", esContradiccion = false }
                }
            });
            niveles.Add(new NivelContradiccionData
            {
                nivel = 2, persona = "Laura Sánchez",
                pregunta = "Arrastra la nota con la contradicción de Laura Sánchez a la Papelera",
                opciones = new List<OpcionNotaPostit>
                {
                    new OpcionNotaPostit { id = 1, texto = "Laura vio a alguien salir con prisa de la biblioteca.", esContradiccion = false },
                    new OpcionNotaPostit { id = 2, texto = "Laura pasó por el corredor cerca del estante de historia.", esContradiccion = false },
                    new OpcionNotaPostit { id = 3, texto = "Notó una marca de tiza cerca del estante.", esContradiccion = false },
                    new OpcionNotaPostit { id = 4, texto = "Laura dijo que la puerta principal del colegio estaba abierta de par en par.", esContradiccion = true, explicacion = "La puerta principal estaba cerrada." }
                }
            });
            niveles.Add(new NivelContradiccionData
            {
                nivel = 3, persona = "Caso General (Final)",
                pregunta = "Arrastra la nota con la gran CONTRADICCIÓN del caso a la Papelera",
                opciones = new List<OpcionNotaPostit>
                {
                    new OpcionNotaPostit { id = 1, texto = "El mapa de la pared mostraba la ubicación del libro robado.", esContradiccion = true, explicacion = "El mapa era decorativo y no tenía relación con el libro." },
                    new OpcionNotaPostit { id = 2, texto = "Encontraron una llave doblada junto a la mesa principal.", esContradiccion = false },
                    new OpcionNotaPostit { id = 3, texto = "Había un mapa de la biblioteca colgado en la pared.", esContradiccion = false },
                    new OpcionNotaPostit { id = 4, texto = "El registro de la biblioteca no tenía la firma requerida.", esContradiccion = false }
                }
            });
        }
        else if (titulo.Contains("3"))
        {
            // NIVELES DEL CASO 3 (El balón de básquetbol)
            niveles.Add(new NivelContradiccionData
            {
                nivel = 1, persona = "Diego Ramírez",
                pregunta = "Arrastra la nota con la contradicción de Diego Ramírez a la Papelera",
                opciones = new List<OpcionNotaPostit>
                {
                    new OpcionNotaPostit { id = 1, texto = "El balón oficial desapareció del depósito deportivo.", esContradiccion = false },
                    new OpcionNotaPostit { id = 2, texto = "Diego fue a sacar los balones al depósito en la hora de deportes.", esContradiccion = false },
                    new OpcionNotaPostit { id = 3, texto = "La puerta del depósito estaba abierta al llegar.", esContradiccion = false },
                    new OpcionNotaPostit { id = 4, texto = "Diego abrió el depósito usando su propia llave dorada.", esContradiccion = true, explicacion = "El candado del depósito fue forzado." }
                }
            });
            niveles.Add(new NivelContradiccionData
            {
                nivel = 2, persona = "Laura Sánchez",
                pregunta = "Arrastra la nota con la contradicción de Laura Sánchez a la Papelera",
                opciones = new List<OpcionNotaPostit>
                {
                    new OpcionNotaPostit { id = 1, texto = "Escuchó botes de balón cerca del área de vestidores.", esContradiccion = false },
                    new OpcionNotaPostit { id = 2, texto = "Laura vio a Diego jugando básquetbol solo en la cancha.", esContradiccion = true, explicacion = "El balón había desaparecido del depósito." },
                    new OpcionNotaPostit { id = 3, texto = "Pasó cerca del depósito durante la clase.", esContradiccion = false },
                    new OpcionNotaPostit { id = 4, texto = "Vio una huella de zapatilla con barro en el suelo.", esContradiccion = false }
                }
            });
            niveles.Add(new NivelContradiccionData
            {
                nivel = 3, persona = "Caso General (Final)",
                pregunta = "Arrastra la nota con la gran CONTRADICCIÓN del caso a la Papelera",
                opciones = new List<OpcionNotaPostit>
                {
                    new OpcionNotaPostit { id = 1, texto = "El candado del depósito deportivo había sido forzado.", esContradiccion = false },
                    new OpcionNotaPostit { id = 2, texto = "Se encontró una huella con barro cerca del depósito.", esContradiccion = false },
                    new OpcionNotaPostit { id = 3, texto = "El silbato olvidado en la banca fue usado para sonar la alarma del robo.", esContradiccion = true, explicacion = "El silbato fue una distracción olvidada que no se usó." },
                    new OpcionNotaPostit { id = 4, texto = "Se escucharon botes de balón cerca de los vestidores.", esContradiccion = false }
                }
            });
        }
        else
        {
            // NIVELES DEL CASO 1 (El celular de Diego)
            niveles.Add(new NivelContradiccionData
            {
                nivel = 1, persona = "Diego Ramírez",
                pregunta = "Arrastra la nota con la contradicción de Diego Ramírez a la Papelera",
                opciones = new List<OpcionNotaPostit>
                {
                    new OpcionNotaPostit { id = 1, texto = "Diego dejó su celular sobre su mesa antes de salir al recreo.", esContradiccion = false },
                    new OpcionNotaPostit { id = 2, texto = "Cuando regresó al salón, su celular ya no estaba en la mesa.", esContradiccion = false },
                    new OpcionNotaPostit { id = 3, texto = "Diego guardó su celular dentro de la mochila verde antes de salir.", esContradiccion = true, explicacion = "Diego dejó su celular sobre su mesa, no en la mochila verde." },
                    new OpcionNotaPostit { id = 4, texto = "Diego estuvo buscando su celular muy preocupado al volver.", esContradiccion = false }
                }
            });
            niveles.Add(new NivelContradiccionData
            {
                nivel = 2, persona = "Laura Sánchez",
                pregunta = "Arrastra la nota con la contradicción de Laura Sánchez a la Papelera",
                opciones = new List<OpcionNotaPostit>
                {
                    new OpcionNotaPostit { id = 1, texto = "Laura vio que Diego regaló su celular a un compañero.", esContradiccion = true, explicacion = "Laura declaró que vio a alguien entrar al salón, no que regalara el celular." },
                    new OpcionNotaPostit { id = 2, texto = "Laura iba caminando hacia el patio durante el recreo.", esContradiccion = false },
                    new OpcionNotaPostit { id = 3, texto = "Laura vio a un estudiante entrar rápidamente al salón.", esContradiccion = false },
                    new OpcionNotaPostit { id = 4, texto = "Laura escuchó después que Diego estaba buscando su celular.", esContradiccion = false }
                }
            });
            niveles.Add(new NivelContradiccionData
            {
                nivel = 3, persona = "Carlos Mendoza (CULPABLE)",
                pregunta = "Arrastra la nota con la contradicción delatadora de Carlos Mendoza a la Papelera",
                opciones = new List<OpcionNotaPostit>
                {
                    new OpcionNotaPostit { id = 1, texto = "Carlos estuvo cerca del salón con su gorra roja puesta al revés.", esContradiccion = false },
                    new OpcionNotaPostit { id = 2, texto = "La mochila verde de Carlos fue encontrada sobre una silla del salón.", esContradiccion = false },
                    new OpcionNotaPostit { id = 3, texto = "Carlos afirma que estuvo jugando básquetbol todo el receso y no tiene ninguna mochila verde.", esContradiccion = true, explicacion = "¡Contradicción delatadora! Laura lo vio entrar con su gorra roja y su mochila verde quedó en la silla del salón." },
                    new OpcionNotaPostit { id = 4, texto = "Carlos estuvo rondando por los pasillos cerca del laboratorio.", esContradiccion = false }
                }
            });
        }
    }

    private void ActualizarNivelUI()
    {
        if (niveles == null || nivelActualIndex >= niveles.Count) return;
        NivelContradiccionData datosNivel = niveles[nivelActualIndex];

        if (badgeNivel != null) badgeNivel.text = $"Nivel {datosNivel.nivel} de {niveles.Count}";
        if (personaNombre != null) personaNombre.text = $"Analizando a: {datosNivel.persona}";
        if (tituloPregunta != null) tituloPregunta.text = datosNivel.pregunta;
        if (txtBtnFinalizar != null) txtBtnFinalizar.text = $"Arrastra la nota falsa a la papelera (Nivel {datosNivel.nivel})";

        // Actualizar Dots de Pasos
        for (int i = 0; i < pasoDots.Count; i++)
        {
            pasoDots[i].RemoveFromClassList("activo");
            pasoDots[i].RemoveFromClassList("completado");

            if (i < nivelActualIndex)
            {
                pasoDots[i].AddToClassList("completado");
            }
            else if (i == nivelActualIndex)
            {
                pasoDots[i].AddToClassList("activo");
            }
        }

        // Renderizar Notas Post-it
        postitsProcesados.Clear();
        for (int i = 0; i < postitsList.Count && i < datosNivel.opciones.Count; i++)
        {
            VisualElement p = postitsList[i];
            OpcionNotaPostit opt = datosNivel.opciones[i];

            Label lbl = p.Q<Label>($"txt-postit-{i + 1}");
            if (lbl != null) lbl.text = opt.texto;

            Label sello = p.Q<Label>($"sello-{i + 1}");
            if (sello != null) sello.AddToClassList("oculta");

            p.RemoveFromClassList("correcta-marcada");
            p.RemoveFromClassList("incorrecta-marcada");
            p.style.translate = new Translate(0, 0, 0);
            p.style.scale = new Scale(new Vector3(1f, 1f, 1f));
            p.style.opacity = 1f;
        }
    }

    // -------------------------------------------------------------------
    // Lógica Drag & Drop y Pointer Events
    // -------------------------------------------------------------------
    private void OnPointerDownPostit(PointerDownEvent evt, int indexCard, VisualElement postit)
    {
        if (juegoTerminado || procesandoSeleccion || postitsProcesados.Contains(indexCard)) return;

        isDragging = true;
        activePostitIndex = indexCard;
        pointerStartPosition = evt.position;

        postit.style.scale = new Scale(new Vector3(1.06f, 1.06f, 1f));
        postit.style.opacity = 0.88f;

        postit.CapturePointer(evt.pointerId);
    }

    private void OnPointerMovePostit(PointerMoveEvent evt, int indexCard, VisualElement postit)
    {
        if (!isDragging || activePostitIndex != indexCard || !postit.HasPointerCapture(evt.pointerId)) return;

        Vector3 delta = evt.position - pointerStartPosition;
        postit.style.translate = new Translate(delta.x, delta.y, 0);

        VisualElement areaImpacto = (papeleraDropzone != null) ? (papeleraDropzone.Q<VisualElement>(className: "icono-papelera") ?? papeleraDropzone) : null;
        if (areaImpacto != null)
        {
            Rect dropBounds = areaImpacto.worldBound;
            if (dropBounds.Contains(evt.position))
            {
                papeleraDropzone.AddToClassList("dropzone-activa");
            }
            else
            {
                papeleraDropzone.RemoveFromClassList("dropzone-activa");
            }
        }
    }

    private void OnPointerUpPostit(PointerUpEvent evt, int indexCard, VisualElement postit)
    {
        if (!isDragging || activePostitIndex != indexCard) return;

        isDragging = false;
        activePostitIndex = -1;

        if (postit.HasPointerCapture(evt.pointerId))
        {
            postit.ReleasePointer(evt.pointerId);
        }

        postit.style.scale = new Scale(new Vector3(1f, 1f, 1f));
        postit.style.opacity = 1f;

        if (papeleraDropzone != null) papeleraDropzone.RemoveFromClassList("dropzone-activa");

        // Evaluar ÚNICAMENTE si la nota fue arrastrada y soltada sobre la imagen del Bote de Basura
        VisualElement areaImpacto = (papeleraDropzone != null) ? (papeleraDropzone.Q<VisualElement>(className: "icono-papelera") ?? papeleraDropzone) : null;
        bool fueEnPapelera = false;
        if (areaImpacto != null)
        {
            Rect dropBounds = areaImpacto.worldBound;
            fueEnPapelera = dropBounds.Contains(evt.position);
        }

        if (fueEnPapelera)
        {
            ProcesarSeleccionPostit(indexCard, postit);
        }
        else
        {
            postit.style.translate = new Translate(0, 0, 0);
        }
    }

    private void OnPointerCaptureOutPostit(PointerCaptureOutEvent evt, VisualElement postit)
    {
        if (isDragging)
        {
            isDragging = false;
            activePostitIndex = -1;

            postit.style.scale = new Scale(new Vector3(1f, 1f, 1f));
            postit.style.opacity = 1f;
            postit.style.translate = new Translate(0, 0, 0);
            if (papeleraDropzone != null) papeleraDropzone.RemoveFromClassList("dropzone-activa");
        }
    }

    private void ProcesarSeleccionPostit(int indexCard, VisualElement postit)
    {
        if (juegoTerminado || procesandoSeleccion || postitsProcesados.Contains(indexCard)) return;

        procesandoSeleccion = true;
        NivelContradiccionData datosNivel = niveles[nivelActualIndex];
        OpcionNotaPostit opt = datosNivel.opciones[indexCard];

        if (opt.esContradiccion)
        {
            // ACIERTO (CONTRADICCIÓN ENCONTRADA)
            postitsProcesados.Add(indexCard);
            postit.AddToClassList("correcta-marcada");

            Label sello = postit.Q<Label>($"sello-{indexCard + 1}");
            if (sello != null)
            {
                sello.text = "✓ CONTRADICCIÓN";
                sello.RemoveFromClassList("oculta");
                sello.RemoveFromClassList("sello-error");
                sello.AddToClassList("sello-exito");
            }

            MostrarToast($"¡Excelente! Descartaste la nota falsa de {datosNivel.persona}.", true);

            StartCoroutine(AvanzarNivelConRetraso());
        }
        else
        {
            // ERROR (INFORMACIÓN REAL)
            postitsProcesados.Add(indexCard);
            postit.AddToClassList("incorrecta-marcada");

            Label sello = postit.Q<Label>($"sello-{indexCard + 1}");
            if (sello != null)
            {
                sello.text = "✗ INFORMACIÓN REAL";
                sello.RemoveFromClassList("oculta");
                sello.RemoveFromClassList("sello-exito");
                sello.AddToClassList("sello-error");
            }

            StartCoroutine(AnimarShakePostit(postit));

            vidas--;
            ActualizarVidas();

            if (vidas > 0)
            {
                MostrarToast($"Esta nota sí coincide. Te quedan {vidas} vidas.", false);
                Invoke(nameof(HabilitarSeleccion), 0.6f);
            }
            else
            {
                juegoTerminado = true;
                Invoke(nameof(MostrarModalDerrota), 0.8f);
            }
        }
    }

    private IEnumerator AvanzarNivelConRetraso()
    {
        yield return new WaitForSeconds(1.1f);
        procesandoSeleccion = false;

        if (nivelActualIndex < niveles.Count - 1)
        {
            nivelActualIndex++;
            ActualizarNivelUI();
            MostrarToast($"Avanzaste al Nivel {nivelActualIndex + 1}: {niveles[nivelActualIndex].persona}", true);
        }
        else
        {
            juegoTerminado = true;
            MostrarModalVictoria();
        }
    }

    private void HabilitarSeleccion()
    {
        procesandoSeleccion = false;
    }

    private IEnumerator AnimarShakePostit(VisualElement postit)
    {
        float[] offsets = new float[] { -8f, 8f, -6f, 6f, -3f, 3f, 0f };
        foreach (float offset in offsets)
        {
            postit.style.translate = new Translate(offset, 0, 0);
            yield return new WaitForSeconds(0.04f);
        }
        postit.style.translate = new Translate(0, 0, 0);
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

    private void MostrarToast(string mensaje, bool esExito)
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
        if (modalTitulo != null) modalTitulo.text = "¡Felicidades, Detective!";
        if (modalBody != null)
        {
            if (casoActual != null && !string.IsNullOrEmpty(casoActual.textoVictoriaResumen))
            {
                modalBody.text = casoActual.textoVictoriaResumen;
            }
            else
            {
                modalBody.text = "¡Ganaste! Encontraste las 3 notas falsas y las tiraste a la papelera.\n\n" +
                                 "Gracias a tu gran trabajo resolviendo el misterio, ¡descubrimos la verdad del caso!\n\n" +
                                 "¡Eres un excelente detective!";
            }
        }

        if (btnModalAccion != null) btnModalAccion.text = "¡Terminar caso!";
        modalResultado.RemoveFromClassList("oculta");
    }

    private void MostrarModalDerrota()
    {
        if (modalResultado == null) return;

        if (btnCerrarModal != null) btnCerrarModal.AddToClassList("oculta");
        if (modalTitulo != null) modalTitulo.text = "Te has quedado sin vidas";
        if (modalBody != null)
        {
            modalBody.text = "Inténtalo de nuevo\n\n" +
                             "Recuerda revisar detenidamente las notas antes de tirarlas a la papelera.\n\n" +
                             "¡Un buen detective analiza dos veces!";
        }

        if (btnModalAccion != null) btnModalAccion.text = "Reiniciar Tablero";
        modalResultado.RemoveFromClassList("oculta");
    }

    public void ReiniciarJuego()
    {
        vidas = MAX_VIDAS;
        nivelActualIndex = 0;
        juegoTerminado = false;
        procesandoSeleccion = false;
        postitsProcesados.Clear();

        ActualizarVidas();
        ActualizarNivelUI();

        if (toastNotification != null) toastNotification.AddToClassList("oculta");
        if (modalResultado != null) modalResultado.AddToClassList("oculta");
    }

    private void OnClicVolverATestimonios()
    {
        NavegacionPrincipal nav = FindAnyObjectByType<NavegacionPrincipal>();
        if (nav != null)
        {
            nav.VolverATestimonios();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnClicModalAccion()
    {
        if (vidas <= 0)
        {
            ReiniciarJuego();
        }
        else
        {
            CerrarModal();
            OnClicVolverATestimonios();
        }
    }

    private void CerrarModal()
    {
        if (modalResultado != null) modalResultado.AddToClassList("oculta");
    }
}
