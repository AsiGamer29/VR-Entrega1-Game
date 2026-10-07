using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs (uno por tipo)")]
    public GameObject firePrefab;
    public GameObject waterPrefab;
    public GameObject plantPrefab;

    [Header("Spawn")]
    public int maxEnemies = 10;
    public int totalEnemiesToSpawn = 25;
    public float spawnInterval = 2f;
    public float spawnIntervalRandomness = 0.5f;

    [Header("Zona de spawn (dentro del campo de vision)")]
    public Transform center;
    public float minDistance = 1.8f;
    public float maxDistance = 3f;
    [Range(0f, 0.45f)] public float viewportMargin = 0.15f;

    [Header("Separacion al spawnear")]
    public float minSeparation = 0.6f;
    public int maxPlacementAttempts = 25;

    [Header("Probabilidades (pesos)")]
    public float fireWeight = 1f;
    public float waterWeight = 1f;
    public float plantWeight = 1f;

    [Header("Profundidad AR")]
    public bool useARDepthCheck = true;
    public ARRaycastManager raycastManager;
    public float surfaceMargin = 0.4f;
    public float unknownSpaceMaxDistance = 2.5f;
    public bool rejectUnknownSpace = false;

    [Header("Fuentes de obstaculos")]
    public bool useDepthRaycast = true;
    public bool usePlanes = true;
    public bool usePointCloud = true;
    public ARPointCloudManager pointCloudManager;
    public float pointRayRadius = 0.3f;
    public int minPointsToBlock = 4;

    [Header("Victoria")]
    public UnityEvent onVictory;

    public int ActiveCount => activeEnemies.Count;
    public int SpawnedCount { get; private set; }
    public int KilledCount { get; private set; }
    public Vector3 LastSpawnPos { get; private set; }

    readonly List<GameObject> activeEnemies = new List<GameObject>();
    readonly List<ARRaycastHit> arHits = new List<ARRaycastHit>();
    Camera arCamera;
    float timer;
    bool victoryFired;

    void OnEnable() { Enemy.OnKilled += HandleEnemyKilled; }
    void OnDisable() { Enemy.OnKilled -= HandleEnemyKilled; }

    void Start()
    {
        if (center != null) arCamera = center.GetComponent<Camera>();
        if (arCamera == null) arCamera = Camera.main;
        if (arCamera == null) arCamera = FindAnyObjectByType<Camera>();

        if (useARDepthCheck)
        {
            if (raycastManager == null) raycastManager = FindAnyObjectByType<ARRaycastManager>();
            if (usePointCloud && pointCloudManager == null)
                pointCloudManager = FindAnyObjectByType<ARPointCloudManager>();
        }

        timer = spawnInterval;
    }

    void Update()
    {
        activeEnemies.RemoveAll(e => e == null || !e.activeInHierarchy);

        if (!victoryFired && SpawnedCount >= totalEnemiesToSpawn && KilledCount >= totalEnemiesToSpawn)
        {
            victoryFired = true;
            onVictory?.Invoke();
        }

        if (SpawnedCount >= totalEnemiesToSpawn) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        if (activeEnemies.Count < maxEnemies)
            SpawnEnemy();

        timer = Mathf.Max(0.1f, spawnInterval + Random.Range(-spawnIntervalRandomness, spawnIntervalRandomness));
    }

    void HandleEnemyKilled(Enemy e)
    {
        KilledCount++;
    }

    void SpawnEnemy()
    {
        GameObject prefab = PickPrefab();
        if (prefab == null) return;

        if (!TryGetFreePosition(out Vector3 pos)) return;

        LastSpawnPos = pos;
        GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);
        activeEnemies.Add(enemy);
        SpawnedCount++;

        if (arCamera != null)
            Debug.Log($"Spawn {SpawnedCount}/{totalEnemiesToSpawn} | dist camara: {Vector3.Distance(arCamera.transform.position, pos):F2} m");
    }

    bool TryGetFreePosition(out Vector3 result)
    {
        result = Vector3.zero;

        for (int i = 0; i < maxPlacementAttempts; i++)
        {
            if (!TryGetCandidate(out Vector3 candidate)) continue;

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

    bool TryGetCandidate(out Vector3 result)
    {
        result = Vector3.zero;
        if (arCamera == null) return false;

        float vx = Random.Range(viewportMargin, 1f - viewportMargin);
        float vy = Random.Range(viewportMargin, 1f - viewportMargin);
        Ray ray = arCamera.ViewportPointToRay(new Vector3(vx, vy, 0f));
        float dist = Random.Range(minDistance, maxDistance);

        if (useARDepthCheck)
        {
            Vector2 screen = new Vector2(vx * Screen.width, vy * Screen.height);
            float obstacle = GetObstacleDistance(screen, ray);

            if (obstacle > 0f)
            {
                float safe = obstacle - surfaceMargin;
                if (safe < minDistance) return false;
                dist = Mathf.Min(dist, safe);
            }
            else
            {
                if (rejectUnknownSpace) return false;
                dist = Mathf.Min(dist, Mathf.Max(minDistance, unknownSpaceMaxDistance));
            }
        }

        result = ray.origin + ray.direction * dist;
        return true;
    }

    float GetObstacleDistance(Vector2 screen, Ray ray)
    {
        float best = float.MaxValue;

        if (raycastManager != null)
        {
            if (useDepthRaycast)
            {
                arHits.Clear();
                if (raycastManager.Raycast(screen, arHits, TrackableType.Depth) && arHits.Count > 0)
                    best = Mathf.Min(best, arHits[0].distance);
            }

            if (usePlanes)
            {
                arHits.Clear();
                if (raycastManager.Raycast(ray, arHits, TrackableType.PlaneWithinPolygon) && arHits.Count > 0)
                    best = Mathf.Min(best, arHits[0].distance);
            }
        }

        if (usePointCloud && pointCloudManager != null)
        {
            float p = PointCloudNearest(ray.origin, ray.direction, maxDistance + surfaceMargin + pointRayRadius);
            if (p > 0f) best = Mathf.Min(best, p);
        }

        return best == float.MaxValue ? -1f : best;
    }

    float PointCloudNearest(Vector3 origin, Vector3 dir, float maxDist)
    {
        float nearest = float.MaxValue;
        int count = 0;

        foreach (var cloud in pointCloudManager.trackables)
        {
            if (!cloud.positions.HasValue) continue;

            foreach (var local in cloud.positions.Value)
            {
                Vector3 v = cloud.transform.TransformPoint(local) - origin;
                float along = Vector3.Dot(v, dir);
                if (along < 0f || along > maxDist) continue;

                if ((v - dir * along).magnitude > pointRayRadius) continue;

                count++;
                if (along < nearest) nearest = along;
            }
        }

        return count >= minPointsToBlock ? nearest : -1f;
    }
}
