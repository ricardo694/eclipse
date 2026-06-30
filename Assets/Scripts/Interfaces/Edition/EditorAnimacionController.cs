using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class EditorAnimacionController : MonoBehaviour
{
    [Header("Referencia al editor")]
    public PixelArtEditor pixelArtEditor;

    [Header("Instruccion")]
    public TMP_Text txtInstruccion;

    [Header("Barra de progreso")]
    public Button btnStepIdle;
    public Button btnStepCorrer;
    public Button btnStepSaltar;
    public Button btnStepAtacar;
    public Button btnStepAgacharse;
    public Button btnStepDanio;
    public TMP_Text txtProgreso;

    [Header("Strip de frames")]
    public Transform panelFrames;
    public GameObject prefabFrame;
    public Button btnAgregarFrame;
    public TMP_Text txtLabelStrip;

    [Header("Navegación")]
    public Button btnNextStep;
    public Button btnBack;

    // ── Datos ───────────────────────────
    public static readonly string[] NOMBRES_ANIM = {
        "Idle", "Run", "Jump", "Attack", "Crouch", "Damage"
    };
    public static readonly int[] MAX_FRAMES = { 3, 3, 3, 3, 3, 3 };
    public static readonly int[] FRAMES_OBLIGATORIOS = { 1, 1, 1, 1, 1, 1 };

    private int _animActual = 0;
    private List<List<Texture2D>> _todasLasAnimaciones = new List<List<Texture2D>>();
    private List<GameObject> _frameObjs = new List<GameObject>();
    private int _frameActual = 0;

    void Start()
    {
        // Inicializar listas vacías para cada animación
        for (int i = 0; i < NOMBRES_ANIM.Length; i++)
            _todasLasAnimaciones.Add(new List<Texture2D>());

        ConectarBotones();
        ActualizarUI();
        CrearFrameInicial();
    }

    void ConectarBotones()
    {
        btnNextStep?.onClick.AddListener(SiguienteAnimacion);
        btnBack?.onClick.AddListener(AnimacionAnterior);
        btnAgregarFrame?.onClick.AddListener(AgregarFrame);
        btnStepIdle?.onClick.AddListener(()      => IrAAnimacion(0));
        btnStepCorrer?.onClick.AddListener(()    => IrAAnimacion(1));
        btnStepSaltar?.onClick.AddListener(()    => IrAAnimacion(2));
        btnStepAtacar?.onClick.AddListener(()    => IrAAnimacion(3));
        btnStepAgacharse?.onClick.AddListener(() => IrAAnimacion(4));
        btnStepDanio?.onClick.AddListener(()     => IrAAnimacion(5));

    }

    // ── Navegación entre animaciones ────
    void SiguienteAnimacion()
    {
        // Guardar frame actual antes de avanzar
        GuardarFrameActual();

        // Validar que tenga los frames obligatorios
        int obligatorios = FRAMES_OBLIGATORIOS[_animActual];
        if (_todasLasAnimaciones[_animActual].Count < obligatorios)
        {
            Debug.Log($"You need at least {obligatorios} frame(s) for {NOMBRES_ANIM[_animActual]}");
            return;
        }

        if (_animActual < NOMBRES_ANIM.Length - 1)
        {
            _animActual++;
            _frameActual = 0;
            ActualizarUI();
            ReconstruirStrip();
            pixelArtEditor.LimpiarLienzoPublico();
        }
        else
        {
            // Última animación — ir al preview
            TerminarYGuardar();
        }
    }

    void AnimacionAnterior()
    {
        GuardarFrameActual();

        if (_animActual > 0)
        {
            _animActual--;
            _frameActual = _todasLasAnimaciones[_animActual].Count - 1;
            ActualizarUI();
            ReconstruirStrip();
            CargarFrameEnLienzo(_frameActual);
        }
    }

    // ── Manejo de frames ─────────────────
    void CrearFrameInicial()
    {
        LimpiarStrip();
        AgregarFrameVisual(0);
        _frameActual = 0;
        ActualizarUI();
    }

    void AgregarFrame()
    {
        int maxF = MAX_FRAMES[_animActual];
        int actual = _todasLasAnimaciones[_animActual].Count;

        // Guardar el frame actual primero
        GuardarFrameActual();

        if (actual >= maxF)
        {
            Debug.Log($"Maximum frames for {NOMBRES_ANIM[_animActual]}: {maxF}");
            return;
        }

        int nuevoIdx = _frameObjs.Count;
        AgregarFrameVisual(nuevoIdx);
        _frameActual = nuevoIdx;
        pixelArtEditor.LimpiarLienzoPublico();
        ActualizarBtnAgregar();
    }

    void AgregarFrameVisual(int idx)
    {
        if (prefabFrame == null || panelFrames == null) return;

        GameObject obj = Instantiate(prefabFrame, panelFrames);
        obj.name = $"Frame_{idx}";
        _frameObjs.Add(obj);
        // Forzar tamaño correcto
        RectTransform rt = obj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = new Vector2(70, 70);
        }

        LayoutElement le = obj.GetComponent<LayoutElement>();
        if (le == null) le = obj.AddComponent<LayoutElement>();
        le.preferredWidth = 70;
        le.preferredHeight = 70;
        le.minWidth = 70;
        le.minHeight = 70;
        // Número del frame
        // Buscar el texto de cualquier manera posible
        TMP_Text txt = obj.transform.Find("Txt_NumFrame")?.GetComponent<TMP_Text>();
        if (txt == null) txt = obj.GetComponentInChildren<TMP_Text>(true); 
        if (txt != null) txt.text = (idx + 1).ToString();
        Debug.Log($"Frame_{idx} — TMP encontrado: {(txt != null ? txt.gameObject.name : "null")} — texto: {(txt != null ? txt.text : "ninguno")}");
        // Clic para seleccionar
        int capturedIdx = idx;
        Button btn = obj.GetComponent<Button>();
        if (btn == null) btn = obj.AddComponent<Button>();
        btn.onClick.AddListener(() => SeleccionarFrame(capturedIdx));

        // Mover el Btn_AgregarFrame al final
        if (btnAgregarFrame != null)
            btnAgregarFrame.transform.SetAsLastSibling();

        ActualizarBtnAgregar();
    }

    void SeleccionarFrame(int idx)
    {

        GuardarFrameActual();
       
        _frameActual = idx;
        for (int i = 0; i < _frameObjs.Count; i++)
            {
                Image img = _frameObjs[i].GetComponent<Image>();
                if (img != null)
                    img.color = i == idx
                        ? new Color(0.36f, 0.31f, 0.94f, 0.5f)
                        : new Color(0.05f, 0.05f, 0.10f, 1f);
            }
        CargarFrameEnLienzo(idx);
        ActualizarUI();
    }

    void GuardarFrameActual()
    {
        if (pixelArtEditor == null) return;

        Texture2D snap = pixelArtEditor.ObtenerSnapshotLienzo();
        if(snap == null) return;
        var lista = _todasLasAnimaciones[_animActual];

        if (_frameActual < lista.Count)
            lista[_frameActual] = snap;
        else
            lista.Add(snap);

        // Actualizar miniatura en el strip
        if (_frameActual < _frameObjs.Count)
        {
            RawImage ri = _frameObjs[_frameActual].transform.Find("RawImage_MiniFrame")?.GetComponent<RawImage>(); 
            if (ri != null)
            {
                ri.texture = snap;
                ri.uvRect = new Rect(0, 1, 1, -1); 
            }
        }
    }

    void CargarFrameEnLienzo(int idx)
    {
        var lista = _todasLasAnimaciones[_animActual];
        if (idx < lista.Count && lista[idx] != null)
            pixelArtEditor.CargarTexturaEnLienzo(lista[idx]);
        else
            pixelArtEditor.LimpiarLienzoPublico();
    }

    void ReconstruirStrip()
    {
        LimpiarStrip();
        var lista = _todasLasAnimaciones[_animActual];

        if (lista.Count == 0)
        {
            AgregarFrameVisual(0);
        }
        else
        {
            for (int i = 0; i < lista.Count; i++)
            {
                AgregarFrameVisual(i);
                RawImage ri = _frameObjs[i].transform.Find("RawImage_MiniFrame")?.GetComponent<RawImage>();
                if (ri != null && lista[i] != null)
                {
                    ri.texture = lista[i];
                    ri.uvRect = new Rect(0, 1, 1, -1);
                }
            }
        }

        SeleccionarFrame(_frameActual);
        ActualizarBtnAgregar();
        ActualizarUI();
    }

    void LimpiarStrip()
    {
        foreach (var obj in _frameObjs)
            Destroy(obj);
        _frameObjs.Clear();
    }

    void ActualizarBtnAgregar()
    {
        if (btnAgregarFrame == null) return;
        int max = MAX_FRAMES[_animActual];
        int actual = _todasLasAnimaciones[_animActual].Count;
        btnAgregarFrame.gameObject.SetActive(actual < max);
    }

    // ── UI ──────────────────────────────
    void ActualizarUI()
    {
        // Progreso
        if (txtProgreso != null)
            txtProgreso.text = $"{_animActual + 1} / {NOMBRES_ANIM.Length}";

        // Label strip
        if (txtLabelStrip != null)
        txtLabelStrip.text = $"frames de\n{NOMBRES_ANIM[_animActual]}:";

        // Botones de paso
        Button[] btns = {
            btnStepIdle, btnStepCorrer, btnStepSaltar,
            btnStepAtacar, btnStepAgacharse, btnStepDanio
        };

        // Texto del botón next
        if (btnNextStep != null)
        {
            TMP_Text txt = btnNextStep.GetComponentInChildren<TMP_Text>();
            if (txt != null)
                txt.text = _animActual < NOMBRES_ANIM.Length - 1
                    ? "Next step →"
                    : "¡Finish!";
        }

        if (txtInstruccion != null)
        {
            txtInstruccion.text = $"Draw: {NOMBRES_ANIM[_animActual]} — frame {_frameActual + 1} of {Mathf.Max(1, _todasLasAnimaciones[_animActual].Count)}";
        }
    }
        void IrAAnimacion(int idx)
        {
            if (idx > _animActual) return;

            GuardarFrameActual();
            _animActual = idx;
            _frameActual = 0;
            ActualizarUI();
            ReconstruirStrip();
        }
    // ── Finalizar ───────────────────────
    void TerminarYGuardar()
    {
        GuardarFrameActual();

        // Pasar todos los frames al CharacterData
        CharacterData data = new CharacterData();
        data.nombrePersonaje = "Mi character";
        data.fechaCreacion   = System.DateTime.Now.ToString("dd/MM/yyyy");
        data.todasLasAnimaciones = _todasLasAnimaciones;

        // Contar píxeles del primer frame de idle
        if (_todasLasAnimaciones[0].Count > 0 && _todasLasAnimaciones[0][0] != null)
        {
            int pixeles = 0;
            Color[] px = _todasLasAnimaciones[0][0].GetPixels();
            foreach (Color c in px)
                if (c.a > 0.01f) pixeles++;
            data.pixelesPintados = pixeles;
            data.spriteBase = _todasLasAnimaciones[0][0];
        }

        if (CharacterDataHolder.Instance != null)
            CharacterDataHolder.Instance.SetData(data);

        UnityEngine.SceneManagement.SceneManager.LoadScene("PreviewPersonaje");
    }
}