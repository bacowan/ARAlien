using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alien : MonoBehaviour
{
    public Camera ARCamera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var targetPostition = new Vector3(
            ARCamera.transform.position.x,
            this.transform.position.y,
            ARCamera.transform.position.z);
        transform.LookAt(targetPostition);
    }
}
