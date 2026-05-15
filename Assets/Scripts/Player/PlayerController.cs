
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    //movimiento
    public float velocidad = 5f;

    //salto
    public float fuerzaSalto = 10f;
    public float longitudRaycast = 0.1f;
    public LayerMask capaSuelo;
    public Animator animator;
    private Rigidbody2D rb;
    private bool enSuelo;
    private int saltosRestantes = 0; 
    public int maxSaltos = 2;        

    public Vector2 colliderSizeSalto;
    public Vector2 colliderOffsetSalto;

    //vida 

    public int vida =3;
    public bool muerto;
    //inmunidad
    public float duracionInmunidad = 2f;
    private int layerEnemigo;
    public float duracionAnimDano= 0.2f;
    //daño
    private bool recibiendoDano;
    public float fuerzaRebote = 0.2f;

    //ataque
    private bool atacando;

    //dash
    public float fuerzaDash = 14f;
    public float duracionDash = 0.15f;
    public float cooldownDash = 1f;
    private bool dasheando;
    private bool dashDisponible = true;
    private float timerDash;
    private float timerCooldown;
    private float direccionDash;

    // agacharse
    private bool agachado;
    public float velocidadAgachado = 2.5f;    
    public Vector2 colliderSizeNormal;
    public Vector2 colliderOffsetNormal;
    public Vector2 colliderSizeAgachado;
    public Vector2 colliderOffsetAgachado;
    public CapsuleCollider2D col;              
    public float radioCheckArriba = 0.2f;
    public LayerMask capaTecho;  

    // New Input System 
    private Vector2 inputMovimiento;
    private bool saltoPulsado;
    private bool dashPulsado;
    private bool ataquePulsado;
    private bool agacharPulsado;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        transform.position = new Vector3(transform.position.x, transform.position.y, 0f); // ← fuerza Z=0

        colliderSizeNormal   = col.size;
        colliderOffsetNormal = col.offset;
        colliderSizeAgachado   = new Vector2(col.size.x, col.size.y * 0.5f);
        colliderOffsetAgachado = new Vector2(col.offset.x, col.offset.y - col.size.y * 0.25f);

        colliderSizeSalto   = new Vector2(col.size.x, col.size.y * 0.8f);   
        colliderOffsetSalto = new Vector2(col.offset.x, col.offset.y + col.size.y * 0.1f);

        layerEnemigo= LayerMask.NameToLayer("Enemy");
    }
    //===================================================================== Callbacks del nuevo Input System =====================================================================
 
public void OnMove(InputAction.CallbackContext context)
{
    inputMovimiento = context.ReadValue<Vector2>();
}

public void OnJump(InputAction.CallbackContext context)
{
    if (context.performed) saltoPulsado = true;
}

public void OnDash(InputAction.CallbackContext context)
{
    if (context.performed) dashPulsado = true;
}

public void OnAttack(InputAction.CallbackContext context)
{
    if (context.performed) ataquePulsado = true;
}

public void CrouchStarted(InputAction.CallbackContext context)
{
    if (context.started) agacharPulsado = true;
}

public void CrouchCanceled(InputAction.CallbackContext context)
{
    if (context.canceled) agacharPulsado = false;
}
    // =====================================================================
    void Update()
    {
        // --- Timers del dash ---
        if (dasheando)
        {
            timerDash -= Time.deltaTime;
            if (timerDash <= 0f)
                TerminarDash();
        }

        if (!dashDisponible)
        {
            timerCooldown -= Time.deltaTime;
            if (timerCooldown <= 0f)
                dashDisponible = true;
        }

        //Detección del suelo
        Vector2 origenRaycast = new Vector2(
            transform.position.x + col.offset.x,
            transform.position.y + col.offset.y - col.size.y * 0.5f
        );
        RaycastHit2D hit = Physics2D.Raycast(origenRaycast, Vector2.down, longitudRaycast, capaSuelo);
        enSuelo = hit.collider != null;

        if (!atacando && !dasheando)
        {
            Movimiento();
           
       
        if (enSuelo)
            saltosRestantes = maxSaltos; 

        if (saltoPulsado && saltosRestantes > 0 && !recibiendoDano && !agachado)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); 
            rb.AddForce(new Vector2(0f, fuerzaSalto), ForceMode2D.Impulse);
            saltosRestantes--;
        }
    }

        if (!muerto)
        {
             // --- Input dash (K) ---
            if (dashPulsado && dashDisponible && !dasheando && !atacando && !recibiendoDano && !agachado)
            {
                IniciarDash();
            }

            // --- Input ataque (J) ---
            if (ataquePulsado && !atacando && !dasheando && enSuelo)
            {
                Atacando();
            }

            ManejarAgacharse();

            // Limpiar flags de un solo frame
            saltoPulsado  = false;
            dashPulsado   = false;
            ataquePulsado = false;
        }
       
        Animaciones();
   

        
    }

    public void IniciarDash()
    {
        dasheando = true;
        dashDisponible = false;
        timerDash = duracionDash;
        timerCooldown = cooldownDash;

        direccionDash = transform.localScale.x; 

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(direccionDash * fuerzaDash, 0f), ForceMode2D.Impulse);
    }

    public void TerminarDash()
    {
        dasheando = false;
        rb.gravityScale = 1f;
        rb.linearVelocity = Vector2.zero;
    }

    public void RecibeDano(Vector2 direccion, int cantDano)
    {
        if(!recibiendoDano)
        {
            recibiendoDano = true;
            vida-=cantDano;
            if (vida<=0)
            {
                muerto=true;
            }
            else
            {
                //Rebote
                Vector2 rebote = new Vector2(transform.position.x - direccion.x, 0.2f).normalized;
                rb.AddForce(rebote*fuerzaRebote, ForceMode2D.Impulse);  
            }
    
        }
    }

    void ManejarAgacharse()
{
    bool quiereAgacharse = agacharPulsado && enSuelo;

    if (quiereAgacharse && !agachado)
    {
        agachado = true;
        col.size   = colliderSizeAgachado;
        col.offset = colliderOffsetAgachado;
    }

    if (!quiereAgacharse && agachado)
    {
        Vector2 puntoArriba = (Vector2)transform.position + Vector2.up * (colliderSizeNormal.y * 0.5f);
        bool hayEspacio = !Physics2D.OverlapCircle(puntoArriba, radioCheckArriba, capaTecho);

        if (hayEspacio)
        {
            agachado = false;
        }
    }


    if (!agachado)
    {
        if (!enSuelo) 
        {
            col.size   = colliderSizeSalto;
            col.offset = colliderOffsetSalto;
        }
        else 
        {
            col.size   = colliderSizeNormal;
            col.offset = colliderOffsetNormal;
        }
    }

}
    public void Movimiento()
    {
        float velActual = agachado ? velocidadAgachado : velocidad;
        float inputX = inputMovimiento.x;

        animator.SetFloat("movement", Mathf.Abs(inputX));

        if (inputX < 0) transform.localScale = new Vector3(-1, 1, 1);
        if (inputX > 0) transform.localScale = new Vector3(1, 1, 1);

        if (!recibiendoDano && !agachado)
        {
            rb.linearVelocity = new Vector2(inputX * velActual, rb.linearVelocity.y);
        }
        else if (agachado)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    public void Animaciones()
    {
        animator.SetBool("ensuelo",enSuelo);
        animator.SetBool("atacando",atacando); 
        animator.SetBool("recibiendoDano",recibiendoDano);
        animator.SetBool("dasheando", dasheando);
        animator.SetBool("agachado", agachado);
        animator.SetBool("muerto",muerto);
    }

    void FixedUpdate()
    {
    if (transform.position.z != 0f)
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
    }
    }
    public void DesactivarDano()
    {
        recibiendoDano = false;
        rb.linearVelocity = Vector2.zero;

    }


    public void Atacando()
    {
        atacando=true;
    }

    public void DesactivaAtaque()
    {
        atacando=false;
    }

    void OnDrawGizmos()
    {

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position,transform.position + Vector3.down * longitudRaycast);        
    }
    
}
