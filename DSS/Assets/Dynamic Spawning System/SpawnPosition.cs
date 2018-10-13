using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DDS
{
    public class SpawnPosition : MonoBehaviour
    {
        public bool UseYAxis;

        private int Layer;

        void Start()
        {
            Layer = 1 << LayerMask.NameToLayer("IgnoredSpawnAreaObjects");
        }

        /// <summary>
        /// Returns the Spawn position of this Object
        /// </summary>
        public Vector3 GetSpawnPosition
        {
            get
            {
                if (UseYAxis)
                {
                    return transform.position;
                }

                else
                {
                    return new Vector3(transform.position.x, 0, transform.position.z);
                }

            }
        }

        /// <summary>
        /// Set FrustumCamera to null if you don't want the Frustum Check.
        /// Returns false if it couldn't allocate the desired amount of positions.
        /// </summary>
        public bool GetCheckedSpawnPosition(SpawnAbleObject Object, Camera FrustumCamera, out Vector3 ReturnedPosition)
        {
            PersonalLogicScript PersonalScript = Object.ObjectToSpawn.GetComponent<PersonalLogicScript>();

            bool UsePersonalLogic = false;

            if (PersonalScript != null)
            {
                UsePersonalLogic = true;
            }

            ReturnedPosition = new Vector3();

            ReturnedPosition.x = transform.position.x;
            ReturnedPosition.z = transform.position.z;

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

            if(!UsePersonalLogic)
            {
                RaycastHit Hit = new RaycastHit();

                if (!Physics.BoxCast(new Vector3(transform.position.x, transform.position.y + Object.ObjectToSpawn.GetComponent<Renderer>().bounds.size.y / 2 + Object.AdaptableSpawnHeight, transform.position.z) + CenterOffset, ObjectBounds.extents, Vector3.down, out Hit, Object.ObjectToSpawn.transform.rotation, 100 + Object.AdaptableSpawnHeight, ~Layer))
                {
                    Debug.Log("<color=red> No ground detected, please readjust your Spawn Point height </color>");
                    return false;
                }

                float Distance = 0;

                if (Hit.point.y + ObjectBounds.size.y / 2 < transform.position.y)
                {
                    Distance = Hit.point.y + ObjectBounds.size.y / 2 - transform.position.y + ObjectBounds.size.y / 2;

                    if (Distance < 0)
                        Distance *= -1;
                }




                Collider[] OverlapingColliders = Physics.OverlapBox(new Vector3(transform.position.x, Hit.point.y + ObjectBounds.size.y / 2, transform.position.z) + CenterOffset, ObjectBounds.extents);

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
                    ReturnedPosition.y = Hit.point.y + Object.ObjectToSpawn.GetComponent<Renderer>().bounds.size.y / 2;
                }

                else
                    return false;

                if (FrustumCamera != null)
                {
                    if (SpawningFunctions.IsVisible(FrustumCamera, Object.ObjectToSpawn, ReturnedPosition))
                        return false;

                    else if (Object.ApplyLogicToChilds)
                        if (SpawningFunctions.IsAnyChildVisible(Object.ObjectToSpawn, ReturnedPosition, FrustumCamera))
                            return false;
                }

            }

            else
            {
                RaycastHit Hit = new RaycastHit();

                if (!Physics.BoxCast(new Vector3(transform.position.x, transform.position.y + Object.ObjectToSpawn.GetComponent<Renderer>().bounds.size.y / 2 + Object.AdaptableSpawnHeight, transform.position.z) + CenterOffset, ObjectBounds.extents, Vector3.down, out Hit, Object.ObjectToSpawn.transform.rotation, 100 + Object.AdaptableSpawnHeight, ~Layer))
                {
                    Debug.Log("<color=red> No ground detected, please readjust your Spawn Point height </color>");
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
                    ReturnedPosition.y = Hit.point.y + Object.ObjectToSpawn.GetComponent<Renderer>().bounds.size.y / 2;
                }

                else
                    return false;
            }

            return true;
        }
    }

}

