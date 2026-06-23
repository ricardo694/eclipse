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

    [Header("Checkpoint")]
    public Vector3 checkpointPosition;
    private Checpoint checkpointActual;

    public Button revivirButton;

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

        int[] fpsOptions = { 30, 60, 120, 144, 240, 0 };
            int fpsIndex = PlayerPrefs.GetInt("FPSIndex", 2);
            Application.targetFrameRate = fpsOptions[fpsIndex];

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (reiniciarButton != null)
            reiniciarButton.onClick.AddListener(ReiniciarJuego);

        if (menuButton != null)
            menuButton.onClick.AddListener(VolverAlMenu);

        if (revivirButton != null)
        revivirButton.onClick.AddListener(Revivir); 

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
                checkpointPosition = player.transform.position;
        }
    }

    void Update()
    {
        if (gameOverActivo && Keyboard.current.rKey.wasPressedThisFrame)
            Revivir(); // ← R para revivir en checkpoint

        if (gameOverActivo && Keyboard.current.tKey.wasPressedThisFrame)
            ReiniciarJuego(); // ← t para reiniciar desde cero

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

        Time.timeScale = 0f;

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
        if (MusicaManager.Instance != null)
            MusicaManager.Instance.PararMusica();
        SceneManager.LoadScene("StoryMode");
    }

    public void ActualizarCheckpoint(Vector3 nuevaPosicion, Checpoint nuevoCheckpoint)
    {
        checkpointPosition = nuevaPosicion;
        checkpointActual = nuevoCheckpoint;
    }

    public void RespawnJugador()
    {
        StartCoroutine(RespawnCoroutine());
    }

    public void Revivir()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        gameOverActivo = false;
        Time.timeScale = 1f;
        StartCoroutine(RespawnCoroutine());
    }


    IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            player.GetComponent<PlayerController>().Respawn(checkpointPosition);
    }

  
}