using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// ============================================================
//  RegisterSystem.cs
//  Conecta Unity con Supabase Auth + tabla players
// ============================================================
public class RegisterSystem : MonoBehaviour
{
    [Header("Supabase Config")]
    public string supabaseUrl = "https://ihwughhiqdoiwkctbdcr.supabase.co";
    public string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imlod3VnaGhpcWRvaXdrY3RiZGNyIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzI3ODY2MTUsImV4cCI6MjA4ODM2MjYxNX0.dtKkp9l2G1hG4cLKYADZr93c0ZHKbjbgLVfyibXYkCg";

    // ----------------------------------------------------------
    //  TEST DE CONEXIÓN al arrancar la escena
    // ----------------------------------------------------------
    private void Start()
    {
        StartCoroutine(TestConnection());
    }




    /// <summary>
    /// Hace un ping ligero a la API de Supabase para verificar conectividad.
    /// </summary>
    private IEnumerator TestConnection()
    {
        string url = $"{supabaseUrl}/rest/v1/players?limit=1";

        using UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success ||
            request.responseCode == 200 || request.responseCode == 406)
        {
            // 406 "Not Acceptable" también indica que Supabase respondió → conexión OK
            Debug.Log(" [Supabase] Conectado correctamente.");
        }
        else
        {
            Debug.LogError($" [Supabase] Error de conexión: {request.error} " +
                           $"(HTTP {request.responseCode})");
        }
    }

    // ----------------------------------------------------------
    //  REGISTRO PÚBLICO: llama desde tu UI con los datos del form
    // ----------------------------------------------------------
    /// <summary>
    /// Registra un nuevo usuario en Supabase Auth y luego inserta
    /// su fila en la tabla 'players'.
    /// </summary>
    public void Register(string username, string email, string password, string confirmPassword)
    {
        // Validaciones básicas antes de tocar la red
        if (!ValidateFields(username, email, password, confirmPassword))
            return;

        StartCoroutine(RegisterCoroutine(username, email, password));
    }

    // ----------------------------------------------------------
    //  VALIDACIONES
    // ----------------------------------------------------------
    private bool ValidateFields(string username, string email, string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            Debug.LogWarning("⚠️ El nombre de usuario está vacío.");
            return false;
        }
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
        {
            Debug.LogWarning("⚠️ Email inválido.");
            return false;
        }
        if (password.Length < 6)
        {
            Debug.LogWarning("⚠️ La contraseña debe tener al menos 6 caracteres.");
            return false;
        }
        if (password != confirmPassword)
        {
            Debug.LogWarning("⚠️ Las contraseñas no coinciden.");
            return false;
        }
        return true;
    }

    // ----------------------------------------------------------
    //  COROUTINE PRINCIPAL DE REGISTRO
    // ----------------------------------------------------------
    private IEnumerator RegisterCoroutine(string username, string email, string password)
    {
        // ── PASO 1: Crear usuario en Supabase Auth ──────────────
        Debug.Log(" [Auth] Creando usuario...");
        

        string authUrl  = $"{supabaseUrl}/auth/v1/signup";
        string authJson = $"{{\"email\":\"{email}\",\"password\":\"{password}\"}}";

        string userId = null;
        string accessToken = null;

        using (UnityWebRequest authReq = new UnityWebRequest(authUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(authJson);
            authReq.uploadHandler   = new UploadHandlerRaw(bodyRaw);
            authReq.downloadHandler = new DownloadHandlerBuffer();
            authReq.SetRequestHeader("Content-Type",  "application/json");
            authReq.SetRequestHeader("apikey",        supabaseKey);
            authReq.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");

            yield return authReq.SendWebRequest();

            try
            {
                if (authReq.result != UnityWebRequest.Result.Success)
                    throw new Exception($"HTTP {authReq.responseCode} – {authReq.error}");

                string response = authReq.downloadHandler.text;
                userId      = ExtractJsonField(response, "id");
                accessToken = ExtractJsonField(response, "access_token");
                Debug.Log($" [Auth] Respuesta: {response}");

                // Parseo manual (sin Newtonsoft) – busca el campo "id"
                userId = ExtractJsonField(response, "id");
                // Si no hay access_token, el email confirmation está activado en Supabase
                if (string.IsNullOrEmpty(accessToken))
                {
                    Debug.LogWarning(" No se recibió access_token. ¿Tienes 'Confirm email' activado en Supabase?");
                    Debug.LogWarning(" Ve a Supabase → Authentication → Providers → Email → desactiva 'Confirm email'");
                    throw new Exception("Sin access_token no se puede insertar el perfil.");
                }

                if (string.IsNullOrEmpty(userId))
                    throw new Exception("No se pudo obtener el ID del usuario en la respuesta.");

                Debug.Log($" [Auth] Usuario creado. ID: {userId}");


            }
            catch (Exception ex)
            {
                Debug.LogError($" [Auth] Fallo al registrar: {ex.Message}");
                yield break; // Abortamos si Auth falla
            }
        }

        // ── PASO 2: Insertar fila en la tabla 'players' ─────────
        Debug.Log(" [DB] Insertando perfil en 'players'...");
        

        string dbUrl   = $"{supabaseUrl}/rest/v1/players";
        // id = UUID del usuario Auth, nivel y monedas en 0 por defecto
        string dbJson  = $"{{\"id\":\"{userId}\"," +
                         $"\"username\":\"{EscapeJson(username)}\"," +
                         $"\"level\":1," +
                         $"\"coins\":0}}";
        Debug.Log($" JSON a insertar: {dbJson}");
        Debug.Log($" AccessToken obtenido: {(string.IsNullOrEmpty(accessToken) ? "NULL " : "OK ")}");

        using (UnityWebRequest dbReq = new UnityWebRequest(dbUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(dbJson);
            dbReq.uploadHandler   = new UploadHandlerRaw(bodyRaw);
            dbReq.downloadHandler = new DownloadHandlerBuffer();
            dbReq.SetRequestHeader("Content-Type",  "application/json");
            dbReq.SetRequestHeader("apikey",        supabaseKey);
            dbReq.SetRequestHeader("Authorization", $"Bearer {accessToken}"); 
            dbReq.SetRequestHeader("Prefer",        "return=representation");

            yield return dbReq.SendWebRequest();

            try
            {
                if (dbReq.result != UnityWebRequest.Result.Success &&
                    dbReq.responseCode != 201)
                    throw new Exception($"HTTP {dbReq.responseCode} – {dbReq.error}\n{dbReq.downloadHandler.text}");

                Debug.Log($" [DB] Perfil insertado: {dbReq.downloadHandler.text}");
                Debug.Log($" Registro completo para '{username}'.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ [DB] Fallo al insertar perfil: {ex.Message}");
            }
        }
    }

    // ----------------------------------------------------------
    //  HELPERS
    // ----------------------------------------------------------

    /// <summary>
    /// Extrae el valor de un campo JSON de forma simple (sin librerías).
    /// Sólo funciona para valores de cadena primitivos.
    /// </summary>
    private string ExtractJsonField(string json, string field)
    {
        string key = $"\"{field}\":\"";
        int start  = json.IndexOf(key, StringComparison.Ordinal);
        if (start < 0) return null;
        start += key.Length;
        int end = json.IndexOf('"', start);
        return end < 0 ? null : json.Substring(start, end - start);
    }

    /// <summary>Escapa comillas y barras en strings JSON.</summary>
    private string EscapeJson(string value)
        => value.Replace("\\", "\\\\").Replace("\"", "\\\"");
}