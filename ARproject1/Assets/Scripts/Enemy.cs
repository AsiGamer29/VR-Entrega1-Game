using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;

public class Enemy : MonoBehaviour
{
    Shooter player;
    public int scoreGiven = 10;
    public Color scoreColor = Color.white;

    public enum Types { Fire, Water, Plant };
    public Types type;
    string Weakness = "null";

    [Header("Objetivo")]
    [Tooltip("Si es null usa la camara principal (en AR, la del XR Origin)")]
    public Transform target;

    [Header("Acercamiento")]
    public float approachSpeed = 0.1f;   // velocidad de enemigos
    public float stopDistance = 1.2f;     // deteccion de colision con el player

    [Header("Separacion entre enemigos")]
    public float separationRadius = 1f;
    public float separationStrength = 2f;

    [Header("Mirar al jugador")]
    public bool lookAtPlayer = true;
    public bool lookOnlyHorizontal = false;
    public Vector3 modelRotationOffset = new Vector3(0f, 180f, 0f);
    public float turnSpeed = 10f;

    [Header("Fuego - Circulos")]
    public float fireRadius = 0.1f;
    public float fireSpeed = 1f;

    [Header("Agua - Vertical")]
    public float waterAmplitude = 0.1f;
    public float waterSpeed = 0.8f;

    [Header("Planta - Movimiento en S")]
    public float plantAmplitude = 0.15f;
    public float plantSpeed = 0.8f;

    public bool HasReachedPlayer { get; private set; }

    // Lo escucha el spawner para contar muertes (victoria)
    public static event System.Action<Enemy> OnKilled;
    public static event System.Action<Enemy> OnReachedPlayer;

    static readonly List<Enemy> allEnemies = new List<Enemy>();

    Vector3 basePos;
    Vector3 offsetFromCamera;
    float timeOffset;
    bool isDead = false;
    float deadTimer = 0.8f;

    Animator anim;
    BoxCollider bc;

    //audio
    AudioSource audioSource;
    public AudioClip dieSFX;
    public AudioClip mistakeSFX;

    //text
    public GameObject scoreTextPrefab;
    Transform hudParent;

    private void Awake()
    {
        var hud = GameObject.Find("HUD");
        if (hud != null) hudParent = hud.transform;
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        bc = GetComponent<BoxCollider>();
        audioSource = GetComponent<AudioSource>();

        // El Animator no debe mover el objeto: la posicion la controla este script
        if (anim != null) anim.applyRootMotion = false;

        // Si el prefab tiene Rigidbody, que nada lo empuje (planos AR, otras colisiones).
        // Las balas lo siguen detectando igualmente.
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        player = FindAnyObjectByType<Shooter>();

        switch (type)
        {
            case Types.Fire: Weakness = "BulletWater"; break;
            case Types.Water: Weakness = "BulletPlant"; break;
            case Types.Plant: Weakness = "BulletFire"; break;
        }

        if (target == null)
        {
            if (Camera.main != null) target = Camera.main.transform;
            else
            {
                var cam = FindAnyObjectByType<Camera>();
                if (cam != null) target = cam.transform;
            }
        }

        if (target != null)
            offsetFromCamera = transform.position - target.position;
    }

    void OnEnable()
    {
        allEnemies.Add(this);
        timeOffset = Random.Range(0f, 10f);
        HasReachedPlayer = false;
    }

    void OnDisable()
    {
        allEnemies.Remove(this);
    }


    void LateUpdate()
    {
        if (target == null) return;
        if (isDead) return;

        // Para el enemigo, la camara siempre esta en (0,0,0): hacia ella es -offset
        Vector3 toTarget = -offsetFromCamera;
        float dist = toTarget.magnitude;

        if (dist > stopDistance)
        {
            offsetFromCamera += toTarget.normalized * approachSpeed * Time.deltaTime;
        }
        else if (!HasReachedPlayer)
        {
            HasReachedPlayer = true;
            Debug.Log("Enemy reached player");
            OnReachedPlayer?.Invoke(this);
        }

        // separation between enemies
        for (int i = 0; i < allEnemies.Count; i++)
        {
            Enemy other = allEnemies[i];
            if (other == this) continue;

            Vector3 away = offsetFromCamera - other.offsetFromCamera;
            float d = away.magnitude;
            if (d < separationRadius)
            {
                if (d < 0.001f) away = Random.onUnitSphere;
                offsetFromCamera += away.normalized * (separationRadius - d) * separationStrength * Time.deltaTime;
            }
        }

        // movement by type
        Vector3 forward = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : Vector3.forward;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        if (right.sqrMagnitude < 0.0001f) right = Vector3.right;
        right.Normalize();
        Vector3 up = Vector3.Cross(forward, right).normalized;

        float t = Time.time + timeOffset;
        Vector3 wobble = Vector3.zero;

        switch (type)
        {
            case Types.Fire:
                wobble = right * (Mathf.Cos(t * fireSpeed) * fireRadius)
                       + up * (Mathf.Sin(t * fireSpeed) * fireRadius);
                break;

            case Types.Water:
                wobble = Vector3.up * (Mathf.Sin(t * waterSpeed) * waterAmplitude);
                break;

            case Types.Plant:
                wobble = right * (Mathf.Sin(t * plantSpeed) * plantAmplitude);
                break;
        }

        // camera just gives initial position
        transform.position = target.position + offsetFromCamera + wobble;

        if (lookAtPlayer)
        {
            Vector3 lookDir = target.position - transform.position;
            if (lookOnlyHorizontal) lookDir.y = 0f;

            if (lookDir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir) * Quaternion.Euler(modelRotationOffset);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            }
        }
    }

    IEnumerator Die()
    {
        isDead = true;
        if (anim != null) anim.SetBool("isDead", true);
        if (bc != null) bc.enabled = false;
        if (audioSource != null && dieSFX != null) audioSource.PlayOneShot(dieSFX);

        yield return new WaitForSeconds(deadTimer);
        gameObject.SetActive(false);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;
        if (collision.gameObject.tag == Weakness)
        {
            Debug.Log("Enemy Destroyed");

            int points = player.RegisterKill(scoreGiven);   

            if (scoreTextPrefab != null && hudParent != null)
            {
                GameObject textGO = Instantiate(scoreTextPrefab, hudParent, false);
                TMP_Text scoreText = textGO.GetComponentInChildren<TMP_Text>();
                if (scoreText != null)
                {
                    scoreText.text = $"+{points}";  
                    scoreText.color = scoreColor;
                }
            }

            OnKilled?.Invoke(this);
            StartCoroutine(Die());
        }
        else if (collision.gameObject.tag == "BulletWater" || collision.gameObject.tag == "BulletPlant"
            || collision.gameObject.tag == "BulletFire" || collision.gameObject.tag == "Bullet")
        {
            player.ResetCombo(); 
            if (audioSource != null && mistakeSFX != null) audioSource.PlayOneShot(mistakeSFX);
        }
    }
}