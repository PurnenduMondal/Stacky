using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject gameOverPanel;

    [Header("Game Over UI")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI bestScoreText;

    [Header("Revive")]
    public GameObject reviveButton;
    private bool reviveUsed = false;

    private bool gameStarted = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ShowMainMenu();
    }

    void Update()
    {
        // Start game on tap from main menu
        if (!gameStarted && mainMenuPanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            StartGame();
        }
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        gameStarted = false;
    }

    void StartGame()
    {
        mainMenuPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        gameStarted = true;

        // Tell StackManager to start
        StackManager.Instance.StartGame();
    }

    public void ShowGameOver(int score)
    {
        gameStarted = false;
        gameOverPanel.SetActive(true);

        finalScoreText.text = score.ToString();

        int best = PlayerPrefs.GetInt("BestScore", 0);
        if (score > best)
        {
            best = score;
            PlayerPrefs.SetInt("BestScore", best);
            bestScoreText.text = "BEST: " + best + " 🏆";
        }
        else
        {
            bestScoreText.text = "BEST: " + best;
        }

        
    }

    public void GrantRevive()
    {
        reviveUsed = true;
        reviveButton.SetActive(false);
        gameOverPanel.SetActive(false);
        gameStarted = true;

        // Resume the game from current state
        StackManager.Instance.ResumeAfterRevive();
    }


    public void OnTryAgainPressed()
    {
        // Restart the scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }




    public bool IsGameStarted() => gameStarted;
}