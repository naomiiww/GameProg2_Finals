using UnityEngine;
using System;
using UnityPipeline.Microsoft.CodeAnalysis.FlowAnalysis;

[System.Serializable]
public class PlayerSprites
{
    public Sprite upPlayer;
    public Sprite downPlayer;
    public Sprite leftPlayer;
    public Sprite rightPlayer;
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]

public class PlayerActions : MonoBehaviour, ISwitchable
{
    //The player must detect the nearest enemy within a limited range (e.g., using physics overlaps or distance checks).

    //The player must smoothly rotate to face that nearest enemy.

    [Header("Player Sprites")]
    [SerializeField] private PlayerSprites playerSprites = new PlayerSprites();

    [Header("Enemy Detection")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private LayerMask enemyLayer = ~0;
    [SerializeField] private float aimTransition = 10f;

    [Header("Sprite Facing")]
    [SerializeField] private float spriteForwardAngle = -90f;

    [Header("Shooting")]
    [SerializeField] private BulletProjectile bulletPrefab;
    [SerializeField] private Transform firePointUp;
    [SerializeField] private Transform firePointDown;
    [SerializeField] private Transform firePointLeft;
    [SerializeField] private Transform firePointRight;
    [SerializeField] private float muzzleOffset = 0.5f;
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private float bulletSpeed = 5f;

    [Header ("Player Colors")]
    [SerializeField] private Color redTint = Color.red;
    [SerializeField] private Color blueTint = Color.blue;
    [SerializeField] private Color greenTint = Color.green;
    [SerializeField] private Color violetTint = new Color(0.6f, 0.2f, 1f);
    [SerializeField] private ColorSet firstColor = ColorSet.Red;

    /*[Header("Player Position")]
    [SerializeField] private bool keepPlayerStationary = true;*/

    [Header("Losing")]
    private bool GameOverTriggered;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private GameManager gameManager;

    private Vector2 aimDirection = Vector2.down;
    private Vector2 desiredDirection = Vector2.down;

    private float targetRotation;
    private float nextFireTime;

    public ColorSet CurrentColor { get; private set; } = ColorSet.Red;
    public Vector2 aimDirection2 => aimDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();

        if (playerCollider == null)
        {
            playerCollider = GetComponentInChildren<Collider2D>();
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;

        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;

        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        targetRotation = rb.rotation;
        aimDirection = DirectionFromAngle(rb.rotation + spriteForwardAngle);
        desiredDirection = aimDirection;

        if (playerCollider == null)
        {
            Debug.LogError("Player needs Collider2D.", this);
        }

        SetColor(firstColor);
    }

    private void Start()
    {
        Debug.LogError("TEST: PlayerActions is running!");
        gameManager = GameManager.Instance;
    }

    private void Update()
    {
        Debug.LogWarning("PlayerActions Update is running");
        
        if (GameOverTriggered || isGameOver())
        {
            Debug.LogWarning("DIAGNOSTIC: gameOverTriggered is true");
            return;
        }

        Debug.LogWarning("DIAGNOSTIC: GameManager reports Game Over");

        EnemyBase nearestEnemy = FindNearestEnemy();
        Debug.Log(nearestEnemy != null ? $"Enemy detected: {nearestEnemy.name}" : "No enemy detected");

        if (GameOverTriggered || isGameOver())
        {
            Debug.LogWarning("DIAGNOSTIC: Game Over after enemy search");
            return;
        }

        if (nearestEnemy != null)
        {
            Vector2 directionToEnemy = (Vector2)nearestEnemy.transform.position - (Vector2)transform.position;

            if (directionToEnemy.sqrMagnitude > 0.0001f)
            {
                desiredDirection = directionToEnemy.normalized;

                // Aim and shoot toward the enemy without rotating the player.
                aimDirection = desiredDirection;
            }

            // Manual shooting: Spacebar or left mouse button.
            if ((Input.GetKeyDown(KeyCode.Space) ||
                 Input.GetMouseButtonDown(0)) &&
                Time.time >= nextFireTime)
            {
                Shoot();
            }
        }

        UpdatePlayerSprite();
    }

    private void FixedUpdate()
    {
        if (rb == null || isGameOver())
        {
            return;
        }

        float smoothFactor = 1f - Mathf.Exp(-aimTransition * Time.fixedDeltaTime);

        float smoothedAngle = Mathf.LerpAngle(rb.rotation, targetRotation, smoothFactor);

        rb.MoveRotation(smoothedAngle);
        Debug.Log($"Target: {targetRotation:F1}, " + $"Before: {rb.rotation:F1}, " + $"Requested: {smoothedAngle:F1}");

        aimDirection = DirectionFromAngle(smoothedAngle + spriteForwardAngle);
    }

    private Vector2 DirectionFromAngle(float angleDegrees)
    {
        float radians = angleDegrees * Mathf.Deg2Rad;

        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;
    }

    private bool isGameOver()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }

        return gameManager != null && gameManager.isGameOver;
    }

    private EnemyBase FindNearestEnemy()
    {
        float searchRadius = detectionRange;

        if (playerCollider != null)
        {
            searchRadius = Mathf.Max(searchRadius, playerCollider.bounds.extents.magnitude + 0.5f);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, searchRadius, enemyLayer);
        
        EnemyBase nearestEnemy = null;
        float nearestDistanceSqr = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
            {
                continue;
            }

            EnemyBase enemy = hit.GetComponentInParent<EnemyBase>();

            if (enemy == null)
            {
                continue;
            }

            if (playerCollider != null && playerCollider != hit && playerCollider.Distance(hit).isOverlapped)
            {
                TriggerGameOver();
                return null;
            }

            float distancesqr = ((Vector2)enemy.transform.position - (Vector2)transform.position).sqrMagnitude;

            if (distancesqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distancesqr;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    public void Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("Assign bullet prefab in PlayerActions script", this);
            return;
        }

        ColorCycle();

        Vector2 direction = aimDirection.sqrMagnitude > 0.0001f ? aimDirection.normalized : Vector2.down;

        Transform selectedFirePoint = getFirePointDirection(direction);

        Vector3 spawnPosition;

        if (selectedFirePoint != null)
        {
            spawnPosition = selectedFirePoint.position;
        }
        else
        {
            Debug.LogWarning("Appropriate firepoint is not assigned.");
            spawnPosition = transform.position + (Vector3)(direction * muzzleOffset);
        }

        BulletProjectile bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);

        bullet.Initialize(direction, CurrentColor, bulletSpeed);

        nextFireTime = Time.time + Mathf.Max(0.1f, fireRate);
    }

    private Transform getFirePointDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return direction.x >= 0f ? firePointRight : firePointLeft;
        }

        return direction.y >= 0f ? firePointUp : firePointDown;
    }

    public void ColorCycle()
    {
        int colorCount = Enum.GetValues(typeof(ColorSet)).Length;

        int nextColorIndex = ((int)CurrentColor + 1) % colorCount;
        SetColor((ColorSet)nextColorIndex);
    }

    public void SetColor(ColorSet newColor)
    {
        CurrentColor = newColor;

        UpdatePlayerSprite();
    }

    private Color GetPlayerTint()
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

    private void UpdatePlayerSprite()
    {
        if (spriteRenderer == null || playerSprites == null)
        {
            return;
        }

        if (Mathf.Abs(desiredDirection.x) >
            Mathf.Abs(desiredDirection.y))
        {
            if (desiredDirection.x > 0)
            {
                spriteRenderer.sprite = playerSprites.rightPlayer;
            }
            else
            {
                spriteRenderer.sprite = playerSprites.leftPlayer;
            }
        }
        else
        {
            if (desiredDirection.y > 0)
            {
                spriteRenderer.sprite = playerSprites.upPlayer;
            }
            else
            {
                spriteRenderer.sprite = playerSprites.downPlayer;
            }
        }

        spriteRenderer.color = GetPlayerTint();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        checkEnemyContact(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        checkEnemyContact(collider);
    }

    private void checkEnemyContact(Collider2D other)
    {
        if (GameOverTriggered || other == null)
        {
            return;
        }

        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();

        if (enemy == null)
        {
            return;
        }

        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
            Debug.LogError("GameManager not found in PlayerActions script.");
            return;
        }

        GameOverTriggered = true;
        gameManager.GameOver();
    }

    private void TriggerGameOver()
    {
        if (GameOverTriggered)
        {
            return;
        }

        GameOverTriggered = true;

        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }

        if (gameManager == null)
        {
            Debug.LogError("Game Over Failed! no GamaManager instance found.", this);
            return;
        }

        gameManager.GameOver();
    }
}

