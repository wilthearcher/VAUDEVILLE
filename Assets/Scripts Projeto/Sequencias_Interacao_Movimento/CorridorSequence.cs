using UnityEngine;

public class CorridorSequence : MonoBehaviour
{
    [Header("Personagens")]
    [SerializeField] private NPCFollower receptionist;
    [SerializeField] private PlayerController playerController;

    [Header("Posição final de Ícaro")]
    [SerializeField] private Transform playerFinalPosition;

    [Header("Porta")]
    [SerializeField] private DoorInteraction door;

    [Header("Configuração")]
    [SerializeField] private bool startAutomatically = true;

    private bool sequenceStarted;
    private bool sequenceFinished;

    private void Start()
    {
        if (receptionist == null ||
            playerController == null ||
            playerFinalPosition == null ||
            door == null)
        {
            Debug.LogError(
                "Verifique as referências do CorridorSequence!"
            );

            enabled = false;
            return;
        }

        receptionist.OnDestinationReached +=
            HandleReceptionistArrival;

        playerController.OnSequencePositionReached +=
            HandlePlayerArrival;

        door.SetInteractionEnabled(false);

        if (startAutomatically)
            StartSequence();
    }

    private void OnDestroy()
    {
        if (receptionist != null)
        {
            receptionist.OnDestinationReached -=
                HandleReceptionistArrival;
        }

        if (playerController != null)
        {
            playerController.OnSequencePositionReached -=
                HandlePlayerArrival;
        }
    }

    public void StartSequence()
    {
        if (sequenceStarted)
            return;

        sequenceStarted = true;

        receptionist.StartFollowing();

        Debug.Log("O Recepcionista iniciou o percurso!");
    }

    private void HandleReceptionistArrival()
    {
        if (sequenceFinished)
            return;

        Debug.Log("O Recepcionista chegou à porta!");

        playerController.StopAndMoveTo(
            playerFinalPosition.position
        );
    }

    private void HandlePlayerArrival()
    {
        if (sequenceFinished)
            return;

        sequenceFinished = true;

        Debug.Log("Ícaro chegou à porta!");

        door.SetInteractionEnabled(true);

        Debug.Log("A porta agora pode ser utilizada!");
    }
}
