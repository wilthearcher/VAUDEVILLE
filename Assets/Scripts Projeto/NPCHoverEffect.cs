using UnityEngine;

public class NPCHoverEffect : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private SpriteRenderer npcRenderer;
    [SerializeField] private SpriteRenderer auraRenderer;

    private void Awake()
    {
        if (npcRenderer == null)
            npcRenderer = GetComponent<SpriteRenderer>();

        if (npcRenderer == null || auraRenderer == null)
        {
            Debug.LogError(
                "Configure os SpriteRenderers do NPC e da Aura!",
                this
            );

            enabled = false;
            return;
        }

        auraRenderer.enabled = false;
    }

    private void LateUpdate()
    {
        // Acompanha os frames da animação do NPC.
        auraRenderer.sprite = npcRenderer.sprite;

        // Acompanha a direção do personagem.
        auraRenderer.flipX = npcRenderer.flipX;
        auraRenderer.flipY = npcRenderer.flipY;
    }

    private void OnMouseEnter()
    {
        auraRenderer.enabled = true;
    }

    private void OnMouseExit()
    {
        auraRenderer.enabled = false;
    }

    private void OnDisable()
    {
        if (auraRenderer != null)
            auraRenderer.enabled = false;
    }
}
