using UnityEngine;

public class MusicaManager : MonoBehaviour
{
    public static MusicaManager Instance;

    public AudioSource audioSource;
    public AudioClip musicaNivel;

    [Range(0f, 1f)]
    public float volumen = 0.5f;

    void Awake()
    {
        // Singleton — que no se destruya entre escenas
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        audioSource.clip = musicaNivel;
        audioSource.loop = true;
        audioSource.volume = volumen;
        audioSource.Play();
    }

    public void PararMusica() => audioSource.Stop();
    public void ReanudarMusica() => audioSource.Play();
    public void CambiarVolumen(float nuevoVolumen) => audioSource.volume = nuevoVolumen;
}