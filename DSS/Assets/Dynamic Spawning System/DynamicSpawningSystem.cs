using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DDS
{
    public class DynamicSpawningSystem : MonoBehaviour
    {
        #region Public Fields


        /// <summary>
        /// TestSpawnCheckings
        /// </summary>
        public SpawnSettings TestSpawnSettings;

        /// <summary>
        /// Test object to check the Distance of
        /// </summary>
        public GameObject ObjectToCheck;

        /// <summary>
        /// How to check the test Objects Distance
        /// </summary>
        public DistanceCheckingStyles ObjectDistanceCheck;

        /// <summary>
        /// Visual reprensation if the object is in range
        /// </summary>
        public bool InRange = false;

        /// <summary>
        /// Range to check for
        /// </summary>
        public float RangeToCheck;

        /// <summary>
        /// Range to check for
        /// </summary>
        public float TriggerRange;

        #endregion



        #region Private Fields

        /// <summary>
        /// Sphere to check for the player
        /// Requires at least one object to have a rigidbody
        /// </summary>
        SphereCollider sphereToCheckForThePlayer;

        float TestTimeIntervalCounter;

        #endregion

        void Start()
        {
            if (!ObjectToCheck)
            {
                Debug.Log("Object to check is null");
            }

            if (ObjectDistanceCheck == 0)
            {
                Debug.Log("Object Distance to check wasnt assigned so its 1");
                ObjectDistanceCheck = DistanceCheckingStyles.TwoDimensionalCheck;
            }

            sphereToCheckForThePlayer = gameObject.GetComponent<SphereCollider>();

            sphereToCheckForThePlayer.radius = TriggerRange;
            sphereToCheckForThePlayer.transform.position = transform.position;
            
        }

        // Update is called once per frame
        void Update()
        {
            TestTimeIntervalCounter += Time.deltaTime;

            switch (ObjectDistanceCheck)
            {
                case DistanceCheckingStyles.TwoDimensionalCheck:
                    InRange = DistanceChecking.TwoDimensionalCheck(transform, ObjectToCheck.transform, RangeToCheck);
                    break;

                case DistanceCheckingStyles.ThreeDimensionalCheck:
                    InRange = DistanceChecking.ThreeDimensionalCheck(transform, ObjectToCheck.transform, RangeToCheck);
                    break;
            }

            if(TestSpawnSettings.SpawnIfInRange && InRange || !TestSpawnSettings.SpawnIfInRange)
            {
                if(TestTimeIntervalCounter >= TestSpawnSettings.SpawnDelay)
                {
                    TestTimeIntervalCounter = 0f;
                    Debug.Log("SpawnObject");
                }
            }
        }

        void OnTriggerEnter(Collider collider)
        {
            if(ObjectDistanceCheck == DistanceCheckingStyles.SphereColliderCheck)
                if (collider.gameObject == ObjectToCheck)
                    InRange = true;
        }

        void OnTriggerExit(Collider collider)
        {
            if (ObjectDistanceCheck == DistanceCheckingStyles.SphereColliderCheck)
                if (collider.gameObject == ObjectToCheck)
                    InRange = false;
        }
 
    }

    [CustomEditor(typeof(DynamicSpawningSystem))]
    public class DynamicScriptEditor : Editor
    {

        bool DoShowTestGameSettingsContent = true;
        bool DoShowRangeSettings = false;

        override public void OnInspectorGUI()
        {
            var DynamicSpawned = target as DynamicSpawningSystem;

           
        
            DoShowTestGameSettingsContent = EditorGUILayout.Foldout(DoShowTestGameSettingsContent, new GUIContent("Test Game Settings"));
            if(DoShowTestGameSettingsContent)
            {
                EditorGUI.indentLevel++;
                DynamicSpawned.TestSpawnSettings.SpawnDelay = EditorGUILayout.FloatField(new GUIContent("Spawn Delay", "In Seconds"), DynamicSpawned.TestSpawnSettings.SpawnDelay);

                if (DynamicSpawned.TestSpawnSettings.SpawnDelay < 0f)
                    DynamicSpawned.TestSpawnSettings.SpawnDelay *= -1;
            
                DynamicSpawned.TestSpawnSettings.SpawnIfInRange = EditorGUILayout.Toggle(new GUIContent("Spawn if in Range", "Spawn the Object only if the Player is in range"), DynamicSpawned.TestSpawnSettings.SpawnIfInRange);

                if(DynamicSpawned.TestSpawnSettings.SpawnIfInRange)
                {
                    EditorGUI.indentLevel++;

                    Object obj = DynamicSpawned.ObjectToCheck;

                    DynamicSpawned.ObjectToCheck = (GameObject)EditorGUILayout.ObjectField("Player:", DynamicSpawned.ObjectToCheck, typeof(GameObject), true);

                    GUIContent[] DistanceCheckingOptionsDescription = new GUIContent[3];

                    DistanceCheckingOptionsDescription[0] = new GUIContent("TwoDimensionalCheck", "Checks the Distance by the X and Z axis");
                    DistanceCheckingOptionsDescription[1] = new GUIContent("ThreeDimensionalCheck", "Checks the Distance by the X, y and Z axis");
                    DistanceCheckingOptionsDescription[2] = new GUIContent("SphereCheck", "Checks if the Player is in the Sphere rather than the pure Distance");

                    string[] Options = { "TwoDimensionalCheck", "ThreeDimensionalCheck", "SphereCheck" };

                    //GUILayoutOption[]

                    DynamicSpawned.ObjectDistanceCheck = (DistanceCheckingStyles)EditorGUILayout.Popup(new GUIContent("Check Style", "How to check the Range"),(int)DynamicSpawned.ObjectDistanceCheck, Options);

                    if(DynamicSpawned.ObjectDistanceCheck == DistanceCheckingStyles.TwoDimensionalCheck || DynamicSpawned.ObjectDistanceCheck == DistanceCheckingStyles.ThreeDimensionalCheck)
                    {
                        EditorGUI.indentLevel++;
                        DynamicSpawned.RangeToCheck = EditorGUILayout.FloatField(new GUIContent("Range: "), DynamicSpawned.RangeToCheck);
                    }

                    else if(DynamicSpawned.ObjectDistanceCheck == DistanceCheckingStyles.SphereColliderCheck)
                    {
                        EditorGUI.indentLevel++;

                        if (DynamicSpawned.gameObject.GetComponent<SphereCollider>() == null)
                        {
                            DynamicSpawned.gameObject.AddComponent<SphereCollider>();
                        }

                        DynamicSpawned.gameObject.GetComponent<SphereCollider>().isTrigger = true;

                        DynamicSpawned.gameObject.GetComponent<SphereCollider>().radius = EditorGUILayout.FloatField(new GUIContent("Sphere Radius: "), DynamicSpawned.gameObject.GetComponent<SphereCollider>().radius);

                        DynamicSpawned.gameObject.GetComponent<SphereCollider>().hideFlags = HideFlags.HideInInspector;
                    }

                    if(DynamicSpawned.ObjectDistanceCheck != DistanceCheckingStyles.SphereColliderCheck)
                    {
                        DestroyImmediate(DynamicSpawned.gameObject.GetComponent<SphereCollider>());
                    }
                }
            }
        }
    }

}
