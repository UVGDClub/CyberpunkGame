using UnityEngine;
using System.Collections;

/*
 * Used to allow a later script execution order for FixedUpdate than in GameplayTransform.
 * It is critical this script runs after all other scripts that modify a transform from FixedUpdate.
 */
public class InterpolatedTransformUpdater : MonoBehaviour
{
    private InterpolatedTransform m_interpolatedTransform;
    
	void Awake()
    {
        m_interpolatedTransform = GetComponent<InterpolatedTransform>();
    }
	
	void FixedUpdate()
    {
        m_interpolatedTransform.LateFixedUpdate();
    }
}
