using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }
    public static bool GameStarted { get; private set; }
    public static bool InputBlocked => !GameStarted || IsPaused;

    static bool s_SkipMainMenu;

    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject hudPanel;  //score + pause button

    void Start()
    {
        IsPaused = false;
        GameStarted = false;

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
        if (shooter != null)
        {
            shooter.score = 0;
            shooter.scoreText.text = "Score = 0";
        }

        // Reset Animator
        Animator animator = tutorialText.GetComponent<Animator>();

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
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
    }

    public void Pause()
    {
        if (!GameStarted) return;
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