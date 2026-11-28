
using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CreditsCutsceneController : MonoBehaviour
{
    [Header("Vídeo")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Configurações")]
    [SerializeField] private string menuSceneName = "MenuInicial";

    private bool cutsceneFinished = false;
    private bool videoStarted = false;

    private void Awake()
    {
        if (videoPlayer == null || fadeCanvasGroup == null)
        {
            Debug.LogError("[CREDITOS] Referências não atribuídas!");
            enabled = false;
            return;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;

        // Começa com a tela completamente preta.
        fadeCanvasGroup.alpha = 1f;
        fadeCanvasGroup.interactable = false;
        fadeCanvasGroup.blocksRaycasts = false;
    }

    private void OnEnable()
    {
        if (videoPlayer == null)
            return;

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.errorReceived += OnVideoError;
    }

    private void Start()
    {
        Debug.Log("[CREDITOS] Preparando vídeo...");
        videoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer player)
    {
        if (videoStarted || cutsceneFinished)
            return;

        videoStarted = true;
        StartCoroutine(IniciarCutscene());
    }

    private IEnumerator IniciarCutscene()
    {
        // Inicia o vídeo por trás da tela preta.
        videoPlayer.Play();

        // Revela o vídeo gradualmente.
        yield return StartCoroutine(Fade(1f, 0f));
    }

    private void OnVideoFinished(VideoPlayer player)
    {
        if (cutsceneFinished)
            return;

        cutsceneFinished = true;
        StartCoroutine(EncerrarCutscene());
    }

    private IEnumerator EncerrarCutscene()
    {
        Debug.Log("[CREDITOS] Finalizando vídeo...");

        videoPlayer.Stop();

        // Escurece a tela antes de trocar de cena.
        yield return StartCoroutine(Fade(0f, 1f));

        Debug.Log("[CREDITOS] Retornando ao menu...");

        if (Application.CanStreamedLevelBeLoaded(menuSceneName))
        {
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            Debug.LogError("[CREDITOS] Cena não encontrada: " + menuSceneName);
        }
    }

    private IEnumerator Fade(float startAlpha, float targetAlpha)
    {
        float elapsed = 0f;

        fadeCanvasGroup.alpha = startAlpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / fadeDuration);

            fadeCanvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                progress
            );

            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }

    private void OnVideoError(VideoPlayer player, string message)
    {
        Debug.LogError("[CREDITOS] Erro no vídeo: " + message);

        if (cutsceneFinished)
            return;

        cutsceneFinished = true;
        StartCoroutine(EncerrarCutscene());
    }

    private void OnDisable()
    {
        if (videoPlayer == null)
            return;

        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.loopPointReached -= OnVideoFinished;
        videoPlayer.errorReceived -= OnVideoError;
    }
}
