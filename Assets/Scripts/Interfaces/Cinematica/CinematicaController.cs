using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using TMPro;
using System.Collections;

public class CinematicaController : MonoBehaviour
{
    [Header("UI")]
    public Image fondoEspacial;        
    public Image imagenCinematica;      
    public TextMeshProUGUI textoSubtitulo;
    public Image panelFadeNegro;
    public GameObject botonContinuar;
    public GameObject botonOmitir;

    [Header("Cinemática")]
    public CinematicaFrame[] frames;
    public float tiempoPorFrame = 5f;
    public float velocidadTypewriter = 0.03f;
    public float duracionFade = 0.6f;

    [Header("Localización")]
     public string tableName = "UIText"; 
    private int frameActual = 0;
    private bool escribiendo = false;
    private bool avanzar = false;
    private bool cinematicaTerminada = false;

    void Start()
    {
        panelFadeNegro.color = Color.black;
        botonContinuar.SetActive(false);
        StartCoroutine(IniciarCinematica());
    }

    void Update()
    {
        if (cinematicaTerminada) return;

        if (Keyboard.current.enterKey.wasPressedThisFrame ||
                    Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    avanzar = true;
                }
    }

    IEnumerator IniciarCinematica()
    {  
        yield return LocalizationSettings.InitializationOperation;
        yield return StartCoroutine(FadeNegro(1f, 0f));

        for (frameActual = 0; frameActual < frames.Length; frameActual++)
        {
            yield return StartCoroutine(MostrarFrame(frames[frameActual]));
        }

        yield return StartCoroutine(TerminarCinematica());
    }

    IEnumerator MostrarFrame(CinematicaFrame frame)
    {
        avanzar = false;
        botonContinuar.SetActive(false);

        // Fade solo en la imagen del marco, no en todo
        yield return StartCoroutine(FadeImagen(imagenCinematica, 1f, 0f));
        imagenCinematica.sprite = frame.imagen;
        yield return StartCoroutine(FadeImagen(imagenCinematica, 0f, 1f));

        string texto = GetLocalizedText(frame.claveTexto);

        // Typewriter
        textoSubtitulo.text = "";
        yield return StartCoroutine(EscribirTexto(texto));

        // Muestra botón continuar cuando termina de escribir
        botonContinuar.SetActive(true);

        // Espera timer o input
        float timer = 0f;
        while (timer < tiempoPorFrame && !avanzar)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        avanzar = false;
        botonContinuar.SetActive(false);
    }

    string GetLocalizedText(string key)
    {
        var op = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
        return op; // retorna el texto en el idioma activo
    }

    IEnumerator EscribirTexto(string texto)
    {
        escribiendo = true;
        textoSubtitulo.text = "";

        foreach (char letra in texto)
        {
            if (avanzar)
            {
                textoSubtitulo.text = texto;
                avanzar = false;
                break;
            }
            textoSubtitulo.text += letra;
            yield return new WaitForSeconds(velocidadTypewriter);
        }

        escribiendo = false;
    }

    // Fade solo de la imagen del marco
    IEnumerator FadeImagen(Image imagen, float desde, float hasta)
    {
        float t = 0f;
        Color c = imagen.color;
        while (t < 1f)
        {
            t += Time.deltaTime / duracionFade;
            c.a = Mathf.Lerp(desde, hasta, t);
            imagen.color = c;
            yield return null;
        }
    }

    // Fade negro general (inicio y fin)
    IEnumerator FadeNegro(float desde, float hasta)
    {
        float t = 0f;
        Color color = panelFadeNegro.color;
        while (t < 1f)
        {
            t += Time.deltaTime / duracionFade;
            color.a = Mathf.Lerp(desde, hasta, t);
            panelFadeNegro.color = color;
            yield return null;
        }
    }

    IEnumerator TerminarCinematica()
    {
        cinematicaTerminada = true;
        botonOmitir.SetActive(false);
        botonContinuar.SetActive(false);
        yield return StartCoroutine(FadeNegro(0f, 1f));
         SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void BotonContinuarPresionado()
    {
        avanzar = true;
    }

    public void OmitirCinematica()
    {
        if (cinematicaTerminada) return;
        StopAllCoroutines();
        StartCoroutine(TerminarCinematica());
    }
}

[System.Serializable]
public class CinematicaFrame
{
    public Sprite imagen;
    [TextArea(2, 4)]
    public string claveTexto;
}