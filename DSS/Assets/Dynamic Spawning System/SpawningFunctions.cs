using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEditor;

namespace DDS
{
    public static class SpawningFunctions
    {
        static private int MaxPositionChecks = 50;

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
                    {
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

    }

    
}
