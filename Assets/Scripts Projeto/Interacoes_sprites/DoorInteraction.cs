
using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    [Header("Hover")]
    [SerializeField] private NPCHoverEffect doorHover;

    [Header("Transição")]
    [SerializeField] private PortaTransicao portaTransicao;

    private bool canInteract;

    private void Awake()
    {
        if (portaTransicao == null)
        {
            portaTransicao = GetComponent<PortaTransicao>();

            if (portaTransicao == null)
                portaTransicao = GetComponentInParent<PortaTransicao>();
        }

        // A porta começa disponível assim que a cena é carregada.
        canInteract = true;

        if (doorHover != null)
            doorHover.SetHoverEnabled(true);
    }

    public void SetInteractionEnabled(bool value)
    {
        canInteract = value;

        if (doorHover != null)
            doorHover.SetHoverEnabled(value);
    }

    public void Interact()
    {
        if (!canInteract)
            return;

        if (portaTransicao == null)
        {
            Debug.LogError("[DOOR] PortaTransicao não foi atribuída!");
            return;
        }

        Debug.Log("[DOOR] Porta selecionada!");

        canInteract = false;

        if (doorHover != null)
            doorHover.SetHoverEnabled(false);

        portaTransicao.IrParaScene();
    }

    private void OnDisable()
    {
        canInteract = false;

        if (doorHover != null)
            doorHover.SetHoverEnabled(false);
    }
}
