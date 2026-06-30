public static class SupabaseErrorTranslator
{
    public static string Translate(string rawResponse)
    {
        if (string.IsNullOrEmpty(rawResponse))
            return "Ocurrió un error inesperado. Intenta de nuevo.";

        if (rawResponse.Contains("user_already_exists"))
            return "Ese correo ya está registrado. Intenta iniciar sesión.";

        if (rawResponse.Contains("invalid_credentials") || rawResponse.Contains("Invalid login credentials"))
            return "Usuario o contraseña incorrectos.";

        if (rawResponse.Contains("email_not_confirmed"))
            return "Debes confirmar tu correo antes de iniciar sesión.";

        if (rawResponse.Contains("weak_password"))
            return "La contraseña es demasiado débil. Usa al menos 6 caracteres.";

        if (rawResponse.Contains("over_request_rate_limit") || rawResponse.Contains("rate limit"))
            return "Demasiados intentos. Espera un momento y vuelve a intentar.";

        // Errores de red / conexión
        if (rawResponse.Contains("Cannot connect") || rawResponse.Contains("ConnectionFailed") ||
            rawResponse.Contains("NameResolutionFailure") || rawResponse.Contains("Unknown Error"))
            return "No se pudo conectar al servidor. Revisa tu conexión a internet.";

        return "Ocurrió un error. Intenta de nuevo.";
    }
}