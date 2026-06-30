using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Componente para cada tarjeta de personaje en el panel de seleccion.
/// Agrega este script al prefab de tarjeta.
/// </summary>
public class TarjetaPersonaje : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text txtNombre;
    public TMP_Text txtFecha;
    public RawImage imgThumbnail;
    public Button   btnSeleccionar;
    public string Id { get; private set; }

    public void Configurar(string id, string nombre, string fecha, Action onClic)
    {
        Id = id;

        if (txtNombre != null) txtNombre.text = nombre ?? "Sin nombre";
        if (txtFecha  != null) txtFecha.text  = FormatearFecha(fecha);

        btnSeleccionar?.onClick.RemoveAllListeners();
        btnSeleccionar?.onClick.AddListener(() => onClic?.Invoke());
    }

    public void SetThumbnail(Texture2D tex)
    {
        if (imgThumbnail == null || tex == null) return;
        imgThumbnail.texture = tex;
        imgThumbnail.uvRect  = new Rect(0, 1, 1, -1);
    }



    string FormatearFecha(string fecha)
    {
        if (string.IsNullOrEmpty(fecha)) return "";
        // Supabase devuelve ISO 8601: "2024-01-15T10:30:00"
        if (fecha.Length >= 10)
            return fecha.Substring(0, 10); // "2024-01-15"
        return fecha;
    }
}