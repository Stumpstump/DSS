using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPosition : MonoBehaviour
{
    public bool UseYAxis;

    /// <summary>
    /// Returns the Spawn position of this Object
    /// </summary>
    public Vector3 GetSpawnPosition()
    {
        if(UseYAxis)
        {
            return transform.position;
        }

        else
        {
            return new Vector3(transform.position.x, 0, transform.position.z);
        }

    }
}
