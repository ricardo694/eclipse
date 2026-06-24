using UnityEngine;
using System.Collections;
using Fusion;

public struct EclipseraInput : INetworkInput
{
    public NetworkButtons Buttons;
    public float MoveX;
}

public static class EBtn
{
    public const int JUMP   = 0;
    public const int DASH   = 1;
    public const int ATTACK = 2;
    public const int CROUCH = 3;
}

public class PlayerControllerMultijugador : NetworkBehaviour
{
    [Header("Movimiento")]
    public float velocidad    = 5f;
    public bool  step1        = false;
    public float timeByStep   = 0.4f;
    float cont = 0f;
    private float _localMoveInput = 0f;

    [Header("Salto")]
    public float fuerzaSalto     = 5;
    public int   maxSaltos       = 1;
    public float coyoteTime      = 0.12f;
    public float jumpBufferTime  = 0.12f;
    public float multiplicadorCorte = 0.65f;  
    private Vector2 colliderSizeSalto;
    private Vector2 colliderOffsetSalto;

    [Header("Gravedad")]
    public float fallGravityMultiplier = 1.5f;
    public float maxFallSpeed          = 6f;
    public float hangThreshold         = 1.8f;
    public float hangGravity           = 0.6f;

    [Header("Wall Jump / Wall Slide")]
    public float velocidadWallSlide   = 0.8f;
    public float fuerzaWallJumpX      = 4f;
    public float fuerzaWallJumpY      = 5f;
    public float duracionPostWallJump = 0.15f;

    [Header("Detección de Suelo")]
    public Vector2 tamañoDetector   = new Vector2(0.8f, 0.05f);
    public float   offsetYDetector  = 0.02f;
    public float   margenRaycastLateral  = 0f; 

    [Header("Detección de Pared")]
    public Vector2 tamañoDetectorPared  = new Vector2(0.05f, 0.6f);
    public float   offsetXDetectorPared = 0.5f;

    [Header("Vida")]
    public int vidaMaxima = 3;

    [Header("Daño")]
    public float fuerzaRebote      = 0.2f;
    public float duracionInmunidad = 1f;
    public float duracionAnimDano  = 0.5f;

    [Header("Ataque")]
    private int  comboContador   = 0;
    private bool comboRegistrado = false;

    [Header("Dash")]
    public float fuerzaDash    = 14f;
    public float duracionDash  = 0.15f;
    public float cooldownDash  = 1f;
    private float timerDash;
    private float timerCooldown;
    private float direccionDash;

    [Header("Agacharse")]
    public float     velocidadAgachado = 2.5f;
    public float     radioCheckArriba  = 0.2f;
    public LayerMask capaTecho;
    private Vector2 colliderSizeNormal;
    private Vector2 colliderOffsetNormal;
    private Vector2 colliderSizeAgachado;
    private Vector2 colliderOffsetAgachado;

    [Header("Componentes")]
    public Animator              animator;
    public BoxCollider2D         col;
    public LayerMask             capaSuelo;
    public PlayerSoundController soundController;

    [Header("Hitbox")]
    public AttackHitboxMultijugador hitbox;

    // ── Variables sincronizadas con Fusion ───────────────────────────────────
    [Networked] public  int                  vida            { get; set; }
    [Networked] public  bool                 muerto          { get; set; }
    [Networked] public  int                  PlayerIndex     { get; set; }
    [Networked] public  NetworkString<_64>   NombreJugador   { get; set; }
    [Networked] private bool                 enSuelo         { get; set; }
    [Networked] private bool                 tocandoPared    { get; set; }

    [Networked] private bool  enPared          { get; set; }
    [Networked] private float direccionPared    { get; set; }
    [Networked] private float timerPostWallJump { get; set; }

    [Networked] private bool                 atacando        { get; set; }
    [Networked] private bool                 dasheando       { get; set; }
    [Networked] private bool                 dashDisponible  { get; set; }
    [Networked] private bool                 agachado        { get; set; }
    [Networked] private bool                 recibiendoDano  { get; set; }
    [Networked] private int            saltosRestantes   { get; set; }
    [Networked] private float          coyoteCounter     { get; set; }
    [Networked] private float          jumpBufferCounter { get; set; }
    [Networked] private NetworkButtons PreviousButtons   { get; set; }
    [Networked] private bool esSaltando { get; set; }
    [Networked] private NetworkBool          facingRight     { get; set; }
    [Networked] private int                  comboTrigger    { get; set; }
    [Networked] private NetworkBool          triggerConsumed { get; set; }
    [Networked] private float                moveInput       { get; set; }

    // ── Variables locales ────────────────────────────────────────────────────
    private Rigidbody2D    rb;
    private SpriteRenderer spriteRenderer;

    public override void Spawned()
    {
        rb              = GetComponent<Rigidbody2D>();
        animator        = GetComponent<Animator>();
        spriteRenderer  = GetComponent<SpriteRenderer>();
        comboTrigger    = -1;
        triggerConsumed = false;

        transform.position = new Vector3(
            transform.position.x, transform.position.y, 0f);

        colliderSizeNormal     = col.size;
        colliderOffsetNormal   = col.offset;
        colliderSizeAgachado   = new Vector2(col.size.x, col.size.y * 0.5f);
        colliderOffsetAgachado = new Vector2(
            col.offset.x, col.offset.y - col.size.y * 0.25f);
        colliderSizeSalto      = new Vector2(col.size.x, col.size.y * 0.8f);
        colliderOffsetSalto    = new Vector2(
            col.offset.x, col.offset.y + col.size.y * 0.1f);

        vida            = vidaMaxima;
        muerto          = false;
        dashDisponible  = true;
        saltosRestantes = maxSaltos;
        facingRight     = PlayerIndex == 0;

        
    }

    public override void FixedUpdateNetwork()
    {
        if (transform.position.z != 0f)
            transform.position = new Vector3(
                transform.position.x, transform.position.y, 0f);

        if (muerto) return;

        if (Object.HasInputAuthority && NombreJugador.ToString() == "" && PlayerIndex >= 0)
        {
            string nombre = LoginSystem.Username;
            NombreJugador = string.IsNullOrEmpty(nombre) ? "Jugador" : nombre;
            Debug.Log($"[Player] Nombre asignado: {NombreJugador} para PlayerIndex {PlayerIndex}");
        }

        DetectarSuelo();
        DetectarPared();

        // ── Post wall jump / Wall cling ─────────────────────────────────
        if (timerPostWallJump > 0f)
            timerPostWallJump -= Runner.DeltaTime;

        enPared = tocandoPared && !enSuelo && timerPostWallJump <= 0f;
        if (enPared) direccionPared = facingRight ? 1f : -1f;

        // ── Coyote time ───────────────────────────────────────────────
        if (enSuelo)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Runner.DeltaTime;

        // ── Gravedad ─────────────────────────────────────────────────
        if (dasheando)
        {
            // el dash maneja su propia gravedad
        }
        else if (enPared && rb.linearVelocity.y < 0f)
        {
            rb.gravityScale = 1f;
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                Mathf.Max(rb.linearVelocity.y, -velocidadWallSlide));
        }
        else if (!enSuelo && rb.linearVelocity.y < 0f)
        {
            rb.gravityScale = fallGravityMultiplier;
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
        }
        else if (esSaltando && !enSuelo && Mathf.Abs(rb.linearVelocity.y) < hangThreshold)
        {
            rb.gravityScale = hangGravity;
        }
        else
        {
            rb.gravityScale = 1f;
        }

        if (enSuelo) esSaltando = false;

        if (enPared)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (dasheando)
        {
            timerDash -= Runner.DeltaTime;
            if (timerDash <= 0f) TerminarDash();
        }
        if (!dashDisponible)
        {
            timerCooldown -= Runner.DeltaTime;
            if (timerCooldown <= 0f) dashDisponible = true;
        }

        if (GetInput(out EclipseraInput input))
        {
            // ── Jump buffer ───────────────────────────────────────────
            bool saltoPresionado = input.Buttons.WasPressed(PreviousButtons, EBtn.JUMP);

            if (saltoPresionado)
                jumpBufferCounter = jumpBufferTime;
            else
                jumpBufferCounter -= Runner.DeltaTime;

            if (!atacando && !dasheando)
            {
                Movimiento(input);

                if (enSuelo) saltosRestantes = maxSaltos;

                if (!recibiendoDano && jumpBufferCounter > 0f)
                {
                    // Wall jump — prioridad sobre el salto normal
                    if (enPared)
                    {
                        soundController.PlaySaltar();
                        rb.linearVelocity = new Vector2(-direccionPared * fuerzaWallJumpX, fuerzaWallJumpY);
                        jumpBufferCounter = 0f;
                        esSaltando = true;
                        timerPostWallJump = duracionPostWallJump;
                    }
                    else
                    {
                        bool esPrimerSalto = saltosRestantes == maxSaltos;
                        bool puedeSaltar = saltosRestantes > 0 &&
                                            (!esPrimerSalto || coyoteCounter > 0f) &&
                                            !agachado;

                        if (puedeSaltar)
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
            }

            if (input.Buttons.IsSet(EBtn.DASH) &&
                dashDisponible && !dasheando && !atacando && !recibiendoDano && !agachado)
                IniciarDash();

            if (input.Buttons.IsSet(EBtn.ATTACK) && !dasheando && enSuelo)
            {
                if (!atacando)
                {
                    comboContador = 0;
                    Atacando();
                }
                else if (atacando && !comboRegistrado && comboContador < 1)
                    comboRegistrado = true;
            }

            ManejarAgacharse(input.Buttons.IsSet(EBtn.CROUCH));

            // ── Salto variable (corte al soltar el botón) ──────────────
            bool saltoMantenido = input.Buttons.IsSet(EBtn.JUMP);
            if (!saltoMantenido && rb.linearVelocity.y > 0f && !dasheando && !enPared)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * multiplicadorCorte);

            PreviousButtons = input.Buttons;
        }

        hitbox = GetComponentInChildren<AttackHitboxMultijugador>();
    }
    public override void Render()
    {
        transform.localScale = new Vector3(facingRight ? 1f : -1f, 1f, 1f);

        if (comboTrigger >= 0 && !triggerConsumed)
        {
            animator.SetTrigger(comboTrigger.ToString());
            triggerConsumed = true;
        }

        if (triggerConsumed && comboTrigger >= 0)
        {
            comboTrigger    = -1;
            triggerConsumed = false;
        }

        if (Object.HasInputAuthority)
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null)
            {
                _localMoveInput = 0f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  _localMoveInput = 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) _localMoveInput = 1f;
            }
            animator.SetFloat("movement", _localMoveInput);
        }
        else
        {
            _localMoveInput = Mathf.Lerp(_localMoveInput, moveInput, Time.deltaTime * 10f);
            animator.SetFloat("movement", _localMoveInput);
        }

        animator.SetBool("ensuelo",        enSuelo);
        animator.SetBool("atacando",       atacando);
        animator.SetBool("recibiendoDano", recibiendoDano);
        animator.SetBool("dasheando",      dasheando);
        animator.SetBool("agachado",       agachado);
        animator.SetBool("muerto",         muerto);
        animator.SetBool("enPared",        enPared);
    }

    void DetectarSuelo()
    {
        float mitad     = tamañoDetector.x * 0.5f - margenRaycastLateral;
        float baseY      = transform.position.y + col.offset.y - col.size.y * 0.5f;
        float distancia  = offsetYDetector + 0.05f;

        bool hitIzq    = Physics2D.Raycast(
            new Vector2(transform.position.x + col.offset.x - mitad, baseY),
            Vector2.down, distancia, capaSuelo);
        bool hitCentro = Physics2D.Raycast(
            new Vector2(transform.position.x + col.offset.x, baseY),
            Vector2.down, distancia, capaSuelo);
        bool hitDer    = Physics2D.Raycast(
            new Vector2(transform.position.x + col.offset.x + mitad, baseY),
            Vector2.down, distancia, capaSuelo);

        enSuelo = hitIzq || hitCentro || hitDer;
    }
    void DetectarPared()
    {
        float dir = facingRight ? 1f : -1f;
        RaycastHit2D hitPared = Physics2D.BoxCast(
            new Vector2(transform.position.x + col.offset.x, transform.position.y + col.offset.y),
            new Vector2(0.05f, tamañoDetectorPared.y),
            0f,
            new Vector2(dir, 0f),
            offsetXDetectorPared,
            capaSuelo);

        tocandoPared = hitPared.collider != null && Vector2.Angle(hitPared.normal, Vector2.up) > 45f;
    }
    public void Movimiento(EclipseraInput input)
    {
        if (muerto) return;

        float velActual = agachado ? velocidadAgachado : velocidad;
        float inputX    = input.MoveX;

        if (inputX != 0 && enSuelo && !recibiendoDano && !agachado && !atacando && !dasheando)
        {
            cont += Runner.DeltaTime;
            if (cont >= timeByStep)
            {
                cont = 0f;
                if (!step1) { soundController.PlayMov1(); step1 = true; }
                else        { soundController.PlayMov2(); step1 = false; }
            }
        }

        moveInput = Mathf.Abs(inputX);

        if      (inputX > 0) facingRight = true;
        else if (inputX < 0) facingRight = false;

        if (enPared)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (!recibiendoDano && !agachado)
            rb.linearVelocity = new Vector2(inputX * velActual, rb.linearVelocity.y);
        else if (agachado)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    public void IniciarDash()
    {
        dasheando      = true;
        dashDisponible = false;
        timerDash      = duracionDash;
        timerCooldown  = cooldownDash;
        direccionDash  = facingRight ? 1f : -1f;

        rb.gravityScale   = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(direccionDash * fuerzaDash, 0f), ForceMode2D.Impulse);
    }

    public void TerminarDash()
    {
        dasheando         = false;
        rb.gravityScale   = 1f;
        rb.linearVelocity = Vector2.zero;
    }

    public void RecibeDano(Vector2 direccion, int cantDano)
    {
        if (!Object.HasStateAuthority) return;
        if (recibiendoDano) return;

        Debug.Log($"RecibeDano aplicado, vida antes: {vida}, daño: {cantDano}");
        recibiendoDano = true;
        vida -= cantDano;
        Debug.Log($"Vida después: {vida}");

        if (vida <= 0)
        {
            vida   = 0;
            muerto = true;
            RPC_NotificarMuerte();
        }
        else
        {
            Vector2 rebote = new Vector2(
                transform.position.x - direccion.x, 0.2f).normalized;
            rb.AddForce(rebote * fuerzaRebote, ForceMode2D.Impulse);
            StartCoroutine(RecuperarseDeDano());
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_NotificarMuerte()
    {
        RoundManager.Instance?.OnPlayerDied(Object.InputAuthority);
        Debug.Log($"Jugador {Object.InputAuthority} murió");
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RecibirDano(Vector2 direccion, int cantDano)
    {
        Debug.Log($"RPC_RecibirDano recibido, vida actual: {vida}");
        RecibeDano(direccion, cantDano);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ResetearEstado()
    {
        muerto            = false;
        recibiendoDano    = false;
        atacando          = false;
        dasheando         = false;
        agachado          = false;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale   = 1f;
        animator.Play("idle1");
    }

    private IEnumerator RecuperarseDeDano()
    {
        atacando        = false;
        comboContador   = 0;
        comboRegistrado = false;
        animator.ResetTrigger("0");
        animator.ResetTrigger("1");
        animator.Play("idle1");

        int layerPlayer = gameObject.layer;
        int layerEnemy  = LayerMask.NameToLayer("Enemy");
        Physics2D.IgnoreLayerCollision(layerPlayer, layerEnemy, true);

        StartCoroutine(Parpadeo());

        yield return new WaitForSeconds(duracionAnimDano);
        recibiendoDano    = false;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(duracionInmunidad - duracionAnimDano);
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

    void ManejarAgacharse(bool quiereAgacharse)
    {
        bool debeAgacharse = quiereAgacharse && enSuelo;

        if (debeAgacharse && !agachado)
        {
            agachado   = true;
            col.size   = colliderSizeAgachado;
            col.offset = colliderOffsetAgachado;
        }

        if (!debeAgacharse && agachado)
        {
            Vector2 puntoArriba = (Vector2)transform.position +
                Vector2.up * (colliderSizeNormal.y * 0.5f);
            bool hayEspacio = !Physics2D.OverlapCircle(
                puntoArriba, radioCheckArriba, capaTecho);

            if (hayEspacio)
                agachado = false;
        }

        if (!agachado)
        {
            if (!enSuelo) { col.size = colliderSizeSalto;  col.offset = colliderOffsetSalto; }
            else          { col.size = colliderSizeNormal; col.offset = colliderOffsetNormal; }
        }
    }

    public void Atacando()
    {
        soundController.PlayAtacar();
        atacando        = true;
        comboRegistrado = false;
        comboTrigger    = comboContador;
    }

    public void IniciarCombo()
    {
        if (comboRegistrado && comboContador < 1)
        {
            comboContador++;
            float dir = facingRight ? 1f : -1f;
            rb.AddForce(new Vector2(dir * 6f, 0f), ForceMode2D.Impulse);
            Atacando();
        }
        else
        {
            DesactivaAtaque();
        }
    }

    public void DesactivaAtaque()
    {
        atacando        = false;
        comboContador   = 0;
        comboRegistrado = false;
    }

    public void DesactivarDano()
    {
        rb.linearVelocity = Vector2.zero;
    }

    public void Animaciones()
    {
        animator.SetBool("ensuelo",        enSuelo);
        animator.SetBool("atacando",       atacando);
        animator.SetBool("recibiendoDano", recibiendoDano);
        animator.SetBool("dasheando",      dasheando);
        animator.SetBool("agachado",       agachado);
        animator.SetBool("muerto",         muerto);
    }

    void OnDrawGizmos()
    {
        if (col == null) return;
        if (Object == null || !Object.IsValid) return;

        float mitadGizmo    = tamañoDetector.x * 0.5f - margenRaycastLateral;
        float baseYGizmo     = transform.position.y + col.offset.y - col.size.y * 0.5f;
        float distanciaGizmo = offsetYDetector + 0.05f;

        Gizmos.color = enSuelo ? Color.green : Color.red;
        Gizmos.DrawLine(new Vector2(transform.position.x + col.offset.x - mitadGizmo, baseYGizmo),
                        new Vector2(transform.position.x + col.offset.x - mitadGizmo, baseYGizmo - distanciaGizmo));
        Gizmos.DrawLine(new Vector2(transform.position.x + col.offset.x, baseYGizmo),
                        new Vector2(transform.position.x + col.offset.x, baseYGizmo - distanciaGizmo));
        Gizmos.DrawLine(new Vector2(transform.position.x + col.offset.x + mitadGizmo, baseYGizmo),
                        new Vector2(transform.position.x + col.offset.x + mitadGizmo, baseYGizmo - distanciaGizmo));

        float dir = facingRight ? 1f : -1f;
        Vector2 puntoPared = new Vector2(
            transform.position.x + col.offset.x + (dir * offsetXDetectorPared),
            transform.position.y + col.offset.y);
        Gizmos.color = tocandoPared ? Color.blue : Color.cyan;
        Gizmos.DrawWireCube(puntoPared, tamañoDetectorPared);
    }

    public void ActivarHitbox()  { hitbox?.ActivarHitbox();   }
    public void DesactivarHitbox() { hitbox?.DesactivarHitbox(); }
}