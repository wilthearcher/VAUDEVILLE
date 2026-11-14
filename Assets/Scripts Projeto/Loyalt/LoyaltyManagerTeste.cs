
using UnityEngine;
using System;

public class LoyaltyManagerTeste : MonoBehaviour
{
    public static LoyaltyManagerTeste Instance { get; private set; }

    private const int MIN_LOYALTY = -15;
    private const int MAX_LOYALTY = 15;

    [SerializeField] private int loyalty = 0;

    public int CurrentLoyalty => loyalty;

    public event Action<int> OnLoyaltyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        loyalty = Mathf.Clamp(
            loyalty,
            MIN_LOYALTY,
            MAX_LOYALTY
        );
    }

    public void ChangeLoyalty(int amount)
    {
        int previousValue = loyalty;

        loyalty = Mathf.Clamp(
            loyalty + amount,
            MIN_LOYALTY,
            MAX_LOYALTY
        );

        if (previousValue != loyalty)
            OnLoyaltyChanged?.Invoke(loyalty);

        Debug.Log("Lealdade atual: " + loyalty);
    }

    public void SetLoyalty(int value)
    {
        int previousValue = loyalty;

        loyalty = Mathf.Clamp(
            value,
            MIN_LOYALTY,
            MAX_LOYALTY
        );

        if (previousValue != loyalty)
            OnLoyaltyChanged?.Invoke(loyalty);
    }

    public void ResetLoyalty()
    {
        SetLoyalty(0);
    }

    public void SaveCampaign(string campaignID)
    {
        if (string.IsNullOrWhiteSpace(campaignID))
            return;

        PlayerPrefs.SetInt(
            "Loyalty_" + campaignID,
            loyalty
        );

        PlayerPrefs.Save();
    }

    public void LoadCampaign(string campaignID)
    {
        if (string.IsNullOrWhiteSpace(campaignID))
            return;

        int savedValue = PlayerPrefs.GetInt(
            "Loyalty_" + campaignID,
            0
        );

        SetLoyalty(savedValue);
    }

    public void DeleteCampaignSave(string campaignID)
    {
        if (string.IsNullOrWhiteSpace(campaignID))
            return;

        PlayerPrefs.DeleteKey("Loyalty_" + campaignID);
    }
}
