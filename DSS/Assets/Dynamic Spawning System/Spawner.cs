using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace DDS
{
    public class Spawner : MonoBehaviour
    {
        public GameObject spawnArea;

        public List<GameObject> ignoredObjects = new List<GameObject>();

        [SerializeField] private ContiniousWaveStatus currentContiniousWaveStatus;

        [SerializeField] private int selectedSpawnPosition;

        [SerializeField] private int waveSpawnAmount;

        [SerializeField] private bool useOcclusionCulling;

        [SerializeField] private bool triggerSpawn;

        [SerializeField] private bool showIgnoredObjects;

        [SerializeField] private bool triggerSpawnOverridesLogic;

        [SerializeField] private int maximalSpawnedObjectsAlive;

        [SerializeField] private float spawnDelay;

        [SerializeField] private float rangeToCheck;

        [SerializeField] private bool doSpawnIfNotInRange;

        [SerializeField] private bool doSpawnContinuousWaves;

        [SerializeField] private bool checkFrustum;

        [SerializeField] private bool doLimitObjectsAlive;

        private bool isNotInRange;

        private SpawnedObjectContainer spawnedObjects = new SpawnedObjectContainer();

        [SerializeField] private List<SpawningComponent> SpawnPositions;

        [SerializeField] private GameObject Player;

        [SerializeField] private Camera FrustumCamera;

        [SerializeField] private PositioningOptions selectedSpawnPositionOption;

        [SerializeField] private SpawningStyles SelectedSpawningStyle;

        [SerializeField] private IdentifyPlayer SelectedPlayerIdentification;

        [SerializeField] private DistanceCheckingStyles SelectedDistanceCheck;

        [SerializeField] private Identification PlayerIdentificationData;

        private SpawningFunction SelectedSpawningFunction;

        private float SpawnInterval;        

        private SpawningComponent PositioningComponent = null;

        delegate GameObject[] SpawningFunction(Component PositioningComponent, Camera FrustumCamera);

        void Awake()
        {
            this.InitializeSpawnPositions();
            this.InitializeObjectToCheck();

            SpawningFunctions.waveSpawnAmount = waveSpawnAmount;
        }

        void Update()
        {           
            SpawningFunctions.triggerSpawnOverridesLogic = triggerSpawnOverridesLogic;
            SpawningFunctions.useOcclusionCulling = useOcclusionCulling;          

            spawnedObjects.Update();
            
            if(doSpawnIfNotInRange)
                this.UpdateDistance();

            SpawnInterval += Time.deltaTime;

            PositioningComponent = null;

            switch (SelectedSpawningStyle)
            {
                case SpawningStyles.Wave:
                    PositioningComponent = GetComponentInChildren<spawnArea>();
                    break;

                case SpawningStyles.Continuous:
                    if (selectedSpawnPositionOption == PositioningOptions.Area)
                    {
                        PositioningComponent = GetComponentInChildren<spawnArea>();
                    }

                    else
                    {
                        PositioningComponent = SpawnPositions[selectedSpawnPosition];
                    }
                    break;
            }

            if (!PositioningComponent)
                Debug.LogError("There was no Positioning Component of the Selected Positioning option found");


            Camera camera = null;

            if (checkFrustum)
                camera = FrustumCamera;

            if (IsSpawningAllowed())
            {
                triggerSpawn = false;
                GameObject[] ReturnedObjects = SpawningFunctions.Spawn(PositioningComponent, camera, SelectedSpawningStyle);
                if (ReturnedObjects != null)
                {
                    spawnedObjects.AddObjects(ReturnedObjects);
                    SpawnInterval = 0f;
                }
            }
        }

        /// <summary>
        /// Checks if Spawning is allowed.
        /// </summary>
        /// <returns></returns>
        bool IsSpawningAllowed()
        {
            if(!triggerSpawnOverridesLogic || (!triggerSpawn && triggerSpawnOverridesLogic))
            {
                int DesiredObjectAmount = 1;

                if(SelectedSpawningStyle == SpawningStyles.Wave)
                    DesiredObjectAmount = SpawningFunctions.waveSpawnAmount;
            
                if(SelectedSpawningStyle == SpawningStyles.Wave && doSpawnContinuousWaves)
                {
                    if (spawnedObjects.Size > 0 || currentContiniousWaveStatus == ContiniousWaveStatus.Stopped || doSpawnIfNotInRange && !isNotInRange)
                        return false;
                }         

                else if(SpawnInterval < spawnDelay || doSpawnIfNotInRange && !isNotInRange || (doLimitObjectsAlive && maximalSpawnedObjectsAlive < DesiredObjectAmount + spawnedObjects.Size))            
                    return false;
            }
            return true;            
        }

        /// <summary>
        /// Updates the boolean isNotInRange based on the distance of the PlayerObject and the rangeToCheck.
        /// </summary>
        void UpdateDistance()
        {
            if (doSpawnIfNotInRange && Player)
            {
                switch (SelectedDistanceCheck)
                {
                    case DistanceCheckingStyles.TwoDimensional:
                        isNotInRange = DistanceChecking.TwoDimensional(transform, Player.transform, rangeToCheck);
                        break;

                    case DistanceCheckingStyles.ThreeDimensional:
                        isNotInRange = DistanceChecking.ThreeDimensional(transform, Player.transform, rangeToCheck);
                        break;
                }
            }
        }


        /// <summary>
        /// Used for the Sphere distance checking. 
        /// </summary>
        /// <param name="collider"></param>
        void OnTriggerEnter(Collider collider)
        {
            if (SelectedDistanceCheck == DistanceCheckingStyles.Sphere)
                if (collider.gameObject == Player)
                    isNotInRange = false;
        }

        /// <summary>
        /// Used for the Sphere distance. 
        /// </summary>
        /// <param name="collider"></param>
        void OnTriggerExit(Collider collider)
        {
            if (SelectedDistanceCheck == DistanceCheckingStyles.Sphere)
                if (collider.gameObject == Player)
                    isNotInRange = true;

        }

        /// <summary>
        /// Initializes the PlayerObject for the range check.
        /// </summary>
        public void InitializeObjectToCheck()
        {
            switch (SelectedPlayerIdentification)
            {
                case IdentifyPlayer.byField:
                    Player = PlayerIdentificationData.Object;
                    break;

                case IdentifyPlayer.byName:
                    Player = GameObject.Find(PlayerIdentificationData.Name);
                    break;

                case IdentifyPlayer.byTag:
                    //string Tag = UnityEditorInternal.InternalEditorUtility.tags[PlayerIdentificationData.Tag];
                    Player = GameObject.FindWithTag(PlayerIdentificationData.tag);
                    break;
            }
        }

        /// <summary>
        /// Triggers an Object spawn.
        /// </summary>
        public void TriggerSpawn()
        {
            triggerSpawn = true;
        }


        /// <summary>
        /// Changes the Wave amount of objects to Spawn.
        /// </summary>
        /// <param name="newAmount"> the new amount of objects </param>
        public void ChangeWaveAmount(int newAmount)
        {
            waveSpawnAmount = newAmount;
            SpawningFunctions.waveSpawnAmount = waveSpawnAmount;
        }

        /// <summary>
        ///Loads all SpawnPosition Components of child objects into "spawnPositions".
        /// </summary>
        public void InitializeSpawnPositions()
        {
            SpawnPositions = new List<SpawningComponent>();          
            SpawnPositions = transform.GetComponentsInChildren<SpawnPosition>().ToList().ConvertAll(i => (SpawningComponent)i);
        }

        /// <summary>
        ///Sets the Spawn position active by the given parameter as an identifier.
        /// </summary>
        /// <param name="PositionToSet"> Index of of the spawn position </param>
        public void SetSpawnPosition(int PositionToSet)
        {
            try
            {
                if(SpawnPositions[PositionToSet])
                {
                    selectedSpawnPosition = PositionToSet;
                }
            }
            catch (System.Exception e)
            {
                Debug.Log(e.StackTrace + "" + "Position to set was out of bounds!");                
            }
        }

        /// <summary>
        ///Sets the Spawn position active by the given parameter as an identifier.
        /// </summary>
        /// <param name="PositionToSet"> Transform name of the desired position </param>
        public bool SetSpawnPosition(string PositionName)
        {
            for(int i = 0; i < SpawnPositions.Count; i++)
            {
                if(SpawnPositions[i].name == PositionName)
                {
                    selectedSpawnPosition = i;
                    return true;
                }                
            }

            Debug.Log("Position with the name " + PositionName + " couldn't be found!");
            return false;
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(Spawner))]
    public class DynamicScriptEditor : Editor
    {
        static bool showSpawnIfNotInRangeSettings;
        static bool showFrustumSettings;

        SerializedProperty currentContiniousWaveStatus;
        SerializedProperty foldOutObjectsToSpawn;
        SerializedProperty ShowPointPositions;
        SerializedProperty ignoredObjects;
        SerializedProperty spawnDelay;
        SerializedProperty showIgnoredObjects;
        SerializedProperty waveSpawnAmount;
        SerializedProperty maximalSpawnedObjectsAlive;
        SerializedProperty rangeToCheck;
        SerializedProperty doSpawnIfNotInRange;
        SerializedProperty DoSpawnContiniousWaves;
        SerializedProperty checkFrustum;
        SerializedProperty doLimitObjectsAlive;
        SerializedProperty isNotInRange;
        SerializedProperty SpawnPositions;
        SerializedProperty Player;
        SerializedProperty ObjectToSpawn;
        SerializedProperty selectedSpawnPositionOption;
        SerializedProperty SelectedSpawningStyle;
        SerializedProperty SelectedPlayerIdentification;
        SerializedProperty SelectedDistanceCheck;
        SerializedProperty PlayerIdentificationData;
        SerializedProperty FrustumCamera;
        SerializedProperty useOcclusionCulling;
        SerializedProperty ObjectsToSpawn;
        SerializedProperty triggerSpawnOverridesLogic;
        SerializedProperty ActiveSpawnPoint;

        GUILayoutOption StandardLayout = GUILayout.Height(15);

        Spawner DynamicSpawned; 
        
        protected virtual void OnEnable()
        {
            DynamicSpawned = target as Spawner;

            ActiveSpawnPoint = this.serializedObject.FindProperty("selectedSpawnPosition");
            currentContiniousWaveStatus = this.serializedObject.FindProperty("currentContiniousWaveStatus");
            foldOutObjectsToSpawn = this.serializedObject.FindProperty("FoldoutObjectsToSpawn");
            triggerSpawnOverridesLogic = this.serializedObject.FindProperty("triggerSpawnOverridesLogic");
            ObjectsToSpawn = this.serializedObject.FindProperty("ObjectsToSpawn");
            useOcclusionCulling = this.serializedObject.FindProperty("useOcclusionCulling");
            ShowPointPositions = this.serializedObject.FindProperty("DoShowPointPositions");
            ignoredObjects = this.serializedObject.FindProperty("ignoredObjects");
            spawnDelay = this.serializedObject.FindProperty("spawnDelay");
            showIgnoredObjects = this.serializedObject.FindProperty("showIgnoredObjects");
            waveSpawnAmount = this.serializedObject.FindProperty("waveSpawnAmount");
            maximalSpawnedObjectsAlive = this.serializedObject.FindProperty("maximalSpawnedObjectsAlive");
            rangeToCheck = this.serializedObject.FindProperty("rangeToCheck");
            doSpawnIfNotInRange = this.serializedObject.FindProperty("doSpawnIfNotInRange");
            DoSpawnContiniousWaves = this.serializedObject.FindProperty("doSpawnContinuousWaves");
            checkFrustum = this.serializedObject.FindProperty("checkFrustum");
            doLimitObjectsAlive = this.serializedObject.FindProperty("doLimitObjectsAlive");
            isNotInRange = this.serializedObject.FindProperty("isNotInRange");
            SpawnPositions = this.serializedObject.FindProperty("SpawnPositions");
            Player = this.serializedObject.FindProperty("Player");
            ObjectToSpawn = this.serializedObject.FindProperty("ObjectToSpawn");
            selectedSpawnPositionOption = this.serializedObject.FindProperty("selectedSpawnPositionOption");
            SelectedSpawningStyle = this.serializedObject.FindProperty("SelectedSpawningStyle");
            SelectedPlayerIdentification = this.serializedObject.FindProperty("SelectedPlayerIdentification");
            SelectedDistanceCheck = this.serializedObject.FindProperty("SelectedDistanceCheck");
            PlayerIdentificationData = this.serializedObject.FindProperty("PlayerIdentificationData");
            FrustumCamera = this.serializedObject.FindProperty("FrustumCamera");
        }

        [MenuItem("GameObject/Dynamic Spawning System/SpawnPoint", false, 0)]
        static void AddSpawnPoint()
        {
            GameObject NewSpawnPoint = Instantiate(Resources.Load("SpawnPosition") as GameObject, Selection.activeTransform);

            NewSpawnPoint.name = "New Spawn Point";           
        }

        [MenuItem("GameObject/Dynamic Spawning System/SpawnNode", false, 0)]
        static void CreateSpawnNode()
        {
            GameObject NewSpawnNode = Instantiate(Resources.Load("Spawn Node") as GameObject);
            NewSpawnNode.name = "New Spawn Node";
        }

        bool FoldOut;

        override public void OnInspectorGUI()
        {
         
            this.serializedObject.Update();

            SpawningFunctions.triggerSpawnOverridesLogic = triggerSpawnOverridesLogic.boolValue;
                 
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(SelectedSpawningStyle, new GUIContent("Spawn Style: "), StandardLayout);

            EditorGUI.indentLevel++;

            if (SelectedSpawningStyle.intValue == (int)SpawningStyles.Continuous)
            {
                EditorGUILayout.PropertyField(spawnDelay, new GUIContent("Spawn Delay: "), StandardLayout);

                if (spawnDelay.floatValue < 0f)
                    spawnDelay.floatValue *= -1;
       
                EditorGUILayout.PropertyField(doLimitObjectsAlive, new GUIContent("Limit Object Amount: "), StandardLayout);
      
                if (doLimitObjectsAlive.boolValue)
                {             
                    EditorGUILayout.PropertyField(maximalSpawnedObjectsAlive, new GUIContent("Amount: "), StandardLayout);
              
                    if (maximalSpawnedObjectsAlive.intValue < 0)
                        maximalSpawnedObjectsAlive.intValue = 0;
                }
            }

            if (SelectedSpawningStyle.intValue == (int)SpawningStyles.Wave)
            { 
                EditorGUILayout.PropertyField(DoSpawnContiniousWaves, new GUIContent("Continious Waves: "), StandardLayout);

                if(DoSpawnContiniousWaves.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(currentContiniousWaveStatus, new GUIContent("Current Status: "), true);
                    EditorGUI.indentLevel--;
                }

                else
                {
                    EditorGUILayout.PropertyField(spawnDelay, new GUIContent("Spawn Delay: "), StandardLayout);
                }

                EditorGUILayout.PropertyField(waveSpawnAmount, new GUIContent("Amount: "), StandardLayout);

                if (waveSpawnAmount.intValue > 100)
                    waveSpawnAmount.intValue = 100;

                if (waveSpawnAmount.intValue < 1)
                    waveSpawnAmount.intValue = 1;


                SpawningFunctions.waveSpawnAmount = waveSpawnAmount.intValue;
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(doSpawnIfNotInRange, new GUIContent("Spawn if not in Range: "), StandardLayout);

            if (doSpawnIfNotInRange.boolValue)
            {
                showSpawnIfNotInRangeSettings = EditorGUILayout.Foldout(showSpawnIfNotInRangeSettings, new GUIContent("Range Settings: "), true);
            }

            else
                showSpawnIfNotInRangeSettings = false;

            if (showSpawnIfNotInRangeSettings)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(SelectedPlayerIdentification, new GUIContent("Identification: "), StandardLayout);

                switch ((IdentifyPlayer)SelectedPlayerIdentification.intValue)
                {
                    case IdentifyPlayer.byField:
                        EditorGUILayout.PropertyField(PlayerIdentificationData.FindPropertyRelative("Object"), new GUIContent("Object: "), StandardLayout);
                        break;

                    case IdentifyPlayer.byName:
                        EditorGUILayout.PropertyField(PlayerIdentificationData.FindPropertyRelative("Name"), new GUIContent("Name: "), StandardLayout);
                        break;

                    case IdentifyPlayer.byTag:
                        string[] Tags = UnityEditorInternal.InternalEditorUtility.tags;
                        PlayerIdentificationData.FindPropertyRelative("Tag").intValue = EditorGUILayout.Popup(new GUIContent("Tag: ", "How to check for the Player"), PlayerIdentificationData.FindPropertyRelative("Tag").intValue, Tags);
                        PlayerIdentificationData.FindPropertyRelative("tag").stringValue = UnityEditorInternal.InternalEditorUtility.tags[PlayerIdentificationData.FindPropertyRelative("Tag").intValue];
                        break;
                }

                EditorGUILayout.PropertyField(SelectedDistanceCheck, new GUIContent("Check Style: "), StandardLayout);

                if (SelectedDistanceCheck.intValue == (int)DistanceCheckingStyles.ThreeDimensional || SelectedDistanceCheck.intValue == (int)DistanceCheckingStyles.TwoDimensional)
                {
                    EditorGUILayout.PropertyField(rangeToCheck, new GUIContent("Range: "), StandardLayout);

                    if (DynamicSpawned.gameObject.GetComponent<SphereCollider>())
                        DestroyImmediate(DynamicSpawned.gameObject.GetComponent<SphereCollider>());
                }

                else if (SelectedDistanceCheck.intValue == (int)DistanceCheckingStyles.Sphere)
                {
                    if (DynamicSpawned.GetComponent<SphereCollider>() == null)
                    {
                        DynamicSpawned.gameObject.AddComponent<SphereCollider>();
                    }

                    DynamicSpawned.gameObject.GetComponent<SphereCollider>().isTrigger = true;

                    DynamicSpawned.gameObject.GetComponent<SphereCollider>().radius = EditorGUILayout.FloatField(new GUIContent("Sphere Radius: "), DynamicSpawned.gameObject.GetComponent<SphereCollider>().radius);

                    DynamicSpawned.gameObject.GetComponent<SphereCollider>().hideFlags = HideFlags.HideInInspector;
                }
                EditorGUI.indentLevel--;

            }

            EditorGUILayout.PropertyField(checkFrustum, new GUIContent("Check Frustum: "), StandardLayout);

            if (checkFrustum.boolValue)
                showFrustumSettings = EditorGUILayout.Foldout(showFrustumSettings, new GUIContent("Frustum Settings: "), true);

            else
                showFrustumSettings = false;

            if (showFrustumSettings)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(FrustumCamera, new GUIContent("Camera: "), StandardLayout);

                EditorGUILayout.PropertyField(useOcclusionCulling, new GUIContent("Occlusion Culling: "), StandardLayout);

                if (useOcclusionCulling.boolValue)
                {
                    EditorGUILayout.PropertyField(ignoredObjects, new GUIContent("Ignored Objects: "), true);

                    List<GameObject> IgnoredObjectList = new List<GameObject>();

                    this.InitializeignoredObjects(IgnoredObjectList);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(selectedSpawnPositionOption, new GUIContent("Positioning Component: "), StandardLayout);

            if (SelectedSpawningStyle.intValue == (int)SpawningStyles.Wave)
                selectedSpawnPositionOption.intValue = (int)PositioningOptions.Area;

            switch ((PositioningOptions)selectedSpawnPositionOption.intValue)
            {
                case PositioningOptions.Area:
                    {
                        for (int childIndex = 0; childIndex < DynamicSpawned.transform.childCount; childIndex++)
                        {
                            if (DynamicSpawned.transform.GetChild(childIndex).name == "spawnArea")
                            {
                                DynamicSpawned.spawnArea = DynamicSpawned.transform.GetChild(childIndex).gameObject; DynamicSpawned.spawnArea.GetComponent<MeshCollider>().hideFlags = HideFlags.HideInInspector;
                            }
                        }

                        if (!DynamicSpawned.spawnArea)
                        {
                            DynamicSpawned.spawnArea = Instantiate(Resources.Load("spawnArea", typeof(GameObject))) as GameObject; //Resources.Load<GameObject>("Assets /DynamicSpawningSystem/spawnArea");
                            DynamicSpawned.spawnArea.transform.SetParent(DynamicSpawned.transform);
                            DynamicSpawned.spawnArea.transform.name = "spawnArea";
                            DynamicSpawned.spawnArea.transform.localPosition = new Vector3(0, 0, 0);
                            DynamicSpawned.spawnArea.GetComponent<MeshCollider>().hideFlags = HideFlags.HideInInspector;
                            DynamicSpawned.spawnArea.GetComponent<MeshFilter>().hideFlags = HideFlags.HideInInspector;
                        }

                        break;
                    }

                case PositioningOptions.Point:
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(ActiveSpawnPoint);
                        EditorGUI.indentLevel--;
                        break;
                    }
            }

            EditorGUILayout.PropertyField(triggerSpawnOverridesLogic, new GUIContent("Trigger spawn overrides logic:"), StandardLayout);

            EditorGUI.EndChangeCheck();

            this.serializedObject.ApplyModifiedProperties();

            serializedObject.Update();


            if (GUI.changed)
                EditorUtility.SetDirty(DynamicSpawned);
        }

        void InitializeignoredObjects(List<GameObject> ignoredObjects)
        {
            for (int ObjectIndex = 0; ObjectIndex < DynamicSpawned.ignoredObjects.Count; ObjectIndex++)
            {
                if (DynamicSpawned.ignoredObjects[ObjectIndex] != null)
                {
                    if (DynamicSpawned.ignoredObjects[ObjectIndex].GetComponent<Collider>())
                        ignoredObjects.Add(DynamicSpawned.ignoredObjects[ObjectIndex]);

                    if (DynamicSpawned.ignoredObjects[ObjectIndex].transform.parent)
                        if (DynamicSpawned.ignoredObjects[ObjectIndex].transform.parent.GetComponent<Collider>())
                            ignoredObjects.Add(DynamicSpawned.ignoredObjects[ObjectIndex].transform.parent.gameObject);

                    for (int ChildrenIndex = 0; ChildrenIndex < DynamicSpawned.ignoredObjects[ObjectIndex].transform.childCount; ChildrenIndex++)
                    {
                        if (DynamicSpawned.ignoredObjects[ObjectIndex].transform.GetChild(ChildrenIndex).GetComponent<Collider>())
                            ignoredObjects.Add(DynamicSpawned.ignoredObjects[ObjectIndex].transform.GetChild(ChildrenIndex).gameObject);
                    }

                }
            }

            SpawningFunctions.FrustumignoredObjects = ignoredObjects;
        }
    }
#endif
}
