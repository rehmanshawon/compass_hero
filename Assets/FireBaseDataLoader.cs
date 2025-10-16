using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System.IO;
using System;

public class FireBaseDataLoader : MonoBehaviour
{
    public bool IfInEditor;
    [Space] public List<PlayerDataInfo> playersRoomData = new List<PlayerDataInfo>();
    public List<PlayerDataInfo> AllAiPlayers = new List<PlayerDataInfo>();
    public List<PlayerDataInfo> AllMatchPlayers = new List<PlayerDataInfo>();
    public List<PlayerDataInfo> Round0List = new List<PlayerDataInfo>();
    public List<PlayerDataInfo> Round1List = new List<PlayerDataInfo>();
    public List<PlayerDataInfo> Round2List = new List<PlayerDataInfo>();

    public List<PlayerMatchData> localMatchData;

    string JsonData;

    [ContextMenu("Load Data")]
    public void LoadDataFromFirebase()
    {
        if (IfInEditor)
        {
            string fileName = "UnifiedMatchData.json";
            string path = Path.Combine(Application.dataPath, fileName);

            if (File.Exists(path))
            {
                string jsonFromFile = File.ReadAllText(path);
                Debug.Log("📥 JSON file loaded from:\n" + path);

                try
                {
                    FirebaseDataWrapper wrapper = JsonConvert.DeserializeObject<FirebaseDataWrapper>(jsonFromFile);

                    if (wrapper != null)
                    {
                        playersRoomData = wrapper.playersRoomData ?? new List<PlayerDataInfo>();
                        AllAiPlayers = wrapper.allaiPlayers ?? new List<PlayerDataInfo>();
                        AllMatchPlayers = wrapper.allMatchPlayers ?? new List<PlayerDataInfo>();
                        Round0List = wrapper.round0List ?? new List<PlayerDataInfo>();
                        Round1List = wrapper.round1List ?? new List<PlayerDataInfo>();
                        Round2List = wrapper.round2List ?? new List<PlayerDataInfo>();
                        localMatchData = wrapper.match ?? new List<PlayerMatchData>();
                        Debug.Log("✅ Firebase data loaded and assigned successfully.");
                    }
                    else
                    {
                        Debug.LogError("❌ Failed to deserialize Firebase data. JSON might be malformed.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("❌ Exception during JSON deserialization: " + ex.Message);
                }
            }
            else
            {
                Debug.LogError("❌ File not found: " + path);
            }
        }
        else
        {
            if (FireBaseDataManager.Instance != null && FireBaseDataManager.Instance.WebGL != null)
            {
                FireBaseDataManager.Instance.WebGL.ReadData();
                Debug.Log("Firebase data loaded and assigned successfully." + JsonData);
                Debug.Log("List count " + (playersRoomData != null ? playersRoomData.Count : 0));
            }
            else
            {
                Debug.Log("❌ FireBaseDataManager or its WebGL component is null.");
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
}


[Serializable]
public class FirebaseDataWrapper
{
    public List<PlayerDataInfo> playersRoomData = new List<PlayerDataInfo>();
    public List<PlayerDataInfo> allaiPlayers = new List<PlayerDataInfo>();
    public List<PlayerDataInfo> allMatchPlayers = new List<PlayerDataInfo>();
    public List<PlayerDataInfo> round0List = new List<PlayerDataInfo>();
    public List<PlayerDataInfo> round1List = new List<PlayerDataInfo>();
    public List<PlayerDataInfo> round2List = new List<PlayerDataInfo>();
    public List<PlayerMatchData> match = new List<PlayerMatchData>();
    public string timestamp;
}

[Serializable]
public class PlayerDataInfo_L
{
    public string playerName;
    public int PlayerIndexInList;
}

[Serializable]
public class PlayerMatchDetails
{
    public string player1Name;
    public int player1ActorNumber;
    public string player2Name;
    public int player2ActorNumber;
    public string matchRoom;
    public bool isAgainstAI;
}

[Serializable]
public class PlayerMatchData_L
{
    public string playerId;
    public PlayerMatchDetails data;
}
