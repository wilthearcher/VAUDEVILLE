using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;
using Ink.UnityIntegration;

public class DialogueManager : MonoBehaviour
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

    private bool demoEndPending = false;

    private bool completeInteractionOnFinish = true;

    // Nome da scene a carregar quando o diálogo terminar.
    private string sceneToLoadOnFinish;

    [Header("Configuração do texto")]
    [SerializeField] private int maxLinesPerPage = 4;

    private List<string> dialoguePages = new List<string>();
    private int currentPageIndex = 0;

    public event System.Action<string> OnDialogueFinished;

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
        bool canModifyLoyalty,
        bool completeInteraction = true,
        string sceneToLoad = null)
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
        demoEndPending = false;

        currentNPCID = npcID;
        currentNPCName = npcName;
        currentNPCCanModifyLoyalty = canModifyLoyalty;

        completeInteractionOnFinish = completeInteraction;
        sceneToLoadOnFinish = sceneToLoad;

        Debug.Log("ARQUIVO INK: " + inkFile.name);

        Debug.Log("HISTÓRIA CONTÉM loyalty_delta: " + inkFile.storyJson.Contains("loyalty_delta"));

        story = new Story(inkFile.storyJson);

        Debug.Log("História INK carregada.");

        story.ChoosePathString("start");

        dialoguePanel.SetActive(true);

        IsDialogueOpen = true;
        OnDialogueStateChanged?.Invoke(true);

        npcNameText.text = currentNPCName;

        ContinueDialogue();
    }

    private void ContinueDialogue()
    {
        if (story == null || !IsDialogueOpen)
            return;

        // Primeiro verifica se ainda existem páginas
        // da fala atual para apresentar.
        if (currentPageIndex < dialoguePages.Count - 1)
        {
            currentPageIndex++;

            dialogueText.text = dialoguePages[currentPageIndex];

            return;
        }

        // A fala terminou. Podemos avançar o Ink.
        dialoguePages.Clear();
        currentPageIndex = 0;

        ClearChoices();

        if (story.canContinue)
        {
            string nextText = story.Continue().Trim();

            PrepareDialoguePages(nextText);

            continueButton.gameObject.SetActive(true);

            return;
        }

        if (story.currentChoices.Count > 0)
        {
            ShowChoices();
            return;
        }

        // O Ink terminou completamente.
        bool shouldEndDemo = demoEndPending;

        FinishInteraction();

        if (shouldEndDemo)
        {
            if (DemoEndController.Instance != null)
            {
                DemoEndController.Instance.EndDemo();
            }
            else
            {
                Debug.LogError("DemoEndController não encontrado.");
            }
        }
    }

    private void PrepareDialoguePages(string fullText)
    {
        dialoguePages.Clear();
        currentPageIndex = 0;

        dialogueText.text = fullText;

        // Calcula as linhas considerando o tamanho real
        // do balão e as configurações do TextMeshPro.
        dialogueText.ForceMeshUpdate();

        TMP_TextInfo textInfo = dialogueText.textInfo;

        int totalLines = textInfo.lineCount;

        if (totalLines == 0)
        {
            dialoguePages.Add(fullText);
            dialogueText.text = fullText;
            return;
        }

        for (int startLine = 0; startLine < totalLines;
             startLine += maxLinesPerPage)
        {
            int endLine = Mathf.Min(
                startLine + maxLinesPerPage,
                totalLines
            );

            int startIndex = textInfo.characterInfo[
                textInfo.lineInfo[startLine].firstCharacterIndex
            ].index;

            int endIndex;

            if (endLine < totalLines)
            {
                int nextLineFirstCharacter =
                    textInfo.lineInfo[endLine].firstCharacterIndex;

                endIndex = textInfo.characterInfo[
                    nextLineFirstCharacter
                ].index;
            }
            else
            {
                endIndex = fullText.Length;
            }

            int length = Mathf.Max(0, endIndex - startIndex);

            string page = fullText.Substring(startIndex, length);

            dialoguePages.Add(page);
        }

        // Exibe a primeira página.
        dialogueText.text = dialoguePages[0];
    }

    private void ClearDialoguePages()
    {
        dialoguePages.Clear();
        currentPageIndex = 0;
    }

    private void UpdateContinueButton()
    {
        bool hasChoices = story.currentChoices.Count > 0;
        continueButton.gameObject.SetActive(!hasChoices);
    }

    private void ShowChoices()
    {
        dialogueText.gameObject.SetActive(false);

        ClearChoices();

        continueButton.gameObject.SetActive(false);

        int currentLoyalty =
            LoyaltyManager.Instance != null
            ? LoyaltyManager.Instance.CurrentLoyalty
            : 0;

        for (int i = 0; i < story.currentChoices.Count; i++)
        {
            Choice choice = story.currentChoices[i];

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

            DialogueChoice dialogueChoice =
                buttonObject.GetComponent<DialogueChoice>();

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

        if (currentNPCCanModifyLoyalty && LoyaltyManager.Instance != null)
        {
            Debug.Log("Lealdade antes: " + LoyaltyManager.Instance.CurrentLoyalty);

            LoyaltyManager.Instance.ChangeLoyalty(delta);

            Debug.Log("Lealdade depois: " + LoyaltyManager.Instance.CurrentLoyalty);
        }

        // Verifica se a alternativa selecionada encerra a demo.
        bool endsDemo = false;

        if (story.currentChoices[choiceIndex].tags != null)
        {
            foreach (string tag in story.currentChoices[choiceIndex].tags)
            {
                if (tag.Trim() == "end_demo")
                {
                    endsDemo = true;
                    break;
                }
            }
        }

        // Guarda a informação para encerrar a demo
        // somente depois que o Ink terminar a resposta.
        if (endsDemo)
        {
            demoEndPending = true;
        }

        story.ChooseChoiceIndex(choiceIndex);

        // Continua normalmente, exibindo a resposta do Barão.
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
            if (completeInteractionOnFinish)
            {
                InteractionTracker.Instance.CompleteInteraction(
                    currentNPCID
                );
            }
            else
            {
                // Libera o NPC para a próxima etapa.
                InteractionTracker.Instance.CancelInteraction(
                    currentNPCID
                );
            }

            interactionStarted = false;
        }

        string finishedID = currentNPCID;

        // Captura antes do reset, que limpa esses valores.
        string sceneToLoad = sceneToLoadOnFinish;
        bool endingDemo = demoEndPending;

        Debug.Log("FinishInteraction -> scene: '" + sceneToLoad + "' | endingDemo: " + endingDemo);

        OnDialogueFinished?.Invoke(finishedID);

        ResetDialogueState();

        // Se a demo vai ser encerrada, ela cuida da própria transição.
        if (!endingDemo && !string.IsNullOrEmpty(sceneToLoad))
        {
            if (SceneTransition.Instance != null)
                SceneTransition.Instance.LoadScene(sceneToLoad);
            else
                SceneManager.LoadScene(sceneToLoad); // fallback sem fade
        }
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

        if (demoEndPending)
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
        demoEndPending = false; //alterar se precisar
        story = null;

        ClearDialoguePages();

        currentNPCID = null;
        currentNPCName = null;
        sceneToLoadOnFinish = null;

        currentNPCCanModifyLoyalty = false;

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