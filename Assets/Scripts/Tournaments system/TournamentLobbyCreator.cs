using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

public class TournamentLobbyCreator : MonoBehaviourPunCallbacks
{
    public static bool wasInTournament;
    public static TournamentLobbyCreator Instance;
    public MainUI ui;
    public bool IsPaidUser;
    public bool checkPSTTime = false;
    public bool checkLocalTime = false;
    public bool IfCheckForTime;
    public List<string> Round2List;
    public List<string> Round3List;
    public GameObject startButton;

    public bool CheckForData;
    public float timer;
    public ApiDataManagement api;

    [Header("UI Elements")] public GameObject tournamentPanel;
    public GameObject normalPanel;
    public TextMeshProUGUI playerListText;
    public Button startTournamentButton;
    public TimeCounterOfRoom counter;
    public const string TournamentRoomName = "Tournament_Lobby";
    public const int MaxPlayersInTournament = 20;
    public bool isTournamentOwner = false;
    public GameObject tournamentLoadingScreen;

    public int RoundCount;

    public bool checkForPlayersConditionCount;

    public bool WasRecentlyInTournament;

    public int playersToGO;

    public List<string> tournamentPlayers = new List<string>();
    public List<string> TempList = new List<string>();

    public TournamentsMatchSlots matchSlots;

    // Initialize without touching Photon in field initializers to avoid Resources.Load during construction
    public List<Player> cachedPlayers = new List<Player>();

    bool connectedtomasted = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        else
            Destroy(gameObject);
    }

    public static string GetPlayerName()
    {
        return PlayerPrefs.GetString(Prefs.PlayerTournamentName, "");
    }

    public static void setroundCount(int val)
    {
        PlayerPrefs.SetInt(Prefs.TournamentRoundCount, val);
        PlayerPrefs.Save();
    }

    public static void SavePlayerName(string str)
    {
        PlayerPrefs.SetString(Prefs.PlayerTournamentName, str);
        PlayerPrefs.Save();
    }

    public static void SetIsinTournament(bool val)
    {
        // Debug.LogError("setting intournament : " + val);
        PlayerPrefs.SetInt(Prefs.IsTournamanet, val ? 1 : 0);

        if (val == false)
        {
            PlayerPrefs.SetInt(Prefs.TournamentRoundCount, 0);
        }

        PlayerPrefs.Save();
    }

    public static bool isInTournament()
    {
        wasInTournament = PlayerPrefs.GetInt(Prefs.IsTournamanet, 0) == 0 ? false : true;
        return wasInTournament;
    }

    public bool WasInTournament()
    {
        WasRecentlyInTournament = PlayerPrefs.GetInt(Prefs.WasrecentlyInTournamanet, 0) != 0;

        return WasRecentlyInTournament;
    }

    public List<string> loadedNames;

    private void Start()
    {
        // Populate cachedPlayers safely after MonoBehaviour has started
        try
        {
            var list = PhotonNetwork.PlayerList;
            cachedPlayers = list != null ? list.ToList() : new List<Player>();
        }
        catch
        {
            cachedPlayers = new List<Player>();
        }

        if (isInTournament())
        {
            RoundCount = PlayerPrefs.GetInt(Prefs.TournamentRoundCount, 0);

            loadedNames = LoadList("R1"); // Make sure the key matches what was saved
            TempList.AddRange(loadedNames);

            if (RoundCount >= 3) // REWARD PLAYER FOR WINNING TOURNAMENT
            {
                ui.WonTournament();
                FireBaseDataManager.Instance.GetComponent<ApiDataManagement>().SendApiData();
                RoundCount = 0;
                PlayerPrefs.SetInt(Prefs.TournamentRoundCount, RoundCount);
                PlayerPrefs.Save();


                Debug.Log("Round Count on win: " + RoundCount);
                Debug.Log("Api Data Sent");
            }
        }
    }
    // private void OnToggleValueChangedToggle(bool isOn)
    // {
    //     checkPSTTime = isOn;
    //     IfCheckForTime = isOn;
    //     checkLocalTime = !isOn;
    // }

    void PopulateManualMatchSlots()
    {
        if (RoundCount == 1)
        {
            for (int i = 0; i < FireBaseDataManager.Instance.DataLoader.Round0List.Count; i++)
            {
                matchSlots.PopulateSlotManual(FireBaseDataManager.Instance.DataLoader.Round0List[i].playerName, i, 0,
                    0);
            }

            Debug.Log("Slots are assigned: " + RoundCount);
        }
        else if (RoundCount == 2)
        {
            for (int i = 0; i < FireBaseDataManager.Instance.DataLoader.Round1List.Count; i++)
            {
                matchSlots.PopulateSlotManual(FireBaseDataManager.Instance.DataLoader.Round0List[i].playerName, i, 0,
                    1);
            }

            for (int i = 0; i < FireBaseDataManager.Instance.DataLoader.Round0List.Count; i++)
            {
                matchSlots.PopulateSlotManual(FireBaseDataManager.Instance.DataLoader.Round0List[i].playerName, i, 0,
                    0);
            }

            Debug.Log("Slots are assigned: " + RoundCount);
        }
        else if (RoundCount == 2)
        {
            for (int i = 0; i < FireBaseDataManager.Instance.DataLoader.Round2List.Count; i++)
            {
                matchSlots.PopulateSlotManual(FireBaseDataManager.Instance.DataLoader.Round2List[i].playerName, i, 0,
                    2);
            }

            for (int i = 0; i < FireBaseDataManager.Instance.DataLoader.Round1List.Count; i++)
            {
                matchSlots.PopulateSlotManual(FireBaseDataManager.Instance.DataLoader.Round0List[i].playerName, i, 0,
                    1);
            }

            for (int i = 0; i < FireBaseDataManager.Instance.DataLoader.Round0List.Count; i++)
            {
                matchSlots.PopulateSlotManual(FireBaseDataManager.Instance.DataLoader.Round0List[i].playerName, i, 0,
                    0);
            }

            Debug.Log("Slots are assigned: " + RoundCount);
        }
    }

    private void OnToggleValueChanged(bool value)
    {
        IsPaidUser = value;
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Debug.Log("You are the first player in the tournament room");

            isTournamentOwner = true;
            RoundCount = PlayerPrefs.GetInt(Prefs.TournamentRoundCount, 0);

            if (IsPaidUser && isTournamentOwner && RoundCount == 0)
            {
                Debug.Log("You are a paid user and can start the tournament");
                startButton.SetActive(true);
            }
        }
        else
        {
            Debug.Log("Another player is already in the room. Disabling start button");
            startButton.SetActive(false);
        }

        Debug.Log("room createdd");

        Debug.Log("Owner: " + isTournamentOwner);
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Room is Left");
        int totalRooms = PhotonNetwork.CountOfRooms;
        Debug.Log("TotalRooms: " + totalRooms);

        for (int i = 0; i < tournamentPlayers.Count; i++)
        {
            tournamentPlayers.Remove(i.ToString());
        }

        CheckForData = false;
        timer = 0f;
        RemovePlayerFromFirebaseList();
    }

    [PunRPC]
    public void MakeOtherPlayerJoin()
    {
        ui.JoinBtnClick();
    }

    bool matchroomset = false;
    float waittime = 6;
    float maxtries = 3;

    private void Update()
    {
        if (mymatchroom != "" && matchroomset == false)
        {
            if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.CurrentLobby != null)
            {
                ui.selectedRoom = mymatchroom;
                if (canmakeroom)
                    ui.TournamentCreateBtnClick(mymatchroom);
                else
                {
                    if (waittime > 0)
                    {
                        waittime -= Time.deltaTime;
                        return;
                    }

                    bool available = ui.rooms.Any(x => x.Name.Equals(mymatchroom));

                    Debug.Log("room found : " + available + "Lobby name : " + PhotonNetwork.CurrentLobby.Name);

                    if (!isAiPLayingTournament)
                    {
                        ui.JoinBtnClick(mymatchroom);
                    }
                }

                matchroomset = true;
            }
        }

        if (CheckForData)
        {
            timer += Time.deltaTime;
            if (timer >= 2.5f || Input.GetKeyDown(KeyCode.Alpha2))
            {
                timer = 0f;
                AddNewPlayerAndUpdateData();
                // PopulateManualMatchSlots();
            }
        }
    }

    public void OnTournamentButtonClick()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (!PhotonNetwork.InRoom)
            {
                ui.JoinBtnClick(true, TournamentRoomName);
            }
        }
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("Joined tournament Lobby name : " + PhotonNetwork.CurrentLobby.Name);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        Debug.Log("room list updated tournament lobby" + roomList.Count);

        int roomCount = Mathf.Min(roomList.Count, ui.roomList.Length);
        for (int i = 0; i < roomCount; i++)
        {
            ui.roomList[i].SetActive(true);

            string roomName = roomList[i].Name;
            int index = i;
        }


        if (roomList.Find(x => x.Name.Equals(mymatchroom)) != null)
            if (!canmakeroom)
            {
                Debug.Log("room found and joining");
                ui.JoinBtnClick(mymatchroom);
            }
    }

    [PunRPC]
    public void JoinMatchNow()
    {
        StartCoroutine(DelayToStartGame());
    }

    public override void OnJoinedRoom()
    {
        if (!WasInTournament() && RoundCount == 0 && PhotonNetwork.CurrentRoom.PlayerCount <= playersToGO)
        {
            Debug.Log("Wasn't in tournament: " + WasInTournament());

            AddAllPlayersToFirebaseList();

            Debug.Log("Is in the tournament now?? : " + WasInTournament());
        }
        else if (WasInTournament() && PhotonNetwork.CurrentRoom.PlayerCount <= playersToGO &&
                 (RoundCount == 1 || RoundCount == 2))
        {
            AddAllPlayersToFirebaseList();
        }
        else
        {
            AddAllPlayersToFirebaseList();
        }

        if (ui.isTournament)
        {
            Debug.Log("You are in a tournament mode");

            if (mymatchroom != "")
            {
                Debug.Log($"Match room name found: {mymatchroom}");

                if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
                {
                    Debug.Log("Room is full. Locking and starting match...");

                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    PhotonNetwork.CurrentRoom.IsVisible = false;

                    ui.isMatched = true;
                    Debug.Log("Sending JoinMatchNow RPC to all clients");

                    photonView.RPC("JoinMatchNow", RpcTarget.All);
                }
                else
                {
                    Debug.Log("Waiting for another player to join the match room...");
                }
            }
            else
            {
                Debug.Log("This is the main tournament room");

                if (PhotonNetwork.CurrentRoom.Name == TournamentRoomName)
                {
                    tournamentPanel.SetActive(true);
                    normalPanel.SetActive(false);
                    Debug.Log("Syncing tournament skins...");

                    photonView.RPC("SyncTournamentsSkins", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber,
                        PlayerPrefs.GetInt("SKIN1"));

                    Debug.Log("Updating tournament player list");

                    UpdateTournamentPlayerList();
                }
                else
                {
                    Debug.Log("Not the main tournament room — showing normal panel");
                    tournamentPanel.SetActive(false);
                    normalPanel.SetActive(true);
                }
            }
        }
        else
        {
            Debug.Log("You are not in a tournament");

            if (isInTournament())
            {
                Debug.Log("You were in a tournament before — syncing skins");
                photonView.RPC("SyncTournamentsSkins", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber,
                    PlayerPrefs.GetInt("SKIN1"));
            }

            tournamentPanel.SetActive(false);
            normalPanel.SetActive(true);
        }

        Debug.Log("Is in the tournament: " + isInTournament());
    }

    [ContextMenu("Add the player to list")]
    public void AddAllPlayersToFirebaseList()
    {
        var allPlayers = PhotonNetwork.PlayerList;

        FireBaseDataManager.Instance.playersRoomData.Clear();

        for (int i = 0; i < allPlayers.Length; i++)
        {
            PlayerDataInfo playerDataInfo = new PlayerDataInfo()
            {
                playerName = allPlayers[i].NickName,
                PlayerIndexInList = i,
            };

            FireBaseDataManager.Instance.playersRoomData.Add(playerDataInfo);
        }

        CheckForData = true;

        if (RoundCount == 0)
        {
            if (api.WorkingIDs != null && api.WorkingIDs.Count > 0)
            {
                api.ID = api.WorkingIDs[Random.Range(0, api.WorkingIDs.Count)];
            }
            else
            {
                Debug.LogWarning("WorkingIDs list is empty!");
            }

            PlayerPrefs.SetString(Prefs.PlayerID, api.ID);
            PlayerPrefs.Save();
        }

        FireBaseDataManager.Instance.SendUnifiedFirebaseData();
    }

    [PunRPC]
    public void RemovePlayerFromFirebaseList()
    {
        FireBaseDataManager.Instance.DataLoader.LoadDataFromFirebase();

        Debug.Log("Data is loaded from firebase in removal");

        var playerToRemove = FireBaseDataManager.Instance.DataLoader.playersRoomData.Find(player =>
            player.playerName == PhotonNetwork.LocalPlayer.NickName);

        Debug.Log("Player to remove is set");

        FireBaseDataManager.Instance.playersRoomData = FireBaseDataManager.Instance.DataLoader.playersRoomData;
        Debug.Log("New list is assigned");

        if (playerToRemove != null)
        {
            FireBaseDataManager.Instance.playersRoomData.Remove(playerToRemove);
            Debug.Log("Player is removed");
            matchSlots.clearslots();
            FireBaseDataManager.Instance.SendUnifiedFirebaseData();

            FireBaseDataManager.Instance.playersRoomData.Clear();
            FireBaseDataManager.Instance.DataLoader.playersRoomData.Clear();

            CheckForData = false;

            Debug.Log("New data is send on removal and Coroutine stopped");
        }
        else
        {
            Debug.LogWarning("No matching player found to remove.");
        }
    }

    // [PunRPC]
    // public void AddAiplayer(int pos, string ainame)
    // {
    //     matchSlots.PopulateSlotManual(ainame, pos, -1, RoundCount);
    // }

    [PunRPC]
    private void StartTournamentRPC()
    {
        Debug.Log("Tournament started! Assigning players to matches.");

        try
        {
            AssignPlayersToMatches(); // This should run on ALL clients
            Debug.Log("AssignPlayersToMatches() completed successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in AssignPlayersToMatches(): {ex.Message}\n{ex.StackTrace}");
        }
    }

    [ContextMenu("StartTournamentNow")]
    public void StartTournamentNow()
    {
        Debug.Log("StartTournamentNow called.");

        try
        {
            fillRemainingPlayers();
            Debug.Log("Filled remaining players. Waiting before starting tournament...");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in fillRemainingPlayers(): {ex.Message}\n{ex.StackTrace}");
        }

        Invoke(nameof(DelayedStartTournament), 7f);
    }

    private void DelayedStartTournament()
    {
        Debug.Log("DelayedStartTournament invoked.");

        CheckForData = false;
        timer = 0f;

        try
        {
            StartTournament(); // Owner-only logic
            Debug.Log("StartTournament called successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in StartTournament(): {ex.Message}\n{ex.StackTrace}");
        }

        try
        {
            photonView.RPC(nameof(StartTournamentRPC), RpcTarget.All);
            Debug.Log("StartTournamentRPC sent to all clients.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"RPC call failed: {ex.Message}\n{ex.StackTrace}");
        }
    }

    [ContextMenu("StartTournament")]
    public void StartTournament()
    {
        Debug.Log("StartTournament called");

        if (!isTournamentOwner)
        {
            Debug.LogWarning("Not the tournament owner.");
            return;
        }

        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        Debug.Log($"Current player count: {playerCount}");

        if (playerCount >= 1)
        {
            Debug.Log("Starting tournament with " + playerCount + " players");

            // Already filled
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
        else
        {
            Debug.LogWarning("Not enough players to start tournament.");
        }
    }


    public void SetRound()
    {
        RoundCount++;
        PlayerPrefs.SetInt(Prefs.TournamentRoundCount, RoundCount);
        PlayerPrefs.Save();

        WasRecentlyInTournament = true;
        PlayerPrefs.SetInt(Prefs.WasrecentlyInTournamanet, 1);
        PlayerPrefs.Save();
    }

    void SetRoundsData()
    {
        List<PlayerDataInfo> clonedList =
            FireBaseDataManager.Instance.ClonePlayerDataList(FireBaseDataManager.Instance.playersRoomData);

        switch (RoundCount)
        {
            case 0:
                FireBaseDataManager.Instance.Round0List = clonedList;
                FireBaseDataManager.Instance.SendUnifiedFirebaseData();
                Debug.Log("Round 0 was created.");
                break;
            case 1:
                FireBaseDataManager.Instance.Round1List = clonedList;
                FireBaseDataManager.Instance.SendUnifiedFirebaseData();
                Debug.Log("Round 1 was created.");
                break;
            case 2:
                FireBaseDataManager.Instance.Round2List = clonedList;
                FireBaseDataManager.Instance.SendUnifiedFirebaseData();
                Debug.Log("Round 2 was created.");
                break;
            default:
                Debug.Log("Not enough players to start tournament.");
                break;
        }
    }

    bool hasRoomFilled = false;

    public List<string> namelist = new List<string>();

    [ContextMenu("FillPlayers")]
    public void fillRemainingPlayers()
    {
        RoundCount = PlayerPrefs.GetInt(Prefs.TournamentRoundCount, 0);
        namelist.Clear();
        Round2List = new List<string>();
        Round3List = new List<string>();

        int roundVal = 8;

        switch (RoundCount)
        {
            case 0:
                roundVal = 8;
                break;

            case 1:
                roundVal = 4;
                break;

            case 2:
                roundVal = 2;
                break;
            default:
                roundVal = 1;
                break;
        }

        Debug.Log("Round Count value in ai setup: " + RoundCount + " / Round Val " + roundVal);

        if (isTournamentOwner)
        {
            switch (RoundCount)
            {
                case 0:
                    FillRound0(roundVal);
                    break;
                case 1:
                    FillRound1(roundVal);
                    break;
                case 2:
                    FillRound2(roundVal);
                    break;
            }
        }
    }

// ---------------------- ROUND METHODS ----------------------

    void FillRound0(int roundVal)
    {
        Debug.Log("Before FillRound0 - player count: " + FireBaseDataManager.Instance.playersRoomData.Count);
        Debug.Log("After FillRound0 - player count: " + FireBaseDataManager.Instance.playersRoomData.Count);

        List<PlayerDataInfo> tempList = new List<PlayerDataInfo>();

        foreach (PlayerDataInfo player in FireBaseDataManager.Instance.DataLoader.playersRoomData)
        {
            tempList.Add(player);
        }

        FireBaseDataManager.Instance.playersRoomData = tempList;
        FireBaseDataManager.Instance.DataLoader.playersRoomData.Clear();

        int existingCount = FireBaseDataManager.Instance.playersRoomData.Count;
        Debug.Log("Player list count: " + FireBaseDataManager.Instance.playersRoomData.Count);
        int aiCountNeeded = roundVal - existingCount;

        for (int i = 0; i < aiCountNeeded; i++)
        {
            string aiName = GetRandomName();
            FireBaseDataManager.Instance.AllAiPlayers.Add(new PlayerDataInfo { playerName = aiName });
            FireBaseDataManager.Instance.playersRoomData.Add(new PlayerDataInfo { playerName = aiName });
        }

        PlayerPrefs.SetString(Prefs.AiPref, FireBaseDataManager.Instance.AllAiPlayers[0].playerName);
        PlayerPrefs.Save();
        FireBaseDataManager.Instance.AllAiPlayers.RemoveAt(0);


        int halfCount = FireBaseDataManager.Instance.AllAiPlayers.Count / 2;

        for (int i = 0; i < halfCount; i++)
        {
            int removeIndex = Random.Range(0, FireBaseDataManager.Instance.AllAiPlayers.Count);
            FireBaseDataManager.Instance.AllAiPlayers.RemoveAt(removeIndex);
        }

        Debug.Log(
            $"[FillRound0] Players before: {existingCount}, AI added: {aiCountNeeded}, Total: {FireBaseDataManager.Instance.playersRoomData.Count}");

        Debug.Log("Setted Round count: " + RoundCount);

        FireBaseDataManager.Instance.SendUnifiedFirebaseData();
    }

    void FillRound1(int roundVal)
    {
        Debug.Log("This is Round1 Method");

        foreach (PlayerDataInfo player in FireBaseDataManager.Instance.DataLoader.AllAiPlayers)
        {
            FireBaseDataManager.Instance.playersRoomData.Add(player);
        }

        int existingCount = FireBaseDataManager.Instance.playersRoomData.Count;
        int aiCountNeeded = roundVal - existingCount;

        for (int i = 0; i < aiCountNeeded; i++)
        {
            string aiName = FireBaseDataManager.Instance.DataLoader.AllAiPlayers[i].playerName;
            FireBaseDataManager.Instance.AllAiPlayers.Add(new PlayerDataInfo { playerName = aiName });
            FireBaseDataManager.Instance.playersRoomData.Add(new PlayerDataInfo { playerName = aiName });
        }


        PlayerPrefs.SetString(Prefs.AiPref, FireBaseDataManager.Instance.AllAiPlayers[0].playerName);
        PlayerPrefs.Save();
        FireBaseDataManager.Instance.AllAiPlayers.RemoveAt(0);

        int halfCount = FireBaseDataManager.Instance.AllAiPlayers.Count / 2;

        for (int i = 0; i < halfCount; i++)
        {
            int removeIndex = Random.Range(0, FireBaseDataManager.Instance.AllAiPlayers.Count);

            FireBaseDataManager.Instance.AllAiPlayers.RemoveAt(removeIndex);
        }

        Debug.Log(
            $"[FillRound0] Players before: {existingCount}, AI added: {aiCountNeeded}, Total: {FireBaseDataManager.Instance.playersRoomData.Count}");


        FireBaseDataManager.Instance.SendUnifiedFirebaseData();
    }


    void FillRound2(int roundVal)
    {
        Debug.Log("This is Round1 Method");

        foreach (PlayerDataInfo player in FireBaseDataManager.Instance.DataLoader.AllAiPlayers)
        {
            FireBaseDataManager.Instance.playersRoomData.Add(player);
        }

        PlayerPrefs.SetString(Prefs.AiPref, FireBaseDataManager.Instance.AllAiPlayers[0].playerName);
        PlayerPrefs.Save();

        int existingCount = FireBaseDataManager.Instance.playersRoomData.Count;
        int aiCountNeeded = roundVal - existingCount;

        for (int i = 0; i < aiCountNeeded; i++)
        {
            string aiName = FireBaseDataManager.Instance.DataLoader.AllAiPlayers[i].playerName;
            FireBaseDataManager.Instance.AllAiPlayers.Add(new PlayerDataInfo { playerName = aiName });
            FireBaseDataManager.Instance.playersRoomData.Add(new PlayerDataInfo { playerName = aiName });
        }

        Debug.Log(
            $"[FillRound0] Players before: {existingCount}, AI added: {aiCountNeeded}, Total: {FireBaseDataManager.Instance.playersRoomData.Count}");

        FireBaseDataManager.Instance.SendUnifiedFirebaseData();
    }


// ---------------------- RPC SYNC ----------------------

    void SendNameListRPC(string methodName, List<string> list, int count)
    {
        var args = list.Take(count).Cast<object>().ToArray();
        photonView.RPC(methodName, RpcTarget.Others, args);
    }

    public void SaveList(string key, List<string> Nameslist)
    {
        string listString = string.Join(",", Nameslist); // Convert list to a comma-separated string
        PlayerPrefs.SetString(key, listString); // Save the string in PlayerPrefs
        PlayerPrefs.Save();
    }

// Load List<string> from a comma-separated string
    public List<string> LoadList(string key)
    {
        string listString = PlayerPrefs.GetString(key, "");
        List<string> list = new List<string>();

        if (!string.IsNullOrEmpty(listString))
        {
            string[] items = listString.Split(','); // Split the string into individual items
            foreach (string item in items)
            {
                list.Add(item); // Add each item to the list
            }
        }

        return list;
    }

    IEnumerator DelayToStartGame()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Multi");
    }

    [PunRPC]
    public void SyncTournamentsSkins(int actornum, int skin)
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].ActorNumber == actornum &&
                PhotonNetwork.PlayerList[i].CustomProperties.ContainsKey("PlayerType") &&
                (string)PhotonNetwork.PlayerList[i].CustomProperties["PlayerType"] == "ActualPlayer")
            {
                matchSlots.UpdateSlotSkin(ui.GetPLayerSkin(skin), i, PhotonNetwork.PlayerList[i].ActorNumber);
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!ui.isTournament) return;

        if (!string.IsNullOrEmpty(mymatchroom))
        {
            // Presumably some logic will be added here
        }

        Debug.Log($"A new player {newPlayer} has joined the match room.");

        Debug.Log(" New Data has been updated ");
        photonView.RPC("UpdateSkinValues", RpcTarget.All);
    }


    [ContextMenu("Populate")]
    public void AddNewPlayerAndUpdateData()
    {
        matchSlots.clearslots();
        FireBaseDataManager.Instance.DataLoader.LoadDataFromFirebase();
        for (int i = 0; i < FireBaseDataManager.Instance.DataLoader.playersRoomData.Count; i++)
        {
            matchSlots.PopulateSlotManual(FireBaseDataManager.Instance.DataLoader.playersRoomData[i].playerName, i, 0,
                RoundCount);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (ui.isTournament)
        {
            if (mymatchroom != "")
            {
                //  PhotonNetwork.JoinOrCreateRoom(mymatchroom, new RoomOptions { MaxPlayers = 2 }, TypedLobby.Default);
            }
            else
            {
                photonView.RPC("UpdateSkinValues", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    public void UpdateSkinValues()
    {
        photonView.RPC("SyncTournamentsSkins", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber,
            PlayerPrefs.GetInt("SKIN1"));
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        string fullLog = $"[{type}] {logString}\n{stackTrace} (Script lobby: {name})";
        Debug.Log(fullLog);
    }

    private void UpdateTournamentPlayerList()
    {
        tournamentPlayers.Clear();
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            tournamentPlayers.Add(player.NickName);
        }
    }

    private void AssignPlayersToMatches()
    {
        List<Player> players = PhotonNetwork.PlayerList.ToList();

        int matchNumber = 1;

        int totalplayers = 0;

        int count = players.Count;
        while (count > 1)
        {
            if (count >= 2)
            {
                totalplayers++;
                count -= 2;
            }
        }

        if (count == 1)
        {
            totalplayers++;
        }

        while (players.Count > 1)
        {
            if (players.Count >= 2)
            {
                Player player1 = players[0];
                Player player2 = players[1];
                players.RemoveAt(0);
                players.RemoveAt(0);

                string matchRoom = $"Match_{matchNumber}";
                photonView.RPC("MovePlayersToMatch", RpcTarget.All, player1.ActorNumber, player2.ActorNumber, matchRoom,
                    player1.NickName, player2.NickName, false, totalplayers);

                matchNumber++;
            }
        }

        if (totalplayers < 8) // update this to add event total players
        {
            // fill rest of players with ai
        }

        if (players.Count == 1)
        {
            string matchRoom = $"Match_{matchNumber}";
            Player player1 = players[0];
            players.RemoveAt(0);

            string aiName = "";

            // RoundCount = PlayerPrefs.GetInt(Prefs.TournamentRoundCount, 0);

            photonView.RPC("MovePlayersToMatch", RpcTarget.All, player1.ActorNumber, -1, matchRoom,
                player1.NickName, aiName, true, totalplayers);

            MatchData match = new MatchData(player1.NickName, null, matchRoom, false);

            PlayerMatchData playerMatchData = new PlayerMatchData
            {
                playerId = player1.UserId,
                data = match
            };

            FireBaseDataManager.Instance.localMatchData.Add(playerMatchData);
        }

        SetRoundsData();

        SetRound();
    }

    bool canmakeroom = false;
    public static bool isAiPLayingTournament;
    public List<string> randomFNames;
    public List<string> randomLNames;

    private List<string> usedNames = new List<string>();

    public string GetRandomName()
    {
        string name = randomFNames[Random.Range(0, randomFNames.Count)];

        while (usedNames.Contains(name))
        {
            name = randomFNames[Random.Range(0, randomFNames.Count)];
        }

        usedNames.Add(name);

        return name;
    }

    [PunRPC]
    private void MovePlayersToMatch(int player1ID, int player2ID, string matchRoom, string plName1, string plName2,
        bool isai, int totalplayers)
    {
        PlayerPrefs.SetInt(Prefs.playerCount, totalplayers);
        PlayerPrefs.Save();

        if (PhotonNetwork.LocalPlayer.ActorNumber == player1ID || PhotonNetwork.LocalPlayer.ActorNumber == player2ID)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == player1ID)
            {
                PlayerPrefs.SetString(Prefs.otherplayername, plName2);
                PlayerPrefs.Save();


                if (isai)
                {
                    isAiPLayingTournament = true;

                    ui.PlaySoloBtnClick();
                }
                else
                {
                    canmakeroom = true;
                    isAiPLayingTournament = false;
                }
            }
            else
            {
                PlayerPrefs.SetString(Prefs.otherplayername, plName1);
                PlayerPrefs.Save();
            }

            if (plName2 == "Ai")
            {
                isAiPLayingTournament = true;
            }
            else
            {
                isAiPLayingTournament = false;
            }

            mymatchroom = matchRoom;
            PhotonNetwork.LeaveRoom();
            waittime = 3;
            tournamentLoadingScreen.SetActive(false);
        }
    }

    public string mymatchroom = "";

    public void OnMatchEnd(bool won)
    {
        if (won)
        {
            Debug.Log("Match won! Returning to tournament.");
            photonView.RPC("ReportMatchResult", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, true);
        }
        else
        {
            Debug.Log("Match lost! Eliminated.");
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("MainMenu");
        }
    }

    [PunRPC]
    private void ReportMatchResult(int winnerID, bool won)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == winnerID)
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.JoinRoom(TournamentRoomName);
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Debug.Log("Tournament Winner: " + PhotonNetwork.PlayerList[0].NickName);
        }
    }

    [ContextMenu("LoadScene")]
    public void SceneReseter()
    {
        SceneManager.LoadScene("Main");
    }
}