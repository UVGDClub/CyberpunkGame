using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    Transform myTransform;
    public Transform target;
    public Vector3 offset;

    public bool following = true;

    [Space()]
    [Header("Shake")]
    public int defaultShakeIterations = 45;
    public Vector2 defaultShakeIntensity = Vector2.one * 0.1f;

    [Space()]
    [Header("Panning")]
    public Vector3 defaultPanOffset = new Vector3(2, 2, 0);
    [Range(0,1)]
    public float defaultPanRate = 0.05f;

    private void Awake()
    {
        myTransform = transform;
        StartCoroutine(Follow());
    }

    IEnumerator Follow()
    {
        while(following)
        {
            myTransform.position = target.transform.position + offset;
            yield return null;
        }    
    }

    [ContextMenu("Shake Test")]
    public void StartShake()
    {
        StartCoroutine(Shake(defaultShakeIterations, defaultShakeIntensity));
    }

    public void StartShake(int iterations, Vector2 intensity)
    {
        StartCoroutine(Shake(iterations, intensity));
    }

    IEnumerator Shake(int iterations, Vector2 intensity)
    {
        int count = 0;
        while(count < iterations)
        {
            myTransform.position += new Vector3(Random.Range(-intensity.x, intensity.x), Random.Range(-intensity.y, intensity.y));
            count++;
            yield return null;
        }
    }

    [ContextMenu("Pan Test")]
    public void StartPan()
    {
        StartCoroutine(Pan(myTransform.position + defaultPanOffset, defaultPanRate, true, true));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="panRate"> Should be a value less than 1, as we interpolate with this value additively. </param>
    /// <param name="inAndOut"></param>
    /// <param name="followOnExit"></param>
    public void StartPan(Vector3 destination, float panRate, bool inAndOut, bool followOnExit)
    {
        StartCoroutine(Pan(destination, panRate, inAndOut, followOnExit));
    }

    IEnumerator Pan(Vector3 destination, float panRate, bool inAndOut, bool followOnExit)
    {
        following = false;
        Vector3 initPos = myTransform.position;
        float t = 0;

        while(t < 1)
        {            
            myTransform.position = Vector3.Lerp(initPos, destination, t);
            t += panRate;
            yield return null;
        }
        myTransform.position = destination;

        if(inAndOut)
        {
            t = 0;
            while(t < 1)
            {
                myTransform.position = Vector3.Lerp(destination, initPos, t);
                t += panRate;
                yield return null;
            }
            myTransform.position = initPos;
        }

        if(followOnExit)
        {
            following = true;
            StartCoroutine(Follow());
        }
    }
}
