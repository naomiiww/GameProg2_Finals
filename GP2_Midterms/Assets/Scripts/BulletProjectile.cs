using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BulletProjectile : MonoBehaviour
{
    [Header("Bullet")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;

    [Header("Explosion")]
    [SerializeField] private ParticleSystem destroyEffectPrefab;
    [SerializeField] private float effectCleanupTime = 3f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private CircleCollider2D bulletCollider;

    private bool hasHit;
    private readonly RaycastHit2D[] castHits = new RaycastHit2D[32];

    public ColorSet CurrentColor { get; private set; } = ColorSet.Red;

    private void Awake()
    {
        Debug.LogError("TEST: BulletProjectile Awake ran on " + name);

        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        bulletCollider = GetComponent<CircleCollider2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.simulated = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        bulletCollider.enabled = true;
        bulletCollider.isTrigger = true;
    }

    public void Initialize(Vector2 direction, ColorSet color, float launchSpeed = -1f)
    {
        Debug.LogError("TEST: Bullet Initialize was called");

        if (launchSpeed > 0f)
        {
            speed = launchSpeed;
        }

        SetColor(color);

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.down;
        }

        direction.Normalize();

        rb.linearVelocity = direction * speed;

        Destroy(gameObject, lifetime);
    }


    private void FixedUpdate()
    {
        // Diagnostic: confirm physics updates are running.
        if (Time.frameCount % 30 == 0)
        {
            Debug.LogWarning(
                $"BULLET CHECK | Position: {transform.position} | " +
                $"Velocity: {(rb != null ? rb.linearVelocity.ToString() : "NO RIGIDBODY")} | " +
                $"HasHit: {hasHit}"
            );
        }

        if (hasHit || rb == null || bulletCollider == null)
            return;

        Vector2 velocity = rb.linearVelocity;
        float distance = velocity.magnitude * Time.fixedDeltaTime;

        if (distance <= 0.0001f)
        {
            Debug.LogWarning("BULLET CHECK: Bullet velocity is zero.");
            return;
        }

        float scale = Mathf.Max(
            Mathf.Abs(transform.lossyScale.x),
            Mathf.Abs(transform.lossyScale.y)
        );

        float radius = Mathf.Max(
            0.001f,
            bulletCollider.radius * scale
        );

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.useLayerMask = false;

        int hitCount = bulletCollider.Cast(
            velocity.normalized,
            filter,
            castHits,
            distance
        );

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hitCollider = castHits[i].collider;

            if (hitCollider == null)
                continue;

            // Ignore the bullet's own object and children.
            if (hitCollider.transform == transform ||
                hitCollider.transform.IsChildOf(transform))
                continue;

            // Ignore the player who fired the bullet.
            if (hitCollider.GetComponentInParent<PlayerActions>() != null)
                continue;

            Debug.LogWarning("BULLET HIT DETECTED: " + hitCollider.name);

            handleHit(hitCollider);
            return;
        }

    }

    public void SetColor(ColorSet newColor)
    {
        CurrentColor = newColor;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = getBulletTint();
        }
    }

    private Color getBulletTint()
    {
        switch (CurrentColor)
        {
            case ColorSet.Red:
                return Color.red;

            case ColorSet.Blue:
                return Color.blue;

            case ColorSet.Green:
                return Color.green;

            case ColorSet.Violet:
                return new Color(0.6f, 0.2f, 1f);

            default:
                return Color.white;
        }
    }

    private void OnTrigggerEnter2D(Collider2D other)
    {
        Debug.Log($"BULLET TRIGGERED: {other.name} | " + $"Layer: {LayerMask.LayerToName(other.gameObject.layer)}", this);
        handleHit(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        handleHit(collision.collider);
    }


    private void handleHit(Collider2D other)
    {
        if (hasHit || other == null)
            return;

        if (other.GetComponentInParent<PlayerActions>() != null)
            return;

        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();

        if (enemy != null)
        {
            bool matchingColor = enemy.CurrentColor == CurrentColor;

            hasHit = true;

            Debug.Log($"ENEMY HIT: {enemy.name} | " + $"Bullet color: {CurrentColor} | " + $"Enemy color: {enemy.CurrentColor}");

            enemy.TakeDamage(CurrentColor);

            if (!matchingColor)
            {
                spawnExplosion(Color.white, transform.position);
            }

            Destroy(gameObject);
            return;
        }

        Debug.Log($"Bullet hit a non-enemy object: {other.name}");

        hasHit = true;
        spawnExplosion(Color.white, transform.position);
        Destroy(gameObject);
    }

    private void spawnExplosion(Color color, Vector3 position)
    {
        if (destroyEffectPrefab == null)
        {
            Debug.LogWarning("Explosion prefab not assigned!");
            return;
        }

        ParticleSystem effect = Instantiate(destroyEffectPrefab, position, Quaternion.identity);

        effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem[] systems = effect.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem system in systems)
        {
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = system.main;
            main.startColor = color;
        }

        effect.Play(true);

        Destroy(effect.gameObject, effectCleanupTime);
    }
}
