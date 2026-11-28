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
    [SerializeField] private float actionTimeLimit = 10f;
    [SerializeField] private float keyDelay = 2f;

    [Header("Referências")]
    [SerializeField] private OfficePlayerAction playerAction;
    [SerializeField] private SecurityGuardController securityGuard;
    [SerializeField] private DocumentUI documentUI;

    [Header("Fala de Ícaro / Jogador")]
    [SerializeField] private string icaroName = "Ícaro";

    [TextArea(2, 4)]
    [SerializeField] private string icaroDialogue = "Ufa... Acho que ele já foi.";

    [SerializeField] private float icaroSpeechDuration = 2.5f;

    [Header("Efeitos de Hover")]
    [SerializeField] private NPCHoverEffect deskHover;
    [SerializeField] private NPCHoverEffect doorHover;

    private OfficeState currentState = OfficeState.WaitingForDocument;
    private Coroutine actionTimerCoroutine;
    private Coroutine hideCoroutine;
    private Coroutine icaroSpeechCoroutine;

    private bool secondPageActivated;
    private bool documentsClosed;
    private bool hasHiddenSuccessfully;
    private bool caughtEndingStarted;

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
        Debug.LogWarning("[ESCRITÓRIO] Segunda página já ativada.");
        return;
    }

    secondPageActivated = true;

    Debug.Log("[ESCRITÓRIO] Segunda página ativada.");

    if (doorHover != null)
        doorHover.SetHoverEnabled(true);

    if (deskHover != null)
        deskHover.SetHoverEnabled(false);
}

    // CHAMAR QUANDO O JOGADOR FECHAR OS DOCUMENTOS.
    public void OnDocumentClosed()
    {
        if (!secondPageActivated)
        {
            Debug.LogWarning("[DOCUMENTOS] A segunda página ainda não foi ativada.");
            return;
        }

        if (documentsClosed)
        {
            Debug.LogWarning("[DOCUMENTOS] Os documentos já foram fechados.");
            return;
        }

        if (currentState == OfficeState.Final)
            return;

        documentsClosed = true;
        InspectionsLocked = true;

        if (deskHover != null)
            deskHover.SetHoverEnabled(true);

        if (doorHover != null)
            doorHover.SetHoverEnabled(true);

        Debug.Log("[HOVER] Mesa e porta liberadas após fechar os documentos.");
        Debug.Log("[DOCUMENTOS] Documentos fechados.");

        if (audioSource != null && doorKnockSound != null)
        {
            audioSource.PlayOneShot(doorKnockSound);
            Debug.Log("[ÁUDIO] Batida na porta reproduzida.");
        }

        currentState = OfficeState.WaitingForAction;

        Debug.Log("[TIMER 10s] Iniciado após o fechamento dos documentos.");

        actionTimerCoroutine = StartCoroutine(ActionTimerRoutine());
    }

    // PRAZO DE 10 SEGUNDOS PARA ESCOLHER ENTRE A MESA E A PORTA.
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

    if (currentState != OfficeState.WaitingForAction)
        yield break;

    Debug.Log("[TIMER 10s] Tempo esgotado. Ícaro será capturado.");

    if (audioSource != null && handleSound != null)
        audioSource.PlayOneShot(handleSound);

    StartCaughtEnding();
}

    // INTERAÇÃO COM A MESA.
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

    // Primeiro verifica se Ícaro consegue se esconder.
    if (!playerAction.HideUnderDesk())
    {
        Debug.LogError("[AÇÃO] Não foi possível esconder Ícaro.");
        return false;
    }

    // Cancela os 10 segundos somente após esconder-se com sucesso.
    if (actionTimerCoroutine != null)
    {
        StopCoroutine(actionTimerCoroutine);
        actionTimerCoroutine = null;
    }

    hasHiddenSuccessfully = true;

    if (deskHover != null)
        deskHover.SetHoverEnabled(false);

    if (doorHover != null)
        doorHover.SetHoverEnabled(false);

    currentState = OfficeState.Hiding;

    Debug.Log("[AÇÃO] Ícaro começou a se esconder debaixo da mesa.");
    Debug.Log("[TIMER 10s] Cancelado: Ícaro conseguiu se esconder.");

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
            Debug.LogError("[SEGURANÇA] Não foi possível iniciar a investigação. Recuperando Ícaro.");
            RecoverFromGuardFailure();
        }
    }

    // CHAMADO QUANDO O SEGURANÇA TERMINA A INVESTIGAÇÃO NORMAL.
    private void OnGuardFinished()
    {
        if (currentState == OfficeState.Final)
            return;

        if (deskHover != null)
            deskHover.SetHoverEnabled(false);

        if (doorHover != null)
            doorHover.SetHoverEnabled(true);

        Debug.Log("[SEGURANÇA] Investigação concluída.");

        playerAction.EmergeFromDesk();

        Debug.Log("[ICARO] Ícaro saiu debaixo da mesa.");

        currentState = OfficeState.WaitingForDoor;

        if (icaroSpeechCoroutine != null)
            StopCoroutine(icaroSpeechCoroutine);

        icaroSpeechCoroutine = StartCoroutine(ShowIcaroSpeech());
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

        // Agora que a fala terminou, o segurança pode ser desativado.
        securityGuard.gameObject.SetActive(false);

        icaroSpeechCoroutine = null;
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

    // INTERAÇÃO COM A PORTA.
    public bool TryUseDoor()
    {
        if (!secondPageActivated)
        {
            Debug.LogWarning("[PORTA] A porta ainda não pode ser utilizada.");
            return false;
        }
        // A porta pode ser usada tanto antes quanto depois de fechar os documentos.
        if (currentState != OfficeState.WaitingForDocument &&
            currentState != OfficeState.WaitingForAction &&
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

        // Cancela os temporizadores: o jogador escolheu sair.

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

        if (icaroSpeechCoroutine != null)
        {
            StopCoroutine(icaroSpeechCoroutine);
            icaroSpeechCoroutine = null;

            if (securityGuard != null)
            {
                securityGuard.HideSpeech();
                securityGuard.gameObject.SetActive(false);
            }
        }

        if (deskHover != null)
            deskHover.SetHoverEnabled(false);

        if (doorHover != null)
            doorHover.SetHoverEnabled(false);

        currentState = OfficeState.WalkingToDoor;

        Debug.Log("[TIMER 30s] Cancelado: Ícaro escolheu abrir a porta.");
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

    // FINAL DE FUGA PELA PORTA.
    private void OnPlayerReachedDoor()
    {
        if (currentState != OfficeState.WalkingToDoor)
            return;

        currentState = OfficeState.Final;

        Debug.Log("[DEMO] Ícaro chegou à porta. Encerrando a demo.");

        if (DemoEndController.Instance == null)
        {
            Debug.LogError("[DEMO] DemoEndController não encontrado.");
            return;
        }

        if (hasHiddenSuccessfully)
        {
            DemoEndController.Instance.EndHiddenAndEscaped();
        }
        else
        {
            DemoEndController.Instance.EndDoorEscape();
        }
    }

    // FINAL DE CAPTURA: O JOGADOR NÃO SE ESCONDEU NEM SAIU A TEMPO.
    private void StartCaughtEnding()
{
    if (caughtEndingStarted || currentState == OfficeState.Final)
        return;

    caughtEndingStarted = true;
    currentState = OfficeState.Final;
    InspectionsLocked = true;

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

    Debug.Log("[SEGURANÇA] O segurança está entrando para procurar Ícaro.");

    if (securityGuard == null)
    {
        Debug.LogError("[SEGURANÇA] SecurityGuardController não foi atribuído.");
        FinishCaughtEnding();
        return;
    }

// Reativa o segurança antes de iniciar a sequência.
if (!securityGuard.gameObject.activeSelf)
{
    securityGuard.gameObject.SetActive(true);
}

// Inicia a sequência de captura.
    securityGuard.BeginCatchSequence(FinishCaughtEnding);
}

    private void FinishCaughtEnding()
    {
        if (DemoEndController.Instance == null)
        {
            Debug.LogError("[DEMO] DemoEndController não encontrado.");
            return;
        }

        Debug.Log("[DEMO] Ícaro foi encontrado pelo segurança.");

        DemoEndController.Instance.EndCaught();
    }

    private void OnDestroy()
    {

        if (actionTimerCoroutine != null)
            StopCoroutine(actionTimerCoroutine);

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        if (icaroSpeechCoroutine != null)
            StopCoroutine(icaroSpeechCoroutine);

        if (Instance == this)
            Instance = null;
    }
}