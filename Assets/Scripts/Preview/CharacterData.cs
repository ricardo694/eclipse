using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CharacterData
{
    public string nombrePersonaje;
    public string fechaCreacion;
    public int pixelesPintados;
    public Texture2D spriteBase;

    // Multi-frame — una lista de frames por cada animación
    // índice 0=idle, 1=correr, 2=saltar, 3=atacar, 4=agacharse, 5=daño
    public List<List<Texture2D>> todasLasAnimaciones = new List<List<Texture2D>>();
}