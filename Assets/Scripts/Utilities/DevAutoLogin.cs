using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Development helper that bypasses external login and auto-connects to Photon
// when DevAutoLogin.DevMode is enabled in the Editor. Generates a deterministic
// dummy user id so you can run multiple local instances for testing.
public class DevAutoLogin : MonoBehaviourPunCallbacks
{
    public static bool DevMode = false;
    private const string PrefKey = "DevAutoLogin.DevMode";

    [Tooltip("When true, this instance will automatically join/create a room named 'DevLocalRoom'.")]
    public bool autoJoinRoom = true;

    [Tooltip("Optional explicit fake username. If empty, a deterministic one will be generated.")]
    public string fakeUserName = string.Empty;

    [Tooltip("If true, the script will attempt to connect automatically on Awake when DevMode is enabled.")]
    public bool connectOnAwake = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RuntimeInit()
    {
    // Only enable DevAutoLogin in Editor when toggled, or in Development builds
#if UNITY_EDITOR
    bool enable = EditorPrefs.GetBool(PrefKey, false);
#else
    bool enable = Debug.isDebugBuild;
#endif
    if (!enable) return;
    DevMode = enable;
        if (GameObject.Find("DevAutoLogin") != null) return;
        var go = new GameObject("DevAutoLogin");
        DontDestroyOnLoad(go);
        go.AddComponent<DevAutoLogin>();
    }

    private void Awake()
    {
    // Refresh DevMode flag based on environment
#if UNITY_EDITOR
    DevMode = EditorPrefs.GetBool(PrefKey, DevMode);
#else
    DevMode = Debug.isDebugBuild;
#endif
        if (!DevMode) return;

        DontDestroyOnLoad(gameObject);

        if (connectOnAwake)
        {
            StartConnection();
        }
    }

    public void StartConnection()
    {
        if (!DevMode) return;

        EnsurePlayerName();

        // Ensure we have a deterministic USER_ID in Dev for UI/API code paths
        EnsureDevUserId();

        if (PhotonNetwork.IsConnected)
        {
            TryJoinDevRoom();
            return;
        }

        PhotonNetwork.NickName = GetFakeUserName();
        // Provide a UserId to Photon so Player.UserId is available for display
        string uid = PlayerPrefs.GetInt("USER_ID", 0).ToString();
        PhotonNetwork.AuthValues = new AuthenticationValues(uid);
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.ConnectUsingSettings();
    }

    private void EnsurePlayerName()
    {
        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
        {
            PhotonNetwork.NickName = GetFakeUserName();
        }
    }

    private string GetFakeUserName()
    {
        if (!string.IsNullOrEmpty(fakeUserName)) return fakeUserName;
        string host = Environment.MachineName ?? "LocalHost";
        string id = SystemInfo.deviceUniqueIdentifier ?? Guid.NewGuid().ToString("N").Substring(0,6);
        int pid = 0;
        try { pid = System.Diagnostics.Process.GetCurrentProcess().Id; } catch { pid = 0; }
        return $"Dev_{host}_{id}_{pid}";
    }

    private void EnsureDevUserId()
    {
        if (!PlayerPrefs.HasKey("USER_ID"))
        {
            // Create a stable positive int from device id + pid
            string deviceId = SystemInfo.deviceUniqueIdentifier ?? Guid.NewGuid().ToString("N");
            int pid = 0;
            try { pid = System.Diagnostics.Process.GetCurrentProcess().Id; } catch { pid = 0; }
            int hash = (deviceId.GetHashCode() ^ pid) & 0x7fffffff; // positive
            if (hash == 0) hash = UnityEngine.Random.Range(1000, 999999);
            PlayerPrefs.SetInt("USER_ID", hash);
            PlayerPrefs.Save();
        }
    }

    public override void OnConnectedToMaster()
    {
        UnityEngine.Debug.Log("DevAutoLogin: Connected to Photon Master server as " + PhotonNetwork.NickName);
        if (!DevMode) return;
        if (autoJoinRoom) TryJoinDevRoom();
    }

    void TryJoinDevRoom()
    {
        string roomName = "DevLocalRoom";
        RoomOptions options = new RoomOptions { MaxPlayers = 4, IsVisible = true, IsOpen = true };
        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
        UnityEngine.Debug.Log("DevAutoLogin: Joining/Creating room: " + roomName);
    }

    public override void OnJoinedRoom()
    {
        UnityEngine.Debug.Log("DevAutoLogin: Joined room. Players in room: " + PhotonNetwork.CurrentRoom.PlayerCount);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        UnityEngine.Debug.LogWarning($"DevAutoLogin: Join room failed ({returnCode}): {message}");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        UnityEngine.Debug.LogWarning("DevAutoLogin: Disconnected from Photon: " + cause);
    }
}
