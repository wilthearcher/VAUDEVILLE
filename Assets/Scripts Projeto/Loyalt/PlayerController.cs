
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

    private bool cutsceneMovementLocked;
    private float targetXBeforeCutscene;

    // Temporizador dos logs de diagnóstico.
    private float debugMovementTimer;

    public event Action OnSequencePositionReached;

    private void Awake()
    {
        playerTransform = transform;
        mainCamera = Camera.main;

        if (leftLimit == null || rightLimit == null)
        {
            Debug.LogError("[PLAYER] LeftLimit ou RightLimit não atribuídos!");
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
            Debug.LogError("[PLAYER] Verifique as referências do PlayerController!");
            enabled = false;
            return;
        }

        if (moveSpeed <= 0f)
        {
            Debug.LogError("[PLAYER] MoveSpeed precisa ser maior que zero!");
            enabled = false;
            return;
        }

        minX = leftLimit.position.x + margemLimite;
        maxX = rightLimit.position.x - margemLimite;

        if (minX > maxX)
        {
            Debug.LogError("[PLAYER] Os limites do cenário estão invertidos!");
            enabled = false;
            return;
        }

        targetX = Mathf.Clamp(
            playerTransform.position.x,
            minX,
            maxX
        );

        isDialogueActive = dialogueManager.IsDialogueOpen;

        Debug.Log("[PLAYER] Limites X: " + minX + " até " + maxX);
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
        // Movimento especial tem prioridade sobre os bloqueios.

        if (isMovingToSequencePosition)
        {
            UpdateSequenceMovement();
            return;
        }

        if (isDialogueActive)
        {
            animator.SetBool("isWalking", false);
            return;
        }

        if (cutsceneMovementLocked)
        {
            animator.SetBool("isWalking", false);
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

    // =====================================================
    // MOVIMENTAÇÃO ESPECIAL
    // =====================================================

    private void UpdateSequenceMovement()
    {
        MovePlayer();
        UpdateAnimation();

        debugMovementTimer += Time.deltaTime;

        // Exibe a posição e a distância a cada meio segundo.
        if (debugMovementTimer >= 0.5f)
        {
            debugMovementTimer = 0f;

            float currentX = playerTransform.position.x;
            float distance = Mathf.Abs(currentX - targetX);

            Debug.Log(
                "[PLAYER DEBUG] Atual: " + currentX +
                " | Destino: " + targetX +
                " | Distância: " + distance
            );
        }

        float remainingDistance = Mathf.Abs(
            playerTransform.position.x - targetX
        );

        if (remainingDistance > 0.02f)
            return;

        // Garante que Ícaro termine exatamente no destino.
        Vector3 position = playerTransform.position;
        position.x = targetX;
        playerTransform.position = position;

        isMovingToSequencePosition = false;
        isMovementLocked = false;

        animator.SetBool("isWalking", false);

        Debug.Log("[PLAYER] Chegou à posição especial!");

        if (isSpecialMovement)
        {
            isSpecialMovement = false;

            Action callback = specialMovementCallback;
            specialMovementCallback = null;

            Debug.Log(
                "[PLAYER] Callback encontrado: " + (callback != null)
            );

            if (callback != null)
            {
                Debug.Log("[PLAYER] EXECUTANDO CALLBACK AGORA!");

                callback.Invoke();
            }
            else
            {
                Debug.LogError("[PLAYER] CALLBACK ESTÁ NULO!");
            }
        }
        else
        {
            Debug.Log("[PLAYER] Executando OnSequencePositionReached.");

            OnSequencePositionReached?.Invoke();
        }
    }

    // =====================================================
    // DETECÇÃO DE CLIQUES
    // =====================================================

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
            // PORTAS
            DoorInteraction door =
                hit.GetComponent<DoorInteraction>();

            if (door == null)
                door = hit.GetComponentInParent<DoorInteraction>();

            if (door != null)
            {
                door.Interact();
                return;
            }

            // OBJETOS ESPECIAIS
            OfficeActionObject officeAction =
                hit.GetComponent<OfficeActionObject>();

            if (officeAction == null)
                officeAction = hit.GetComponentInParent<OfficeActionObject>();

            if (officeAction != null)
            {
                officeAction.Interact();
                return;
            }

            // NPCs
            NPCInteraction npc =
                hit.GetComponentInParent<NPCInteraction>();

            if (npc != null)
            {
                npc.Interact(dialogueManager);
                return;
            }
        }

        // CLIQUE NORMAL NO CENÁRIO.
        targetX = Mathf.Clamp(
            clickPosition.x,
            minX,
            maxX
        );
    }

    // =====================================================
    // DIÁLOGOS
    // =====================================================

    private void HandleDialogueState(bool dialogueActive)
    {
        isDialogueActive = dialogueActive;

        if (dialogueActive && !isMovingToSequencePosition)
        {
            targetX = playerTransform.position.x;
            animator.SetBool("isWalking", false);
        }
    }

    // =====================================================
    // MOVIMENTAÇÃO NORMAL
    // =====================================================

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

        bool isMoving = Mathf.Abs(direction) > 0.001f;

        animator.SetBool("isWalking", isMoving);

        if (direction < 0f)
            spriteRenderer.flipX = true;
        else if (direction > 0f)
            spriteRenderer.flipX = false;
    }

    // =====================================================
    // MÉTODOS UTILIZADOS POR OUTROS SCRIPTS
    // =====================================================

    public void StopAndMoveTo(Vector3 destination)
    {
        if (isSpecialMovement)
        {
            Debug.LogWarning(
                "[PLAYER] StopAndMoveTo ignorado: movimento especial em andamento."
            );
            return;
        }

        Debug.Log("[PLAYER] StopAndMoveTo foi chamado!");

        specialMovementCallback = null;
        isSpecialMovement = false;

        isMovementLocked = false;
        isMovingToSequencePosition = true;

        targetX = Mathf.Clamp(destination.x, minX, maxX);
    }

    public void UnlockMovement()
    {
        if (isSpecialMovement)
        {
            Debug.LogWarning(
                "[PLAYER] UnlockMovement ignorado: movimento especial em andamento."
            );
            return;
        }

        Debug.Log("[PLAYER] UnlockMovement foi chamado!");

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

        if (locked && !isMovingToSequencePosition)
        {
            targetX = playerTransform.position.x;
            animator.SetBool("isWalking", false);
        }
    }

    // =====================================================
    // MOVIMENTO ESPECIAL COM CALLBACK
    // =====================================================

    public void MoveToSpecialPosition(
        Vector3 destination,
        Action onReached)
    {
        if (isMovingToSequencePosition)
        {
            Debug.LogWarning(
                "[PLAYER] Já existe uma movimentação especial em andamento."
            );
            return;
        }

        specialMovementCallback = onReached;
        isSpecialMovement = true;

        externalMovementLocked = false;
        isMovementLocked = false;
        cutsceneMovementLocked = false;

        targetX = Mathf.Clamp(
            destination.x,
            minX,
            maxX
        );

        isMovingToSequencePosition = true;

        debugMovementTimer = 0f;

        Debug.Log("[PLAYER] Indo para X: " + targetX);
        Debug.Log("[PLAYER] Posição atual X: " + playerTransform.position.x);
        Debug.Log("[PLAYER] Callback recebido: " + (onReached != null));
    }

    // =====================================================
    // BLOQUEIO DE CUTSCENE
    // =====================================================

    public void SetCutsceneMovementLocked(bool locked)
    {
        if (cutsceneMovementLocked == locked)
            return;

        if (locked)
        {
            if (isMovingToSequencePosition)
            {
                Debug.LogWarning(
                    "[PLAYER] Bloqueio de cutscene ignorado durante movimento especial."
                );
                return;
            }

            targetXBeforeCutscene = targetX;
            targetX = playerTransform.position.x;

            cutsceneMovementLocked = true;

            animator.SetBool("isWalking", false);
        }
        else
        {
            cutsceneMovementLocked = false;

            targetX = targetXBeforeCutscene;

            animator.SetBool("isWalking", false);
        }
    }
}
