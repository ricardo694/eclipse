using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Modal que aparece al pulsar "New Game" en el menu principal.
/// Muestra los personajes guardados del usuario y permite elegir uno.
/// Al confirmar, descarga el CharacterData completo y carga la escena de historia.
/// </summary>
public class PanelSeleccionPersonaje : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panelModal;

    [Header("Contenedor de tarjetas")]
    public Transform contenedorPersonajes;
    public GameObject prefabTarjetaPersonaje; // ver abajo como armarlo

    [Header("Estado")]
    public TMP_Text txtEstado;         // "Cargando...", "No tienes personajes", etc.
    public GameObject loadingSpinner;

    [Header("Acciones")]
    public Button btnCerrar;
    public Button btnJugarSinPersonaje; // jugar con el personaje por defecto
    public Button btnJugar;
    [Header("Escena")]
    public string nombreEscenaHistoria = "StoryMode";

    [Header("Servicio")]
    public SupabaseCharacterService characterService;

    // Estado interno
    private string _idSeleccionado = null;
    private List<GameObject> _tarjetas = new List<GameObject>();

    // ── API publica ──────────────────────────────────────────────────

    public void Abrir()
    {
        panelModal.SetActive(true);
        _idSeleccionado = null;
        LimpiarTarjetas();
        CargarPersonajes();

        btnCerrar?.onClick.RemoveAllListeners();
        btnCerrar?.onClick.AddListener(Cerrar);

        btnJugarSinPersonaje?.onClick.RemoveAllListeners();
        btnJugarSinPersonaje?.onClick.AddListener(JugarConPersonajePorDefecto);

        btnJugar?.onClick.RemoveAllListeners();
        btnJugar?.onClick.AddListener(ConfirmarSeleccion);
        if (btnJugar != null) btnJugar.interactable = false;
    }

    public void Cerrar()
    {
        panelModal.SetActive(false);
    }

    // ── Carga de personajes ──────────────────────────────────────────

    void CargarPersonajes()
    {
        SetEstado("Loading your characters...", true);

        if (!LoginSystem.IsLoggedIn)
        {
            SetEstado("You must be logged in to use custom characters.", false);
            return;
        }

        characterService.CargarMisPersonajes(
            onExito: (json) => {
                Debug.Log($"[Selector] onExito recibido, json length: {json.Length}");
                StartCoroutine(MostrarPersonajes(json));
            },
            onError: (err) => SetEstado($"Error loading: {err}", false)
        );
    }

    IEnumerator MostrarPersonajes(string json)
    {
         Debug.Log($"[Selector] MostrarPersonajes llamado con: {json}");
        SetEstado("", false);
        LimpiarTarjetas();

        List<PersonajeResumen> personajes = ParsearResumenes(json);

        if (personajes == null || personajes.Count == 0)
        {
            SetEstado("You have no saved characters.\nCreate one in the Character Editor!", false);
            yield break;
        }

        foreach (var p in personajes)
        {
            GameObject tarjeta = Instantiate(prefabTarjetaPersonaje, contenedorPersonajes);
            _tarjetas.Add(tarjeta);

            // Configurar tarjeta
            TarjetaPersonaje comp = tarjeta.GetComponent<TarjetaPersonaje>();
            if (comp != null)
            {
                comp.Configurar(p.id, p.nombre, p.fechaCreacion, () => SeleccionarPersonaje(p.id));
            }
            else
            {
                // Fallback si no tiene el componente
                TMP_Text txt = tarjeta.GetComponentInChildren<TMP_Text>();
                if (txt != null) txt.text = p.nombre;

                Button btn = tarjeta.GetComponent<Button>();
                string idCapturado = p.id;
                btn?.onClick.AddListener(() => SeleccionarPersonaje(idCapturado));
            }

            yield return null; // un frame entre tarjetas para no congelar
        }
    }

    // ── Seleccion ────────────────────────────────────────────────────

    void SeleccionarPersonaje(string id)
    {
        _idSeleccionado = id;

        // Activar boton de jugar
        if (btnJugar != null) btnJugar.interactable = true;
    }

    void ConfirmarSeleccion()
    {
        if (string.IsNullOrEmpty(_idSeleccionado)) return;
        SetEstado("Downloading character...", true);
        characterService.CargarPersonajePorId(_idSeleccionado,
            onExito: (json) => StartCoroutine(CargarYEntrar(json)),
            onError: (err)  => SetEstado($"Error: {err}", false)
        );
    }

    IEnumerator CargarYEntrar(string json)
    {
        CharacterData data = DeserializarPersonaje(json);

        if (data == null)
        {
            SetEstado("Error loading character data.", false);
            yield break;
        }

        CharacterDataHolder.Instance?.SetData(data);

         // DEBUG

        Debug.Log($"[Selector] Personaje cargado: {data.nombrePersonaje}");
        
        Debug.Log($"[Selector] Holder instance: {CharacterDataHolder.Instance}");
        Debug.Log($"[Selector] Datos guardados: {CharacterDataHolder.Instance?.DatosActuales?.nombrePersonaje}");

  

        yield return null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    void JugarConPersonajePorDefecto()
    {
        CharacterDataHolder.Instance?.SetData(null);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // ── Deserializacion ──────────────────────────────────────────────

    List<PersonajeResumen> ParsearResumenes(string json)
    {
        var lista = new List<PersonajeResumen>();
        if (string.IsNullOrEmpty(json) || json.Trim() == "[]") return lista;

        // Parseo manual del array JSON
        json = json.Trim();
        if (json.StartsWith("[")) json = json.Substring(1);
        if (json.EndsWith("]"))   json = json.Substring(0, json.Length - 1);

        string[] objetos = SplitJsonObjects(json);
        foreach (string obj in objetos)
        {
            string id     = ExtraerCampo(obj, "id");
            string nombre = ExtraerCampo(obj, "nombre");
            string fecha  = ExtraerCampo(obj, "created_at");

            if (!string.IsNullOrEmpty(id))
                lista.Add(new PersonajeResumen { id = id, nombre = nombre, fechaCreacion = fecha });
        }

        return lista;
    }

    CharacterData DeserializarPersonaje(string json)
    {
        try
        {
            // Sacar el objeto del array
            json = json.Trim();
            if (json.StartsWith("["))
            {
                int inicio = json.IndexOf('{');
                int fin    = json.LastIndexOf('}');
                if (inicio < 0 || fin < 0) return null;
                json = json.Substring(inicio, fin - inicio + 1);
            }

            string nombre      = ExtraerCampo(json, "nombre");
            string fechaStr    = ExtraerCampo(json, "created_at");
            string id          = ExtraerCampo(json, "id");
            string frameData   = ExtraerCampoLargo(json, "frame_data");
            string hitboxData  = ExtraerCampoLargo(json, "hitbox_data");

            CharacterData data = new CharacterData
            {
                id              = id,
                nombrePersonaje = nombre,
                fechaCreacion   = fechaStr
            };

            // Deserializar frames
            if (!string.IsNullOrEmpty(frameData))
                data.todasLasAnimaciones = DeserializarFrames(frameData);

            // Deserializar hitboxes
            if (!string.IsNullOrEmpty(hitboxData))
                DeserializarHitboxes(hitboxData, data);

            // Sprite base = primer frame idle
            if (data.todasLasAnimaciones?.Count > 0 &&
                data.todasLasAnimaciones[0]?.Count > 0)
                data.spriteBase = data.todasLasAnimaciones[0][0];

            return data;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Selector] Error deserializando: {ex.Message}");
            return null;
        }
    }

    List<List<Texture2D>> DeserializarFrames(string frameDataJson)
    {
        var resultado = new List<List<Texture2D>>();
        string[] animNombres = { "idle", "run", "jump", "attack", "crouch", "damage" };

        foreach (string anim in animNombres)
        {
            var frames = new List<Texture2D>();
            string arrayStr = ExtraerArray(frameDataJson, anim);

            if (!string.IsNullOrEmpty(arrayStr))
            {
                string[] b64s = SplitB64Array(arrayStr);
                foreach (string b64 in b64s)
                {
                    if (string.IsNullOrEmpty(b64)) continue;
                    try
                    {
                        byte[]    bytes = System.Convert.FromBase64String(b64);
                        Texture2D tex   = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                        tex.filterMode  = FilterMode.Point;
                        tex.LoadImage(bytes);
                        frames.Add(tex);
                    }
                    catch { /* frame corrupto, saltar */ }
                }
            }

            resultado.Add(frames);
        }

        return resultado;
    }

    void DeserializarHitboxes(string hitboxJson, CharacterData data)
    {
        // Body hitbox
        string bodyStr = ExtraerObjetoAnidado(hitboxJson, "body");
        if (!string.IsNullOrEmpty(bodyStr))
        {
            data.bodyHitbox = new HitboxData
            {
                offsetX = ParseFloat(ExtraerCampo(bodyStr, "offsetX")),
                offsetY = ParseFloat(ExtraerCampo(bodyStr, "offsetY")),
                width   = ParseFloat(ExtraerCampo(bodyStr, "width")),
                height  = ParseFloat(ExtraerCampo(bodyStr, "height"))
            };
        }

        // Attack hitbox por frame
        string atkArrayStr = ExtraerArray(hitboxJson, "attack_por_frame");
        data.attackHitboxPorFrame = new List<HitboxData>();

        if (!string.IsNullOrEmpty(atkArrayStr))
        {
            string[] items = SplitJsonObjects(atkArrayStr);
            foreach (string item in items)
            {
                string trimmed = item.Trim();
                if (trimmed == "null" || string.IsNullOrEmpty(trimmed))
                {
                    data.attackHitboxPorFrame.Add(null);
                }
                else
                {
                    data.attackHitboxPorFrame.Add(new HitboxData
                    {
                        offsetX = ParseFloat(ExtraerCampo(trimmed, "offsetX")),
                        offsetY = ParseFloat(ExtraerCampo(trimmed, "offsetY")),
                        width   = ParseFloat(ExtraerCampo(trimmed, "width")),
                        height  = ParseFloat(ExtraerCampo(trimmed, "height"))
                    });
                }
            }
        }
    }

    // ── UI helpers ───────────────────────────────────────────────────

    void SetEstado(string mensaje, bool loading)
    {
        if (txtEstado     != null) { txtEstado.text = mensaje; txtEstado.gameObject.SetActive(!string.IsNullOrEmpty(mensaje)); }
        if (loadingSpinner != null) loadingSpinner.SetActive(loading);
    }

    void LimpiarTarjetas()
    {
        foreach (var t in _tarjetas)
            if (t != null) Destroy(t);
        _tarjetas.Clear();
    }

    // ── Parseo JSON manual ───────────────────────────────────────────

    string ExtraerCampo(string json, string campo)
    {
        // Busca "campo":"valor" o "campo":valor
        string keyQ = $"\"{campo}\":\"";
        int idx = json.IndexOf(keyQ, System.StringComparison.Ordinal);
        if (idx >= 0)
        {
            int start = idx + keyQ.Length;
            int end   = json.IndexOf('"', start);
            return end < 0 ? null : json.Substring(start, end - start);
        }
        string keyN = $"\"{campo}\":";
        idx = json.IndexOf(keyN, System.StringComparison.Ordinal);
        if (idx >= 0)
        {
            int start = idx + keyN.Length;
            int end   = json.IndexOfAny(new char[] { ',', '}', ']' }, start);
            return end < 0 ? null : json.Substring(start, end - start).Trim();
        }
        return null;
    }

    // Para campos que contienen JSON escapado como string
    string ExtraerCampoLargo(string json, string campo)
    {
        string key = $"\"{campo}\":\"";
        int idx = json.IndexOf(key, System.StringComparison.Ordinal);
        if (idx < 0) return null;

        int start = idx + key.Length;
        // Buscar el cierre de la string respetando escapes
        int i = start;
        var sb = new System.Text.StringBuilder();
        while (i < json.Length)
        {
            char c = json[i];
            if (c == '\\' && i + 1 < json.Length)
            {
                char next = json[i + 1];
                if (next == '"')       { sb.Append('"');  i += 2; continue; }
                if (next == '\\')      { sb.Append('\\'); i += 2; continue; }
                if (next == 'n')       { sb.Append('\n'); i += 2; continue; }
                sb.Append(c); i++; continue;
            }
            if (c == '"') break;
            sb.Append(c);
            i++;
        }
        return sb.ToString();
    }

    string ExtraerArray(string json, string campo)
    {
        string key = $"\"{campo}\":[";
        int idx = json.IndexOf(key, System.StringComparison.Ordinal);
        if (idx < 0) return null;

        int start = idx + key.Length;
        int depth = 1;
        int i     = start;
        while (i < json.Length && depth > 0)
        {
            if (json[i] == '[') depth++;
            else if (json[i] == ']') depth--;
            if (depth > 0) i++;
        }
        return json.Substring(start, i - start);
    }

    string ExtraerObjetoAnidado(string json, string campo)
    {
        string key = $"\"{campo}\":{{";
        int idx = json.IndexOf(key, System.StringComparison.Ordinal);
        if (idx < 0) return null;

        int start = idx + key.Length - 1;
        int depth = 1;
        int i     = start + 1;
        while (i < json.Length && depth > 0)
        {
            if (json[i] == '{') depth++;
            else if (json[i] == '}') depth--;
            i++;
        }
        return json.Substring(start, i - start);
    }

    string[] SplitJsonObjects(string json)
    {
        var result = new List<string>();
        int depth  = 0;
        int start  = 0;

        for (int i = 0; i < json.Length; i++)
        {
            char c = json[i];
            if (c == '{') depth++;
            else if (c == '}')
            {
                depth--;
                if (depth == 0)
                {
                    result.Add(json.Substring(start, i - start + 1));
                    start = i + 2; // saltar la coma
                }
            }
        }
        return result.ToArray();
    }

    string[] SplitB64Array(string arrayStr)
    {
        // El array es: "base64...", "base64...", ...
        var result = new List<string>();
        int i = 0;
        while (i < arrayStr.Length)
        {
            if (arrayStr[i] == '"')
            {
                int end = arrayStr.IndexOf('"', i + 1);
                if (end < 0) break;
                result.Add(arrayStr.Substring(i + 1, end - i - 1));
                i = end + 2; // saltar coma
            }
            else i++;
        }
        return result.ToArray();
    }

    float ParseFloat(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0f;
        float.TryParse(s, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float v);
        return v;
    }

    // ── Clases auxiliares ────────────────────────────────────────────

    private class PersonajeResumen
    {
        public string id;
        public string nombre;
        public string fechaCreacion;
    }
}