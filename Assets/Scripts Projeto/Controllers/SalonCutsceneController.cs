
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;

public class SalonCutsceneController : MonoBehaviour
{
    [Header("Vídeo")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject cutsceneCanvas;

    [Header("Fade")]
    [SerializeField] private CanvasGroup cutsceneCanvasGroup;
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 1.5f;

    [Header("Botão para pular")]
    [SerializeField] private Button skipButton;

    [Header("Referências")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private NPCInteraction baronInteraction;

    [Header("Áudios da cena")]
    [SerializeField] private AudioSource[] sceneAudioSources;

    private bool[] audioWasPlaying;

    private bool cutsceneStarted;
    private bool cutsceneFinished;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (cutsceneCanvas != null)
            cutsceneCanvas.SetActive(false);

        if (cutsceneCanvasGroup != null)
            cutsceneCanvasGroup.alpha = 0f;

        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
        }
    }

    private void OnEnable()
    {
        if (dialogueManager != null)
            dialogueManager.OnDialogueFinished += HandleDialogueFinished;

        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted += HandleVideoPrepared;
            videoPlayer.loopPointReached += HandleVideoFinished;
            videoPlayer.errorReceived += HandleVideoError;
        }

        if (skipButton != null)
            skipButton.onClick.AddListener(SkipCutscene);
    }

    private void OnDisable()
    {
        if (dialogueManager != null)
            dialogueManager.OnDialogueFinished -= HandleDialogueFinished;

        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= HandleVideoPrepared;
            videoPlayer.loopPointReached -= HandleVideoFinished;
            videoPlayer.errorReceived -= HandleVideoError;
        }

        if (skipButton != null)
            skipButton.onClick.RemoveListener(SkipCutscene);

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
    }

    private void HandleDialogueFinished(string finishedNPCID)
    {
        if (cutsceneStarted)
            return;

        if (baronInteraction == null)
            return;

        if (finishedNPCID != baronInteraction.NPCID)
            return;

        StartCutscene();
    }

    private void PauseSceneAudio()
    {
        if (sceneAudioSources == null)
            return;

        audioWasPlaying = new bool[sceneAudioSources.Length];

        for (int i = 0; i < sceneAudioSources.Length; i++)
        {
            AudioSource source = sceneAudioSources[i];

            if (source == null)
                continue;

            audioWasPlaying[i] = source.isPlaying;

            if (audioWasPlaying[i])
                source.Pause();
        }

        Debug.Log("Áudios do salão pausados.");
    }

    private void ResumeSceneAudio()
    {
        if (sceneAudioSources == null || audioWasPlaying == null)
            return;

        for (int i = 0; i < sceneAudioSources.Length; i++)
        {
            AudioSource source = sceneAudioSources[i];

            if (source == null)
                continue;

            if (audioWasPlaying[i])
                source.UnPause();
        }

        Debug.Log("Áudios do salão retomados.");
    }

    private void StartCutscene()
    {
        if (videoPlayer == null ||
            cutsceneCanvas == null ||
            cutsceneCanvasGroup == null ||
            playerController == null ||
            baronInteraction == null)
        {
            Debug.LogError(
                "SalonCutsceneController: referências não configuradas."
            );

            return;
        }

        cutsceneStarted = true;
        cutsceneFinished = false;

        // Bloqueia a movimentação de Ícaro.
        playerController.SetCutsceneMovementLocked(true);

        // Pausa os áudios do salão.
        PauseSceneAudio();

        // Ativa a interface da cutscene.
        cutsceneCanvas.SetActive(true);

        cutsceneCanvasGroup.alpha = 0f;
        cutsceneCanvasGroup.interactable = true;
        cutsceneCanvasGroup.blocksRaycasts = true;

        // Exibe o botão de pular.
        if (skipButton != null)
            skipButton.gameObject.SetActive(true);

        // Prepara o vídeo.
        videoPlayer.Stop();
        videoPlayer.Prepare();

        Debug.Log("Cutscene do salão iniciada.");
    }

    private void HandleVideoPrepared(VideoPlayer source)
    {
        if (!cutsceneStarted || cutsceneFinished)
            return;

        source.Play();

        // Revela o vídeo gradualmente.
        fadeCoroutine = StartCoroutine(
            FadeCanvas(1f, fadeInDuration)
        );

        Debug.Log("Reproduzindo cutscene.");
    }

    private void HandleVideoFinished(VideoPlayer source)
    {
        FinishCutscene();
    }

    private void HandleVideoError(
        VideoPlayer source,
        string message)
    {
        Debug.LogError("Erro na cutscene: " + message);

        FinishCutscene();
    }

    private void SkipCutscene()
    {
        if (!cutsceneStarted || cutsceneFinished)
            return;

        Debug.Log("Jogador pulou a cutscene.");

        FinishCutscene();
    }

    private void FinishCutscene()
    {
        if (!cutsceneStarted || cutsceneFinished)
            return;

        // Impede que o encerramento seja executado novamente.
        cutsceneFinished = true;

        // Interrompe o fade anterior, caso ainda esteja acontecendo.
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        // Esconde o botão durante o encerramento.
        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        // Inicia o fade-out.
        if (cutsceneCanvasGroup != null)
        {
            fadeCoroutine = StartCoroutine(
                FinishCutsceneRoutine()
            );
        }
        else
        {
            CompleteCutscene();
        }
    }

    private IEnumerator FinishCutsceneRoutine()
    {
        yield return FadeCanvas(0f, fadeOutDuration);

        CompleteCutscene();
    }

    private IEnumerator FadeCanvas(float targetAlpha, float duration)
    {
        float startAlpha = cutsceneCanvasGroup.alpha;

        if (duration <= 0f)
        {
            cutsceneCanvasGroup.alpha = targetAlpha;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            cutsceneCanvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                Mathf.Clamp01(elapsed / duration)
            );

            yield return null;
        }

        cutsceneCanvasGroup.alpha = targetAlpha;
    }

    private void CompleteCutscene()
    {
        if (videoPlayer != null)
            videoPlayer.Stop();

        if (cutsceneCanvas != null)
            cutsceneCanvas.SetActive(false);

        // Libera a movimentação de Ícaro.
        playerController.SetCutsceneMovementLocked(false);

        // Libera o segundo diálogo do Barão.
        baronInteraction.UnlockSecondDialogue();

        // Retoma os áudios que estavam tocando antes.
        ResumeSceneAudio();

        fadeCoroutine = null;

        Debug.Log(
            "Cutscene concluída. Segundo diálogo do Barão liberado."
        );
    }
}
