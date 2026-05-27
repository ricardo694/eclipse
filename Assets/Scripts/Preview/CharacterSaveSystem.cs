using UnityEngine;
using System.Collections.Generic;

public class CharacterSaveSystem : MonoBehaviour
{
    public static CharacterSaveSystem Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void GuardarPersonaje(CharacterData data)
    {
        // Convertir textura a bytes
        byte[] bytes = data.spriteBase.EncodeToPNG();
        string base64 = System.Convert.ToBase64String(bytes);

        // Obtener lista actual
        int cantidad = PlayerPrefs.GetInt("TotalPersonajes", 0);

        // Guardar datos
        PlayerPrefs.SetString($"Personaje_{cantidad}_Nombre", data.nombrePersonaje);
        PlayerPrefs.SetString($"Personaje_{cantidad}_Fecha", data.fechaCreacion);
        PlayerPrefs.SetInt($"Personaje_{cantidad}_Pixeles", data.pixelesPintados);
        PlayerPrefs.SetString($"Personaje_{cantidad}_Sprite", base64);

        // Actualizar contador
        PlayerPrefs.SetInt("TotalPersonajes", cantidad + 1);
        PlayerPrefs.Save();

        Debug.Log($"Personaje guardado: {data.nombrePersonaje} (total: {cantidad + 1})");
    }

    public List<CharacterData> CargarTodosLosPersonajes()
    {
        List<CharacterData> lista = new List<CharacterData>();
        int cantidad = PlayerPrefs.GetInt("TotalPersonajes", 0);

        for (int i = 0; i < cantidad; i++)
        {
            CharacterData data = new CharacterData();
            data.nombrePersonaje = PlayerPrefs.GetString($"Personaje_{i}_Nombre", "Sin nombre");
            data.fechaCreacion   = PlayerPrefs.GetString($"Personaje_{i}_Fecha", "");
            data.pixelesPintados = PlayerPrefs.GetInt($"Personaje_{i}_Pixeles", 0);

            // Reconstruir textura desde base64
            string base64 = PlayerPrefs.GetString($"Personaje_{i}_Sprite", "");
            if (!string.IsNullOrEmpty(base64))
            {
                byte[] bytes = System.Convert.FromBase64String(base64);
                Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Point;
                tex.LoadImage(bytes);
                data.spriteBase = tex;
            }

            lista.Add(data);
        }

        return lista;
    }
}