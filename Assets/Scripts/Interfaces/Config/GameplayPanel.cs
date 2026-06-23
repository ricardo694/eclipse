using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;
using System.Collections;

public class GameplaySettings : MonoBehaviour
{
    [Header("Language")]
    public TMP_Dropdown languageDropdown;

    [Header("FPS Cap")]
    public TMP_Dropdown fpsDropdown;

    private int[] fpsOptions = { 30, 60, 120, 144, 240, 0 };
    private bool _localesLoaded = false;

    void Start()
    {
        PopulateFPSDropdown();
        LoadFPSSettings();
        StartCoroutine(SetupLanguageDropdown());
    }

    // ── Idioma ────────────────────────────────────────────────
   private IEnumerator SetupLanguageDropdown()
{
    yield return LocalizationSettings.InitializationOperation;

    languageDropdown.ClearOptions();
    var options = new System.Collections.Generic.List<string>();

    var locales = LocalizationSettings.AvailableLocales.Locales;
    foreach (var locale in locales)
        options.Add(locale.LocaleName);

    languageDropdown.AddOptions(options);

    // Seleccionar el idioma activo actualmente
    var current = LocalizationSettings.SelectedLocale;
    int currentIndex = locales.IndexOf(current);
    
    // ← primero marcamos loaded, luego asignamos el valor
    _localesLoaded = true;
    languageDropdown.value = currentIndex >= 0 ? currentIndex : 0;
    
    languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
}

    private void OnLanguageChanged(int index)
    {
         
        if (!_localesLoaded) return;
        var locales = LocalizationSettings.AvailableLocales.Locales;
        if (index < locales.Count)
        {
            string codigo = locales[index].Identifier.Code;
            IdiomaManager.Instance?.CambiarIdioma(codigo);
        }
    }

    // ── FPS ───────────────────────────────────────────────────
    void PopulateFPSDropdown()
    {
        fpsDropdown.ClearOptions();
        var fpsList = new System.Collections.Generic.List<string>();
        foreach (int fps in fpsOptions)
            fpsList.Add(fps == 0 ? "Unlimited" : fps.ToString());
        fpsDropdown.AddOptions(fpsList);
    }

    void LoadFPSSettings()
    {
        int fpsIndex = PlayerPrefs.GetInt("FPSIndex", 2);
        fpsDropdown.value = fpsIndex;
        Application.targetFrameRate = fpsOptions[fpsIndex];
    }

    // ── Botones ───────────────────────────────────────────────
    public void OnSave()
    {
        PlayerPrefs.SetInt("FPSIndex", fpsDropdown.value);
        Application.targetFrameRate = fpsOptions[fpsDropdown.value];
        PlayerPrefs.Save();
    }


    public void OnDefault()
    {
        languageDropdown.value = 0;   // primer idioma
        fpsDropdown.value = 2;        // 120 FPS
        OnLanguageChanged(0);
    }

    public void OnExit()
    {
        if (SceneManager.GetActiveScene().name == "config")
            SceneManager.LoadScene("Menu");
        else
            FindAnyObjectByType<Pausa>()?.RegresarAPausa();
    }
}