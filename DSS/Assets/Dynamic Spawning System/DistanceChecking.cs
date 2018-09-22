using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public class DistanceChecking : MonoBehaviour
    {
        static public bool TwoDimensionalCheck(Transform SpawnerPosition, Transform ObjectToCheckPosition, float MaxDistance)
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

            return Mathf.Sqrt(DistanceX + DistanceZ) <= MaxDistance;
        }

    static public bool ThreeDimensionalCheck(Transform SpawnerPosition, Transform ObjectToCheckPosition, float MaxDistance)
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

        if(DistanceY < 0f)
        {
            DistanceY *= 1;
        }

        DistanceX *= DistanceX;
        DistanceZ *= DistanceZ;
        DistanceY *= DistanceY;

        return Mathf.Sqrt(DistanceX + DistanceZ + DistanceY) <= MaxDistance;
    }
}

