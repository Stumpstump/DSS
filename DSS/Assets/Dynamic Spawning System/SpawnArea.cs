using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DDS
{ 
    public class SpawnArea : MonoBehaviour
    {
        public LayerMask IgnoredRoofObjects;

        private int Layer;

        void Start()
        {
            Layer = 1 << LayerMask.NameToLayer("IgnoredSpawnAreaObjects");
        }

        /// <summary>
        /// Returns a random Point in the Area of the boundings
        /// </summary>
        public Vector3 GetRandomPosition
        {
            get
            {
                float MinZ, MinX, MaxX, MaxZ;

                MaxX = transform.position.x + GetComponent<MeshCollider>().bounds.extents.x;
                MinX = transform.position.x - GetComponent<MeshCollider>().bounds.extents.x;

                MaxZ = transform.position.z + GetComponent<MeshCollider>().bounds.extents.z;
                MinZ = transform.position.z - GetComponent<MeshCollider>().bounds.extents.z;                

                return new Vector3(Random.Range(MinX, MaxX), 0, Random.Range(MinZ, MaxZ));
            }
        }


        /// <summary>
        /// Set FrustumCamera to null if you don't want the Frustum Check.
        /// Returns false if it couldn't allocate the desired amount of positions.
        /// </summary>
        public bool GetRandomCheckedPositions(SpawnAbleObject Object, int DesiredAmountOfPositions, Camera FrustumCamera, out Vector3[] ReturnedPositions)
        {

            PersonalLogicScript PersonalScript = Object.ObjectToSpawn.GetComponent<PersonalLogicScript>();

            bool UsePersonalLogic = false;

            if (PersonalScript != null)
            {
                UsePersonalLogic = true;
            }


            Bounds ObjectBounds = Object.ObjectToSpawn.GetComponent<Renderer>().bounds;

            Vector3 CenterOffset = new Vector3();

            if (Object.ApplyLogicToChilds)
            {
                foreach (Renderer renderer in Object.ObjectToSpawn.GetComponentsInChildren<Renderer>())
                {
                    ObjectBounds.Encapsulate(renderer.bounds);
                }
            }

            CenterOffset = ObjectBounds.center - Object.ObjectToSpawn.transform.position;


            float AreaWidth, AreaLength, ObjectWidth, ObjectLength;

            ReturnedPositions = new Vector3[0];

            Vector3 AreaTopRightPosition = new Vector3();

            AreaTopRightPosition.x = transform.position.x - GetComponent<MeshCollider>().bounds.extents.x;
            AreaTopRightPosition.z = transform.position.z - GetComponent<MeshCollider>().bounds.extents.z;

            AreaWidth = GetComponent<MeshCollider>().bounds.size.x;
            AreaLength = GetComponent<MeshCollider>().bounds.size.z;


            ObjectWidth = ObjectBounds.size.x;
            ObjectLength = ObjectBounds.size.z;


            float ContainableSizeWidth = (AreaWidth / ObjectWidth);
            float ContainableSizeHeight = (AreaLength / ObjectLength);


            int ContainableAreaSize = (int)(ContainableSizeHeight * ContainableSizeWidth);

            Vector3[] Positions = new Vector3[ContainableAreaSize];

            Vector3 LastPosition = new Vector3();

            LastPosition.x = transform.position.x - GetComponent<MeshCollider>().bounds.size.x;
            LastPosition.x += ObjectWidth / 2;

            LastPosition.z = transform.position.z - GetComponent<MeshCollider>().bounds.size.z;
            LastPosition.z += ObjectLength / 2;

            int CurrentRow = 1, CurrentColumn = 1;

            Positions[0] = LastPosition;

            for (int i = 1; i < ContainableAreaSize; i++)
            {
                if(CurrentColumn > ContainableSizeWidth)
                {
                    CurrentColumn = 1;
                    CurrentRow += 1; 
                }

                Vector3 CurrentPosition = new Vector3();


                if(CurrentColumn == 1)
                {
                    CurrentPosition.x = AreaTopRightPosition.x + ObjectWidth / 2;
                }

                else
                {
                    CurrentPosition.x = AreaTopRightPosition.x + ObjectWidth / 2 + (CurrentColumn - 1) * ObjectWidth;               
                }

                if(CurrentRow == 1)
                {
                    CurrentPosition.z = AreaTopRightPosition.z + ObjectLength / 2;
                }

                else
                {
                    CurrentPosition.z = AreaTopRightPosition.z + ObjectLength / 2 + (CurrentRow - 1) * ObjectLength;
                }

                CurrentColumn++;

                Positions[i] = CurrentPosition;
            }

            List<Vector3> SpawnAblePositions = new List<Vector3>();

            List<Vector3> IndexOfObjectsToRemove = new List<Vector3>();

            if (!UsePersonalLogic && (SpawningFunctions.IsTriggerSpawn && SpawningFunctions.Trigger_Spawn_Overrides_Logic))
            {
                Debug.Log(SpawningFunctions.IsTriggerSpawn);
                Debug.Log(SpawningFunctions.Trigger_Spawn_Overrides_Logic);
                for (int i = 0; i < Positions.Length; i++)
                {
                    RaycastHit Hit;

                    if (!Physics.BoxCast(new Vector3(Positions[i].x, transform.position.y + Object.ObjectToSpawn.GetComponent<Renderer>().bounds.size.y / 2 + Object.AdaptableSpawnHeight, Positions[i].z) + CenterOffset, ObjectBounds.extents, Vector3.down, out Hit, Object.ObjectToSpawn.transform.rotation, 100 + Object.AdaptableSpawnHeight, ~Layer))
                    {
                        Debug.Log("<color=red> No ground detected, please readjust your Spawn Area height </color>");
                        return false;
                    }

                    float Distance = 0;

                    if (Hit.point.y + ObjectBounds.size.y / 2 < transform.position.y)
                    {
                        Distance = Hit.point.y + ObjectBounds.size.y / 2 - transform.position.y + ObjectBounds.size.y / 2;

                        if (Distance < 0)
                            Distance *= -1;
                    }

                    Collider[] OverlapingColliders = Physics.OverlapBox(new Vector3(Positions[i].x, Hit.point.y + ObjectBounds.size.y / 2, Positions[i].z) + CenterOffset, ObjectBounds.extents);

                    List<Collider> OverlappingColliderList = new List<Collider>(OverlapingColliders);

                    bool DoDeletePosition = false;

                    for (int a = 0; a < OverlappingColliderList.Count; a++)
                    {
                        if (OverlappingColliderList[a].gameObject != transform.gameObject && OverlappingColliderList[a].gameObject != Hit.transform.gameObject)
                        {
                            DoDeletePosition = true;
                        }

                    }

                    if (Distance < Object.AdaptableSpawnHeight && !DoDeletePosition)
                    {
                        Positions[i].y = Hit.point.y + Object.ObjectToSpawn.GetComponent<Renderer>().bounds.size.y / 2;
                        SpawnAblePositions.Add(Positions[i]);
                    }

                    if (FrustumCamera != null)
                    {

                        if (SpawningFunctions.IsVisible(FrustumCamera, Object.ObjectToSpawn, SpawnAblePositions[i]))
                            IndexOfObjectsToRemove.Add(SpawnAblePositions[i]);


                        else if (Object.ApplyLogicToChilds)
                            if (SpawningFunctions.IsAnyChildVisible(Object.ObjectToSpawn, SpawnAblePositions[i], FrustumCamera))
                                IndexOfObjectsToRemove.Add(SpawnAblePositions[i]);


                    }                       
                }
            }

            

            else
            {
                Debug.Log("NEIN");
                for (int i = 0; i < Positions.Length; i++)
                {
                    RaycastHit Hit;

                    if (!Physics.BoxCast(new Vector3(Positions[i].x, transform.position.y + Object.ObjectToSpawn.GetComponent<Renderer>().bounds.size.y / 2 + Object.AdaptableSpawnHeight, Positions[i].z) + CenterOffset, ObjectBounds.extents, Vector3.down, out Hit, Object.ObjectToSpawn.transform.rotation, 100 + Object.AdaptableSpawnHeight, ~Layer))
                    {
                        Debug.Log("<color=red> No ground detected, please readjust your Spawn Area height </color>");
                        return false;
                    }

                    float Distance = 0;

                    if (Hit.point.y + ObjectBounds.size.y / 2 < transform.position.y)
                    {
                        Distance = Hit.point.y + ObjectBounds.size.y / 2 - transform.position.y + ObjectBounds.size.y / 2;

                        if (Distance < 0)
                            Distance *= -1;
                    }


                    if (Distance < Object.AdaptableSpawnHeight)
                    {
                        Positions[i].y = Hit.point.y + Object.ObjectToSpawn.GetComponent<Renderer>().bounds.size.y / 2;
                    }

                    else
                        return false;


                    SpawnAblePositions.Add(Positions[i]);

                }
            }

            for (int i = 0; i < IndexOfObjectsToRemove.Count; i++)
            {
                SpawnAblePositions.Remove(IndexOfObjectsToRemove[i]);
            }





            if (SpawnAblePositions.Count < DesiredAmountOfPositions)
                return false;

            int MaxLoops = DesiredAmountOfPositions * 2;

            int Loop = 0;

            List<Vector3> BufferList = new List<Vector3>();

            for (int i = 0; i < DesiredAmountOfPositions; i++)
            {
                Loop++;

                if (Loop > MaxLoops)
                    return false;

                int SelectedPosition = Random.Range(0, SpawnAblePositions.Count - 1);

                bool AlreadyUsed = false;

                foreach (Vector3 Position in BufferList)
                {
                    if(Position == SpawnAblePositions[SelectedPosition])
                    {
                        AlreadyUsed = true;
                    }
                }

                if (AlreadyUsed == true)
                {
                    i--;
                }

                else
                {
                    BufferList.Add(SpawnAblePositions[SelectedPosition]);
                }
            }


            BufferList.ToArray();

            ReturnedPositions = BufferList.ToArray();

            if (ReturnedPositions.Length < DesiredAmountOfPositions)
            {
                Debug.Log("Spawn Area couldnt find enough positions to spawn!");
                return false;
            }

            return true;
        }

    }

}

