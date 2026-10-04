using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs (uno por tipo)")]
    public GameObject firePrefab;
    public GameObject waterPrefab;
    public GameObject plantPrefab;

    [Header("Spawn")]
    public int maxEnemies = 10;
    public float spawnInterval = 2f;
    [Tooltip("Variación aleatoria del intervalo (+/-)")]
    public float spawnIntervalRandomness = 0.5f;

    [Header("Zona de spawn (alrededor del jugador/cámara)")]
    public Transform center;            // si es null busca la cámara automáticamente
    public float minDistance = 2f;
    public float maxDistance = 5f;
    public float minHeight = -0.5f;
    public float maxHeight = 1.5f;
    [Tooltip("360 = alrededor completo (delante, detrás, izquierda, derecha)")]
    [Range(10f, 360f)] public float spawnAngle = 360f;

    [Header("Separación al spawnear")]
    [Tooltip("Distancia mínima entre un enemigo nuevo y los ya existentes")]
    public float minSeparation = 0.6f;
    public int maxPlacementAttempts = 10;

    [Header("Probabilidades (pesos)")]
    public float fireWeight = 1f;
    public float waterWeight = 1f;
    public float plantWeight = 1f;

    // Datos para el DebugOverlay
    public int ActiveCount => activeEnemies.Count;
    public Vector3 LastSpawnPos { get; private set; }

    readonly List<GameObject> activeEnemies = new List<GameObject>();
    float timer;

    void Start()
    {
        if (center == null)
        {
            if (Camera.main != null)
            {
                center = Camera.main.transform;
            }
            else
            {
                var cam = FindAnyObjectByType<Camera>();
                if (cam != null) center = cam.transform;
                Debug.LogWarning("Camera.main era null, usando: " + (center != null ? center.name : "NINGUNA"));
            }
        }

        timer = spawnInterval;
    }

    void Update()
    {
        activeEnemies.RemoveAll(e => e == null || !e.activeInHierarchy);

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        if (activeEnemies.Count < maxEnemies)
            SpawnEnemy();

        timer = spawnInterval + Random.Range(-spawnIntervalRandomness, spawnIntervalRandomness);
        timer = Mathf.Max(0.1f, timer);
    }

    void SpawnEnemy()
    {
        GameObject prefab = PickPrefab();
        if (prefab == null)
        {
            Debug.LogWarning("Spawner: prefab null");
            return;
        }

        if (!TryGetFreePosition(out Vector3 pos))
        {
            Debug.Log("Spawner: no hay hueco libre, se salta este spawn");
            return;
        }

        LastSpawnPos = pos;
        GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);
        activeEnemies.Add(enemy);

        Debug.Log($"Spawn {prefab.name} en {pos}");
    }

    bool TryGetFreePosition(out Vector3 result)
    {
        result = Vector3.zero;

        for (int i = 0; i < maxPlacementAttempts; i++)
        {
            Vector3 candidate = GetSpawnPosition();
            bool free = true;

            foreach (var e in activeEnemies)
            {
                if (e == null) continue;
                if (Vector3.Distance(e.transform.position, candidate) < minSeparation)
                {
                    free = false;
                    break;
                }
            }

            if (free)
            {
                result = candidate;
                return true;
            }
        }
        return false;
    }

    GameObject PickPrefab()
    {
        float total = fireWeight + waterWeight + plantWeight;
        if (total <= 0f) return null;

        float r = Random.Range(0f, total);
        if (r < fireWeight) return firePrefab;
        if (r < fireWeight + waterWeight) return waterPrefab;
        return plantPrefab;
    }

    Vector3 GetSpawnPosition()
    {
        Transform c = center != null ? center : transform;

        Vector3 flatForward = Vector3.ProjectOnPlane(c.forward, Vector3.up);
        if (flatForward.sqrMagnitude < 0.0001f) flatForward = Vector3.forward;
        flatForward.Normalize();

        float angle = Random.Range(-spawnAngle / 2f, spawnAngle / 2f);
        Vector3 dir = Quaternion.Euler(0f, angle, 0f) * flatForward;

        float dist = Random.Range(minDistance, maxDistance);
        float height = Random.Range(minHeight, maxHeight);

        return c.position + dir * dist + Vector3.up * height;
    }
}