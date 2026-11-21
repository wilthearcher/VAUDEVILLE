using System.Collections;
using UnityEngine;

public class OfficeEventController : MonoBehaviour
{
    public static OfficeEventController Instance { get; private set; }

    private enum OfficeState
    {
        WaitingForDocument,
        WaitingForAction,
        Hiding,
        GuardSequence,
        WaitingForDoor,
        WalkingToDoor,
        Final
    }

    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip handleSound;
    [SerializeField] private AudioClip doorKnockSound;
    [SerializeField] private AudioClip keySound;

    [Header("Tempos")]
    [SerializeField] private float handleDelay = 30f;
    [SerializeField] private float actionTimeLimit = 10f;
    [SerializeField] private float keyDelay = 2f;

    [Header("Referências")]
    [SerializeField] private OfficePlayerAction playerAction;
    [SerializeField] private SecurityGuardController securityGuard;

    [Header("Fala de Ícaro / Jogador")]
    [SerializeField] private string icaroName = "Ícaro";
    [TextArea(2, 4)]
    [SerializeField] private string icaroDialogue = "Ufa... Acho que ele já foi.";
    [SerializeField] private float icaroSpeechDuration = 2.5f;

    [Header("Efeitos de Hover")]
    [SerializeField] private NPCHoverEffect deskHover;
    [SerializeField] private NPCHoverEffect doorHover;

    private OfficeState currentState = OfficeState.WaitingForDocument;

    private Coroutine handleCoroutine;
    private Coroutine actionTimerCoroutine;
    private Coroutine hideCoroutine;

    private bool secondPageActivated;
    private bool documentsClosed;

    public bool InspectionsLocked { get; private set; }

    private void Awake()
{
    if (Instance != null && Instance != this)
    {
        Debug.LogError("[ESCRITÓRIO] Existe mais de um OfficeEventController na cena.");
        Destroy(gameObject);
        return;
    }

    Instance = this;

    if (deskHover != null)
        deskHover.SetHoverEnabled(false);
    else
        Debug.LogError("[ESCRITÓRIO] Desk Hover não foi atribuído no Inspector.");

    if (doorHover != null)
        doorHover.SetHoverEnabled(false);
    else
        Debug.LogError("[ESCRITÓRIO] Door Hover não foi atribuído no Inspector.");
}

    // CHAMAR ASSIM QUE A SEGUNDA PÁGINA FOR ATIVADA.
    
public void OnSecondPageOpened()
{
    if (secondPageActivated)
    {
        Debug.LogWarning("[ESCRITÓRIO] A segunda página já havia sido ativada. O timer de 30s não será reiniciado.");
        return;
    }

    secondPageActivated = true;

    Debug.Log("[ESCRITÓRIO] Segunda página ativada.");
    Debug.Log("[TIMER 30s] Iniciado pela ativação da segunda página.");

    // A porta fica disponível desde a abertura da segunda página.
    doorHover.SetHoverEnabled(true);

    // A mesa ainda não fica disponível nesta etapa.
    deskHover.SetHoverEnabled(false);

    handleCoroutine = StartCoroutine(HandleDelayRoutine());
}

    private IEnumerator HandleDelayRoutine()
    {
        float elapsed = 0f;
        int nextLog = 25;

        while (elapsed < handleDelay)
        {
            yield return new WaitForSecondsRealtime(1f);
            elapsed += 1f;

            int remaining = Mathf.CeilToInt(handleDelay - elapsed);

            if (remaining <= nextLog && remaining > 0)
            {
                Debug.Log($"[TIMER 30s] {remaining} segundos restantes.");
                nextLog -= 5;
            }
        }

        handleCoroutine = null;

        Debug.Log("[TIMER 30s] Concluído.");

        if (audioSource != null && handleSound != null)
        {
            audioSource.PlayOneShot(handleSound);
            Debug.Log("[ÁUDIO] Som de maçaneta reproduzido.");
        }
        else
        {
            Debug.LogWarning("[ÁUDIO] handleSound ou audioSource não foi atribuído.");
        }
    }

    // CHAMAR QUANDO O JOGADOR FECHAR OS DOCUMENTOS.
    public void OnDocumentClosed()
    {
        if (!secondPageActivated)
        {
            Debug.LogWarning("[DOCUMENTOS] Tentativa de fechar os documentos antes da ativação da segunda página.");
            return;
        }

        if (documentsClosed)
        {
            Debug.LogWarning("[DOCUMENTOS] Os documentos já foram fechados. O timer de 10s não será reiniciado.");
            return;
        }

        documentsClosed = true;
        InspectionsLocked = true;

        // Libera o hover somente depois que os documentos forem fechados.
        if (deskHover != null)
            deskHover.SetHoverEnabled(true);

        if (doorHover != null)
            doorHover.SetHoverEnabled(true);

        Debug.Log("[HOVER] Mesa e porta liberadas após fechar os documentos.");

        Debug.Log("[DOCUMENTOS] Documentos fechados após a leitura da segunda página.");

        if (audioSource != null && doorKnockSound != null)
        {
            audioSource.PlayOneShot(doorKnockSound);
            Debug.Log("[ÁUDIO] Batida na porta reproduzida.");
        }

        currentState = OfficeState.WaitingForAction;

        Debug.Log("[TIMER 10s] Iniciado após o fechamento dos documentos.");

        actionTimerCoroutine = StartCoroutine(ActionTimerRoutine());
    }

    private IEnumerator ActionTimerRoutine()
    {
        float elapsed = 0f;
        int nextLog = Mathf.CeilToInt(actionTimeLimit) - 1;

        while (elapsed < actionTimeLimit)
        {
            yield return new WaitForSecondsRealtime(1f);
            elapsed += 1f;

            int remaining = Mathf.CeilToInt(actionTimeLimit - elapsed);

            if (remaining > 0 && remaining <= nextLog)
            {
                Debug.Log($"[TIMER 10s] {remaining} segundos restantes.");
                nextLog = remaining - 1;
            }
        }

        actionTimerCoroutine = null;

        if (currentState == OfficeState.WaitingForAction)
        {
            currentState = OfficeState.WaitingForDoor;

            Debug.Log("[TIMER 10s] Concluído. Somente a porta está disponível.");

            currentState = OfficeState.WaitingForDoor;

            deskHover.SetHoverEnabled(false);
            doorHover.SetHoverEnabled(true);

            Debug.Log("[HOVER] Mesa desabilitada. Porta continua disponível.");
        }
        else
        {
            Debug.Log("[TIMER 10s] Encerrado. O jogador já iniciou uma ação.");
        }
    }

    public bool TryHide()
    {
        if (currentState != OfficeState.WaitingForAction)
        {
            Debug.LogWarning("[AÇÃO] Não é possível esconder-se neste momento.");
            return false;
        }

        if (playerAction == null)
        {
            Debug.LogError("[AÇÃO] OfficePlayerAction não foi atribuído.");
            return false;
        }

        if (securityGuard == null)
        {
            Debug.LogError("[AÇÃO] SecurityGuardController não foi atribuído.");
            return false;
        }

        if (actionTimerCoroutine != null)
        {
            StopCoroutine(actionTimerCoroutine);
            actionTimerCoroutine = null;
        }

        if (!playerAction.HideUnderDesk())
        {
            Debug.LogError("[AÇÃO] Não foi possível esconder Ícaro.");
            currentState = OfficeState.WaitingForDoor;
            return false;
        }

        if (deskHover != null)
            deskHover.SetHoverEnabled(false);

        if (doorHover != null)
            doorHover.SetHoverEnabled(false);

        currentState = OfficeState.Hiding;

        Debug.Log("[AÇÃO] Ícaro começou a se esconder debaixo da mesa.");

        hideCoroutine = StartCoroutine(HideSequence());

        return true;
    }

    private IEnumerator HideSequence()
    {
        Debug.Log($"[ESCONDERIJO] Aguardando {keyDelay} segundos para o som das chaves.");

        yield return new WaitForSecondsRealtime(keyDelay);

        hideCoroutine = null;

        if (audioSource != null && keySound != null)
        {
            audioSource.PlayOneShot(keySound);
            Debug.Log("[ÁUDIO] Som das chaves reproduzido.");
        }

        currentState = OfficeState.GuardSequence;

        Debug.Log("[SEGURANÇA] Iniciando sequência de investigação.");

        bool started = securityGuard.BeginSequence(OnGuardFinished);

        if (!started)
        {
            Debug.LogError("[SEGURANÇA] Não foi possível iniciar a sequência. Recuperando Ícaro.");
            RecoverFromGuardFailure();
        }
    }


    private void OnGuardFinished()
    {
       deskHover.SetHoverEnabled(false);
    doorHover.SetHoverEnabled(true);

        Debug.Log("[SEGURANÇA] Investigação concluída.");

        playerAction.EmergeFromDesk();

        Debug.Log("[ICARO] Ícaro saiu debaixo da mesa.");

        StartCoroutine(ShowIcaroSpeech());

        currentState = OfficeState.WaitingForDoor;
    }


    private IEnumerator ShowIcaroSpeech()
    {
        if (securityGuard == null)
        {
            Debug.LogError("[BALÃO] SecurityGuardController não encontrado.");
            yield break;
        }

        Debug.Log("[BALÃO] Exibindo fala de Ícaro.");

        securityGuard.ShowSpeechFor(
            icaroName,
            icaroDialogue
        );

        yield return new WaitForSecondsRealtime(icaroSpeechDuration);

        securityGuard.HideSpeech();

        Debug.Log("[BALÃO] Fala de Ícaro encerrada.");
    }



    private void RecoverFromGuardFailure()
    {
        if (playerAction != null)
            playerAction.EmergeFromDesk();

        if (deskHover != null)
            deskHover.SetHoverEnabled(false);

        if (doorHover != null)
            doorHover.SetHoverEnabled(true);

        currentState = OfficeState.WaitingForDoor;
    }


    public bool TryUseDoor()
    {
        deskHover.SetHoverEnabled(false);
        doorHover.SetHoverEnabled(false);
       
        if (currentState != OfficeState.WaitingForAction &&
            currentState != OfficeState.WaitingForDoor)
        {
            Debug.LogWarning("[PORTA] A porta não pode ser utilizada neste momento.");
            return false;
        }

        if (playerAction == null)
        {
            Debug.LogError("[PORTA] OfficePlayerAction não foi atribuído.");
            return false;
        }

        if (actionTimerCoroutine != null)
        {
            StopCoroutine(actionTimerCoroutine);
            actionTimerCoroutine = null;
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (deskHover != null)
            deskHover.SetHoverEnabled(false);

        if (doorHover != null)
            doorHover.SetHoverEnabled(false);

        currentState = OfficeState.WalkingToDoor;

        Debug.Log("[AÇÃO] Caminhada de Ícaro até a porta iniciada.");

        bool started = playerAction.WalkToDoor(OnPlayerReachedDoor);

        if (!started)
        {
            Debug.LogError("[PORTA] Não foi possível iniciar a caminhada.");
            currentState = OfficeState.WaitingForDoor;
            return false;
        }

        return true;
    }

    private void OnPlayerReachedDoor()
    {
        if (currentState != OfficeState.WalkingToDoor)
            return;

        currentState = OfficeState.Final;

        Debug.Log("[DEMO] Ícaro chegou à porta. Encerrando a demo.");

        if (DemoEndController.Instance != null)
        {
            DemoEndController.Instance.EndDemo();
        }
        else
        {
            Debug.LogError("[DEMO] DemoEndController não encontrado.");
        }
    }

    private void OnDestroy()
    {
        if (handleCoroutine != null)
            StopCoroutine(handleCoroutine);

        if (actionTimerCoroutine != null)
            StopCoroutine(actionTimerCoroutine);

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        if (Instance == this)
            Instance = null;
    }
}
