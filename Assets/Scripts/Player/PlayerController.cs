
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Rendering;


public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public bool step1 = false;
    public float timeByStep = 0.5f;
    float cont = 0f;

    [Header("Salto")]
    public float fuerzaSalto = 4.8f;
    public int maxSaltos = 1;
    private int saltosRestantes = 0;
    private bool enSuelo;
    private bool esSaltando;
    private Vector2 colliderSizeSalto;
    private Vector2 colliderOffsetSalto;
    public float multiplicadorCorte = 0.65f;

    [Header("Detección de Suelo")]
    public Vector2 tamañoDetector = new Vector2(0.8f, 0.05f);
    public float offsetYDetector = 0.02f;
    public float margenRaycastLateral = 0.05f;

    [Header("Feel")]
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool enSueloFisico; 

    public float fallGravityMultiplier = 1.3f;
    public float maxFallSpeed = 2f;

    public float hangThreshold = 1.5f; 
    public float hangGravity = 0.4f;

    [Header("Detección de Pared")]
    public Vector2 tamañoDetectorPared = new Vector2(0.05f, 0.6f);
    public float offsetXDetectorPared = 0.5f;
    private bool tocandoPared;
    private float direccionPared; 
    [Header("Wall Jump / Wall Slide")]
    public float velocidadWallSlide = 0.8f;  // qué tan lento baja en la pared
    public float fuerzaWallJumpX = 4f;       // impulso horizontal al saltar
    public float fuerzaWallJumpY = 5f;       // impulso vertical al saltar
    private bool enPared;  

    [Header("Vida")]
    public int vida = 20;
    public bool muerto;

    [Header("Daño")]
    public float fuerzaRebote = 0.2f;
    public float duracionInmunidad = 1f; 
    public float duracionAnimDano = 0.5f;
    private bool recibiendoDano;
    private SpriteRenderer spriteRenderer;


    [Header("Ataque")]
    private int comboContador = 0;
    private bool comboRegistrado = false;
    private bool atacando;


    [Header("Dash")]
    public float fuerzaDash = 14f;
    public float duracionDash = 0.15f;
    public float cooldownDash = 1f;
    private bool dasheando;
    private bool dashDisponible = true;
    private float timerDash;
    private float timerCooldown;
    private float direccionDash;

    [Header("Agacharse")]
    public float velocidadAgachado = 2.5f;
    public float radioCheckArriba = 0.2f;
    public LayerMask capaTecho;
    private bool agachado;
    private Vector2 colliderSizeNormal;
    private Vector2 colliderOffsetNormal;
    private Vector2 colliderSizeAgachado;
    private Vector2 colliderOffsetAgachado;


    [Header("Componentes")]
    public Animator animator;
    public BoxCollider2D col;
    public LayerMask capaSuelo;
    private Rigidbody2D rb;
    public PlayerSoundController soundController;

    [Header("Input")]
    private Vector2 inputMovimiento;
    private bool saltoPulsado;
    private bool saltoPulsadoMantenido;
    private bool dashPulsado;
    private bool ataquePulsado;
    private bool agacharPulsado;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f); // ← fuerza Z=0

        colliderSizeNormal   = col.size;
        colliderOffsetNormal = col.offset;
        colliderSizeAgachado   = new Vector2(col.size.x, col.size.y * 0.5f);
        colliderOffsetAgachado = new Vector2(col.offset.x, col.offset.y - col.size.y * 0.25f);

        colliderSizeSalto   = new Vector2(col.size.x, col.size.y * 0.8f);   
        colliderOffsetSalto = new Vector2(col.offset.x, col.offset.y + col.size.y * 0.1f);
    }
    //===================================================================== Callbacks del nuevo Input System =====================================================================
 
public void OnMove(InputAction.CallbackContext context)
{
    inputMovimiento = context.ReadValue<Vector2>();
}

public void OnJump(InputAction.CallbackContext context)
{
    if (context.performed) saltoPulsado = true;
    if (context.performed)  saltoPulsadoMantenido = true;
    if (context.canceled)   saltoPulsadoMantenido = false;
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
    if (Time.timeScale == 0) return;

    // --- Timers del dash ---
    if (dasheando)
    {
        timerDash -= Time.deltaTime;
        if (timerDash <= 0f) TerminarDash();
    }
    if (!dashDisponible)
    {
        timerCooldown -= Time.deltaTime;
        if (timerCooldown <= 0f) dashDisponible = true;
    }

    // ── 1. DETECCIÓN (suelo y pared primero) ──────────────────────────
    float mitad = tamañoDetector.x * 0.5f - margenRaycastLateral;
    float baseY = transform.position.y + col.offset.y - col.size.y * 0.5f;
    float distancia = offsetYDetector + 0.05f;

    bool hitIzq    = Physics2D.Raycast(new Vector2(transform.position.x + col.offset.x - mitad, baseY), Vector2.down, distancia, capaSuelo);
    bool hitCentro = Physics2D.Raycast(new Vector2(transform.position.x + col.offset.x,         baseY), Vector2.down, distancia, capaSuelo);
    bool hitDer    = Physics2D.Raycast(new Vector2(transform.position.x + col.offset.x + mitad,  baseY), Vector2.down, distancia, capaSuelo);

    enSueloFisico = hitIzq || hitCentro || hitDer;
    enSuelo = enSueloFisico;

    float dirección = transform.localScale.x;
    RaycastHit2D hitPared = Physics2D.BoxCast(
        new Vector2(transform.position.x + col.offset.x, transform.position.y + col.offset.y),
        new Vector2(0.05f, tamañoDetectorPared.y),
        0f,
        new Vector2(dirección, 0f),
        offsetXDetectorPared,
        capaSuelo
    );
    tocandoPared = hitPared.collider != null && Vector2.Angle(hitPared.normal, Vector2.up) > 45f;
    enPared = tocandoPared && !enSuelo;
    if (enPared) direccionPared = dirección;

    // ── 2. COYOTE TIME ────────────────────────────────────────────────
    if (enSueloFisico)
        coyoteCounter = coyoteTime;
    else
        coyoteCounter -= Time.deltaTime;

    // ── 3. JUMP BUFFER ────────────────────────────────────────────────
    if (saltoPulsado)
        jumpBufferCounter = jumpBufferTime;
    else
        jumpBufferCounter -= Time.deltaTime;

    // ── 4. SALTOS ─────────────────────────────────────────────────────
    if (!atacando && !dasheando && !recibiendoDano)
    {
        if (enSueloFisico)
            saltosRestantes = maxSaltos;

        if (jumpBufferCounter > 0f)
        {
            // Wall jump — prioridad sobre salto normal
            if (enPared)
            {
                soundController.PlaySaltar();
                rb.linearVelocity = new Vector2(-direccionPared * fuerzaWallJumpX, fuerzaWallJumpY);
                jumpBufferCounter = 0f;
                esSaltando = true;
            }
            // Salto normal
            else if (coyoteCounter > 0f && saltosRestantes > 0 && !agachado)
            {
                soundController.PlaySaltar();
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(new Vector2(0f, fuerzaSalto), ForceMode2D.Impulse);
                saltosRestantes--;
                jumpBufferCounter = 0f;
                coyoteCounter = 0f;
                esSaltando = true;
            }
        }
    }

    // ── 5. GRAVEDAD ───────────────────────────────────────────────────
    if (dasheando)
    {
        // dash maneja su propia gravedad
    }
    else if (enPared && rb.linearVelocity.y < 0f)
    {
        // Wall slide — pisa todo lo demás
        rb.gravityScale = 1f;
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            Mathf.Max(rb.linearVelocity.y, -velocidadWallSlide)
        );
    }
    else if (!enSuelo && rb.linearVelocity.y < 0f)
    {
        // Cayendo normal
        rb.gravityScale = fallGravityMultiplier;
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            Mathf.Max(rb.linearVelocity.y, -maxFallSpeed)
        );
    }
    else if (esSaltando && !enSuelo && Mathf.Abs(rb.linearVelocity.y) < hangThreshold)
    {
        // Jump hang
        rb.gravityScale = hangGravity;
    }
    else
    {
        rb.gravityScale = 1f;
    }

    if (enSueloFisico) esSaltando = false;

    // ── 6. SALTO VARIABLE ─────────────────────────────────────────────
    if (!saltoPulsadoMantenido && rb.linearVelocity.y > 0f && !dasheando && !enPared)
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.65f);

    // ── 7. MOVIMIENTO + ACCIONES ──────────────────────────────────────
    if (!atacando && !dasheando)
        Movimiento();

    if (!muerto)
    {
        if (dashPulsado && dashDisponible && !dasheando && !atacando && !recibiendoDano && !agachado)
            IniciarDash();

        if (ataquePulsado && !dasheando && enSuelo)
        {
            if (!atacando)
            {
                comboContador = 0;
                Atacando();
            }
            else if (atacando && !comboRegistrado && comboContador < 1)
            {
                comboRegistrado = true;
            }
        }

        ManejarAgacharse();

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

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GameOver();
                }
            }
            else
            {
                 StopAllCoroutines();
                //Rebote
                Vector2 rebote = new Vector2(transform.position.x - direccion.x, 0.2f).normalized;
                rb.AddForce(rebote*fuerzaRebote, ForceMode2D.Impulse);  

                StartCoroutine(RecuperarseDeDano());
            }
    
        }
    }

    private IEnumerator RecuperarseDeDano()
    {
    
        atacando = false;
        comboContador = 0;
        comboRegistrado = false;
        animator.ResetTrigger("0");
        animator.ResetTrigger("1");
        animator.Play("idle1"); 

        //inmunidad 
        int layerPlayer = gameObject.layer;
        int layerEnemy = LayerMask.NameToLayer("Enemy");
        Physics2D.IgnoreLayerCollision(layerPlayer, layerEnemy, true);

        // Parpadeo del sprite
        StartCoroutine(Parpadeo());

        yield return new WaitForSeconds(duracionAnimDano);

        recibiendoDano = false;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(duracionInmunidad - duracionAnimDano);

        //restaurar colisiones
        Physics2D.IgnoreLayerCollision(layerPlayer, layerEnemy, false);
  
    }

    private IEnumerator Parpadeo()
    {
        float intervalo = 0.1f;
        
        while (recibiendoDano)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(intervalo);
        }

        spriteRenderer.enabled = true;
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

            LayerMask mascaraCheck = capaTecho != 0 ? capaTecho : capaSuelo;

            Vector2 puntoArriba = new Vector2(
                transform.position.x + colliderOffsetNormal.x,
                transform.position.y + colliderOffsetNormal.y + colliderSizeNormal.y * 0.5f - 0.05f
            );

            bool hayObstaculo = Physics2D.OverlapBox(puntoArriba, 
                new Vector2(colliderSizeNormal.x * 0.8f, 0.1f), 0f, mascaraCheck);

            if (!hayObstaculo)
            {
                agachado = false;
                col.size   = colliderSizeNormal;
                col.offset = colliderOffsetNormal;
            }
  
        }

        if (!agachado)
        {
            col.size   = colliderSizeNormal;
            col.offset = colliderOffsetNormal;
        }
    }
    public void Movimiento()
    {

        if (muerto) return;

        float velActual = agachado ? velocidadAgachado : velocidad;
        float inputX = inputMovimiento.x;

        
        if (inputX != 0 && enSuelo && !recibiendoDano && !agachado &&!atacando && !dasheando )
        {
            cont += Time.deltaTime;
            if (cont >= timeByStep)
            {
                cont = 0f;
                if (!step1)
                {
                    soundController.PlayMov1();
                    step1 = true;
                }
                else
                {
                    soundController.PlayMov2();
                    step1 = false;
                }
            }
        }

        animator.SetFloat("movement", Mathf.Abs(inputX));
        
        if (!muerto)
        {
            if (inputX < 0) transform.localScale = new Vector3(-1, 1, 1);
            if (inputX > 0) transform.localScale = new Vector3(1, 1, 1);
        }


           
        if (tocandoPared && !enSuelo)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return; 
        }


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
        animator.SetBool("enPared",enPared);
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
        rb.linearVelocity = Vector2.zero;

    }


    public void Atacando()
    {
        soundController.PlayAtacar();
        atacando=true;
        comboRegistrado=false;
        animator.SetTrigger(comboContador.ToString());
    }

    public void IniciarCombo()
    {
        if (comboRegistrado && comboContador < 1)
        {
            comboContador++;
            Atacando();
        }
        else
        {
            
            DesactivaAtaque();
        }
    }

    public void DesactivaAtaque()
    {

        atacando=false;
        comboContador = 0;
        comboRegistrado = false;
    }

    void OnDrawGizmos()
    {
        if (col == null) return;

        // Raycasts de suelo
        float mitadGizmo = tamañoDetector.x * 0.5f - margenRaycastLateral;
        float baseYGizmo = transform.position.y + col.offset.y - col.size.y * 0.5f;
        float distanciaGizmo = offsetYDetector + 0.05f;

        Gizmos.color = enSuelo ? Color.green : Color.red;
        Gizmos.DrawLine(new Vector2(transform.position.x + col.offset.x - mitadGizmo, baseYGizmo),
                        new Vector2(transform.position.x + col.offset.x - mitadGizmo, baseYGizmo - distanciaGizmo));
        Gizmos.DrawLine(new Vector2(transform.position.x + col.offset.x,              baseYGizmo),
                        new Vector2(transform.position.x + col.offset.x,              baseYGizmo - distanciaGizmo));
        Gizmos.DrawLine(new Vector2(transform.position.x + col.offset.x + mitadGizmo, baseYGizmo),
                        new Vector2(transform.position.x + col.offset.x + mitadGizmo, baseYGizmo - distanciaGizmo));

        // Detector pared
        float dirección = transform.localScale.x;
        Vector2 puntoPared = new Vector2(
            transform.position.x + col.offset.x + (dirección * offsetXDetectorPared),
            transform.position.y + col.offset.y
        );
        Gizmos.color = tocandoPared ? Color.blue : Color.cyan;
        Gizmos.DrawWireCube(puntoPared, tamañoDetectorPared);
    }
    
}
