using UnityEngine;

public class UISoundController : MonoBehaviour
{
    public static UISoundController Instance;

    public AudioSource audioSource;
    public AudioClip sonidoClick;
    public AudioClip sonidoHover; // opcional, al pasar el mouse por encima

    void Awake()
    {
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

    public void PlayClick()
    {
        audioSource.PlayOneShot(sonidoClick);
    }

    public void PlayHover()
    {
        audioSource.PlayOneShot(sonidoHover);
    }
}