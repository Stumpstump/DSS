using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DDS
{
    public class SpawnedObjectContainer
    {
        public GameObject this[int Index]
        {
            get
            {
                return SpawnedObjects[Index];
            }

            set
            {
                SpawnedObjects[Index] = value;
            }
        }

        public int Size
        {
            get
            {
                return SpawnedObjects.Count;
            }
        }


        public void AddObjects(GameObject[] ObjectsToAdd)
        {
            foreach (var ObjectToAdd in ObjectsToAdd.ToList())
                SpawnedObjects.Add(ObjectToAdd);
        }

        public void Clear()
        {
            SpawnedObjects.Clear();
        }

        public void Update()
        {
            foreach (var ObjectToCheck in SpawnedObjects.ToList())
                if (!ObjectToCheck)
                    SpawnedObjects.Remove(ObjectToCheck);
        }
        private List<GameObject> SpawnedObjects = new List<GameObject>(); 
    }
}
