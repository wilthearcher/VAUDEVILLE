
using System;
using System.Collections;
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

    [Header("Identificação")]
    [SerializeField] private string npcName = "Segurança";

    [Header("Animação")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private string walkingParameter = "isWalking";

    [Header("Balão de fala")]
    [SerializeField] private GameObject speechBubble;
    [SerializeField] private TMPro.TMP_Text speechNameText;
    [SerializeField] private TMPro.TMP_Text speechText;

    [Header("Posição da captura")]
    [SerializeField] private Transform caughtPosition;

    [TextArea(2, 4)]
    [SerializeField] private string dialogue = "Hmm... Não há ninguém aqui.";

    [TextArea(2, 4)]
    [SerializeField] private string caughtDialogue = "Então era aqui que você estava!";

    [SerializeField] private float caughtSpeechDuration = 2.5f;

    private Coroutine sequenceCoroutine;

    private bool sequenceRunning;
    private Action onSequenceFinished;

    private void Awake()
    {
        HideSpeech();
        SetWalking(false);
    }

    // INVESTIGAÇÃO NORMAL
    public bool BeginSequence(Action onFinished)
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        if (sequenceRunning)
        {
            Debug.LogWarning("[SEGURANÇA] Já existe uma sequência em andamento.");
            return false;
        }

        if (!ValidatePoints(true))
            return false;

        sequenceRunning = true;
        onSequenceFinished = onFinished;

        gameObject.SetActive(true);
        transform.position = entryPoint.position;

        HideSpeech();

        sequenceCoroutine = StartCoroutine(GuardSequence());

        Debug.Log("[SEGURANÇA] Sequência normal iniciada.");

        return true;
    }

    private IEnumerator GuardSequence()
    {
        yield return MoveTo(centerPoint);

        yield return LookAround();

        ShowSpeechFor(npcName, dialogue);

        yield return new WaitForSecondsRealtime(2.5f);

        HideSpeech();

        yield return MoveTo(exitPoint);

        FinishSequence();
    }

    private IEnumerator LookAround()
    {
        SetWalking(false);

        float elapsed = 0f;
        float switchInterval = 0.5f;
        float switchTimer = 0f;

        while (elapsed < lookDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            switchTimer += Time.unscaledDeltaTime;

            if (switchTimer >= switchInterval)
            {
                switchTimer = 0f;

                if (spriteRenderer != null)
                    spriteRenderer.flipX = !spriteRenderer.flipX;
            }

            yield return null;
        }
    }

    // SEQUÊNCIA DE CAPTURA
    public void BeginCatchSequence(Action onFinished)
    {
        if (sequenceRunning)
            return;

        if (caughtPosition == null && centerPoint == null)
        {
            Debug.LogError("[SEGURANÇA] Nenhum ponto de captura configurado.");
            return;
        }

        sequenceRunning = true;
        onSequenceFinished = onFinished;

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        if (sequenceCoroutine != null)
            StopCoroutine(sequenceCoroutine);

        gameObject.SetActive(true);

        sequenceCoroutine = StartCoroutine(CatchSequence());

        Debug.Log("[SEGURANÇA] Sequência de captura iniciada.");
    }

    private IEnumerator CatchSequence()
    {
        // Começa no ponto de entrada.
        if (entryPoint != null)
            transform.position = entryPoint.position;

        // Prioriza o ponto exclusivo de captura.
        if (caughtPosition != null)
        {
            yield return MoveToCaughtPosition(caughtPosition);
        }
        else
        {
            Debug.LogWarning(
                "[SEGURANÇA] CaughtPosition não configurado. Usando CenterPoint."
            );

            yield return MoveToCaughtPosition(centerPoint);
        }

        SetWalking(false);

        ShowSpeechFor(npcName, caughtDialogue);

        yield return new WaitForSecondsRealtime(caughtSpeechDuration);

        HideSpeech();

        FinishCatchSequence();
    }

    // MOVIMENTAÇÃO EXCLUSIVA DA CAPTURA: X E Y.
    private IEnumerator MoveToCaughtPosition(Transform target)
    {
        if (target == null)
            yield break;

        SetWalking(true);

        while (Vector2.Distance(transform.position, target.position) > arrivalDistance)
        {
            float direction = Mathf.Sign(target.position.x - transform.position.x);

            transform.position = Vector3.MoveTowards(
                transform.position,
                target.position,
                moveSpeed * Time.unscaledDeltaTime
            );

            if (spriteRenderer != null && direction != 0)
                spriteRenderer.flipX = direction < 0;

            yield return null;
        }

        transform.position = target.position;

        SetWalking(false);
    }

    private bool ValidatePoints(bool requireExit)
    {
        if (entryPoint == null)
        {
            Debug.LogError("[SEGURANÇA] Entry Point não foi atribuído.");
            return false;
        }

        if (centerPoint == null)
        {
            Debug.LogError("[SEGURANÇA] Center Point não foi atribuído.");
            return false;
        }

        if (requireExit && exitPoint == null)
        {
            Debug.LogError("[SEGURANÇA] Exit Point não foi atribuído.");
            return false;
        }

        return true;
    }

    // MOVIMENTAÇÃO NORMAL: SOMENTE EIXO X.
    private IEnumerator MoveTo(Transform target)
    {
        if (target == null)
            yield break;

        SetWalking(true);

        while (Mathf.Abs(transform.position.x - target.position.x) > arrivalDistance)
        {
            Vector3 currentPosition = transform.position;

            float direction = Mathf.Sign(target.position.x - currentPosition.x);

            currentPosition.x = Mathf.MoveTowards(
                currentPosition.x,
                target.position.x,
                moveSpeed * Time.unscaledDeltaTime
            );

            transform.position = currentPosition;

            if (spriteRenderer != null && direction != 0)
                spriteRenderer.flipX = direction < 0;

            yield return null;
        }

        Vector3 finalPosition = transform.position;
        finalPosition.x = target.position.x;
        transform.position = finalPosition;

        SetWalking(false);
    }

    public void ShowSpeechFor(string characterName, string message)
    {
        if (speechBubble == null)
        {
            Debug.LogWarning("[BALÃO] Speech Bubble não foi atribuído.");
            return;
        }

        speechBubble.SetActive(true);

        if (speechNameText != null)
            speechNameText.text = characterName;

        if (speechText != null)
            speechText.text = message;
    }

    public void HideSpeech()
    {
        if (speechBubble != null)
            speechBubble.SetActive(false);
    }

    private void SetWalking(bool walking)
    {
        if (animator != null && !string.IsNullOrEmpty(walkingParameter))
            animator.SetBool(walkingParameter, walking);
    }

    // FINALIZA INVESTIGAÇÃO NORMAL.
    private void FinishSequence()
    {
        sequenceRunning = false;
        sequenceCoroutine = null;

        SetWalking(false);
        HideSpeech();

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        Action callback = onSequenceFinished;
        onSequenceFinished = null;

        if (callback != null)
            callback.Invoke();

        Debug.Log("[SEGURANÇA] Investigação normal finalizada.");
    }

    // FINALIZA CAPTURA.
    private void FinishCatchSequence()
    {
        sequenceRunning = false;
        sequenceCoroutine = null;

        SetWalking(false);
        HideSpeech();

        Action callback = onSequenceFinished;
        onSequenceFinished = null;

        if (callback != null)
            callback.Invoke();

        gameObject.SetActive(false);

        Debug.Log("[SEGURANÇA] Sequência de captura finalizada.");
    }

    private void OnDisable()
    {
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }

        sequenceRunning = false;
        SetWalking(false);
        HideSpeech();
    }
}
