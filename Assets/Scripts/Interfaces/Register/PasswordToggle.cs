using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PasswordToggle : MonoBehaviour
{
    [Header("Referencias")]
    public TMP_InputField passwordInput;
    public Button toggleButton;

    [Header("Iconos (opcional)")]
    public Sprite eyeOpenIcon;
    public Sprite eyeClosedIcon;
    public Image toggleIcon;

    private bool _isPasswordVisible = false;

    void Awake()
    {
        if (toggleButton != null)
            toggleButton.onClick.AddListener(Toggle);
    }

    private void Toggle()
    {
        _isPasswordVisible = !_isPasswordVisible;

        passwordInput.contentType = _isPasswordVisible
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;

        // Fuerza que el input se refresque visualmente
        passwordInput.ForceLabelUpdate();

        if (toggleIcon != null && eyeOpenIcon != null && eyeClosedIcon != null)
            toggleIcon.sprite = _isPasswordVisible ? eyeOpenIcon : eyeClosedIcon;
    }
}