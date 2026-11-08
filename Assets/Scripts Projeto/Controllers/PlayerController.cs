using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [Header("Movimentação")] //velocidade de movimentação do jogador, pode ser alterada mas é uma informação privada
    [SerializeField] private float moveSpeed = 5f;

    [Header("Cenário")] // algo importante 
    [SerializeField] private SpriteRenderer cenario; 

    [Header("Limites do Cenário")] // limites do cenario, nas fases irão existir esses limites e eles são objetos, o codigo vai interpretar a posição deles
    [SerializeField] private Transform leftLimit;
    [SerializeField] private Transform rightLimit;

    [Header("Animação")] // nossa animação e sprites
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Diálogo")]
    [SerializeField] private DialogueManager dialogueManager;

    [SerializeField] private float minX;
    [SerializeField] private float maxX;
    [SerializeField] private float margemLimite = 0.5f;
    

    private float targetX;
    private Transform playerTransform;
    private Camera mainCamera;
    private float cameraDistance;
    private bool isDialogueActive;

    private void Awake()
{
    playerTransform = transform;

    mainCamera = Camera.main;

    minX = leftLimit.position.x + margemLimite;
    maxX = rightLimit.position.x - margemLimite;

    if (leftLimit == null || rightLimit == null){
    
        Debug.LogError("leftLimit ou rightLimit não atribuídos!");
        enabled = false;
        return;
    }

    if (animator == null)
        animator = GetComponent<Animator>();

    if (spriteRenderer == null)
        spriteRenderer = GetComponent<SpriteRenderer>();

    if (leftLimit == null || rightLimit == null){
        Debug.LogError("leftLimit ou rightLimit não foram atribuídos!");
        enabled = false;
        return;
    }

    targetX = Mathf.Clamp(playerTransform.position.x,minX,maxX);

    if (mainCamera == null || animator == null || spriteRenderer == null || dialogueManager == null){
        
        Debug.LogError("Verifique as referências do PlayerController!");
        enabled = false;
        return;
    }

    cameraDistance = Mathf.Abs(mainCamera.transform.position.z - playerTransform.position.z);
    isDialogueActive = dialogueManager.IsDialogueOpen;
}

    private void OnEnable(){
        if (dialogueManager != null){
            dialogueManager.OnDialogueStateChanged += HandleDialogueState;
        }
    }

    private void OnDisable(){
        if (dialogueManager != null){
            dialogueManager.OnDialogueStateChanged -= HandleDialogueState;
        }
    }

    private void Update(){
        // Não permite movimentação durante o diálogo.
        if (isDialogueActive){
            animator.SetBool("isWalking", false);
            return;
        }

        // Detecta o clique do mouse.
        if (Input.GetMouseButtonDown(0)){
            // Evita movimentação ao clicar na interface.
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject()){
                return;
            }

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = cameraDistance;

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

        Collider2D hit = Physics2D.OverlapPoint(new Vector2(worldPosition.x, worldPosition.y));

        if (hit == null){
        Debug.Log("Nenhum Collider2D encontrado.");
        }
        else{
            Debug.Log("Collider encontrado: " + hit.name);

            NPCInteraction npc = hit.GetComponentInParent<NPCInteraction>();

            if (npc != null){
                Debug.Log("NPC encontrado: " + npc.name);
                npc.Interact(dialogueManager);
                return;
            }
        }
                // Se não clicou em um NPC,
                // movimenta o jogador até o destino.
                targetX = Mathf.Clamp(
                    worldPosition.x,
                    minX,
                    maxX
                );
            }

            MovePlayer();

            UpdateAnimation();
        }

    private void HandleDialogueState(bool dialogueActive){
        isDialogueActive = dialogueActive;
        if (dialogueActive){
            // Interrompe qualquer movimento em andamento.
            targetX = playerTransform.position.x;

            animator.SetBool("isWalking", false);
        }
    }

    private void MovePlayer(){
        Vector3 currentPosition = playerTransform.position;
        float newX = Mathf.MoveTowards(currentPosition.x,targetX,moveSpeed * Time.deltaTime);

        newX = Mathf.Clamp(newX, minX, maxX);

        playerTransform.position = new Vector3(newX,currentPosition.y,currentPosition.z);
    }

    private void UpdateAnimation()
    {
        float direction = targetX - playerTransform.position.x;

        bool isMoving = !Mathf.Approximately(direction, 0f);

        animator.SetBool("isWalking", isMoving);

        if (direction < 0){
            spriteRenderer.flipX = false;
        }
        else if (direction > 0){
            spriteRenderer.flipX = true;
        }
    }
}
