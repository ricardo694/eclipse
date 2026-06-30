using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class StoryModeSystem : MonoBehaviour
{
    public PanelSeleccionPersonaje panelSeleccion;
// en el onClick:

    public void Jugar()
    {
        panelSeleccion.Abrir();
    }


    public void Devolverse()
    {
        SceneManager.LoadScene("Menu");

    }

    public void irAlPerfil()
    {
        SceneManager.LoadScene("Perfil");
    }

    public void irAConfiguracion()
    {
        SceneManager.LoadScene("config");
    }
}
