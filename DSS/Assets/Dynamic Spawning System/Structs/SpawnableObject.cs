using UnityEngine;


namespace DDS
{
    [System.Serializable]
    public struct SpawnAbleObject
    {
        public GameObject ObjectToSpawn;
        public bool ApplyLogicToChilds;
        public float ChanceToSpawn;
    }
}