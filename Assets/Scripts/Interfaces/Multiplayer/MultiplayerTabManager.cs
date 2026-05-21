using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MultiplayerTabManager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelJoinServer;      
    public GameObject panelCreateServer;    

    [Header("Botones tab")]
    public Button btnJoin;
    public Button btnCreate;

    [Header("Colores tab activo/inactivo")]
    public Color colorActivo   = new Color(0f, 0.8f, 0.8f);     
    public Color colorInactivo = new Color(0.3f, 0.3f, 0.3f);   

    [Header("Inputs del formulario")]
    public TMP_InputField inputNombre;
    public TMP_InputField inputMaxJugadores;
    public TMP_InputField inputPassword;

    void Start()
    {
        
        MostrarJoinServer();

       
        btnJoin.onClick.AddListener(MostrarJoinServer);
        btnCreate.onClick.AddListener(MostrarCreateServer);
    }

    public void MostrarJoinServer()
    {
        panelJoinServer.SetActive(true);
        panelCreateServer.SetActive(false);

        // Resalta el tab activo
        btnJoin.GetComponentInChildren<TMP_Text>().color   = colorActivo;
        btnCreate.GetComponentInChildren<TMP_Text>().color = colorInactivo;
    }

    public void MostrarCreateServer()
    {
        panelJoinServer.SetActive(false);
        panelCreateServer.SetActive(true);

        
        btnCreate.GetComponentInChildren<TMP_Text>().color = colorActivo;
        btnJoin.GetComponentInChildren<TMP_Text>().color   = colorInactivo;
    }

    
    public void CrearServidor()
    {
        string nombre      = inputNombre.text;
        string maxJugadores = inputMaxJugadores.text;
        string password    = inputPassword.text;

        
        if (string.IsNullOrEmpty(nombre))
        {
            Debug.Log("El nombre del servidor no puede estar vacío");
            return;
        }

        // Por ahora solo lo imprime — aquí irá la lógica de red después
        Debug.Log($"Crear servidor: {nombre} | Max: {maxJugadores} | Pass: {password}");
    }
}