using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace DDS
{
    public static class SpawningFunctions
    {
        /// <summary>
        /// Change this variable to adjust the number of objects you want to spawn.
        /// </summary>
        static public int WaveSpawnAmount;

        /// <summary>
        /// If true any logic without the adjustable spawn height will be ignored on trigger spawns. 
        /// </summary>
        static public bool TriggerSpawnOverridesLogic;

        /// <summary>
        /// If this is true the IsVisible will return false if the object is in frustum but behind a wall.
        /// </summary>
        static public bool UseOcclusionCulling;

        /// <summary>
        /// On every trigger spawn this variable has to set to true to use the TriggerSpawnOverridesLogic variable.
        /// </summary>
        static public bool IsTriggerSpawn;

        /// <summary>
        /// This list holds all objects which are ignored by the OcclusionCulling.
        /// </summary>
        static public List<GameObject> FrustumIgnoredObjects;

        public static GameObject[] Spawn(SpawningComponent PositioningComponent, Camera FrustumCamera, SpawningStyles SpawnType)
        {            
            if (PositioningComponent is SpawnPosition && SpawnType == SpawningStyles.Wave)
                return null;

            List<SpawnAbleObject> NotEmptyObjects = new List<SpawnAbleObject>();

            for (int i = 0; i < PositioningComponent.Objects_to_Spawn.Length; i++)
            {
                if (PositioningComponent.Objects_to_Spawn[i].ObjectToSpawn != null)
                    NotEmptyObjects.Add(PositioningComponent.Objects_to_Spawn[i]);
            }

            foreach (var Object in NotEmptyObjects)
            {
                if (Object.ObjectToSpawn.GetComponent<PersonalLogicScript>())
                    if (!Object.ObjectToSpawn.GetComponent<PersonalLogicScript>().DoSpawn)
                        NotEmptyObjects.Remove(Object);
            }

            if (NotEmptyObjects.Count == 0)
                return null;

            SpawnAbleObject[] Objects = NotEmptyObjects.ToArray();

            int IndexOfObject = 0;
            if (!GetHighestSpawnPriority(Objects, out IndexOfObject))
                return null;

            Vector3[] Positions = new Vector3[0];

            int SpawnAmount = WaveSpawnAmount;

            if (SpawnType == SpawningStyles.Continuous)
                SpawnAmount = 1;

            if (!PositioningComponent.GetPositions(Objects[IndexOfObject], SpawnAmount, FrustumCamera, out Positions))
                return null;

            GameObject[] ObjectsToReturn = new GameObject[Positions.Length];

            for(int index = 0; index < Positions.Length; index++)
            {
                ObjectsToReturn[index] = GameObject.Instantiate(Objects[IndexOfObject].ObjectToSpawn, Positions[index], Objects[IndexOfObject].ObjectToSpawn.transform.rotation);
            }
           
            return ObjectsToReturn;
        }

     

        /// <summary>
        /// Checks if any child of the given object is in the frustum.
        /// </summary>
        /// <returns></returns>
        public static bool IsAnyChildVisible(GameObject Object, Vector3 DesiredPosition, Camera FrustumCamera)
        {
            for (int ChildIndex = 0; ChildIndex < Object.transform.childCount; ChildIndex++)
            {
                if (IsVisibleChild(FrustumCamera, Object.transform.GetChild(ChildIndex).gameObject, Object, DesiredPosition))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the object bounds is in the Camera Frustum.
        /// </summary>
        /// <returns></returns>
        public static bool IsVisible(Camera FrustumCamera, GameObject ObjectToCheck, Vector3 DesiredPosition)
        {
            GameObject BufferObject = ObjectToCheck;
            BufferObject.transform.position = DesiredPosition;

            if (!BufferObject.GetComponent<Renderer>())
                return false;

            Bounds BoundsToCheck = BufferObject.GetComponent<Renderer>().bounds;
            Plane[] CameraBounds = GeometryUtility.CalculateFrustumPlanes(FrustumCamera);
            if(GeometryUtility.TestPlanesAABB(CameraBounds, BoundsToCheck))
            {
                if (!UseOcclusionCulling)
                    return false;

                Vector3[] RayCastPositions = new Vector3[8];

                RayCastPositions[0] = new Vector3(-BoundsToCheck.extents.x, +BoundsToCheck.extents.y, -BoundsToCheck.extents.z);
                RayCastPositions[1] = new Vector3(-BoundsToCheck.extents.x, -BoundsToCheck.extents.y, -BoundsToCheck.extents.z);
                RayCastPositions[2] = new Vector3(+BoundsToCheck.extents.x, -BoundsToCheck.extents.y, -BoundsToCheck.extents.z);
                RayCastPositions[3] = new Vector3(+BoundsToCheck.extents.x, +BoundsToCheck.extents.y, -BoundsToCheck.extents.z);
                RayCastPositions[4] = new Vector3(-BoundsToCheck.extents.x, +BoundsToCheck.extents.y, BoundsToCheck.extents.z);
                RayCastPositions[5] = new Vector3(-BoundsToCheck.extents.x, -BoundsToCheck.extents.y, BoundsToCheck.extents.z);
                RayCastPositions[6] = new Vector3(+BoundsToCheck.extents.x, -BoundsToCheck.extents.y, BoundsToCheck.extents.z);
                RayCastPositions[7] = new Vector3(+BoundsToCheck.extents.x, +BoundsToCheck.extents.y, BoundsToCheck.extents.z);

                for (int index = 0; index < RayCastPositions.Length; index++)
                {
                    RaycastHit hit;
                    if (!Physics.Linecast(RayCastPositions[index] + BoundsToCheck.center, FrustumCamera.transform.position, out hit))
                            return true;
                    else
                    {
                        bool IsIgnoredObject = false;
                        if (FrustumIgnoredObjects != null)
                        {
                            for (int IgnoredObjectIndex = 0; IgnoredObjectIndex < FrustumIgnoredObjects.Count; IgnoredObjectIndex++)
                            {
                                if (FrustumIgnoredObjects[IgnoredObjectIndex] != null)
                                {
                                    if (FrustumIgnoredObjects[IgnoredObjectIndex].transform != null)
                                        if (FrustumIgnoredObjects[IgnoredObjectIndex].gameObject != null)
                                            if (FrustumIgnoredObjects[IgnoredObjectIndex].gameObject == hit.transform.gameObject)
                                            {
                                                IsIgnoredObject = true;
                                            }
                                }
                            }
                        }

                        if (IsIgnoredObject)
                            return true;
                    }                                     
                }
                return false;
            }

            return false;
        }

        /// <summary>
        /// Checks if the child is in the cameras frustum.
        /// </summary>
        /// <returns></returns>
        public static bool IsVisibleChild(Camera FrustumCamera, GameObject ObjectToCheck, GameObject Parent, Vector3 DesiredPosition)
        {
            GameObject BufferObject = ObjectToCheck;
            BufferObject.transform.position = DesiredPosition + ObjectToCheck.transform.localPosition;

            if (!BufferObject.GetComponent<Renderer>())
                return false;

            Bounds BoundsToCheck = BufferObject.GetComponent<Renderer>().bounds;
            Plane[] CameraBounds = GeometryUtility.CalculateFrustumPlanes(FrustumCamera);
            if (GeometryUtility.TestPlanesAABB(CameraBounds, BoundsToCheck))
            {
                if (!UseOcclusionCulling)
                    return true;

                Vector3[] RayCastPositions = new Vector3[8];

                RayCastPositions[0] = new Vector3(-BoundsToCheck.extents.x, +BoundsToCheck.extents.y, -BoundsToCheck.extents.z);
                RayCastPositions[1] = new Vector3(-BoundsToCheck.extents.x, -BoundsToCheck.extents.y, -BoundsToCheck.extents.z);
                RayCastPositions[2] = new Vector3(+BoundsToCheck.extents.x, -BoundsToCheck.extents.y, -BoundsToCheck.extents.z);
                RayCastPositions[3] = new Vector3(+BoundsToCheck.extents.x, +BoundsToCheck.extents.y, -BoundsToCheck.extents.z);
                RayCastPositions[4] = new Vector3(-BoundsToCheck.extents.x, +BoundsToCheck.extents.y, BoundsToCheck.extents.z);
                RayCastPositions[5] = new Vector3(-BoundsToCheck.extents.x, -BoundsToCheck.extents.y, BoundsToCheck.extents.z);
                RayCastPositions[6] = new Vector3(+BoundsToCheck.extents.x, -BoundsToCheck.extents.y, BoundsToCheck.extents.z);
                RayCastPositions[7] = new Vector3(+BoundsToCheck.extents.x, +BoundsToCheck.extents.y, BoundsToCheck.extents.z);

                for (int index = 0; index < RayCastPositions.Length; index++)
                {
                    RaycastHit hit;
                    if (!Physics.Linecast(RayCastPositions[index] + BoundsToCheck.center, FrustumCamera.transform.position, out hit))
                        return true;
                    else
                    {
                        bool IsIgnoredObject = false;
                        if (FrustumIgnoredObjects != null)
                        {
                            for (int IgnoredObjectIndex = 0; IgnoredObjectIndex < FrustumIgnoredObjects.Count; IgnoredObjectIndex++)
                            {
                                if (FrustumIgnoredObjects[IgnoredObjectIndex] != null)
                                {
                                    if (FrustumIgnoredObjects[IgnoredObjectIndex].transform != null)
                                        if (FrustumIgnoredObjects[IgnoredObjectIndex].gameObject != null)
                                            if (FrustumIgnoredObjects[IgnoredObjectIndex].gameObject == hit.transform.gameObject)
                                            {
                                                IsIgnoredObject = true;
                                            }
                                }
                            }
                        }

                        if (IsIgnoredObject)
                            return true;
                    }
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// Returns all childs and objects which are in the given List.
        /// </summary>
        /// <param name="Objects"></param>
        /// <returns></returns>
        public static GameObject[] GetAllIgnoredObjects(List<IgnoredObject> Objects)
        {
            List<GameObject> ObjectsToReturn = new List<GameObject>();

            for (int ObjectIndex = 0; ObjectIndex < Objects.Count; ObjectIndex++)
            {
                if (Objects[ObjectIndex].Object != null)
                {
                    if (Objects[ObjectIndex].Object.GetComponent<Collider>())
                        ObjectsToReturn.Add(Objects[ObjectIndex].Object);

                    if (Objects[ObjectIndex].IgnoreParent)
                        if (Objects[ObjectIndex].Object.transform.parent)
                            if (Objects[ObjectIndex].Object.transform.parent.GetComponent<Collider>())
                                ObjectsToReturn.Add(Objects[ObjectIndex].Object.transform.parent.gameObject);


                    if (Objects[ObjectIndex].IgnoreChildrens)
                    {
                        for (int ChildrenIndex = 0; ChildrenIndex < Objects[ObjectIndex].Object.transform.childCount; ChildrenIndex++)
                        {
                            if (Objects[ObjectIndex].Object.transform.GetChild(ChildrenIndex).GetComponent<Collider>())
                                ObjectsToReturn.Add(Objects[ObjectIndex].Object.transform.GetChild(ChildrenIndex).gameObject);
                        }
                    }
                }

            }

            GameObject[] ReturnField = new GameObject[ObjectsToReturn.Count];

            for (int Index = 0; Index < ObjectsToReturn.Count; Index++)
                ReturnField[Index] = ObjectsToReturn[Index];
            if (ReturnField.Length > 0)
                foreach (GameObject G in ReturnField)
                    Debug.Log(G.name);


            return ReturnField;
        }

        /// <summary>
        /// Calculates the spawn weight and returns a random object based on that number.
        /// </summary>
        /// <param name="Objects"></param>
        /// <param name="ObjectIndex"></param>
        /// <returns></returns>
        public static bool GetHighestSpawnPriority(SpawnAbleObject[] Objects, out int ObjectIndex)
        {
            List<SpawnAbleObject> SpawnableObjects = new List<SpawnAbleObject>();                       

            ObjectIndex = 0;

            for(int i = 0; i < Objects.Length; i++)
            {
                if (!Objects[i].ObjectToSpawn)
                {
                    Debug.Log("Object is null");
                    return false;
                }

                if (Objects[i].ObjectToSpawn.GetComponent<PersonalLogicScript>() == null)                
                    SpawnableObjects.Add(Objects[i]);
                
                else if (Objects[i].ObjectToSpawn.GetComponent<PersonalLogicScript>().DoSpawn)
                    SpawnableObjects.Add(Objects[i]);             
            }

            float[] SpawnRangeMin = new float[SpawnableObjects.Count];
            float[] SpawnRangeMax = new float[SpawnableObjects.Count];


            float CollectiveWeight = 0;

            foreach (SpawnAbleObject SpawnableObject in SpawnableObjects)
            {
                CollectiveWeight += SpawnableObject.ChanceToSpawn;
            }

            for(int index= 0; index < SpawnableObjects.Count; index++)
            {
                float SpawnMin = 0;
                float SpawnMax = 0;

                if(index != 0)                
                    SpawnMin = SpawnRangeMax[index - 1];

                SpawnMax = SpawnMin + (SpawnableObjects[index].ChanceToSpawn / CollectiveWeight * 100);

                SpawnRangeMin[index] = SpawnMin;
                SpawnRangeMax[index] = SpawnMax;
            }


            int RandomNumber = UnityEngine.Random.Range(1, 100);

            for(int index = 0; index < SpawnRangeMin.Length; index++)
            {
                if(RandomNumber >= SpawnRangeMin[index] && RandomNumber <= SpawnRangeMax[index])
                {
                    ObjectIndex = index;
                    return true;
                }
            }
            
            return false;
        }
    }


}

