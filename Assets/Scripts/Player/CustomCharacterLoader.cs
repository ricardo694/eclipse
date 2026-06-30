using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EclipseraGlitch;
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(PlayerController))]
public class CustomCharacterLoader : MonoBehaviour
{
    [Header("Config")]
    public float fps = 8f;
    public bool EstaActivo => _spritesPorAnim != null;

    private const int ANIM_IDLE   = 0;
    private const int ANIM_RUN    = 1;
    private const int ANIM_JUMP   = 2;
    private const int ANIM_ATTACK = 3;
    private const int ANIM_CROUCH = 4;
    private const int ANIM_DAMAGE = 5;

    // Componentes
    private SpriteRenderer  _sr;
    private BoxCollider2D   _col;
    private PlayerController _pc;
    private Animator        _animator;
    private AttackHitbox _attackHitboxObj;
    
    [Header("Hitbox de ataque")]
    public GameObject hitboxAtaqueObj;
    private BoxCollider2D _hitboxAtaqueCol; 

    // Datos del personaje
    private CharacterData _data;
    private List<List<Sprite>> _spritesPorAnim; 

    // Estado de animacion
  
    private int   _animActual  = ANIM_IDLE;
    private int   _frameActual = 0;
    private float _timer       = 0f;
    private bool _animForzada = false;
    private float _timerAnimForzada = 0f;
    private const float PPU = 85f;

    // Hitbox original para restaurar si es necesario
    private Vector2 _colSizeOriginal;
    private Vector2 _colOffsetOriginal;

    // ── Ciclo de vida ────────────────────────────────────────────────

    void Awake()
    {
        _sr       = GetComponent<SpriteRenderer>();
        _col      = GetComponent<BoxCollider2D>();
        _pc       = GetComponent<PlayerController>();
        _animator = GetComponent<Animator>();

        _colSizeOriginal   = _col.size;
        _colOffsetOriginal = _col.offset;

        _attackHitboxObj = GetComponentInChildren<AttackHitbox>(true);
        if (_attackHitboxObj != null)
            _attackHitboxObj.gameObject.SetActive(false);
    }

    void Start()
    {
        _data = CharacterDataHolder.Instance?.DatosActuales;

        if (_data == null || _data.todasLasAnimaciones == null ||
            _data.todasLasAnimaciones.Count == 0)
        {
            Debug.Log("[CustomLoader] No hay personaje custom. Usando personaje por defecto.");
            return;
        }

        // Desactivar el Animator — nosotros manejamos los sprites
        if (_animator != null)
            _animator.enabled = false;

        ConvertirTexturasASprites();
        AplicarHitboxCuerpo();

        Debug.Log("[CustomLoader] Personaje custom cargado correctamente.");

         // Guardar referencia al collider de ataque
        if (hitboxAtaqueObj != null)
        {
            _hitboxAtaqueCol = hitboxAtaqueObj.GetComponent<BoxCollider2D>();
            hitboxAtaqueObj.SetActive(true); // siempre activo como el por defecto
            if (_hitboxAtaqueCol != null)
                _hitboxAtaqueCol.enabled = false; // pero collider desactivado
        }
    }

    void Update()
    {
        if (_spritesPorAnim == null) return;

        ActualizarEstado();
        CiclarFrames();
    }

    // ── Conversion Texture2D → Sprite ────────────────────────────────
    Texture2D VoltearTextura(Texture2D original)
    {
        Texture2D volteada = new Texture2D(original.width, original.height, TextureFormat.RGBA32, false);
        volteada.filterMode = FilterMode.Point;
        for (int y = 0; y < original.height; y++)
        for (int x = 0; x < original.width; x++)
            volteada.SetPixel(x, original.height - 1 - y, original.GetPixel(x, y));
        volteada.Apply();
        return volteada;
    }
    void ConvertirTexturasASprites()
    {
        _spritesPorAnim = new List<List<Sprite>>();

        for (int a = 0; a < _data.todasLasAnimaciones.Count; a++)
        {
            var listaSprites = new List<Sprite>();
            var listaFrames  = _data.todasLasAnimaciones[a];

            for (int f = 0; f < listaFrames.Count; f++)
            {
                Texture2D tex = listaFrames[f];
                if (tex == null) continue;

                // Crear textura 
                Texture2D texVolteada = VoltearTextura(tex);
                Sprite sp = Sprite.Create(
                    texVolteada,
                    new Rect(0, 0, texVolteada.width, texVolteada.height),
                    new Vector2(0.5f, 0.5f),
                    PPU 
                );

                listaSprites.Add(sp);
            }

            _spritesPorAnim.Add(listaSprites);
        }
    }

    // ── Hitbox ───────────────────────────────────────────────────────

    // void AplicarHitboxCuerpo()
    // {
    //     if (_data?.bodyHitbox == null) return;


    //     float escala = 128f / 128f; 

    //     _col.size   = new Vector2(
    //         _data.bodyHitbox.width  * escala,
    //         _data.bodyHitbox.height * escala);
    //     _col.offset = new Vector2(
    //         _data.bodyHitbox.offsetX * escala,
    //         _data.bodyHitbox.offsetY * escala);

    //     Debug.Log($"[CustomLoader] Hitbox cuerpo: size={_col.size} offset={_col.offset}");
    // }
    void AplicarHitboxCuerpo()
    {
        if (_data?.bodyHitbox == null) return;

        // El sprite mide 128px / PPU(85) = ~1.5 unidades de Unity
        float tamañoSpriteEnUnidades = 128f / PPU;

        _col.size = new Vector2(
            _data.bodyHitbox.width  * tamañoSpriteEnUnidades,
            _data.bodyHitbox.height * tamañoSpriteEnUnidades
        );
        _col.offset = new Vector2(
            _data.bodyHitbox.offsetX * tamañoSpriteEnUnidades,
            _data.bodyHitbox.offsetY * tamañoSpriteEnUnidades
        );

        Debug.Log($"[Hitbox] size={_col.size} offset={_col.offset}");
    }
    // Llama esto desde PlayerController cuando activa el ataque
    public void AplicarHitboxAtaque(int frameAtaque)
    {
        if (_data?.attackHitboxPorFrame == null) return;
        if (frameAtaque < 0 || frameAtaque >= _data.attackHitboxPorFrame.Count) return;

        HitboxData hd = _data.attackHitboxPorFrame[frameAtaque];
        if (hd == null) return;

        float escala = 128f / 128f;
        _col.size   = new Vector2(hd.width  * escala, hd.height * escala);
        _col.offset = new Vector2(hd.offsetX * escala, hd.offsetY * escala);
    }

    public void RestaurarHitboxCuerpo()
    {
        if (_data == null) // sin personaje custom, restaurar valores originales
        {
            _col.size   = _colSizeOriginal;
            _col.offset = _colOffsetOriginal;
            return;
        }
        AplicarHitboxCuerpo();
    }

    // ── Estado → animacion ───────────────────────────────────────────

    void ActualizarEstado()
    {
        // Si hay animación forzada activa, esperar que termine
        if (_animForzada)
        {
            _timerAnimForzada -= Time.deltaTime;
            if (_timerAnimForzada <= 0f)
                _animForzada = false;
            return; // no sobreescribir
        }

        int nuevaAnim = DetectarAnimacion();
        if (nuevaAnim != _animActual)
        {
            _animActual  = nuevaAnim;
            _frameActual = 0;
            _timer       = 0f;
        }
    }


    int DetectarAnimacion()
    {
        if (_pc.EstaMuerto)        return ANIM_DAMAGE;
        if (_pc.EstaRecibiendoDano) return ANIM_DAMAGE;
        if (_pc.EstaAgachado)      return ANIM_CROUCH;
        if (_pc.EstaAtacando)      return ANIM_ATTACK;

        Rigidbody2D rb = _pc.GetComponent<Rigidbody2D>();
        if (rb == null) return ANIM_IDLE;

        float velX = Mathf.Abs(rb.linearVelocity.x);
        float velY = rb.linearVelocity.y;
        bool enSuelo = IsGrounded(rb);

        if (!enSuelo) return ANIM_JUMP;
        if (velX > 0.1f) return ANIM_RUN;
        return ANIM_IDLE;
    }

    bool IsGrounded(Rigidbody2D rb)
    {
        // Raycast rapido hacia abajo
        return Physics2D.Raycast(
            transform.position,
            Vector2.down,
            _col.size.y / 2f + 0.1f,
            _pc.capaSuelo);
    }

    // ── Ciclar frames ────────────────────────────────────────────────

    void CiclarFrames()
    {
        if (_spritesPorAnim == null) return;
        if (_animActual >= _spritesPorAnim.Count) return;

        List<Sprite> frames = _spritesPorAnim[_animActual];
        if (frames == null || frames.Count == 0) return;

        _timer += Time.deltaTime;
        if (_timer >= 1f / fps)
        {
            _timer = 0f;
            _frameActual = (_frameActual + 1) % frames.Count;
            _sr.sprite   = frames[_frameActual];
        }
    }

    // ── API publica para PlayerController ────────────────────────────

    /// Llama esto desde PlayerController.Atacando()
    void ForzarAnimacion(int animIdx, float duracion = 0.5f)
    {
        _animActual       = animIdx;
        _frameActual      = 0;
        _timer            = 0f;
        _animForzada      = true;
        _timerAnimForzada = duracion;
    }
    public void NotificarAtaque()
    {
        ForzarAnimacion(ANIM_ATTACK, 0.25f);
        if (_hitboxAtaqueCol != null)
            StartCoroutine(ActivarHitboxTemporal());
    }
   
    public void NotificarFinAtaque()
    {
        if (_data == null) return;
        if (_hitboxAtaqueCol != null)
            _hitboxAtaqueCol.enabled = false;
    }
    public void NotificarDano()
    {
        ForzarAnimacion(ANIM_DAMAGE);
    }

    
    public void NotificarAgacharse(bool agachado)
    {
        if (agachado) ForzarAnimacion(ANIM_CROUCH);
    }

    IEnumerator ActivarHitboxTemporal()
    {
        _hitboxAtaqueCol.enabled = true;
        yield return new WaitForSeconds(0.15f);
        _hitboxAtaqueCol.enabled = false;
    }
        // ── Debug ────────────────────────────────────────────────────────

    void OnDrawGizmos()
    {
        if (_data?.bodyHitbox == null) return;
        if (_col == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            transform.position + (Vector3)_col.offset,
            _col.size);
    }
}