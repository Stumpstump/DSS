using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace DDS
{
    public class SpawningComponent : MonoBehaviour
    {
        [SerializeField]
        public SpawnAbleObject[] Objects_to_Spawn;

        virtual public bool GetPositions(SpawnAbleObject Object, int DesiredAmountOfPositions, Camera FrustumCamera, out Vector3[] ReturnedPositions)
        {
            ReturnedPositions = new Vector3[0];
            return true;
        }
           

    }
}
