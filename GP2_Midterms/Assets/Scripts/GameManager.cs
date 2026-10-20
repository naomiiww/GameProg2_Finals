using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public TextMeshProUGUI scoreText;
    private int score = 0;

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
    }

    public void AddScore(int points)
    {
        score += points;
        Debug.Log("Score: " + score);
    }

    [Header("Game Over")]
    [SerializeField] private GameObject GameOverPanel;
    public bool isGameOver { get; private set; }

    public void GameOver()
    {
        if (isGameOver)
        {
            Debug.Log("Game Over!");
            return;
        }

        isGameOver = true;

        if (GameOverPanel != null)
        {
            GameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }
}
