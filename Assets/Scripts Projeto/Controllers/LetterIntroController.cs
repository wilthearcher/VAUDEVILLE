
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LetterIntroController : MonoBehaviour
{
    [Header("Carta")]
    [SerializeField] private Image letterImage;
    [SerializeField] private Sprite[] animationFrames;
    [SerializeField] private float frameDuration = 0.15f;

    [Header("Interface")]
    [SerializeField] private GameObject letterPanel;
    [SerializeField] private Button closeButton;

    public static bool InteractionsUnlocked { get; private set; } = true;

    private bool animationFinished;
    private bool letterClosed;

    private void Awake()
    {
        InteractionsUnlocked = false;

        animationFinished = false;
        letterClosed = false;

        if (letterImage == null || letterPanel == null ||
            closeButton == null)
        {
            Debug.LogError("[CARTA] Verifique as referências no Inspector.");
            enabled = false;
            return;
        }

        if (animationFrames == null || animationFrames.Length == 0)
        {
            Debug.LogError("[CARTA] Nenhum sprite foi configurado.");
            enabled = false;
            return;
        }

        closeButton.gameObject.SetActive(false);

        letterImage.gameObject.SetActive(true);
        letterImage.enabled = true;

        letterImage.sprite = animationFrames[0];
        letterImage.color = Color.white;
        letterImage.preserveAspect = true;
    }

    private void Start()
    {
        StartCoroutine(PlayLetterAnimation());
    }

    private IEnumerator PlayLetterAnimation()
    {
        Debug.Log("[CARTA] Iniciando animação.");

        foreach (Sprite frame in animationFrames)
        {
            if (frame == null)
                continue;

            letterImage.sprite = frame;

            yield return new WaitForSeconds(frameDuration);
        }

        letterImage.sprite = animationFrames[animationFrames.Length - 1];

        animationFinished = true;

        closeButton.gameObject.SetActive(true);

        Debug.Log("[CARTA] Animação concluída. Botão liberado.");
    }

    public void CloseLetter()
    {
        if (!animationFinished || letterClosed)
            return;

        letterClosed = true;

        letterPanel.SetActive(false);

        InteractionsUnlocked = true;

        Debug.Log("[CARTA] Carta fechada. Interações liberadas.");
    }
}
