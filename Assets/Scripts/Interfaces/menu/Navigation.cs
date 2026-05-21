using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Navigation : MonoBehaviour
{
    
    public static string escenaAnterior = "Menu";

    public void irAlStoryMode()
    {
        SceneManager.LoadScene("StoryMode");
    }

    public void irAlLogin()
    {
        SceneManager.LoadScene("Login");
    }

    public void irAMlultiplayer()
    {
        SceneManager.LoadScene("Multiplayer");
    }

    public void irAConfiguracion()
    {
        SceneManager.LoadScene("config");
    }

    public void irAMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("Menu");
    }

   
public void regresarDesdeConfig()
{

    SceneManager.LoadScene("Menu");
}
}