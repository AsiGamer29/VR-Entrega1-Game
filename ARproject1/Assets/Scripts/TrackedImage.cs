using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TrackedImages : MonoBehaviour
{
    [Serializable]
    public struct ImageEffect
    {
        [Tooltip("Nombre")]
        public string imageName;
        public GameObject effectPrefab;
        public AudioClip soundClip;
    }

    [SerializeField] private ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] private Shooter m_Shooter;
    [SerializeField] private ImageEffect[] m_Effects;
    [SerializeField, Range(0f, 1f)] private float m_SoundVolume = 1f;

    private readonly Dictionary<string, GameObject> m_PrefabsByName = new();
    private readonly Dictionary<string, AudioClip> m_ClipsByName = new();
    private readonly Dictionary<TrackableId, GameObject> m_Instances = new();
    private readonly Dictionary<TrackableId, AudioSource> m_AudioSources = new();

    void Awake()
    {
        foreach (var e in m_Effects)
        {
            m_PrefabsByName[e.imageName] = e.effectPrefab;
            m_ClipsByName[e.imageName] = e.soundClip;
        }
    }

    void OnEnable() => m_TrackedImageManager.trackablesChanged.AddListener(OnChanged);
    void OnDisable() => m_TrackedImageManager.trackablesChanged.RemoveListener(OnChanged);

    void OnChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (var image in args.added)
        {
            Debug.Log($"[ADDED] {image.referenceImage.name} | {image.trackingState}");
            CreateEffect(image);
            UpdateEffect(image);
        }

        foreach (var image in args.updated)
            UpdateEffect(image);

        foreach (var removed in args.removed)
        {
            Debug.Log($"[REMOVED] {removed.Key}");
            if (m_Instances.TryGetValue(removed.Key, out var go) && go != null)
                Destroy(go);
            m_Instances.Remove(removed.Key);
            m_AudioSources.Remove(removed.Key);
        }
    }

    void CreateEffect(ARTrackedImage image)
    {
        if (!m_PrefabsByName.TryGetValue(image.referenceImage.name, out var prefab) || prefab == null)
        {
            Debug.LogWarning($"No hay prefab para la imagen '{image.referenceImage.name}'");
            return;
        }

        // Instanciamos el prefab que toque, en este caso es la particula
        var instance = Instantiate(prefab, image.transform);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.SetActive(false);
        m_Instances[image.trackableId] = instance;

        // Se crea el audiosource para el audio que toque
        var audioSource = instance.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.volume = m_SoundVolume;
        m_AudioSources[image.trackableId] = audioSource;
    }

    void UpdateEffect(ARTrackedImage image)
    {
        if (!m_Instances.TryGetValue(image.trackableId, out var instance) || instance == null)
            return;

        bool isTracking = image.trackingState == TrackingState.Tracking;

        if (isTracking && !instance.activeSelf)
        {
            instance.SetActive(true);
            foreach (var ps in instance.GetComponentsInChildren<ParticleSystem>())
            {
                ps.Clear(true);
                ps.Play(true);
            }

            if (m_Shooter != null)
                m_Shooter.SetProjectileType(image.referenceImage.name);

            if (m_ClipsByName.TryGetValue(image.referenceImage.name, out var clip) && clip != null
                && m_AudioSources.TryGetValue(image.trackableId, out var src) && src != null)
            {
                src.PlayOneShot(clip);
            }
        }
        else if (!isTracking && instance.activeSelf)
        {
            instance.SetActive(false);
        }
    }
}