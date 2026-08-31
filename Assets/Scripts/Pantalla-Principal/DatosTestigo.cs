using UnityEngine;

[CreateAssetMenu(fileName = "NuevoDatosTestigo", menuName = "ScriptableObjects/DatosTestigo", order = 1)]
public class DatosTestigo : ScriptableObject
{
    [Header("Información del Testigo")]
    public string nombreTestigo;
    public string rolTestigo;
    
    [Tooltip("Foto grande para la tarjeta de la pantalla principal (ej: 355x444).")]
    public Sprite fotoTarjeta;
    
    [Tooltip("Retrato pequeño para la lista del menú emergente (ej: 50x58).")]
    public Sprite fotoRetrato;
    
    [Tooltip("Imagen de la escena o pista detallada para el panel del menú emergente (ej: 161x195).")]
    public Sprite fotoDetalle;

    [Header("Testimonios")]
    [Tooltip("Testimonio resumido para la tarjeta de la pantalla principal.")]
    [TextArea(4, 8)]
    public string textoTestimonio;

    [Tooltip("Testimonio más detallado que se mostrará en el menú emergente.")]
    [TextArea(5, 10)]
    public string textoTestimonioDetalle;
}
