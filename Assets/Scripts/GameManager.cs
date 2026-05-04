using UnityEngine;
using TMPro;
using System.Collections;

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

        // Zoom out before showing game over panel
        if (Camera.main != null && StackManager.Instance != null)
        {
            CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
            if (cam != null)
            {
                float towerHeight = StackManager.Instance.GetTowerHeight();
                cam.ZoomOutToShowTower(towerHeight);
            }
            else
            {
                Debug.LogWarning("CameraFollow script not found on Main Camera!");
            }
        }
        else
        {
            Debug.LogWarning("Camera.main or StackManager.Instance is null!");
        }

        StartCoroutine(ShowGameOverDelayed(score));
    }

    IEnumerator ShowGameOverDelayed(int score)
    {
        yield return new WaitForSeconds(1.5f);
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
        // No need to reset camera — scene reload handles everything
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }




    public bool IsGameStarted() => gameStarted;
}