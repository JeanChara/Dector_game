using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class EfectoHoverBoton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float escalaHover = 1.08f;
    [SerializeField] private float tiempoTransicion = 0.15f;

    private Vector3 escalaOriginal;
    private Coroutine corrutinaAnimacion;

    private void Awake()
    {
        escalaOriginal = transform.localScale;
    }

    private void OnDisable()
    {
        transform.localScale = escalaOriginal;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var btn = GetComponent<UnityEngine.UI.Button>();
        if (btn != null && !btn.interactable) return;

        IniciarAnimacion(escalaOriginal * escalaHover);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IniciarAnimacion(escalaOriginal);
    }

    private void IniciarAnimacion(Vector3 escalaObjetivo)
    {
        if (corrutinaAnimacion != null)
        {
            StopCoroutine(corrutinaAnimacion);
        }
        if (gameObject.activeInHierarchy)
        {
            corrutinaAnimacion = StartCoroutine(AnimarEscala(escalaObjetivo));
        }
        else
        {
            transform.localScale = escalaObjetivo;
        }
    }

    private IEnumerator AnimarEscala(Vector3 escalaObjetivo)
    {
        Vector3 escalaInicial = transform.localScale;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoTransicion)
        {
            tiempoTranscurrido += Time.unscaledDeltaTime;
            float t = tiempoTranscurrido / tiempoTransicion;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            transform.localScale = Vector3.Lerp(escalaInicial, escalaObjetivo, t);
            yield return null;
        }

        transform.localScale = escalaObjetivo;
        corrutinaAnimacion = null;
    }
}
