using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class PixelArtEditor : MonoBehaviour
{
    [Header("Lienzo")]
    public RawImage lienzoDisplay;
    public RawImage guiaDisplay;

    [Header("Herramientas")]
    public Button btnPincel;
    public Button btnBorrador;
    public Button btnRelleno;
    public Button btnCuentagotas;
    public Slider sliderTamanio;

    [Header("Color")]
    public Image imgColorActivo;

    [Header("Paleta")]
    public Transform contenedorPaleta;
    public GameObject prefabSwatch;

    [Header("Guia")]
    public Toggle toggleGuia;

    [Header("Acciones")]
    public Button btnGuardar;
    public Button btnLimpiar;

    // ── Estado interno ──────────────────
    private Texture2D _lienzo;
    private Color _colorActivo = Color.red;
    private int _tamanio = 1;
    private bool _pintando = false;
    private bool _cuentagotasActivo = false;

    private enum Herramienta { Pincel, Borrador, Relleno, Cuentagotas }
    private Herramienta _herramientaActual = Herramienta.Pincel;

    private static readonly Color[] PALETA = {
        new Color(0.90f, 0.22f, 0.27f),
        new Color(0.96f, 0.64f, 0.38f),
        new Color(0.91f, 0.77f, 0.41f),
        new Color(0.16f, 0.62f, 0.56f),
        new Color(0.27f, 0.48f, 0.62f),
        new Color(0.66f, 0.72f, 0.87f),
        new Color(1.00f, 1.00f, 1.00f),
        new Color(0.78f, 0.72f, 1.00f),
        new Color(1.00f, 0.42f, 0.62f),
        new Color(0.02f, 0.84f, 0.63f),
        new Color(0.08f, 0.08f, 0.15f),
        new Color(0.40f, 0.40f, 0.50f),
        new Color(0.25f, 0.25f, 0.35f),
        new Color(0.60f, 0.45f, 0.25f),
        new Color(0.17f, 0.40f, 0.53f),
        new Color(0.00f, 0.00f, 0.00f),
    };

    // ── Inicio ──────────────────────────
    void Start()
    {
        InicializarLienzo();
        InicializarPaleta();
        ConectarBotones();
    }

    void InicializarLienzo()
    {
        _lienzo = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        _lienzo.filterMode = FilterMode.Point;
        LimpiarLienzo();
        lienzoDisplay.texture = _lienzo;
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
        sliderTamanio?.onValueChanged.AddListener(val => _tamanio = Mathf.RoundToInt(val));
        toggleGuia?.onValueChanged.AddListener(activar => {
            if (guiaDisplay != null)
                guiaDisplay.gameObject.SetActive(activar);
        });
    }

    // ── Input de dibujo ─────────────────
    public void OnPointerDown(BaseEventData data)
    {
        _pintando = true;
        ProcesarInput((PointerEventData)data);
    }

    public void OnDrag(BaseEventData data)
    {
        if (_pintando) ProcesarInput((PointerEventData)data);
    }

    public void OnPointerUp(BaseEventData data)
    {
        _pintando = false;
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
        }

        _lienzo.Apply();
    }

    // ── Herramientas ────────────────────
    void PintarPinchazo(int cx, int cy)
    {
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

    // ── Utilidades ──────────────────────
    void CambiarHerramienta(Herramienta h)
    {
        _herramientaActual = h;
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