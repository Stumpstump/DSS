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
        ContiniousWaveStatus Current_Continious_Wave_Status;


        [SerializeField]
        int Wave_Spawn_Amount;

        [SerializeField]
        bool FoldoutObjectsToSpawn;

        [SerializeField]
        bool UseOcclusionCulling;

        [SerializeField]
        public List<GameObject> IgnoredObjects;

        [SerializeField]
        public List<int> TestList;

        [SerializeField]
        public bool Trigger_Spawn;

        [SerializeField]
        public bool ShowIgnoredObjects;

        [SerializeField]
        public bool Do_Show_Point_Positions;

        [SerializeField]
        public bool Spawn_Wave_Trigger;

        [SerializeField]
        public bool Trigger_Spawn_Overrides_Logic;

        /// <summary>
        /// Maximum of Objects that can be alive at the same time
        /// Also used for the Wave continious wave spawner to check when its time for another wave
        /// </summary>
        [SerializeField]
        public int Maximal_Spawned_Objects_Alive;

        /// <summary>
        /// Delay between spawns with the continious spawner
        /// </summary>
        [SerializeField]
        public float Spawn_Delay;

        /// <summary>
        /// Range the Player has to be in for the spawner to spawn
        /// </summary>
        [SerializeField]
        public float Range_To_Check;

        /// <summary>
        /// Do spawn objects even the Player is not in range
        /// </summary>
        [SerializeField]
        public bool Do_Spawn_If_Not_In_Range;

        /// <summary>
        /// Do spawn another wave as soon as the other one is dead
        /// </summary>
        [SerializeField]
        public bool Do_Spawn_Continuous_Waves;

        /// <summary>
        /// Do spawn in the selected cameras frustum
        /// </summary>
        [SerializeField]
        public bool Do_Spawn_In_Frustum = true;

        /// <summary>
        /// Do limit the Objects for the continious spawner
        /// </summary>
        [SerializeField]
        public bool Do_Limit_Objects_Alive;

        /// <summary>
        /// Is the Player/Selected Object in range
        /// Dont change this manually
        /// </summary>
        [SerializeField]
        public bool Is_Not_In_Range;

        /// <summary>
        /// List of the Spawned Objects which are alive
        /// </summary>
        /// 

        [SerializeField]
        public SpawnAbleObject[] Objects_To_Spawn;


        [SerializeField]
        public List<GameObject> Spawned_Objects;

        /// <summary>
        /// List of the Spawn positions
        /// </summary>
        [SerializeField]
        public List<Component> Spawn_Positions;

        /// <summary>
        /// The Player Object to check the Is_In_Range variable
        /// </summary>
        [SerializeField]
        public GameObject Player;

        /// <summary>
        /// Selected Object to spawn
        /// </summary>
        [SerializeField]
        public GameObject Object_To_Spawn;

        /// <summary>
        /// The Area for spawning 
        /// </summary>
        [SerializeField]
        public GameObject Spawn_Area;

        [SerializeField]
        public PositioningOptions Selected_Spawn_Position_Option;

        [SerializeField]
        public SpawningStyles Selected_Spawning_Style;

        [SerializeField]
        public IdentifyPlayer Selected_Player_Identification;

        [SerializeField]
        public DistanceCheckingStyles Selected_Distance_Check;

        [SerializeField]
        public Identification Player_Identification_Data;

        [SerializeField]
        public Vector3 TestPosition;
        /// <summary>
        /// Camera to check the frustum of
        /// </summary>
        [SerializeField]
        public Camera Frustum_Camera;

        [SerializeField]
        float SpawnInterval;

        void Awake()
        {
            this.InitializeSpawnPositions();
        }

        void Start()
        {
            this.InitializeObjectstoCheck();
        }

        void Update()
        {
            if(Do_Spawn_If_Not_In_Range)
                this.UpdateDistance();

            SpawningFunctions.Trigger_Spawn_Overrides_Logic = Trigger_Spawn_Overrides_Logic;
            SpawningFunctions.IsTriggerSpawn = Trigger_Spawn;
            
            SpawningFunctions.UseOcclusionCulling = UseOcclusionCulling;
            this.InitializeObjectstoCheck();
            this.CheckSpawnedObjects();

            SpawnInterval += Time.deltaTime;

            Component PositioningComponent = null;

            switch (Selected_Spawning_Style)
            {
                case SpawningStyles.Wave:
                    {
                        SelectedSpawningFunction  = SpawningFunctions.SpawnWaveInArea;
                        PositioningComponent = Spawn_Area.GetComponent<SpawnArea>();
                    }
                    break;

                case SpawningStyles.Continuous:
                    if (Selected_Spawn_Position_Option == PositioningOptions.Area)
                    {
                        PositioningComponent = Spawn_Area.GetComponent<SpawnArea>();
                        SelectedSpawningFunction = SpawningFunctions.SpawnPriorityObjectInArea;
                    }

                    else
                    {
                        PositioningComponent = Spawn_Positions[0];
                        SelectedSpawningFunction = SpawningFunctions.SpawnPriorityObjectAtSpawnPoint;                                            
                    }
                    break;
            }
            
                Camera FrustumCamera = null;

                if (!Do_Spawn_In_Frustum)
                    FrustumCamera = Frustum_Camera;

            if (IsSpawningAllowed())
            {
                Trigger_Spawn = false;

                GameObject[] SpawnedObjects = SelectedSpawningFunction(PositioningComponent, FrustumCamera);
                if (SpawnedObjects != null)
                {
                    if (SpawnedObjects.Length > 0)
                    {
                        SpawnInterval = 0f;

                        foreach (var NewObject in SpawnedObjects)
                        {
                            Spawned_Objects.Add(NewObject);
                        }
                    }
                }
            }

        }

        bool IsSpawningAllowed()
        {
            if(!Trigger_Spawn_Overrides_Logic || (!Trigger_Spawn && Trigger_Spawn_Overrides_Logic))
            {
                int DesiredObjectAmount = 1;

                if(SelectedSpawningFunction == SpawningFunctions.SpawnWaveInArea)            
                    DesiredObjectAmount = SpawningFunctions.WaveSpawnAmount;
            
                if(Selected_Spawning_Style == SpawningStyles.Wave && Do_Spawn_Continuous_Waves)
                {
                    if (Spawned_Objects.Count > 0 || Current_Continious_Wave_Status == ContiniousWaveStatus.Stopped || Do_Spawn_If_Not_In_Range && !Is_Not_In_Range)
                        return false;
                }         

                else if(SpawnInterval < Spawn_Delay || Do_Spawn_If_Not_In_Range && !Is_Not_In_Range || (Do_Limit_Objects_Alive && Maximal_Spawned_Objects_Alive < DesiredObjectAmount + Spawned_Objects.Count))            
                    return false;


            }
            return true;            
        }

        void UpdateDistance()
        {
            if (Do_Spawn_If_Not_In_Range && Player)
            {
                switch (Selected_Distance_Check)
                {
                    case DistanceCheckingStyles.TwoDimensionalCheck:
                        Is_Not_In_Range = DistanceChecking.TwoDimensionalCheck(transform, Player.transform, Range_To_Check);
                        break;

                    case DistanceCheckingStyles.ThreeDimensionalCheck:
                        Is_Not_In_Range = DistanceChecking.ThreeDimensionalCheck(transform, Player.transform, Range_To_Check);
                        break;
                }
            }
        }

        void OnTriggerEnter(Collider collider)
        {
            if (Selected_Distance_Check == DistanceCheckingStyles.SphereColliderCheck)
                if (collider.gameObject == Player)
                    Is_Not_In_Range = false;
        }

        void OnTriggerExit(Collider collider)
        {
            if (Selected_Distance_Check == DistanceCheckingStyles.SphereColliderCheck)
                if (collider.gameObject == Player)
                    Is_Not_In_Range = true;

        }

        void InitializeObjectstoCheck()
        {
            switch (Selected_Player_Identification)
            {
                case IdentifyPlayer.byField:
                    Player = Player_Identification_Data.Object;
                    break;

                case IdentifyPlayer.byName:
                    Player = GameObject.Find(Player_Identification_Data.Name);
                    break;

                case IdentifyPlayer.byTag:
                    string Tag = UnityEditorInternal.InternalEditorUtility.tags[Player_Identification_Data.Tag];
                    Player = GameObject.FindWithTag(Tag);
                    break;
            }
        }

        public void InitializeSpawnPositions()
        {
            Spawn_Positions = new List<Component>();

            foreach (var Child in transform.GetComponentsInChildren<SpawnPosition>())
            {
                Spawn_Positions.Add(Child);
            }
        }

        public void ResetSpawnedObjects()
        {
            for (int i = 0; i < Spawned_Objects.Count; i++)
                Destroy(Spawned_Objects[i]);

            Spawned_Objects.Clear();
        }

        void CheckSpawnedObjects()
        {
            for (int Index = 0; Index < Spawned_Objects.Count; Index++)
            {
                if (!Spawned_Objects[Index])
                {
                    Spawned_Objects.RemoveAt(Index);
                }
            }
        }

        bool IsPositionEmpty(GameObject Object)
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
                    if (bounds.Intersects(ObjectBounds) && Colliders[index].gameObject != Object)
                        return false;
            }
            return true;
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

            if (DynamicSpawned.IgnoredObjects == null)
            {
                DynamicSpawned.IgnoredObjects = new List<GameObject>();
            }

            if (DynamicSpawned.Spawn_Positions == null)
            {
                DynamicSpawned.Spawned_Objects = new List<GameObject>();
            }

            if(DynamicSpawned.Objects_To_Spawn == null)
            {
                DynamicSpawned.Objects_To_Spawn = new SpawnAbleObject[0];
            }

            CurrentContiniousWaveStatus = this.serializedObject.FindProperty("Current_Continious_Wave_Status");
            foldOutObjectsToSpawn = this.serializedObject.FindProperty("FoldoutObjectsToSpawn");
            TriggerSpawnOverridesLogic = this.serializedObject.FindProperty("Trigger_Spawn_Overrides_Logic");
            ObjectsToSpawn = this.serializedObject.FindProperty("Objects_To_Spawn");
            UseOcclusionCulling = this.serializedObject.FindProperty("UseOcclusionCulling");
            ShowPointPositions = this.serializedObject.FindProperty("Do_Show_Point_Positions");
            IgnoredObjects = this.serializedObject.FindProperty("IgnoredObjects");
            SpawnDelay = this.serializedObject.FindProperty("Spawn_Delay");
            ShowIgnoredObjects = this.serializedObject.FindProperty("ShowIgnoredObjects");
            WaveSpawnAmount = this.serializedObject.FindProperty("Wave_Spawn_Amount");
            MaximalSpawnedObjectsAlive = this.serializedObject.FindProperty("Maximal_Spawned_Objects_Alive");
            RangeToCheck = this.serializedObject.FindProperty("Range_To_Check");
            DoSpawnIfNotInRange = this.serializedObject.FindProperty("Do_Spawn_If_Not_In_Range");
            DoSpawnContiniousWaves = this.serializedObject.FindProperty("Do_Spawn_Continuous_Waves");
            DoSpawnInFrustum = this.serializedObject.FindProperty("Do_Spawn_In_Frustum");
            DoLimitObjectsAlive = this.serializedObject.FindProperty("Do_Limit_Objects_Alive");
            IsNotInRange = this.serializedObject.FindProperty("Is_Not_In_Range");
            SpawnPositions = this.serializedObject.FindProperty("Spawn_Positions");
            Player = this.serializedObject.FindProperty("Player");
            ObjectToSpawn = this.serializedObject.FindProperty("Object_To_Spawn");
            SelectedSpawnPositionOption = this.serializedObject.FindProperty("Selected_Spawn_Position_Option");
            SelectedSpawningStyle = this.serializedObject.FindProperty("Selected_Spawning_Style");
            SelectedPlayerIdentification = this.serializedObject.FindProperty("Selected_Player_Identification");
            SelectedDistanceCheck = this.serializedObject.FindProperty("Selected_Distance_Check");
            PlayerIdentificationData = this.serializedObject.FindProperty("Player_Identification_Data");
            FrustumCamera = this.serializedObject.FindProperty("Frustum_Camera");
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

        [MenuItem("GameObject/Spawner/Create a Spawn Area", false, 0)]
        void AddSpawnArea()
        {
            
        }

        public void Awake()
        {
        }

        bool FoldOut;

        override public void OnInspectorGUI()
        {
            this.serializedObject.Update();

            SpawningFunctions.Trigger_Spawn_Overrides_Logic = TriggerSpawnOverridesLogic.boolValue;
                 
            EditorGUI.BeginChangeCheck();
// 
// //             EditorGUILayout.PropertyField(ObjectsToSpawn, new GUIContent("Objects to Spawn: "), true);
// 
//             SerializedProperty ObjectSize = ObjectsToSpawn.FindPropertyRelative("Array.size");
// 
//             if (ObjectSize.intValue > 20)
//                 ObjectSize.intValue = 20;
// 
// //             for (int i = 0; i < ObjectSize.intValue; i++)
// //             {
// //                 string Name = "Empty";
// //                 if (ObjectsToSpawn.GetArrayElementAtIndex(i).FindPropertyRelative("ObjectToSpawn").objectReferenceValue != null)
// //                     ObjectsToSpawn.GetArrayElementAtIndex(i).FindPropertyRelative("ObjectName").stringValue = ObjectsToSpawn.GetArrayElementAtIndex(i).FindPropertyRelative("ObjectToSpawn").objectReferenceValue.name;
// // 
// //                 else
// //                 {
// //                     ObjectsToSpawn.GetArrayElementAtIndex(i).FindPropertyRelative("ObjectName").stringValue = Name;
// //                 }
// //             }

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
                    EditorGUILayout.PropertyField(CurrentContiniousWaveStatus, new GUIContent("Current Continous Wave Status: "), true);
                    EditorGUI.indentLevel--;

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
                                DynamicSpawned.Spawn_Area = DynamicSpawned.transform.GetChild(childIndex).gameObject; DynamicSpawned.Spawn_Area.GetComponent<MeshCollider>().hideFlags = HideFlags.HideInInspector;
                            }
                        }

                        if (!DynamicSpawned.Spawn_Area)
                        {
                            DynamicSpawned.Spawn_Area = Instantiate(Resources.Load("SpawnArea", typeof(GameObject))) as GameObject; //Resources.Load<GameObject>("Assets /DynamicSpawningSystem/SpawnArea");
                            DynamicSpawned.Spawn_Area.transform.SetParent(DynamicSpawned.transform);
                            DynamicSpawned.Spawn_Area.transform.name = "SpawnArea";
                            DynamicSpawned.Spawn_Area.transform.localPosition = new Vector3(0, 0, 0);
                            DynamicSpawned.Spawn_Area.GetComponent<MeshCollider>().hideFlags = HideFlags.HideInInspector;
                            DynamicSpawned.Spawn_Area.GetComponent<MeshFilter>().hideFlags = HideFlags.HideInInspector;
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
