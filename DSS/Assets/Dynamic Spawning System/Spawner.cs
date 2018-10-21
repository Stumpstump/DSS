using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace DDS
{
    public class Spawner : MonoBehaviour
    {
        delegate GameObject[] SpawningFunction(Component PositioningComponent, Camera FrustumCamera);

        SpawningFunction SelectedSpawningFunction;

        [SerializeField]
        public ContiniousWaveStatus CurrentContiniousWaveStatus;


        [SerializeField]
        public int WaveSpawnAmount;

        [SerializeField]
        public bool UseOcclusionCulling;

        [SerializeField]
        public List<GameObject> IgnoredObjects = new List<GameObject>();

        [SerializeField]
        public bool TriggerSpawn;

        [SerializeField]
        public bool ShowIgnoredObjects;

        [SerializeField]
        public bool TriggerSpawnOverridesLogic;

        [SerializeField]
        public int MaximalSpawnedObjectsAlive;

        [SerializeField]
        public float SpawnDelay;

        [SerializeField]
        public float RangeToCheck;

        [SerializeField]
        public bool DoSpawnIfNotInRange;

        [SerializeField]
        public bool DoSpawnContinuousWaves;

        [SerializeField]
        public bool DoSpawnInFrustum;

        [SerializeField]
        public bool DoLimitObjectsAlive;

        [SerializeField]
        public bool IsNotInRange;

        [SerializeField]
        public SpawnedObjectContainer SpawnedObjects = new SpawnedObjectContainer();

        [SerializeField]
        public List<Component> SpawnPositions;

        [SerializeField]
        public GameObject Player;

        [SerializeField]
        public GameObject SpawnArea;

        [SerializeField]
        public PositioningOptions SelectedSpawnPositionOption;

        [SerializeField]
        public SpawningStyles SelectedSpawningStyle;

        [SerializeField]
        public IdentifyPlayer SelectedPlayerIdentification;

        [SerializeField]
        public DistanceCheckingStyles SelectedDistanceCheck;

        [SerializeField]
        public Identification PlayerIdentificationData;

        [SerializeField]
        public Camera FrustumCamera;

        private float SpawnInterval;

        void Awake()
        {
            this.InitializeSpawnPositions();
            this.InitializeObjecttoCheck();
        }

        void Update()
        {
            SpawningFunctions.TriggerSpawnOverridesLogic = TriggerSpawnOverridesLogic;
            SpawningFunctions.IsTriggerSpawn = TriggerSpawn;           
            SpawningFunctions.UseOcclusionCulling = UseOcclusionCulling;

            SpawnedObjects.Update();
            
            if(DoSpawnIfNotInRange)
                this.UpdateDistance();

            this.InitializeObjecttoCheck();

            SpawnInterval += Time.deltaTime;

            Component PositioningComponent = null;

            switch (SelectedSpawningStyle)
            {
                case SpawningStyles.Wave:
                    SelectedSpawningFunction = SpawningFunctions.SpawnWaveInArea;
                    PositioningComponent = GetComponentInChildren<SpawnArea>();
                    break;

                case SpawningStyles.Continuous:
                    if (SelectedSpawnPositionOption == PositioningOptions.Area)
                    {
                        PositioningComponent = GetComponentInChildren<SpawnArea>();
                        SelectedSpawningFunction = SpawningFunctions.SpawnPriorityObjectInArea;
                    }

                    else
                    {
                        PositioningComponent = SpawnPositions[0];
                        SelectedSpawningFunction = SpawningFunctions.SpawnPriorityObjectAtSpawnPoint;                                            
                    }
                    break;
            }

            Camera camera = null;

            if (!DoSpawnInFrustum)
                camera = FrustumCamera;

            if (IsSpawningAllowed())
            {
                TriggerSpawn = false;

                GameObject[] ReturnedObjects = SelectedSpawningFunction(PositioningComponent, camera);
                if (ReturnedObjects != null)
                {
                    SpawnedObjects.AddObjects(ReturnedObjects);
                    SpawnInterval = 0f;
                }
            }
        }

        bool IsSpawningAllowed()
        {
            if(!TriggerSpawnOverridesLogic || (!TriggerSpawn && TriggerSpawnOverridesLogic))
            {
                int DesiredObjectAmount = 1;

                if(SelectedSpawningFunction == SpawningFunctions.SpawnWaveInArea)            
                    DesiredObjectAmount = SpawningFunctions.WaveSpawnAmount;
            
                if(SelectedSpawningStyle == SpawningStyles.Wave && DoSpawnContinuousWaves)
                {
                    if (SpawnedObjects.Size > 0 || CurrentContiniousWaveStatus == ContiniousWaveStatus.Stopped || DoSpawnIfNotInRange && !IsNotInRange)
                        return false;
                }         

                else if(SpawnInterval < SpawnDelay || DoSpawnIfNotInRange && !IsNotInRange || (DoLimitObjectsAlive && MaximalSpawnedObjectsAlive < DesiredObjectAmount + SpawnedObjects.Size))            
                    return false;
            }
            return true;            
        }

        void UpdateDistance()
        {
            if (DoSpawnIfNotInRange && Player)
            {
                switch (SelectedDistanceCheck)
                {
                    case DistanceCheckingStyles.TwoDimensionalCheck:
                        IsNotInRange = DistanceChecking.TwoDimensionalCheck(transform, Player.transform, RangeToCheck);
                        break;

                    case DistanceCheckingStyles.ThreeDimensionalCheck:
                        IsNotInRange = DistanceChecking.ThreeDimensionalCheck(transform, Player.transform, RangeToCheck);
                        break;
                }
            }
        }

        void OnTriggerEnter(Collider collider)
        {
            if (SelectedDistanceCheck == DistanceCheckingStyles.SphereColliderCheck)
                if (collider.gameObject == Player)
                    IsNotInRange = false;
        }

        void OnTriggerExit(Collider collider)
        {
            if (SelectedDistanceCheck == DistanceCheckingStyles.SphereColliderCheck)
                if (collider.gameObject == Player)
                    IsNotInRange = true;

        }

        void InitializeObjecttoCheck()
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
                    string Tag = UnityEditorInternal.InternalEditorUtility.tags[PlayerIdentificationData.Tag];
                    Player = GameObject.FindWithTag(Tag);
                    break;
            }
        }

        public void InitializeSpawnPositions()
        {
            SpawnPositions = new List<Component>();

            foreach (var Child in transform.GetComponentsInChildren<SpawnPosition>())
            {
                SpawnPositions.Add(Child);
            }
        }     
    }



    [CustomEditor(typeof(Spawner))]
    public class DynamicScriptEditor : Editor
    {
        bool FoldOutTest;

        SerializedProperty CurrentContiniousWaveStatus;
        SerializedProperty foldOutObjectsToSpawn;
        SerializedProperty ShowPointPositions;
        SerializedProperty IgnoredObjects;
        SerializedProperty SpawnDelay;
        SerializedProperty ShowIgnoredObjects;
        SerializedProperty WaveSpawnAmount;
        SerializedProperty MaximalSpawnedObjectsAlive;
        SerializedProperty RangeToCheck;
        SerializedProperty DoSpawnIfNotInRange;
        SerializedProperty DoSpawnContiniousWaves;
        SerializedProperty DoSpawnInFrustum;
        SerializedProperty DoLimitObjectsAlive;
        SerializedProperty IsNotInRange;
        SerializedProperty SpawnPositions;
        SerializedProperty Player;
        SerializedProperty ObjectToSpawn;
        SerializedProperty SelectedSpawnPositionOption;
        SerializedProperty SelectedSpawningStyle;
        SerializedProperty SelectedPlayerIdentification;
        SerializedProperty SelectedDistanceCheck;
        SerializedProperty PlayerIdentificationData;
        SerializedProperty FrustumCamera;
        SerializedProperty UseOcclusionCulling;
        SerializedProperty ObjectsToSpawn;
        SerializedProperty TriggerSpawnOverridesLogic;

        GUILayoutOption StandardLayout = GUILayout.Height(15);

        Spawner DynamicSpawned; 
        
        protected virtual void OnEnable()
        {
            DynamicSpawned = target as Spawner;

            CurrentContiniousWaveStatus = this.serializedObject.FindProperty("CurrentContiniousWaveStatus");
            foldOutObjectsToSpawn = this.serializedObject.FindProperty("FoldoutObjectsToSpawn");
            TriggerSpawnOverridesLogic = this.serializedObject.FindProperty("TriggerSpawnOverridesLogic");
            ObjectsToSpawn = this.serializedObject.FindProperty("ObjectsToSpawn");
            UseOcclusionCulling = this.serializedObject.FindProperty("UseOcclusionCulling");
            ShowPointPositions = this.serializedObject.FindProperty("DoShowPointPositions");
            IgnoredObjects = this.serializedObject.FindProperty("IgnoredObjects");
            SpawnDelay = this.serializedObject.FindProperty("SpawnDelay");
            ShowIgnoredObjects = this.serializedObject.FindProperty("ShowIgnoredObjects");
            WaveSpawnAmount = this.serializedObject.FindProperty("WaveSpawnAmount");
            MaximalSpawnedObjectsAlive = this.serializedObject.FindProperty("MaximalSpawnedObjectsAlive");
            RangeToCheck = this.serializedObject.FindProperty("RangeToCheck");
            DoSpawnIfNotInRange = this.serializedObject.FindProperty("DoSpawnIfNotInRange");
            DoSpawnContiniousWaves = this.serializedObject.FindProperty("DoSpawnContinuousWaves");
            DoSpawnInFrustum = this.serializedObject.FindProperty("DoSpawnInFrustum");
            DoLimitObjectsAlive = this.serializedObject.FindProperty("DoLimitObjectsAlive");
            IsNotInRange = this.serializedObject.FindProperty("IsNotInRange");
            SpawnPositions = this.serializedObject.FindProperty("SpawnPositions");
            Player = this.serializedObject.FindProperty("Player");
            ObjectToSpawn = this.serializedObject.FindProperty("ObjectToSpawn");
            SelectedSpawnPositionOption = this.serializedObject.FindProperty("SelectedSpawnPositionOption");
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

            SpawningFunctions.TriggerSpawnOverridesLogic = TriggerSpawnOverridesLogic.boolValue;
                 
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(SelectedSpawningStyle, new GUIContent("Spawn Style: "), StandardLayout);

            EditorGUI.indentLevel++;

            if (SelectedSpawningStyle.intValue == (int)SpawningStyles.Continuous)
            {
                EditorGUILayout.PropertyField(SpawnDelay, new GUIContent("Spawn Delay: "), StandardLayout);

                if (SpawnDelay.floatValue < 0f)
                    SpawnDelay.floatValue *= -1;
       
                EditorGUILayout.PropertyField(DoLimitObjectsAlive, new GUIContent("Limit Object Amount: "), StandardLayout);
      
                if (DoLimitObjectsAlive.boolValue)
                {             
                    EditorGUILayout.PropertyField(MaximalSpawnedObjectsAlive, new GUIContent("Amount: "), StandardLayout);
              
                    if (MaximalSpawnedObjectsAlive.intValue < 0)
                        MaximalSpawnedObjectsAlive.intValue = 0;
                }
            }

            if (SelectedSpawningStyle.intValue == (int)SpawningStyles.Wave)
            { 
                EditorGUILayout.PropertyField(DoSpawnContiniousWaves, new GUIContent("Continious Waves: "), StandardLayout);

                if(DoSpawnContiniousWaves.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(CurrentContiniousWaveStatus, new GUIContent("Current Status: "), true);
                    EditorGUI.indentLevel--;
                }

                else
                {
                    EditorGUILayout.PropertyField(SpawnDelay, new GUIContent("Spawn Delay: "), StandardLayout);
                }

                EditorGUILayout.PropertyField(WaveSpawnAmount, new GUIContent("Amount: "), StandardLayout);

                if (WaveSpawnAmount.intValue > 100)
                    WaveSpawnAmount.intValue = 100;

                if (WaveSpawnAmount.intValue < 1)
                    WaveSpawnAmount.intValue = 1;


                SpawningFunctions.WaveSpawnAmount = WaveSpawnAmount.intValue;
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(DoSpawnIfNotInRange, new GUIContent("Spawn if not in Range: "), StandardLayout);

            if (DoSpawnIfNotInRange.boolValue)
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
                        break;
                }

                EditorGUILayout.PropertyField(SelectedDistanceCheck, new GUIContent("Check Style: "), StandardLayout);

                if (SelectedDistanceCheck.intValue == (int)DistanceCheckingStyles.ThreeDimensionalCheck || SelectedDistanceCheck.intValue == (int)DistanceCheckingStyles.TwoDimensionalCheck)
                {
                    EditorGUILayout.PropertyField(RangeToCheck, new GUIContent("Range: "), StandardLayout);

                    if (DynamicSpawned.gameObject.GetComponent<SphereCollider>())
                        DestroyImmediate(DynamicSpawned.gameObject.GetComponent<SphereCollider>());
                }

                else if (SelectedDistanceCheck.intValue == (int)DistanceCheckingStyles.SphereColliderCheck)
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

            EditorGUILayout.PropertyField(DoSpawnInFrustum, new GUIContent("Spawn in Frustum: "), StandardLayout);

            if (!DoSpawnInFrustum.boolValue)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(FrustumCamera, new GUIContent("Camera: "), StandardLayout);

                EditorGUILayout.PropertyField(UseOcclusionCulling, new GUIContent("Occlusion Culling: "), StandardLayout);

                if (UseOcclusionCulling.boolValue)
                {
                    EditorGUILayout.PropertyField(IgnoredObjects, new GUIContent("Ignored Objects: "), true);

                    List<GameObject> IgnoredObjectList = new List<GameObject>();

                    this.InitializeIgnoredObjects(IgnoredObjectList);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(SelectedSpawnPositionOption, new GUIContent("Spawn Style: "), StandardLayout);

            if (SelectedSpawningStyle.intValue == (int)SpawningStyles.Wave)
                SelectedSpawnPositionOption.intValue = (int)PositioningOptions.Area;

            switch ((PositioningOptions)SelectedSpawnPositionOption.intValue)
            {
                case PositioningOptions.Area:
                    {
                        for (int childIndex = 0; childIndex < DynamicSpawned.transform.childCount; childIndex++)
                        {
                            if (DynamicSpawned.transform.GetChild(childIndex).name == "SpawnArea")
                            {
                                DynamicSpawned.SpawnArea = DynamicSpawned.transform.GetChild(childIndex).gameObject; DynamicSpawned.SpawnArea.GetComponent<MeshCollider>().hideFlags = HideFlags.HideInInspector;
                            }
                        }

                        if (!DynamicSpawned.SpawnArea)
                        {
                            DynamicSpawned.SpawnArea = Instantiate(Resources.Load("SpawnArea", typeof(GameObject))) as GameObject; //Resources.Load<GameObject>("Assets /DynamicSpawningSystem/SpawnArea");
                            DynamicSpawned.SpawnArea.transform.SetParent(DynamicSpawned.transform);
                            DynamicSpawned.SpawnArea.transform.name = "SpawnArea";
                            DynamicSpawned.SpawnArea.transform.localPosition = new Vector3(0, 0, 0);
                            DynamicSpawned.SpawnArea.GetComponent<MeshCollider>().hideFlags = HideFlags.HideInInspector;
                            DynamicSpawned.SpawnArea.GetComponent<MeshFilter>().hideFlags = HideFlags.HideInInspector;
                        }

                        break;
                    }

                case PositioningOptions.Points:
                    {
                        break;
                    }
            }

            EditorGUILayout.PropertyField(TriggerSpawnOverridesLogic, new GUIContent("Trigger spawn overrides logic:"), StandardLayout);

            EditorGUI.EndChangeCheck();

            this.serializedObject.ApplyModifiedProperties();

            serializedObject.Update();


            if (GUI.changed)
                EditorUtility.SetDirty(DynamicSpawned);
        }

        void InitializeIgnoredObjects(List<GameObject> IgnoredObjects)
        {
            for (int ObjectIndex = 0; ObjectIndex < DynamicSpawned.IgnoredObjects.Count; ObjectIndex++)
            {
                if (DynamicSpawned.IgnoredObjects[ObjectIndex] != null)
                {
                    if (DynamicSpawned.IgnoredObjects[ObjectIndex].GetComponent<Collider>())
                        IgnoredObjects.Add(DynamicSpawned.IgnoredObjects[ObjectIndex]);

                    if (DynamicSpawned.IgnoredObjects[ObjectIndex].transform.parent)
                        if (DynamicSpawned.IgnoredObjects[ObjectIndex].transform.parent.GetComponent<Collider>())
                            IgnoredObjects.Add(DynamicSpawned.IgnoredObjects[ObjectIndex].transform.parent.gameObject);

                    for (int ChildrenIndex = 0; ChildrenIndex < DynamicSpawned.IgnoredObjects[ObjectIndex].transform.childCount; ChildrenIndex++)
                    {
                        if (DynamicSpawned.IgnoredObjects[ObjectIndex].transform.GetChild(ChildrenIndex).GetComponent<Collider>())
                            IgnoredObjects.Add(DynamicSpawned.IgnoredObjects[ObjectIndex].transform.GetChild(ChildrenIndex).gameObject);
                    }

                }
            }

            SpawningFunctions.FrustumIgnoredObjects = IgnoredObjects;
        }
    }

}
