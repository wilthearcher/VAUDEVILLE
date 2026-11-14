
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;
using Ink.UnityIntegration;

public class DialogueManagerTeste : MonoBehaviour
{
    [Header("Interface")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button closeButton;

    public bool IsDialogueOpen { get; private set; }

    public event Action<bool> OnDialogueStateChanged;

    private Story story;

    private string currentNPCID;
    private string currentNPCName;

    private bool currentNPCCanModifyLoyalty;
    private bool interactionStarted;
    private bool waitingToFinish;

    private readonly List<GameObject> spawnedChoices =
        new List<GameObject>();

    private void Awake()
    {
        dialoguePanel.SetActive(false);

        continueButton.onClick.AddListener(ContinueDialogue);
        closeButton.onClick.AddListener(CloseDialogue);
    }

    public void StartDialogue(
        InkFile inkFile,
        string npcID,
        string npcName,
        bool canModifyLoyalty
    )
    {
        if (IsDialogueOpen)
            return;

        if (InteractionTracker.Instance == null)
        {
            Debug.LogError("InteractionTracker não encontrado.");
            return;
        }

        if (InteractionTracker.Instance.HasInteracted(npcID))
        {
            ShowAlreadyInteractedMessage(npcName);
            return;
        }

        if (!InteractionTracker.Instance.TryStartInteraction(npcID))
            return;

        interactionStarted = true;

        currentNPCID = npcID;
        currentNPCName = npcName;
        currentNPCCanModifyLoyalty = canModifyLoyalty;

       Debug.Log("ARQUIVO INK: " + inkFile.name);

Debug.Log(
    "HISTÓRIA CONTÉM loyalty_delta: " +
    inkFile.storyJson.Contains("loyalty_delta")
);

story = new Story(inkFile.storyJson);

Debug.Log("História INK carregada.");

        story.ChoosePathString("start");

        dialoguePanel.SetActive(true);

        IsDialogueOpen = true;
        OnDialogueStateChanged?.Invoke(true);

        npcNameText.text = currentNPCName;

        waitingToFinish = false;

        ContinueDialogue();
    }

    
private void ContinueDialogue()
{
    if (story == null || !IsDialogueOpen)
        return;

    ClearChoices();

    if (story.canContinue)
    {
        dialogueText.text = story.Continue().Trim();
        continueButton.gameObject.SetActive(true);
        return;
    }

    if (story.currentChoices.Count > 0)
    {
        ShowChoices();
        return;
    }

    // Não existem mais falas nem escolhas.
    FinishInteraction();
}


    private void UpdateContinueButton()
    {
        bool hasChoices = story.currentChoices.Count > 0;

        continueButton.gameObject.SetActive(
            !hasChoices
        );
    }

    private void ShowChoices()
    {
//        Debug.Log("ShowChoices foi chamado!");

    //Debug.Log("Quantidade de escolhas no Ink: " + story.currentChoices.Count);

    // Continue aqui com o código atual do método.
        dialogueText.gameObject.SetActive(false);
        
        ClearChoices();

        continueButton.gameObject.SetActive(false);

        int currentLoyalty =
            LoyaltyManagerTeste.Instance != null
            ? LoyaltyManagerTeste.Instance.CurrentLoyalty
            : 0;

        for (int i = 0; i < story.currentChoices.Count; i++)
        {
            Choice choice = story.currentChoices[i];
            Debug.Log("========== TESTE DAS TAGS ==========");
          //  Debug.Log("Alternativa: " + choice.text);

    Debug.Log(
    "Quantidade de tags: " +
    (choice.tags == null ? -1 : choice.tags.Count)
);

if (choice.tags != null)
{
    foreach (string tag in choice.tags)
    {
        Debug.Log("TAG RECEBIDA: [" + tag + "]");
    }
}

Debug.Log("====================================");

            int minLoyalty = GetTagValue(
                choice,
                "min_loyalty",
                -15
            );

            int maxLoyalty = GetTagValue(
                choice,
                "max_loyalty",
                15
            );

            int delta = GetTagValue(
                choice,
                "loyalty_delta",
                0
            );

            bool unlocked =
                currentLoyalty >= minLoyalty &&
                currentLoyalty <= maxLoyalty;

            if (!currentNPCCanModifyLoyalty)
                delta = 0;

            GameObject buttonObject = Instantiate(
                choiceButtonPrefab,
                choicesContainer
            );

            DialogueChoiceTeste dialogueChoice =
                buttonObject.GetComponent<DialogueChoiceTeste>();

            if (dialogueChoice == null)
            {
                Debug.LogError(
                    "O prefab precisa ter o componente DialogueChoice."
                );

                Destroy(buttonObject);
                continue;
            }

            int choiceIndex = i;

            dialogueChoice.Setup(
                choice.text,
                minLoyalty,
                maxLoyalty,
                delta,
                unlocked,
                () => ChooseChoice(choiceIndex, delta)
            );

            spawnedChoices.Add(buttonObject);
        }
    }

    
private void ChooseChoice(int choiceIndex, int delta)
{
    if (story == null)
        return;

    // Reexibe o texto após selecionar uma alternativa.
    dialogueText.gameObject.SetActive(true);

    if (choiceIndex < 0 ||
        choiceIndex >= story.currentChoices.Count)
        return;


    ClearChoices();

    if (currentNPCCanModifyLoyalty &&
        LoyaltyManagerTeste.Instance != null)
    {
        //LoyaltyManagerTeste.Instance.ChangeLoyalty(delta);
        /*Debug.Log("===== TESTE DE LEALDADE =====");
        Debug.Log("NPC: " + currentNPCID);
        Debug.Log("Pode modificar lealdade: " + currentNPCCanModifyLoyalty);
        Debug.Log("Delta recebido: " + delta);*/

if (LoyaltyManagerTeste.Instance == null)
{
    Debug.LogError("LoyaltyManagerTeste não encontrado!");
}
else if (currentNPCCanModifyLoyalty)
{
    Debug.Log("Lealdade antes: " +
        LoyaltyManagerTeste.Instance.CurrentLoyalty);

    LoyaltyManagerTeste.Instance.ChangeLoyalty(delta);

    Debug.Log("Lealdade depois: " +
        LoyaltyManagerTeste.Instance.CurrentLoyalty);
}
else
{
    Debug.LogWarning("Este NPC não está autorizado a modificar a lealdade.");
}

Debug.Log("=============================");
    }

    // A escolha já foi feita: a interação não poderá ser repetida.
    if (interactionStarted)
    {
        InteractionTracker.Instance.CompleteInteraction(
            currentNPCID
        );

        interactionStarted = false;
    }

    story.ChooseChoiceIndex(choiceIndex);

    ContinueDialogue();
}


   
private int GetTagValue(
    Choice choice,
    string tagName,
    int defaultValue
)
{
    // Verifica se a escolha existe.
    if (choice == null)
    {
        Debug.LogWarning(
            "GetTagValue recebeu uma escolha nula."
        );

        return defaultValue;
    }

    // Verifica se a escolha possui tags.
    if (choice.tags == null || choice.tags.Count == 0)
    {
        Debug.Log(
            "A escolha '" + choice.text +
            "' não possui tags. Valor padrão: " + defaultValue
        );

        return defaultValue;
    }

    foreach (string tag in choice.tags)
    {
        if (string.IsNullOrWhiteSpace(tag))
            continue;

        string[] parts = tag.Split(
            new char[] { ':' },
            2
        );

        if (parts.Length != 2)
            continue;

        if (parts[0].Trim() == tagName)
        {
            if (int.TryParse(
                parts[1].Trim(),
                out int result
            ))
            {
                Debug.Log(
                    "Tag encontrada: " + tagName +
                    " = " + result
                );

                return result;
            }
        }
    }

    return defaultValue;
}

    private void FinishInteraction()
    {
        if (interactionStarted)
        {
            InteractionTracker.Instance.CompleteInteraction(
                currentNPCID
            );

            interactionStarted = false;
        }

        ResetDialogueState();
    }

    private void ShowAlreadyInteractedMessage(string npcName)
    {
        dialoguePanel.SetActive(true);

        IsDialogueOpen = true;
        OnDialogueStateChanged?.Invoke(true);

        npcNameText.text = npcName;
        dialogueText.text = "Você já conversou comigo.";

        continueButton.gameObject.SetActive(false);
        ClearChoices();
    }

    public void CloseDialogue()
    {
        if (!IsDialogueOpen)
            return;

        if (interactionStarted)
        {
            InteractionTracker.Instance.CancelInteraction(
                currentNPCID
            );

            interactionStarted = false;
        }

        ResetDialogueState();
    }

    private void ResetDialogueState()
    {
        story = null;

        currentNPCID = null;
        currentNPCName = null;

        currentNPCCanModifyLoyalty = false;
        waitingToFinish = false;

        ClearChoices();

        dialoguePanel.SetActive(false);

        IsDialogueOpen = false;
        OnDialogueStateChanged?.Invoke(false);
    }

    private void ClearChoices()
    {
        foreach (GameObject choice in spawnedChoices)
        {
            if (choice != null)
                Destroy(choice);
        }

        spawnedChoices.Clear();
    }

    private void OnDestroy()
    {
        continueButton.onClick.RemoveListener(ContinueDialogue);
        closeButton.onClick.RemoveListener(CloseDialogue);
    }
}
