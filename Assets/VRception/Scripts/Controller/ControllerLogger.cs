using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRception;
using Photon.Pun;
//using RootMotion.Demos; UNCOMMENT when you imported FinalIK for inverse kinematics of the player character

namespace VRception
{
    /// <summary>
    /// The logger provided in the VRception toolkit allows to log all online interactions taking place in-game
    /// </summary>
    public class ControllerLogger : MonoBehaviour
    {
        [HideInInspector]
        public static ControllerLogger instance;

        //// SECTION "Logger Settings"
        [Header("Logger Settings", order = 0)]
        [Helpbox("The logger provided in the VRception toolkit allows to log all online interactions taking place in-game. A MongoDB can be specified to which the toolkit stores the data in the binary JSON format. The data can then be used for replay (see replay below) or be analyzed manually or automatically via another program/script.", order = 1)]
        [Tooltip("Specify if the logging to the database is active.", order = 2)]
        public bool active = false;
        
        [Tooltip("Specify the server config object of the Mongo database.")]
        public string mongoDatabaseURL = "mongodb+srv://URL";

        [Tooltip("Specify the name of the database.")]
        public string databaseName = "VRception";

        [Tooltip("Specify the current participant ID when conducting a study. For studies with multiple participants, specify a ID that helps identifying the group of particpants.")]
        public string participantID = "not_set";

        [Tooltip("Specify the limit after which the buffer with log events gets written to the database.")]
        public int writeToDatabaseLimit = 200;

        [Tooltip("For performance reasons, one can specify to log only every specfied number of frames.")]
        [Range(1, 30)]
        public int logEveryXFrames = 5;

        //// SECTION "Logger Debugging"
        [Header("Logger Debugging", order = 0)]
        [Helpbox("For debugging and monitoring purposes, all gameobjects that have been automatically detected by the VRception toolkit for logging are listed below.", order = 1)]
        [Tooltip("List of all gameobjects that are currently logged.", order = 2)]
        public List<GameObject> currentlyLoggedGameobjects = new List<GameObject>();

        // Store the session ID that is generated automatically
        private string sessionID;

        // Store the connection to the Mongo database and the database itself
        private MongoClient client;
        private IMongoDatabase database;

        // Store BSON documents related to the logging
        private List<BsonDocument> logBuffer = new List<BsonDocument>();
        private List<BsonDocument> currentLogBuffer;
        private Dictionary<int, BsonDocument> previousObjects = new Dictionary<int, BsonDocument>();

        // Awake is called when the script instance is being loaded
        void Awake()
        {
            // Singleton reference
            ControllerLogger.instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            // Only continue if logging is active
            if(!this.active)
                return;

            // Try to start the connection to the database
            try
            {
                client = new MongoClient(this.mongoDatabaseURL);
                database = client.GetDatabase(this.databaseName);
            }
            catch (Exception) 
            {
                Debug.LogWarning("[VRception] No connection to the configured Mongo database could be established.", this);
            }

            // Are we successfully connected?
            if(!this.IsConnectedToDatabase())
                return;

            // Get all sessions that are already stored in the database
            var collection = database.GetCollection<BsonDocument>("sessions");

            // Get current timestamp
            long timestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;

            // Prepare new session in form of a BSON document
            var document = new BsonDocument {
                { "PID", participantID },
                { "timestamp", timestamp }
            };

            // Insert new session into the existing collection of sessions
            collection.InsertOne(document);

            // Get assigned session ID and store it in this class
            this.sessionID = document.GetValue("_id").ToString();

            // Send event that logging for this session has started
            BsonDocument startEvent = new BsonDocument()
            {
                {"event_name", "app_started"}
            };
            LogEvent(startEvent);

            Debug.Log("[VRception] Logging to the Mongo database " + this.databaseName + " has started under session ID " + this.sessionID, this);
        }

        // Update is called once per frame
        void Update()
        {
            // Only continue if logging is active
            if(!this.active)
                return;

            // Are we successfully connected?
            if(!this.IsConnectedToDatabase())
                return;

            // Check if logging should happen in current frame
            if (Time.frameCount % logEveryXFrames != 0)
                return;

            // Get current timestamp
            long timestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;

            // Create an empty array of BSON documents
            BsonArray gameObjectInFrame = new BsonArray();

            // Log all gameobject that currently need logging
            foreach (GameObject gameObjectToLog in currentlyLoggedGameobjects)
            {
                // If the gameobject that should be logged is null; we skip it
                if (gameObjectToLog == null)
                    continue;
                    
                // Get position and roation of gameobject to log
                Vector3 pos = gameObjectToLog.transform.position;
                Vector3 eulerAngles = gameObjectToLog.transform.rotation.eulerAngles;

                // Log space of gameobject (e.g., left, right)
                Interactable interactable = gameObjectToLog.GetComponent<Interactable>();
                string space = "no_space_set";
                if (interactable != null)
                {
                    space = interactable.GetSpace().ToString();
                }

                // Log ID of photon view
                PhotonView photonView = PhotonView.Get(gameObjectToLog);
                int photonViewID = -1;
                if (photonView != null)
                {
                    photonViewID = photonView.ViewID;
                }

                // Log name of cloneable
                Cloneable cloneable = gameObjectToLog.GetComponent<Cloneable>();
                string nameClonable = "not_given";
                if (cloneable != null)
                {
                    nameClonable = cloneable.prefabName;
                }

                // Start document for gameobject
                var document = new BsonDocument {
                    { "gameObjectName", gameObjectToLog.name },
                    { "clonable_prefabName", nameClonable },
                    { "pos_x", pos.x },
                    { "pos_y", pos.y },
                    { "pos_z", pos.z },
                    { "euler_x", eulerAngles.x },
                    { "euler_y", eulerAngles.y },
                    { "euler_z", eulerAngles.z },
                    // Scale not required because every gameobject should be an interactable and they apply scale to their child gameobject model
                    //{ "scale_x", scale.x },
                    //{ "scale_y", scale.y },
                    //{ "scale_z", scale.z },
                    //{ "lossy_scale_x", lossyScale.x },
                    //{ "lossy_scale_y", lossyScale.y },
                    //{ "lossy_scale_z", lossyScale.z },
                    { "space", space },
                    { "photonViewID", photonViewID }
                };

                //TODO the following parts are not very generic and do not utilize the attributes interactables provide
                // For example, the child of an interactbale containing the Model can be named different but the interactable will have the correct reference

                // Start document for child model
                Transform modelTransform = gameObjectToLog.transform.Find("Model");
                if (modelTransform != null)
                {
                    GameObject model = gameObjectToLog.transform.Find("Model").gameObject;

                    if (model != null)
                    {
                        var child = new BsonDocument {
                            { "gameObjectName", "Model" },
                            { "pos_x", model.transform.position.x },
                            { "pos_y", model.transform.position.y },
                            { "pos_z", model.transform.position.z },
                            { "euler_x", model.transform.rotation.eulerAngles.x },
                            { "euler_y", model.transform.rotation.eulerAngles.y },
                            { "euler_z", model.transform.rotation.eulerAngles.z },
                            { "scale_x", model.transform.localScale.x },
                            { "scale_y", model.transform.localScale.y },
                            { "scale_z", model.transform.localScale.z },
                            //{ "lossy_scale_x", model.transform.lossyScale.x },
                            //{ "lossy_scale_y", model.transform.lossyScale.y },
                            //{ "lossy_scale_z", model.transform.lossyScale.z }
                        };
                        document.Add("child", child);
                    }
                }

                // Start document for camera
                Transform cameraTransform = gameObjectToLog.transform.Find("Camera");
                if (cameraTransform != null)
                {
                    var cam = new BsonDocument {
                        { "gameObjectName", "Camera" },
                        { "pos_x", cameraTransform.position.x },
                        { "pos_y", cameraTransform.position.y },
                        { "pos_z", cameraTransform.position.z },
                        { "euler_x", cameraTransform.rotation.eulerAngles.x },
                        { "euler_y", cameraTransform.rotation.eulerAngles.y },
                        { "euler_z", cameraTransform.rotation.eulerAngles.z }
                    };
                    document.Add("camera", cam);
                }

                // Start to log players, if they use the VRIK plugin
                if (gameObjectToLog.name.Contains("Player"))
                {
                    // Distinguish between remote and local player
                    if (gameObjectToLog.name.Contains("Remote"))
                    {
                        // UNCOMMENT when you imported FinalIK for inverse kinematics of the player character
                        /*
                        // Get VRIK component of player
                        VRIK_PUN_Player player = gameObjectToLog.GetComponent<VRIK_PUN_Player>();

                        if(player != null)
                        {
                            Transform transformHead = player.headIKProxy;
                            Transform transformLeftHand = player.leftHandIKProxy;
                            Transform transformRightHand = player.rightHandIKProxy;

                            if (transformHead != null && transformLeftHand != null && transformRightHand != null)
                            {
                                var headDoc = new BsonDocument {
                                    { "gameObjectName", "HeadTarget" },
                                    { "pos_x", transformHead.position.x },
                                    { "pos_y", transformHead.position.y },
                                    { "pos_z", transformHead.position.z },
                                    { "euler_x", transformHead.rotation.eulerAngles.x },
                                    { "euler_y", transformHead.rotation.eulerAngles.y },
                                    { "euler_z", transformHead.rotation.eulerAngles.z }
                                };
                                document.Add("head", headDoc);

                                var leftHandDoc = new BsonDocument {
                                    { "gameObjectName", "LeftHandTarget" },
                                    { "pos_x", transformLeftHand.position.x },
                                    { "pos_y", transformLeftHand.position.y },
                                    { "pos_z", transformLeftHand.position.z },
                                    { "euler_x", transformLeftHand.rotation.eulerAngles.x },
                                    { "euler_y", transformLeftHand.rotation.eulerAngles.y },
                                    { "euler_z", transformLeftHand.rotation.eulerAngles.z }
                                };
                                document.Add("leftHand", leftHandDoc);

                                var rightHandDoc = new BsonDocument {
                                    { "gameObjectName", "RightHandTarget" },
                                    { "pos_x", transformRightHand.position.x },
                                    { "pos_y", transformRightHand.position.y },
                                    { "pos_z", transformRightHand.position.z },
                                    { "euler_x", transformRightHand.rotation.eulerAngles.x },
                                    { "euler_y", transformRightHand.rotation.eulerAngles.y },
                                    { "euler_z", transformRightHand.rotation.eulerAngles.z }
                                };
                                document.Add("rightHand", rightHandDoc);
                            }
                        }
                        */
                    }
                    // It is a local player
                    else
                    {
                        GameObject head = gameObjectToLog.GetComponent<ControllerPlayer>().targetHead;
                        GameObject leftHand = gameObjectToLog.GetComponent<ControllerPlayer>().targetLeftHand;
                        GameObject rightHand = gameObjectToLog.GetComponent<ControllerPlayer>().targetRightHand;

                        Transform transformHead = head.transform;
                        Transform transformLeftHand = leftHand.transform;
                        Transform transformRightHand = rightHand.transform;

                        var headDoc = new BsonDocument {
                            { "gameObjectName", "HeadTarget" },
                            { "pos_x", transformHead.position.x },
                            { "pos_y", transformHead.position.y },
                            { "pos_z", transformHead.position.z },
                            { "euler_x", transformHead.rotation.eulerAngles.x },
                            { "euler_y", transformHead.rotation.eulerAngles.y },
                            { "euler_z", transformHead.rotation.eulerAngles.z }
                        };
                        document.Add("head", headDoc);

                        var leftHandDoc = new BsonDocument {
                            { "gameObjectName", "LeftHandTarget" },
                            { "pos_x", transformLeftHand.position.x },
                            { "pos_y", transformLeftHand.position.y },
                            { "pos_z", transformLeftHand.position.z },
                            { "euler_x", transformLeftHand.rotation.eulerAngles.x },
                            { "euler_y", transformLeftHand.rotation.eulerAngles.y },
                            { "euler_z", transformLeftHand.rotation.eulerAngles.z }
                        };
                        document.Add("leftHand", leftHandDoc);

                        var rightHandDoc = new BsonDocument {
                            { "gameObjectName", "RightHandTarget" },
                            { "pos_x", transformRightHand.position.x },
                            { "pos_y", transformRightHand.position.y },
                            { "pos_z", transformRightHand.position.z },
                            { "euler_x", transformRightHand.rotation.eulerAngles.x },
                            { "euler_y", transformRightHand.rotation.eulerAngles.y },
                            { "euler_z", transformRightHand.rotation.eulerAngles.z }
                        };
                        document.Add("rightHand", rightHandDoc);
                    }
                }

                // Only send updates on change
                if (previousObjects.ContainsKey(photonViewID))
                {
                    if (!document.Equals(previousObjects[photonViewID]))
                    {
                        previousObjects[photonViewID] = document;
                        gameObjectInFrame.Add(document);
                    }
                }
                // Photon ID unknown to previous objects
                else
                {
                    previousObjects[photonViewID] = document;
                    gameObjectInFrame.Add(document);
                }
            }

            // Store frame that contains all changed gameobjects
            BsonDocument frame = new BsonDocument() {
                { "frameCount", Time.frameCount },
                { "timestamp", timestamp },
                { "session", sessionID },
                { "PID", participantID },
            };
            frame.Add("objects_in_frame", gameObjectInFrame);
            logBuffer.Add(frame);

            // Write to database if enough data is in buffer
            if (logBuffer.Count == writeToDatabaseLimit)
            {
                StartCoroutine(WriteToDatabase());
            }
        }
        
        // Sent to all GameObjects before the application quits.
        void OnApplicationQuit()
        {
            // Only continue if logging is active
            if(!this.active)
                return;

            // Are we successfully connected?
            if(!this.IsConnectedToDatabase())
                return;

            // Inform about next actions
            Debug.Log("[VRception] Writing current buffer of logging into database because the player closed the VRception toolkit");

            // Create BSON end event
            BsonDocument endEvent = new BsonDocument() 
            { 
                {"event_name", "app_closed"}
            };

            // Get current timestamp
            long timestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;

            // Add further details to end event
            endEvent.Add("timestamp", timestamp);
            endEvent.Add("session", sessionID);
            endEvent.Add("PID", participantID);
            endEvent.Add("frameCount", Time.frameCount);

            // Get current collection of events
            var collectionEvents = database.GetCollection<BsonDocument>("events");

            // Send closing event
            collectionEvents.InsertOne(endEvent);

            // Write everything that is in the buffer into the database
            currentLogBuffer = new List<BsonDocument>(logBuffer);
            logBuffer.Clear();
            var collection = database.GetCollection<BsonDocument>("objects");
            collection.InsertMany(currentLogBuffer);

            // Inform about success
            Debug.Log("[VRception] Writing buffer of logging is complete and all data has been saved");
        }

        // Method is called to register a gameobject for logging
        public void RegisterObjectForLogging(GameObject gameObject)
        {
            if(this == null)
                return;

            if(!isActiveAndEnabled)
                return;

            // Is logging active?
            if(!this.active)
                return;

            // Add gameobject to list
            currentlyLoggedGameobjects.Add(gameObject);
        }

        // Method is called to register a gameobject for logging
        public void UnregisterObjectForLogging(GameObject gameObject)
        {
            if (this == null)
                return;

            if (!isActiveAndEnabled)
                return;

            // Is logging active?
            if(!this.active)
                return;

            // Remove gameobject to list
            currentlyLoggedGameobjects.Remove(gameObject);
        }

        // Method to log an event to the database
        public void LogEvent(BsonDocument eventDocument)
        {
            if (this == null)
                return;

            if (!isActiveAndEnabled)
                return;

            // Only continue if logging is active
            if(!this.active)
                return;

            // Are we successfully connected?
            if(!this.IsConnectedToDatabase())
                return;

            long timestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
            eventDocument.Add("timestamp", timestamp);
            eventDocument.Add("session", sessionID);
            eventDocument.Add("PID", participantID);
            eventDocument.Add("frameCount", Time.frameCount);

            StartCoroutine(WriteEventToDatabase(eventDocument));
        }

        // Returns true, if the connection to the database is healthy
        private bool IsConnectedToDatabase()
        {
            // An impotant object has not been initialized?
            if(this.client == null || this.databaseName == null)
                return false;

            return true;
        }

        // Method to write current buffer into database
        private IEnumerator WriteToDatabase()
        {
            // Get current buffer and clear it
            currentLogBuffer = new List<BsonDocument>(logBuffer);
            logBuffer.Clear();

            // Get collection from database and add previously buffered objects in it
            var collection = database.GetCollection<BsonDocument>("objects");
            collection.InsertMany(currentLogBuffer);

            yield return null;
        }

        // Method to write an object to the database
        private IEnumerator WriteEventToDatabase(BsonDocument eventDocument)
        {
            // Get collection from database and add document
            var collection = database.GetCollection<BsonDocument>("events");
            collection.InsertOne(eventDocument);

            yield return null;
        }
    }
}