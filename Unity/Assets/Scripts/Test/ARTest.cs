using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARTest : MonoBehaviour
{
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private XROrigin sessionOrigin;
    [SerializeField] private ARSession session;

    // Visual indicator for debugging
    [SerializeField] private GameObject debugPlaneIndicator;

    void Start()
    {
        if (planeManager == null)
            planeManager = FindObjectOfType<ARPlaneManager>();

        if (sessionOrigin == null)
            sessionOrigin = FindObjectOfType<XROrigin>();

        if (session == null)
            session = FindObjectOfType<ARSession>();

        // Make sure we can detect planes
        if (planeManager != null)
        {
            planeManager.planesChanged += PlanesChanged;
        }

        Debug.Log("AR Test script initialized. Looking for planes...");
    }

    private void PlanesChanged(ARPlanesChangedEventArgs args)
    {
        foreach (var plane in args.added)
        {
            Debug.Log($"Plane detected! Alignment: {plane.alignment}, Size: {plane.size}");

            // Optionally instantiate a debug indicator on each detected plane
            if (debugPlaneIndicator != null)
            {
                Instantiate(debugPlaneIndicator, plane.transform.position, plane.transform.rotation);
            }
        }
    }

    void OnDisable()
    {
        if (planeManager != null)
        {
            planeManager.planesChanged -= PlanesChanged;
        }
    }
}
