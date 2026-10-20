using UnityEngine;
using UnityEngine.UIElements;


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
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Player Colors")]
    [SerializeField] private PlayerSprites[] playerSpritesColors = {new PlayerSprites(), new PlayerSprites(), new PlayerSprites(), new PlayerSprites()};

    [SerializeField] private ColorSet firstColor = ColorSet.Red;

    [Header("Enemy Detection")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private LayerMask enemyLayer = ~0;
    [SerializeField] private float aimTransition = 10f;

    [Header("Shooting")]
    [SerializeField] private BulletProjectile bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float muzzleOffset = 0.5f;
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private bool autoFire = true;
    [SerializeField] private float bulletSpeed = 5f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private GameManager gameManager;

    private Vector2 movementInput;
    private Vector2 aimDirection = Vector2.down;

    private float 

    [Header("Losing")]
    private bool GameOverTriggered = false;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        checkEnemyContact(collision.collider);
    }

    private void checkEnemyContact(Collider2D collider)
    {
        if (GameOverTriggered)
        {
            return;
        }

        if (collider.CompareTag("Enemy"))
        {
            Debug.Log("Player has collided with an enemy!");
            GameManager.Instance.GameOver();
        }
    }
}

