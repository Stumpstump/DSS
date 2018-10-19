using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DDS
{
    public class SpawnPosition : MonoBehaviour
    {
        [SerializeField]
        public SpawnAbleObject[] Objects_to_Spawn;

        public bool UseYAxis;

        [Tooltip("Assign all Objects the ground detection should ignore to this mask")]
        public LayerMask IgnoredSpawnObject;

        [Tooltip("Adjust this height to not collide with the roof of the room etc.")]
        public float GroundDetectionHeight;


        void Start()
        {
        }

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

            RaycastHit Hit = new RaycastHit();

            if (!Physics.BoxCast(new Vector3(transform.position.x, transform.position.y + Object.ObjectToSpawn.GetComponent<Renderer>().bounds.size.y / 2 + GroundDetectionHeight, transform.position.z) + CenterOffset, ObjectBounds.extents, Vector3.down, out Hit, Object.ObjectToSpawn.transform.rotation, GroundDetectionHeight + Object.ObjectToSpawn.GetComponent<Renderer>().bounds.size.y / 2, ~IgnoredSpawnObject))
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

            ReturnedPosition.y = Hit.point.y + Object.ObjectToSpawn.GetComponent<Renderer>().bounds.size.y / 2;
            

            if (!UsePersonalLogic)
            {
                Collider[] OverlapingColliders = Physics.OverlapBox(new Vector3(transform.position.x, Hit.point.y + ObjectBounds.size.y / 2, transform.position.z) + CenterOffset, ObjectBounds.extents);

                List<Collider> OverlappingColliderList = new List<Collider>(OverlapingColliders);


                for (int a = 0; a < OverlappingColliderList.Count; a++)
                {
                    if (OverlappingColliderList[a].gameObject != transform.gameObject && OverlappingColliderList[a].gameObject != Hit.transform.gameObject)
                    {
                        return false;
                    }

                }

                if (Distance > Object.AdaptableSpawnHeight)
                {
                    return false;
                }                                

                if (FrustumCamera != null)
                {
                    if (SpawningFunctions.IsVisible(FrustumCamera, Object.ObjectToSpawn, ReturnedPosition))
                        return false;

                    else if (Object.ApplyLogicToChilds)
                        if (SpawningFunctions.IsAnyChildVisible(Object.ObjectToSpawn, ReturnedPosition, FrustumCamera))
                            return false;
                }
            }

            return true;
        }
    }

}