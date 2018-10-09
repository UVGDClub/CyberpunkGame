using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    Transform myTransform;
    public Transform target;
    public Vector3 offset;

    private void Awake()
    {
        myTransform = transform;
    }
	
	// Update is called once per frame
	void Update () {
        myTransform.position = target.transform.position + offset;
	}
}
