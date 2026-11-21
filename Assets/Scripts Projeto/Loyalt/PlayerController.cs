using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [Header("Movimentação")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Limites do Cenário")]
    [SerializeField] private Transform leftLimit;
    [SerializeField] private Transform rightLimit;
    [SerializeField] private float margemLimite = 0.5f;

    [Header("Animação")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Diálogo")]
    [SerializeField] private DialogueManager dialogueManager;

    private float minX;
    private float maxX;
    private float targetX;

    private Transform playerTransform;
    private Camera mainCamera;

    private bool isDialogueActive;

    private bool isMovingToSequencePosition;
    private bool isMovementLocked;

    private bool externalMovementLocked;
    private bool isSpecialMovement;
    private Action specialMovementCallback;

    public event Action OnSequencePositionReached;

    private void Awake()
    {
        playerTransform = transform;
        mainCamera = Camera.main;

        if (leftLimit == null || rightLimit == null)
        {
            Debug.LogError("LeftLimit ou RightLimit não foram atribuídos!");
            enabled = false;
            return;
        }

        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (mainCamera == null ||
            animator == null ||
            spriteRenderer == null ||
            dialogueManager == null)
        {
            Debug.LogError("Verifique as referências do PlayerController!");
            enabled = false;
            return;
        }

        minX = leftLimit.position.x + margemLimite;
        maxX = rightLimit.position.x - margemLimite;

        if (minX > maxX)
        {
            Debug.LogError("Os limites do cenário estão invertidos ou muito próximos!");
            enabled = false;
            return;
        }

        targetX = Mathf.Clamp(
            playerTransform.position.x,
            minX,
            maxX
        );

        isDialogueActive = dialogueManager.IsDialogueOpen;
    }

    private void OnEnable()
    {
        if (dialogueManager != null)
            dialogueManager.OnDialogueStateChanged += HandleDialogueState;
    }

    private void OnDisable()
    {
        if (dialogueManager != null)
            dialogueManager.OnDialogueStateChanged -= HandleDialogueState;
    }

    private void Update()
    {
        if (isDialogueActive)
        {
            animator.SetBool("isWalking", false);
            return;
        }

        if (isMovingToSequencePosition)
        {
            MovePlayer();
            UpdateAnimation();

            if (Mathf.Approximately(playerTransform.position.x, targetX))
            {
                isMovingToSequencePosition = false;
                isMovementLocked = false;

                animator.SetBool("isWalking", false);

                if (isSpecialMovement)
                {
                    isSpecialMovement = false;

                    Action callback = specialMovementCallback;
                    specialMovementCallback = null;

                    callback?.Invoke();
                }
                else
                {
                    OnSequencePositionReached?.Invoke();
                }
            }

            return;
        }

        if (externalMovementLocked)
        {
            animator.SetBool("isWalking", false);
            return;
        }

        if (isMovementLocked)
        {
            animator.SetBool("isWalking", false);
            return;
        }

        DetectMouseClick();

        MovePlayer();

        UpdateAnimation();
    }

    private void DetectMouseClick()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        Vector3 clickPosition = mainCamera.ScreenToWorldPoint(
            Input.mousePosition
        );

        clickPosition.z = 0f;

        Collider2D[] hits = Physics2D.OverlapPointAll(clickPosition);

        foreach (Collider2D hit in hits)
        {
            // Objetos especiais: mesa e porta.
            OfficeActionObject officeAction =
    hit.GetComponent<OfficeActionObject>();

if (officeAction == null)
{
    officeAction = hit.GetComponentInParent<OfficeActionObject>();
}

if (officeAction != null)
{
    officeAction.Interact();
    return;
}
            
            // NPCs interativos.
            NPCInteraction npc =
                hit.GetComponentInParent<NPCInteraction>();

            if (npc != null)
            {
                npc.Interact(dialogueManager);
                return;
            }
        }

        // Clique no cenário: movimentação normal.
        targetX = Mathf.Clamp(
            clickPosition.x,
            minX,
            maxX
        );
    }

    private void HandleDialogueState(bool dialogueActive)
    {
        isDialogueActive = dialogueActive;

        if (dialogueActive)
        {
            targetX = playerTransform.position.x;
            animator.SetBool("isWalking", false);
        }
    }

    private void MovePlayer()
    {
        Vector3 currentPosition = playerTransform.position;

        float newX = Mathf.MoveTowards(
            currentPosition.x,
            targetX,
            moveSpeed * Time.deltaTime
        );

        newX = Mathf.Clamp(newX, minX, maxX);

        playerTransform.position = new Vector3(
            newX,
            currentPosition.y,
            currentPosition.z
        );
    }

    private void UpdateAnimation()
    {
        float direction = targetX - playerTransform.position.x;

        bool isMoving = !Mathf.Approximately(direction, 0f);

        animator.SetBool("isWalking", isMoving);

        if (direction < 0f)
            spriteRenderer.flipX = true;
        else if (direction > 0f)
            spriteRenderer.flipX = false;
    }

    public void StopAndMoveTo(Vector3 destination)
    {
        specialMovementCallback = null;
        isSpecialMovement = false;

        isMovementLocked = false;
        isMovingToSequencePosition = true;

        targetX = Mathf.Clamp(
            destination.x,
            minX,
            maxX
        );
    }

    public void UnlockMovement()
    {
        isMovingToSequencePosition = false;
        isMovementLocked = false;

        specialMovementCallback = null;
        isSpecialMovement = false;

        animator.SetBool("isWalking", false);

        OnSequencePositionReached?.Invoke();
    }

    public void SetExternalMovementLocked(bool locked)
    {
        externalMovementLocked = locked;

        if (locked)
        {
            isMovingToSequencePosition = false;

            specialMovementCallback = null;
            isSpecialMovement = false;

            targetX = playerTransform.position.x;

            animator.SetBool("isWalking", false);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }

    public void MoveToSpecialPosition(
        Vector3 destination,
        Action onReached)
    {
        specialMovementCallback = onReached;
        isSpecialMovement = true;

        externalMovementLocked = false;
        isMovementLocked = false;
        isMovingToSequencePosition = true;

        targetX = Mathf.Clamp(
            destination.x,
            minX,
            maxX
        );
    }
}
