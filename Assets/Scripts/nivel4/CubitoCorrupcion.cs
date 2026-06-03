using UnityEngine;

public class CubitoCorrupcion : MonoBehaviour
{
    [Header("Configuración")]
    public float tiempoDeVida = 6.0f; // Los cubos se destruyen tras 6 segundos para no ralentizar el juego

    void Start()
    {
        // Hace que el cubo se destruya solo después de unos segundos
        Destroy(gameObject, tiempoDeVida);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si el cubito toca al jugador, ejecuta la lógica de daño
        if (collision.CompareTag("Player"))
        {
            Debug.Log("¡La corrupción tocó al jugador!");
            // Aquí puedes restar vida al jugador, ej: collision.GetComponent<VidaJugador>().RecibirDano();
        }
    }
}