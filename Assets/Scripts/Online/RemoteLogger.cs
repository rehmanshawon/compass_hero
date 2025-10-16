using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// RemoteLogger captures Unity logs into an in-memory circular buffer and can POST them
/// to a configured HTTP endpoint. Initialized automatically before the first scene loads.
/// Use RemoteLogger.Instance.UploadLogsNow(url) to send the buffered logs.
/// </summary>
public class RemoteLogger : MonoBehaviour
{
    public static RemoteLogger Instance { get; private set; }

    [Tooltip("Maximum number of log entries to keep in memory")]
    public int maxEntries = 300;

    Queue<LogEntry> buffer;

    [Serializable]
    class LogEntry
    {
        public string timestamp;
        public string type;
        public string condition;
        public string stackTrace;
    }

    [Serializable]
    class LogPayload
    {
        public string buildVersion;
        public string sessionId;
        public string platform;
        public List<LogEntry> logs;
    }

    // Ensure this runs before any scene code that may need logging
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitializeOnLoad()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("_RemoteLogger");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<RemoteLogger>();
            Instance.Init();
        }
    }

    void Init()
    {
        if (buffer == null)
            buffer = new Queue<LogEntry>(maxEntries);
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string condition, string stackTrace, LogType type)
    {
        if (buffer == null) buffer = new Queue<LogEntry>(maxEntries);

        if (buffer.Count >= maxEntries)
            buffer.Dequeue();

        buffer.Enqueue(new LogEntry
        {
            timestamp = DateTime.UtcNow.ToString("o"),
            type = type.ToString(),
            condition = condition,
            stackTrace = stackTrace
        });
    }

    /// <summary>
    /// Start uploading buffered logs to the specified endpoint.
    /// The payload is JSON and contains build version, session id and the buffered logs.
    /// </summary>
    public void UploadLogsNow(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("RemoteLogger: Upload URL is empty; skipping upload.");
            return;
        }

        StartCoroutine(UploadCoroutine(url));
    }

    IEnumerator UploadCoroutine(string url)
    {
        if (buffer == null || buffer.Count == 0)
        {
            Debug.Log("RemoteLogger: no logs to upload.");
            yield break;
        }

        // Copy current buffer to list to avoid locking it while uploading
        List<LogEntry> toSend = new List<LogEntry>(buffer);

        LogPayload payload = new LogPayload
        {
            buildVersion = Application.version,
            sessionId = SystemInfo.deviceUniqueIdentifier,
            platform = Application.platform.ToString(),
            logs = toSend
        };

        string json = JsonUtility.ToJson(payload);

        using (UnityWebRequest uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
            uwr.uploadHandler = new UploadHandlerRaw(body);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("RemoteLogger: uploading " + toSend.Count + " entries to " + url);
            yield return uwr.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
#else
            if (uwr.isNetworkError || uwr.isHttpError)
#endif
            {
                Debug.LogWarning("RemoteLogger: upload failed: " + uwr.error + " responseCode=" + uwr.responseCode);
            }
            else
            {
                Debug.Log("RemoteLogger: upload succeeded. Clearing sent logs.");
                // Remove sent entries from buffer
                for (int i = 0; i < toSend.Count && buffer.Count > 0; i++)
                    buffer.Dequeue();
            }
        }
    }
}
