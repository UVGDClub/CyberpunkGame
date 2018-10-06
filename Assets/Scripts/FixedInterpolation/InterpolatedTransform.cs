using UnityEngine;
using System.Collections;

[RequireComponent(typeof(InterpolatedTransformUpdater))]
/*
 * Interpolates an object to the transform at the latest FixedUpdate from the transform at the previous FixedUpdate.
 * It is critical this script's execution order is set before all other scripts that modify a transform from FixedUpdate.
 */
public class InterpolatedTransform : MonoBehaviour
{
    private TransformData[] m_lastTransforms; // Stores the transform of the object from the last two FixedUpdates
    private int m_newTransformIndex; // Keeps track of which index is storing the newest value.

    /*
     * Initializes the list of previous orientations
     */
    void OnEnable()
    {
        ForgetPreviousTransforms();
    }

    /*
     * Resets the previous transform list to store only the objects's current transform. Useful to prevent
     * interpolation when an object is teleported, for example.
     */
    public void ForgetPreviousTransforms()
    {
        m_lastTransforms = new TransformData[2];
        TransformData t = new TransformData(transform.localPosition, transform.localRotation, transform.localScale);
        m_lastTransforms[0] = t;
        m_lastTransforms[1] = t;
        m_newTransformIndex = 0;
    }

    /*
     * Sets the object transform to what it was last FixedUpdate instead of where is was last interpolated to,
     * ensuring it is in the correct position for gameplay scripts.
     */
    void FixedUpdate()
    {
        TransformData mostRecentTransform = m_lastTransforms[m_newTransformIndex];
        transform.localPosition = mostRecentTransform.position;
        transform.localRotation = mostRecentTransform.rotation;
        transform.localScale = mostRecentTransform.scale;
    }
    
    /*
     * Runs after ofther scripts to save the objects's final transform.
     */
    public void LateFixedUpdate()
    {
        m_newTransformIndex = OldTransformIndex(); // Set new index to the older stored transform.
        m_lastTransforms[m_newTransformIndex] = new TransformData(transform.localPosition, transform.localRotation, transform.localScale);
    }

    /*
     * Interpolates the object transform to the latest FixedUpdate's transform
     */
    void Update()
    {
        TransformData newestTransform = m_lastTransforms[m_newTransformIndex];
        TransformData olderTransform = m_lastTransforms[OldTransformIndex()];

        transform.localPosition = Vector3.Lerp(olderTransform.position, newestTransform.position, InterpolationController.InterpolationFactor);
        transform.localRotation = Quaternion.Slerp(olderTransform.rotation, newestTransform.rotation, InterpolationController.InterpolationFactor);
        transform.localScale = Vector3.Lerp(olderTransform.scale, newestTransform.scale, InterpolationController.InterpolationFactor);
    }

    /*
     * The index of the older stored transform
     */
    private int OldTransformIndex()
    {
        return (m_newTransformIndex == 0 ? 1 : 0);
    }

    /*
     * Stores transform data
     */
    private struct TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public TransformData(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }
}
