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
        static public bool UseOcclusionCulling;
        static private int MaxPositionChecks = 50;
        static public List<GameObject> FrustumIgnoredObjects;
                     

        public static GameObject SpawnObjectInArea(SpawnArea Area, GameObject DesiredObject, bool UseAreaHeight)
        {
            Vector3 Position = GetAreaPosition(Area, DesiredObject.transform.position.y, UseAreaHeight);

            int Loop = 0;

            while(IsPositionBlocked(DesiredObject, Position))
            {
                Position = GetAreaPosition(Area, DesiredObject.transform.position.y, UseAreaHeight);
                if (Loop > MaxPositionChecks)
                    return null;                
            }

            return GameObject.Instantiate(DesiredObject, Position, DesiredObject.transform.rotation);
        }

        public static GameObject SpawnObjectInArea(SpawnArea Area, GameObject DesiredObject, bool UseAreaHeight, Camera FrustumCamera)
        {
            Vector3 Position = GetAreaPosition(Area, DesiredObject.transform.position.y, UseAreaHeight);
            int Loop = 0;

            while (IsPositionBlocked(DesiredObject, Position) || IsVisible(FrustumCamera, DesiredObject, Position))
            {
                Position = GetAreaPosition(Area, DesiredObject.transform.position.y, UseAreaHeight);
                Loop++;
                if (Loop > MaxPositionChecks)
                    return null;
            }

            return GameObject.Instantiate(DesiredObject, Position, DesiredObject.transform.rotation);
        }

        public static GameObject[] SpawnWaveInArea(SpawnArea Area, GameObject DesiredObject, int SpawnAmount, bool UseAreaHeight)
        {
            GameObject[] SpawnedObjects = new GameObject[SpawnAmount];

            for(int ObjectIndex = 0; ObjectIndex < SpawnAmount; ObjectIndex++)
            {
                Vector3 Position = GetAreaPosition(Area, DesiredObject.transform.position.y, UseAreaHeight);

                int Loop = 0;

                while(IsPositionBlocked(DesiredObject, Position))
                {
                    Loop++;
                    if (Loop > MaxPositionChecks)
                    {
                        for (int index = 0; index < SpawnedObjects.Length; index++)
                        {
                            GameObject.Destroy(SpawnedObjects[index]);
                        }

                        return null;
                    }

                    Position = GetAreaPosition(Area, DesiredObject.transform.position.y, UseAreaHeight);
                }

                SpawnedObjects[ObjectIndex] = GameObject.Instantiate(DesiredObject, Position, DesiredObject.transform.rotation);
            }
            
            return SpawnedObjects;
        }



        public static GameObject[] SpawnWaveInArea(SpawnArea Area, GameObject DesiredObject, int SpawnAmount, bool UseAreaHeight, Camera FrustumCamera)
        {
            GameObject[] SpawnedObjects = new GameObject[SpawnAmount];

            for (int ObjectIndex = 0; ObjectIndex < SpawnAmount; ObjectIndex++)
            {
                Vector3 Position = GetAreaPosition(Area, DesiredObject.transform.position.y, UseAreaHeight);

                int Loop = 0;

                while (IsPositionBlocked(DesiredObject, Position) || IsVisible(FrustumCamera, DesiredObject, Position))
                {
                    Loop++;
                    if (Loop > MaxPositionChecks)
                    {                           
                        for (int index = 0; index < SpawnedObjects.Length; index++)
                        {                        
                            GameObject.Destroy(SpawnedObjects[index]);
                        }
                        return null;
                    }

                    Position = GetAreaPosition(Area, DesiredObject.transform.position.y, UseAreaHeight);
                }
                SpawnedObjects[ObjectIndex] = GameObject.Instantiate(DesiredObject, Position, DesiredObject.transform.rotation);
            }

            return SpawnedObjects;
        }

        public static GameObject SpawnPriorityObjectInArea(SpawnArea Area, SpawnAbleObject[] Objects, bool UseAreaHeight, Camera FrustumCamera)
        {
            int IndexOfObject = GetHighestSpawnPriority(Objects);

            Vector3 Position = GetAreaPosition(Area, Objects[IndexOfObject].ObjectToSpawn.transform.position.y, UseAreaHeight);

            int Loop = 0;

            bool ChildIsBlockedOrVisible = false;

            if (Objects[IndexOfObject].ApplyLogicToChilds)
            {
                for (int index = 0; index < Objects[IndexOfObject].ObjectToSpawn.transform.childCount; index++)
                {
                    if (IsPositionBlockedChild(Objects[IndexOfObject].ObjectToSpawn.transform.GetChild(index).gameObject, Objects[IndexOfObject].ObjectToSpawn, Position) || IsVisibleChild(FrustumCamera, Objects[IndexOfObject].ObjectToSpawn.transform.GetChild(index).gameObject, Objects[IndexOfObject].ObjectToSpawn, Position))
                    {
                        ChildIsBlockedOrVisible = true;
                        break;
                    }
                }
            }


            while (IsPositionBlocked(Objects[IndexOfObject].ObjectToSpawn, Position) || IsVisible(FrustumCamera, Objects[IndexOfObject].ObjectToSpawn, Position) || ChildIsBlockedOrVisible)
            {
                ChildIsBlockedOrVisible = false;
                Loop++;
                Position = GetAreaPosition(Area, Objects[IndexOfObject].ObjectToSpawn.transform.position.y, UseAreaHeight);

                for (int index = 0; index < Objects[IndexOfObject].ObjectToSpawn.transform.childCount; index++)
                {
                    if (IsPositionBlockedChild(Objects[IndexOfObject].ObjectToSpawn.transform.GetChild(index).gameObject, Objects[IndexOfObject].ObjectToSpawn, Position) || IsVisibleChild(FrustumCamera, Objects[IndexOfObject].ObjectToSpawn.transform.GetChild(index).gameObject, Objects[IndexOfObject].ObjectToSpawn, Position))
                    {
                        ChildIsBlockedOrVisible = true;
                        break;
                    }
                }
                if (Loop > MaxPositionChecks)
                    return null;
            }

            return GameObject.Instantiate(Objects[IndexOfObject].ObjectToSpawn, Position, Objects[IndexOfObject].ObjectToSpawn.transform.rotation);
        }

        public static GameObject SpawnPriorityObjectInArea(SpawnArea Area, SpawnAbleObject[] Objects, bool UseAreaHeight)
        {
            int IndexOfObject = GetHighestSpawnPriority(Objects);

            Vector3 Position = GetAreaPosition(Area, Objects[IndexOfObject].ObjectToSpawn.transform.position.y, UseAreaHeight);

            int Loop = 0;

            bool ChildIsBlockedOrVisible = false;

            if (Objects[IndexOfObject].ApplyLogicToChilds)
            {
                for (int index = 0; index < Objects[IndexOfObject].ObjectToSpawn.transform.childCount; index++)
                {
                    if (IsPositionBlockedChild(Objects[IndexOfObject].ObjectToSpawn.transform.GetChild(index).gameObject, Objects[IndexOfObject].ObjectToSpawn, Position))
                    {
                        ChildIsBlockedOrVisible = true;
                        break;
                    }
                }
            }


            while (IsPositionBlocked(Objects[IndexOfObject].ObjectToSpawn, Position) || ChildIsBlockedOrVisible)
            {
                ChildIsBlockedOrVisible = false;
                Loop++;
                Position = GetAreaPosition(Area, Objects[IndexOfObject].ObjectToSpawn.transform.position.y, UseAreaHeight);

                for (int index = 0; index < Objects[IndexOfObject].ObjectToSpawn.transform.childCount; index++)
                {
                    if (IsPositionBlockedChild(Objects[IndexOfObject].ObjectToSpawn.transform.GetChild(index).gameObject, Objects[IndexOfObject].ObjectToSpawn, Position))
                    {
                        ChildIsBlockedOrVisible = true;
                        break;
                    }
                }
                if (Loop > MaxPositionChecks)
                    return null;
            }

            return GameObject.Instantiate(Objects[IndexOfObject].ObjectToSpawn, Position, Objects[IndexOfObject].ObjectToSpawn.transform.rotation);
        }

        public static GameObject SpawnObjectAtSpawnPoint(SpawnPosition SpawnPoint, GameObject DesiredObject, bool UsePointHeight)
        {

            Vector3 Position = GetSpawnPoint(SpawnPoint, DesiredObject.transform.position.y, UsePointHeight);

            int Loop = 0;

            while (IsPositionBlocked(DesiredObject, Position))
            {
                Loop++;
                Position = GetSpawnPoint(SpawnPoint, DesiredObject.transform.position.y, UsePointHeight);
                if (Loop > MaxPositionChecks)
                    return null;
            }

            return GameObject.Instantiate(DesiredObject, Position, DesiredObject.transform.rotation);
        }

        public static GameObject SpawnObjectAtSpawnPoint(SpawnPosition SpawnPoint, GameObject DesiredObject, bool UsePointHeight, Camera FrustumCamera)
        {

            Vector3 Position = GetSpawnPoint(SpawnPoint, DesiredObject.transform.position.y, UsePointHeight);

            int Loop = 0;

            while (IsPositionBlocked(DesiredObject, Position) || IsVisible(FrustumCamera, DesiredObject, Position))
            {
                Loop++;
                Position = GetSpawnPoint(SpawnPoint, DesiredObject.transform.position.y, UsePointHeight);
                if (Loop > MaxPositionChecks)
                    return null;
            }

            return GameObject.Instantiate(DesiredObject, Position, DesiredObject.transform.rotation);
        }

        public static GameObject SpawnPriorityObjectAtSpawnPoint(SpawnPosition SpawnPoint, SpawnAbleObject[] Objects, bool UsePointHeight, Camera FrustumCamera)
        {
            int IndexOfObject = GetHighestSpawnPriority(Objects);

            Vector3 Position = GetSpawnPoint(SpawnPoint, Objects[IndexOfObject].ObjectToSpawn.transform.position.y, UsePointHeight);

            int Loop = 0;

            bool ChildIsBlockedOrVisible = false;

            if(Objects[IndexOfObject].ApplyLogicToChilds)
            {
                for(int index = 0; index < Objects[IndexOfObject].ObjectToSpawn.transform.childCount; index++)
                {
                    if(IsPositionBlockedChild(Objects[IndexOfObject].ObjectToSpawn.transform.GetChild(index).gameObject, Objects[IndexOfObject].ObjectToSpawn, Position) || IsVisibleChild(FrustumCamera, Objects[IndexOfObject].ObjectToSpawn.transform.GetChild(index).gameObject, Objects[IndexOfObject].ObjectToSpawn, Position))
                    {
                        ChildIsBlockedOrVisible = true;
                        break;
                    }
                }
            }


            while (IsPositionBlocked(Objects[IndexOfObject].ObjectToSpawn, Position) || IsVisible(FrustumCamera, Objects[IndexOfObject].ObjectToSpawn, Position) || ChildIsBlockedOrVisible)
            {
                ChildIsBlockedOrVisible = false;
                Loop++;
                Position = GetSpawnPoint(SpawnPoint, Objects[IndexOfObject].ObjectToSpawn.transform.position.y, UsePointHeight);

                for (int index = 0; index < Objects[IndexOfObject].ObjectToSpawn.transform.childCount; index++)
                {
                    if (IsPositionBlockedChild(Objects[IndexOfObject].ObjectToSpawn.transform.GetChild(index).gameObject, Objects[IndexOfObject].ObjectToSpawn, Position) || IsVisibleChild(FrustumCamera, Objects[IndexOfObject].ObjectToSpawn.transform.GetChild(index).gameObject, Objects[IndexOfObject].ObjectToSpawn, Position))
                    {
                        ChildIsBlockedOrVisible = true;
                        break;
                    }
                }
                if (Loop > MaxPositionChecks)
                    return null;
            }

            return GameObject.Instantiate(Objects[IndexOfObject].ObjectToSpawn, Position, Objects[IndexOfObject].ObjectToSpawn.transform.rotation);
        }


        public static GameObject SpawnPriorityObjectAtSpawnPoint(SpawnPosition SpawnPoint, SpawnAbleObject[] Objects, bool UsePointHeight)
        {
            int IndexOfObject = GetHighestSpawnPriority(Objects);

            Vector3 Position = GetSpawnPoint(SpawnPoint, Objects[IndexOfObject].ObjectToSpawn.transform.position.y, UsePointHeight);

            int Loop = 0;

            bool ChildBlocked = false;

            if (Objects[IndexOfObject].ApplyLogicToChilds)
            {
                for (int Child = 0; Child < Objects[IndexOfObject].ObjectToSpawn.transform.childCount; Child++)
                {
                    if (IsPositionBlockedChild(Objects[IndexOfObject].ObjectToSpawn.transform.GetChild(Child).gameObject, Objects[IndexOfObject].ObjectToSpawn, Position))
                    {
                        ChildBlocked = true;
                    }
                }
            }

            while (IsPositionBlocked(Objects[IndexOfObject].ObjectToSpawn, Position) || ChildBlocked)
            {
                Loop++;
                Position = GetSpawnPoint(SpawnPoint, Objects[IndexOfObject].ObjectToSpawn.transform.position.y, UsePointHeight);

                ChildBlocked = false;

                if (Objects[IndexOfObject].ApplyLogicToChilds)
                {
                    for (int Child = 0; Child < Objects[IndexOfObject].ObjectToSpawn.transform.childCount; Child++)
                    {
                        if (IsPositionBlockedChild(Objects[IndexOfObject].ObjectToSpawn.transform.GetChild(Child).gameObject, Objects[IndexOfObject].ObjectToSpawn, Position))
                        {
                            ChildBlocked = true;
                        }
                    }
                }

                if (Loop > MaxPositionChecks)
                    return null;
            }

            return GameObject.Instantiate(Objects[IndexOfObject].ObjectToSpawn, Position, Objects[IndexOfObject].ObjectToSpawn.transform.rotation);
        }

        private static Vector3 GetSpawnPoint(SpawnPosition SpawnPoint, float ObjectHeight, bool UsePointHeight)
        {
            Vector3 Position = SpawnPoint.GetSpawnPosition;
            if (!UsePointHeight)
                Position.y = ObjectHeight;

            return Position;
        }

        private static Vector3 GetAreaPosition(SpawnArea Area, float ObjectHeight, bool UseAreaHeight)
        {
            Vector3 Position = Area.GetRandomPosition;
            if (!UseAreaHeight)
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
        public static int GetHighestSpawnPriority(SpawnAbleObject[] Objects)
        {
            float[] SpawnRangeMin = new float[Objects.Length];
            float[] SpawnRangeMax = new float[Objects.Length];


            float CollectiveWeight = 0;

            foreach (SpawnAbleObject SpawnableObject in Objects)
            {
                CollectiveWeight += SpawnableObject.ChanceToSpawn;
            }

            for(int index= 0; index < Objects.Length; index++)
            {
                float SpawnMin = 0;
                float SpawnMax = 0;

                if(index != 0)                
                    SpawnMin = SpawnRangeMax[index - 1];

                SpawnMax = SpawnMin + (Objects[index].ChanceToSpawn / CollectiveWeight * 100);

                SpawnRangeMin[index] = SpawnMin;
                SpawnRangeMax[index] = SpawnMax;
            }


            int RandomNumber = UnityEngine.Random.Range(1, 100);

            for(int index = 0; index < SpawnRangeMin.Length; index++)
            {
                if(RandomNumber >= SpawnRangeMin[index] && RandomNumber <= SpawnRangeMax[index])
                {
                    return index;
                }
            }

            Debug.Log("<color=blue> Highest Priority not in range </color>");
            return 0;
        }
    }


}
