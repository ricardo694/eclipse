using UnityEngine;

public class RangoEnemigo : MonoBehaviour
{
    public EnemyController enemigo;  // asignar en Inspector

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.CompareTag("Player") && !enemigo.atacando)
        {
            enemigo.EmpezarAtaque();
        }
    }

    void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.CompareTag("Player"))
        {
            // si el jugador sale del rango, cancela el ataque
            enemigo.FinAtaque();
        }
    }
}