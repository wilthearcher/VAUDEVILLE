using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Alvo")]
    [SerializeField] private Transform target;

    [Header("Cenário")]
    [SerializeField] private SpriteRenderer cenario;

    [Header("Configuração")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    private float minCamX;
    private float maxCamX;

    private void Start()
{
    SpriteRenderer[] sprites =
        cenario.GetComponentsInChildren<SpriteRenderer>();

    if (sprites.Length == 0)
        return;

    Bounds bounds = sprites[0].bounds;

    foreach (SpriteRenderer sr in sprites)
    {
        bounds.Encapsulate(sr.bounds);
    }

    float cameraHalfWidth =
        Camera.main.orthographicSize *
        Camera.main.aspect;

    minCamX = bounds.min.x + cameraHalfWidth;
    maxCamX = bounds.max.x - cameraHalfWidth;
}

    private void LateUpdate()
    {
        if (target == null)
            return;

        float clampedX = Mathf.Clamp(
            target.position.x + offset.x,
            minCamX,
            maxCamX
        );

        Vector3 desiredPosition = new Vector3(
            clampedX,
            transform.position.y,
            offset.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}