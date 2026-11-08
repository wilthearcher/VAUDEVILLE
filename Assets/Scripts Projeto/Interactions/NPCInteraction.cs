
using UnityEngine;
using Ink.UnityIntegration;

public class NPCInteraction : MonoBehaviour
{
    [Header("Informações do NPC")]
    [SerializeField] private string npcID;
    [SerializeField] private string npcName;
    [SerializeField] private InkFile inkFile;

    public bool Interact(DialogueManager dialogueManager)
    {
        if (dialogueManager == null)
        {
            Debug.LogError(
                "DialogueManager não foi encontrado."
            );

            return false;
        }

        if (inkFile == null)
        {
            Debug.LogError(
                "Arquivo Ink não foi associado ao NPC: " + npcName
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

        Debug.Log(
            "Interagindo com NPC: " +
            npcName +
            " | ID: " +
            npcID
        );

        dialogueManager.StartDialogue(
            inkFile,
            npcID,
            npcName
        );

        return true;
    }
}
