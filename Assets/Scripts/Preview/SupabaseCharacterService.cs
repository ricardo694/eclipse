using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Sube y descarga personajes de la tabla 'characters' en Supabase.
/// Usa el mismo patron de UnityWebRequest que LoginSystem y RegisterSystem.
/// </summary>
public class SupabaseCharacterService : MonoBehaviour
{
    [Header("Supabase Config")]
    public string supabaseUrl = "https://ihwughhiqdoiwkctbdcr.supabase.co";
    public string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imlod3VnaGhpcWRvaXdrY3RiZGNyIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzI3ODY2MTUsImV4cCI6MjA4ODM2MjYxNX0.dtKkp9l2G1hG4cLKYADZr93c0ZHKbjbgLVfyibXYkCg";

    // ── Subir personaje ──────────────────────────────────────────────────────

    /// <summary>
    /// Sube el CharacterData actual a Supabase.
    /// onExito: recibe el id del personaje guardado.
    /// onError: recibe el mensaje de error.
    /// </summary>
    public void SubirPersonaje(CharacterData data, bool esPublico,
        Action<string> onExito, Action<string> onError)
    {
        if (string.IsNullOrEmpty(LoginSystem.AccessToken))
        {
            onError?.Invoke("No hay sesion activa.");
            return;
        }

        if (data == null)
        {
            onError?.Invoke("No hay datos de personaje.");
            return;
        }

        StartCoroutine(SubirCoroutine(data, esPublico, onExito, onError));
    }

    private IEnumerator SubirCoroutine(CharacterData data, bool esPublico,
        Action<string> onExito, Action<string> onError)
    {
        Debug.Log("[Characters] Serializando personaje...");

        // Serializar fuera del frame para no bloquear (son operaciones pesadas)
        string frameDataJson  = CharacterDataSerializer.SerializarFrames(data);
        string hitboxDataJson = CharacterDataSerializer.SerializarHitboxes(data);

        Debug.Log($"[Characters] Frame data size: {frameDataJson.Length} chars");
        Debug.Log($"[Characters] Hitbox data: {hitboxDataJson}");

        // Escapar los JSON para meterlos como strings dentro del JSON principal
        string frameDataEscaped  = EscapeJson(frameDataJson);
        string hitboxDataEscaped = EscapeJson(hitboxDataJson);
        string nombreEscaped     = EscapeJson(data.nombrePersonaje ?? "Sin nombre");
        string esPublicoStr      = esPublico ? "true" : "false";
        string playerId          = LoginSystem.UserId;

        string body = $"{{" +
                      $"\"player_id\":\"{playerId}\"," +
                      $"\"nombre\":\"{nombreEscaped}\"," +
                      $"\"es_publico\":{esPublicoStr}," +
                      $"\"frame_data\":\"{frameDataEscaped}\"," +
                      $"\"hitbox_data\":\"{hitboxDataEscaped}\"" +
                      $"}}";

        string url = $"{supabaseUrl}/rest/v1/characters";

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
            req.uploadHandler   = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type",  "application/json");
            req.SetRequestHeader("apikey",        supabaseKey);
            req.SetRequestHeader("Authorization", $"Bearer {LoginSystem.AccessToken}");
            req.SetRequestHeader("Prefer",        "return=representation");

            yield return req.SendWebRequest();

            string response = req.downloadHandler.text;
            Debug.Log($"[Characters] Respuesta upload: {response} | HTTP {req.responseCode}");

            if (req.result == UnityWebRequest.Result.Success ||
                req.responseCode == 201 || req.responseCode == 200)
            {
                string id = ExtractJsonField(response, "id");
                Debug.Log($"[Characters] Personaje subido. ID: {id}");
                onExito?.Invoke(id);
            }
            else
            {
                string error = $"HTTP {req.responseCode} – {req.error}\n{response}";
                Debug.LogError($"[Characters] Error al subir: {error}");
                onError?.Invoke(error);
            }
        }
    }

    // ── Cargar personajes del jugador ────────────────────────────────────────

    /// <summary>
    /// Carga todos los personajes del jugador logueado.
    /// onExito: recibe el JSON array de Supabase.
    /// </summary>
    public void CargarMisPersonajes(Action<string> onExito, Action<string> onError)
    {
        if (string.IsNullOrEmpty(LoginSystem.AccessToken))
        {
            onError?.Invoke("No hay sesion activa.");
            return;
        }

        StartCoroutine(CargarCoroutine(
            $"{supabaseUrl}/rest/v1/characters?player_id=eq.{LoginSystem.UserId}&select=id,nombre,created_at,es_publico",
            onExito, onError));
    }

    // ── Cargar personajes de la comunidad ────────────────────────────────────

    /// <summary>
    /// Carga los personajes publicos de la comunidad (para el panel de seleccion).
    /// </summary>
    public void CargarPersonajesComunidad(Action<string> onExito, Action<string> onError)
    {
        StartCoroutine(CargarCoroutine(
            $"{supabaseUrl}/rest/v1/characters?es_publico=eq.true&select=id,nombre,player_id,created_at",
            onExito, onError));
    }

    // ── Cargar un personaje completo por ID ──────────────────────────────────

    /// <summary>
    /// Descarga un personaje completo (con frames y hitboxes) por su ID.
    /// Usado al seleccionar un personaje para jugar.
    /// onExito: recibe el JSON completo del personaje.
    /// </summary>
    public void CargarPersonajePorId(string characterId,
        Action<string> onExito, Action<string> onError)
    {
        StartCoroutine(CargarCoroutine(
            $"{supabaseUrl}/rest/v1/characters?id=eq.{characterId}&select=*",
            onExito, onError));
    }

    private IEnumerator CargarCoroutine(string url,
        Action<string> onExito, Action<string> onError)
    {
        Debug.Log($"[Characters] GET {url}");

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.SetRequestHeader("apikey",        supabaseKey);
            req.SetRequestHeader("Authorization", $"Bearer {LoginSystem.AccessToken ?? supabaseKey}");
            req.SetRequestHeader("Accept",        "application/json");

            yield return req.SendWebRequest();

            string response = req.downloadHandler.text;
            Debug.Log($"[Characters] Respuesta: {response}");

            if (req.result == UnityWebRequest.Result.Success)
            {
                onExito?.Invoke(response);
            }
            else
            {
                string error = $"HTTP {req.responseCode} – {req.error}\n{response}";
                Debug.LogError($"[Characters] Error al cargar: {error}");
                onError?.Invoke(error);
            }
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private string ExtractJsonField(string json, string field)
    {
        if (string.IsNullOrEmpty(json)) return null;

        string working = json.Trim();
        if (working.StartsWith("["))
        {
            int firstObj = working.IndexOf('{');
            int lastObj  = working.LastIndexOf('}');
            if (firstObj < 0 || lastObj < 0) return null;
            working = working.Substring(firstObj, lastObj - firstObj + 1);
        }

        string keyWithQuotes = $"\"{field}\":\"";
        int start = working.IndexOf(keyWithQuotes, StringComparison.Ordinal);
        if (start >= 0)
        {
            start += keyWithQuotes.Length;
            int end = working.IndexOf('"', start);
            return end < 0 ? null : working.Substring(start, end - start);
        }

        return null;
    }

    private string EscapeJson(string value)
        => value.Replace("\\", "\\\\").Replace("\"", "\\\"");
}