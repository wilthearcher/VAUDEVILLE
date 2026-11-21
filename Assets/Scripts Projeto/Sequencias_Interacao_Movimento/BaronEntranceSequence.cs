
using UnityEngine;

public class BaronEntranceSequence : MonoBehaviour
{
    [Header("IDs dos NPCs que precisam ser concluídos")]
    [SerializeField] private string madameNPCID;
    [SerializeField] private string croupierNPCID;

    [Header("Personagens")]
    [SerializeField] private NPCFollower receptionist;
    [SerializeField] private NPCFollower baron;

    [Header("Posições")]
    [SerializeField] private Transform receptionistLeadPosition;
    [SerializeField] private Transform receptionistExitPosition;
    [SerializeField] private Transform baronFinalPosition;

    [Header("Interação do Barão")]
    [SerializeField] private NPCInteraction baronInteraction;

    private SpriteRenderer[] receptionistRenderers;
    private SpriteRenderer[] baronRenderers;

    private bool sequenceStarted;
    private bool baronArrived;
    private bool receptionistArrivedAhead;
    private bool receptionistLeaving;
    private bool sequenceFinished;

    private void Awake()
    {
        // Obtém os SpriteRenderers dos personagens.
        if (receptionist != null)
        {
            receptionistRenderers =
                receptionist.GetComponentsInChildren<SpriteRenderer>(true);

            SetVisibility(receptionistRenderers, false);
        }

        if (baron != null)
        {
            baronRenderers =
                baron.GetComponentsInChildren<SpriteRenderer>(true);

            SetVisibility(baronRenderers, false);
        }

        // Impede a interação antes da sequência.
        if (baronInteraction != null)
            baronInteraction.enabled = false;
    }

    private void Start()
    {
        if (InteractionTracker.Instance == null ||
            string.IsNullOrWhiteSpace(madameNPCID) ||
            string.IsNullOrWhiteSpace(croupierNPCID) ||
            receptionist == null ||
            baron == null ||
            receptionistLeadPosition == null ||
            receptionistExitPosition == null ||
            baronFinalPosition == null ||
            baronInteraction == null)
        {
            Debug.LogError(
                "Verifique as referências do BaronEntranceSequence!",
                this
            );

            enabled = false;
            return;
        }

        receptionist.OnDestinationReached += HandleReceptionistArrival;
        baron.OnDestinationReached += HandleBaronArrival;

        Debug.Log("Aguardando as interações com Madame e Croupier...");
    }

    private void Update()
    {
        if (sequenceStarted || sequenceFinished)
            return;

        if (InteractionTracker.Instance == null)
            return;

        bool madameCompleted =
            InteractionTracker.Instance.HasInteracted(madameNPCID);

        bool croupierCompleted =
            InteractionTracker.Instance.HasInteracted(croupierNPCID);

        if (madameCompleted && croupierCompleted)
        {
            StartSequence();
        }
    }

    private void OnDestroy()
    {
        if (receptionist != null)
            receptionist.OnDestinationReached -= HandleReceptionistArrival;

        if (baron != null)
            baron.OnDestinationReached -= HandleBaronArrival;
    }

    private void StartSequence()
    {
        if (sequenceStarted)
            return;

        sequenceStarted = true;

        Debug.Log("Madame e Croupier concluídos. Iniciando sequência!");

        // Mostra os dois personagens.
        SetVisibility(receptionistRenderers, true);
        SetVisibility(baronRenderers, true);

        // Os dois começam a caminhar simultaneamente.
        receptionist.SetDestinationAndStart(
            receptionistLeadPosition
        );

        baron.SetDestinationAndStart(
            baronFinalPosition
        );
    }

    private void HandleBaronArrival()
    {
        if (sequenceFinished)
            return;

        baronArrived = true;

        Debug.Log("Barão chegou à posição final.");

        TryStartReceptionistExit();
    }

    private void HandleReceptionistArrival()
    {
        if (sequenceFinished)
            return;

        // Primeira chegada: ponto à frente do Barão.
        if (!receptionistLeaving)
        {
            receptionistArrivedAhead = true;

            Debug.Log("Recepcionista chegou ao ponto de passagem.");

            TryStartReceptionistExit();
            return;
        }

        // Segunda chegada: porta de saída.
        FinishSequence();
    }

    private void TryStartReceptionistExit()
    {
        if (!baronArrived ||
            !receptionistArrivedAhead ||
            receptionistLeaving)
            return;

        receptionistLeaving = true;

        Debug.Log("Recepcionista está saindo do salão.");

        receptionist.SetDestinationAndStart(
            receptionistExitPosition
        );
    }

    private void FinishSequence()
    {
        if (sequenceFinished)
            return;

        sequenceFinished = true;

        // Recepcionista desaparece ao sair.
        SetVisibility(receptionistRenderers, false);

        // Barão fica disponível para interação.
        baronInteraction.enabled = true;

        Debug.Log("Sequência concluída! Barão disponível para interação.");
    }

    private void SetVisibility(
        SpriteRenderer[] renderers,
        bool visible)
    {
        if (renderers == null)
            return;

        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer != null)
                renderer.enabled = visible;
        }
    }
}
