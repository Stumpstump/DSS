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
        delegate void WriteToConsole(string Text);

        static WriteToConsole WriteError = delegate (string Text) { Debug.Log(Text); };


        static public bool Trigger_Spawn_Overrides_Logic;
        static public bool UseOcclusionCulling;
        static public bool IsTriggerSpawn = false;
        static private int MaxPositionChecks = 50;
        static public List<GameObject> FrustumIgnoredObjects;


        public static GameObject SpawnPriorityObjectInArea(SpawnArea Area, SpawnAbleObject[] Objects, bool UseAreaHeight, Camera FrustumCamera)
        {
            int IndexOfObject = 0;
            if (!GetHighestSpawnPriority(Objects, out IndexOfObject))
                return null;
            Vector3[] Position;
            if (!Area.GetRandomCheckedPositions(Objects[IndexOfObject], 1, FrustumCamera, out Position))
                return null;

            return GameObject.Instantiate(Objects[IndexOfObject].ObjectToSpawn, Position[0], Objects[IndexOfObject].ObjectToSpawn.transform.rotation);
        }

        public static GameObject SpawnPriorityObjectAtSpawnPoint(SpawnPosition Point, SpawnAbleObject[] Objects, Camera FrustumCamera)
        {
            int IndexOfObject = 0;
            if (!GetHighestSpawnPriority(Objects, out IndexOfObject))
                return null;

            Vector3 SpawnPosition;
            if (!Point.GetCheckedSpawnPosition(Objects[IndexOfObject], FrustumCamera, out SpawnPosition))
                return null;

            

            return GameObject.Instantiate(Objects[IndexOfObject].ObjectToSpawn, SpawnPosition, Objects[IndexOfObject].ObjectToSpawn.transform.rotation);
        }

        public static GameObject[] SpawnWaveInArea(SpawnArea Area, SpawnAbleObject[] Objects, int ObjectAmount, Camera FrustumCamera)
        {
            int IndexOfObject = 0;
            if (!GetHighestSpawnPriority(Objects, out IndexOfObject))
                return null;

            Vector3[] Positions;

            if (!Area.GetRandomCheckedPositions(Objects[IndexOfObject], ObjectAmount, FrustumCamera, out Positions))
                return null;

            GameObject[] ObjectsToReturn = new GameObject[ObjectAmount];

            for(int i = 0; i < ObjectAmount; i++)
            {
                ObjectsToReturn[i] = GameObject.Instantiate(Objects[IndexOfObject].ObjectToSpawn, Positions[i], Objects[IndexOfObject].ObjectToSpawn.transform.rotation);
            }

            return ObjectsToReturn;
        }

        public static bool IsAnyChildBlocked(GameObject Object, Vector3 DesiredPosition)
        {
            for(int ChildIndex = 0; ChildIndex < Object.transform.childCount; ChildIndex++)
            {
                if (IsPositionBlockedChild(Object.transform.GetChild(ChildIndex).gameObject, Object, DesiredPosition))
                    return true;
            }

            return false;
        }

        public static bool IsAnyChildVisible(GameObject Object, Vector3 DesiredPosition, Camera FrustumCamera)
        {
            for (int ChildIndex = 0; ChildIndex < Object.transform.childCount; ChildIndex++)
            {
                if (IsVisibleChild(FrustumCamera, Object.transform.GetChild(ChildIndex).gameObject, Object, DesiredPosition))
                    return true;
            }

            return false;
        }


        private static Vector3 GetSpawnPoint(SpawnPosition SpawnPoint, float ObjectHeight, bool UsePointHeight)
        {
            Vector3 Position = SpawnPoint.GetSpawnPosition;
            if (!UsePointHeight)
                Position.y = ObjectHeight;

            return Position;
        }

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

        public static bool IsPositionBlocked(GameObject Object, Vector3 DesiredPosition)
        {
            Bounds ObjectBounds = new Bounds();

            if (Object.GetComponent<Renderer>())
                ObjectBounds = Object.GetComponent<Renderer>().bounds;

            else if (Object.GetComponentInChildren<Renderer>())
                ObjectBounds = Object.GetComponentInChildren<Renderer>().bounds;

            else if (Object.GetComponentInParent<Renderer>())
                ObjectBounds = Object.GetComponentInParent<Renderer>().bounds;

            if (ObjectBounds == null)
                return true;

            Vector3 BoundsOffset = Object.transform.position - ObjectBounds.center;
            ObjectBounds.center = DesiredPosition + BoundsOffset;

            Collider[] Colliders = Physics.OverlapBox(ObjectBounds.center, ObjectBounds.extents, Object.transform.rotation);

            for(int index = 0; index < Colliders.Length; index++)
            {
                if (Colliders[index].transform.parent == Object)
                    Colliders[index] = null;
            }

            for (int index = 0; index < Colliders.Length; index++)
            {
                Bounds bounds = new Bounds();

                if (Colliders[index].GetComponent<Renderer>())
                    bounds = Colliders[index].GetComponent<Renderer>().bounds;

                else if (Colliders[index].GetComponentInChildren<Renderer>())
                    bounds = Colliders[index].GetComponentInChildren<Renderer>().bounds;

                else if (Colliders[index].GetComponentInParent<Renderer>())
                    bounds = Colliders[index].GetComponentInParent<Renderer>().bounds;

                if (bounds != null)
                    if (bounds.Intersects(ObjectBounds) && Colliders[index].gameObject != Object )
                        return true;

            }

            return false;
        }

        public static bool IsPositionBlockedChild(GameObject Object, GameObject Parent, Vector3 DesiredPosition)
        {
            Bounds ObjectBounds = new Bounds();

            if (Object.GetComponent<Renderer>())
                ObjectBounds = Object.GetComponent<Renderer>().bounds;

            else if (Object.GetComponentInChildren<Renderer>())
                ObjectBounds = Object.GetComponentInChildren<Renderer>().bounds;

            else if (Object.GetComponentInParent<Renderer>())
                ObjectBounds = Object.GetComponentInParent<Renderer>().bounds;

            if (ObjectBounds == null)
                return true;

            Vector3 BoundsOffset = Object.transform.position - ObjectBounds.center;
            ObjectBounds.center = DesiredPosition + BoundsOffset + Object.transform.localPosition;

            Collider[] Colliders = Physics.OverlapBox(ObjectBounds.center, ObjectBounds.extents, Object.transform.rotation);

            for (int index = 0; index < Colliders.Length; index++)
            {
                if (Colliders[index].transform.parent == Parent)
                    Colliders[index] = null;
            }

            for (int index = 0; index < Colliders.Length; index++)
            {
                Bounds bounds = new Bounds();

                if (Colliders[index].GetComponent<Renderer>())
                    bounds = Colliders[index].GetComponent<Renderer>().bounds;

                else if (Colliders[index].GetComponentInChildren<Renderer>())
                    bounds = Colliders[index].GetComponentInChildren<Renderer>().bounds;

                else if (Colliders[index].GetComponentInParent<Renderer>())
                    bounds = Colliders[index].GetComponentInParent<Renderer>().bounds;

                if (bounds != null && Colliders[index].gameObject != Parent)
                    if (bounds.Intersects(ObjectBounds) && Colliders[index].gameObject != Object)
                    {
                        return true;
                    }

            }

            return false;
        }

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

        //Returns the index number of the highest priority
        public static bool GetHighestSpawnPriority(SpawnAbleObject[] Objects, out int ObjectIndex)
        {
            List<SpawnAbleObject> SpawnableObjects = new List<SpawnAbleObject>();

            ObjectIndex = 0;

            for(int i = 0; i < Objects.Length; i++)
            {
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
            Debug.Log("Returned false : " + CollectiveWeight);
            
            return false;
        }
    }
}

