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

    float m_NextShotTime;
    string bulletTag = "Bullet";

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
    }

    void Update()
    {

        //temp debug bullet types
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.digit1Key.wasPressedThisFrame) { bulletTag = "BulletWater"; Debug.Log(bulletTag); }
            if (keyboard.digit2Key.wasPressedThisFrame) { bulletTag = "BulletPlant"; Debug.Log(bulletTag); }
            if (keyboard.digit3Key.wasPressedThisFrame) { bulletTag = "BulletFire"; Debug.Log(bulletTag); }
        }

        var pointer = Pointer.current; // Covers all screen touches
        if (pointer == null || !pointer.press.wasPressedThisFrame) return;
        if (Time.time < m_NextShotTime) return;

        // Get the screen position of the pointer 
        Vector2 screenPos = pointer.position.ReadValue();

        if (ignoreUI && EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        // Shoot a projectile from the camera through the pointer position
        Shoot(screenPos);
        m_NextShotTime = Time.time + cooldown;
    }

    void Shoot(Vector2 screenPos)
    {
        // Raycast from the camera through the pointer position
        Ray ray = arCamera.ScreenPointToRay(screenPos);

        // Spawn the projectile away from the camera
        Vector3 spawnPos = arCamera.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, spawnDistance));

        Rigidbody projectile = Instantiate(projectilePrefab, spawnPos,
                                           Quaternion.LookRotation(ray.direction));

        // avoids the projectile to go through colliders when moving fast 
        projectile.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        projectile.linearVelocity = ray.direction * projectileSpeed;
        projectile.gameObject.tag = bulletTag;
    }
}
