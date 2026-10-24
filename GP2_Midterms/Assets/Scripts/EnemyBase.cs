using UnityEngine;

[System.Serializable]
public class EnemySprites
{
    public Sprite upEnemy;
    public Sprite downEnemy;
    public Sprite leftEnemy;
    public Sprite rightEnemy;
}
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    //standard enemy movement
    //Spawn colored enemies that inherit from this abstract class and continuously move towards the player.
    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 1f;

    [Header("Enemy Sprites")]
    [SerializeField] private EnemySprites enemySprites = new EnemySprites();

    [Header("Enemy Colors")]
    [SerializeField] private Color redTint = Color.red;
    [SerializeField] private Color blueTint = Color.blue;
    [SerializeField] private Color greenTint = Color.green;
    [SerializeField] private Color violetTint = new Color(0.6f, 0.2f, 1f);

    [Header("Scoring")]
    [SerializeField] private int scoreValue = 5;

    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    protected Transform playerTarget;

    public ColorSet CurrentColor { get; private set; }

    private bool initialized;
    private bool defeated;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    public virtual void Initialize(Transform player)
    {
        playerTarget = player;

        if (enemySprites == null || enemySprites.upEnemy == null || enemySprites.downEnemy == null || enemySprites.leftEnemy == null || enemySprites.rightEnemy == null)
        {
            Debug.LogError("Enemy sprites are not assigned.", this);
            return;
        }

        int ColorCount = System.Enum.GetValues(typeof(ColorSet)).Length;

        CurrentColor = (ColorSet)Random.Range(0, ColorCount);

        initialized = true;

        if (playerTarget != null)
        {
            Vector2 direction = (Vector2)playerTarget.position - (Vector2)transform.position;

            UpdateFacingSprite(direction);
        }
        else
        {
            UpdateFacingSprite(Vector2.down);
        }
    }

    protected virtual void FixedUpdate()
    {
        if (!initialized || playerTarget == null || defeated)
        {
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
        {
            return;
        }

        moveTowardsTarget();
    }

    protected abstract void moveTowardsTarget();

    protected void UpdateFacingSprite(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            spriteRenderer.sprite = direction.x > 0 ? enemySprites.rightEnemy : enemySprites.leftEnemy;
        }
        else
        {
            spriteRenderer.sprite = direction.y > 0 ? enemySprites.upEnemy : enemySprites.downEnemy;
        }

        spriteRenderer.color = GetEnemyTint();
    }

    private Color GetEnemyTint()
    {
        switch (CurrentColor)
        {
            case ColorSet.Red:
                return redTint;

            case ColorSet.Blue:
                return blueTint;

            case ColorSet.Green:
                return greenTint;

            case ColorSet.Violet:
                return violetTint;

            default:
                return Color.white;
        }
    }

    public virtual void TakeDamage(ColorSet bulletColor)
    {
        if (defeated)
        {
            return;
        }

        if (bulletColor != CurrentColor)
        {
            return;
        }

        defeated = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.addScore(scoreValue);
        }
        else
        {
            Debug.LogWarning("Enemy defeated, but GameManager cannot be found to add points.", this);
        }

        Destroy(gameObject);
    }
}
