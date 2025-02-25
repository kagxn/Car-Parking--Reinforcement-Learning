using UnityEngine;
using System.Collections.Generic;

public class ResetManager : MonoBehaviour
{
    // List of GameObjects to be reset (drag and drop the objects in the Inspector)
    public List<GameObject> resettableObjects;

    // Dictionaries to store the initial positions and rotations of each object
    private Dictionary<GameObject, Vector3> initialPositions = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Quaternion> initialRotations = new Dictionary<GameObject, Quaternion>();

    void Awake()
    {
        // Record the initial position and rotation of each object
        foreach (GameObject obj in resettableObjects)
        {
            if (obj != null)
            {
                initialPositions[obj] = obj.transform.position;
                initialRotations[obj] = obj.transform.rotation;
            }
        }
    }

    // Call this method to reset all objects to their initial values
    public void ResetAll()
    {
        foreach (GameObject obj in resettableObjects)
        {
            if (obj != null)
            {
                if (initialPositions.ContainsKey(obj))
                    obj.transform.position = initialPositions[obj];

                if (initialRotations.ContainsKey(obj))
                    obj.transform.rotation = initialRotations[obj];

                // If the object has a Rigidbody, reset its velocity and angular velocity
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }
    }
}
