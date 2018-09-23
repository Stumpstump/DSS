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
        /// Position the Object can spawn at
        /// </summary>
        public List<GameObject> spawnPositions = new List<GameObject>();

        /// <summary>
        /// Area in which objects can spawn
        /// </summary>
        public GameObject spawnArea;

        /// <summary>
        /// The way the Position of the Spawned Objects get decided
        /// /// </summary>
        public PositioningOptions spawnPositionOptions;

        /// <summary>
        /// The Style Objects get spawned Waves etc.
        /// </summary>
        public SpawnStyle spawnStyle;

        /// <summary>
        /// The way the Player gets identificated and a struct with the data for it
        /// </summary>
        public IdentifyPlayer playerIndetification;
        public Identification identificationData;

        /// <summary>
        /// The test Object which we are spawning.
        /// </summary>
        public GameObject SpawnObject;

        /// <summary>
        /// TestSpawnCheckings
        /// </summary>
        public SpawnSettings TestSpawnSettings;

        /// <summary>
        /// The Object to check the range of
        /// </summary>
        public GameObject Player;

        /// <summary>
        /// They way to check the distance of the Player to the Spawner
        /// </summary>
        public DistanceCheckingStyles ObjectDistanceCheck;

        /// <summary>
        /// Is the player in range of the spawner
        /// </summary>
        public bool isInRange = false;

        /// <summary>
        /// The Range to check for used with 2D and 3D check
        /// </summary>
        public float RangeToCheck;

        /// <summary>
        /// Should the Object spawn if its in the player frustum.
        /// </summary>
        public bool spawnInFrustum = true;

        /// <summary>
        /// Camera to check the frustum.
        /// </summary>
        public Camera frustumCamera;

        #endregion

        #region Private Fields

        /// <summary>
        /// Counter of the current spawn interval time
        /// </summary>
        float SpawnInterval;

        #endregion

        #region Private Methods        

        void Start()
        {
            InitializeObjectstoCheck();         
        }

        void Update()
        {
            InitializeObjectstoCheck();
            SpawnInterval += Time.deltaTime;

            
            if(TestSpawnSettings.SpawnIfInRange && Player)
            {
                switch (ObjectDistanceCheck)
                {
                    case DistanceCheckingStyles.TwoDimensionalCheck:
                        isInRange = DistanceChecking.TwoDimensionalCheck(transform, Player.transform, RangeToCheck);
                        break;

                    case DistanceCheckingStyles.ThreeDimensionalCheck:
                        isInRange = DistanceChecking.ThreeDimensionalCheck(transform, Player.transform, RangeToCheck);
                        break;
                }
            }

            if (TestSpawnSettings.SpawnIfInRange && isInRange || !TestSpawnSettings.SpawnIfInRange)
            {
                if(SpawnInterval >= TestSpawnSettings.SpawnDelay)
                {
                    SpawnInterval = 0f;

                    if (spawnStyle == SpawnStyle.Severally)
                    {
                        switch(spawnPositionOptions)
                        {
                            case PositioningOptions.Area:
                                this.SpawnSeverallInArea();
                                break;

                            case PositioningOptions.Points:
                                this.SpawnSeverallInARandomPositions();
                                break;
                        }

                    }

        
                }
            }
        }
        
        void OnTriggerEnter(Collider collider)
        {
            if(ObjectDistanceCheck == DistanceCheckingStyles.SphereColliderCheck)
                if (collider.gameObject == Player)
                    isInRange = true;
        }

        void OnTriggerExit(Collider collider)
        {
            if (ObjectDistanceCheck == DistanceCheckingStyles.SphereColliderCheck)
                if (collider.gameObject == Player)
                    isInRange = false;
        }

        #endregion

        #region Dynamic Spawner Methods

        void InitializeObjectstoCheck()
        {
            switch(playerIndetification)
            {
                case IdentifyPlayer.byField:
                    Player = identificationData.Object;
                    break;

                case IdentifyPlayer.byName:
                    Player = GameObject.Find(identificationData.Name);
                    break;

                case IdentifyPlayer.byTag:
                    string Tag = UnityEditorInternal.InternalEditorUtility.tags[identificationData.Tag];
                    Player = GameObject.FindWithTag(Tag);
                    break;
            }
        }

        void SpawnSeverallInArea()
        {
            GameObject bufferObject = Instantiate<GameObject>(SpawnObject, new Vector3(spawnArea.GetComponent<SpawnArea>().GetRandomPosition().x, SpawnObject.transform.position.y, spawnArea.GetComponent<SpawnArea>().GetRandomPosition().z), SpawnObject.transform.rotation);
            if (!spawnInFrustum)
                if (IsVisible(bufferObject))
                    Destroy(bufferObject);
        }

        void SpawnSeverallInARandomPositions()
        {
            GameObject bufferObject;
            int Position = Random.Range(0, spawnPositions.Count);
            Vector3 RandomPosition = new Vector3(spawnPositions[Position].GetComponent<SpawnPosition>().GetSpawnPosition().x, spawnPositions[Position].GetComponent<SpawnPosition>().GetSpawnPosition().y, spawnPositions[Position].GetComponent<SpawnPosition>().GetSpawnPosition().z);
            bufferObject = Instantiate<GameObject>(SpawnObject, RandomPosition, SpawnObject.transform.rotation);

            if(!spawnInFrustum)
                if (IsVisible(bufferObject))
                    Destroy(bufferObject);

        }

        bool IsVisible(GameObject checkObject)
        {
            Plane[] CameraBounds = GeometryUtility.CalculateFrustumPlanes(frustumCamera);
            return GeometryUtility.TestPlanesAABB(CameraBounds, checkObject.GetComponent<MeshRenderer>().bounds);                 
        }

        #endregion
    }



    /// <summary>
    /// Class for personalization the editor, ill document this later on
    /// </summary>
    [CustomEditor(typeof(DynamicSpawningSystem))]
    public class DynamicScriptEditor : Editor
    {
        int UniqueNumber = 0;

        bool DoShowPointPositions = true;

        bool DoShowTestGameSettingsContent = true;

        int SpawnPointPositionsArraySize;

        int DesiredSpawnPositionIndex;

        override public void OnInspectorGUI()
        {
            var DynamicSpawned = target as DynamicSpawningSystem;

            DesiredSpawnPositionIndex = DynamicSpawned.spawnPositions.Count;

            EditorGUILayout.Toggle("In range:", DynamicSpawned.isInRange);

            DoShowTestGameSettingsContent = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), DoShowTestGameSettingsContent, "Test Game Settings", true);


            DynamicSpawned.SpawnObject = (GameObject)EditorGUILayout.ObjectField("Object:", DynamicSpawned.SpawnObject, typeof(GameObject), true);


            if (DoShowTestGameSettingsContent)
            {
                string[] SpawnStyleOptions = new string[2];

                SpawnStyleOptions[0] = "Waves";
                SpawnStyleOptions[1] = "Severally";

                DynamicSpawned.spawnStyle = (SpawnStyle)EditorGUILayout.Popup(new GUIContent("Spawn Style: "), (int)DynamicSpawned.spawnStyle, SpawnStyleOptions);

                DynamicSpawned.TestSpawnSettings.SpawnDelay = EditorGUILayout.FloatField(new GUIContent("Spawn Delay", "In Seconds"), DynamicSpawned.TestSpawnSettings.SpawnDelay);

      

                if (DynamicSpawned.TestSpawnSettings.SpawnDelay < 0f)
                    DynamicSpawned.TestSpawnSettings.SpawnDelay *= -1;
            
                DynamicSpawned.TestSpawnSettings.SpawnIfInRange = EditorGUILayout.Toggle(new GUIContent("Spawn if in Range", "Spawn the Object only if the Player is in range"), DynamicSpawned.TestSpawnSettings.SpawnIfInRange);


                if (DynamicSpawned.TestSpawnSettings.SpawnIfInRange)
                {
                    EditorGUI.indentLevel++;
                    string[] PlayerIdentificationOptions = new string[3];

                    PlayerIdentificationOptions[0] = "By Tag";
                    PlayerIdentificationOptions[1] = "By Name";
                    PlayerIdentificationOptions[2] = "By Field";


                    DynamicSpawned.playerIndetification = (IdentifyPlayer)EditorGUILayout.Popup(new GUIContent("Identification: ", "How to check for the Player"), (int)DynamicSpawned.playerIndetification, PlayerIdentificationOptions);

                    switch(DynamicSpawned.playerIndetification)
                    {
                        case IdentifyPlayer.byField:
                            DynamicSpawned.identificationData.Object = (GameObject)EditorGUILayout.ObjectField("Object:", DynamicSpawned.identificationData.Object, typeof(GameObject), true);
                            break;

                        case IdentifyPlayer.byName:
                            DynamicSpawned.identificationData.Name = EditorGUILayout.TextField(new GUIContent("Name: "), DynamicSpawned.identificationData.Name);
                            break;

                        case IdentifyPlayer.byTag:
                            string[] Tags = UnityEditorInternal.InternalEditorUtility.tags;
                            DynamicSpawned.identificationData.Tag = EditorGUILayout.Popup(new GUIContent("Tag: "), DynamicSpawned.identificationData.Tag,  Tags);
                            break;
                    }

                    GUIContent[] DistanceCheckingOptionsDescription = new GUIContent[3];

                    DistanceCheckingOptionsDescription[0] = new GUIContent("TwoDimensionalCheck", "Checks the Distance by the X and Z axis");
                    DistanceCheckingOptionsDescription[1] = new GUIContent("ThreeDimensionalCheck", "Checks the Distance by the X, y and Z axis");
                    DistanceCheckingOptionsDescription[2] = new GUIContent("SphereCheck", "Checks if the Player is in the Sphere rather than the pure Distance");

                    string[] Options = { "TwoDimensionalCheck", "ThreeDimensionalCheck", "SphereCheck" };

                    //GUILayoutOption[]

                    DynamicSpawned.ObjectDistanceCheck = (DistanceCheckingStyles)EditorGUILayout.Popup(new GUIContent("Check Style", "How to check the Range"),(int)DynamicSpawned.ObjectDistanceCheck, Options);

                    if(DynamicSpawned.ObjectDistanceCheck == DistanceCheckingStyles.TwoDimensionalCheck || DynamicSpawned.ObjectDistanceCheck == DistanceCheckingStyles.ThreeDimensionalCheck)
                    {
                        DynamicSpawned.RangeToCheck = EditorGUILayout.FloatField(new GUIContent("Range: "), DynamicSpawned.RangeToCheck);
                    }

                    else if(DynamicSpawned.ObjectDistanceCheck == DistanceCheckingStyles.SphereColliderCheck)
                    {

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

                    EditorGUI.indentLevel--;
                }

                DynamicSpawned.spawnInFrustum = EditorGUILayout.Toggle(new GUIContent("Spawn in Frustum: ", "Should objects spawn if they are in the Camera frustum"), DynamicSpawned.spawnInFrustum);

                if(!DynamicSpawned.spawnInFrustum)
                {
                    DynamicSpawned.frustumCamera = (Camera)EditorGUILayout.ObjectField(new GUIContent("Camera:", "Camera to check the frustum of"), DynamicSpawned.frustumCamera, typeof(Camera), true);
                }


                string[] SpawnPositioningStyleOptions = new string[2];
                SpawnPositioningStyleOptions[0] = "Area";
                SpawnPositioningStyleOptions[1] = "Points";


                DynamicSpawned.spawnPositionOptions = (PositioningOptions)EditorGUILayout.Popup(new GUIContent("Spawn Style: "), (int)DynamicSpawned.spawnPositionOptions, SpawnPositioningStyleOptions);

                switch(DynamicSpawned.spawnPositionOptions)
                {
                    case PositioningOptions.Area:


                        for(int childIndex = 0; childIndex < DynamicSpawned.transform.childCount; childIndex++)
                        {
                            if (DynamicSpawned.transform.GetChild(childIndex).name == "SpawnArea")
                            {
                                DynamicSpawned.spawnArea = DynamicSpawned.transform.GetChild(childIndex).gameObject; DynamicSpawned.spawnArea.GetComponent<MeshCollider>().hideFlags = HideFlags.HideInInspector;
                            }
                        }

                        if(!DynamicSpawned.spawnArea)
                        {

                            DynamicSpawned.spawnArea = Instantiate(Resources.Load("SpawnArea", typeof(GameObject))) as GameObject; //Resources.Load<GameObject>("Assets /DynamicSpawningSystem/SpawnArea");
                            DynamicSpawned.spawnArea.transform.SetParent(DynamicSpawned.transform);
                            DynamicSpawned.spawnArea.transform.name = "SpawnArea";
                            DynamicSpawned.spawnArea.transform.localPosition = new Vector3(0, 0, 0);
                            DynamicSpawned.spawnArea.GetComponent<MeshCollider>().hideFlags = HideFlags.HideInInspector;
                            DynamicSpawned.spawnArea.GetComponent<MeshFilter>().hideFlags = HideFlags.HideInInspector;
                            DynamicSpawned.spawnArea.GetComponent<SpawnArea>().hideFlags = HideFlags.HideInInspector;
                        }                        
                        break;

                    case PositioningOptions.Points:

                        if (DynamicSpawned.spawnArea)
                            DestroyImmediate(DynamicSpawned.spawnArea);

                        DoShowPointPositions = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), DoShowPointPositions, "SpawnPoint Positions", true);

                        if(DoShowPointPositions)
                        {
                            EditorGUI.indentLevel++;

                            DesiredSpawnPositionIndex = EditorGUILayout.IntField(new GUIContent("Spawn Position Size"), DesiredSpawnPositionIndex);

                            while(DesiredSpawnPositionIndex < DynamicSpawned.spawnPositions.Count)
                            {
                                DestroyImmediate(DynamicSpawned.spawnPositions[DynamicSpawned.spawnPositions.Count - 1]);
                                DynamicSpawned.spawnPositions.RemoveAt(DynamicSpawned.spawnPositions.Count -1);
                            }

                            while(DesiredSpawnPositionIndex > DynamicSpawned.spawnPositions.Count)
                            {
                                GameObject bufferPosition = Instantiate(Resources.Load("SpawnPosition", typeof(GameObject))) as GameObject;
                                bufferPosition.transform.SetParent(DynamicSpawned.transform);
                                bufferPosition.transform.name = "SpawnPosition " + UniqueNumber;
                                bufferPosition.transform.localPosition = new Vector3(0, 0, 0);
                                DynamicSpawned.spawnPositions.Add(bufferPosition);

                                UniqueNumber++;
                            }

                            for (int spawnPositionIndex = 0; spawnPositionIndex < DynamicSpawned.spawnPositions.Count; spawnPositionIndex++)
                            {
                                DynamicSpawned.spawnPositions[spawnPositionIndex] = EditorGUILayout.ObjectField("Spawn Position: ", DynamicSpawned.spawnPositions[spawnPositionIndex], typeof(GameObject), true) as GameObject;
                            }



                            EditorGUI.indentLevel--;
                        }

                        //GUILayoutOption Button = GUILayout.Button()

                        if (GUILayout.Button("Create Position"))
                        {
                            DesiredSpawnPositionIndex++;
                            GameObject bufferPosition = Instantiate(Resources.Load("SpawnPosition", typeof(GameObject))) as GameObject;
                            bufferPosition.transform.SetParent(DynamicSpawned.transform);
                            bufferPosition.transform.name = "SpawnPosition " + UniqueNumber;
                            bufferPosition.transform.localPosition = new Vector3(0, 0, 0);
                            DynamicSpawned.spawnPositions.Add(bufferPosition);

                            UniqueNumber++;
                        }

                        break;


                }
            }
        }
    }

}
