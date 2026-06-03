using UnityEngine;

public class CorrupcionGenerator : MonoBehaviour
{
    [Header("Componentes")]
    public GameObject prefabCubito; // Aquí arrastraremos el clon de tu cubo

    [Header("Movimiento de la Ola")]
    public float velocidadAvance = 3.0f; // Qué tan rápido avanza la pared de corrupción hacia la derecha

    [Header("Configuración del Enjambre")]
    public float alturaPantalla = 10.0f; // Qué tan alta es la zona donde aparecerán los cubos (eje Y)
    public float cubosPorSegundo = 30.0f; // Cuántos cubitos spawnear por segundo (¡Prueba con números altos!)
    public float desordenHorizontal = 0.5f; // Qué tan atrás o adelante del eje de la ola pueden aparecer

    private float cronometro = 0f;

    void Update()
    {
        // 1. Hace que el generador avance de forma constante hacia la derecha persiguiendo al jugador
        transform.Translate(Vector3.right * velocidadAvance * Time.deltaTime);

        // 2. Control del tiempo para spawnear los cubos rápidamente
        cronometro += Time.deltaTime;
        float tiempoEntreSpawns = 1.0f / cubosPorSegundo;

        if (cronometro >= tiempoEntreSpawns)
        {
            SpawnearCubitoCaotico();
            cronometro = 0f;
        }
    }

    void SpawnearCubitoCaotico()
    {
        if (prefabCubito == null) return;

        // Calcula una posición vertical aleatoria dentro del rango establecido
        float yAleatorio = Random.Range(-alturaPantalla / 2f, alturaPantalla / 2f);

        // Genera el "desorden" para romper la organización perfecta (como en tu mockup)
        float xAleatorio = Random.Range(-desordenHorizontal, desordenHorizontal);

        // Posición final del cubito individual
        Vector3 posicionSpawn = new Vector3(
            transform.position.x + xAleatorio,
            transform.position.y + yAleatorio,
            0f
        );

        // Crea el cubito en el mundo de forma independiente
        Instantiate(prefabCubito, posicionSpawn, Quaternion.identity);
    }

    // Dibujar una línea guía en el editor de Unity para saber el tamaño de la ola
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(transform.position.x, transform.position.y - (alturaPantalla / 2f), 0),
            new Vector3(transform.position.x, transform.position.y + (alturaPantalla / 2f), 0)
        );
    }
}