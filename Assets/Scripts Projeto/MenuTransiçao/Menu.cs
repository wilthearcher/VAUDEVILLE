
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    private static Menu instance;
    private bool isTransitioning = false;

    private void Awake()
    {
        // Evita gerenciadores duplicados.
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // Mantém o gerenciador ativo durante as trocas de cena.
        DontDestroyOnLoad(gameObject);
    }

    public void Jogar()
    {
        if (isTransitioning)
            return;

        StartCoroutine(IniciarJogo());
    }

    private IEnumerator IniciarJogo()
    {
        isTransitioning = true;

        // Verifica se a cena de aviso existe.
        if (!Application.CanStreamedLevelBeLoaded("TelaAviso"))
        {
            Debug.LogError("[MENU] Cena TelaAviso não encontrada!");
            isTransitioning = false;
            yield break;
        }

        // Verifica se a cena cinematográfica existe.
        if (!Application.CanStreamedLevelBeLoaded("CineEden"))
        {
            Debug.LogError("[MENU] Cena CineEden não encontrada!");
            isTransitioning = false;
            yield break;
        }

        Debug.Log("[MENU] Carregando TelaAviso...");

        SceneManager.LoadScene("TelaAviso");

        // Aguarda 15 segundos, mesmo se o jogo estiver pausado.
        yield return new WaitForSecondsRealtime(15f);

        Debug.Log("[MENU] Carregando CineEden...");

        SceneManager.LoadScene("CineEden");
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
