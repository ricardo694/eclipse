using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PortalController : MonoBehaviour
{
    [Header("Rotación")]
    public Transform anilloExterior;
    public Transform anilloMedio;
    public float velocidadAnilloExterior = 40f;
    public float velocidadAnilloMedio    = -65f;

    [Header("Pulso del core")]
    public Transform core;
    public float velocidadPulso = 2f;
    public float escalaPulsoMin = 0.85f;
    public float escalaPulsoMax = 1.15f;

    [Header("Pulso de luz")]
    public Light2D luzPortal;
    public float intensidadMin = 0.8f;
    public float intensidadMax = 2.5f;

    void Update()
    {
        anilloExterior.Rotate(0f, 0f, velocidadAnilloExterior * Time.deltaTime);
        anilloMedio.Rotate(0f, 0f, velocidadAnilloMedio * Time.deltaTime);

        float t = (Mathf.Sin(Time.time * velocidadPulso) + 1f) * 0.5f;
        core.localScale = new Vector3(Mathf.Lerp(escalaPulsoMin, escalaPulsoMax, t),
                                      Mathf.Lerp(escalaPulsoMin, escalaPulsoMax, t), 1f);
        luzPortal.intensity = Mathf.Lerp(intensidadMin, intensidadMax, t);
    }
}