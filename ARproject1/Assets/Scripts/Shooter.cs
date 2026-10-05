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

    void Awake()
    {
        score = 0;
        scoreText.text = "Score = 0";

        if (arCamera == null)
            arCamera = Camera.main;

        if (arCamera == null)
        {
            Debug.LogError("Camera not found");
            enabled = false;
        }
        SetProjectileType("Image_Plant");

    }

    public void SetProjectileType(string imageName)
    {
        if (imageName == "Image_Plant")
        {
            currentBullet = plantBullet;
            bulletTag = "BulletPlant";
        }
        else if (imageName == "Image_Fire")
        {
            currentBullet = fireBullet;
            bulletTag = "BulletFire";
        }
        else if (imageName == "Image_Water")
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

    void Update()
    {
        if (MenuManager.InputBlocked)
        {
            //Debug.Log("Bloqueado por MenuManager.InputBlocked");
            return;
        }

        //temp debug bullet types
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.digit1Key.wasPressedThisFrame) SetProjectileType("Image_Water");
            if (keyboard.digit2Key.wasPressedThisFrame) SetProjectileType("Image_Plant");
            if (keyboard.digit3Key.wasPressedThisFrame) SetProjectileType("Image_Fire");
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
