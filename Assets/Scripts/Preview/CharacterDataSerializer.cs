using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Convierte un CharacterData completo a JSON listo para subir a Supabase.
/// Los frames de cada animacion se convierten a PNG en base64.
/// </summary>
public static class CharacterDataSerializer
{
    // Indices de animaciones (igual que en PreviewPersonaje)
    private const int ANIM_IDLE   = 0;
    private const int ANIM_RUN    = 1;
    private const int ANIM_JUMP   = 2;
    private const int ANIM_ATTACK = 3;
    private const int ANIM_CROUCH = 4;
    private const int ANIM_DAMAGE = 5;

    private static readonly string[] ANIM_NOMBRES =
        { "idle", "run", "jump", "attack", "crouch", "damage" };

    // ── API publica ──────────────────────────────────────────────────────────

    /// <summary>
    /// Serializa todas las animaciones a JSON.
    /// Formato: { "idle": ["base64...", "base64..."], "run": [...], ... }
    /// </summary>
    public static string SerializarFrames(CharacterData data)
    {
        if (data?.todasLasAnimaciones == null)
            return "{}";

        StringBuilder sb = new StringBuilder();
        sb.Append("{");

        for (int i = 0; i < ANIM_NOMBRES.Length; i++)
        {
            if (i > 0) sb.Append(",");

            sb.Append($"\"{ANIM_NOMBRES[i]}\":[");

            List<Texture2D> frames = (i < data.todasLasAnimaciones.Count)
                ? data.todasLasAnimaciones[i]
                : null;

            if (frames != null)
            {
                for (int f = 0; f < frames.Count; f++)
                {
                    if (f > 0) sb.Append(",");

                    string b64 = frames[f] != null
                        ? Convert.ToBase64String(frames[f].EncodeToPNG())
                        : "";

                    sb.Append($"\"{b64}\"");
                }
            }

            sb.Append("]");
        }

        sb.Append("}");
        return sb.ToString();
    }

    /// <summary>
    /// Serializa las hitboxes a JSON.
    /// Formato: { "body": {...}, "attack_por_frame": [{...}, null, ...] }
    /// </summary>
    public static string SerializarHitboxes(CharacterData data)
    {
        if (data == null) return "{}";

        StringBuilder sb = new StringBuilder();
        sb.Append("{");

        // Body hitbox
        sb.Append("\"body\":");
        sb.Append(SerializarHitboxData(data.bodyHitbox));

        // Attack hitbox por frame
        sb.Append(",\"attack_por_frame\":[");

        if (data.attackHitboxPorFrame != null)
        {
            for (int i = 0; i < data.attackHitboxPorFrame.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append(SerializarHitboxData(data.attackHitboxPorFrame[i]));
            }
        }

        sb.Append("]}");
        return sb.ToString();
    }

    // ── Helpers privados ─────────────────────────────────────────────────────

    private static string SerializarHitboxData(HitboxData h)
    {
        if (h == null) return "null";

        return $"{{" +
               $"\"offsetX\":{h.offsetX.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)}," +
               $"\"offsetY\":{h.offsetY.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)}," +
               $"\"width\":{h.width.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)}," +
               $"\"height\":{h.height.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)}," +
               $"\"esCirculo\":{h.esCirculo.ToString().ToLower()}" +
               $"}}";
    }
}