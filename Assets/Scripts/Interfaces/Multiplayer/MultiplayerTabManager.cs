using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MultiplayerTabManager : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject panelJoinServer;      
    [SerializeField] private GameObject panelCreateServer;    

    [Header("Botones tab")]
    [SerializeField] private Button btnJoin;
    [SerializeField] private Button btnCreate;

    [Header("Botones de acción")]
    [SerializeField] private Button btnConnect;
    [SerializeField] private Button btnRefresh;

    [Header("Lista de Salas")]
    [SerializeField] private Transform contenedorSalas;
    [SerializeField] private GameObject prefabServerItem;

    [Header("Inputs del formulario")]
    [SerializeField] private TMP_InputField inputNombre;
    [SerializeField] private TMP_InputField inputMaxJugadores;
    [SerializeField] private TMP_InputField inputPassword;

    [Header("Feedback")]
    [SerializeField] private TMP_Text txtEstado;

    private string _salaSeleccionada;

    void Start()
    {
        
        MostrarJoinServer();

       
        btnJoin.onClick.AddListener(() => MostrarPanel(true));
        btnCreate.onClick.AddListener(() => MostrarPanel(false));
        btnConnect.onClick.AddListener(ConectarSala);
        btnRefresh.onClick.AddListener(RefrescarLista);

        MostrarPanel(true);
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnSesionesActualizadas += ActualizarLista;
            LobbyManager.Instance.IniciarLobby();
        }
        else
        {
            Debug.LogError("LobbyManager.Instance es NULL en Start — revisa el orden de ejecución");
        }

    }

    void OnEnable()
    {
        if( LobbyManager.Instance != null)
            LobbyManager.Instance.OnSesionesActualizadas += ActualizarLista;
    }

    void OnDisable()
    {
        if (LobbyManager.Instance != null)
            LobbyManager.Instance.OnSesionesActualizadas -= ActualizarLista;
    }
        void MostrarPanel(bool mostrarJoin)
    {
        panelJoinServer.SetActive(mostrarJoin);
        panelCreateServer.SetActive(!mostrarJoin);
    }


    void ActualizarLista()
    {
        foreach (Transform hijo in contenedorSalas)
            Destroy(hijo.gameObject);

        foreach (var sesion in LobbyManager.Instance.SesionesDisponibles)
        {
            var item = Instantiate(prefabServerItem, contenedorSalas);
            var ui   = item.GetComponent<ServerItemUI>();
            string nombre = sesion.Name;
            ui.Setup(nombre, sesion.PlayerCount, sesion.MaxPlayers, () =>
            {
                _salaSeleccionada = nombre;
            });
        }
    }

    void RefrescarLista()
    {
        SetEstado("Refrescando...");
        ActualizarLista();
    }

    void ConectarSala()
    {
        if (string.IsNullOrEmpty(_salaSeleccionada))
        {
            SetEstado("Selecciona una sala primero.");
            return;
        }
        SetEstado($"Conectando a {_salaSeleccionada}...");
        LobbyManager.Instance.UnirseASesion(_salaSeleccionada);
    }

    public void CrearServidor()
    {
        string nombre = inputNombre.text.Trim();
        string maxStr = inputMaxJugadores.text.Trim();

        if (string.IsNullOrEmpty(nombre))
        {
            SetEstado("Escribe un nombre para la sala.");
            return;
        }

        int max = 4;
        if (!string.IsNullOrEmpty(maxStr)) int.TryParse(maxStr, out max);

        SetEstado($"Creando sala '{nombre}'...");
        LobbyManager.Instance.CrearSesion(nombre, max);
    }

    void SetEstado(string msg)
    {
        if (txtEstado != null) txtEstado.text = msg;
        Debug.Log("[Lobby] " + msg);
    }

    public void MostrarJoinServer()
    {
        panelJoinServer.SetActive(true);
        panelCreateServer.SetActive(false);


    }

    public void MostrarCreateServer()
    {
        panelJoinServer.SetActive(false);
        panelCreateServer.SetActive(true);

  
    }
}