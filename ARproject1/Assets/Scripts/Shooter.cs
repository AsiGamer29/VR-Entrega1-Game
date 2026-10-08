using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class Shooter : MonoBehaviour
{
    [SerializeField] Camera arCamera;
    [SerializeField] Rigidbody projectilePrefab;
    [SerializeField] float projectileSpeed = 5f;
    [SerializeField] float spawnDistance = 0.3f;      // camera distance to spawn the projectile
    [SerializeField] float cooldown = 0.15f;
    [SerializeField] bool ignoreUI = true;            // Don't shoot when the pointer is over a UI element

    [SerializeField] Rigidbody plantBullet;
    [SerializeField] Rigidbody fireBullet;
    [SerializeField] Rigidbody waterBullet;

    float m_NextShotTime;
    string bulletTag = "Bullet";
    Rigidbody currentBullet;

    //score
    public int score;
    public TMP_Text scoreText;

    [Header("Combo")]
    public TMP_Text multiplierText;
    public float comboTimeout = 0f;        // set a time for the combo to run out
    public float scoreCountSpeed = 8f;     // velocidad del contador rodante

    int combo;
    int displayedScore;
    float lastKillTime;
    Vector3 multiplierBaseScale = Vector3.one;

    void Awake()
    {
        score = 0;
        displayedScore = 0;
        combo = 0;
        scoreText.text = "Score = 0";

        if (multiplierText != null)
        {
            multiplierBaseScale = multiplierText.transform.localScale;
            multiplierText.gameObject.SetActive(false);
        }

        if (arCamera == null)
            arCamera = Camera.main;

        if (arCamera == null)
        {
            Debug.LogError("Camera not found");
            enabled = false;
        }
        SetProjectileType("Image_Plant 2");

    }

    public void SetProjectileType(string imageName)
    {
        if (imageName == "Image_Plant 2")
        {
            currentBullet = plantBullet;
            bulletTag = "BulletPlant";
        }
        else if (imageName == "Image_Fire 2")
        {
            currentBullet = fireBullet;
            bulletTag = "BulletFire";
        }
        else if (imageName == "Image_Water 2")
        {
            currentBullet = waterBullet;
            bulletTag = "BulletWater";
        }
        else
        {
            Debug.LogWarning("Imagen desconocida: " + imageName);
            return;
        }

        Debug.Log(bulletTag);
    }

    // Calls enemy when you kill with the correct bullet
    public int RegisterKill(int basePoints)
    {
        combo++;
        lastKillTime = Time.time;

        int points = basePoints * combo;
        score += points;

        UpdateMultiplierUI(true);
        return points;
    }

    // calls enemy when the bullet is incorrect
    public void ResetCombo()
    {
        if (combo == 0) return;
        combo = 0;
        UpdateMultiplierUI(false);
    }

    // calls menu manager when the game its reset
    public void ResetScore()
    {
        score = 0;
        displayedScore = 0;
        combo = 0;
        scoreText.text = "Score = 0";
        UpdateMultiplierUI(false);
    }

    void UpdateMultiplierUI(bool punch)
    {
        if (multiplierText == null) return;

        // only shows at 2x
        bool show = combo >= 2;
        multiplierText.gameObject.SetActive(show);
        if (!show) return;

        multiplierText.text = $"x{combo}";
        if (punch) multiplierText.transform.localScale = multiplierBaseScale * 1.5f;
    }

    void UpdateScoreDisplay()
    {
        
        if (displayedScore != score)
        {
            float step = Mathf.Max(1f, Mathf.Abs(score - displayedScore) * scoreCountSpeed * Time.unscaledDeltaTime);
            displayedScore = (int)Mathf.MoveTowards(displayedScore, score, step);
            scoreText.text = $"Score = {displayedScore}";
        }


        if (multiplierText != null && multiplierText.gameObject.activeSelf)
        {
            multiplierText.transform.localScale = Vector3.Lerp(
                multiplierText.transform.localScale, multiplierBaseScale, 10f * Time.unscaledDeltaTime);
        }
    }

    void Update()
    {

        UpdateScoreDisplay();

        if (MenuManager.InputBlocked)
        {
            //Debug.Log("Bloqueado por MenuManager.InputBlocked");
            return;
        }

        if (comboTimeout > 0f && combo > 0 && Time.time - lastKillTime > comboTimeout) ResetCombo();

        //temp debug bullet types
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.digit1Key.wasPressedThisFrame) SetProjectileType("Image_Water 2");
            if (keyboard.digit2Key.wasPressedThisFrame) SetProjectileType("Image_Plant 2");
            if (keyboard.digit3Key.wasPressedThisFrame) SetProjectileType("Image_Fire 2");
        }

        var pointer = Pointer.current;
        if (pointer == null || !pointer.press.wasPressedThisFrame) return;

        Debug.Log("Toque detectado");

        if (Time.time < m_NextShotTime)
        {
            Debug.Log("Cooldown");
            return;
        }

        Vector2 screenPos = pointer.position.ReadValue();

        if (ignoreUI && EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Toque sobre la UI, no se dispara");
            return;
        }

        Shoot(screenPos);
        m_NextShotTime = Time.time + cooldown;
    }

    void Shoot(Vector2 screenPos)
    {
        Debug.Log("Shoot llamado. Bala: " + currentBullet + " | Tag: " + bulletTag);
        if (currentBullet == null) return;
        // Raycast from the camera through the pointer position
        Ray ray = arCamera.ScreenPointToRay(screenPos);

        // Spawn the projectile away from the camera
        Vector3 spawnPos = arCamera.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, spawnDistance));

        Rigidbody projectile = Instantiate(currentBullet, spawnPos,
                                   Quaternion.LookRotation(ray.direction));

        // avoids the projectile to go through colliders when moving fast 
        projectile.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        projectile.linearVelocity = ray.direction * projectileSpeed;
        projectile.gameObject.tag = bulletTag;
        Destroy(projectile.gameObject, 5f);
    }
}