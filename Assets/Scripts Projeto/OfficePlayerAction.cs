
using System;
using UnityEngine;

public class OfficePlayerAction : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform doorStandPoint;

    [Header("Sprites de Ícaro")]
    [SerializeField] private SpriteRenderer normalSprite;
    [SerializeField] private SpriteRenderer hiddenSprite;

    private void Start()
    {
        if (hiddenSprite != null)
            hiddenSprite.enabled = false;
    }

    public bool HideUnderDesk()
    {
        if (playerController == null ||
            normalSprite == null ||
            hiddenSprite == null)
        {
            Debug.LogError("[ESCONDERIJO] Referências de OfficePlayerAction incompletas.");
            return false;
        }

        playerController.SetExternalMovementLocked(true);

        normalSprite.enabled = false;
        hiddenSprite.enabled = true;

        Debug.Log("[ESCONDERIJO] Sprite de Ícaro escondido ativado.");

        return true;
    }

    public void EmergeFromDesk()
    {
        if (normalSprite == null || hiddenSprite == null)
        {
            Debug.LogError("[ESCONDERIJO] Sprites não atribuídos.");
            return;
        }

        hiddenSprite.enabled = false;
        normalSprite.enabled = true;

        if (playerController != null)
            playerController.SetExternalMovementLocked(false);

        Debug.Log("[ESCONDERIJO] Ícaro voltou à posição em pé.");
    }

    public bool WalkToDoor(Action onArrival)
    {
        if (playerController == null || doorStandPoint == null)
        {
            Debug.LogError("[PORTA] PlayerController ou DoorStandPoint não foi atribuído.");
            return false;
        }

        Debug.Log("[PORTA] Movendo Ícaro até DoorStandPoint.");

        playerController.MoveToSpecialPosition(
            doorStandPoint.position,
            onArrival
        );

        return true;
    }
}
