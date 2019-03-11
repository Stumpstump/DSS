using System.Collections;
using System.Collections.Generic;
using UnityEngine;

        
    static public class DistanceChecking 
    {
        /// <summary>
        /// returns if the (X,Z) distance of the given Objects is smaller than the given Max Distance  
        /// </summary>
        /// <param name="SpawnerPosition"> The position of the Spawner </param>
        /// <param name="ObjectToCheckPosition"> The position of the Object to check the Position of </param>
        /// <param name="MaxDistance"> The Maximum distance which will return into true </param>
        static public bool TwoDimensional(Transform SpawnerPosition, Transform ObjectToCheckPosition, float MaxDistance)
        {

            float DistanceX, DistanceZ;

            DistanceX = SpawnerPosition.position.x - ObjectToCheckPosition.position.x;
            DistanceZ = SpawnerPosition.position.z - ObjectToCheckPosition.position.z;

            if (DistanceX < 0f)
            {
                DistanceX *= -1;
            }

            if (DistanceZ < 0f)
            {
                DistanceZ *= -1;
            }

            DistanceX *= DistanceX;
            DistanceZ *= DistanceZ;

            return Mathf.Sqrt(DistanceX + DistanceZ) >= MaxDistance;
        }

    static public bool TwoDimensional(Vector3 Position1, Vector3 Position2, float MaxDistance)
    {

        float DistanceX, DistanceZ;

        DistanceX = Position1.x - Position2.x;
        DistanceZ = Position1.z - Position2.z;

        if (DistanceX < 0f)
        {
            DistanceX *= -1;
        }

        if (DistanceZ < 0f)
        {
            DistanceZ *= -1;
        }

        DistanceX *= DistanceX;
        DistanceZ *= DistanceZ;

        return Mathf.Sqrt(DistanceX + DistanceZ) >= MaxDistance;
    }

    /// <summary>
    /// returns if the (X,Y,Z) distance of the given Objects is smaller than the given Max Distance  
    /// </summary>
    /// <param name="SpawnerPosition"> The position of the Spawner </param>
    /// <param name="ObjectToCheckPosition"> The position of the Object to check the Position of </param>
    /// <param name="MaxDistance"> The Maximum distance which will return into true </param>
    static public bool ThreeDimensional(Transform SpawnerPosition, Transform ObjectToCheckPosition, float MaxDistance)
    {

        float DistanceX, DistanceY, DistanceZ;

        DistanceX = SpawnerPosition.position.x - ObjectToCheckPosition.position.x;
        DistanceZ = SpawnerPosition.position.z - ObjectToCheckPosition.position.z;
        DistanceY = SpawnerPosition.position.y - ObjectToCheckPosition.position.y;

        if (DistanceX < 0f)
        {
            DistanceX *= -1;
        }

        if (DistanceZ < 0f)
        {
            DistanceZ *= -1;
        }

        if (DistanceY < 0f)
        {
            DistanceY *= 1;
        }

        DistanceX *= DistanceX;
        DistanceZ *= DistanceZ;
        DistanceY *= DistanceY;

        return Mathf.Sqrt(DistanceX + DistanceZ + DistanceY) >= MaxDistance;
    }
}

