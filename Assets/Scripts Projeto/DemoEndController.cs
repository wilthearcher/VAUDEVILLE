using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DemoEndController : MonoBehaviour
{
    public static DemoEndController Instance { get; private set; }

    public enum EndingType
    {
        HiddenAndEscaped,
        DoorEscape,
        Caught
    }

    [Header("Painel de encerramento")]
    [SerializeField] private GameObject demoEndPanel;
    [SerializeField] private CanvasGroup panelCanvasGroup;

    [Header("Textos")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Animação")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float typingSpeed = 30f;
    [SerializeField] private float displayDuration = 15f;

    [Header("Cena de creditos")]
    [SerializeField] private string creditsSceneName = "Creditos";

    private bool demoFinished;
    private Coroutine endingCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("[DEMO] Existe mais de um DemoEndController na cena.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (demoEndPanel != null)
            demoEndPanel.SetActive(false);

        if (panelCanvasGroup == null && demoEndPanel != null)
            panelCanvasGroup = demoEndPanel.GetComponent<CanvasGroup>();

        if (panelCanvasGroup == null)
            Debug.LogError("[DEMO] CanvasGroup não foi atribuído ou encontrado no painel.");
    }

    public void EndHiddenAndEscaped()
    {
        EndDemo(EndingType.HiddenAndEscaped);
    }

    public void EndDoorEscape()
    {
        EndDemo(EndingType.DoorEscape);
    }

    public void EndCaught()
    {
        EndDemo(EndingType.Caught);
    }

    public void EndDemo()
    {
        EndDemo(EndingType.DoorEscape);
    }

    public void EndDemo(EndingType ending)
    {
        if (demoFinished)
            return;

        demoFinished = true;

        endingCoroutine = StartCoroutine(ShowEnding(ending));
    }

    private IEnumerator ShowEnding(EndingType ending)
    {
        if (demoEndPanel == null ||
            panelCanvasGroup == null ||
            titleText == null ||
            descriptionText == null)
        {
            Debug.LogError("[DEMO] Verifique as referências do painel no Inspector.");
            yield break;
        }

        string endingTitle;
        string endingDescription;

        switch (ending)
        {
            case EndingType.HiddenAndEscaped:

                endingTitle = "Fuga silenciosa";

                endingDescription =
                    "Ícaro Delair Castro, após o funeral de seu avô, não retornou para sua faculdade em Londres." +
                    "Sem parentes restantes ou amigos próximos, o jovem desapareceu em Manaus... levando consigo o nome Delair.";

                break;

            case EndingType.DoorEscape:

                endingTitle = "Uma saída arriscada";

                endingDescription =
                    "O Barão, apesar das razões claras para desconfiar do jovem, viu sua persistência por respostas com potencial." +
                    "O resultado: a chance de Ícaro fazer a diferença, seja para o Éden, para Manaus ou para o mundo.";

                break;

            case EndingType.Caught:

                endingTitle = "Fim da linha";

                endingDescription =
                    "Ícaro Delair, mesmo com as desconfianças do Barão, foi convidado a ficar pelo restante da noite com um aviso claro: se quer descobrir algo, saiba como falar. " +
                    "Talvez, em outra tentativa, o jovem aprenda sua lição e seu poder.";

                break;

            default:

                endingTitle = "Fim da demonstração";
                endingDescription = "A demonstração foi encerrada.";

                break;
        }

        demoEndPanel.SetActive(true);

        // Começa totalmente transparente.
        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = true;

        titleText.text = endingTitle;
        descriptionText.text = endingDescription;
        descriptionText.maxVisibleCharacters = 0;

        Debug.Log("[DEMO] Iniciando fade-in do painel.");

        // FADE-IN.
        yield return FadeCanvasGroup(0f, 1f, fadeInDuration);

        // TEXTO DIGITADO.
        int totalCharacters = descriptionText.text.Length;

        float delay = typingSpeed > 0f
            ? 1f / typingSpeed
            : 0f;

        for (int i = 0; i <= totalCharacters; i++)
        {
            descriptionText.maxVisibleCharacters = i;

            if (delay > 0f)
                yield return new WaitForSecondsRealtime(delay);
            else
                yield return null;
        }

        descriptionText.maxVisibleCharacters = totalCharacters;

        Debug.Log("[DEMO] Texto completamente exibido.");

        // TEMPO DE LEITURA.
        yield return new WaitForSecondsRealtime(displayDuration);

        Debug.Log("[DEMO] Iniciando fade-out do painel.");

        // FADE-OUT.
        yield return FadeCanvasGroup(1f, 0f, fadeOutDuration);

        demoEndPanel.SetActive(false);

        Debug.Log("[DEMO] Carregando creditos");

        SceneManager.LoadScene(creditsSceneName);
    }

    private IEnumerator FadeCanvasGroup(
        float startAlpha,
        float targetAlpha,
        float duration)
    {
        if (duration <= 0f)
        {
            panelCanvasGroup.alpha = targetAlpha;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            panelCanvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                progress
            );

            yield return null;
        }

        panelCanvasGroup.alpha = targetAlpha;
    }

    private void OnDestroy()
    {
        if (endingCoroutine != null)
            StopCoroutine(endingCoroutine);

        if (Instance == this)
            Instance = null;
    }
}