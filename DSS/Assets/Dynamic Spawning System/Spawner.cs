using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DDS
{
    /// <summary>
    /// The basic Spawner Object
    /// </summary>
    public class Spawner : MonoBehaviour
    {
        public bool Spawn_Wave_Trigger = false;

        public int Wave_Spawn_Amount;

        /// <summary>
        /// Maximum of Objects that can be alive at the same time
        /// Also used for the Wave continious wave spawner to check when its time for another wave
        /// </summary>
        public int Maximal_Spawned_Objects_Alive;

        /// <summary>
        /// Delay between spawns with the continious spawner
        /// </summary>
        public float Spawn_Delay;

        /// <summary>
        /// Range the Player has to be in for the spawner to spawn
        /// </summary>
        public float Range_To_Check;

        /// <summary>
        /// Do spawn objects even the Player is not in range
        /// </summary>
        public bool Do_Spawn_If_Not_In_Range;

        /// <summary>
        /// Do spawn another wave as soon as the other one is dead
        /// </summary>
        public bool Do_Spawn_Continuous_Waves;

        /// <summary>
        /// Do spawn in the selected cameras frustum
        /// </summary>
        public bool Do_Spawn_In_Frustum = true;

        /// <summary>
        /// Do limit the Objects for the continious spawner
        /// </summary>
        public bool Do_Limit_Objects_Alive;

        /// <summary>
        /// Is the Player/Selected Object in range
        /// Dont change this manually
        /// </summary>
        public bool Is_Not_In_Range;

        /// <summary>
        /// List of the Spawned Objects which are alive
        /// </summary>
        public List<GameObject> Spawned_Objects;

        /// <summary>
        /// List of the Spawn positions
        /// </summary>
        public List<GameObject> Spawn_Positions;

        /// <summary>
        /// The Player Object to check the Is_In_Range variable
        /// </summary>
        public GameObject Player;

        /// <summary>
        /// Selected Object to spawn
        /// </summary>
        public GameObject Object_To_Spawn;

        /// <summary>
        /// The Area for spawning 
        /// </summary>
        public GameObject Spawn_Area;

        public PositioningOptions Selected_Spawn_Position_Option;
        public SpawningStyles Selected_Spawning_Style;
        public IdentifyPlayer Selected_Player_Identification;
        public DistanceCheckingStyles Selected_Distance_Check;
        public Identification Player_Identification_Data;

        public Vector3 TestPosition;
        /// <summary>
        /// Camera to check the frustum of
        /// </summary>
        public Camera Frustum_Camera;

        float SpawnInterval;

        void Start()
        {

            TestPosition = GameObject.Find("Test").transform.position;

            this.InitializeObjectstoCheck();         
        }

        void Update()
        {

            this.InitializeObjectstoCheck();
            this.CheckSpawnedObjects();

            SpawnInterval += Time.deltaTime;

            if (Is_Not_In_Range && Do_Spawn_If_Not_In_Range || !Do_Spawn_If_Not_In_Range)
            {               
                switch(Selected_Spawning_Style)
                {
                    case SpawningStyles.Continuous:
                        if(SpawnInterval > Spawn_Delay)
                        {
                            if (Do_Limit_Objects_Alive)
                                if (Spawned_Objects.Count >= Maximal_Spawned_Objects_Alive)
                                    return;

                            GameObject bufferObject = null;

                            switch (Selected_Spawn_Position_Option)
                            {
                                case PositioningOptions.Area:

                                    if(!Do_Spawn_In_Frustum)                                   
                                        bufferObject = SpawningFunctions.SpawnObjectInArea(Spawn_Area.GetComponent<SpawnArea>(), Object_To_Spawn, false, Frustum_Camera);                                    
                                    else
                                        bufferObject = SpawningFunctions.SpawnObjectInArea(Spawn_Area.GetComponent<SpawnArea>(), Object_To_Spawn, false);

                                    if (bufferObject)
                                    {
                                        Spawned_Objects.Add(bufferObject);
                                        SpawnInterval = 0f;
                                    }                                        
                                    break;

                                case PositioningOptions.Points:   
                                    if(!Do_Spawn_In_Frustum)
                                        bufferObject = SpawningFunctions.SpawnObjectAtSpawnPoint(Spawn_Positions[0].GetComponent<SpawnPosition>(), Object_To_Spawn, false, Frustum_Camera);
                                    else
                                        bufferObject = SpawningFunctions.SpawnObjectAtSpawnPoint(Spawn_Positions[0].GetComponent<SpawnPosition>(), Object_To_Spawn, false);

                                    if (bufferObject)
                                    {
                                        Spawned_Objects.Add(bufferObject);
                                        SpawnInterval = 0f;
                                    }
                                    break;
                            }
                        }
                        break;



                    case SpawningStyles.Wave:

                        if(Spawn_Wave_Trigger || Do_Spawn_Continuous_Waves)
                        {
                            if (Do_Spawn_Continuous_Waves)
                                if (Spawned_Objects.Count > 0)
                                    return;

                            GameObject[] ReturnedObjects = null;

                            switch (Selected_Spawn_Position_Option)
                            {
                                case PositioningOptions.Area:
                                    if (!Do_Spawn_In_Frustum)
                                        ReturnedObjects = SpawningFunctions.SpawnWaveInArea(Spawn_Area.GetComponent<SpawnArea>(), Object_To_Spawn, Wave_Spawn_Amount, false, Frustum_Camera);
                                    else
                                        ReturnedObjects = SpawningFunctions.SpawnWaveInArea(Spawn_Area.GetComponent<SpawnArea>(), Object_To_Spawn, Wave_Spawn_Amount, false);

                                    break;
                            }

                            if(ReturnedObjects != null)
                            {
                                Spawn_Wave_Trigger = false;

                                foreach(GameObject ToAddObject in ReturnedObjects)
                                {
                                    Spawned_Objects.Add(ToAddObject);
                                }
                            }
                        }
                        break;
                }
            }
            
            

            /*
            this.InitializeObjectstoCheck();
            this.CheckSpawnedObjects();

            
            if(Do_Spawn_If_Not_In_Range && Player)
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

            if (Is_Not_In_Range && Do_Spawn_If_Not_In_Range || !Do_Spawn_If_Not_In_Range)
            {
                if (Selected_Spawning_Style == SpawningStyles.Continuous)
                {
                    SpawningFunctions.SpawnObjectAtSpawnPoint(Spawn_Positions[0].GetComponent<SpawnPosition>(), Object_To_Spawn, false);

//                     if (Maximal_Spawned_Objects_Alive <= Spawned_Objects.Count && Do_Limit_Objects_Alive)
//                         return;
// 
//                      Debug.Log("yea");
//                     if (SpawnInterval >= Spawn_Delay)
//                     {
//                         bool wasSuccessful = false;
// 
//                         switch (Selected_Spawn_Position_Option)
//                         {
//                             case PositioningOptions.Area:
//                                 wasSuccessful = this.SpawnContiniousInArea();
//                                 break;
// 
//                             case PositioningOptions.Points:
//                                 wasSuccessful = this.SpawnContiniuosInPoints();
//                                 break;
//                         }
// 
//                         if (wasSuccessful)
//                             SpawnInterval = 0f; 
// 
//                     }
//                 }
// 
//                 else if(Selected_Spawning_Style == SpawningStyles.Wave)
//                 {
//                     if(Do_Spawn_Continuous_Waves)
//                     {
//                         this.SpawnWaveInArea();
//                     }
                 }
            }
                 */
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
            if(Selected_Distance_Check== DistanceCheckingStyles.SphereColliderCheck)
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
            switch(Selected_Player_Identification)
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

        bool SpawnWaveInArea()
        {
            if (Spawned_Objects.Count == 0)
            {
                for (int SpawnedIndex = 0; SpawnedIndex < Wave_Spawn_Amount; SpawnedIndex++)
                {
                    int Iteration = 0;
                    GameObject bufferObject = Instantiate<GameObject>(Object_To_Spawn, new Vector3(Spawn_Area.GetComponent<SpawnArea>().GetRandomPosition.x, Object_To_Spawn.transform.position.y, Spawn_Area.GetComponent<SpawnArea>().GetRandomPosition.z), Object_To_Spawn.transform.rotation);

                    while (!IsPositionEmpty(bufferObject))
                    {
                        Iteration++;
                        if (Iteration > 200)
                        {
                            Debug.Log("NoEmptyPositionFound");
                            Destroy(bufferObject);
                            this.ResetSpawnedObjects();
                            return false;
                            
                        }
                        
                        bufferObject.transform.position = new Vector3(Spawn_Area.GetComponent<SpawnArea>().GetRandomPosition.x, Object_To_Spawn.transform.position.y, Spawn_Area.GetComponent<SpawnArea>().GetRandomPosition.z);
                    }

                    Iteration = 0;
                    if(!Do_Spawn_In_Frustum&& IsVisible(bufferObject))
                    {
                        Debug.Log("Cant spawn one Object of the wave is visible");
                        Destroy(bufferObject);
                        this.ResetSpawnedObjects();
                        return false;
                    }

                  
                    Spawned_Objects.Add(bufferObject);

                }
            }

            return false;
        }

        bool SpawnContiniousInArea()
        {
            GameObject bufferObject = null;

            bufferObject = Instantiate<GameObject>(Object_To_Spawn, new Vector3(Spawn_Area.GetComponent<SpawnArea>().GetRandomPosition.x, Object_To_Spawn.transform.position.y, Spawn_Area.GetComponent<SpawnArea>().GetRandomPosition.z), Object_To_Spawn.transform.rotation);
            if (!Do_Spawn_In_Frustum)
                if (IsVisible(bufferObject))
                    Destroy(bufferObject);

            if (bufferObject)
            {
                Spawned_Objects.Add(bufferObject);
            }

            return bufferObject;
        }

        bool SpawnContiniuosInPoints()
        {
            GameObject bufferObject = null;

            if(Spawn_Positions.Count > 0)
            {
                bool DestroyObject = false;

                int Position = Random.Range(0, Spawn_Positions.Count);
                Vector3 RandomPosition = new Vector3(Spawn_Positions[Position].GetComponent<SpawnPosition>().GetSpawnPosition.x, Object_To_Spawn.transform.position.y, Spawn_Positions[Position].GetComponent<SpawnPosition>().GetSpawnPosition.z);
                bufferObject = Instantiate<GameObject>(Object_To_Spawn, RandomPosition, Object_To_Spawn.transform.rotation);

                Ray testRay = new Ray(bufferObject.GetComponent<Renderer>().bounds.center, Frustum_Camera.transform.position);
            

                if (!Do_Spawn_In_Frustum)
                    if (IsVisible(bufferObject))
                        DestroyObject = true;
                                    
                if(DestroyObject)
                    Destroy(bufferObject);

                if (bufferObject)
                {
                    Spawned_Objects.Add(bufferObject);
                }
            }

            return bufferObject;

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

            for( int index = 0; index < Colliders.Length; index++)
            {
                Bounds bounds = new Bounds();

                if (Colliders[index].GetComponent<Renderer>())
                    bounds = Colliders[index].GetComponent<Renderer>().bounds;

                else if (Colliders[index].GetComponentInChildren<Renderer>())
                    bounds = Colliders[index].GetComponentInChildren<Renderer>().bounds;

                else if(Colliders[index].GetComponentInParent<Renderer>())
                    bounds = Colliders[index].GetComponentInParent<Renderer>().bounds;

                if(bounds != null)
                    if (bounds.Intersects(ObjectBounds) && Colliders[index].gameObject != Object)
                        return false;

            }


            return true;
            
        }


        bool IsVisible(GameObject checkObject)
        {
           
            if (!checkObject.GetComponent<Renderer>())
                return false;

            Plane[] CameraBounds = GeometryUtility.CalculateFrustumPlanes(Frustum_Camera);
            if(GeometryUtility.TestPlanesAABB(CameraBounds, checkObject.GetComponent<Renderer>().bounds))
            {
                RaycastHit hit;
                return !Physics.Linecast(checkObject.GetComponent<Renderer>().bounds.center, Frustum_Camera.transform.position, out hit);
            }

            return false;
        }

    }



    [CustomEditor(typeof(Spawner))]
    public class DynamicScriptEditor : Editor
    {
        int UniqueNumber = 0;

        bool DoShowPointPositions = true;

        bool DoShowTestGameSettingsContent = true;

        int SpawnPointPositionsArraySize;

        int DesiredSpawnPositionIndex;

        override public void OnInspectorGUI()
        {
            var DynamicSpawned = target as Spawner;

            DesiredSpawnPositionIndex = DynamicSpawned.Spawn_Positions.Count;

            EditorGUILayout.Toggle("In range:", DynamicSpawned.Is_Not_In_Range);

            DoShowTestGameSettingsContent = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), DoShowTestGameSettingsContent, "Test Game Settings", true);


            DynamicSpawned.Object_To_Spawn = (GameObject)EditorGUILayout.ObjectField("Object:", DynamicSpawned.Object_To_Spawn, typeof(GameObject), true);


            if (DoShowTestGameSettingsContent)
            {
                string[] SpawnStyleOptions = new string[2];

                SpawnStyleOptions[0] = "Waves";
                SpawnStyleOptions[1] = "Continuous";

                DynamicSpawned.Selected_Spawning_Style = (SpawningStyles)EditorGUILayout.Popup(new GUIContent("Spawn Style: "), (int)DynamicSpawned.Selected_Spawning_Style, SpawnStyleOptions);

                EditorGUI.indentLevel++;


                if(DynamicSpawned.Selected_Spawning_Style == SpawningStyles.Continuous)
                {
                    DynamicSpawned.Spawn_Delay = EditorGUILayout.FloatField(new GUIContent("Spawn Delay", "In Seconds"), DynamicSpawned.Spawn_Delay);

                    if (DynamicSpawned.Spawn_Delay < 0f)
                        DynamicSpawned.Spawn_Delay *= -1;

                    DynamicSpawned.Do_Limit_Objects_Alive = EditorGUILayout.Toggle(new GUIContent("Limit Object Amount: "), DynamicSpawned.Do_Limit_Objects_Alive);

                    if(DynamicSpawned.Do_Limit_Objects_Alive)
                    {
                        DynamicSpawned.Maximal_Spawned_Objects_Alive = EditorGUILayout.IntField(new GUIContent("Amout: "), DynamicSpawned.Maximal_Spawned_Objects_Alive);
                        if (DynamicSpawned.Maximal_Spawned_Objects_Alive < 0)
                            DynamicSpawned.Maximal_Spawned_Objects_Alive = 0;
                    }
               
                }

                else if(DynamicSpawned.Selected_Spawning_Style == SpawningStyles.Wave)
                {
                    DynamicSpawned.Do_Spawn_Continuous_Waves = EditorGUILayout.Toggle(new GUIContent("Continuous Waves: ", "Spawn the next Wave as soon as this on is dead"), DynamicSpawned.Do_Spawn_Continuous_Waves);



                        DynamicSpawned.Wave_Spawn_Amount = EditorGUILayout.IntField(new GUIContent("Spawn Amount: "), DynamicSpawned.Wave_Spawn_Amount);
                    if (DynamicSpawned.Wave_Spawn_Amount > 100)
                        DynamicSpawned.Wave_Spawn_Amount = 100;

                    if(DynamicSpawned.Wave_Spawn_Amount < 1)
                        DynamicSpawned.Wave_Spawn_Amount = 1;
                      
                }
                

                EditorGUI.indentLevel--;

                DynamicSpawned.Do_Spawn_If_Not_In_Range = EditorGUILayout.Toggle(new GUIContent("Spawn if not in Range: ", "Spawn the Object only if the Player is in range"), DynamicSpawned.Do_Spawn_If_Not_In_Range);


                if (DynamicSpawned.Do_Spawn_If_Not_In_Range)
                {
                    EditorGUI.indentLevel++;
                    string[] PlayerIdentificationOptions = new string[3];

                    PlayerIdentificationOptions[0] = "By Tag";
                    PlayerIdentificationOptions[1] = "By Name";
                    PlayerIdentificationOptions[2] = "By Field";


                    DynamicSpawned.Selected_Player_Identification = (IdentifyPlayer)EditorGUILayout.Popup(new GUIContent("Identification: ", "How to check for the Player"), (int)DynamicSpawned.Selected_Player_Identification, PlayerIdentificationOptions);

                    switch(DynamicSpawned.Selected_Player_Identification)
                    {
                        case IdentifyPlayer.byField:
                            DynamicSpawned.Player_Identification_Data.Object = (GameObject)EditorGUILayout.ObjectField("Object: ", DynamicSpawned.Player_Identification_Data.Object, typeof(GameObject), true);
                            break;

                        case IdentifyPlayer.byName:
                            DynamicSpawned.Player_Identification_Data.Name = EditorGUILayout.TextField(new GUIContent("Name: "), DynamicSpawned.Player_Identification_Data.Name);
                            break;

                        case IdentifyPlayer.byTag:
                            string[] Tags = UnityEditorInternal.InternalEditorUtility.tags;
                            DynamicSpawned.Player_Identification_Data.Tag = EditorGUILayout.Popup(new GUIContent("Tag: "), DynamicSpawned.Player_Identification_Data.Tag,  Tags);
                            break;
                    }

                    GUIContent[] DistanceCheckingOptionsDescription = new GUIContent[3];

                    DistanceCheckingOptionsDescription[0] = new GUIContent("TwoDimensionalCheck", "Checks the Distance by the X and Z axis");
                    DistanceCheckingOptionsDescription[1] = new GUIContent("ThreeDimensionalCheck", "Checks the Distance by the X, y and Z axis");
                    DistanceCheckingOptionsDescription[2] = new GUIContent("SphereCheck", "Checks if the Player is in the Sphere rather than the pure Distance");

                    string[] Options = { "TwoDimensionalCheck", "ThreeDimensionalCheck", "SphereCheck" };

                    DynamicSpawned.Selected_Distance_Check = (DistanceCheckingStyles)EditorGUILayout.Popup(new GUIContent("Check Style: ", "How to check the Range"),(int)DynamicSpawned.Selected_Distance_Check, Options);

                    if(DynamicSpawned.Selected_Distance_Check == DistanceCheckingStyles.TwoDimensionalCheck || DynamicSpawned.Selected_Distance_Check == DistanceCheckingStyles.ThreeDimensionalCheck)
                    {
                        DynamicSpawned.Range_To_Check = EditorGUILayout.FloatField(new GUIContent("Range: "), DynamicSpawned.Range_To_Check);
                    }

                    else if(DynamicSpawned.Selected_Distance_Check == DistanceCheckingStyles.SphereColliderCheck)
                    {
                        if (DynamicSpawned.gameObject.GetComponent<SphereCollider>() == null)
                        {
                            DynamicSpawned.gameObject.AddComponent<SphereCollider>();
                        }

                        DynamicSpawned.gameObject.GetComponent<SphereCollider>().isTrigger = true;

                        DynamicSpawned.gameObject.GetComponent<SphereCollider>().radius = EditorGUILayout.FloatField(new GUIContent("Sphere Radius: "), DynamicSpawned.gameObject.GetComponent<SphereCollider>().radius);

                        DynamicSpawned.gameObject.GetComponent<SphereCollider>().hideFlags = HideFlags.HideInInspector;
                    }

                    if(DynamicSpawned.Selected_Distance_Check != DistanceCheckingStyles.SphereColliderCheck)
                    {                        
                        DestroyImmediate(DynamicSpawned.gameObject.GetComponent<SphereCollider>());
                    }

                    EditorGUI.indentLevel--;
                }

                DynamicSpawned.Do_Spawn_In_Frustum = EditorGUILayout.Toggle(new GUIContent("Spawn in Frustum: ", "Should objects spawn if they are in the Camera frustum"), DynamicSpawned.Do_Spawn_In_Frustum);

                if(!DynamicSpawned.Do_Spawn_In_Frustum)
                {
                    EditorGUI.indentLevel++;
                    DynamicSpawned.Frustum_Camera= (Camera)EditorGUILayout.ObjectField(new GUIContent("Camera: ", "Camera to check the frustum of"), DynamicSpawned.Frustum_Camera, typeof(Camera), true);
                    EditorGUI.indentLevel--;
                }


                string[] SpawnPositioningStyleOptions = new string[2];
                SpawnPositioningStyleOptions[0] = "Area";
                SpawnPositioningStyleOptions[1] = "Points";


                DynamicSpawned.Selected_Spawn_Position_Option = (PositioningOptions)EditorGUILayout.Popup(new GUIContent("Spawn Style: "), (int)DynamicSpawned.Selected_Spawn_Position_Option, SpawnPositioningStyleOptions);

                if(DynamicSpawned.Selected_Spawning_Style == SpawningStyles.Wave)
                    DynamicSpawned.Selected_Spawn_Position_Option = PositioningOptions.Area;


                switch (DynamicSpawned.Selected_Spawn_Position_Option)
                {
                    case PositioningOptions.Area:


                        for(int childIndex = 0; childIndex < DynamicSpawned.transform.childCount; childIndex++)
                        {
                            if (DynamicSpawned.transform.GetChild(childIndex).name == "SpawnArea")
                            {
                                DynamicSpawned.Spawn_Area = DynamicSpawned.transform.GetChild(childIndex).gameObject; DynamicSpawned.Spawn_Area.GetComponent<MeshCollider>().hideFlags = HideFlags.HideInInspector;
                            }
                        }

                        if(!DynamicSpawned.Spawn_Area)
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

                    case PositioningOptions.Points:

                        if (DynamicSpawned.Spawn_Area)
                            DestroyImmediate(DynamicSpawned.Spawn_Area);

                        DoShowPointPositions = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), DoShowPointPositions, "SpawnPoint Positions: ", true);

                        if(DoShowPointPositions)
                        {
                            EditorGUI.indentLevel++;

                            DesiredSpawnPositionIndex = EditorGUILayout.IntField(new GUIContent("Spawn Position Size: "), DesiredSpawnPositionIndex);

                            while(DesiredSpawnPositionIndex < DynamicSpawned.Spawn_Positions.Count)
                            {
                                DestroyImmediate(DynamicSpawned.Spawn_Positions[DynamicSpawned.Spawn_Positions.Count - 1]);
                                DynamicSpawned.Spawn_Positions.RemoveAt(DynamicSpawned.Spawn_Positions.Count -1);
                            }

                            while(DesiredSpawnPositionIndex > DynamicSpawned.Spawn_Positions.Count)
                            {
                                GameObject bufferPosition = Instantiate(Resources.Load("SpawnPosition", typeof(GameObject))) as GameObject;
                                bufferPosition.transform.SetParent(DynamicSpawned.transform);
                                bufferPosition.transform.name = "SpawnPosition " + UniqueNumber;
                                bufferPosition.transform.localPosition = new Vector3(0, 0, 0);
                                DynamicSpawned.Spawn_Positions.Add(bufferPosition);

                                UniqueNumber++;
                            }

                            for (int spawnPositionIndex = 0; spawnPositionIndex < DynamicSpawned.Spawn_Positions.Count; spawnPositionIndex++)
                            {
                                DynamicSpawned.Spawn_Positions[spawnPositionIndex] = EditorGUILayout.ObjectField("Spawn Position: ", DynamicSpawned.Spawn_Positions[spawnPositionIndex], typeof(GameObject), true) as GameObject;
                            }



                            EditorGUI.indentLevel--;
                        }




                        if (GUILayout.Button("Create Position"))
                        {
                            DesiredSpawnPositionIndex++;
                            DoShowPointPositions = true;
                            GameObject bufferPosition = Instantiate(Resources.Load("SpawnPosition", typeof(GameObject))) as GameObject;
                            bufferPosition.transform.SetParent(DynamicSpawned.transform);
                            bufferPosition.transform.name = "SpawnPosition " + UniqueNumber;
                            bufferPosition.transform.localPosition = new Vector3(0, 0, 0);
                            DynamicSpawned.Spawn_Positions.Add(bufferPosition);

                            UniqueNumber++;
                        }

                        break;


                }
            }
        }
    }

}
