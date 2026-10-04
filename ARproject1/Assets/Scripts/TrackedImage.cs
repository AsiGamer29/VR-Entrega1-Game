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
        public string imageName;
        public GameObject effectPrefab;
    }
    [SerializeField] private ImageEffect[] m_Effects;
    [SerializeField] private ARTrackedImageManager m_TrackedImageManager;
    

    private readonly Dictionary<string, GameObject> m_PrefabsByName = new();
    private readonly Dictionary<TrackableId, GameObject> m_Instances = new();

    void Awake()
    {
        foreach (var e in m_Effects)
            m_PrefabsByName[e.imageName] = e.effectPrefab;
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
        }
    }

    void CreateEffect(ARTrackedImage image)
    {
        if (!m_PrefabsByName.TryGetValue(image.referenceImage.name, out var prefab) || prefab == null)
        {
            Debug.LogWarning($"ERROR - Prefab not assigned for image: '{image.referenceImage.name}'");
            return;
        }

        // We instantiate the effect on the corresponding image
        var instance = Instantiate(prefab, image.transform);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.SetActive(false);
        m_Instances[image.trackableId] = instance;
    }

    void UpdateEffect(ARTrackedImage image)
    {
        if (!m_Instances.TryGetValue(image.trackableId, out var instance) || instance == null)
            return;

        bool isTracking = image.trackingState == TrackingState.Tracking;

        if (isTracking && !instance.activeSelf)
        {
            // Here we trigger the effect another time if the image is detected again
            instance.SetActive(true);
            foreach (var ps in instance.GetComponentsInChildren<ParticleSystem>())
            {
                ps.Clear(true);
                ps.Play(true);
            }
        }
        else if (!isTracking && instance.activeSelf)
        {
            // Delete if the image is not being tracked
            instance.SetActive(false);
        }
    }
}