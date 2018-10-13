using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DDS
{
    public class Spawner : MonoBehaviour
    {
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
   
        [SerializeField]
        public int Wave_Spawn_Amount;

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
        public List<GameObject> Spawn_Positions;

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

        bool DoTest = true;

        void Start()
        {

            TestPosition = GameObject.Find("Test").transform.position;

            this.InitializeObjectstoCheck();
        }

        void Update()
        {
            ///Test Mode

            SpawningFunctions.Trigger_Spawn_Overrides_Logic = Trigger_Spawn_Overrides_Logic;
            SpawningFunctions.IsTriggerSpawn = Trigger_Spawn;
            
            SpawningFunctions.UseOcclusionCulling = UseOcclusionCulling;
            this.InitializeObjectstoCheck();
            this.CheckSpawnedObjects();

            SpawnInterval += Time.deltaTime;

            if (Is_Not_In_Range && Do_Spawn_If_Not_In_Range || !Do_Spawn_If_Not_In_Range || Trigger_Spawn_Overrides_Logic && Trigger_Spawn)
            {
                switch (Selected_Spawning_Style)
                {
                    case SpawningStyles.Continuous:
                        if (SpawnInterval > Spawn_Delay || Trigger_Spawn)
                        {
                            
                            if (Do_Limit_Objects_Alive)
                                if (Spawned_Objects.Count >= Maximal_Spawned_Objects_Alive && !(Trigger_Spawn_Overrides_Logic && Trigger_Spawn))
                                   return;

                            GameObject bufferObject = null;
                            switch (Selected_Spawn_Position_Option)
                            {
                                case PositioningOptions.Area:

                                    if (!Do_Spawn_In_Frustum)
                                        bufferObject = SpawningFunctions.SpawnPriorityObjectInArea(Spawn_Area.GetComponent<SpawnArea>(), Objects_To_Spawn, false, Frustum_Camera);
                                    else
                                        bufferObject = SpawningFunctions.SpawnPriorityObjectInArea(Spawn_Area.GetComponent<SpawnArea>(), Objects_To_Spawn, false, null);

                                    if (bufferObject)
                                    {
                                        Trigger_Spawn = false;
                                        Spawned_Objects.Add(bufferObject);
                                        SpawnInterval = 0f;
                                    }

                                    break;

                                case PositioningOptions.Points:
                                    if (!Do_Spawn_In_Frustum) 
                                        bufferObject = SpawningFunctions.SpawnPriorityObjectAtSpawnPoint(Spawn_Positions[0].GetComponent<SpawnPosition>(), Objects_To_Spawn, Frustum_Camera);
                                    else
                                      bufferObject = SpawningFunctions.SpawnPriorityObjectAtSpawnPoint(Spawn_Positions[0].GetComponent<SpawnPosition>(), Objects_To_Spawn, null);

                                    if (bufferObject)
                                    {
                                        Trigger_Spawn = false;
                                        Spawned_Objects.Add(bufferObject);
                                        SpawnInterval = 0f;
                                    }
                                    break;
                            }
                        }
                        break;



                    case SpawningStyles.Wave:

                        if (Spawn_Wave_Trigger || Do_Spawn_Continuous_Waves || Trigger_Spawn_Overrides_Logic && Trigger_Spawn)
                        {
                            if (Do_Spawn_Continuous_Waves && (!Trigger_Spawn_Overrides_Logic && Trigger_Spawn))
                                if (Spawned_Objects.Count > 0)
                                    return;

                            GameObject[] ReturnedObjects = null;

                            switch (Selected_Spawn_Position_Option)
                            {
                                case PositioningOptions.Area:
                                    if (!Do_Spawn_In_Frustum) 
                                        ReturnedObjects = SpawningFunctions.SpawnWaveInArea(Spawn_Area.GetComponent<SpawnArea>(), Objects_To_Spawn, Wave_Spawn_Amount, Frustum_Camera);
                                    else
                                        ReturnedObjects = SpawningFunctions.SpawnWaveInArea(Spawn_Area.GetComponent<SpawnArea>(), Objects_To_Spawn, Wave_Spawn_Amount, null);
                                    break;
                            }

                            if (ReturnedObjects != null)
                            {
                                Trigger_Spawn = false;
                                Spawn_Wave_Trigger = false;

                                foreach (GameObject ToAddObject in ReturnedObjects)
                                {
                                    Spawned_Objects.Add(ToAddObject);
                                }
                            }
                        }
                        break;
                }
            }
            
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

        public void Awake()
        {
            DynamicSpawned = target as Spawner;
        }

        override public void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(TriggerSpawnOverridesLogic, new GUIContent("Trigger spawn overrides logic:"), StandardLayout);
            EditorGUI.EndChangeCheck();

            SpawningFunctions.Trigger_Spawn_Overrides_Logic = TriggerSpawnOverridesLogic.boolValue;

            //Test start

            

            EditorGUI.BeginChangeCheck();

            SerializedProperty ObjectSize = ObjectsToSpawn.FindPropertyRelative("Array.size");


            EditorGUILayout.PropertyField(ObjectSize, new GUIContent("Objects to Spawn: "), StandardLayout);

            if (ObjectSize.intValue > 20)
                ObjectSize.intValue = 20;

           
            for (int i = 0; i < ObjectSize.intValue; i++)
            {
                EditorGUI.indentLevel++;
                string Name = "Empty";
                if (ObjectsToSpawn.GetArrayElementAtIndex(i).FindPropertyRelative("ObjectToSpawn").objectReferenceValue != null)
                    Name = ObjectsToSpawn.GetArrayElementAtIndex(i).FindPropertyRelative("ObjectToSpawn").objectReferenceValue.name;


                EditorGUILayout.PropertyField(ObjectsToSpawn.GetArrayElementAtIndex(i), new GUIContent(Name), true);
                
                EditorGUI.indentLevel--;

            }


            //Test end

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(SelectedSpawningStyle, new GUIContent("Spawn Style: "), StandardLayout);
            EditorGUI.EndChangeCheck();


            EditorGUI.indentLevel++;

            if (SelectedSpawningStyle.intValue == (int)SpawningStyles.Continuous)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(SpawnDelay, new GUIContent("Spawn Delay: "), StandardLayout);
                EditorGUI.EndChangeCheck();

                if (SpawnDelay.floatValue < 0f)
                    SpawnDelay.floatValue *= -1;


                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(DoLimitObjectsAlive, new GUIContent("Limit Object Amount: "), StandardLayout);
                EditorGUI.EndChangeCheck();

                if (DoLimitObjectsAlive.boolValue)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(MaximalSpawnedObjectsAlive, new GUIContent("Amount: "), StandardLayout);
                    EditorGUI.EndChangeCheck();

                    if (MaximalSpawnedObjectsAlive.intValue < 0)
                        MaximalSpawnedObjectsAlive.intValue = 0;
                }
            }

            if (SelectedSpawningStyle.intValue == (int)SpawningStyles.Wave)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(DoSpawnContiniousWaves, new GUIContent("Continious Waves: "), StandardLayout);
                EditorGUI.EndChangeCheck();

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(WaveSpawnAmount, new GUIContent("Amount: "), StandardLayout);
                EditorGUI.EndChangeCheck();

                if (WaveSpawnAmount.intValue > 100)
                    WaveSpawnAmount.intValue = 100;

                if (WaveSpawnAmount.intValue < 1)
                    WaveSpawnAmount.intValue = 1;
            }

            EditorGUI.indentLevel--;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(DoSpawnIfNotInRange, new GUIContent("Spawn if not in Range: "), StandardLayout);
            EditorGUI.EndChangeCheck();

            if (DoSpawnIfNotInRange.boolValue)
            {
                EditorGUI.indentLevel++;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(SelectedPlayerIdentification, new GUIContent("Identification: "), StandardLayout);
                EditorGUI.EndChangeCheck();

                switch ((IdentifyPlayer)SelectedPlayerIdentification.intValue)
                {
                    case IdentifyPlayer.byField:
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(PlayerIdentificationData.FindPropertyRelative("Object"), new GUIContent("Object: "), StandardLayout);
                        EditorGUI.EndChangeCheck();
                        break;

                    case IdentifyPlayer.byName:


                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(PlayerIdentificationData.FindPropertyRelative("Name"), new GUIContent("Name: "), StandardLayout);
                        EditorGUI.EndChangeCheck();
                        break;

                    case IdentifyPlayer.byTag:
                        string[] Tags = UnityEditorInternal.InternalEditorUtility.tags;

                        EditorGUI.BeginChangeCheck();
                        PlayerIdentificationData.FindPropertyRelative("Tag").intValue = EditorGUILayout.Popup(new GUIContent("Tag: ", "How to check for the Player"), PlayerIdentificationData.FindPropertyRelative("Tag").intValue, Tags);
                        EditorGUI.EndChangeCheck();
                        break;

                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(SelectedDistanceCheck, new GUIContent("Check Style: "), StandardLayout);
                EditorGUI.EndChangeCheck();

                if (SelectedDistanceCheck.intValue == (int)DistanceCheckingStyles.ThreeDimensionalCheck || SelectedDistanceCheck.intValue == (int)DistanceCheckingStyles.TwoDimensionalCheck)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(RangeToCheck, new GUIContent("Range: "), StandardLayout);
                    EditorGUI.EndChangeCheck();

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

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(DoSpawnInFrustum, new GUIContent("Spawn in Frustum: "), StandardLayout);
            EditorGUI.EndChangeCheck();

            if (!DoSpawnInFrustum.boolValue)
            {
                EditorGUI.indentLevel++;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(FrustumCamera, new GUIContent("Camera: "), StandardLayout);
                EditorGUI.EndChangeCheck();

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(UseOcclusionCulling, new GUIContent("Occlusion Culling: "), StandardLayout);
                EditorGUI.EndChangeCheck();

                if (UseOcclusionCulling.boolValue)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(IgnoredObjects, new GUIContent("Ignored Objects: "), true);
                    EditorGUI.EndChangeCheck();

                    List<GameObject> IgnoredObjectList = new List<GameObject>();

                    this.InitializeIgnoredObjects(IgnoredObjectList);
                }

                EditorGUI.indentLevel--;
            }


            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(SelectedSpawnPositionOption, new GUIContent("Spawn Style: "), StandardLayout);
            EditorGUI.EndChangeCheck();

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
                            DynamicSpawned.Spawn_Area.GetComponent<SpawnArea>().hideFlags = HideFlags.HideInInspector;
                        }

                        break;
                    }


                case PositioningOptions.Points:
                    {
                        if (DynamicSpawned.Spawn_Area)
                            DestroyImmediate(DynamicSpawned.Spawn_Area);

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(ShowPointPositions, new GUIContent("Show Spawn Points: "), StandardLayout);
                        EditorGUI.EndChangeCheck();

                        if (ShowPointPositions.boolValue)
                        {
                            EditorGUI.indentLevel++;

                            EditorGUI.BeginChangeCheck();

                            for (int i = 0; i < SpawnPositions.arraySize; i++)
                            {
                                EditorGUILayout.PropertyField(SpawnPositions.GetArrayElementAtIndex(i));
                            }

                            if (GUILayout.Button("Create Spawn Point"))
                            {
                                GameObject bufferPosition = Instantiate(Resources.Load("SpawnPosition", typeof(GameObject))) as GameObject;
                                bufferPosition.transform.SetParent(DynamicSpawned.transform);
                                bufferPosition.transform.name = "New Spawn Position";
                                bufferPosition.transform.localPosition = new Vector3(0, 0, 0);
                                DynamicSpawned.Spawn_Positions.Add(bufferPosition);
                            }

                            EditorGUI.EndChangeCheck();

                            EditorGUI.indentLevel--;

                        }

                        break;
                    }
            }

            this.serializedObject.ApplyModifiedProperties();



            if (GUI.changed)
                EditorUtility.SetDirty(DynamicSpawned);
        }

        void InitializeIgnoredObjects(List<GameObject> Test)
        {
            for (int ObjectIndex = 0; ObjectIndex < DynamicSpawned.IgnoredObjects.Count; ObjectIndex++)
            {
                if (DynamicSpawned.IgnoredObjects[ObjectIndex] != null)
                {
                    if (DynamicSpawned.IgnoredObjects[ObjectIndex].GetComponent<Collider>())
                        Test.Add(DynamicSpawned.IgnoredObjects[ObjectIndex]);

                    if (DynamicSpawned.IgnoredObjects[ObjectIndex].transform.parent)
                        if (DynamicSpawned.IgnoredObjects[ObjectIndex].transform.parent.GetComponent<Collider>())
                            Test.Add(DynamicSpawned.IgnoredObjects[ObjectIndex].transform.parent.gameObject);

                    for (int ChildrenIndex = 0; ChildrenIndex < DynamicSpawned.IgnoredObjects[ObjectIndex].transform.childCount; ChildrenIndex++)
                    {
                        if (DynamicSpawned.IgnoredObjects[ObjectIndex].transform.GetChild(ChildrenIndex).GetComponent<Collider>())
                            Test.Add(DynamicSpawned.IgnoredObjects[ObjectIndex].transform.GetChild(ChildrenIndex).gameObject);
                    }

                }
            }

            SpawningFunctions.FrustumIgnoredObjects = Test;
        }



    }

}
