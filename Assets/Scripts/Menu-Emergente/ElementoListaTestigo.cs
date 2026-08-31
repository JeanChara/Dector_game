using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ElementoListaTestigo : MonoBehaviour
{
    [Header("Referencias de UI del Elemento")]
    [SerializeField] private Image imagenRetrato;
    [SerializeField] private TextMeshProUGUI textoNombre;
    [SerializeField] private TextMeshProUGUI textoRol;
    [Tooltip("El componente Image del fondo (si se deja vacío, buscará el del mismo GameObject).")]
    [SerializeField] private Image fondoElemento;

    [Header("Configuración Visual (Sprites de Fondo)")]
    [Tooltip("Sprite de fondo cuando el testigo está seleccionado (ej. caja-seccion-testimonios.png)")]
    [SerializeField] private Sprite spriteSeleccionado;
    
    [Tooltip("Sprite de fondo cuando el testigo NO está seleccionado (opcional, puede ser nulo para transparente)")]
    [SerializeField] private Sprite spriteDeseleccionado;

    private int indiceElemento = -1;
    private MenuEmergenteController menuControlador;
    private Button botonElemento;

    /// <summary>
    /// Configura dinámicamente los datos y la interacción del elemento de la lista.
    /// </summary>
    public void Configurar(int indice, DatosTestigo datos, MenuEmergenteController controlador)
    {
        indiceElemento = indice;
        menuControlador = controlador;

        if (fondoElemento == null)
        {
            fondoElemento = GetComponent<Image>();
        }

        if (textoNombre != null) textoNombre.text = datos.nombreTestigo;
        if (textoRol != null) textoRol.text = datos.rolTestigo;
        if (imagenRetrato != null)
        {
            if (datos.fotoRetrato != null)
            {
                imagenRetrato.sprite = datos.fotoRetrato;
                imagenRetrato.gameObject.SetActive(true);
            }
            else
            {
                imagenRetrato.gameObject.SetActive(false);
            }
        }

        // Configurar evento de click del botón
        botonElemento = GetComponent<Button>();
        if (botonElemento != null)
        {
            botonElemento.onClick.RemoveAllListeners();
            botonElemento.onClick.AddListener(AlHacerClick);
        }

        EstablecerSeleccionado(false);
    }

    /// <summary>
    /// Modifica visualmente el fondo para reflejar el estado seleccionado (cambiando sprites).
    /// </summary>
    public void EstablecerSeleccionado(bool seleccionado)
    {
        if (fondoElemento != null)
        {
            if (seleccionado)
            {
                fondoElemento.sprite = spriteSeleccionado;
                fondoElemento.color = Color.white; // Opacidad completa para el sprite
            }
            else
            {
                fondoElemento.sprite = spriteDeseleccionado;
                // Si el sprite deseleccionado es nulo, volvemos la imagen transparente.
                // Si tiene un sprite, lo mantenemos visible.
                fondoElemento.color = spriteDeseleccionado != null ? Color.white : Color.clear;
            }
        }
    }

    private void AlHacerClick()
    {
        if (menuControlador != null && indiceElemento != -1)
        {
            menuControlador.SeleccionarTestigo(indiceElemento);
        }
    }
}
