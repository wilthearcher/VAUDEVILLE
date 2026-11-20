
using UnityEngine;

public class NPCHoverEffect : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private SpriteRenderer npcRenderer;
    [SerializeField] private SpriteRenderer auraRenderer;

    private void Awake()
    {
        // Primeiro, procura o SpriteRenderer do próprio NPC.
        if (npcRenderer == null)
            npcRenderer = GetComponent<SpriteRenderer>();

        // Se não encontrou, procura nos filhos.
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

        // Procura a aura pelo nome.
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

        // Se não encontrou pelo nome, procura outro
        // SpriteRenderer nos objetos filhos.
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

        if (npcRenderer == null)
        {
            Debug.LogError(
                "SpriteRenderer do NPC não encontrado em: " + gameObject.name,
                this
            );

            enabled = false;
            return;
        }

        if (auraRenderer == null)
        {
            Debug.LogError(
                "SpriteRenderer da Aura não encontrado em: " + gameObject.name,
                this
            );

            enabled = false;
            return;
        }

        auraRenderer.enabled = false;
    }

    private void LateUpdate()
    {
        if (npcRenderer == null || auraRenderer == null)
            return;

        auraRenderer.sprite = npcRenderer.sprite;
        auraRenderer.flipX = npcRenderer.flipX;
        auraRenderer.flipY = npcRenderer.flipY;
    }

    private void OnMouseEnter()
    {
        if (auraRenderer != null)
            auraRenderer.enabled = true;
    }

    private void OnMouseExit()
    {
        if (auraRenderer != null)
            auraRenderer.enabled = false;
    }

    private void OnDisable()
    {
        if (auraRenderer != null)
            auraRenderer.enabled = false;
    }
}
