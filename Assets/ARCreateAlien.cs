using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine;
using System.Linq;
using System;

public class ARCreateAlien : MonoBehaviour
{
    public float MinSpawnDistance;
    public float MaxSpawnDistance;
    public GameObject Character;
    public GameObject SpeechBubble;
    public Camera ARCamera;
    public TextAsset Script;

    private ARRaycastManager _raycastManager;

    private static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();

    private int? SpawnedIndex = null;

    private ScriptCollection scriptItems;

    private GameObject spawnedAlien;

    void Awake()
    {
        _raycastManager = GetComponent<ARRaycastManager>();
        /*ARPlaneManager planeManager = GetComponent<ARPlaneManager>();
        if (planeManager != null)
        {
            planeManager.planesChanged += OnPlanesChanged;
        }*/
        scriptItems = JsonUtility.FromJson<ScriptCollection>(Script.text);
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
    IEnumerator Start()
    {
#if UNITY_EDITOR
        // No permission handling needed in Editor
#elif UNITY_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation)) {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation);
        }

        // First, check if user has location service enabled
        if (!UnityEngine.Input.location.isEnabledByUser) {
            // TODO Failure
            Debug.LogFormat("Android and Location not enabled");
            yield break;
        }

#elif UNITY_IOS
        if (!UnityEngine.Input.location.isEnabledByUser) {
            // TODO Failure
            Debug.LogFormat("IOS and Location not enabled");
            yield break;
        }
#endif

        // Starts the location service.
        Input.location.Start();

        // Waits until the location service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // If the service didn't initialize in 20 seconds this cancels location service use.
        if (maxWait < 1)
        {
            print("Timed out");
            yield break;
        }

        // If the connection failed this cancels location service use.
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            // TODO: Error handling
            print("Unable to determine device location");
            yield break;
        }
        else
        {
            // If the connection succeeded, this retrieves the device's current location and displays it in the Console window.
            print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
        }
    }

    // Update is called once per frame
    void Update()
    {
        var location = Input.location.lastData;
        if (SpawnedIndex == null)
        {
            int? scriptIndex = null;
            ScriptCollection.ScriptItem scriptForLocation = null;
            for (int i = 0; i < scriptItems.items.Length; i++)
            {
                scriptForLocation = scriptItems.items[i];
                if (HaversineDistance(location.latitude, location.longitude, scriptForLocation.lat, scriptForLocation.lon) <= scriptForLocation.maxDist)
                {
                    scriptIndex = i;
                }
            }
            if (scriptIndex != null)
            {
                if (_raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), Hits, TrackableType.AllTypes))
                {
                    var hit = Hits[0];
                    if (hit.distance >= MinSpawnDistance && hit.distance <= MaxSpawnDistance)
                    {
                        SpawnAlien(hit, scriptForLocation);
                        SpawnedIndex = scriptIndex;
                    }
                }
            }
        }
        else
        {
            var script = scriptItems.items[SpawnedIndex.Value];
            if (HaversineDistance(location.latitude, location.longitude, script.lat, script.lon) > script.maxDist)
            {
                print("left area");
                Destroy(spawnedAlien);
            }
        }
    }

    private void SpawnAlien(ARRaycastHit hit, ScriptCollection.ScriptItem script)
    {
        spawnedAlien = Instantiate(Character, hit.pose.position, hit.pose.rotation);
        spawnedAlien.GetComponent<Alien>().ARCamera = ARCamera;
        var speechBubble = Instantiate(SpeechBubble, spawnedAlien.transform);

        var canvas = speechBubble?.transform?.GetChild(1)?.GetComponent<Canvas>();
        if (canvas != null)
        {
            var body = canvas?.transform?.GetChild(0)?.GetComponent<TMPro.TextMeshProUGUI>();
            if (body != null)
            {
                body.text = script.text;
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

    [Serializable]
    private class ScriptCollection
    {
        public ScriptItem[] items;

        [Serializable]
        public class ScriptItem
        {
            public double lat;
            public double lon;
            public string text;
            public double maxDist;
        }
    }

    // See https://stormconsultancy.co.uk/blog/storm-news/the-haversine-formula-in-c-and-sql/
    private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        double R = 6371;
        var lat = ToRadians(lat2 - lat1);
        var lng = ToRadians(lon2 - lon1);
        var h1 = Math.Sin(lat / 2) * Math.Sin(lat / 2) +
                      Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                      Math.Sin(lng / 2) * Math.Sin(lng / 2);
        var h2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h1)));
        return R * h2;
    }

    private static double ToRadians(double val)
    {
        return (Math.PI / 180) * val;
    }
}
