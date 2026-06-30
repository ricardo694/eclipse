using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuAuthUI : MonoBehaviour
{
    [Header("Botón Login/Perfil")]
    public Button authButton;
    public TMP_Text authButtonLabel;

    [Header("Mensaje de bienvenida")]
    public TMP_Text welcomeText;

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (LoginSystem.IsLoggedIn)
        {
            if (authButtonLabel != null) authButtonLabel.text = "PERFIL";
            if (welcomeText != null) welcomeText.text = $"Bienvenido, {LoginSystem.Username}!";

            authButton.onClick.RemoveAllListeners();
            authButton.onClick.AddListener(() => SceneManager.LoadScene("Profile"));
        }
        else
        {
            if (authButtonLabel != null) authButtonLabel.text = "LOGIN";
            if (welcomeText != null) welcomeText.text = "";

            authButton.onClick.RemoveAllListeners();
            authButton.onClick.AddListener(() => SceneManager.LoadScene("Login"));
        }
    }
}