
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueChoiceTeste : MonoBehaviour
{
    [Header("Interface")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text choiceText;

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

        RequiredLoyalty = minimum;
        MaximumLoyalty = maximum;
        LoyaltyDelta = delta;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);

        button.interactable = unlocked;
    }
}
