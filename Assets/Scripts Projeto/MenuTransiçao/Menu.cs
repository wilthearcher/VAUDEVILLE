using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    //private static Menu instance;
    public static Menu Instance { get; private set; }
    private bool isTransitioning = false;

    private void Awake()
    {
        // Evita gerenciadores duplicados.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Mantém o gerenciador ativo durante as trocas de cena.
        DontDestroyOnLoad(gameObject);
    }

    // BOTÃO JOGAR
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

    // BOTÃO OPÇÕES
    public void AbrirOpcoes()
{
    if (isTransitioning)
        return;

    if (!Application.CanStreamedLevelBeLoaded("TelaOpcoes"))
    {
        Debug.LogError("[MENU] Cena TelaOpcoes não encontrada!");
        return;
    }

    isTransitioning = true;

    Debug.Log("[MENU] Abrindo tela de opções...");

    SceneManager.LoadScene("TelaOpcoes");

    isTransitioning = false;
}

    // BOTÃO SAIR
    public void Sair()
    {
        Debug.Log("[MENU] Encerrando jogo...");

        Application.Quit();

        // Permite testar o botão Sair dentro do Editor.
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

public void VoltarMenu()
{
    if (isTransitioning)
        return;

    if (!Application.CanStreamedLevelBeLoaded("MenuInicial"))
    {
        Debug.LogError("[MENU] Cena MenuInicial não encontrada!");
        return;
    }

    isTransitioning = true;

    Debug.Log("[MENU] Retornando ao menu principal...");

    SceneManager.LoadScene("MenuInicial");

    isTransitioning = false;
}

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
