
using System.Collections.Generic;

using System.Collections;

using UnityEngine;

public class Enemy : MonoBehaviour
{
    Shooter player;
    public int scoreGiven = 10;

    public enum Types { Fire, Water, Plant };
    public Types type;
    string Weakness = "null";


    [Header("Objetivo")]
    [Tooltip("Si es null usa la c�mara principal (en AR, la del XR Origin)")]
    public Transform target;

    [Header("Acercamiento")]
    public float approachSpeed = 0.04f;   // velocidad de enemigos
    public float stopDistance = 1.2f;     // deteccion de colision con el player

    [Header("Separaci�n entre enemigos")]
    public float separationRadius = 1f;   // distancia m�nima deseada entre enemigos
    public float separationStrength = 2f;   // cu�nto se empujan al solaparse

    [Header("Mirar al jugador")]
    public bool lookAtPlayer = true;
    [Tooltip("Si est� activo, solo gira en horizontal (no se inclina arriba/abajo)")]
    public bool lookOnlyHorizontal = false;
    [Tooltip("Corrige el modelo si su 'frente' no es el eje Z")]
    public Vector3 modelRotationOffset = new Vector3(0f, 180f, 0f);
    public float turnSpeed = 10f;

    [Header("Fuego - C�rculos")]
    public float fireRadius = 0.1f;
    public float fireSpeed = 1f;

    [Header("Agua - Vertical")]
    public float waterAmplitude = 0.1f;
    public float waterSpeed = 0.8f;

    [Header("Planta - Movimiento en S")]
    public float plantAmplitude = 0.15f;   
    public float plantSpeed = 0.8f;        

    public bool HasReachedPlayer { get; private set; }

    static readonly List<Enemy> allEnemies = new List<Enemy>();

    Vector3 basePos;
    float timeOffset;
    bool isDead = false;
    float deadTimer = 1f;

    Animator anim;
    BoxCollider bc;
    

    void Start()
    {
        anim = GetComponent<Animator>();
        bc = GetComponent<BoxCollider>();

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
    }

    void OnEnable()
    {
        allEnemies.Add(this);
        basePos = transform.position;
        timeOffset = Random.Range(0f, 10f); // enemigos en pantalla
        HasReachedPlayer = false;
    }

    void OnDisable()
    {
        allEnemies.Remove(this);
    }

    void Update()
    {
        if (target == null) return;

        Vector3 toTarget = target.position - basePos;
        float dist = toTarget.magnitude;

        if (dist > stopDistance)
        {
            basePos += toTarget.normalized * approachSpeed * Time.deltaTime;
        }
        else if (!HasReachedPlayer)
        {
            HasReachedPlayer = true;
            Debug.Log("Enemy reached player");
            
        }

        
        for (int i = 0; i < allEnemies.Count; i++)
        {
            Enemy other = allEnemies[i];
            if (other == this) continue;

            Vector3 away = basePos - other.basePos;
            float d = away.magnitude;
            if (d < separationRadius)
            {
                if (d < 0.001f) away = Random.onUnitSphere;
                basePos += away.normalized * (separationRadius - d) * separationStrength * Time.deltaTime;
            }
        }

        
        Vector3 forward = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : transform.forward;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        if (right.sqrMagnitude < 0.0001f) right = Vector3.right; 
        right.Normalize();
        Vector3 up = Vector3.Cross(forward, right).normalized;

        float t = Time.time + timeOffset;
        Vector3 offset = Vector3.zero;

        switch (type)
        {
            case Types.Fire:
                offset = right * (Mathf.Cos(t * fireSpeed) * fireRadius)
                       + up * (Mathf.Sin(t * fireSpeed) * fireRadius);
                break;

            case Types.Water:
                offset = Vector3.up * (Mathf.Sin(t * waterSpeed) * waterAmplitude);
                break;

            case Types.Plant:
                offset = right * (Mathf.Sin(t * plantSpeed) * plantAmplitude);
                break;
        }

        transform.position = basePos + offset;

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
        anim.SetBool("isDead", true);
        bc.enabled = false;

        yield return new WaitForSeconds(deadTimer);
        gameObject.SetActive(false);
    }

    //Destroy when bullet
    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;
        if (collision.gameObject.tag == Weakness)
        {
            Debug.Log("Enemy Destroyed");

            player.score += scoreGiven;
            player.scoreText.text = $"Score = {player.score}";

            StartCoroutine(Die());
        }
    }
}