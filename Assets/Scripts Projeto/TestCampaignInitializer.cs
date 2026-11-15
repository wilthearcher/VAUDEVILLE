
using UnityEngine;

public class TestCampaignInitializer : MonoBehaviour
{
    [SerializeField] private string testCampaignID = "campanha_teste_001";

    private void Start()
    {
        if (InteractionTracker.Instance == null)
        {
            Debug.LogError("InteractionTracker não encontrado!");
            return;
        }

        if (LoyaltyManager.Instance == null)
        {
            Debug.LogError("LoyaltyManager não encontrado!");
            return;
        }

        // Inicializa uma campanha de teste.
        InteractionTracker.Instance.BeginNewCampaign(
            testCampaignID
        );

    }
}
