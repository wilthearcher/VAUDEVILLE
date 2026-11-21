using UnityEngine;
using Ink.UnityIntegration;

public class NPCInteraction : MonoBehaviour
{
    [Header("Informações do NPC")]
    [SerializeField] private string npcID;
    [SerializeField] private string npcName;
    [SerializeField] private InkFile inkFile;

    [Header("Segundo diálogo")]
    [SerializeField] private InkFile secondInkFile;

    [Tooltip("Libera o segundo diálogo automaticamente após o primeiro. Desative quando a cutscene estiver pronta.")]
    [SerializeField] private bool unlockSecondDialogueAfterFirst = true;

    [Header("Influência sobre o Vaudeville")]
    [SerializeField] private bool canModifyLoyalty = false;

    private bool firstDialogueCompleted;
    private bool secondDialogueUnlocked;
    private bool secondDialogueCompleted;

    private int currentStage;

    public string NPCID => npcID;
    public string NPCName => npcName;
    public bool CanModifyLoyalty => canModifyLoyalty;

    private DialogueManager subscribedManager;


    public bool Interact(DialogueManager dialogueManager)
    {

        if (!LetterIntroController.InteractionsUnlocked)
        {
            Debug.Log("[NPC] Interação bloqueada: a carta ainda está aberta.");
            return false;
        }

        if (dialogueManager == null)
        {
            Debug.LogError("DialogueManager não foi encontrado.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(npcID))
        {
            Debug.LogError("NPC sem ID configurado: " + npcName);
            return false;
        }

        if (dialogueManager.IsDialogueOpen)
            return false;

        if (secondDialogueCompleted)
        {
            Debug.Log("O NPC já concluiu as duas interações.");
            return false;
        }

        InkFile selectedInkFile;
        bool completeInteraction;

        // NPC comum: conclui normalmente.
        if (secondInkFile == null)
        {
            selectedInkFile = inkFile;
            completeInteraction = true;
            currentStage = 0;
        }
        // NPC com dois diálogos: comportamento especial do Barão.
        else if (!firstDialogueCompleted)
        {
            selectedInkFile = inkFile;
            completeInteraction = false;
            currentStage = 1;
        }
        else
        {
            if (!secondDialogueUnlocked)
            {
                Debug.Log("O segundo diálogo ainda não foi liberado.");
                return false;
            }

            selectedInkFile = secondInkFile;
            completeInteraction = true;
            currentStage = 2;
        }

        if (selectedInkFile == null)
        {
            Debug.LogError("Arquivo INK não configurado para " + npcName);
            return false;
        }

        // Só precisamos acompanhar o evento para NPCs com dois diálogos.
        if (secondInkFile != null)
            SubscribeToDialogueManager(dialogueManager);

        dialogueManager.StartDialogue(
            selectedInkFile,
            npcID,
            npcName,
            canModifyLoyalty,
            completeInteraction
        );

        return dialogueManager.IsDialogueOpen;
    }


    private void SubscribeToDialogueManager(DialogueManager manager)
    {
        if (subscribedManager != null)
            subscribedManager.OnDialogueFinished -= HandleDialogueFinished;

        subscribedManager = manager;
        subscribedManager.OnDialogueFinished += HandleDialogueFinished;
    }


    private void HandleDialogueFinished(string finishedNPCID)
    {
        if (finishedNPCID != npcID)
            return;

        if (currentStage == 1)
        {
            firstDialogueCompleted = true;

            if (unlockSecondDialogueAfterFirst)
            {
                secondDialogueUnlocked = true;
                Debug.Log("Segundo diálogo do Barão liberado para teste!");
            }
            else
            {
                Debug.Log("Aguardando o término da cutscene.");
            }
        }
        else if (currentStage == 2)
        {
            secondDialogueCompleted = true;
            Debug.Log("As duas interações do Barão foram concluídas.");
        }
    }


    // Será chamado pelo controlador da cutscene futuramente.
    public void UnlockSecondDialogue()
    {
        if (!firstDialogueCompleted || secondDialogueCompleted)
            return;

        secondDialogueUnlocked = true;

        Debug.Log("Segundo diálogo do Barão liberado!");
    }

    private void OnDestroy()
    {
        if (subscribedManager != null)
            subscribedManager.OnDialogueFinished -= HandleDialogueFinished;
    }
}

