using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panel que aparece despues de confirmar el hitbox.
/// Permite al usuario darle nombre final al personaje y elegir si lo publica.
/// </summary>
public class PanelGuardarPersonaje : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panelGuardar;

    [Header("UI")]
    public TMP_InputField inputNombre;
    public Toggle togglePublico;
    public TMP_Text txtEstadoPublico;    // "Publico" / "Privado"
    public Button btnConfirmar;
    public Button btnCancelar;

    [Header("Feedback")]
    public GameObject panelCargando;    // spinner o texto "Guardando..."
    public TMP_Text txtError;

    [Header("Servicio")]
    public SupabaseCharacterService characterService;

    // ── API publica ──────────────────────────────────────────────────────────

    public void Abrir()
    {
        panelGuardar.SetActive(true);

        // Prellenar nombre si ya tiene uno
        CharacterData data = CharacterDataHolder.Instance?.DatosActuales;
        if (inputNombre != null && data != null)
            inputNombre.text = data.nombrePersonaje ?? "";

        // Estado inicial
        if (togglePublico != null)   togglePublico.isOn = false;
        if (panelCargando != null)   panelCargando.SetActive(false);
        if (txtError != null)        txtError.gameObject.SetActive(false);

        ActualizarLabelPublico();
        ConectarBotones();
    }

    void ConectarBotones()
    {
        btnConfirmar?.onClick.RemoveAllListeners();
        btnCancelar?.onClick.RemoveAllListeners();
        togglePublico?.onValueChanged.RemoveAllListeners();

        btnConfirmar?.onClick.AddListener(Confirmar);
        btnCancelar?.onClick.AddListener(Cerrar);
        togglePublico?.onValueChanged.AddListener(_ => ActualizarLabelPublico());
    }

    void ActualizarLabelPublico()
    {
        if (txtEstadoPublico == null || togglePublico == null) return;
        txtEstadoPublico.text = togglePublico.isOn
            ? "Publico — visible en la comunidad"
            : "Privado — solo para ti";
    }

    // ── Guardar ──────────────────────────────────────────────────────────────

    void Confirmar()
    {
        CharacterData data = CharacterDataHolder.Instance?.DatosActuales;
        if (data == null)
        {
            MostrarError("No hay datos de personaje.");
            return;
        }

        string nombre = inputNombre != null ? inputNombre.text.Trim() : "";
        if (string.IsNullOrEmpty(nombre))
        {
            MostrarError("Ponle un nombre al personaje.");
            return;
        }

        data.nombrePersonaje = nombre;
        bool esPublico = togglePublico != null && togglePublico.isOn;

        // Mostrar spinner
        if (panelCargando != null) panelCargando.SetActive(true);
        if (txtError != null)      txtError.gameObject.SetActive(false);
        btnConfirmar.interactable = false;

        characterService.SubirPersonaje(
            data,
            esPublico,
            onExito: (id) =>
            {
                Debug.Log($"[Guardar] Personaje guardado con ID: {id}");
                if (panelCargando != null) panelCargando.SetActive(false);
                panelGuardar.SetActive(false);

                // Limpiar datos
                CharacterDataHolder.Instance?.SetData(null);

                // TODO: mostrar panel de exito o volver al menu principal
                Debug.Log("[Guardar] Listo. Ir al menu principal o multijugador.");
            },
            onError: (error) =>
            {
                Debug.LogError($"[Guardar] Error: {error}");
                if (panelCargando != null) panelCargando.SetActive(false);
                btnConfirmar.interactable = true;
                MostrarError("Error al guardar. Intenta de nuevo.");
            }
        );
    }

    void Cerrar()
    {
        panelGuardar.SetActive(false);
    }

    void MostrarError(string msg)
    {
        if (txtError == null) return;
        txtError.text = msg;
        txtError.gameObject.SetActive(true);
    }
}