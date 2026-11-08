
using System;
using UnityEngine;
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
    [SerializeField] private Button choiceButtonPrefab;
    [SerializeField] private Button closeButton;

    private Story story;
    private string currentNPCID;

    public bool IsDialogueOpen { get; private set; }

    public event Action<bool> OnDialogueStateChanged;

    private void Start()
    {
        if (
            dialoguePanel == null ||
            npcNameText == null ||
            dialogueText == null ||
            choicesContainer == null ||
            choiceButtonPrefab == null ||
            closeButton == null
        )
        {
            Debug.LogError(
                "DialogueManager possui referências faltando."
            );

            enabled = false;
            return;
        }

        dialoguePanel.SetActive(false);

        // Configura o botão de fechar.
        closeButton.onClick.RemoveListener(CloseDialogue);
        closeButton.onClick.AddListener(CloseDialogue);

        closeButton.interactable = true;

        // Garante que a imagem do botão receba o Raycast.
        Image closeImage = closeButton.GetComponent<Image>();

        if (closeImage != null)
        {
            closeImage.raycastTarget = true;
            closeButton.targetGraphic = closeImage;
        }

        closeButton.gameObject.SetActive(false);

        Debug.Log("DialogueManager inicializado.");
    }

    public void StartDialogue(
        InkFile inkFile,
        string npcID,
        string npcName
    )
    {
        if (inkFile == null)
        {
            Debug.LogError("Arquivo Ink não informado.");
            return;
        }

        if (IsDialogueOpen)
            return;

        if (string.IsNullOrEmpty(inkFile.storyJson))
        {
            Debug.LogError(
                "O JSON compilado do InkFile está vazio."
            );

            return;
        }

        try
        {
            story = new Story(inkFile.storyJson);
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "Erro ao criar Story: " + ex.Message
            );

            return;
        }

        currentNPCID = npcID != null ? npcID.Trim() : "";

        npcNameText.text = npcName;

        try
        {
            story.ChoosePathString("start");
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "Não foi possível encontrar o caminho start no INK: " +
                ex.Message
            );

            story = null;
            return;
        }

        dialoguePanel.SetActive(true);

        IsDialogueOpen = true;

        OnDialogueStateChanged?.Invoke(true);

        SetCloseButtonVisible(false);

        Debug.Log(
            "Diálogo iniciado com NPC: " + currentNPCID
        );

        ContinueDialogue();
    }

    private void ContinueDialogue()
    {
        if (story == null)
            return;

        ClearChoices();

        string dialogueContent = "";

        // Continua avançando até encontrar escolhas
        // ou até chegar ao final da história.
        while (
            story.canContinue &&
            story.currentChoices.Count == 0
        )
        {
            string nextText = story.Continue().Trim();

            if (!string.IsNullOrEmpty(nextText))
            {
                if (!string.IsNullOrEmpty(dialogueContent))
                    dialogueContent += "\n";

                dialogueContent += nextText;
            }
        }

        dialogueText.text = dialogueContent;

        if (story.currentChoices.Count > 0)
        {
            SetCloseButtonVisible(false);
            ShowChoices();
        }
        else
        {
            SetCloseButtonVisible(true);

            Debug.Log(
                "Diálogo chegou ao fim. Botão de fechar ativado."
            );
        }
    }

    private void ShowChoices()
    {
        float currentLoyalty = 0f;

        if (LoyaltyManager.Instance != null)
        {
            currentLoyalty =
                LoyaltyManager.Instance.CurrentLoyalty;
        }

        Debug.Log(
            "Quantidade de escolhas: " +
            story.currentChoices.Count
        );

        foreach (Choice inkChoice in story.currentChoices)
        {
            DialogueChoice choice =
                new DialogueChoice(inkChoice);

            Debug.Log(
                "Escolha: " + choice.Text +
                " | Delta: " + choice.LoyaltyDelta +
                " | Possui delta: " + choice.HasLoyaltyDelta
            );

            Button button = Instantiate(
                choiceButtonPrefab,
                choicesContainer
            );

            TMP_Text buttonText =
                button.GetComponentInChildren<TMP_Text>();

            if (buttonText == null)
            {
                Debug.LogError(
                    "O prefab do botão não possui TMP_Text."
                );

                Destroy(button.gameObject);
                continue;
            }

            bool unlocked =
                choice.IsUnlocked(currentLoyalty);

            buttonText.text = choice.Text;

            if (!unlocked)
            {
                buttonText.text +=
                    "\n(Requer lealdade: " +
                    choice.RequiredLoyalty + ")";
            }

            button.interactable = unlocked;

            DialogueChoice selectedChoice = choice;

            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(() =>
            {
                Debug.Log(
                    "Escolha clicada: " + selectedChoice.Text
                );

                ChooseChoice(selectedChoice);
            });

            Debug.Log(
                "Botão criado: " + choice.Text +
                " | Interactable: " + button.interactable +
                " | Lealdade: " + currentLoyalty
            );
        }
    }

    private void ChooseChoice(DialogueChoice choice)
    {
        if (story == null || choice == null)
            return;

        float currentLoyalty = 0f;

        if (LoyaltyManager.Instance != null)
        {
            currentLoyalty =
                LoyaltyManager.Instance.CurrentLoyalty;
        }

        if (!choice.IsUnlocked(currentLoyalty))
        {
            Debug.LogWarning(
                "Escolha bloqueada pelo requisito de lealdade."
            );

            return;
        }

        // Aplica a alteração de lealdade.
        if (LoyaltyManager.Instance == null)
        {
            Debug.LogError(
                "LoyaltyManager não encontrado na cena."
            );
        }
        else
        {
            Debug.Log(
                "NPC ID recebido: " + currentNPCID
            );

            Debug.Log(
                "Delta recebido: " + choice.LoyaltyDelta
            );

            Debug.Log(
                "Lealdade antes: " +
                LoyaltyManager.Instance.CurrentLoyalty
            );

            if (!LoyaltyManager.Instance.CanAffectLoyalty(currentNPCID))
            {
                Debug.LogWarning(
                    "Este NPC não está autorizado a modificar a lealdade: " +
                    currentNPCID
                );
            }
            else if (!choice.HasLoyaltyDelta)
            {
                Debug.LogWarning(
                    "Esta escolha não possui uma tag delta no INK."
                );
            }
            else
            {
                LoyaltyManager.Instance.ChangeLoyalty(
                    currentNPCID,
                    choice.LoyaltyDelta
                );

                Debug.Log(
                    "Lealdade depois: " +
                    LoyaltyManager.Instance.CurrentLoyalty
                );
            }
        }

        // Avança a história INK.
        story.ChooseChoiceIndex(
            choice.InkChoice.index
        );

        ClearChoices();

        ContinueDialogue();
    }

    private void SetCloseButtonVisible(bool visible)
    {
        closeButton.gameObject.SetActive(visible);

        if (visible)
        {
            closeButton.interactable = true;

            // Coloca o botão na frente dos outros elementos
            // que estejam no mesmo objeto pai.
            closeButton.transform.SetAsLastSibling();
        }
    }

    private void ClearChoices()
    {
        foreach (Transform child in choicesContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void CloseDialogue()
    {
        Debug.Log("Clique recebido no botão de fechar.");

        if (!IsDialogueOpen)
            return;

        ClearChoices();

        dialogueText.text = "";
        npcNameText.text = "";

        SetCloseButtonVisible(false);

        dialoguePanel.SetActive(false);

        story = null;

        currentNPCID = "";

        IsDialogueOpen = false;

        OnDialogueStateChanged?.Invoke(false);

        Debug.Log("Diálogo encerrado.");
    }
}
