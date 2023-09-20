using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine;
using System.Linq;
using System;
using TMPro;

public class ARCreateAlien : MonoBehaviour
{
    public float MinSpawnDistance;
    public float MaxSpawnDistance;
    public GameObject Character;
    public GameObject SpeechBubble;
    public Camera ARCamera;
    public TextAsset Script;

    private readonly Dictionary<string, Tuple<GameObject, ScriptCollection.ScriptItem>> instantiatedScripts
        = new Dictionary<string, Tuple<GameObject, ScriptCollection.ScriptItem>>();
    private static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();

    private int? SpawnedIndex = null;

    private ScriptCollection scriptItems;

    private GameObject spawnedAlien;
    private ARTrackedImageManager trackedImagesManager;

    void Awake()
    {
        trackedImagesManager = GetComponent<ARTrackedImageManager>();
        scriptItems = JsonUtility.FromJson<ScriptCollection>(Script.text);
    }

    void OnEnable()
    {
        trackedImagesManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImagesManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // go through each recognized image and check for them in the email config
        foreach (var trackedImage in eventArgs.added)
        {
            var imageName = trackedImage.referenceImage.name;
            foreach (var script in scriptItems.items)
            {
                if (string.Compare(script.name, imageName, StringComparison.Ordinal) == 0
                    && !instantiatedScripts.ContainsKey(imageName))
                {
                    // script found; create it and update the UI
                    var newPrefab = SpawnAlien(trackedImage, script);
                    instantiatedScripts[imageName] = new Tuple<GameObject, ScriptCollection.ScriptItem>(newPrefab, script);
                }
            }
        }

        //var active = instantiatedEmails.Values.FirstOrDefault(v => v.Item1.activeSelf)?.Item2;

        // Go through images with state changes
        /*foreach (var trackedImage in eventArgs.updated)
        {
            var trackedItem = instantiatedEmails[trackedImage.referenceImage.name];
            trackedItem.Item1.SetActive(trackedImage.trackingState == TrackingState.Tracking);

            if (questionStates.TryGetValue(trackedImage.referenceImage.name, out var questionState))
            {
                UpdateBackgroundText(active, trackedItem.Item1, questionState);
            }
        }*/

        // go through items which the tracker has deemed removed
        foreach (var trackedImage in eventArgs.removed)
        {
            Destroy(instantiatedScripts[trackedImage.referenceImage.name].Item1);
            instantiatedScripts.Remove(trackedImage.referenceImage.name);
        }

        // update the active email for the UI to display correctly
        //SetActiveEmail(active);
    }








    // Update is called once per frame
    /*void Update()
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
                    break;
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
                Destroy(spawnedAlien);
                SpawnedIndex = null;
            }
        }
    }*/

    private GameObject SpawnAlien(ARTrackedImage trackedImage, ScriptCollection.ScriptItem script)
    {
        spawnedAlien = Instantiate(Character, trackedImage.transform);
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
        return spawnedAlien;
    }

    [Serializable]
    private class ScriptCollection
    {
        public ScriptItem[] items;

        [Serializable]
        public class ScriptItem
        {
            public string name;
            public string text;
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
