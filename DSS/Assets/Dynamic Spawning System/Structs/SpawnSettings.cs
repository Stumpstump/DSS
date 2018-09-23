using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace DDS
{
    [System.Serializable]
    public struct SpawnSettings
    {
        [Tooltip("Delay from spawning one Object to the other")]
        public float SpawnDelay;

        [Tooltip("Spawn if chosen Object/Objects is/are in range")]
        public bool SpawnIfInRange;       
    }
}
