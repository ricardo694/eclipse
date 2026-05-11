using UnityEngine;

public class HitEnemy : MonoBehaviour
{
    public int danio = 1;
    public EnemyController enemigo;  // asignar en Inspector

    void Start()
    {
        // lo busca en el padre automáticamente
        enemigo = GetComponentInParent<EnemyController>();
    }
    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.CompareTag("Player"))
        {
            // solo golpea si el jugador no está en inmunidad
            if (enemigo.PuedeGolpear())
            {
                PlayerController player = coll.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.RecibeDano(transform.position, danio);
                    enemigo.ActivarInmunidad();  // activa el periodo de inmunidad
                }
            }
        }
    }
}