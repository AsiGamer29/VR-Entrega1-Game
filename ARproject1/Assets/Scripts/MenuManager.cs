using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

using TMPro;

public class MenuManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }
    public static bool GameStarted { get; private set; }
    public static bool GameEnded { get; private set; }
    public static bool InputBlocked => !GameStarted || IsPaused;

    [SerializeField] GameObject endPanel;   // panel Win/Lose
    [SerializeField] TMP_Text endText;      
    EnemySpawner spawner;

    static bool s_SkipMainMenu;

    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject hudPanel;  //score + pause button

    void OnEnable()
    {
        Enemy.OnReachedPlayer += HandleEnemyReached;
    }

    void OnDisable()
    {
        Enemy.OnReachedPlayer -= HandleEnemyReached;
        if (spawner != null) spawner.onVictory.RemoveListener(Win);
    }

    void HandleEnemyReached(Enemy e) => Lose();

    public void Win() => EndGame(true);
    public void Lose() => EndGame(false);

    void EndGame(bool won)
    {
        if (!GameStarted || GameEnded) return;
        GameEnded = true;
        Time.timeScale = 0f;
        pauseButton.SetActive(false);
        endText.text = won ? "YOU WIN!" : "YOU LOSE";
        endPanel.SetActive(true);
    }

    void Start()
    {
        IsPaused = false;
        GameStarted = false;

        GameEnded = false;
        spawner = FindAnyObjectByType<EnemySpawner>();
        if (spawner != null) spawner.onVictory.AddListener(Win);

        if (s_SkipMainMenu)
        {
            s_SkipMainMenu = false;
            StartGame();
        }
        else
        {
            ShowMainMenu();
        }
    }

    public GameObject tutorialText;
    public GameObject pauseButton;

    // return Android button
    void Update()
    {
        var kb = Keyboard.current;
        if (GameStarted && kb != null && kb.escapeKey.wasPressedThisFrame)
        {
            if (IsPaused) Resume(); else Pause();
        }
    }

    void ShowMainMenu()
    {
        GameStarted = false;
        Time.timeScale = 0f;
        mainMenuPanel.SetActive(true);
        pausePanel.SetActive(false);
        hudPanel.SetActive(false);
        endPanel.SetActive(false);
    }

    void ResetGame()
    {
        // Delete all enemies
        foreach (var e in FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            Destroy(e.gameObject);

        // Delete bullets in scene
        foreach (var p in FindObjectsByType<Projectile>(FindObjectsSortMode.None))
            Destroy(p.gameObject);

        // Restart score
        var shooter = FindAnyObjectByType<Shooter>();
        if (shooter != null) shooter.ResetScore();

        // Reset Animator
        Animator animator = tutorialText.GetComponent<Animator>();

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        if (spawner != null) spawner.ResetSpawner();

    }

    // Buttons
    public void StartGame()
    {
        GameStarted = true;
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        mainMenuPanel.SetActive(false);
        pausePanel.SetActive(false);
        hudPanel.SetActive(true);
        endPanel.SetActive(false);
        pauseButton.SetActive(true);
    }

    public void Pause()
    {
        if (!GameStarted || GameEnded) return;
        IsPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        pausePanel.SetActive(true);
        pauseButton.SetActive(false);
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        pausePanel.SetActive(false);
        pauseButton.SetActive(true);
    }

    public void Restart()
    {
        ResetGame();
        StartGame();
    }

    public void BackToMainMenu()
    {
        ResetGame();
        IsPaused = false;
        AudioListener.pause = false;
        ShowMainMenu();
    }
}