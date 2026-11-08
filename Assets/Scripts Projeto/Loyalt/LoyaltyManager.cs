
using System;
using System.Collections.Generic;
using UnityEngine;

public class LoyaltyManager : MonoBehaviour
{
    public static LoyaltyManager Instance { get; private set; }

    private const float MIN_LOYALTY = -15.0f;
    private const float MAX_LOYALTY = 15.0f;

    [SerializeField]
    private float currentLoyalty = 0.0f;

    public float CurrentLoyalty => currentLoyalty;

    public event Action<float> OnLoyaltyChanged;

    private readonly HashSet<string> allowedNPCs = new HashSet<string>
    {
        "npc_01",
        "npc_02",
        "npc_03"
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Já existe um LoyaltyManager na cena.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentLoyalty = 0.0f;

        Debug.Log("LoyaltyManager iniciado. Pontuação: 0.0");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool CanAffectLoyalty(string npcID)
    {
        return !string.IsNullOrEmpty(npcID) &&
               allowedNPCs.Contains(npcID);
    }

    public void ChangeLoyalty(string npcID, float amount)
    {
        if (!CanAffectLoyalty(npcID))
        {
            Debug.LogWarning(
                "NPC não autorizado a modificar a lealdade: " + npcID
            );
            return;
        }

        if (!IsValidAmount(amount))
        {
            Debug.LogWarning(
                "Valor inválido! Utilize apenas -2, -1, 0, +1 ou +2."
            );
            return;
        }

        float previousScore = currentLoyalty;

        currentLoyalty = Mathf.Clamp(
            currentLoyalty + amount,
            MIN_LOYALTY,
            MAX_LOYALTY
        );

        Debug.Log(
            "Lealdade modificada pelo NPC " + npcID +
            ": " + previousScore + " -> " + currentLoyalty
        );

        OnLoyaltyChanged?.Invoke(currentLoyalty);
    }

    private bool IsValidAmount(float amount)
    {
        return Mathf.Approximately(amount, -2.0f) ||
               Mathf.Approximately(amount, -1.0f) ||
               Mathf.Approximately(amount, 0.0f) ||
               Mathf.Approximately(amount, 1.0f) ||
               Mathf.Approximately(amount, 2.0f);
    }
}
