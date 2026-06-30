using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class UIRegister : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField usernameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;

    [Header("Buttons")]
    public Button createAccountButton;

    [Header("Systems")]
    public RegisterSystem registerSystem;
    public LoginSystem loginSystem;
    
    [Header("Feedback")]
    public AlertPanel alertPanel;
    public GameObject loadingIndicator;

    // -------------------------------------------------------
    void Start()
    {
        createAccountButton.onClick.AddListener(OnClickCreateAccount);

        if (alertPanel != null) alertPanel.Hide();
        if (loadingIndicator != null) loadingIndicator.SetActive(false);
    }

    // -------------------------------------------------------
    private void OnClickCreateAccount()
    {
        string username        = usernameInput.text.Trim();
        string email           = emailInput.text.Trim();
        string password        = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        HideError();
        SetLoading(true);
        registerSystem.Register(username, email, password, confirmPassword);
    }
    public void OnRegisterSuccess()
    {
        SetLoading(false);
        ClearFields();
        Debug.Log($" Registro exitoso, bienvenido {LoginSystem.Username ?? "usuario"}!");
        SceneManager.LoadScene("Menu");
    }

    public void OnRegisterFailed(string message)
    {
        SetLoading(false);
        ShowError(message);
    }
    private void ShowError(string message)
    {
        if (alertPanel != null) alertPanel.Show(message);
    }

    private void HideError()
    {
        if (alertPanel != null) alertPanel.Hide();
    }

    private void SetLoading(bool isLoading)
    {
        if (loadingIndicator != null) loadingIndicator.SetActive(isLoading);
        if (createAccountButton != null) createAccountButton.interactable = !isLoading;

    }
    public void ClearFields()
    {
        usernameInput.text        = "";
        emailInput.text           = "";
        passwordInput.text        = "";
        confirmPasswordInput.text = "";
    }
}