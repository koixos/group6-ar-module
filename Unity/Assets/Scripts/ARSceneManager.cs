using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARSceneManager : MonoBehaviour
{
    [SerializeField] private ARSession arSession;
    [SerializeField] private XROrigin xrOrigin;

    private void Awake()
    {
        if (arSession == null)
            arSession = FindObjectOfType<ARSession>();

        if (xrOrigin == null)
            xrOrigin = FindObjectOfType<XROrigin>();
    }
}
