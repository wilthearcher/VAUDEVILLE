
using UnityEngine;
using Ink.UnityIntegration;

public class NPCInteractionTeste : MonoBehaviour
{
    [Header("Informações do NPC")]
    [SerializeField] private string npcID;
    [SerializeField] private string npcName;
    [SerializeField] private InkFile inkFile;

    [Header("Influência sobre o Vaudeville")]
    [SerializeField] private bool canModifyLoyalty = false;

    public string NPCID => npcID;
    public string NPCName => npcName;
    public bool CanModifyLoyalty => canModifyLoyalty;

    public bool Interact(DialogueManagerTeste dialogueManager)
    {
        if (dialogueManager == null)
        {
            Debug.LogError("DialogueManager não foi encontrado.");
            return false;
        }

        if (inkFile == null)
        {
            Debug.LogError(
                "Arquivo Ink não associado ao NPC: " + npcName
            );
            return false;
        }

        if (string.IsNullOrWhiteSpace(npcID))
        {
            Debug.LogError(
                "O NPC não possui um ID configurado: " + npcName
            );
            return false;
        }

        if (dialogueManager.IsDialogueOpen)
            return false;

        dialogueManager.StartDialogue(
            inkFile,
            npcID,
            npcName,
            canModifyLoyalty
        );

        return true;
    }
}
