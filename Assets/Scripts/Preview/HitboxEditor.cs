using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class HitboxEditor : MonoBehaviour
{
    // ── Referencias UI ───────────────────────────────────────────────────────

    [Header("Modal")]
    public GameObject panelHitboxModal;

    [Header("Vista sprite")]
    public RawImage rawImageSpriteHitbox;          // muestra el frame actual

    [Header("Hitboxes")]
    public RectTransform imgBodyHitbox;
    public RectTransform imgAtackHitbox;

    [Header("Controles body")]
    public Button btnTogleAtack;

    [Header("Navegacion frames de ataque")]
    public GameObject panelNavFrames;              // panel que agrupa los controles de navegacion
    public Button btnFrameAnterior;
    public Button btnFrameSiguiente;
    public TMP_Text txtFrameActual;                // "Frame 2 / 4"

    [Header("Acciones")]
    public Button btnCancelarHitbox;
    public Button btnGuardarHitbox;

    // ── Handles de redimension ───────────────────────────────────────────────
    // Asigna 4 RectTransforms pequenos (imagenes cuadradas ~12px) en las esquinas
    // de cada hitbox para que el usuario pueda redimensionar arrastrando.
    [Header("Handles redimension Body")]
    public RectTransform handleBodyBL;   // esquina inferior-izquierda
    public RectTransform handleBodyBR;
    public RectTransform handleBodyTL;
    public RectTransform handleBodyTR;

    [Header("Handles redimension Attack")]
    public RectTransform handleAtackBL;
    public RectTransform handleAtackBR;
    public RectTransform handleAtackTL;
    public RectTransform handleAtackTR;

    // ── Estado ───────────────────────────────────────────────────────────────

    private bool _modoAtaque = false;

    // Frames de la animacion de ataque
    private List<Texture2D> _framesAtaque;
    private int _frameAtaqueSeleccionado = 0;   // indice del frame con hitbox

    // Para drag de posicion
    private RectTransform _dragging;
    private Vector2 _dragOffset;

    // Para redimension
    private RectTransform _resizing;             // hitbox que se esta redimensionando
    private RectTransform _handleActivo;         // handle que inicio el resize
    private Vector2 _resizeAnchorPos;            // posicion del corner opuesto (fijo)
    private Vector2 _resizeAnchorSize;           // tamano al inicio del resize

    // Referencia al area de vista
    private RectTransform _vistaRect;

    // Colores
    private readonly Color COLOR_BODY   = new Color(0.2f, 0.9f, 0.2f, 0.45f);
    private readonly Color COLOR_ATTACK = new Color(1.0f, 0.2f, 0.2f, 0.45f);
    private readonly Color COLOR_HANDLE = new Color(1.0f, 1.0f, 1.0f, 0.90f);

    // Tamano de los handles en pixeles UI
    private const float HANDLE_SIZE = 12f;

    // ── API publica ──────────────────────────────────────────────────────────

    /// <summary>
    /// Abre el editor de hitbox.
    /// spriteIdle  : primer frame idle para mostrar de fondo.
    /// framesAtaque: lista de frames de la animacion de ataque.
    /// </summary>
    public void Abrir(Texture2D spriteIdle, List<Texture2D> framesAtaque)
    {
        panelHitboxModal.SetActive(true);

        _framesAtaque = framesAtaque ?? new List<Texture2D>();
        _frameAtaqueSeleccionado = 0;

        // Vista inicial: idle
        MostrarSprite(spriteIdle);

        // Area de vista
        _vistaRect = rawImageSpriteHitbox?.transform.parent.GetComponent<RectTransform>();

        // Posicion y tamano inicial de hitboxes
        ResetBodyHitbox();
        ResetAttackHitbox();

        // Modo inicial: body
        _modoAtaque = false;
        imgAtackHitbox.gameObject.SetActive(false);
        OcultarHandlesAtaque();

        ActualizarColores();
        ActualizarPanelNavFrames();
        ConectarBotones();

        // Conectar drag y resize de body
        ConectarDragYResize(imgBodyHitbox, handleBodyBL, handleBodyBR, handleBodyTL, handleBodyTR);
        // Conectar drag y resize de attack
        ConectarDragYResize(imgAtackHitbox, handleAtackBL, handleAtackBR, handleAtackTL, handleAtackTR);

        ActualizarPosicionHandles(imgBodyHitbox,
            handleBodyBL, handleBodyBR, handleBodyTL, handleBodyTR);
        ActualizarPosicionHandles(imgAtackHitbox,
            handleAtackBL, handleAtackBR, handleAtackTL, handleAtackTR);
    }

    // ── Reset ────────────────────────────────────────────────────────────────

    void ResetBodyHitbox()
    {
        imgBodyHitbox.anchoredPosition = Vector2.zero;
        imgBodyHitbox.sizeDelta        = new Vector2(80, 100);
        imgBodyHitbox.gameObject.SetActive(true);
    }

    void ResetAttackHitbox()
    {
        imgAtackHitbox.anchoredPosition = new Vector2(70, 0);
        imgAtackHitbox.sizeDelta        = new Vector2(60, 40);
    }

    // ── Botones ──────────────────────────────────────────────────────────────

    void ConectarBotones()
    {
        btnTogleAtack?.onClick.RemoveAllListeners();
        btnFrameAnterior?.onClick.RemoveAllListeners();
        btnFrameSiguiente?.onClick.RemoveAllListeners();
        btnCancelarHitbox?.onClick.RemoveAllListeners();
        btnGuardarHitbox?.onClick.RemoveAllListeners();

        btnTogleAtack?.onClick.AddListener(ToggleModoAtaque);
        btnFrameAnterior?.onClick.AddListener(FrameAnterior);
        btnFrameSiguiente?.onClick.AddListener(FrameSiguiente);
        btnCancelarHitbox?.onClick.AddListener(Cerrar);
        btnGuardarHitbox?.onClick.AddListener(Guardar);
    }

    void ToggleModoAtaque()
    {
        _modoAtaque = !_modoAtaque;

        if (_modoAtaque)
        {
            // Cambiar sprite al frame de ataque seleccionado
            MostrarFrameAtaqueActual();
            imgAtackHitbox.gameObject.SetActive(true);
            MostrarHandlesAtaque();
        }
        else
        {
            // Volver al sprite idle
            MostrarSprite(ObtenerSpriteIdle());
            imgAtackHitbox.gameObject.SetActive(false);
            OcultarHandlesAtaque();
        }

        ActualizarPanelNavFrames();
        ActualizarTextoBotonAtaque();
    }

    void FrameAnterior()
    {
        if (_framesAtaque.Count == 0) return;
        _frameAtaqueSeleccionado =
            (_frameAtaqueSeleccionado - 1 + _framesAtaque.Count) % _framesAtaque.Count;
        MostrarFrameAtaqueActual();
        ActualizarTextoNavFrames();
    }

    void FrameSiguiente()
    {
        if (_framesAtaque.Count == 0) return;
        _frameAtaqueSeleccionado =
            (_frameAtaqueSeleccionado + 1) % _framesAtaque.Count;
        MostrarFrameAtaqueActual();
        ActualizarTextoNavFrames();
    }

    // ── Sprite display ───────────────────────────────────────────────────────

    void MostrarSprite(Texture2D tex)
    {
        if (rawImageSpriteHitbox == null || tex == null) return;
        rawImageSpriteHitbox.texture = tex;
        rawImageSpriteHitbox.uvRect  = new Rect(0, 1, 1, -1);   // flip Y
    }

    void MostrarFrameAtaqueActual()
    {
        if (_framesAtaque == null || _framesAtaque.Count == 0) return;
        MostrarSprite(_framesAtaque[_frameAtaqueSeleccionado]);
        ActualizarTextoNavFrames();
    }

    Texture2D ObtenerSpriteIdle()
    {
        CharacterData data = CharacterDataHolder.Instance?.DatosActuales;
        if (data?.todasLasAnimaciones != null &&
            data.todasLasAnimaciones.Count > 0 &&
            data.todasLasAnimaciones[0].Count > 0)
            return data.todasLasAnimaciones[0][0];
        return null;
    }

    // ── UI helpers ───────────────────────────────────────────────────────────

    void ActualizarColores()
    {
        SetImgColor(imgBodyHitbox,   COLOR_BODY);
        SetImgColor(imgAtackHitbox,  COLOR_ATTACK);
        SetHandleColors(handleBodyBL, handleBodyBR, handleBodyTL, handleBodyTR);
        SetHandleColors(handleAtackBL, handleAtackBR, handleAtackTL, handleAtackTR);
    }

    void SetImgColor(RectTransform rt, Color c)
    {
        Image img = rt?.GetComponent<Image>();
        if (img != null) img.color = c;
    }

    void SetHandleColors(params RectTransform[] handles)
    {
        foreach (var h in handles)
            SetImgColor(h, COLOR_HANDLE);
    }

    void ActualizarTextoBotonAtaque()
    {
        if (btnTogleAtack == null) return;
        TMP_Text txt = btnTogleAtack.GetComponentInChildren<TMP_Text>();
        if (txt != null)
            txt.text = _modoAtaque ? "Hide attack hitbox" : "Configure attack hitbox";
    }

    void ActualizarPanelNavFrames()
    {
        if (panelNavFrames != null)
            panelNavFrames.SetActive(_modoAtaque && _framesAtaque.Count > 0);
    }

    void ActualizarTextoNavFrames()
    {
        if (txtFrameActual != null)
            txtFrameActual.text =
                $"Frame {_frameAtaqueSeleccionado + 1} / {_framesAtaque.Count}";
    }

    void MostrarHandlesAtaque()
    {
        SetActiveHandles(true, handleAtackBL, handleAtackBR, handleAtackTL, handleAtackTR);
    }

    void OcultarHandlesAtaque()
    {
        SetActiveHandles(false, handleAtackBL, handleAtackBR, handleAtackTL, handleAtackTR);
    }

    void SetActiveHandles(bool active, params RectTransform[] handles)
    {
        foreach (var h in handles)
            h?.gameObject.SetActive(active);
    }

    // ── Drag y Resize ────────────────────────────────────────────────────────

    /// <summary>
    /// Conecta drag de posicion en la hitbox y resize en cada handle.
    /// </summary>
    void ConectarDragYResize(RectTransform hitbox,
        RectTransform hBL, RectTransform hBR,
        RectTransform hTL, RectTransform hTR)
    {
        // Drag de posicion en la hitbox principal
        ConectarEventos(hitbox,
            onDown: (ped) => IniciarDrag(hitbox, ped),
            onDrag: (ped) => HacerDrag(ped),
            onUp:   (ped) => TerminarInteraccion());

        // Resize desde cada esquina
        // BL = Bottom-Left → anchor opuesto es TR
        ConectarResize(hBL, hitbox, esquinaOpuesta: () => GetCornerTR(hitbox));
        ConectarResize(hBR, hitbox, esquinaOpuesta: () => GetCornerTL(hitbox));
        ConectarResize(hTL, hitbox, esquinaOpuesta: () => GetCornerBR(hitbox));
        ConectarResize(hTR, hitbox, esquinaOpuesta: () => GetCornerBL(hitbox));
    }

    void ConectarResize(RectTransform handle, RectTransform hitbox,
        System.Func<Vector2> esquinaOpuesta)
    {
        if (handle == null) return;
        ConectarEventos(handle,
            onDown: (ped) => IniciarResize(handle, hitbox, esquinaOpuesta()),
            onDrag: (ped) => HacerResize(hitbox, ped),
            onUp:   (ped) => TerminarInteraccion());
    }

    void ConectarEventos(RectTransform rt,
        System.Action<PointerEventData> onDown,
        System.Action<PointerEventData> onDrag,
        System.Action<PointerEventData> onUp)
    {
        EventTrigger trigger = rt.GetComponent<EventTrigger>();
        if (trigger == null) trigger = rt.gameObject.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        AddEntry(trigger, EventTriggerType.PointerDown, onDown);
        AddEntry(trigger, EventTriggerType.Drag,        onDrag);
        AddEntry(trigger, EventTriggerType.PointerUp,   onUp);
    }

    void AddEntry(EventTrigger trigger, EventTriggerType type,
        System.Action<PointerEventData> action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((data) => action((PointerEventData)data));
        trigger.triggers.Add(entry);
    }

    // ── Drag de posicion ─────────────────────────────────────────────────────

    void IniciarDrag(RectTransform rt, PointerEventData data)
    {
        _dragging = rt;
        _resizing = null;
        // Usamos _vistaRect como parent (mismo espacio para hitboxes y handles)
        ScreenToLocal(_vistaRect, data, out Vector2 local);
        _dragOffset = rt.anchoredPosition - local;
    }

    void HacerDrag(PointerEventData data)
    {
        if (_dragging == null) return;
        ScreenToLocal(_vistaRect, data, out Vector2 local);
        Vector2 pos = local + _dragOffset;
        _dragging.anchoredPosition = ClampPosicion(_dragging, pos);

        // Actualizar handles de la hitbox que se esta moviendo
        if (_dragging == imgBodyHitbox)
            ActualizarPosicionHandles(imgBodyHitbox,
                handleBodyBL, handleBodyBR, handleBodyTL, handleBodyTR);
        else if (_dragging == imgAtackHitbox)
            ActualizarPosicionHandles(imgAtackHitbox,
                handleAtackBL, handleAtackBR, handleAtackTL, handleAtackTR);
    }

    // ── Resize ───────────────────────────────────────────────────────────────

    void IniciarResize(RectTransform handle, RectTransform hitbox, Vector2 anchorPos)
    {
        _resizing       = hitbox;
        _handleActivo   = handle;
        _resizeAnchorPos = anchorPos;
        _dragging       = null;
    }

    void HacerResize(RectTransform hitbox, PointerEventData data)
    {
        if (_resizing == null || _resizing != hitbox) return;

        // Los handles estan en Panel_VistaHitbox, igual que la hitbox.
        // Usamos ese mismo parent para convertir la posicion de pantalla.
        ScreenToLocal(_vistaRect, data, out Vector2 local);

        // El anchor fijo (_resizeAnchorPos) y el cursor (local) definen el nuevo rect.
        float minX = Mathf.Min(_resizeAnchorPos.x, local.x);
        float minY = Mathf.Min(_resizeAnchorPos.y, local.y);
        float w    = Mathf.Abs(local.x - _resizeAnchorPos.x);
        float h    = Mathf.Abs(local.y - _resizeAnchorPos.y);

        // Tamano minimo
        w = Mathf.Max(w, 20f);
        h = Mathf.Max(h, 20f);

        // anchoredPosition = centro del rect
        hitbox.anchoredPosition = new Vector2(minX + w * 0.5f, minY + h * 0.5f);
        hitbox.sizeDelta        = new Vector2(w, h);

        // Actualizar handles
        if (hitbox == imgBodyHitbox)
            ActualizarPosicionHandles(imgBodyHitbox,
                handleBodyBL, handleBodyBR, handleBodyTL, handleBodyTR);
        else if (hitbox == imgAtackHitbox)
            ActualizarPosicionHandles(imgAtackHitbox,
                handleAtackBL, handleAtackBR, handleAtackTL, handleAtackTR);
    }

    void TerminarInteraccion()
    {
        _dragging = null;
        _resizing = null;
    }

    // ── Handles: posicion ────────────────────────────────────────────────────

    void ActualizarPosicionHandles(RectTransform hitbox,
        RectTransform hBL, RectTransform hBR,
        RectTransform hTL, RectTransform hTR)
    {
        SetHandlePos(hBL, GetCornerBL(hitbox));
        SetHandlePos(hBR, GetCornerBR(hitbox));
        SetHandlePos(hTL, GetCornerTL(hitbox));
        SetHandlePos(hTR, GetCornerTR(hitbox));
    }

    void SetHandlePos(RectTransform h, Vector2 pos)
    {
        if (h == null) return;
        h.anchoredPosition = pos;
        h.sizeDelta        = new Vector2(HANDLE_SIZE, HANDLE_SIZE);
    }

    // Esquinas en coordenadas locales del parent.
    // anchoredPosition es el CENTRO de la hitbox (pivot = 0.5, 0.5).
    // Los handles viven en el mismo parent (Panel_VistaHitbox), asi que
    // sus anchoredPosition deben estar en ese mismo espacio → suma/resta directa.
    Vector2 GetCornerBL(RectTransform rt) =>
        rt.anchoredPosition + new Vector2(-rt.sizeDelta.x * 0.5f, -rt.sizeDelta.y * 0.5f);
    Vector2 GetCornerBR(RectTransform rt) =>
        rt.anchoredPosition + new Vector2( rt.sizeDelta.x * 0.5f, -rt.sizeDelta.y * 0.5f);
    Vector2 GetCornerTL(RectTransform rt) =>
        rt.anchoredPosition + new Vector2(-rt.sizeDelta.x * 0.5f,  rt.sizeDelta.y * 0.5f);
    Vector2 GetCornerTR(RectTransform rt) =>
        rt.anchoredPosition + new Vector2( rt.sizeDelta.x * 0.5f,  rt.sizeDelta.y * 0.5f);

    // ── Utilidades ───────────────────────────────────────────────────────────

    bool ScreenToLocal(RectTransform parent, PointerEventData data, out Vector2 local)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent, data.position, data.pressEventCamera, out local);
    }

    Vector2 ClampPosicion(RectTransform rt, Vector2 pos)
    {
        if (_vistaRect == null) return pos;
        float halfW = rt.sizeDelta.x / 2f;
        float halfH = rt.sizeDelta.y / 2f;
        float limX  = _vistaRect.sizeDelta.x / 2f;
        float limY  = _vistaRect.sizeDelta.y / 2f;
        pos.x = Mathf.Clamp(pos.x, -limX + halfW, limX - halfW);
        pos.y = Mathf.Clamp(pos.y, -limY + halfH, limY - halfH);
        return pos;
    }

    // ── Guardar ──────────────────────────────────────────────────────────────

    public void Guardar()
    {
        float areaSize = _vistaRect != null ? _vistaRect.sizeDelta.x : 256f;

        CharacterData data = CharacterDataHolder.Instance?.DatosActuales;
        if (data == null)
        {
            Debug.LogError("[Hitbox] No hay CharacterData.");
            return;
        }

        // Body hitbox
        data.bodyHitbox = new HitboxData
        {
            offsetX   = imgBodyHitbox.anchoredPosition.x / areaSize,
            offsetY   = imgBodyHitbox.anchoredPosition.y / areaSize,
            width     = imgBodyHitbox.sizeDelta.x        / areaSize,
            height    = imgBodyHitbox.sizeDelta.y        / areaSize,
            esCirculo = false
        };

        // Attack hitbox por frame
        // Inicializar lista con null para cada frame
        int totalFrames = _framesAtaque.Count;
        data.attackHitboxPorFrame = new List<HitboxData>();
        for (int i = 0; i < totalFrames; i++)
            data.attackHitboxPorFrame.Add(null);

        // Solo guardar en el frame seleccionado (si el modo ataque estaba activo)
        if (_modoAtaque && totalFrames > 0)
        {
            data.attackHitboxPorFrame[_frameAtaqueSeleccionado] = new HitboxData
            {
                offsetX   = imgAtackHitbox.anchoredPosition.x / areaSize,
                offsetY   = imgAtackHitbox.anchoredPosition.y / areaSize,
                width     = imgAtackHitbox.sizeDelta.x        / areaSize,
                height    = imgAtackHitbox.sizeDelta.y        / areaSize,
                esCirculo = false
            };

            Debug.Log($"[Hitbox] Attack hitbox en frame {_frameAtaqueSeleccionado}. " +
                      $"Guardado. Listo para subir a Supabase.");
        }
        else
        {
            Debug.Log("[Hitbox] Sin attack hitbox. Body guardado.");
        }

        panelHitboxModal.SetActive(false);
        FindObjectOfType<PreviewPersonaje>()?.OnHitboxConfirmado();
    }

    void Cerrar()
    {
        panelHitboxModal.SetActive(false);
    }
}