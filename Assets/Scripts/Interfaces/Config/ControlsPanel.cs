using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ControlsPanel : MonoBehaviour
{
    public enum GameAction
    {
        MoveLeft,
        MoveRight,
        Jump,
        Dash,
        Attack,
        Crouch
    }

    [System.Serializable]
    public class KeyBindingRow
    {
        public GameAction action;
        public Button button;
        public TMP_Text buttonLabel;
    }

    [Header("Input Action Asset")]
    public InputActionAsset inputActions; // ← arrastra tu .inputactions aquí

    [Header("Filas del panel")]
    public List<KeyBindingRow> rows = new List<KeyBindingRow>();

    [Header("UI de feedback")]
    public GameObject listeningOverlay;
    public TMP_Text listeningText;

    [Header("Botones")]
    public Button saveButton;
    public Button defaultButton;
    public Button exitButton;

    // Mapeo acción → nombre en el Input Action Asset
    private static readonly Dictionary<GameAction, (string map, string action)> _actionPaths = new()
    {
        { GameAction.MoveLeft,  ("Player", "Move") },
        { GameAction.MoveRight, ("Player", "Move") },
        { GameAction.Jump,      ("Player", "Jump") },
        { GameAction.Dash,      ("Player", "Dash") },
        { GameAction.Attack,    ("Player", "Attack") },
        { GameAction.Crouch,    ("Player", "Crouch") },
    };

    private GameAction? _listeningAction = null;
    private KeyBindingRow _listeningRow = null;
    private InputActionRebindingExtensions.RebindingOperation _rebindOp;

    private void Awake()
    {
        RefreshAllLabels();
        SetListeningUI(false);

        foreach (var row in rows)
        {
            var captured = row;
            row.button.onClick.AddListener(() => StartRebind(captured));
        }

        if (saveButton != null)    saveButton.onClick.AddListener(SaveBindings);
        if (defaultButton != null) defaultButton.onClick.AddListener(ResetToDefaults);
        if (exitButton != null)    exitButton.onClick.AddListener(OnExit);
    }

    private void OnDestroy()
    {
        _rebindOp?.Dispose();
    }

    // ── Rebind ────────────────────────────────────────────────
    private void StartRebind(KeyBindingRow row)
    {
        if (!_actionPaths.TryGetValue(row.action, out var path)) return;

        var action = inputActions.FindActionMap(path.map)?.FindAction(path.action);
        if (action == null) return;

        // Para Move necesitamos el binding correcto (izq o der)
        int bindingIndex = GetBindingIndex(row.action, action);
        if (bindingIndex < 0) return;

        _listeningAction = row.action;
        _listeningRow = row;
        row.buttonLabel.text = "...";
        SetListeningUI(true, row.action);

        action.Disable();

        _rebindOp = action.PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(op =>
            {
                op.Dispose();
                action.Enable();
                RefreshLabel(row.action);
                SetListeningUI(false);
                _listeningAction = null;
                _listeningRow = null;
            })
            .OnCancel(op =>
            {
                op.Dispose();
                action.Enable();
                RefreshLabel(row.action);
                SetListeningUI(false);
                _listeningAction = null;
                _listeningRow = null;
            })
            .Start();
    }

    private int GetBindingIndex(GameAction gameAction, InputAction action)
    {
        if (gameAction == GameAction.MoveLeft)
        {
            for (int i = 0; i < action.bindings.Count; i++)
                if (action.bindings[i].name.ToLower() == "left" && 
                    action.bindings[i].isPartOfComposite) return i;
        }
        else if (gameAction == GameAction.MoveRight)
        {
            for (int i = 0; i < action.bindings.Count; i++)
                if (action.bindings[i].name.ToLower() == "right" && 
                    action.bindings[i].isPartOfComposite) return i;
        }
        else
        {
            for (int i = 0; i < action.bindings.Count; i++)
                if (!action.bindings[i].isComposite && 
                    !action.bindings[i].isPartOfComposite) return i;
        }
        return -1;
    }

    // ── Labels ────────────────────────────────────────────────
    private void RefreshLabel(GameAction gameAction)
    {
        var row = rows.Find(r => r.action == gameAction);
        if (row == null) return;

        if (!_actionPaths.TryGetValue(gameAction, out var path)) return;
        
        // ← verifica que inputActions no sea null
        if (inputActions == null) return;
        
        var action = inputActions.FindActionMap(path.map)?.FindAction(path.action);
        if (action == null) return;

        int bindingIndex = GetBindingIndex(gameAction, action);
        if (bindingIndex < 0) return;

        string display = InputControlPath.ToHumanReadableString(
            action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice
        );

        row.buttonLabel.text = display.ToUpper();
    }
    private void RefreshAllLabels()
    {
        foreach (var row in rows)
            RefreshLabel(row.action);
    }

    // ── Guardar / Cargar / Resetear ───────────────────────────
    public void SaveBindings()
    {
        string json = inputActions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("inputBindings", json);
        PlayerPrefs.Save();
        Debug.Log("[ControlsPanel] Bindings guardados.");
    }

    public void ResetToDefaults()
    {
        inputActions.RemoveAllBindingOverrides();
        RefreshAllLabels();
        Debug.Log("[ControlsPanel] Bindings reseteados.");
    }

    private void OnExit()
    {
        SaveBindings();
        if (SceneManager.GetActiveScene().name == "config")
            SceneManager.LoadScene("Menu");
        else
            FindAnyObjectByType<Pausa>()?.RegresarAPausa();
    }

    // ── UI feedback ───────────────────────────────────────────
    private void SetListeningUI(bool active, GameAction? action = null)
    {
        if (listeningOverlay != null)
            listeningOverlay.SetActive(active);

        if (active && action != null && listeningText != null)
            listeningText.text = $"Press a key for:\n<b>{ActionName(action.Value)}</b>\n\n<size=70%>ESC to cancel</size>";
    }

    private static string ActionName(GameAction action) => action switch
    {
        GameAction.MoveLeft  => "Move left",
        GameAction.MoveRight => "Move right",
        GameAction.Jump      => "Jump",
        GameAction.Dash      => "Dash",
        GameAction.Attack    => "Attack",
        GameAction.Crouch    => "Crouch",
        _ => action.ToString()
    };

    public static ControlsPanel Instance { get; private set; }
    private void OnEnable()  { if (Instance == null) Instance = this; }
    private void OnDisable() { if (Instance == this) Instance = null; }
}