using UnityEngine;

public class SortLayers : MonoBehaviour
{
    [Header("Sort")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Tooltip("Optional point at the base of sprite")]
    [SerializeField] private Transform sortingPoint;

    [SerializeField] private int orderrOffset = 0;

    [SerializeField] private float precision = 100f;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            Debug.LogError("SortLayer requires a spriteRenderer on this object");

            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        float yPosition = sortingPoint != null ? sortingPoint.position.y : transform.position.y;

        spriteRenderer.sortingOrder = Mathf.RoundToInt(-yPosition * precision) + orderrOffset;
    }
}
