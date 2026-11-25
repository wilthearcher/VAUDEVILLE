using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [SerializeField] private CanvasGroup fadeGroup;   // painel preto em tela cheia
    [SerializeField] private float fadeDuration = 1f;

    private bool isLoading;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false;
    }

    public void LoadScene(string sceneName)
    {
        if (isLoading) return;
        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        isLoading = true;
        fadeGroup.blocksRaycasts = true;

        // Fade out
        yield return Fade(0f, 1f);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
            yield return null;

        // Fade in
        yield return Fade(1f, 0f);

        fadeGroup.blocksRaycasts = false;
        isLoading = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        fadeGroup.alpha = to;
    }
}