using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnArea : MonoBehaviour
{
    
    /// <summary>
    /// Returns a random Point in the Area of the boundings
    /// </summary>
    public Vector3 GetRandomPosition()
    {
        float MinZ, MinX, MaxX, MaxZ;

        MaxX = transform.position.x + GetComponent<MeshCollider>().bounds.extents.x;
        MinX = transform.position.x - GetComponent<MeshCollider>().bounds.extents.x;        

        MaxZ = transform.position.z + GetComponent<MeshCollider>().bounds.extents.z;
        MinZ = transform.position.z - GetComponent<MeshCollider>().bounds.extents.z;

        return new Vector3(Random.Range(MinX, MaxX), 0, Random.Range(MinZ, MaxZ));
    }
}
