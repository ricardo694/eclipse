using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

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

    // ── Estado interno ──────────────────
    private CharacterData _datos;
    private string _animActual = "idle";

    private Texture2D _texIdle;
    private Texture2D _texCorrer;
    private Texture2D _texAtacar;
    private Texture2D _texAgacharse;

    private float _t = 0f;
    private Texture2D _lienzoAnimado;

    // Skeleton — posiciones de huesos normalizadas (0-1)
    // Se escalan al tamaño del sprite en runtime
    private struct PoseFrame
    {
        public float offsetY;
        public float armAngle;
        public float legAngle;
        public float lean;
        public float squat;
    }

    // ── Inicio ──────────────────────────
    void Start()
    {
        // Obtener datos del editor
        if (CharacterDataHolder.Instance != null)
            _datos = CharacterDataHolder.Instance.DatosActuales;

        // Si no hay datos (prueba directa de escena) crear datos de prueba
        if (_datos == null || _datos.spriteBase == null)
        {
            _datos = new CharacterData();
            _datos.nombrePersonaje = "Personaje de prueba";
            _datos.fechaCreacion = System.DateTime.Now.ToString("dd/MM/yyyy");
            _datos.pixelesPintados = 0;
            _datos.spriteBase = CrearTexturaVacia();
        }

        InicializarFondoTablero();
        InicializarTexturas();
        InicializarUI();
        ConectarBotones();
    }

    void Update()
    {
        _t += Time.deltaTime;
        ActualizarAnimacion();
    }

    // ── Inicialización ──────────────────
    void InicializarFondoTablero()
    {
        if (rawImageFondoPreview == null) return;
        Texture2D fondo = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        fondo.filterMode = FilterMode.Point;
        Color grisClaro = new Color(0.78f, 0.78f, 0.78f, 1f);
        Color grisOscuro = new Color(0.55f, 0.55f, 0.55f, 1f);
        for (int y = 0; y < 128; y++)
        for (int x = 0; x < 128; x++)
        {
            bool par = ((x / 4) + (y / 4)) % 2 == 0;
            fondo.SetPixel(x, y, par ? grisClaro : grisOscuro);
        }
        fondo.Apply();
        rawImageFondoPreview.texture = fondo;
    }

    void InicializarTexturas()
    {
        // Textura animada principal
        _lienzoAnimado = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        _lienzoAnimado.filterMode = FilterMode.Point;
        if (rawImagePersonaje != null)
            rawImagePersonaje.texture = _lienzoAnimado;

        // Generar miniaturas estáticas de cada pose
        _texIdle      = GenerarMiniatura(0f,   0.1f,  0f,   0f,  0f);
        _texCorrer    = GenerarMiniatura(-3f,   0.4f,  0.5f, 2f,  0f);
        _texAtacar    = GenerarMiniatura(0f,   -0.8f,  0f,  -3f,  0f);
        _texAgacharse = GenerarMiniatura(10f,   0.3f,  0f,   0f,  1f);

        if (miniIdle      != null) miniIdle.texture      = _texIdle;
        if (miniCorrer    != null) miniCorrer.texture    = _texCorrer;
        if (miniAtacar    != null) miniAtacar.texture    = _texAtacar;
        if (miniAgacharse != null) miniAgacharse.texture = _texAgacharse;
    }

    void InicializarUI()
    {
        if (inputNombre != null)
            inputNombre.text = _datos.nombrePersonaje;

        if (txtPixelesPintados != null)
            txtPixelesPintados.text = $"Píxeles pintados: {_datos.pixelesPintados}";

        if (txtAnimActual != null)
            txtAnimActual.text = "idle";
    }

    void ConectarBotones()
    {
        btnTabIdle?.onClick.AddListener(()      => CambiarAnimacion("idle"));
        btnTabCorrer?.onClick.AddListener(()    => CambiarAnimacion("correr"));
        btnTabAtacar?.onClick.AddListener(()    => CambiarAnimacion("atacar"));
        btnTabAgacharse?.onClick.AddListener(() => CambiarAnimacion("agacharse"));

        btnGuardar?.onClick.AddListener(GuardarPersonaje);
        btnDescartar?.onClick.AddListener(Descartar);
        btnVolver?.onClick.AddListener(VolverAlEditor);
    }

    // ── Animación ───────────────────────
    void CambiarAnimacion(string anim)
    {
        _animActual = anim;
        _t = 0f;
        if (txtAnimActual != null)
            txtAnimActual.text = anim;

        // Actualizar color de tabs
        ActualizarColorTabs();
    }

    void ActualizarColorTabs()
    {
        Color activo   = new Color(0.36f, 0.31f, 0.94f);
        Color inactivo = new Color(0.16f, 0.16f, 0.27f);

        if (btnTabIdle      != null) btnTabIdle.image.color      = _animActual == "idle"       ? activo : inactivo;
        if (btnTabCorrer    != null) btnTabCorrer.image.color    = _animActual == "correr"     ? activo : inactivo;
        if (btnTabAtacar    != null) btnTabAtacar.image.color    = _animActual == "atacar"     ? activo : inactivo;
        if (btnTabAgacharse != null) btnTabAgacharse.image.color = _animActual == "agacharse"  ? activo : inactivo;
    }

    void ActualizarAnimacion()
    {
        float oY = 0, arm = 0, leg = 0, lean = 0, sq = 0;

        switch (_animActual)
        {
            case "idle":
                oY  = Mathf.Sin(_t) * 2f;
                arm = Mathf.Sin(_t * 0.5f) * 0.15f;
                break;
            case "correr":
                oY   = Mathf.Abs(Mathf.Sin(_t * 2f)) * -3f;
                leg  = Mathf.Sin(_t * 2f) * 0.5f;
                arm  = Mathf.Sin(_t * 2f) * 0.4f;
                lean = Mathf.Sin(_t * 2f) * 2f;
                break;
            case "atacar":
                arm  = -0.8f + Mathf.Sin(_t * 3f) * 0.3f;
                oY   = Mathf.Sin(_t * 0.5f) * 1f;
                lean = -3f;
                break;
            case "agacharse":
                float prog = Mathf.Min(_t * 2f, 1f);
                sq  = prog;
                oY  = prog * 18f + Mathf.Sin(_t * 0.8f) * 0.5f;
                arm = 0.3f + Mathf.Sin(_t) * 0.05f;
                break;
        }

        DibujarPersonaje(_lienzoAnimado, _datos.spriteBase, oY, arm, leg, lean, sq);
    }

    // ── Dibujado del skeleton ────────────
    void DibujarPersonaje(Texture2D dest, Texture2D spriteOrig,
        float offsetY, float armAngle, float legAngle, float lean, float squat)
    {
        if (dest == null || spriteOrig == null) return;

        Color[] clear = new Color[128 * 128];
        dest.SetPixels(clear);

        // Calcular centro de masa del sprite
        float cx = 0, cy = 0;
        int count = 0;
        Color[] pixOrig = spriteOrig.GetPixels();
        for (int y = 0; y < 128; y++)
        for (int x = 0; x < 128; x++)
        {
            if (pixOrig[y * 128 + x].a > 0.01f)
            {
                cx += x; cy += y; count++;
            }
        }
        if (count == 0) return;
        cx /= count; cy /= count;

        // Aplicar transformaciones al sprite completo
        float cos = Mathf.Cos(lean * 0.03f);
        float sin = Mathf.Sin(lean * 0.03f);

        for (int y = 0; y < 128; y++)
        for (int x = 0; x < 128; x++)
        {
            Color c = pixOrig[(127 - y) * 128 + x];
            if (c.a < 0.01f) continue;

            // Offset desde el centro de masa
            float lx = x - cx;
            float ly = y - cy;

            // Aplicar rotación (lean)
            int rx = Mathf.RoundToInt(cos * lx - sin * ly + cx);
            int ry = Mathf.RoundToInt(sin * lx + cos * ly + cy + offsetY - squat * 8f);

            if (rx >= 0 && rx < 128 && ry >= 0 && ry < 128)
                dest.SetPixel(rx, ry, c);
        }

        dest.Apply();
    }

    void PintarRegion(Texture2D dest, Texture2D src,
        RectInt region, int destCX, int destCY, float angle)
    {
        int pivX = region.x + region.width  / 2;
        int pivY = region.y + region.height / 2;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        for (int y = region.y; y < region.y + region.height; y++)
        for (int x = region.x; x < region.x + region.width;  x++)
        {
            Color c = src.GetPixel(x, y);
            if (c.a < 0.01f) continue;

            float lx = x - pivX;
            float ly = y - pivY;
            int rx = Mathf.RoundToInt(cos * lx - sin * ly) + destCX;
            int ry = Mathf.RoundToInt(sin * lx + cos * ly) + destCY;

            if (rx >= 0 && rx < 128 && ry >= 0 && ry < 128)
                dest.SetPixel(rx, ry, c);
        }
    }

    Texture2D GenerarMiniatura(float oY, float arm, float leg, float lean, float sq)
    {
        Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        if (_datos?.spriteBase != null)
            DibujarPersonaje(tex, _datos.spriteBase, oY, arm, leg, lean, sq);
        return tex;
    }

    Texture2D CrearTexturaVacia()
    {
        Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] px = new Color[128 * 128];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    // ── Acciones ────────────────────────
    void GuardarPersonaje()
    {
        if (_datos == null) return;

        // Actualizar nombre desde el input
        if (inputNombre != null)
            _datos.nombrePersonaje = inputNombre.text;

        _datos.fechaCreacion = System.DateTime.Now.ToString("dd/MM/yyyy");

        // Guardar
        if (CharacterSaveSystem.Instance != null)
            CharacterSaveSystem.Instance.GuardarPersonaje(_datos);

        Debug.Log($"Personaje guardado: {_datos.nombrePersonaje}");

        // Volver al menú (por ahora vuelve al editor)
        SceneManager.LoadScene("MenuPrincipal");
    }

    void Descartar()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }

    void VolverAlEditor()
    {
        SceneManager.LoadScene("Edition");
    }
}