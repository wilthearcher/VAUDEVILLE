
using UnityEngine;

public class OfficeActionObject : MonoBehaviour
{
    public enum ActionType
    {
        Desk,
        Door
    }

    [Header("Tipo de interação")]
    [SerializeField] private ActionType actionType;

    private int lastInteractionFrame = -1;

    public void Interact()
    {
        // Evita que o mesmo clique seja processado duas vezes.
        if (lastInteractionFrame == Time.frameCount)
            return;

        lastInteractionFrame = Time.frameCount;

        if (OfficeEventController.Instance == null)
        {
            Debug.LogError("[CLIQUE] OfficeEventController não encontrado na cena.");
            return;
        }

        switch (actionType)
        {
            case ActionType.Desk:
                Debug.Log("[CLIQUE] Mesa detectada. Tentando iniciar a ação de esconder-se.");
                OfficeEventController.Instance.TryHide();
                break;

            case ActionType.Door:
                Debug.Log("[CLIQUE] Porta detectada. Tentando iniciar a ação de sair.");
                OfficeEventController.Instance.TryUseDoor();
                break;
        }
    }

    private void OnMouseDown()
    {
        Interact();
    }
}
