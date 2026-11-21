
using UnityEngine;

public class InspectableObject : MonoBehaviour
{
    [Header("Informações do objeto")]
    [SerializeField] private string objectName;

    [TextArea(3, 8)]
    [SerializeField] private string description;

    [Header("Tipo de interação")]
    [SerializeField] private bool opensDocument;

    [SerializeField] private Sprite[] documentPages;

    [Header("Contorno")]
    [SerializeField] private GameObject outline;

    private bool IsInspectionLocked =>
    OfficeEventController.Instance != null &&
    OfficeEventController.Instance.InspectionsLocked;

    private void Start()
    {
        if (outline != null)
            outline.SetActive(false);
    }

    private void OnMouseEnter()
{
    if (IsInspectionLocked)
        return;

    if (outline != null)
        outline.SetActive(true);
}

private void OnMouseExit()
{
    if (outline != null)
        outline.SetActive(false);
}

private void OnMouseDown()
{
    if (IsInspectionLocked)
        return;

    if (opensDocument)
    {
        if (InspectionUI.Instance != null)
            InspectionUI.Instance.ClosePanel();

        if (DocumentUI.Instance != null)
            DocumentUI.Instance.ShowDocument(documentPages);
    }
    else
    {
        if (InspectionUI.Instance != null)
        {
            InspectionUI.Instance.ShowDescription(
                objectName,
                description
            );
        }
    }
}

}
