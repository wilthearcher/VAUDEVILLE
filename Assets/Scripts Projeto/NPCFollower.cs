using UnityEngine;
using System;

public class NPCFollower : MonoBehaviour
{
    [Header("Movimentação")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Transform destination;
    [SerializeField] private float arrivalDistance = 0.05f;

    [Header("Animação")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool isMoving;
    private bool hasReachedDestination;

    public event Action OnDestinationReached;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!isMoving || destination == null)
            return;

        MoveTowardsDestination();
    }

    public void StartFollowing()
    {
        if (destination == null)
        {
            Debug.LogError("Destino do NPC não foi configurado!");
            return;
        }

        hasReachedDestination = false;
        isMoving = true;

        animator.SetBool("isWalking", true);
    }

    public void StopFollowing()
    {
        isMoving = false;

        if (animator != null)
            animator.SetBool("isWalking", false);
    }

    private void MoveTowardsDestination()
    {
        Vector3 currentPosition = transform.position;

        float direction =
            destination.position.x - currentPosition.x;

        if (direction < 0f)
            spriteRenderer.flipX = true;
        else if (direction > 0f)
            spriteRenderer.flipX = false;

        float newX = Mathf.MoveTowards(
            currentPosition.x,
            destination.position.x,
            moveSpeed * Time.deltaTime
        );

        transform.position = new Vector3(
            newX,
            currentPosition.y,
            currentPosition.z
        );

        if (Mathf.Abs(destination.position.x - newX)
            <= arrivalDistance)
        {
            transform.position = new Vector3(
                destination.position.x,
                currentPosition.y,
                currentPosition.z
            );

            StopFollowing();

            if (!hasReachedDestination)
            {
                hasReachedDestination = true;
                OnDestinationReached?.Invoke();
            }
        }
    }
}
