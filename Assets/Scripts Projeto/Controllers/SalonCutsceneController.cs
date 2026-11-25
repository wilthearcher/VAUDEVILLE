
using UnityEngine;
using UnityEngine.Video;

public class SalonCutsceneController : MonoBehaviour
{
    [Header("Vídeo")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject cutsceneCanvas;

    [Header("Referências")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private NPCInteraction baronInteraction;

    [Header("Áudios da cena")]
    [SerializeField] private AudioSource[] sceneAudioSources;

    private bool[] audioWasPlaying;

    private bool cutsceneStarted;
    private bool cutsceneFinished;

    private void Awake()
    {
        if (cutsceneCanvas != null)
            cutsceneCanvas.SetActive(false);

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
            playerController == null ||
            baronInteraction == null)
        {
            Debug.LogError(
                "SalonCutsceneController: referências não configuradas."
            );

            return;
        }

        cutsceneStarted = true;

        playerController.SetCutsceneMovementLocked(true);

        cutsceneCanvas.SetActive(true);

        videoPlayer.Stop();
        videoPlayer.Prepare();

        playerController.SetCutsceneMovementLocked(true);

        // Pausa os áudios do salão.
        PauseSceneAudio();

        cutsceneCanvas.SetActive(true);

        Debug.Log("Cutscene do salão iniciada.");
    }

    private void HandleVideoPrepared(VideoPlayer source)
    {
        if (!cutsceneStarted || cutsceneFinished)
            return;

        source.Play();

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

    private void FinishCutscene()
    {
        if (cutsceneFinished)
            return;

        cutsceneFinished = true;

        videoPlayer.Stop();

        cutsceneCanvas.SetActive(false);

        playerController.SetCutsceneMovementLocked(false);

        baronInteraction.UnlockSecondDialogue();

        // Retoma os áudios do salão.
        ResumeSceneAudio();

        playerController.SetCutsceneMovementLocked(false);

        baronInteraction.UnlockSecondDialogue();

        Debug.Log(
            "Cutscene concluída. Segundo diálogo do Barão liberado."
        );
    }
}
