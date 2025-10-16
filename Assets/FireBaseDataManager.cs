using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System.IO;
using System;

public class FireBaseDataManager : MonoBehaviour
{
    public FirebaseExample WebGL;
    public FireBaseDataLoader DataLoader;
    public bool IfInEditor;

    public List<PlayerDataInfo> playersRoomData = new List<PlayerDataInfo>();
    public List<PlayerDataInfo> AllMatchPlayers = new List<PlayerDataInfo>();
    public List<PlayerDataInfo> AllAiPlayers = new List<PlayerDataInfo>();
    public List<PlayerDataInfo> Round0List = new List<PlayerDataInfo>();
    public List<PlayerDataInfo> Round1List = new List<PlayerDataInfo>();
    public List<PlayerDataInfo> Round2List = new List<PlayerDataInfo>();

    [Space(10)] public List<PlayerMatchData> localMatchData = new List<PlayerMatchData>();

    public static FireBaseDataManager Instance;

    private string JsonData;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [ContextMenu("Print All Data as Unified JSON")]
    public void SendUnifiedFirebaseData()
    {
        if (playersRoomData == null || AllMatchPlayers == null || Round0List == null || Round1List == null || Round2List == null || localMatchData == null)
        {
            Debug.LogError("❌ One or more data lists are null. Aborting SendUnifiedFirebaseData.");
            return;
        }

        FirebaseDataPackage dataPackage = new FirebaseDataPackage
        {
            playersRoomData = playersRoomData ?? new List<PlayerDataInfo>(),
            allMatchPlayers = AllMatchPlayers ?? new List<PlayerDataInfo>(),
            allaiPlayers = AllAiPlayers ?? new List<PlayerDataInfo>(),
            round0List = Round0List ?? new List<PlayerDataInfo>(),
            round1List = Round1List ?? new List<PlayerDataInfo>(),
            round2List = Round2List ?? new List<PlayerDataInfo>(),
            match = localMatchData ?? new List<PlayerMatchData>(),
            timestamp = DateTime.UtcNow.ToString("o")
        };

        try
        {
            JsonData = JsonConvert.SerializeObject(dataPackage);
            Debug.Log("📦 JSON Package:\n" + JsonData);
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Failed to serialize data package: " + e.Message);
            return;
        }

        if (IfInEditor)
        {
            try
            {
                string fileName = "UnifiedMatchData.json";
                string path = Path.Combine(Application.dataPath, fileName);
                File.WriteAllText(path, JsonData);
                Debug.Log("📁 JSON file saved in Assets:\n" + path);
            }
            catch (Exception e)
            {
                Debug.LogError("❌ Failed to save JSON file: " + e.Message);
            }
        }
        else
        {
            if (WebGL != null)
            {
                WebGL.WriteData(JsonData);
            }
            else
            {
                Debug.LogWarning("⚠️ WebGL reference is missing.");
            }
        }
    }

    public void ClearAllData()
    {
        playersRoomData.Clear();
        AllMatchPlayers.Clear();
        AllAiPlayers.Clear();
        Round0List.Clear();
        Round1List.Clear();
        Round2List.Clear();
    }

    public List<PlayerDataInfo> ClonePlayerDataList(List<PlayerDataInfo> sourceList)
    {
        List<PlayerDataInfo> newList = new List<PlayerDataInfo>();
        if (sourceList == null)
        {
            Debug.LogWarning("⚠️ Source list is null in ClonePlayerDataList.");
            return newList;
        }

        foreach (var player in sourceList)
        {
            if (player != null)
            {
                newList.Add(new PlayerDataInfo
                {
                    playerName = player.playerName,
                    PlayerIndexInList = player.PlayerIndexInList
                });
            }
            else
            {
                Debug.LogWarning("⚠️ Null player found in source list during cloning.");
            }
        }

        return newList;
    }
}


[Serializable]
public class FirebaseDataPackage
{
    public List<PlayerDataInfo> playersRoomData = new List<PlayerDataInfo>();
    public List<PlayerDataInfo> allMatchPlayers = new List<PlayerDataInfo>();
    public List<PlayerDataInfo> allaiPlayers = new List<PlayerDataInfo>();
    public List<PlayerDataInfo> round0List = new List<PlayerDataInfo>();
    public List<PlayerDataInfo> round1List = new List<PlayerDataInfo>();
    public List<PlayerDataInfo> round2List = new List<PlayerDataInfo>();
    public List<PlayerMatchData> match ;
    public string timestamp;
}

[Serializable]
public class PlayerMatchData
{
    public string playerId;
    public MatchData data;
}

[Serializable]
public class PlayerDataInfo
{
    public string playerName;
    public int PlayerIndexInList;

    public PlayerDataInfo()
    {
    }

    public PlayerDataInfo(string playerName, string playerType, int playerIndex)
    {
        this.playerName = playerName;
        this.PlayerIndexInList = playerIndex;
    }
}

[Serializable]
public class MatchData
{
    public string player1Name;
    public int player1ActorNumber;
    public string player2Name;
    public int player2ActorNumber;
    public string matchRoom;
    public bool isAgainstAI;

    public MatchData()
    {
    }

    public MatchData(string player1Name, string player2Name, string matchRoom, bool isAgainstAI)
    {
        this.player1Name = player1Name;
        this.player2Name = player2Name;
        this.matchRoom = matchRoom;
        this.isAgainstAI = isAgainstAI;
    }
}