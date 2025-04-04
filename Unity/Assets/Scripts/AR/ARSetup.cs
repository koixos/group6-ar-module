using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARSetup : MonoBehaviour
{
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private ARSession session;
    [SerializeField] private GameObject gameArena;

    private List<ARRaycastHit> hits = new();
    private bool isPlacementValid = false;
    private bool isSetupComplete = false;

    void Start()
    {
        if (raycastManager == null)
            raycastManager = FindObjectOfType<ARRaycastManager>();

        if (planeManager == null)
            planeManager = FindObjectOfType<ARPlaneManager>();

        if (session == null)
            session = FindObjectOfType<ARSession>();

        if (gameArena != null)
            gameArena.SetActive(false);
        else
            Debug.LogError("Game Arena prefab is not assigned in the inspector.");
    }

    void Update()
    {
        if (isSetupComplete)
            return;

        Vector2 screenCenter = new(Screen.width / 2, Screen.height / 2);

        isPlacementValid = raycastManager.Raycast(screenCenter, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon);

        UpdatePlacementIndicator();
    }

    public void PlaceArena()
    {
        if (!isPlacementValid || isSetupComplete)
            return;

        if (hits.Count > 0 && gameArena != null)
        {
            Pose hitPose = hits[0].pose;
            gameArena.transform.position = hitPose.position;
            gameArena.transform.rotation = hitPose.rotation;
            gameArena.SetActive(true);
            isSetupComplete = true;
            planeManager.enabled = false;
            session.enabled = false;
            OnArenaPlaced();
        }
    }

    private void UpdatePlacementIndicator()
    {
        /*
         * update placement indic visual -
         * implement would show/hide & pos an indic obj 
        */
    }

    private void OnArenaPlaced()
    {
        GameController gameController = FindObjectOfType<GameController>();
        if (gameController != null)
        {
            gameController.OnARSetupComplete();
        }
        else
        {
            Debug.LogError("GameController not found in the scene.");
        }
    }
}
