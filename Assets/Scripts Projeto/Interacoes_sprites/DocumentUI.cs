using UnityEngine;
using UnityEngine.UI;

public class DocumentUI : MonoBehaviour
{
    public static DocumentUI Instance { get; private set; }

    [Header("Painel do documento")]
    [SerializeField] private GameObject documentPanel;

    [Header("Imagem")]
    [SerializeField] private Image documentImage;

    [Header("Botões")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button closeButton;

    private Sprite[] pages;
    private int currentPage;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        documentPanel.SetActive(false);

        continueButton.onClick.AddListener(NextPage);
        closeButton.onClick.AddListener(CloseDocument);
    }

    public void ShowDocument(Sprite[] documentPages)
    {
        if (documentPages == null || documentPages.Length == 0)
        {
            Debug.LogWarning("O documento não possui imagens.");
            return;
        }

        pages = documentPages;
        currentPage = 0;

        documentPanel.SetActive(true);

        UpdatePage();
    }

    private void NextPage()
    {
        if (pages == null || currentPage >= pages.Length - 1)
            return;

        currentPage++;

        UpdatePage();
    }

    
private void UpdatePage()
{
    documentImage.sprite = pages[currentPage];

    documentImage.preserveAspect = true;

    continueButton.gameObject.SetActive(
        currentPage < pages.Length - 1
    );

    // Avisa quando a segunda página é apresentada.
    if (currentPage == 1 &&
        OfficeEventController.Instance != null)
    {
        OfficeEventController.Instance.OnSecondPageOpened();
    }
}


    
public void CloseDocument()
{
    bool wasOpen = documentPanel.activeSelf;

    documentPanel.SetActive(false);

    pages = null;
    currentPage = 0;

    if (wasOpen &&
        OfficeEventController.Instance != null)
    {
        OfficeEventController.Instance.OnDocumentClosed();
    }
}

}
