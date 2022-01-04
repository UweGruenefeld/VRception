//#define DEBUG_DISCARD

namespace Photon.Voice.Unity.Editor
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using Unity;
    using Realtime;

    [CustomEditor(typeof(VoiceConnection))]
    public class VoiceConnectionEditor : Editor
    {
        private VoiceConnection connection;

        private SerializedProperty updateIntervalSp;
        private SerializedProperty enableSupportLoggerSp;
        private SerializedProperty settingsSp;
        #if !UNITY_ANDROID && !UNITY_IOS
        private SerializedProperty runInBackground;
        #endif
        #if !UNITY_IOS
        private SerializedProperty keepAliveInBackgroundSp;
        #endif
        private SerializedProperty applyDontDestroyOnLoadSp;
        private SerializedProperty statsResetInterval;
        private SerializedProperty primaryRecorderSp;
        private SerializedProperty speakerPrefabSp;
        private SerializedProperty autoCreateSpeakerIfNotFoundSp;
        private SerializedProperty globalRecordersLogLevelSp;
        private SerializedProperty globalSpeakersLogLevelSp;
        private SerializedProperty globalPlayDelaySettingsSp;

        protected virtual void OnEnable()
        {
            this.connection = this.target as VoiceConnection;
            this.updateIntervalSp = this.serializedObject.FindProperty("updateInterval");
            this.enableSupportLoggerSp = this.serializedObject.FindProperty("enableSupportLogger");
            this.settingsSp = this.serializedObject.FindProperty("Settings");
            #if !UNITY_ANDROID && !UNITY_IOS
            this.runInBackground = this.serializedObject.FindProperty("runInBackground");
            #endif
            #if !UNITY_IOS
            this.keepAliveInBackgroundSp = this.serializedObject.FindProperty("KeepAliveInBackground");
            #endif
            this.applyDontDestroyOnLoadSp = this.serializedObject.FindProperty("ApplyDontDestroyOnLoad");
            this.statsResetInterval = this.serializedObject.FindProperty("statsResetInterval");
            this.primaryRecorderSp = this.serializedObject.FindProperty("primaryRecorder");
            if (this.primaryRecorderSp == null) // [FormerlySerializedAs("PrimaryRecorder")]
            {
                this.primaryRecorderSp = this.serializedObject.FindProperty("PrimaryRecorder");
            }
            this.speakerPrefabSp = this.serializedObject.FindProperty("speakerPrefab");
            this.autoCreateSpeakerIfNotFoundSp = this.serializedObject.FindProperty("AutoCreateSpeakerIfNotFound");
            this.globalRecordersLogLevelSp = this.serializedObject.FindProperty("globalRecordersLogLevel");
            this.globalSpeakersLogLevelSp = this.serializedObject.FindProperty("globalSpeakersLogLevel");
            this.globalPlayDelaySettingsSp = this.serializedObject.FindProperty("globalPlaybackDelaySettings");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.UpdateIfRequiredOrScript();

            VoiceLogger.ExposeLogLevel(this.serializedObject, this.connection);
            EditorGUI.BeginChangeCheck();
            this.connection.GlobalRecordersLogLevel = VoiceLogger.ExposeLogLevel(this.globalRecordersLogLevelSp);
            this.connection.GlobalSpeakersLogLevel = VoiceLogger.ExposeLogLevel(this.globalSpeakersLogLevelSp);
            EditorGUILayout.PropertyField(this.autoCreateSpeakerIfNotFoundSp, new GUIContent("Create Speaker If Not Found", "Auto instantiate a GameObject and attach a Speaker component to link to a remote audio stream if no candidate could be foun"));
            EditorGUILayout.PropertyField(this.updateIntervalSp, new GUIContent("Update Interval (ms)", "time [ms] between consecutive SendOutgoingCommands calls"));
            if (PhotonVoiceEditorUtils.IsInTheSceneInPlayMode(this.connection.gameObject))
            {
                this.connection.PrimaryRecorder = EditorGUILayout.ObjectField(
                    new GUIContent("Primary Recorder", "Main Recorder to be used for transmission by default"),
                    this.connection.PrimaryRecorder, typeof(Recorder), true) as Recorder;
                if (this.connection.SpeakerPrefab == null)
                {
                    EditorGUILayout.HelpBox("Speaker prefab needs to have a Speaker component in the hierarchy.", MessageType.Info);
                }
                this.connection.SpeakerPrefab = EditorGUILayout.ObjectField(new GUIContent("Speaker Prefab",
                        "Prefab that contains Speaker component to be instantiated when receiving a new remote audio source info"), this.connection.SpeakerPrefab, 
                    typeof(GameObject), false) as GameObject;
                EditorGUILayout.PropertyField(this.globalPlayDelaySettingsSp, new GUIContent("Global Playback Delay Configuration", "Remote audio stream playback delay to compensate packets latency variations."), true);
                this.connection.SetGlobalPlaybackDelaySettings(
                    this.globalPlayDelaySettingsSp.FindPropertyRelative("MinDelaySoft").intValue, 
                    this.globalPlayDelaySettingsSp.FindPropertyRelative("MaxDelaySoft").intValue,
                    this.globalPlayDelaySettingsSp.FindPropertyRelative("MaxDelayHard").intValue); 
            }
            else
            {
                EditorGUILayout.PropertyField(this.enableSupportLoggerSp, new GUIContent("Support Logger", "Logs additional info for debugging.\nUse this when you submit bugs to the Photon Team."));
                #if !UNITY_ANDROID && !UNITY_IOS
                EditorGUILayout.PropertyField(this.runInBackground, new GUIContent("Run In Background", "Sets Unity's Application.runInBackground: Should the application keep running when the application is in the background?"));
                #endif
                #if !UNITY_IOS
                EditorGUILayout.PropertyField(this.keepAliveInBackgroundSp, new GUIContent("Background Timeout (ms)", "Defines for how long the Fallback Thread should keep the connection, before it may time out as usual."));
                #endif
                EditorGUILayout.PropertyField(this.applyDontDestroyOnLoadSp, new GUIContent("Don't Destroy On Load", "Persists the GameObject across scenes using Unity's GameObject.DontDestroyOnLoad"));
                if (this.applyDontDestroyOnLoadSp.boolValue && !PhotonVoiceEditorUtils.IsPrefab(this.connection.gameObject))
                {
                    if (this.connection.transform.parent != null)
                    {
                        EditorGUILayout.HelpBox("DontDestroyOnLoad only works for root GameObjects or components on root GameObjects.", MessageType.Warning);
                        if (GUILayout.Button("Detach"))
                        {
                            this.connection.transform.parent = null;
                        }
                    }
                }
                EditorGUILayout.PropertyField(this.primaryRecorderSp,
                    new GUIContent("Primary Recorder", "Main Recorder to be used for transmission by default"));
                GameObject prefab = this.speakerPrefabSp.objectReferenceValue as GameObject;
                if (prefab == null)
                {
                    EditorGUILayout.HelpBox("Speaker prefab needs to have a Speaker component in the hierarchy.", MessageType.Info);
                }
                prefab = EditorGUILayout.ObjectField(new GUIContent("Speaker Prefab",
                        "Prefab that contains Speaker component to be instantiated when receiving a new remote audio source info"), prefab, 
                    typeof(GameObject), false) as GameObject;
                if (prefab == null || prefab.GetComponentInChildren<Speaker>() != null)
                {
                    this.speakerPrefabSp.objectReferenceValue = prefab;
                }
                else
                {
                    Debug.LogError("SpeakerPrefab must have a component of type Speaker in its hierarchy.", this);
                }
                EditorGUILayout.PropertyField(this.globalPlayDelaySettingsSp, new GUIContent("Global Playback Delay Settings", "Remote audio stream playback delay to compensate packets latency variations."), true);
            }
            if (!this.connection.Client.IsConnected)
            {
                this.DisplayAppSettings();
            }
            EditorGUILayout.PropertyField(this.statsResetInterval, new GUIContent("Stats Reset Interval (ms)", "time [ms] between statistics calculations"));

            if (EditorGUI.EndChangeCheck())
            {
                this.serializedObject.ApplyModifiedProperties();
            }

            if (PhotonVoiceEditorUtils.IsInTheSceneInPlayMode(this.connection.gameObject))
            {
                this.DisplayVoiceStats();
                this.DisplayDebugInfo(this.connection.Client);
                this.DisplayCachedVoiceInfo();
                this.DisplayTrafficStats(this.connection.Client.LoadBalancingPeer);
            }
        }

        private bool showVoiceStats;
        private bool showPlayersList;
        private bool showDebugInfo;
        private bool showCachedVoices;
        private bool showTrafficStats;

        protected virtual void DisplayVoiceStats()
        {
            this.showVoiceStats =
                EditorGUILayout.Foldout(this.showVoiceStats, new GUIContent("Voice Frames Stats", "Show frames stats"));
            if (this.showVoiceStats)
            {
                this.DrawLabel("Frames Received /s", this.connection.FramesReceivedPerSecond.ToString());
                this.DrawLabel("Frames Lost /s", this.connection.FramesLostPerSecond.ToString());
                this.DrawLabel("Frames Lost %", this.connection.FramesLostPercent.ToString());
            }
        }

        protected virtual void DisplayDebugInfo(LoadBalancingClient client)
        {
            this.showDebugInfo = EditorGUILayout.Foldout(this.showDebugInfo, new GUIContent("Client Debug Info", "Debug info for Photon client"));
            if (this.showDebugInfo)
            {
                EditorGUI.indentLevel++;
                this.DrawLabel("Client State", client.State.ToString());
                if (!string.IsNullOrEmpty(client.AppId))
                {
                    this.DrawLabel("AppId", client.AppId);
                }
                if (!string.IsNullOrEmpty(client.AppVersion))
                {
                    this.DrawLabel("AppVersion", client.AppVersion);
                }
                if (!string.IsNullOrEmpty(client.CloudRegion))
                {
                    this.DrawLabel("Current Cloud Region", client.CloudRegion);
                }
                if (client.IsConnected)
                {
                    this.DrawLabel("Current Server Address", client.CurrentServerAddress);
                }
                if (client.InRoom)
                {
                    this.DrawLabel("Room Name", client.CurrentRoom.Name);
                    this.showPlayersList = EditorGUILayout.Foldout(this.showPlayersList, new GUIContent("Players List", "List of players joined to the room"));
                    if (this.showPlayersList)
                    {
                        EditorGUI.indentLevel++;
                        foreach (Player player in client.CurrentRoom.Players.Values)
                        {
                            this.DisplayPlayerDebugInfo(player);
                            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
                        }
                        EditorGUI.indentLevel--;
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        protected virtual void DisplayPlayerDebugInfo(Player player)
        {
            this.DrawLabel("Actor Number", player.ActorNumber.ToString());
            if (!string.IsNullOrEmpty(player.UserId))
            {
                this.DrawLabel("UserId", player.UserId);
            }
            if (!string.IsNullOrEmpty(player.NickName))
            {
                this.DrawLabel("NickName", player.NickName);
            }
            if (player.IsMasterClient)
            {
                EditorGUILayout.LabelField("Master Client");
            }
            if (player.IsLocal)
            {
                EditorGUILayout.LabelField("Local");
            }
            if (player.IsInactive)
            {
                EditorGUILayout.LabelField("Inactive");
            }
        }

        protected virtual void DisplayCachedVoiceInfo()
        {
            this.showCachedVoices =
                EditorGUILayout.Foldout(this.showCachedVoices, new GUIContent("Cached Remote Voices' Info", "Show remote voices info cached by local client"));
            if (this.showCachedVoices)
            {
                List<RemoteVoiceLink> cachedVoices = this.connection.CachedRemoteVoices;
                Speaker[] speakers = FindObjectsOfType<Speaker>();
                for (int i = 0; i < cachedVoices.Count; i++)
                {
                    //VoiceInfo info = cachedVoices[i].Info;
                    EditorGUI.indentLevel++;
                    this.DrawLabel("Voice #", cachedVoices[i].VoiceId.ToString());
                    this.DrawLabel("Player #", cachedVoices[i].PlayerId.ToString());
                    this.DrawLabel("Channel #", cachedVoices[i].ChannelId.ToString());
                    if (cachedVoices[i].Info.UserData != null)
                    {
                        this.DrawLabel("UserData: ", cachedVoices[i].Info.UserData.ToString());
                    }
                    bool linked = false;
                    for (int j = 0; j < speakers.Length; j++)
                    {
                        Speaker speaker = speakers[j];
                        if (speaker.IsLinked && speaker.RemoteVoiceLink.PlayerId == cachedVoices[i].PlayerId &&
                            speaker.RemoteVoiceLink.VoiceId == cachedVoices[i].VoiceId)
                        {
                            linked = true;
                            EditorGUILayout.ObjectField(new GUIContent("Linked Speaker"), speaker, typeof(Speaker), false);
                            break;
                        }
                    }
                    if (!linked)
                    {
                        EditorGUILayout.LabelField("Not Linked");
                    }
                    EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
                    EditorGUI.indentLevel--;
                }
            }
        }

        #if DEBUG_DISCARD
        private int maxDeltaUnreliableNumber;
        private int maxCountDiscarded;
        #endif

        // inspired by PhotonVoiceStatsGui.TrafficStatsWindow
        protected virtual void DisplayTrafficStats(LoadBalancingPeer peer)
        {

            this.showTrafficStats = EditorGUILayout.Foldout(this.showTrafficStats, new GUIContent("Traffic Stats", "Traffic Statistics for Photon Client"));
            if (this.showTrafficStats)
            {
                #if DEBUG_DISCARD
                if (peer.DeltaUnreliableNumber > this.maxDeltaUnreliableNumber) this.maxDeltaUnreliableNumber = peer.DeltaUnreliableNumber;
                if (peer.CountDiscarded > this.maxCountDiscarded) this.maxCountDiscarded = peer.CountDiscarded;
                GUILayout.Label(string.Format("Discarded: {0} (max: {1}) UnreliableDelta: {2} (max: {3})",peer.CountDiscarded, this.maxCountDiscarded, peer.DeltaUnreliableNumber, maxDeltaUnreliableNumber));
                #endif

                peer.TrafficStatsEnabled = EditorGUILayout.Toggle(new GUIContent("Enabled", "Enable or disable traffic Statistics for Photon Peer"), peer.TrafficStatsEnabled);
                if (peer.TrafficStatsEnabled)
                {
                    GUILayout.Box("Game Level Stats");
                    var gls = peer.TrafficStatsGameLevel;
                    long elapsedSeconds = peer.TrafficStatsElapsedMs / 1000;
                    if (elapsedSeconds == 0)
                    {
                        elapsedSeconds = 1;
                    }
                    GUILayout.Label(string.Format("Time elapsed: {0} seconds", elapsedSeconds));
                    GUILayout.Label(string.Format("Total: Out {0,4} | In {1,4} | Sum {2,4}", 
                        gls.TotalOutgoingMessageCount, 
                        gls.TotalIncomingMessageCount, 
                        gls.TotalMessageCount));
                    GUILayout.Label(string.Format("Average: Out {0,4} | In {1,4} | Sum {2,4}", 
                        gls.TotalOutgoingMessageCount / elapsedSeconds, 
                        gls.TotalIncomingMessageCount / elapsedSeconds, 
                        gls.TotalMessageCount / elapsedSeconds));
                    GUILayout.Box("Packets Stats");
                    GUILayout.Label(string.Concat("Incoming: \n", peer.TrafficStatsIncoming));
                    GUILayout.Label(string.Concat("Outgoing: \n", peer.TrafficStatsOutgoing));
                    GUILayout.Box("Health Stats");
                    GUILayout.Label(string.Format("ping: {0}[+/-{1}]ms resent:{2}", 
                        peer.RoundTripTime,
                        peer.RoundTripTimeVariance,
                        peer.ResentReliableCommands));
                    GUILayout.Label(string.Format(
                        "max ms between\nsend: {0,4} \ndispatch: {1,4} \nlongest dispatch for: \nev({3}):{2,3}ms \nop({5}):{4,3}ms",
                        gls.LongestDeltaBetweenSending,
                        gls.LongestDeltaBetweenDispatching,
                        gls.LongestEventCallback,
                        gls.LongestEventCallbackCode,
                        gls.LongestOpResponseCallback,
                        gls.LongestOpResponseCallbackOpCode));
                    if (GUILayout.Button("Reset"))
                    {
                        peer.TrafficStatsReset();
                    }
                }
            }
        }

        private void DrawLabel(string prefix, string text)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(prefix);
            EditorGUILayout.LabelField(text);
            EditorGUILayout.EndHorizontal();
        }

        protected virtual void DisplayAppSettings()
        {
            this.connection.ShowSettings = EditorGUILayout.Foldout(this.connection.ShowSettings, new GUIContent("App Settings", "Settings to be used by this voice connection"));
            if (this.connection.ShowSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                SerializedProperty sP = this.settingsSp.FindPropertyRelative("AppIdVoice");
                EditorGUILayout.PropertyField(sP);
                string appId = sP.stringValue;
                string url = "https://dashboard.photonengine.com/en-US/PublicCloud";
                if (!string.IsNullOrEmpty(appId))
                {
                    url = string.Concat("https://dashboard.photonengine.com/en-US/App/Manage/", appId);
                }
                if (GUILayout.Button("Dashboard", EditorStyles.miniButton, GUILayout.Width(70)))
                {
                    Application.OpenURL(url);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(this.settingsSp.FindPropertyRelative("AppVersion"));
                EditorGUILayout.PropertyField(this.settingsSp.FindPropertyRelative("UseNameServer"), new GUIContent("Use Name Server", "Photon Cloud requires this checked.\nUncheck for Photon Server SDK (OnPremises)."));
                EditorGUILayout.PropertyField(this.settingsSp.FindPropertyRelative("FixedRegion"), new GUIContent("Fixed Region", "Photon Cloud setting, needs a Name Server.\nDefine one region to always connect to.\nLeave empty to use the best region from a server-side region list."));
                EditorGUILayout.PropertyField(this.settingsSp.FindPropertyRelative("Server"), new GUIContent("Server", "Typically empty for Photon Cloud.\nFor Photon Server, enter your host name or IP. Also uncheck \"Use Name Server\" for older Photon Server versions."));
                EditorGUILayout.PropertyField(this.settingsSp.FindPropertyRelative("Port"), new GUIContent("Port", "Use 0 for Photon Cloud.\nOnPremise uses 5055 for UDP and 4530 for TCP."));
                EditorGUILayout.PropertyField(this.settingsSp.FindPropertyRelative("ProxyServer"), new GUIContent("Proxy Server", "HTTP Proxy Server for WebSocket connection. See LoadBalancingClient.ProxyServerAddress for options."));
                EditorGUILayout.PropertyField(this.settingsSp.FindPropertyRelative("Protocol"), new GUIContent("Protocol", "Use UDP where possible.\nWSS works on WebGL and Xbox exports.\nDefine WEBSOCKET for use on other platforms."));
                EditorGUILayout.PropertyField(this.settingsSp.FindPropertyRelative("EnableProtocolFallback"), new GUIContent("Protocol Fallback", "Automatically try another network protocol, if initial connect fails.\nWill use default Name Server ports."));
                EditorGUILayout.PropertyField(this.settingsSp.FindPropertyRelative("EnableLobbyStatistics"), new GUIContent("Lobby Statistics", "When using multiple room lists (lobbies), the server can send info about their usage."));
                EditorGUILayout.PropertyField(this.settingsSp.FindPropertyRelative("NetworkLogging"), new GUIContent("Network Logging", "Log level for the Photon libraries."));
                EditorGUI.indentLevel--;

                #region Best Region Box

                GUIStyle verticalBoxStyle = new GUIStyle("HelpBox") { padding = new RectOffset(6, 6, 6, 6) };
                EditorGUILayout.BeginVertical(verticalBoxStyle);

                string prefLabel;
                const string notAvailableLabel = "n/a";
                string bestRegionSummaryInPrefs = this.connection.BestRegionSummaryInPreferences;
                if (!string.IsNullOrEmpty(bestRegionSummaryInPrefs))
                {
                    string[] regionsPrefsList = bestRegionSummaryInPrefs.Split(';');
                    if (regionsPrefsList.Length == 0 || string.IsNullOrEmpty(regionsPrefsList[0]))
                    {
                        prefLabel = notAvailableLabel;
                    }
                    else
                    {
                        prefLabel = string.Format("'{0}' ping:{1}ms ", regionsPrefsList[0], regionsPrefsList[1]);
                    }
                }
                else
                {
                    prefLabel = notAvailableLabel;
                }

                EditorGUILayout.LabelField(new GUIContent(string.Concat("Best Region Preference: ", prefLabel), "Best region is used if Fixed Region is empty."));

                EditorGUILayout.BeginHorizontal();

                Rect resetRect = EditorGUILayout.GetControlRect(GUILayout.MinWidth(64));
                Rect editRect = EditorGUILayout.GetControlRect(GUILayout.MinWidth(64));
                if (GUI.Button(resetRect, "Reset", EditorStyles.miniButton))
                {
                    this.connection.BestRegionSummaryInPreferences = null;
                }

                if (!string.IsNullOrEmpty(appId) && GUI.Button(editRect, "Edit Regions WhiteList", EditorStyles.miniButton))
                {
                        url = string.Concat("https://dashboard.photonengine.com/en-US/App/RegionsWhitelistEdit/", appId);
                        Application.OpenURL(url);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                #endregion Best Region Box
            }
        }
    }
}