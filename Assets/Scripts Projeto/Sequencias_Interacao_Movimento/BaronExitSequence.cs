
using UnityEngine;

public class BaronExitSequence : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private NPCFollower baronFollower;
    [SerializeField] private GameObject baronObject;

    [Header("Saída")]
    [SerializeField] private Transform exitPosition;

    [Header("Identificação")]
    [SerializeField] private string baronID;

    private bool exitStarted;

    private void OnEnable()
    {
        if (dialogueManager != null)
            dialogueManager.OnDialogueFinished += HandleDialogueFinished;
    }

    private void OnDisable()
    {
        if (dialogueManager != null)
            dialogueManager.OnDialogueFinished -= HandleDialogueFinished;

        if (baronFollower != null)
            baronFollower.OnDestinationReached -= HandleBaronReachedExit;
    }

    private void HandleDialogueFinished(string finishedNPCID)
    {
        // Verifica se foi o diálogo do Barão.
        if (finishedNPCID != baronID)
            return;

        if (exitStarted)
            return;

        // Confirma que a segunda interação foi concluída.
        if (InteractionTracker.Instance == null ||
            !InteractionTracker.Instance.HasInteracted(baronID))
            return;

        StartBaronExit();
    }

    private void StartBaronExit()
    {
        if (baronFollower == null || exitPosition == null)
        {
            Debug.LogError("Referências da saída do Barão não configuradas.");
            return;
        }

        exitStarted = true;

        baronFollower.OnDestinationReached += HandleBaronReachedExit;

        baronFollower.SetDestinationAndStart(exitPosition);

        Debug.Log("Barão iniciou sua saída do salão.");
    }

    private void HandleBaronReachedExit()
    {
        baronFollower.OnDestinationReached -= HandleBaronReachedExit;

        baronFollower.StopFollowing();

        baronObject.SetActive(false);

        Debug.Log("Barão saiu do salão.");
    }
}
