using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using MongoDB.Bson;
using MongoDB.Driver;

namespace VRception
{
    /// <summary>
    /// The replay feature of the VRception toolkit enables one to replay previousely logged sessions
    /// </summary>
    public class ControllerReplay : MonoBehaviour
    {
        //// SECTION "Replay Settings"
        [Header("Replay Settings", order = 0)]
        [Helpbox("The replay feature of the VRception toolkit enables one to replay previousely logged sessions. Below, one can specify a new Mongo database connection or select to reuse the connection from the Logger. Moreover, below one finds the replay controls and a list of sessions in the database for replaying.", order = 1)]
        [Tooltip("Specify the server config object of the Mongo database.", order = 2)]
        public bool useDatabaseFromLogger = true;

        [Tooltip("Specify the server config object of the Mongo database.")]
        public string mongoDatabaseURL = "mongodb+srv://URL";

        [Tooltip("Specify the name of the database.")]
        public string databaseName = "VRception";

        //// SECTION "Replay Control"
        [Header("Replay Control", order = 0)]
        [Helpbox("Below, one finds replay controls and an input field in which one can add the session from the list of replayable sessions underneath that should be replayed.", order = 1)]
        [Tooltip("Specify the session to replay from the session list below.", order = 2)]
        public string sessionToReplay = "";

        [Tooltip("Will be automatically set to true, if the session is loaded.")]
        public bool dataAvailable = false;

        [Tooltip("If yes, then the replay is running.")]
        public bool play = false;

        //// SECTION "Replay Monitoring"
        [Header("Replay Monitoring", order = 0)]
        [Helpbox("Allows one to observe all curently replayed objects and events as well as the currently instantiated gameobjects.", order = 1)]
        [Tooltip("Will be automatically filled with all objects that are currently replayed.", order = 2)]
        public List<BsonDocument> objectsForReplay = new List<BsonDocument>();

        [Tooltip("Will be automatically filled with all events that are currently replayed.")]
        public List<BsonDocument> eventsForReplay = new List<BsonDocument>();

        [Tooltip("Will be automatically filled with all currently instantiated objects.")]
        public List<GameObject> objectsInititated = new List<GameObject>();
        
        //// SECTION "Replayable Sessions"
        [Header("Replayable Sessions", order = 0)]
        [Helpbox("Shows all sessions in the specified database that are availablae for replay.", order = 1)]
        [Tooltip("All sessions available for replay.", order = 2)]
        public List<string> sessions = new List<string>();

        // Store the connection to the Mongo database and the database itself
        private MongoClient client;
        private IMongoDatabase database;

        // Store all View IDs of all initiated objects
        private List<int> objectViewIDInititated = new List<int>();
        
        // Store internal states for replaying of frames
        private int replayFrameCount = 0;
        private long runningTime = -1;
        List<int> seenFrames = new List<int>();
        int bufferFrameCount = 0;

        // Editor-only function that Unity calls when the script is loaded or a value changes in the Inspector.
        void OnValidate()
        {
            // Make sure the component is enabled
            if (!this.enabled)
                return;
            
            // Connect to database
            this.ConnectToDatabase();

            // Are we successfully connected?
            if(!this.IsConnectedToDatabase())
                return;

            // Update list with sessions
            this.sessions = new List<string>();
            var collection = database.GetCollection<BsonDocument>("sessions");
            var documents = collection.AsQueryable();
            foreach (BsonDocument document in documents)
            {
                var id = document.GetValue("_id").ToString();
                sessions.Add(id);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            // Connect to database
            this.ConnectToDatabase();

            // Are we successfully connected?
            if(!this.IsConnectedToDatabase())
                return;

            // Load the replay necessary from the database
            var thread = new System.Threading.Thread(LoadingDatabase);
            thread.Start();
        }

        // Update is called once per frame
        void Update()
        {
            // Are we successfully connected?
            if(!this.IsConnectedToDatabase())
                return;

            // Is play true and the data from the specified session is available?
            if(this.play && this.dataAvailable)
            {
                if (objectsForReplay.Count != 0)
                {
                    BsonDocument currentFrame = objectsForReplay[replayFrameCount];
                    if (runningTime == -1)
                    {
                        runningTime = currentFrame["timestamp"].AsInt64;
                    }
                    else
                    {
                        runningTime = runningTime + (long)(Time.deltaTime * 1000.0f);
                        long currentTime = currentFrame["timestamp"].AsInt64;
                        if (currentTime < runningTime)
                        {
                            replayFrameCount++;
                            BsonDocument checkFrame = objectsForReplay[replayFrameCount];
                            int currFrameCount = checkFrame["frameCount"].AsInt32;
                            while(currFrameCount < bufferFrameCount)
                            {
                                replayFrameCount++;
                                checkFrame = objectsForReplay[replayFrameCount];
                                currFrameCount = checkFrame["frameCount"].AsInt32;
                            }
                            bufferFrameCount = currFrameCount;
                        }
                    }

                    BsonArray gameObjectInFrame = currentFrame["objects_in_frame"].AsBsonArray;

                    if (!seenFrames.Contains(currentFrame["frameCount"].AsInt32))
                    {
                        seenFrames.Add(currentFrame["frameCount"].AsInt32);

                        foreach (BsonDocument doc in gameObjectInFrame)
                        {
                            // Get position 
                            float posX = (float)doc["pos_x"].AsDouble;
                            float posY = (float)doc["pos_y"].AsDouble;
                            float posZ = (float)doc["pos_z"].AsDouble;

                            // Get rotation
                            float rotX = (float)doc["euler_x"].AsDouble;
                            float rotY = (float)doc["euler_y"].AsDouble;
                            float rotZ = (float)doc["euler_z"].AsDouble;

                            // Get scale
                            //float scaleX = (float)doc["scale_x"].AsDouble;
                            //float scaleY = (float)doc["scale_y"].AsDouble;
                            //float scaleZ = (float)doc["scale_z"].AsDouble;

                            Vector3 position = new Vector3(posX, posY, posZ);
                            Vector3 rotation = new Vector3(rotX, rotY, rotZ);
                            //Vector3 scale = new Vector3(scaleX, scaleY, scaleZ);

                            // Get Photon ID
                            int photonViewID = doc["photonViewID"].AsInt32;
                            string name = doc["gameObjectName"].AsString.Replace("(Clone)", "");

                            // Get cloneable 
                            string clonablePrefabName = doc["clonable_prefabName"].AsString;
                            if (!clonablePrefabName.Equals("not_given"))
                            {
                                name = clonablePrefabName;
                            }

                            // Get space
                            Space space;
                            if (name.Contains("Player"))
                            {
                                space = Space.SHARED;
                                name = "Player";
                            }
                            else
                            {
                                space = (Space)Enum.Parse(typeof(Space), doc["space"].AsString, false);
                            }

                            if (!objectViewIDInititated.Contains(photonViewID))
                            {
                                if (!name.Equals("SimpleCube (1)") && //TODO: needs to be removed in a future version
                                    !name.Equals("SimpleCube (2)") &&
                                    !name.Equals("SimpleCube (3)"))
                                {
                                    object[] info = { space };
                                    GameObject replayObject = PhotonNetwork.Instantiate(name, position, Quaternion.Euler(rotation), 0, info);

                                    if (replayObject != null)
                                    {
                                        replayObject.name = name + "_" + photonViewID;
                                        objectsInititated.Add(replayObject);
                                        objectViewIDInititated.Add(photonViewID);

                                        // Interactables are not presented in the interface
                                        DestroyImmediate(replayObject.GetComponent<ModuleInterface>());
                                    }
                                }
                            }
                            else
                            {
                                string nameCurrentObject = name + "_" + photonViewID;
                                GameObject objectToReplay = GameObject.Find(nameCurrentObject);
                                if (objectToReplay != null)
                                {
                                    objectToReplay.transform.position = position;
                                    objectToReplay.transform.rotation = Quaternion.Euler(rotation);

                                    Transform modelTransform = objectToReplay.transform.Find("Model");
                                    if (modelTransform != null)
                                    {
                                        GameObject model = modelTransform.gameObject;
                                        if (model != null)
                                        {
                                            BsonDocument modelDoc = doc["child"].AsBsonDocument;
                                            float modelScaleX = (float)modelDoc["scale_x"].AsDouble;
                                            float modelScaleY = (float)modelDoc["scale_y"].AsDouble;
                                            float modelScaleZ = (float)modelDoc["scale_z"].AsDouble;
                                            Vector3 modelScale = new Vector3(modelScaleX, modelScaleY, modelScaleZ);

                                            model.transform.localScale = modelScale;
                                        }
                                    }

                                    Transform transformCamera = objectToReplay.transform.Find("Camera");
                                    if (transformCamera != null)
                                    {
                                        GameObject camera = transformCamera.gameObject;
                                        DestroyImmediate(camera.GetComponent<CameraAdjustable>());
                                        if (camera != null)
                                        {
                                            BsonDocument camDoc = doc["camera"].AsBsonDocument;

                                            rotX = (float)camDoc["euler_x"].AsDouble;
                                            rotY = (float)camDoc["euler_y"].AsDouble;
                                            rotZ = (float)camDoc["euler_z"].AsDouble;

                                            rotation = new Vector3(rotX, rotY, rotZ);
                                            camera.transform.rotation = Quaternion.Euler(rotation);
                                        }
                                    }

                                    if (nameCurrentObject.Contains("Player"))
                                    {
                                        GameObject head = objectToReplay.GetComponent<ControllerPlayer>().targetHead;
                                        GameObject leftHand = objectToReplay.GetComponent<ControllerPlayer>().targetLeftHand;
                                        GameObject rightHand = objectToReplay.GetComponent<ControllerPlayer>().targetRightHand;

                                        //TODO: check for potential null pointer exception
                                        Transform transformHead = head.transform;
                                        Transform transformLeftHand = leftHand.transform;
                                        Transform transformRightHand = rightHand.transform;

                                        BsonDocument headDoc = doc["head"].AsBsonDocument;
                                        BsonDocument leftHandDoc = doc["leftHand"].AsBsonDocument;
                                        BsonDocument rightHandDoc = doc["rightHand"].AsBsonDocument;

                                        SetPositionAndRotationFromDocument(transformHead, headDoc);
                                        SetPositionAndRotationFromDocument(transformLeftHand, leftHandDoc);
                                        SetPositionAndRotationFromDocument(transformRightHand, rightHandDoc);
                                    }
                                }
                            }
                        }
                    }

                    for (int i = eventsForReplay.Count - 1; i >= 0; i--)
                    {
                        BsonDocument eventDoc = eventsForReplay[i];

                        long currentTime = eventDoc["timestamp"].AsInt64;
                        if (currentTime < runningTime)
                        {
                            // Execute event
                            string eventName = eventDoc["event_name"].AsString;
                            if (eventName.Equals("deleted"))
                            {
                                int viewId = eventDoc["photonViewID"].AsInt32;
                                string nameCurrentObject = eventDoc["gameObjectName"].AsString.Replace("(Clone)", "") + "_" + viewId;

                                string clonablePrefabName = eventDoc["clonable_prefabName"].AsString;
                                if (!clonablePrefabName.Equals("not_given"))
                                {
                                    nameCurrentObject = clonablePrefabName + "_" + viewId;
                                }

                                GameObject gameObject = GameObject.Find(nameCurrentObject);
                                if (gameObject != null)
                                {
                                    PhotonNetwork.Destroy(gameObject);
                                    objectViewIDInititated.Remove(viewId);
                                    objectsInititated.Remove(gameObject);
                                    eventsForReplay.RemoveAt(i);
                                }
                            }
                        }
                    }

                    // Start the replay from the beginning
                    if (replayFrameCount == objectsForReplay.Count)
                    {
                        replayFrameCount = 0;
                        runningTime = -1;
                        bufferFrameCount = 0;

                        eventsForReplay.Clear();
                        foreach (GameObject gO in objectsInititated)
                        {
                            PhotonNetwork.Destroy(gO);
                        }

                        objectsInititated.Clear();
                        objectViewIDInititated.Clear();
                        var collectionEvents = database.GetCollection<BsonDocument>("events");
                        var filter = Builders<BsonDocument>.Filter.Eq("session", sessionToReplay);
                        var eventsToReplay = collectionEvents.Find(filter);
                        eventsForReplay.AddRange(eventsToReplay.ToList<BsonDocument>());
                    }
                }
                else
                {
                    print("[VRception] There is nothing to replay!");
                }
            }
        }

        // Method to connect to the Mongo database
        private void ConnectToDatabase()
        {
            // Are we connected already?
            if(this.IsConnectedToDatabase())
                return;

            // Try to start the connection to the database
            try
            {
                if(this.useDatabaseFromLogger)
                {
                    client = new MongoClient(ControllerLogger.instance.mongoDatabaseURL);
                    database = client.GetDatabase(ControllerLogger.instance.databaseName);

                }
                else
                {
                    client = new MongoClient(this.mongoDatabaseURL);
                    database = client.GetDatabase(this.databaseName);
                }
            }
            catch (Exception) 
            {
                Debug.LogWarning("[VRception] No connection to the configured Mongo database could be established.", this);
            }
        }

        // Returns true, if the connection to the database is healthy
        private bool IsConnectedToDatabase()
        {
            // An impotant object has not been initialized?
            if(this.client == null || this.databaseName == null)
                return false;

            return true;
        }

        // Method that loads the specified session out of the database
        private void LoadingDatabase()
        {
            // Get collections for objects and events
            var collectionObjects = database.GetCollection<BsonDocument>("objects");
            var collectionEvents = database.GetCollection<BsonDocument>("events");

            // Filter the collection with the specified session
            var filter = Builders<BsonDocument>.Filter.Eq("session", sessionToReplay);

            // Get all objects and events that match the filter
            var objectLogsToReplay = collectionObjects.Find(filter);
            var eventsToReplay = collectionEvents.Find(filter);

            // Add objects and events to monitoring variables
            objectsForReplay.AddRange(objectLogsToReplay.ToList<BsonDocument>());
            eventsForReplay.AddRange(eventsToReplay.ToList<BsonDocument>());

            Debug.Log("[VRception] We successfully loaded " + objectsForReplay.Count + " objects and " + eventsForReplay.Count + " events for replay", this);

            // Inform that the loading is complete
            dataAvailable = true;
        }

        // Apply the position and rotation from the database to the gameobjects used for replay
        private void SetPositionAndRotationFromDocument(Transform transformBodyPart, BsonDocument bodyPartDocument)
        {
            float posX = (float)bodyPartDocument["pos_x"].AsDouble;
            float posY = (float)bodyPartDocument["pos_y"].AsDouble;
            float posZ = (float)bodyPartDocument["pos_z"].AsDouble;

            float rotX = (float)bodyPartDocument["euler_x"].AsDouble;
            float rotY = (float)bodyPartDocument["euler_y"].AsDouble;
            float rotZ = (float)bodyPartDocument["euler_z"].AsDouble;

            Vector3 position = new Vector3(posX, posY, posZ);
            Vector3 rotation = new Vector3(rotX, rotY, rotZ);

            transformBodyPart.position = position;
            transformBodyPart.rotation = Quaternion.Euler(rotation);
        }
    }
}
