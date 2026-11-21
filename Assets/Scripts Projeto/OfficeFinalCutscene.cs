
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OfficeFinalCutscene : MonoBehaviour
{
    [Header("Painel final")]
    [SerializeField] private GameObject finalPanel;
    [SerializeField] private Image finalImage;
    [SerializeField] private TMP_Text finalText;

    [Header("Conteúdo")]
    [SerializeField] private Sprite endingImage;

    [TextArea(3, 8)]
    [SerializeField] private string endingMessage;

    private void Start()
    {
        if (finalPanel != null)
            finalPanel.SetActive(false);
    }

    public void ShowFinal()
    {
        if (finalPanel == null)
        {
            Debug.LogError("Final Panel não foi atribuído.");
            return;
        }

        if (finalImage != null)
            finalImage.sprite = endingImage;

        if (finalText != null)
            finalText.text = endingMessage;

        finalPanel.SetActive(true);
    }
}
