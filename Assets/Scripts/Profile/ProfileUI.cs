using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ProfileUI : MonoBehaviour
{
    [Header("Datos del jugador")]
    public TMP_Text usernameText;
    public TMP_Text idText;

    [Header("Edición de username")]
    public Button editButton;
    public GameObject editPanel;          // panel/popup simple con un InputField + Confirmar
    public TMP_InputField editUsernameInput;
    public Button confirmEditButton;

    [Header("Navegación")]
    public Button closeSessionButton;
    public Button backButton;

    [Header("Sistemas")]
    public AlertPanel alertPanel;

    [Header("Feedback")]
    public TMP_Text errorText; // opcional si no usas AlertPanel aquí

    void Start()
    {
        RefreshProfileData();

        if (editButton != null) editButton.onClick.AddListener(OpenEditPanel);
        if (confirmEditButton != null) confirmEditButton.onClick.AddListener(ConfirmEdit);
        if (closeSessionButton != null) closeSessionButton.onClick.AddListener(CloseSession);
        if (backButton != null) backButton.onClick.AddListener(GoBack);

        if (editPanel != null) editPanel.SetActive(false);
    }

    private void RefreshProfileData()
    {
        if (usernameText != null) usernameText.text = LoginSystem.Username;
        if (idText != null) idText.text = $"ID: {LoginSystem.UserId.Substring(0, 8).ToUpper()}";
    }

    private void OpenEditPanel()
    {
        if (editPanel != null) editPanel.SetActive(true);
        if (editUsernameInput != null) editUsernameInput.text = LoginSystem.Username;
    }

    private void ConfirmEdit()
    {
        string newUsername = editUsernameInput.text.Trim();

        LoginSystem.Instance.UpdateUsername(newUsername, (success, errorMessage) =>
        {
            if (success)
            {
                RefreshProfileData();
                if (editPanel != null) editPanel.SetActive(false);
            }
            else
            {
                if (alertPanel != null) alertPanel.Show(errorMessage);
            }
        });
    }
    private void CloseSession()
    {
        LoginSystem.Instance.Logout();
        SceneManager.LoadScene("Menu");
    }

    private void GoBack()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
}