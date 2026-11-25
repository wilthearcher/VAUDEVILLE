
using UnityEngine;

public class NPCHoverEffect : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SpriteRenderer npcRenderer;
    [SerializeField] private SpriteRenderer auraRenderer;

    private bool hoverAllowed = true;
    private bool isPointerOver;

    private void Awake()
    {
        if (npcRenderer == null)
            npcRenderer = GetComponent<SpriteRenderer>();

        SpriteRenderer[] renderers =
            GetComponentsInChildren<SpriteRenderer>(true);

        if (npcRenderer == null)
        {
            foreach (SpriteRenderer renderer in renderers)
            {
                if (!renderer.name.ToLower().Contains("aura"))
                {
                    npcRenderer = renderer;
                    break;
                }
            }
        }

        if (auraRenderer == null)
        {
            foreach (SpriteRenderer renderer in renderers)
            {
                if (renderer.transform != transform &&
                    renderer.name.ToLower().Contains("aura"))
                {
                    auraRenderer = renderer;
                    break;
                }
            }
        }

        if (auraRenderer == null && npcRenderer != null)
        {
            foreach (SpriteRenderer renderer in renderers)
            {
                if (renderer != npcRenderer &&
                    renderer.transform != transform)
                {
                    auraRenderer = renderer;
                    break;
                }
            }
        }

        if (npcRenderer == null || auraRenderer == null)
        {
            Debug.LogError(
                "NPCHoverEffect: referências não encontradas.", this);

            enabled = false;
            return;
        }

        auraRenderer.enabled = false;
    }

    private void LateUpdate()
    {
        if (npcRenderer == null || auraRenderer == null)
            return;

        // Mantem a aura sincronizada com o sprite do NPC.
        auraRenderer.sprite = npcRenderer.sprite;
        auraRenderer.flipX = npcRenderer.flipX;
        auraRenderer.flipY = npcRenderer.flipY;

        // Se o NPC estiver invisivel, a aura tambem ficara invisivel.
        if (!npcRenderer.enabled)
        {
            auraRenderer.enabled = false;
            isPointerOver = false;
        }
        else
        {
            UpdateAura();
        }
    }

    private void OnMouseEnter()
    {
        if (npcRenderer == null || !npcRenderer.enabled)
            return;

        isPointerOver = true;
        UpdateAura();
    }

    private void OnMouseExit()
    {
        isPointerOver = false;
        UpdateAura();
    }

    public void SetHoverEnabled(bool allowed)
    {
        hoverAllowed = allowed;
        UpdateAura();
    }

    private void UpdateAura()
    {
        if (auraRenderer == null || npcRenderer == null)
            return;

        auraRenderer.enabled =
            hoverAllowed &&
            isPointerOver &&
            npcRenderer.enabled;
    }

    private void OnDisable()
    {
        isPointerOver = false;

        if (auraRenderer != null)
            auraRenderer.enabled = false;
    }
}
