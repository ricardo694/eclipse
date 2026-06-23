using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicaManager : MonoBehaviour
{
    public static MusicaManager Instance;

    public AudioSource audioSource;

    [Header("Músicas")]
    public AudioClip musicaMenu;       // Menu, StoryMode, Login, Settings, etc.
    public AudioClip musicaCinematica; // Escena Cinematica
    public AudioClip musicaNivel;      // Level_1, Level_2, etc.

    [Range(0f, 1f)]
    public float volumen = 0.5f;

    [Header("Escenas por grupo")]
    public string[] escenasMenu = { "Menu", "StoryMode", "Login", "config", "Perfil", "Multiplayer", "Edicion" };
    public string[] escenasCinematica = { "Cinematica" };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        audioSource.loop = true;
        audioSource.volume = volumen;
        TocarSegunEscena(SceneManager.GetActiveScene().name);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TocarSegunEscena(scene.name);
    }

    void TocarSegunEscena(string nombreEscena)
    {
        
        foreach (string nombre in escenasMenu)
        {
            if (nombreEscena == nombre)
            {
                CambiarMusica(musicaMenu);
                return;
            }
        }

        
        foreach (string nombre in escenasCinematica)
        {
            if (nombreEscena == nombre)
            {
                CambiarMusica(musicaCinematica);
                return;
            }
        }

       
        CambiarMusica(musicaNivel);
    }

    void CambiarMusica(AudioClip nuevoClip)
    {
        
        if (audioSource.clip == nuevoClip && audioSource.isPlaying) return;

        audioSource.Stop();
        audioSource.clip = nuevoClip;
        audioSource.Play();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void PararMusica() => audioSource.Stop();
    public void ReanudarMusica() => audioSource.Play();
    public void CambiarVolumen(float nuevoVolumen) => audioSource.volume = nuevoVolumen;
}