using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Localization.Settings;
//holaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
public class GuideNPC : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI npcNameText;
    public GameObject interactIcon;
    public Transform dialoguePivot;
    public GameObject continueIcon;

    [Header("Configuración NPC")]
    public string npcName = "Amigo";

    [Header("Diálogo")]
    public string[] dialogueKeys;
    public string tableName = "UIText";

    [Header("Typewriter")]
    public float velocidadTexto = 0.03f;
    private bool escribiendo = false;
    private string textoCompleto = "";

    [Header("Sonido")]
    public AudioClip sonidoLetra;
    public AudioClip sonidoAbrir;
    private AudioSource audioSource;

    [Header("Parpadeo Ícono")]
    public float velocidadParpadeo = 0.5f;

    private Transform player;
    private bool playerInRange = false;
    private bool dialogueOpen = false;
    private int lineIndex = 0;
    private PlayerController playerController;
    private string[] dialogueLines;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        dialoguePanel.SetActive(false);
        if (interactIcon) interactIcon.SetActive(false);
        if (continueIcon) continueIcon.SetActive(false);
        if (npcNameText) npcNameText.text = npcName;

        StartCoroutine(CargarTextos());
    }
    IEnumerator CargarTextos()
    {
        yield return LocalizationSettings.InitializationOperation;
        ResolverTextos();

        
        LocalizationSettings.SelectedLocaleChanged += _ => ResolverTextos();
    }

    void ResolverTextos()
    {
        dialogueLines = new string[dialogueKeys.Length];
        for (int i = 0; i < dialogueKeys.Length; i++)
        {
            dialogueLines[i] = LocalizationSettings.StringDatabase
                .GetLocalizedString(tableName, dialogueKeys[i]);
        }

        if (npcNameText)
            npcNameText.text = LocalizationSettings.StringDatabase
                .GetLocalizedString(tableName, "npc_amigo_nombre");
    }
    void OnDestroy()
    {
        LocalizationSettings.SelectedLocaleChanged -= _ => ResolverTextos();
    }

    void Update()
    {
        if (playerInRange && player != null && playerController != null)
        {
            FlipTowardsPlayer();

            if (playerController.InteractPulsado)
            {
                if (!dialogueOpen)
                    OpenDialogue();
                else if (!escribiendo)
                    NextLine();
                else
                    TerminarEscritura(); 
            }
        }
    }

    void FlipTowardsPlayer()
    {
        float dirX = player.position.x - transform.position.x;
        float scaleX = dirX > 0 ? 1f : -1f;

        transform.localScale = new Vector3(scaleX, 1f, 1f);
        dialogueText.transform.localScale = new Vector3(scaleX, 1f, 1f);
    }

    void OpenDialogue()
    {
        dialogueOpen = true;
        lineIndex = 0;
        if (interactIcon) interactIcon.SetActive(false);
        if (continueIcon) continueIcon.SetActive(false);

        if (sonidoAbrir && audioSource)
            audioSource.PlayOneShot(sonidoAbrir);

        StartCoroutine(AbrirConAnimacion());
    }

    IEnumerator AbrirConAnimacion()
    {
        dialoguePanel.SetActive(true);
        dialoguePanel.transform.localScale = Vector3.zero;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 8f;
            dialoguePanel.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            yield return null;
        }

        dialoguePanel.transform.localScale = Vector3.one;
        StartCoroutine(MostrarTexto(dialogueLines[lineIndex]));
    }

    IEnumerator MostrarTexto(string texto)
    {
        escribiendo = true;
        textoCompleto = texto;
        dialogueText.text = "";
        if (continueIcon) continueIcon.SetActive(false);

        foreach (char letra in texto)
        {
            dialogueText.text += letra;

            if (sonidoLetra && audioSource)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f); // variación de tono
                audioSource.PlayOneShot(sonidoLetra, 0.3f);
            }

            yield return new WaitForSeconds(velocidadTexto);
        }

        escribiendo = false;
        if (continueIcon) continueIcon.SetActive(true);
        StartCoroutine(ParpadeaContinue());
    }

    void TerminarEscritura()
    {
        StopAllCoroutines();
        escribiendo = false;
        dialogueText.text = textoCompleto;
        if (continueIcon) continueIcon.SetActive(true);
        StartCoroutine(ParpadeaContinue());
    }

    IEnumerator ParpadeaContinue()
    {
        while (dialogueOpen && !escribiendo)
        {
            continueIcon.SetActive(!continueIcon.activeSelf);
            yield return new WaitForSeconds(velocidadParpadeo);
        }
    }

    void NextLine()
    {
        lineIndex++;
        if (lineIndex < dialogueLines.Length)
        {
            if (continueIcon) continueIcon.SetActive(false);
            StartCoroutine(MostrarTexto(dialogueLines[lineIndex]));
        }
        else
        {
            CloseDialogue();
        }
    }

    void CloseDialogue()
    {
        StopAllCoroutines();
        dialogueOpen = false;
        escribiendo = false;
        if (continueIcon) continueIcon.SetActive(false);
        
        // Solo anima el cierre si el objeto está activo
        if (gameObject.activeInHierarchy)
            StartCoroutine(CerrarConAnimacion());
        else
            dialoguePanel.SetActive(false); 
    }

    IEnumerator CerrarConAnimacion()
    {
        float t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime * 8f;
            dialoguePanel.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            yield return null;
        }

        dialoguePanel.SetActive(false);
        dialoguePanel.transform.localScale = Vector3.one;
        if (interactIcon) interactIcon.SetActive(true);
        StartCoroutine(ParpadeaIcono());
    }

    // ── Parpadeo del ícono E ──────────────────────────
    IEnumerator ParpadeaIcono()
    {
        while (playerInRange && !dialogueOpen)
        {
            if (interactIcon) interactIcon.SetActive(!interactIcon.activeSelf);
            yield return new WaitForSeconds(velocidadParpadeo);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            playerController = other.GetComponentInParent<PlayerController>();
                
            playerInRange = true;
            if (interactIcon) interactIcon.SetActive(true);
            StartCoroutine(ParpadeaIcono());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
            playerController = null;
            StopAllCoroutines();
            CloseDialogue();
            if (interactIcon) interactIcon.SetActive(false);
        }
    }
}
