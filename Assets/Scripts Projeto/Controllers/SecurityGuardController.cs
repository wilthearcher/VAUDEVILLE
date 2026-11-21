
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class SecurityGuardController : MonoBehaviour
{
    [Header("Pontos de movimentação")]
    [SerializeField] private Transform entryPoint;
    [SerializeField] private Transform centerPoint;
    [SerializeField] private Transform exitPoint;

    [Header("Movimentação")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float arrivalDistance = 0.05f;

    [Header("Investigação")]
    [SerializeField] private float lookDuration = 1.5f;

    [Header("Informações do NPC")]
    [SerializeField] private string npcName = "Segurança";

    [Header("Animação")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private string walkingParameter = "isWalking";

    [Header("Balão de fala")]
    [SerializeField] private GameObject speechBubble;
    [SerializeField] private TMP_Text speechNameText;
    [SerializeField] private TMP_Text speechText;
    [SerializeField] private string dialogue = "Hmm... Não há ninguém aqui.";

    private Coroutine sequenceCoroutine;
    private Action onSequenceFinished;

    private bool isSequenceRunning;
    private bool hasFinished;

    private void Awake()
    {
        if (speechBubble != null)
            speechBubble.SetActive(false);
    }

    public bool BeginSequence(Action onFinished)
    {
        if (isSequenceRunning)
        {
            Debug.LogWarning("[SEGURANÇA] A sequência já está em andamento.");
            return false;
        }

        if (entryPoint == null || centerPoint == null || exitPoint == null)
        {
            Debug.LogError("[SEGURANÇA] Pontos de movimentação não atribuídos.");
            return false;
        }

        if (moveSpeed <= 0f)
        {
            Debug.LogError("[SEGURANÇA] moveSpeed precisa ser maior que zero.");
            return false;
        }

        onSequenceFinished = onFinished;
        isSequenceRunning = true;
        hasFinished = false;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        transform.position = entryPoint.position;

        Debug.Log("[SEGURANÇA] Ativado no EntryPoint.");

        sequenceCoroutine = StartCoroutine(GuardSequence());

        return true;
    }

    private IEnumerator GuardSequence()
    {
        Debug.Log("[SEGURANÇA] Caminhando até o ponto central.");

        yield return MoveTo(centerPoint);

        Debug.Log("[SEGURANÇA] Chegou ao ponto central.");

        yield return LookAround();

        ShowSpeechFor(npcName, dialogue);

        Debug.Log("[SEGURANÇA] Investigação: balão de fala ativado.");

        yield return new WaitForSecondsRealtime(2.5f);

        HideSpeech();

        Debug.Log("[SEGURANÇA] Balão de fala desativado.");

        Debug.Log("[SEGURANÇA] Caminhando até o ExitPoint.");

        yield return MoveTo(exitPoint);

        FinishSequence();
    }

    private IEnumerator MoveTo(Transform destination)
    {
        if (destination == null)
            yield break;

        float targetX = destination.position.x;

        while (Mathf.Abs(transform.position.x - targetX) > arrivalDistance)
        {
            float direction = Mathf.Sign(targetX - transform.position.x);

            if (spriteRenderer != null)
                spriteRenderer.flipX = direction < 0f;

            SetWalking(true);

            Vector3 position = transform.position;

            position.x = Mathf.MoveTowards(
                position.x,
                targetX,
                moveSpeed * Time.unscaledDeltaTime
            );

            transform.position = position;

            yield return null;
        }

        Vector3 finalPosition = transform.position;
        finalPosition.x = targetX;
        transform.position = finalPosition;

        SetWalking(false);
    }

    private IEnumerator LookAround()
    {
        SetWalking(false);

        float elapsed = 0f;

        while (elapsed < lookDuration)
        {
            if (spriteRenderer != null)
                spriteRenderer.flipX = !spriteRenderer.flipX;

            yield return new WaitForSecondsRealtime(0.5f);

            elapsed += 0.5f;
        }

        Debug.Log("[SEGURANÇA] Observação concluída.");
    }

    
public void ShowSpeechFor(string characterName, string message)
{
    if (speechBubble == null)
    {
        Debug.LogError("[BALÃO] SpeechBubble não atribuído.");
        return;
    }

    if (speechNameText != null)
    {
        speechNameText.text = characterName;
        speechNameText.gameObject.SetActive(true);
    }

    if (speechText != null)
    {
        speechText.text = message;
        speechText.gameObject.SetActive(true);
    }

    speechBubble.SetActive(true);

    Debug.Log("[BALÃO] Exibindo fala de " + characterName);
}


    public void HideSpeech()
{
    if (speechBubble != null)
        speechBubble.SetActive(false);
}

    private void SetWalking(bool walking)
    {
        if (animator != null)
            animator.SetBool(walkingParameter, walking);
    }

    private void FinishSequence()
    {
        if (hasFinished)
            return;

        hasFinished = true;
        isSequenceRunning = false;

        SetWalking(false);
        HideSpeech();

        Debug.Log("[SEGURANÇA] Saiu da sala. Sequência finalizada.");

        Action callback = onSequenceFinished;
        onSequenceFinished = null;

        sequenceCoroutine = null;

        callback?.Invoke();

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (isSequenceRunning && !hasFinished)
        {
            if (sequenceCoroutine != null)
                StopCoroutine(sequenceCoroutine);

            sequenceCoroutine = null;
            isSequenceRunning = false;
            onSequenceFinished = null;

            SetWalking(false);
            HideSpeech();

            Debug.LogWarning("[SEGURANÇA] Sequência interrompida.");
        }
    }
}
