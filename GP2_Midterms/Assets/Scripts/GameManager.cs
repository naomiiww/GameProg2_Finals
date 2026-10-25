using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalScoreText;
    public bool isGameOver { get; private set; }

    [Header("Scoring")]
    private int score;
    public int Score => score;

    [Header("Timer")]
    private float elapsedTime;
    private int lastDisplayedSec = -1;

    [Header("Enemy Spawn")]
    [SerializeField] private EnemyBase enemyPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private float spawnRadius = 12f;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private int maxAliveEnemies = 25;

    private void Awake()
    {
        Debug.Log("GAME MANAGER IS RUNNING!");
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        Time.timeScale = 1f;

        isGameOver = false;
        score = 0;
        elapsedTime = 0f;
        lastDisplayedSec = -1;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        RefreshScoreUI();
        RefreshTimerUI();
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (player == null)
        {
            Debug.LogError("Player not found! Assign player.");
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("Assign enemy prefab!", this);
            return;
        }

        StartCoroutine(SpawnEnemies());
    }

    private void Update()
    {
        if (isGameOver)
        {
            return;
        }

        elapsedTime += Time.deltaTime;

        RefreshTimerUI();
    }

    public void addScore(int points)
    {
        if (isGameOver || points <= 0)
        {
            return;
        }

        score += points;

        RefreshScoreUI();
    }

    private void RefreshScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    private void RefreshTimerUI()
    {
        int totalSeconds = Mathf.FloorToInt(elapsedTime);

        if (totalSeconds == lastDisplayedSec)
        {
            return;
        }

        lastDisplayedSec = totalSeconds;

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        if (timerText != null)
        {
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
        }
    }

    private IEnumerator SpawnEnemies()
    {
        while (!isGameOver)
        {
            if (CountActiveEnemies() < maxAliveEnemies)
            {
                spawnEnemy();
            }

            yield return new WaitForSecondsRealtime(Mathf.Max(0.05f, spawnInterval));
        }
    }

    private int CountActiveEnemies()
    {
        EnemyBase[] enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        return enemies.Length;
    }

    private void spawnEnemy()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);

        Vector3 offset = new Vector3(Mathf.Cos(angle) * spawnRadius, Mathf.Sin(angle) * spawnRadius, 0f);
        Vector3 spawnPosition = player.position + offset;

        EnemyBase enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.Initialize(player);
    }

    public void GameOver()
    {
        if (isGameOver)
        {
            Debug.Log("Game Over!");
            return;
        }

        isGameOver = true;

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {score}";
            Debug.Log($"Final Score: {score}");
        }
        else
        {
            Debug.LogError("Final score not assigned!");
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Game Over Panel not assigned!");
        }

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
