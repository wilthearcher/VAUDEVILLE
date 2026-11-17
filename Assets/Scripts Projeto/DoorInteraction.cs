using UnityEngine;
using System;

public class DoorInteraction : MonoBehaviour
{
    [Header("Cursor")]
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Vector2 cursorHotspot;

    private bool canInteract;
    private bool isMouseOver;

    public event Action OnDoorClicked;

    public void SetInteractionEnabled(bool value)
    {
        canInteract = value;

        if (!canInteract)
        {
            isMouseOver = false;
            Cursor.SetCursor(
                null,
                Vector2.zero,
                CursorMode.Auto
            );
        }
    }

    private void OnMouseEnter()
    {
        if (!canInteract)
            return;

        isMouseOver = true;

        if (cursorTexture != null)
        {
            Cursor.SetCursor(
                cursorTexture,
                cursorHotspot,
                CursorMode.Auto
            );
        }
    }

    private void OnMouseExit()
    {
        isMouseOver = false;

        Cursor.SetCursor(
            null,
            Vector2.zero,
            CursorMode.Auto
        );
    }

    private void OnMouseDown()
    {
        if (!canInteract)
            return;

        if (!isMouseOver)
            return;

        Debug.Log("Porta selecionada!");

        canInteract = false;

        Cursor.SetCursor(
            null,
            Vector2.zero,
            CursorMode.Auto
        );

        OnDoorClicked?.Invoke();
    }

    private void OnDisable()
    {
        Cursor.SetCursor(
            null,
            Vector2.zero,
            CursorMode.Auto
        );
    }
}
