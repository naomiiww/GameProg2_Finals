using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Bullet")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;

    [Header("Bullet Colors")]
    [SerializeField] private Color redTint = Color.red;
    [SerializeField] private Color blueTint = Color.blue;
    [SerializeField] private Color greenTint = Color.green;
    [SerializeField] private Color violetTint = new Color(0.6f, 0.2f, 1f);

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private CircleCollider2D bulletCollider;

    public ColorSet CurrentColor { get; private set; } = ColorSet.Red;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        bulletCollider = GetComponent<CircleCollider2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;

        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        bulletCollider.isTrigger = true;
    }

    public void Initialize(Vector2 direction, ColorSet color, float launchSpeed = -1f)
    {
        if (launchSpeed > 0f)
        {
            speed = launchSpeed;
        }

        SetColor(color);

        rb.linearVelocity = direction.normalized * speed;

        Destroy(gameObject, lifetime);
    }

    public void SetColor(ColorSet newColor)
    {
        CurrentColor = newColor;

        switch (CurrentColor)
        {
            case ColorSet.Red:
                spriteRenderer.color = redTint;
                break;

            case ColorSet.Blue:
                spriteRenderer.color = blueTint;
                break;

            case ColorSet.Green:
                spriteRenderer.color = greenTint;
                break;

            case ColorSet.Violet:
                spriteRenderer.color = violetTint;
                break;
        }
    }

    private void OnTrigggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerActions>() != null)
        {
            return;
        }

        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();

        if (enemy != null)
        {
            IDamageable damageable = enemy;
            damageable.TakeDamage(CurrentColor);

            Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}
