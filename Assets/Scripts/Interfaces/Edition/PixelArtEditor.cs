using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PixelArtEditor : MonoBehaviour
{
    [Header("Lienzo")]
    public RawImage lienzoDisplay;
    public RawImage guiaDisplay;
    public RawImage seleccionDisplay;
    public RawImage fondoDisplay;

    [Header("Herramientas")]
    public Button btnPincel;
    public Button btnBorrador;
    public Button btnRelleno;
    public Button btnCuentagotas;
    public Slider sliderTamanio;

    [Header("Color")]
    public Image imgColorActivo;
    public Image imgPreviewAvanzado;
    public Slider sliderH;
    public Slider sliderS;
    public Slider sliderB;
    public TMP_InputField inputHex;

    [Header("Paleta")]
    public Transform contenedorPaleta;
    public GameObject prefabSwatch;

    [Header("Guia")]
    public Toggle toggleGuia;

    [Header("Seleccion")]
    public Button btnSeleccion;
    public GameObject panelMenuSeleccion;
    public Button btnMover;
    public Button btnCopiar;
    public Button btnBorrarSeleccion;
    private int _anchoSeleccionOriginal;
    private int _altoSeleccionOriginal;
    private RectInt _rectSeleccionOriginal;

    [Header("Acciones")]
    public Button btnGuardar;
    public Button btnLimpiar;

    // ── Estado interno ──────────────────
    private Texture2D _lienzo;
    private Color _colorActivo = Color.red;
    private int _tamanio = 1;
    private bool _pintando = false;

    // Selección
    private Vector2Int _seleccionInicio;
    private Vector2Int _seleccionFin;
    private Color[] _pixelesSeleccionados;
    private RectInt _rectSeleccion;
    private bool _haySeleccion = false;
    private bool _moviendo = false;
    private bool _redimensionando = false;
    private Vector2Int _esquinaRedimension;
    private Texture2D _lienzoAntes;
    private Texture2D _texturaSeleccion;
    private float _tiempoMarco = 0f;
    private bool _marcoFase = false;
    private const int MARGEN_ESQUINA = 4;
    private Stack<Color[]> _historial = new Stack<Color[]>();
    private const int MAX_HISTORIAL = 30;

    private enum Herramienta { Pincel, Borrador, Relleno, Cuentagotas, Seleccion }
    private Herramienta _herramientaActual = Herramienta.Pincel;

    private static readonly Color[] PALETA = {
        // Rojos
        new Color(1.00f,0.00f,0.00f), new Color(0.86f,0.08f,0.24f),
        new Color(0.70f,0.13f,0.13f), new Color(0.50f,0.00f,0.00f),
        new Color(1.00f,0.27f,0.00f), new Color(1.00f,0.41f,0.38f),
        new Color(0.96f,0.64f,0.38f), new Color(1.00f,0.55f,0.00f),
        // Amarillos
        new Color(1.00f,1.00f,0.00f), new Color(0.93f,0.93f,0.17f),
        new Color(0.91f,0.77f,0.41f), new Color(1.00f,0.84f,0.00f),
        new Color(0.74f,0.72f,0.42f), new Color(0.60f,0.53f,0.19f),
        new Color(0.49f,0.42f,0.07f), new Color(0.30f,0.26f,0.04f),
        // Verdes
        new Color(0.00f,1.00f,0.00f), new Color(0.13f,0.70f,0.33f),
        new Color(0.00f,0.50f,0.00f), new Color(0.00f,0.27f,0.13f),
        new Color(0.49f,0.99f,0.00f), new Color(0.60f,0.80f,0.20f),
        new Color(0.16f,0.62f,0.56f), new Color(0.02f,0.84f,0.63f),
        // Azules
        new Color(0.00f,0.00f,1.00f), new Color(0.27f,0.48f,0.62f),
        new Color(0.17f,0.40f,0.53f), new Color(0.00f,0.00f,0.50f),
        new Color(0.66f,0.72f,0.87f), new Color(0.53f,0.81f,0.98f),
        new Color(0.00f,0.75f,1.00f), new Color(0.25f,0.88f,0.82f),
        // Morados
        new Color(0.58f,0.00f,0.83f), new Color(0.78f,0.72f,1.00f),
        new Color(0.50f,0.00f,0.50f), new Color(0.29f,0.00f,0.51f),
        new Color(1.00f,0.08f,0.58f), new Color(1.00f,0.42f,0.62f),
        new Color(0.86f,0.44f,0.84f), new Color(0.73f,0.33f,0.83f),
        // Cafés y piel
        new Color(0.60f,0.45f,0.25f), new Color(0.80f,0.52f,0.25f),
        new Color(0.96f,0.76f,0.76f), new Color(0.96f,0.64f,0.38f),
        new Color(0.87f,0.72f,0.53f), new Color(0.55f,0.27f,0.07f),
        new Color(0.36f,0.25f,0.20f), new Color(0.24f,0.15f,0.10f),
        // Grises
        new Color(1.00f,1.00f,1.00f), new Color(0.85f,0.85f,0.85f),
        new Color(0.66f,0.66f,0.66f), new Color(0.50f,0.50f,0.50f),
        new Color(0.40f,0.40f,0.50f), new Color(0.25f,0.25f,0.35f),
        new Color(0.08f,0.08f,0.15f), new Color(0.00f,0.00f,0.00f),
        // Metálicos y especiales
        new Color(0.83f,0.83f,0.83f), new Color(1.00f,0.84f,0.00f),
        new Color(0.72f,0.45f,0.20f), new Color(0.40f,0.40f,0.40f),
        new Color(0.00f,0.50f,0.50f), new Color(0.00f,1.00f,0.50f),
        new Color(0.50f,1.00f,0.83f), new Color(0.94f,0.97f,1.00f),
    };

    // ── Inicio ──────────────────────────
    void Start()
    {
        InicializarLienzo();
        InicializarPaleta();
        ConectarBotones();
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Animar marco punteado
        if (_haySeleccion)
        {
            _tiempoMarco += Time.deltaTime;
            if (_tiempoMarco > 0.4f)
            {
                _tiempoMarco = 0f;
                _marcoFase = !_marcoFase;
                DibujarMarcoVisual();
            }
        }

        bool ctrl = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;

        // Ctrl+Z → deshacer
        if (ctrl && keyboard.zKey.wasPressedThisFrame)
            Deshacer();

        // Solo si hay selección activa
        if (_herramientaActual == Herramienta.Seleccion)
        {
            if (ctrl)
            {
                if (keyboard.cKey.wasPressedThisFrame) CopiarSeleccion();
                if (keyboard.xKey.wasPressedThisFrame) CortarSeleccion();
                if (keyboard.vKey.wasPressedThisFrame) PegarSeleccion();
            }
            if (keyboard.deleteKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame)
                BorrarSeleccion();
        }
    }

    void InicializarLienzo()
    {
        _lienzo = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        _lienzo.filterMode = FilterMode.Point;
        LimpiarLienzo();
        lienzoDisplay.texture = _lienzo;
        // Fondo tablero de ajedrez
        if (fondoDisplay != null)
        {
            Texture2D fondo = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            fondo.filterMode = FilterMode.Point;
            Color grisClaro = new Color(0.78f, 0.78f, 0.78f, 1f);
            Color grisOscuro = new Color(0.55f, 0.55f, 0.55f, 1f);
            int tamCelda = 4;

            for (int y = 0; y < 128; y++)
            for (int x = 0; x < 128; x++)
            {
                bool par = ((x / tamCelda) + (y / tamCelda)) % 2 == 0;
                fondo.SetPixel(x, y, par ? grisClaro : grisOscuro);
            }
            fondo.Apply();
            fondoDisplay.texture = fondo;
        }
        _texturaSeleccion = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        _texturaSeleccion.filterMode = FilterMode.Point;
        LimpiarMarco();
        if (seleccionDisplay != null)
            seleccionDisplay.texture = _texturaSeleccion;
    }

    void InicializarPaleta()
    {
        foreach (Color color in PALETA)
        {
            GameObject swatch = Instantiate(prefabSwatch, contenedorPaleta);
            Color c = color;
            Image img = swatch.GetComponent<Image>();
            if (img != null) img.color = c;
            Button btn = swatch.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => SeleccionarColor(c));
        }
    }

    void ConectarBotones()
    {
        btnPincel?.onClick.AddListener(() => CambiarHerramienta(Herramienta.Pincel));
        btnBorrador?.onClick.AddListener(() => CambiarHerramienta(Herramienta.Borrador));
        btnRelleno?.onClick.AddListener(() => CambiarHerramienta(Herramienta.Relleno));
        btnCuentagotas?.onClick.AddListener(() => CambiarHerramienta(Herramienta.Cuentagotas));
        btnGuardar?.onClick.AddListener(GuardarPersonaje);
        btnLimpiar?.onClick.AddListener(LimpiarLienzo);
        btnSeleccion?.onClick.AddListener(() => CambiarHerramienta(Herramienta.Seleccion));
        btnMover?.onClick.AddListener(() => Debug.Log("Usa el mouse para mover la selección"));
        btnCopiar?.onClick.AddListener(CopiarSeleccion);
        btnBorrarSeleccion?.onClick.AddListener(BorrarSeleccion);
        sliderTamanio?.onValueChanged.AddListener(val => _tamanio = Mathf.RoundToInt(val));
        toggleGuia?.onValueChanged.AddListener(activar => {
            if (guiaDisplay != null)
                guiaDisplay.gameObject.SetActive(activar);
        });

        // Sliders HSB
        if (sliderH != null) sliderH.onValueChanged.AddListener(_ => ActualizarDesdeHSB());
        if (sliderS != null) sliderS.onValueChanged.AddListener(_ => ActualizarDesdeHSB());
        if (sliderB != null) sliderB.onValueChanged.AddListener(_ => ActualizarDesdeHSB());

        // Input hex
        if (inputHex != null)
            inputHex.onEndEdit.AddListener(HexAColor);

        // Valores iniciales de los sliders
        if (sliderH != null) { sliderH.minValue = 0; sliderH.maxValue = 360; sliderH.value = 0; }
        if (sliderS != null) { sliderS.minValue = 0; sliderS.maxValue = 100; sliderS.value = 100; }
        if (sliderB != null) { sliderB.minValue = 0; sliderB.maxValue = 100; sliderB.value = 100; }
    }

    // ── Input de dibujo ─────────────────
    public void OnPointerDown(BaseEventData data)
    {
        _pintando = true;
        PointerEventData ped = (PointerEventData)data;
        Vector2Int pixel = ObtenerPixel(ped);
        if (pixel.x < 0) return;

        if (_herramientaActual == Herramienta.Seleccion)
        {
            Vector2Int esquina;
            if (_haySeleccion && EsquinaCercana(pixel.x, pixel.y, out esquina))
            {
                _esquinaRedimension = new Vector2Int(
                    esquina.x == _rectSeleccion.x
                        ? _rectSeleccion.x + _rectSeleccion.width - 1
                        : _rectSeleccion.x,
                    esquina.y == _rectSeleccion.y
                        ? _rectSeleccion.y + _rectSeleccion.height - 1
                        : _rectSeleccion.y
                );
                // Guardar estado original antes de redimensionar
                GuardarPixelesSeleccionados();
                _anchoSeleccionOriginal = _rectSeleccion.width;
                _altoSeleccionOriginal = _rectSeleccion.height;
                _rectSeleccionOriginal = _rectSeleccion;
                _redimensionando = true;
                _moviendo = false;
            }
            else if (_haySeleccion && EstaDentroDeSeleccion(pixel.x, pixel.y))
            {
                // Clic dentro → mover
                IniciarMoverSeleccion(pixel.x, pixel.y);
            }
            else
            {
                // Clic fuera → nueva selección
                _seleccionInicio = pixel;
                _seleccionFin = pixel;
                _haySeleccion = false;
                _moviendo = false;
                _redimensionando = false;
            }
            return;
        }

        ProcesarInput(ped);
    }


    public void OnDrag(BaseEventData data)
    {
        if (!_pintando) return;
        PointerEventData ped = (PointerEventData)data;
        Vector2Int pixel = ObtenerPixel(ped);
        if (pixel.x < 0) return;

        if (_herramientaActual == Herramienta.Seleccion)
        {
            if (_redimensionando)
                RedimensionarSeleccion(pixel.x, pixel.y);
            else if (_moviendo)
                MoverSeleccion(pixel.x, pixel.y);
            else
            {
                _seleccionFin = pixel;
                _haySeleccion = true;
                DibujarMarcoSeleccion();
            }
            return;
        }

        ProcesarInput(ped);
    }

    public void OnPointerUp(BaseEventData data)
    {
        _pintando = false;
        if (_moviendo) TerminarMover();
        if (_redimensionando)
        {
            _redimensionando = false;
            _haySeleccion = true;
            AplicarRedimension();
        }
    }
    Vector2Int ObtenerPixel(PointerEventData ped)
    {
        Vector2 localPoint;
        RectTransform rt = lienzoDisplay.rectTransform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt, ped.position, ped.pressEventCamera, out localPoint))
            return new Vector2Int(-1, -1);

        Rect rect = rt.rect;
        float u = (localPoint.x - rect.x) / rect.width;
        float v = (localPoint.y - rect.y) / rect.height;
        if (u < 0 || u > 1 || v < 0 || v > 1) return new Vector2Int(-1, -1);

        int px = Mathf.Clamp(Mathf.FloorToInt(u * 128), 0, 127);
        int py = Mathf.Clamp(Mathf.FloorToInt(v * 128), 0, 127);
        return new Vector2Int(px, py);
    }
    void ProcesarInput(PointerEventData data)
    {
        Vector2 localPoint;
        RectTransform rt = lienzoDisplay.rectTransform;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt, data.position, data.pressEventCamera, out localPoint)) return;

        Rect rect = rt.rect;
        float u = (localPoint.x - rect.x) / rect.width;
        float v = (localPoint.y - rect.y) / rect.height;

        if (u < 0 || u > 1 || v < 0 || v > 1) return;

        int px = Mathf.FloorToInt(u * 128);
        int py = Mathf.FloorToInt(v * 128);
        px = Mathf.Clamp(px, 0, 127);
        py = Mathf.Clamp(py, 0, 127);

        switch (_herramientaActual)
        {
            case Herramienta.Pincel:
                PintarPinchazo(px, py);
                break;
            case Herramienta.Borrador:
                BorrarPinchazo(px, py);
                break;
            case Herramienta.Relleno:
                RellenarFloodFill(px, py, _lienzo.GetPixel(px, py), _colorActivo);
                break;
            case Herramienta.Cuentagotas:
                Color colorRecogido = _lienzo.GetPixel(px, py);
                SeleccionarColor(colorRecogido);
                CambiarHerramienta(Herramienta.Pincel);
                break;
            case Herramienta.Seleccion:
                ActualizarSeleccion(px, py);
                break;
        }

        _lienzo.Apply();
    }

    // ── Herramientas ────────────────────
    void PintarPinchazo(int cx, int cy)
    {
        GuardarEstado();
        for (int dx = 0; dx < _tamanio; dx++)
        for (int dy = 0; dy < _tamanio; dy++)
        {
            int x = cx + dx;
            int y = cy + dy;
            if (x >= 0 && x < 128 && y >= 0 && y < 128)
                _lienzo.SetPixel(x, y, _colorActivo);
        }
    }

    void BorrarPinchazo(int cx, int cy)
    {
        GuardarEstado();
        for (int dx = 0; dx < _tamanio; dx++)
        for (int dy = 0; dy < _tamanio; dy++)
        {
            int x = cx + dx;
            int y = cy + dy;
            if (x >= 0 && x < 128 && y >= 0 && y < 128)
                _lienzo.SetPixel(x, y, Color.clear);
        }
    }

        void RellenarFloodFill(int startX, int startY, Color colorObjetivo, Color colorNuevo)
        {
            GuardarEstado();
            if (colorObjetivo == colorNuevo) return;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));

        while (queue.Count > 0)
        {
            Vector2Int p = queue.Dequeue();
            int x = p.x, y = p.y;
            if (x < 0 || x >= 128 || y < 0 || y >= 128) continue;
            if (_lienzo.GetPixel(x, y) != colorObjetivo) continue;
            _lienzo.SetPixel(x, y, colorNuevo);
            queue.Enqueue(new Vector2Int(x + 1, y));
            queue.Enqueue(new Vector2Int(x - 1, y));
            queue.Enqueue(new Vector2Int(x, y + 1));
            queue.Enqueue(new Vector2Int(x, y - 1));
        }
    }

    // ── Selector HSB ────────────────────
void ActualizarDesdeHSB()
{
    if (sliderH == null || sliderS == null || sliderB == null) return;
    float h = sliderH.value / 360f;
    float s = sliderS.value / 100f;
    float b = sliderB.value / 100f;

    Color color = Color.HSVToRGB(h, s, b);
    SeleccionarColor(color);

    // Actualizar hex
    if (inputHex != null)
        inputHex.text = ColorToHex(color);

    // Actualizar preview avanzado
    if (imgPreviewAvanzado != null)
        imgPreviewAvanzado.color = color;
}

    void HexAColor(string hex)
    {
        // Limpiar el input
        hex = hex.Trim();
        if (!hex.StartsWith("#")) hex = "#" + hex;

        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color))
        {
            SeleccionarColor(color);

            // Actualizar sliders desde el color hex
            float h, s, b;
            Color.RGBToHSV(color, out h, out s, out b);
            if (sliderH != null) sliderH.value = h * 360f;
            if (sliderS != null) sliderS.value = s * 100f;
            if (sliderB != null) sliderB.value = b * 100f;

            if (imgPreviewAvanzado != null)
                imgPreviewAvanzado.color = color;
        }
        else
        {
            Debug.Log("Hex inválido: " + hex);
        }
    }

    string ColorToHex(Color color)
    {
        return "#" + ColorUtility.ToHtmlStringRGB(color);
    }

    // ── Selección ───────────────────────
    void ActualizarSeleccion(int px, int py)
    {
        _seleccionFin = new Vector2Int(px, py);
        _haySeleccion = true;
        DibujarMarcoSeleccion();
    }

    void DibujarMarcoSeleccion()
    {
        // Recalcular rect
        int x = Mathf.Min(_seleccionInicio.x, _seleccionFin.x);
        int y = Mathf.Min(_seleccionInicio.y, _seleccionFin.y);
        int w = Mathf.Abs(_seleccionFin.x - _seleccionInicio.x) + 1;
        int h = Mathf.Abs(_seleccionFin.y - _seleccionInicio.y) + 1;
        _rectSeleccion = new RectInt(x, y, w, h);
        DibujarMarcoVisual();
    }

    bool EstaDentroDeSeleccion(int px, int py)
    {
        return _haySeleccion && _rectSeleccion.Contains(new Vector2Int(px, py));
    }

    void IniciarMoverSeleccion(int px, int py)
    {
        if (!_haySeleccion) return;
        GuardarPixelesSeleccionados();
        BorrarAreaSeleccionada();
        _moviendo = true;
        _seleccionInicio = new Vector2Int(px, py);
    }

    void MoverSeleccion(int px, int py)
    {
        if (!_moviendo || _pixelesSeleccionados == null) return;

        int deltaX = px - _seleccionInicio.x;
        int deltaY = py - _seleccionInicio.y;

        // Nueva posición del rect
        int nuevoX = Mathf.Clamp(_rectSeleccion.x + deltaX, 0, 128 - _rectSeleccion.width);
        int nuevoY = Mathf.Clamp(_rectSeleccion.y + deltaY, 0, 128 - _rectSeleccion.height);

        // Limpiar posición anterior
        Color[] vacios = new Color[_rectSeleccion.width * _rectSeleccion.height];
        for (int i = 0; i < vacios.Length; i++) vacios[i] = Color.clear;
        _lienzo.SetPixels(_rectSeleccion.x, _rectSeleccion.y,
            _rectSeleccion.width, _rectSeleccion.height, vacios);

        // Pegar en nueva posición
        _lienzo.SetPixels(nuevoX, nuevoY,
            _rectSeleccion.width, _rectSeleccion.height, _pixelesSeleccionados);
        _lienzo.Apply();

        // Actualizar rect y origen
        _rectSeleccion = new RectInt(nuevoX, nuevoY, _rectSeleccion.width, _rectSeleccion.height);
        _seleccionInicio = new Vector2Int(px, py);
    }

    void TerminarMover()
    {
        _moviendo = false;
    }

    void GuardarPixelesSeleccionados()
    {
        _pixelesSeleccionados = _lienzo.GetPixels(
            _rectSeleccion.x, _rectSeleccion.y,
            _rectSeleccion.width, _rectSeleccion.height);
    }

    void BorrarAreaSeleccionada()
    {
         GuardarEstado();
        Color[] vacios = new Color[_rectSeleccion.width * _rectSeleccion.height];
        for (int i = 0; i < vacios.Length; i++) vacios[i] = Color.clear;
        _lienzo.SetPixels(_rectSeleccion.x, _rectSeleccion.y,
            _rectSeleccion.width, _rectSeleccion.height, vacios);
        _lienzo.Apply();
    }

    void CortarSeleccion()
    {
        if (!_haySeleccion) return;
        GuardarPixelesSeleccionados();
        BorrarAreaSeleccionada();
        _haySeleccion = false;
        LimpiarMarco();
        Debug.Log("Cortado — Ctrl+V para pegar");
    }

    void CopiarSeleccion()
    {
        if (!_haySeleccion) return;
        GuardarPixelesSeleccionados();
        Debug.Log("Copiado — Ctrl+V para pegar");
    }

    void PegarSeleccion()
    {
        GuardarEstado();
        if (_pixelesSeleccionados == null) return;
        // Pegar en el centro del lienzo
        int px = 64 - _rectSeleccion.width / 2;
        int py = 64 - _rectSeleccion.height / 2;
        px = Mathf.Clamp(px, 0, 128 - _rectSeleccion.width);
        py = Mathf.Clamp(py, 0, 128 - _rectSeleccion.height);
        _lienzo.SetPixels(px, py, _rectSeleccion.width, _rectSeleccion.height, _pixelesSeleccionados);
        _lienzo.Apply();
        _rectSeleccion = new RectInt(px, py, _rectSeleccion.width, _rectSeleccion.height);
        _haySeleccion = true;
    }

    void BorrarSeleccion()
    {
        if (!_haySeleccion) return;
        BorrarAreaSeleccionada();
        _haySeleccion = false;
        LimpiarMarco();
        if (panelMenuSeleccion != null)
            panelMenuSeleccion.SetActive(false);
    }

    // ── Historial (Ctrl+Z) ──────────────
    void GuardarEstado()
    {
        // Solo guardar al inicio del trazo, no en cada píxel
        if (_pintando) return;

        Color[] copia = _lienzo.GetPixels();
        _historial.Push(copia);

        if (_historial.Count > MAX_HISTORIAL)
        {
            // Reconstruir el stack sin el más antiguo
            Color[][] temp = _historial.ToArray();
            _historial.Clear();
            for (int i = temp.Length - 2; i >= 0; i--)
                _historial.Push(temp[i]);
        }
    }

    void Deshacer()
    {
        if (_historial.Count == 0)
        {
            Debug.Log("No hay más pasos para deshacer");
            return;
        }

        Color[] estadoAnterior = _historial.Pop();
        _lienzo.SetPixels(estadoAnterior);
        _lienzo.Apply();
        Debug.Log("Deshecho — pasos restantes: " + _historial.Count);
    }

    // ── Marco de selección visual ────────
    void LimpiarMarco()
    {
        Color[] pixeles = new Color[128 * 128];
        for (int i = 0; i < pixeles.Length; i++)
            pixeles[i] = Color.clear;
        _texturaSeleccion.SetPixels(pixeles);
        _texturaSeleccion.Apply();
    }

    void DibujarMarcoVisual()
    {
        if (_texturaSeleccion == null) return;
        LimpiarMarco();

        int x = _rectSeleccion.x;
        int y = _rectSeleccion.y;
        int w = _rectSeleccion.width;
        int h = _rectSeleccion.height;

        Color colorMarco = _marcoFase ? Color.white : new Color(0f, 0f, 0f, 0.8f);
        Color colorAlter = _marcoFase ? new Color(0f, 0f, 0f, 0.8f) : Color.white;

        // Borde superior e inferior
        for (int i = 0; i < w; i++)
        {
            Color c = (i % 4 < 2) ? colorMarco : colorAlter;
            SetPixelSeguro(x + i, y, c);
            SetPixelSeguro(x + i, y + h - 1, c);
        }

        // Borde izquierdo y derecho
        for (int j = 0; j < h; j++)
        {
            Color c = (j % 4 < 2) ? colorMarco : colorAlter;
            SetPixelSeguro(x, y + j, c);
            SetPixelSeguro(x + w - 1, y + j, c);
        }
        // Puntos en las 4 esquinas
        DibujarPuntoEsquina(_rectSeleccion.x, _rectSeleccion.y, colorMarco);
        DibujarPuntoEsquina(_rectSeleccion.x + w - 1, _rectSeleccion.y, colorMarco);
        DibujarPuntoEsquina(_rectSeleccion.x, _rectSeleccion.y + h - 1, colorMarco);
        DibujarPuntoEsquina(_rectSeleccion.x + w - 1, _rectSeleccion.y + h - 1, colorMarco);

        _texturaSeleccion.Apply();
    }

    void DibujarPuntoEsquina(int cx, int cy, Color c)
    {
        for (int dx = -1; dx <= 1; dx++)
        for (int dy = -1; dy <= 1; dy++)
            SetPixelSeguro(cx + dx, cy + dy, c);
    }

    void SetPixelSeguro(int px, int py, Color c)
    {
        if (px >= 0 && px < 128 && py >= 0 && py < 128)
            _texturaSeleccion.SetPixel(px, py, c);
    }

        // ── Redimensión ─────────────────────
        bool EsquinaCercana(int px, int py, out Vector2Int esquina)
        {
            esquina = Vector2Int.zero;
            if (!_haySeleccion) return false;

            Vector2Int[] esquinas = new Vector2Int[]
            {
                new Vector2Int(_rectSeleccion.x, _rectSeleccion.y),                                          // inferior izquierda
                new Vector2Int(_rectSeleccion.x + _rectSeleccion.width - 1, _rectSeleccion.y),               // inferior derecha
                new Vector2Int(_rectSeleccion.x, _rectSeleccion.y + _rectSeleccion.height - 1),              // superior izquierda
                new Vector2Int(_rectSeleccion.x + _rectSeleccion.width - 1, _rectSeleccion.y + _rectSeleccion.height - 1) // superior derecha
            };

            foreach (var e in esquinas)
            {
                if (Mathf.Abs(px - e.x) <= MARGEN_ESQUINA && Mathf.Abs(py - e.y) <= MARGEN_ESQUINA)
                {
                    esquina = e;
                    return true;
                }
            }
            return false;
        }

        void RedimensionarSeleccion(int px, int py)
        {
            if (!_redimensionando) return;

            // Calcular nuevo rect manteniendo la esquina opuesta fija
            int x1 = _esquinaRedimension.x;
            int y1 = _esquinaRedimension.y;
            int x2 = Mathf.Clamp(px, 0, 127);
            int y2 = Mathf.Clamp(py, 0, 127);

            int nuevoX = Mathf.Min(x1, x2);
            int nuevoY = Mathf.Min(y1, y2);
            int nuevoW = Mathf.Abs(x2 - x1) + 1;
            int nuevoH = Mathf.Abs(y2 - y1) + 1;

            _rectSeleccion = new RectInt(nuevoX, nuevoY, nuevoW, nuevoH);
            DibujarMarcoVisual();
        }

        void AplicarRedimension()
    {
        if (_pixelesSeleccionados == null) return;

        // Tamaño original
        int anchoOriginal = 0;
        int altoOriginal = 0;

        // Calcular dimensiones originales desde los píxeles guardados
        // Los guardamos cuando iniciamos la redimensión
        anchoOriginal = _anchoSeleccionOriginal;
        altoOriginal = _altoSeleccionOriginal;

        if (anchoOriginal <= 0 || altoOriginal <= 0) return;

        int nuevoAncho = _rectSeleccion.width;
        int nuevoAlto = _rectSeleccion.height;

        // Escalar los píxeles al nuevo tamaño
        Color[] pixelesEscalados = new Color[nuevoAncho * nuevoAlto];

        for (int y = 0; y < nuevoAlto; y++)
        for (int x = 0; x < nuevoAncho; x++)
        {
            // Mapear coordenada nueva → coordenada original
            int ox = Mathf.FloorToInt((float)x / nuevoAncho * anchoOriginal);
            int oy = Mathf.FloorToInt((float)y / nuevoAlto * altoOriginal);
            ox = Mathf.Clamp(ox, 0, anchoOriginal - 1);
            oy = Mathf.Clamp(oy, 0, altoOriginal - 1);
            pixelesEscalados[y * nuevoAncho + x] = _pixelesSeleccionados[oy * anchoOriginal + ox];
        }

        // Limpiar área anterior y pintar la escalada
        Color[] vacios = new Color[128 * 128];
        for (int i = 0; i < vacios.Length; i++) vacios[i] = Color.clear;

        // Solo limpiar el área que ocupaba antes
        _lienzo.SetPixels(
            Mathf.Clamp(_rectSeleccionOriginal.x, 0, 127),
            Mathf.Clamp(_rectSeleccionOriginal.y, 0, 127),
            Mathf.Clamp(_rectSeleccionOriginal.width, 1, 128 - _rectSeleccionOriginal.x),
            Mathf.Clamp(_rectSeleccionOriginal.height, 1, 128 - _rectSeleccionOriginal.y),
            new Color[_rectSeleccionOriginal.width * _rectSeleccionOriginal.height]
        );

        // Pintar los píxeles escalados en la nueva posición
        _lienzo.SetPixels(
            _rectSeleccion.x, _rectSeleccion.y,
            nuevoAncho, nuevoAlto,
            pixelesEscalados
        );
        _lienzo.Apply();

        // Actualizar píxeles seleccionados al nuevo tamaño
        _pixelesSeleccionados = pixelesEscalados;
        _anchoSeleccionOriginal = nuevoAncho;
        _altoSeleccionOriginal = nuevoAlto;
        _rectSeleccionOriginal = _rectSeleccion;
    }
    // ── Utilidades ──────────────────────
    void CambiarHerramienta(Herramienta h)
    {
        _herramientaActual = h;

        // Limpiar selección al cambiar de herramienta
        if (h != Herramienta.Seleccion)
        {
            _haySeleccion = false;
            _moviendo = false;
            LimpiarMarco();
            if (panelMenuSeleccion != null)
                panelMenuSeleccion.SetActive(false);
        }

        Debug.Log("Herramienta: " + h);
    }

    void SeleccionarColor(Color c)
    {
        _colorActivo = c;
        if (imgColorActivo != null)
            imgColorActivo.color = c;
    }

    void LimpiarLienzo()
    {
        Color[] pixeles = new Color[128 * 128];
        for (int i = 0; i < pixeles.Length; i++)
            pixeles[i] = Color.clear;
        _lienzo?.SetPixels(pixeles);
        _lienzo?.Apply();
    }

    void GuardarPersonaje()
    {
        Debug.Log("Personaje guardado — conectaremos esto en el Módulo 2");
    }
}