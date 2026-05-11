using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // patrulla
    public float speedWalk = 2.5f;
    public float cronometro = 4f;
    public int rutina;
    public int direccion;

    // detección
    public float rangoVision;
    public float rangoAtaque;

    // estado
    public bool atacando;
    private bool enInmunidad;
    public float tiempoInmunidad = 1f;
    private float timerInmunidad;

    // referencias
    public GameObject target;
    public GameObject Hit;
    private Animator animator;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        target = GameObject.Find("Player");
    }

    void Update()
    {
        if (enInmunidad)
        {
            timerInmunidad -= Time.deltaTime;
            if (timerInmunidad <= 0f)
                enInmunidad = false;
        }

        Comportamiento();
    }

    void Mover(float direccionX)
    {
        rb.linearVelocity = new Vector2(direccionX * speedWalk, rb.linearVelocity.y);
    }

    void Comportamiento()
    {
        float distancia = Mathf.Abs(transform.position.x - target.transform.position.x);

        if (distancia > rangoVision)
        {
            // --- fuera de visión: patrulla ---
            animator.SetBool("run", false);
            animator.SetBool("attack", false);

            cronometro += Time.deltaTime;
            if (cronometro >= 4f)
            {
                rutina = Random.Range(0, 2);
                cronometro = 0f;
            }

            switch (rutina)
            {
                case 0:
                    animator.SetBool("walk", false);
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                    break;
                case 1:
                    direccion = Random.Range(0, 2);
                    rutina++;
                    break;
                case 2:
                    transform.rotation = Quaternion.Euler(0, direccion == 0 ? 180 : 0, 0);
                    Mover(direccion == 0 ? -1f : 1f);
                    animator.SetBool("walk", true);
                    break;
            }
        }
        else if (distancia > rangoAtaque)
        {
            // --- dentro de visión, fuera de ataque: caminar hacia el jugador ---
            if (!atacando)
            {
                animator.SetBool("attack", false);
                animator.SetBool("run", false);
                animator.SetBool("walk", true);

                if (transform.position.x < target.transform.position.x)
                {
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                    Mover(1f);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                    Mover(-1f);
                }
            }
        }
        else
        {
            // --- dentro de rango de ataque ---
            if (!atacando)
            {
                if (transform.position.x < target.transform.position.x)
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                else
                    transform.rotation = Quaternion.Euler(0, 180, 0);

                animator.SetBool("walk", false);
                animator.SetBool("run", false);
                EmpezarAtaque();
            }
        }
    }

    public void EmpezarAtaque()
    {
        atacando = true;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        animator.SetBool("attack", true);
    }

    public void FinAtaque()
    {
        atacando = false;
        animator.SetBool("attack", false);
    }

    public void ColliderWeaponTrue()
    {
        Hit.GetComponent<BoxCollider2D>().enabled = true;
    }

    public void ColliderWeaponFalse()
    {
        Hit.GetComponent<BoxCollider2D>().enabled = false;
    }

    public bool PuedeGolpear()
    {
        return !enInmunidad;
    }

    public void ActivarInmunidad()
    {
        enInmunidad = true;
        timerInmunidad = tiempoInmunidad;
    }
}