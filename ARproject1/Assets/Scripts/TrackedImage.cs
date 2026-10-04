using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TrackedImages : MonoBehaviour
{
    [SerializeField]
    private ARTrackedImageManager m_TrackedImageManager;

    private readonly Dictionary<TrackableId, TrackingState> m_PreviousStates = new();

    void OnEnable()
    {
        m_TrackedImageManager.trackablesChanged.AddListener(OnChanged);
    }

    void OnDisable()
    {
        m_TrackedImageManager.trackablesChanged.RemoveListener(OnChanged);
    }

    void OnChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // Cuando se ve por primera vez
        foreach (var image in eventArgs.added)
        {
            Debug.Log(
                $"[ADDED] {image.referenceImage.name} | " +
                $"State: {image.trackingState} | " +
                $"Position: {image.transform.position}"
            );

            m_PreviousStates[image.trackableId] = image.trackingState;
        }

        // Si algo cambia
        foreach (var image in eventArgs.updated)
        {
            if (!m_PreviousStates.TryGetValue(image.trackableId, out var previousState))
            {
                m_PreviousStates[image.trackableId] = image.trackingState;
                continue;
            }

            if (previousState != image.trackingState)
            {
                Debug.Log(
                    $"[STATE CHANGED] {image.referenceImage.name}: " +
                    $"{previousState} -> {image.trackingState}"
                );

                m_PreviousStates[image.trackableId] = image.trackingState;
            }
        }

        // Esto deberia salir cuando se borra la imagen
        foreach (var removed in eventArgs.removed)
        {
            Debug.Log($"[REMOVED] {removed.Key}");

            m_PreviousStates.Remove(removed.Key);
        }
    }
}