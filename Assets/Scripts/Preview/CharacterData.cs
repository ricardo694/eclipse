using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class HitboxData
{
    public float offsetX;
    public float offsetY;
    public float width;
    public float height;
    public bool  esCirculo;

    public string ToJson()
    {
        return $"{{\"offsetX\":{offsetX},\"offsetY\":{offsetY}," +
               $"\"width\":{width},\"height\":{height}," +
               $"\"esCirculo\":{esCirculo.ToString().ToLower()}}}";
    }
}

[System.Serializable]
public class CharacterData
{
    public string id;
    public string nombrePersonaje;
    public string fechaCreacion;
    public int    pixelesPintados;
    public bool   esPublico;
    public Texture2D spriteBase;

    // Animaciones: indice 0=idle,1=run,2=jump,3=attack,4=crouch,5=damage
    public List<List<Texture2D>> todasLasAnimaciones = new List<List<Texture2D>>();

    // Hitboxes
    public HitboxData bodyHitbox;
    public List<HitboxData> attackHitboxPorFrame = new List<HitboxData>();
}