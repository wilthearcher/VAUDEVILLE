
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

        if (LoyaltyManagerTeste.Instance == null)
        {
            Debug.LogError("LoyaltyManager não encontrado!");
            return;
        }

        // Inicializa uma campanha de teste.
        InteractionTracker.Instance.BeginNewCampaign(
            testCampaignID
        );

//        Debug.Log("Campanha de teste iniciada!");
      //  Debug.Log("Lealdade inicial: " + LoyaltyManagerTeste.Instance.CurrentLoyalty);
    }
}
