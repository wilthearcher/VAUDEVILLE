using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoyaltyUI : MonoBehaviour
{
    [Header("Elementos da Interface")]
    [SerializeField] private Slider loyaltySlider;
    [SerializeField] private TMP_Text loyaltyText;

    private void Start()
    {
        if (LoyaltyManager.Instance == null)
        {
            Debug.LogError(
                "LoyaltyManager não foi encontrado na cena."
            );

            return;
        }

        // Recebe um aviso sempre que a lealdade mudar.
        LoyaltyManager.Instance.OnLoyaltyChanged += UpdateUI;

        // Atualiza a interface com o valor que já está salvo.
        UpdateUI(LoyaltyManager.Instance.CurrentLoyalty);
    }

    private void OnDestroy()
    {
        // Deixa de receber avisos quando este objeto for destruído.
        if (LoyaltyManager.Instance != null)
        {
            LoyaltyManager.Instance.OnLoyaltyChanged -= UpdateUI;
        }
    }

    private void UpdateUI(float currentLoyalty)
    {
        if (loyaltySlider != null)
        {
            loyaltySlider.value = currentLoyalty;
        }

        if (loyaltyText != null)
        {
            loyaltyText.text =
                "Lealdade: " + currentLoyalty.ToString("0.##");
        }
    }
}