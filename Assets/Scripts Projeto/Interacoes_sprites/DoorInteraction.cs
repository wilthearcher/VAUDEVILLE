
using UnityEngine;
using System;

public class DoorInteraction : MonoBehaviour
{
    [Header("Hover")]
    [SerializeField] private NPCHoverEffect doorHover;

    private bool canInteract;

    public event Action OnDoorClicked;

    public void SetInteractionEnabled(bool value)
    {
        canInteract = value;

        // Ativa ou desativa o hover junto com a interação.
        if (doorHover != null)
        {
            doorHover.SetHoverEnabled(value);
        }
        else
        {
            Debug.LogWarning("[DOOR] NPCHoverEffect não foi atribuído.");
        }
    }

    private void OnMouseDown()
    {
        if (!canInteract)
            return;

        Debug.Log("[DOOR] Porta selecionada!");

        // Impede interações repetidas.
        canInteract = false;

        if (doorHover != null)
        {
            doorHover.SetHoverEnabled(false);
        }

        OnDoorClicked?.Invoke();
    }

    private void OnDisable()
    {
        canInteract = false;

        if (doorHover != null)
        {
            doorHover.SetHoverEnabled(false);
        }
    }
}
