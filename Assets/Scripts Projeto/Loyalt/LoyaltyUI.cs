
using UnityEngine;
using UnityEngine.UI;

public class LoyaltyUI : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Image pointerImage;

    [Header("Sprites de lealdade")]
    [SerializeField] private Sprite negativeStrong;
    [SerializeField] private Sprite negativeWeak;
    [SerializeField] private Sprite neutral;
    [SerializeField] private Sprite positiveWeak;
    [SerializeField] private Sprite positiveStrong;

    private void Start()
    {
        if (pointerImage == null)
        {
            Debug.LogError("[LOYALTY UI] Image não atribuída.");
            enabled = false;
            return;
        }

        if (LoyaltyManager.Instance == null)
        {
            Debug.LogError("[LOYALTY UI] LoyaltyManager não encontrado.");
            enabled = false;
            return;
        }

        UpdatePointer(LoyaltyManager.Instance.CurrentLoyalty);

        LoyaltyManager.Instance.OnLoyaltyChanged += UpdatePointer;
    }

    private void UpdatePointer(int value)
    {
        if (value <= -6)
            pointerImage.sprite = negativeStrong;
        else if (value < 0)
            pointerImage.sprite = negativeWeak;
        else if (value == 0)
            pointerImage.sprite = neutral;
        else if (value <= 5)
            pointerImage.sprite = positiveWeak;
        else
            pointerImage.sprite = positiveStrong;
    }

    private void OnDestroy()
    {
        if (LoyaltyManager.Instance != null)
            LoyaltyManager.Instance.OnLoyaltyChanged -= UpdatePointer;
    }
}
