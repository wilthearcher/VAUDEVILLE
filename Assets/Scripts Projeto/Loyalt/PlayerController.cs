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
    private float cameraDistance;

    private bool isDialogueActive;
    private void Awake()
    {
        playerTransform = transform;
        mainCamera = Camera.main;

        // Verifica os limites do cenário.
        if (leftLimit == null || rightLimit == null)
        {
            Debug.LogError(
                "LeftLimit ou RightLimit não foram atribuídos!"
            );

            enabled = false;
            return;
        }

        // Obtém os componentes necessários.
        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Verifica as referências.
        if (mainCamera == null ||
            animator == null ||
            spriteRenderer == null ||
            dialogueManager == null)
        {
            Debug.LogError(
                "Verifique as referências do PlayerController!"
            );

            enabled = false;
            return;
        }

        // Define os limites de movimentação.
        minX = leftLimit.position.x + margemLimite;
        maxX = rightLimit.position.x - margemLimite;

        if (minX > maxX)
        {
            Debug.LogError(
                "Os limites do cenário estão invertidos ou muito próximos!"
            );

            enabled = false;
            return;
        }

        // Define a posição inicial como destino.
        targetX = Mathf.Clamp(
            playerTransform.position.x,
            minX,
            maxX
        );

        // Calcula a distância entre a câmera e o jogador.
        cameraDistance = Mathf.Abs(
            mainCamera.transform.position.z -
            playerTransform.position.z
        );

        isDialogueActive = dialogueManager.IsDialogueOpen;
    }

    private void OnEnable()
    {
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueStateChanged +=
                HandleDialogueState;
        }
    }

    private void OnDisable()
    {
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueStateChanged -=
                HandleDialogueState;
        }
    }

    private void Update()
    {
        // Bloqueia a movimentação durante o diálogo.
        if (isDialogueActive)
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
        // Verifica se o botão esquerdo do mouse foi pressionado.
        if (!Input.GetMouseButtonDown(0))
            return;

        // Ignora cliques sobre a interface.
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // Converte a posição do mouse para o mundo.
        Vector3 mousePosition = Input.mousePosition;

        mousePosition.z = cameraDistance;

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(mousePosition);

        Vector2 clickPosition = new Vector2(
            worldPosition.x,
            worldPosition.y
        );

        // Procura todos os Colliders na posição clicada.
        Collider2D[] hits =
            Physics2D.OverlapPointAll(clickPosition);

        // Verifica se algum dos objetos encontrados é um NPC.
        foreach (Collider2D hit in hits)
        {
            NPCInteraction npc =
                hit.GetComponentInParent<NPCInteraction>();

            if (npc != null)
            {
                npc.Interact(dialogueManager);
                return;
            }
        }

        // Se não encontrou um NPC, movimenta o jogador.
        targetX = Mathf.Clamp(
            worldPosition.x,
            minX,
            maxX
        );
    }

    private void HandleDialogueState(bool dialogueActive)
    {
        isDialogueActive = dialogueActive;

        if (dialogueActive)
        {
            // Interrompe qualquer movimento em andamento.
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
        float direction =
            targetX - playerTransform.position.x;

        bool isMoving =
            !Mathf.Approximately(direction, 0f);

        animator.SetBool("isWalking", isMoving);

        // Ajusta a direção do personagem.
        if (direction < 0f)
        {
            spriteRenderer.flipX = true;
        }
        else if (direction > 0f)
        {
            spriteRenderer.flipX = false;
        }
    }
}
