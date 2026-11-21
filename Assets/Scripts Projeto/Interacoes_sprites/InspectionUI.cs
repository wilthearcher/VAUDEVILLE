
using UnityEngine;
using TMPro;

public class InspectionUI : MonoBehaviour
{
    public static InspectionUI Instance { get; private set; }

    [Header("Painel")]
    [SerializeField] private GameObject inspectionPanel;

    [Header("Textos")]
    [SerializeField] private TMP_Text objectNameText;
    [SerializeField] private TMP_Text descriptionText;

    private void Awake()
    {
        Instance = this;
        inspectionPanel.SetActive(false);
    }

    public void ShowDescription(string objectName, string description)
    {
        objectNameText.text = objectName;
        descriptionText.text = description;

        inspectionPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        inspectionPanel.SetActive(false);
    }
}
