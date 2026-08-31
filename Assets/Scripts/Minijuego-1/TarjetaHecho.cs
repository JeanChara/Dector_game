using UnityEngine;
using TMPro;

public class TarjetaHecho : MonoBehaviour
{
    [Header("Configuración de Hecho")]
    [Tooltip("El orden cronológico correcto del hecho (1 a 5).")]
    public int idCronologico;

    [Tooltip("Texto descriptivo del hecho.")]
    [TextArea(2, 4)]
    public string descripcionHecho;

    [Header("Referencias de UI")]
    [SerializeField] private TextMeshProUGUI campoTexto;

    private void Start()
    {
        InicializarUI();
    }

    public void InicializarUI()
    {
        if (campoTexto != null && !string.IsNullOrEmpty(descripcionHecho))
        {
            campoTexto.text = descripcionHecho;
        }
    }
}
