using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InteractionSaveData
{
    public List<string> completedNPCs = new List<string>();
}

public class InteractionTracker : MonoBehaviour
{
    public static InteractionTracker Instance { get; private set; }

    private HashSet<string> completedNPCs = new HashSet<string>();
    private HashSet<string> activeNPCs = new HashSet<string>();

    private string currentCampaignID;

    private string SaveKey =>
        "CompletedInteractions_" + currentCampaignID;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    
public void BeginNewCampaign(string campaignID)
{
    if (string.IsNullOrWhiteSpace(campaignID))
    {
        Debug.LogError("ID da campanha inválido.");
        return;
    }

    currentCampaignID = campaignID;

    completedNPCs.Clear();
    activeNPCs.Clear();

    if (LoyaltyManager.Instance != null)
        LoyaltyManager.Instance.ResetLoyalty();

    SaveInteractions();
}


    public void LoadCampaign(string campaignID)
    {
        if (string.IsNullOrWhiteSpace(campaignID))
        {
            Debug.LogError("ID da campanha inválido.");
            return;
        }

        currentCampaignID = campaignID;

        completedNPCs.Clear();
        activeNPCs.Clear();

        string json = PlayerPrefs.GetString(SaveKey, "");

        if (!string.IsNullOrEmpty(json))
        {
            InteractionSaveData data =
                JsonUtility.FromJson<InteractionSaveData>(json);

            if (data != null && data.completedNPCs != null)
            {
                completedNPCs = new HashSet<string>(
                    data.completedNPCs
                );
            }
        }

        if (LoyaltyManager.Instance != null)
            LoyaltyManager.Instance.LoadCampaign(campaignID);
    }

    public bool TryStartInteraction(string npcID)
    {
        if (string.IsNullOrWhiteSpace(currentCampaignID))
        {
            Debug.LogError("Nenhuma campanha foi inicializada.");
            return false;
        }

        if (HasInteracted(npcID) || activeNPCs.Contains(npcID))
            return false;

        activeNPCs.Add(npcID);
        return true;
    }

    public void CompleteInteraction(string npcID)
    {
        if (string.IsNullOrWhiteSpace(npcID))
            return;

        activeNPCs.Remove(npcID);
        completedNPCs.Add(npcID);

        SaveInteractions();
    }

    public void CancelInteraction(string npcID)
    {
        if (string.IsNullOrWhiteSpace(npcID))
            return;

        activeNPCs.Remove(npcID);
    }

    public bool HasInteracted(string npcID)
    {
        return completedNPCs.Contains(npcID);
    }

    private void SaveInteractions()
    {
        if (string.IsNullOrWhiteSpace(currentCampaignID))
            return;

        InteractionSaveData data = new InteractionSaveData();

        data.completedNPCs = new List<string>(completedNPCs);

        string json = JsonUtility.ToJson(data);

        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();

        if (LoyaltyManager.Instance != null)
            LoyaltyManager.Instance.SaveCampaign(currentCampaignID);
    }
}
