using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class PreviewPersonaje : MonoBehaviour
{
    [Header("Vista personaje")]
    public RawImage rawImagePersonaje;
    public RawImage rawImageFondoPreview;
    public TMP_Text txtAnimActual;

    [Header("Tabs animacion")]
    public Button btnTabIdle;
    public Button btnTabCorrer;
    public Button btnTabAtacar;
    public Button btnTabAgacharse;

    [Header("Detalles")]
    public TMP_InputField inputNombre;
    public TMP_Text txtPixelesPintados;

    [Header("Miniaturas")]
    public RawImage miniIdle;
    public RawImage miniCorrer;
    public RawImage miniAtacar;
    public RawImage miniAgacharse;

    [Header("Acciones")]
    public Button btnGuardar;
    public Button btnDescartar;
    public Button btnVolver;

    [Header("Hitbox")]
    public HitboxEditor hitboxEditor;

    [Header("Guardar")]
    public PanelGuardarPersonaje panelGuardar;

    // FPS de reproduccion de animacion
    private const float FPS = 8f;

    // indices en todasLasAnimaciones
    private const int ANIM_IDLE      = 0;
    private const int ANIM_RUN       = 1;
    private const int ANIM_JUMP      = 2;
    private const int ANIM_ATTACK    = 3;
    private const int ANIM_CROUCH    = 4;
    private const int ANIM_DAMAGE    = 5;

    private CharacterData _datos;
    private int _animActual = ANIM_IDLE;
    private int _frameActual = 0;
    private float _timer = 0f;

    // colores de tab
    private readonly Color COLOR_ACTIVO   = new Color(0.36f, 0.31f, 0.94f);
    private readonly Color COLOR_INACTIVO = new Color(0.16f, 0.16f, 0.27f);

    void Start()
    {
        _datos = CharacterDataHolder.Instance?.DatosActuales;

        if (_datos == null)
        {
            Debug.LogWarning("[Preview] No hay datos de personaje.");
            return;
        }

        InicializarFondo();
        InicializarMiniaturas();
        InicializarUI();
        ConectarBotones();
        CambiarAnimacion(ANIM_IDLE);
    }

    void Update()
    {
        if (_datos == null) return;

        List<Texture2D> frames = ObtenerFrames(_animActual);
        if (frames == null || frames.Count == 0) return;

        _timer += Time.deltaTime;
        if (_timer >= 1f / FPS)
        {
            _timer = 0f;
            _frameActual = (_frameActual + 1) % frames.Count;
            MostrarFrame(frames[_frameActual]);
        }
    }

    // ── Inicializacion ───────────────────

    void InicializarFondo()
    {
        if (rawImageFondoPreview == null) return;

        Texture2D fondo = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        fondo.filterMode = FilterMode.Point;
        Color grisClaro  = new Color(0.78f, 0.78f, 0.78f);
        Color grisOscuro = new Color(0.55f, 0.55f, 0.55f);

        for (int y = 0; y < 128; y++)
        for (int x = 0; x < 128; x++)
        {
            bool par = ((x / 4) + (y / 4)) % 2 == 0;
            fondo.SetPixel(x, y, par ? grisClaro : grisOscuro);
        }
        fondo.Apply();
        rawImageFondoPreview.texture = fondo;
    }

    void InicializarMiniaturas()
    {
        // Mostrar primer frame de cada animacion en su miniatura
        AsignarMiniatura(miniIdle,       ANIM_IDLE);
        AsignarMiniatura(miniCorrer,     ANIM_RUN);
        AsignarMiniatura(miniAtacar,     ANIM_ATTACK);
        AsignarMiniatura(miniAgacharse,  ANIM_CROUCH);
    }

    void AsignarMiniatura(RawImage img, int animIdx)
    {
        if (img == null) return;
        List<Texture2D> frames = ObtenerFrames(animIdx);
        if (frames != null && frames.Count > 0 && frames[0] != null)
        {
            img.texture = frames[0];
            img.uvRect = new Rect(0, 1, 1, -1);
        }
    }

    void InicializarUI()
    {
        if (inputNombre != null)
            inputNombre.text = _datos.nombrePersonaje;

        if (txtPixelesPintados != null)
        {
            // Contar pixeles del primer frame idle
            int pixeles = ContarPixeles(ANIM_IDLE);
            txtPixelesPintados.text = $"Pixels painted: {pixeles}";
            _datos.pixelesPintados = pixeles;
        }
    }

    void ConectarBotones()
    {
        btnTabIdle?.onClick.AddListener(()      => CambiarAnimacion(ANIM_IDLE));
        btnTabCorrer?.onClick.AddListener(()    => CambiarAnimacion(ANIM_RUN));
        btnTabAtacar?.onClick.AddListener(()    => CambiarAnimacion(ANIM_ATTACK));
        btnTabAgacharse?.onClick.AddListener(() => CambiarAnimacion(ANIM_CROUCH));

        // Miniaturas tambien cambian la animacion al hacer clic
        miniIdle?.GetComponent<Button>()?.onClick.AddListener(()      => CambiarAnimacion(ANIM_IDLE));
        miniCorrer?.GetComponent<Button>()?.onClick.AddListener(()    => CambiarAnimacion(ANIM_RUN));
        miniAtacar?.GetComponent<Button>()?.onClick.AddListener(()    => CambiarAnimacion(ANIM_ATTACK));
        miniAgacharse?.GetComponent<Button>()?.onClick.AddListener(() => CambiarAnimacion(ANIM_CROUCH));

        btnVolver?.onClick.AddListener(VolverAlEditor);
        btnDescartar?.onClick.AddListener(Descartar);
        btnGuardar?.onClick.AddListener(AvanzarAHitbox);
    }

    // ── Animacion ────────────────────────

    void CambiarAnimacion(int idx)
    {
        _animActual  = idx;
        _frameActual = 0;
        _timer       = 0f;

        // Mostrar primer frame inmediatamente
        List<Texture2D> frames = ObtenerFrames(idx);
        if (frames != null && frames.Count > 0)
            MostrarFrame(frames[0]);

        // Nombre de la animacion en pantalla
        if (txtAnimActual != null)
            txtAnimActual.text = NombreAnim(idx);

        ActualizarColorTabs(idx);
    }

    void MostrarFrame(Texture2D frame)
    {
        if (rawImagePersonaje != null && frame != null)
        {
            rawImagePersonaje.texture = frame;
            rawImagePersonaje.uvRect = new Rect(0, 1, 1, -1);
        }
    }

    // ── Helpers ─────────────────────────

    List<Texture2D> ObtenerFrames(int animIdx)
    {
        if (_datos?.todasLasAnimaciones == null) return null;
        if (animIdx < 0 || animIdx >= _datos.todasLasAnimaciones.Count) return null;
        return _datos.todasLasAnimaciones[animIdx];
    }

    int ContarPixeles(int animIdx)
    {
        List<Texture2D> frames = ObtenerFrames(animIdx);
        if (frames == null || frames.Count == 0 || frames[0] == null) return 0;

        int count = 0;
        foreach (Color c in frames[0].GetPixels())
            if (c.a > 0.01f) count++;
        return count;
    }

    string NombreAnim(int idx)
    {
        string[] nombres = { "IDLE", "RUN", "JUMP", "ATTACK", "CROUCH", "DAMAGE" };
        return idx >= 0 && idx < nombres.Length ? nombres[idx] : "?";
    }

    void ActualizarColorTabs(int idx)
    {
        Button[] tabs = { btnTabIdle, btnTabCorrer, btnTabAtacar, btnTabAgacharse };
        int[]    idxs = { ANIM_IDLE,  ANIM_RUN,    ANIM_ATTACK,  ANIM_CROUCH   };

        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i] == null) continue;
            Image img = tabs[i].GetComponent<Image>();
            if (img != null)
                img.color = idxs[i] == idx ? COLOR_ACTIVO : COLOR_INACTIVO;
        }
    }

    // ── Acciones ────────────────────────

    void VolverAlEditor()
    {
        // Los datos siguen en CharacterDataHolder, el editor los recupera
        SceneManager.LoadScene("Edition");
    }

    void Descartar()
    {
        if (CharacterDataHolder.Instance != null)
            CharacterDataHolder.Instance.SetData(null);
        SceneManager.LoadScene("MenuPrincipal");
    }

    void AvanzarAHitbox()
    {
        if (_datos != null && inputNombre != null)
            _datos.nombrePersonaje = inputNombre.text;

        // Frame idle para mostrar de fondo
        Texture2D spriteIdle = null;
        if (_datos?.todasLasAnimaciones != null &&
            _datos.todasLasAnimaciones.Count > ANIM_IDLE &&
            _datos.todasLasAnimaciones[ANIM_IDLE].Count > 0)
        {
            spriteIdle = _datos.todasLasAnimaciones[ANIM_IDLE][0];
        }

        // Frames de la animacion de ataque (indice 3)
        List<Texture2D> framesAtaque = null;
        if (_datos?.todasLasAnimaciones != null &&
            _datos.todasLasAnimaciones.Count > ANIM_ATTACK)
        {
            framesAtaque = _datos.todasLasAnimaciones[ANIM_ATTACK];
        }

        hitboxEditor?.Abrir(spriteIdle, framesAtaque);
    }

    public void OnHitboxConfirmado()
    {
        panelGuardar?.Abrir();
    }
    }