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
        // Espera a que el sistema de localización esté listo
        yield return LocalizationSettings.InitializationOperation;

        languageDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>();

        var locales = LocalizationSettings.AvailableLocales.Locales;
        foreach (var locale in locales)
            options.Add(locale.LocaleName);

        languageDropdown.AddOptions(options);

        // Seleccionar el idioma actual guardado
        int savedIndex = PlayerPrefs.GetInt("LanguageIndex", 0);
        languageDropdown.value = savedIndex;
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);

        _localesLoaded = true;
    }

    private void OnLanguageChanged(int index)
    {
        if (!_localesLoaded) return;
        var locales = LocalizationSettings.AvailableLocales.Locales;
        if (index < locales.Count)
            LocalizationSettings.SelectedLocale = locales[index];
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
        // Guardar idioma
        PlayerPrefs.SetInt("LanguageIndex", languageDropdown.value);

        // Guardar FPS
        int fpsIndex = fpsDropdown.value;
        PlayerPrefs.SetInt("FPSIndex", fpsIndex);
        Application.targetFrameRate = fpsOptions[fpsIndex];

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