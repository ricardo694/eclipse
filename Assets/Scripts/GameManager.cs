using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button reiniciarButton;
    public Button menuButton;

    private bool gameOverActivo = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Cargar bindings guardados
        if (PlayerPrefs.HasKey("inputBindings"))
        {
            string json = PlayerPrefs.GetString("inputBindings");
            PlayerInput playerInput = FindAnyObjectByType<PlayerInput>();
            if (playerInput != null)
                playerInput.actions.LoadBindingOverridesFromJson(json);
        }
    }

    void Start()
    {
        StartCoroutine(CargarIdioma());

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (reiniciarButton != null)
            reiniciarButton.onClick.AddListener(ReiniciarJuego);

        if (menuButton != null)
            menuButton.onClick.AddListener(VolverAlMenu);
    }

    // ← CargarIdioma va AQUÍ, fuera de Start y Awake
    private IEnumerator CargarIdioma()
    {
        yield return LocalizationSettings.InitializationOperation;

        int savedIndex = PlayerPrefs.GetInt("LanguageIndex", 0);
        var locales = LocalizationSettings.AvailableLocales.Locales;
        if (savedIndex < locales.Count)
            LocalizationSettings.SelectedLocale = locales[savedIndex];
    }

    void Update()
    {
        if (gameOverActivo && Keyboard.current.rKey.wasPressedThisFrame)
            ReiniciarJuego();

        if (gameOverActivo && (Keyboard.current.mKey.wasPressedThisFrame || 
            Keyboard.current.escapeKey.wasPressedThisFrame))
            VolverAlMenu();
    }

    public void GameOver()
    {
        if (gameOverActivo) return;

        gameOverActivo = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverText != null)
            gameOverText.text = "¡Has Perdido!";
    }

    public void ReiniciarJuego()
    {
        gameOverActivo = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}