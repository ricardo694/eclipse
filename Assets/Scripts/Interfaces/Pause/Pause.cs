using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class Pausa : MonoBehaviour
{

    public GameObject menuPausa;
    public GameObject menuConfig;
    public bool juegoPausado = false ;
    private PlayerInput playerInput;
    private InputAction pauseAction ;



    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            pauseAction = playerInput.actions["Pause"];
        }
        else
        {
            pauseAction = new InputAction("Pause", binding: "<Keyboard>/escape");
            pauseAction.Enable();
        }
    }

    void OnEnable()
    {
        pauseAction.performed += OnPausePerformed;
    }

    void OnDisable()
    {
        pauseAction.performed -= OnPausePerformed;

        
        if (playerInput == null)
        {
            pauseAction.Disable();
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (juegoPausado)
            Reanudar();
        else
            Pausar();
    
    }

    public void Reanudar()
    {
        menuPausa.SetActive(false);
        menuConfig.SetActive(false);
        Time.timeScale = 1;
        juegoPausado = false;
    }

    public void Pausar()
    {
        menuConfig.SetActive(false);
        menuPausa.SetActive(true);
        Time.timeScale = 0;
        juegoPausado = true;
    }

    public void IrAConfigDesdePausa()
    {
        menuPausa.SetActive(false);
        menuConfig.SetActive(true); 
    }
    public void RegresarAPausa()
    {
        menuConfig.SetActive(false);
        menuPausa.SetActive(true);
    }
    public void VolverAlMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("StoryMode");
    }
}