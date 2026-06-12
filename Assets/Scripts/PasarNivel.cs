using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PasarNivel : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si el jugador entra en el trigger, carga el siguiente nivel
        if (collision.CompareTag("Player"))
        {
            Debug.Log("¡Pasando al siguiente nivel!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // Carga la siguiente escena en el orden de construcción
        }
    }
}