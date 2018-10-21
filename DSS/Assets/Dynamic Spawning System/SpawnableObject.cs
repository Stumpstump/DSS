using UnityEngine;


namespace DDS
{
    [System.Serializable]
    public class SpawnAbleObject
    {
        public SpawnAbleObject()
        {
            ChanceToSpawn = 1;
            AdaptableSpawnHeight = 1;            
        }

        [HideInInspector]
        public string ObjectName;

        public GameObject ObjectToSpawn;

        public bool ApplyLogicToChilds;

        public float ChanceToSpawn;

        public float AdaptableSpawnHeight;
    }
}