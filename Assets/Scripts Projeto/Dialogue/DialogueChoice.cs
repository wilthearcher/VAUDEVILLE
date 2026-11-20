using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueChoice : MonoBehaviour
{
    [Header("Interface")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text choiceText;
    [SerializeField] private LayoutElement layoutElement;

    [Header("Dimensões do botão")]
    [SerializeField] private float minimumButtonHeight = 85f;
    [SerializeField] private float verticalPadding = 24f;
    [SerializeField] private float horizontalPadding = 30f;

    public int RequiredLoyalty { get; private set; }
    public int MaximumLoyalty { get; private set; }
    public int LoyaltyDelta { get; private set; }

    public void Setup(
        string text,
        int minimum,
        int maximum,
        int delta,
        bool unlocked,
        UnityEngine.Events.UnityAction onClick
    )
    {
        choiceText.text = text;

        // Impede que a fonte diminua automaticamente.
        choiceText.fontSize = 36f;
        choiceText.enableAutoSizing = false;

        // Permite que textos longos ocupem várias linhas.
        choiceText.textWrappingMode = TextWrappingModes.Normal;

        choiceText.overflowMode = TextOverflowModes.Overflow;

        RequiredLoyalty = minimum;
        MaximumLoyalty = maximum;
        LoyaltyDelta = delta;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);

        button.interactable = unlocked;

        AjustarAltura();
    }

    private void AjustarAltura()
    {
        if (layoutElement == null)
            layoutElement = GetComponent<LayoutElement>();

        if (layoutElement == null)
            layoutElement = gameObject.AddComponent<LayoutElement>();

        choiceText.ForceMeshUpdate();

        float availableWidth =
            choiceText.rectTransform.rect.width;

        if (availableWidth <= 0)
        {
            availableWidth =
                GetComponent<RectTransform>().rect.width;
        }

        if (availableWidth <= 0 &&
            transform.parent is RectTransform parent)
        {
            availableWidth = parent.rect.width;
        }

        availableWidth = Mathf.Max(
            1f,
            availableWidth - horizontalPadding
        );

        Vector2 preferredSize =
            choiceText.GetPreferredValues(
                choiceText.text,
                availableWidth,
                0f
            );

        float calculatedHeight =
            preferredSize.y + verticalPadding;

        layoutElement.minHeight = minimumButtonHeight;

        layoutElement.preferredHeight = Mathf.Max(
            minimumButtonHeight,
            calculatedHeight
        );
    }
}
