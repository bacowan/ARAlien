using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine;

public class ARCreateAlien : MonoBehaviour
{
    public float MinSpawnDistance;
    public float MaxSpawnDistance;
    public GameObject Character;
    public Camera ARCamera;

    private ARRaycastManager _raycastManager;

    private static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();

    private bool hasSpawned = false;

    void Awake()
    {
        _raycastManager = GetComponent<ARRaycastManager>();
        /*ARPlaneManager planeManager = GetComponent<ARPlaneManager>();
        if (planeManager != null)
        {
            planeManager.planesChanged += OnPlanesChanged;
        }*/
    }

    void Destroy()
    {
        /*ARPlaneManager planeManager = GetComponent<ARPlaneManager>();
        if (planeManager != null)
        {
            planeManager.planesChanged -= OnPlanesChanged;
        }*/
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!hasSpawned)
        {
            if (_raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), Hits, TrackableType.AllTypes))
            {
                var hit = Hits[0];
                if (hit.distance >= MinSpawnDistance && hit.distance <= MaxSpawnDistance)
                {
                    var obj = Instantiate(Character, hit.pose.position, hit.pose.rotation);
                    obj.GetComponent<Alien>().ARCamera = ARCamera;
                    hasSpawned = true;
                }
            }
        }
    }

    /*void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        Debug.Log("Detected");
        foreach (var plane in args.added)
        {
            Debug.Log(plane.boundary);
        }
        // TODO: If this is the first time seeing such a plane, spawn the alien.
        // Reset "first time" if we leave the GPS area.
    }*/
}
