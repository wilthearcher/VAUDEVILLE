
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MainHallEndController : MonoBehaviour
{
    public static MainHallEndController Instance { get; private set; }

    [Header("Painel de encerramento")]
    [SerializeField] private GameObject demoEndPanel;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Textos")]
    [SerializeField] private TMP_Text endingTitleText;
    [SerializeField] private TMP_Text endingDescriptionText;

    [Header("Sistemas da cena")]
    [SerializeField] private GameObject loyaltyUI;
    [SerializeField] private BaronExitSequence baronExitSequence;

    [Header("Animação")]
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 1.5f;
    [SerializeField] private float typingSpeed = 30f;
    [SerializeField] private float displayDuration = 5f;

    [Header("Cena de destino")]
    [SerializeField] private string creditsSceneName = "Creditos";

    private bool demoFinished;
    private Coroutine endingCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (demoEndPanel != null)
            demoEndPanel.SetActive(false);

        if (canvasGroup == null && demoEndPanel != null)
            canvasGroup = demoEndPanel.GetComponent<CanvasGroup>();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void EndDemo()
    {
        if (demoFinished)
            return;

        demoFinished = true;

        // Desativa a interface de lealdade.
        if (loyaltyUI != null)
            loyaltyUI.SetActive(false);

        // Cancela a sequência de saída do Barão.
        if (baronExitSequence != null)
            baronExitSequence.CancelExitSequence();

        // Interrompe os sons da cena.
        StopSceneAudio();

        if (endingTitleText != null)
            endingTitleText.text = "Cuidado com as palavras";

        if (endingDescriptionText != null)
        {
            endingDescriptionText.text =
                "Ícaro Delair Castro, aos 29 anos, voltou para Londres e terminou sua formação médica." +
                "Professores relatam que, após sua viagem a Manaus, o jovem havia mudado: pálido e silencioso." +
                "Os que perguntavam a razão recebiam a mesma resposta: o completo silêncio...";

            // Esconde todas as letras inicialmente.
            endingDescriptionText.maxVisibleCharacters = 0;
        }

        if (demoEndPanel != null)
        {
            demoEndPanel.SetActive(true);

            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }

        endingCoroutine = StartCoroutine(EndingSequence());

        Debug.Log("[MAIN HALL] DEMO DO SALÃO PRINCIPAL FINALIZADA!");
    }

    private void StopSceneAudio()
    {
        AudioSource[] audioSources =
            FindObjectsOfType<AudioSource>(true);

        foreach (AudioSource source in audioSources)
        {
            if (source != null && source.isPlaying)
                source.Stop();
        }
    }

    private IEnumerator EndingSequence()
    {
        if (canvasGroup == null ||
            endingTitleText == null ||
            endingDescriptionText == null)
        {
            Debug.LogError(
                "[MAIN HALL] Verifique as referências do painel e dos textos."
            );
            yield break;
        }

        // Garante que o texto comece oculto.
        endingDescriptionText.maxVisibleCharacters = 0;

        // FADE-IN.
        Debug.Log("[MAIN HALL] Iniciando fade-in.");
        yield return Fade(0f, 1f, fadeInDuration);

        // ANIMAÇÃO DE ESCRITA.
        Debug.Log("[MAIN HALL] Iniciando animação de texto.");

        int totalCharacters = endingDescriptionText.text.Length;

        float delay = typingSpeed > 0f
            ? 1f / typingSpeed
            : 0f;

        for (int i = 0; i <= totalCharacters; i++)
        {
            endingDescriptionText.maxVisibleCharacters = i;

            if (delay > 0f)
                yield return new WaitForSecondsRealtime(delay);
            else
                yield return null;
        }

        endingDescriptionText.maxVisibleCharacters = totalCharacters;

        Debug.Log("[MAIN HALL] Texto completamente exibido.");

        // TEMPO PARA LEITURA.
        yield return new WaitForSecondsRealtime(displayDuration);

        // FADE-OUT.
        Debug.Log("[MAIN HALL] Iniciando fade-out.");
        yield return Fade(1f, 0f, fadeOutDuration);

        // Carrega a cena de créditos.
        LoadCreditsScene();
    }

    private IEnumerator Fade(float start, float end, float duration)
    {
        if (duration <= 0f)
        {
            canvasGroup.alpha = end;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            canvasGroup.alpha = Mathf.Lerp(
                start,
                end,
                Mathf.Clamp01(elapsed / duration)
            );

            yield return null;
        }

        canvasGroup.alpha = end;
    }

    private void LoadCreditsScene()
    {
        if (string.IsNullOrWhiteSpace(creditsSceneName))
        {
            Debug.LogError(
                "[MAIN HALL] O nome da cena de créditos não foi configurado."
            );
            return;
        }

        Debug.Log("[MAIN HALL] Carregando cena de créditos: " + creditsSceneName);

        SceneManager.LoadScene(creditsSceneName);
    }

    private void OnDestroy()
    {
        if (endingCoroutine != null)
            StopCoroutine(endingCoroutine);

        if (Instance == this)
            Instance = null;
    }
}
