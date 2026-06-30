using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class AlertPanel : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject panel;
    public TMP_Text messageText;
    public Button closeButton;

    [Header("Config")]
    public float autoCloseSeconds = 3f;

    private Coroutine _autoCloseRoutine;

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(Hide);
    }

    public void Show(string message)
    {
        if (messageText != null) messageText.text = message;
        if (panel != null) panel.SetActive(true);

        if (_autoCloseRoutine != null) StopCoroutine(_autoCloseRoutine);
        if (autoCloseSeconds > 0)
            _autoCloseRoutine = StartCoroutine(AutoClose());
    }

    public void Hide()
    {
        if (_autoCloseRoutine != null) StopCoroutine(_autoCloseRoutine);
        if (panel != null) panel.SetActive(false);
    }

    private IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(autoCloseSeconds);
        Hide();
    }
}