
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortaTransicao : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private PlayerController playerController;

    [Tooltip("Ponto onde Ícaro deve parar antes da porta.")]
    [SerializeField] private Transform pontoChegada;

    [Header("Transição de cena")]
    [SerializeField] private string sceneToLoad = "Corredor 2";

    private bool isLoading = false;

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = FindAnyObjectByType<PlayerController>();
        }
    }

    private void Start()
    {
        if (playerController == null)
            Debug.LogError("[PORTA] PlayerController não encontrado!");

        if (pontoChegada == null)
            Debug.LogError("[PORTA] Ponto de chegada não atribuído!");

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
            Debug.LogError("[PORTA] Cena não encontrada nas configurações: " + sceneToLoad);
    }

    public void IrParaScene()
    {
        if (isLoading)
        {
            Debug.LogWarning("[PORTA] Transição já iniciada.");
            return;
        }

        if (playerController == null || pontoChegada == null)
        {
            Debug.LogError("[PORTA] Referências incompletas!");
            return;
        }

        if (string.IsNullOrWhiteSpace(sceneToLoad))
        {
            Debug.LogError("[PORTA] Nome da cena está vazio!");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            Debug.LogError("[PORTA] Não foi possível encontrar a cena: " + sceneToLoad);
            return;
        }

        isLoading = true;

        Debug.Log("[PORTA] Iniciando movimento de Ícaro até a porta.");

        playerController.MoveToSpecialPosition(
            pontoChegada.position,
            AoChegarNaPorta
        );
    }

    private void AoChegarNaPorta()
    {
        Debug.Log("[PORTA] CALLBACK EXECUTADO! Ícaro chegou à porta.");

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            Debug.LogError("[PORTA] A cena não pode ser carregada: " + sceneToLoad);
            isLoading = false;
            return;
        }

        Debug.Log("[PORTA] Carregando cena: " + sceneToLoad);

        SceneManager.LoadScene(sceneToLoad);
    }
}
