using UnityEngine;
using UnityEngine.Localization.Settings;
using System.Collections;

public class IdiomaManager : MonoBehaviour
{
    public static IdiomaManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);

          
            if (!PlayerPrefs.HasKey("IdiomaCode"))
                PlayerPrefs.SetString("IdiomaCode", "en");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        StartCoroutine(CargarIdioma());
    }

    public IEnumerator CargarIdioma()
    {
        yield return LocalizationSettings.InitializationOperation;

        var locales = LocalizationSettings.AvailableLocales.Locales;

        // Forzar siempre el idioma guardado, ignorando detección automática
        string codigoGuardado = PlayerPrefs.GetString("IdiomaCode", "en");
        var locale = locales.Find(l => l.Identifier.Code == codigoGuardado);
        
        if (locale != null && LocalizationSettings.SelectedLocale != locale)
            LocalizationSettings.SelectedLocale = locale;
    }

    public void CambiarIdioma(string codigo) // "es" o "en"
    {
        PlayerPrefs.SetString("IdiomaCode", codigo);
        PlayerPrefs.Save();
        StartCoroutine(CargarIdioma());
    }
}