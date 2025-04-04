using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class TournamentLobbyCreator : MonoBehaviourPunCallbacks
{
    public static bool wasInTournament;
    public static TournamentLobbyCreator Instance;
    public MainUI ui;
    [Header("UI Elements")] public GameObject tournamentPanel;
    public GameObject normalPanel;
    public TextMeshProUGUI playerListText;
    public Button startTournamentButton;
    public TimeCounterOfRoom counter;
    public const string TournamentRoomName = "Tournament_Lobby";
    public const int MaxPlayersInTournament = 8;
    private bool isTournamentOwner = false;
    public GameObject tournamentLoadingScreen;

    private List<string> tournamentPlayers = new List<string>();

    public TournamentsMatchSlots matchSlots;


    bool connectedtomasted = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
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

    public static int getroundCount()
    {
        return PlayerPrefs.GetInt(Prefs.TournamentRoundCount, 0);
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

    private void Start()
    {
        if (isInTournament())
        {
            int roundcount = PlayerPrefs.GetInt(Prefs.TournamentRoundCount, 0);
            roundcount++;
            PlayerPrefs.SetInt(Prefs.TournamentRoundCount, roundcount);
            PlayerPrefs.Save();


            Debug.Log("ROundCount : " + roundcount);

            if (roundcount >= 3) // REWARD PLAYER FOR WINNING TOURNAMENT
            {
                ui.WonTournament();
            }
        }
    }

    public void fillAiRooms()
    {
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();

        Debug.Log("room createdd");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        Debug.Log("room list updated tournament lobby" + roomList.Count);

        if (roomList.Find(x => x.Name.Equals(mymatchroom)) != null)
            if (!canmakeroom)
            {
                Debug.Log("room found and joining");
                ui.JoinBtnClick(mymatchroom);
            }
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

                    Debug.Log("room found : " + available + "         Lobby name : " + PhotonNetwork.CurrentLobby.Name);

                    if (!isAiPLayingTournament)
                    {
                        ui.JoinBtnClick(mymatchroom);
                    }
                    else
                    {
                    }
                }

                matchroomset = true;
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

    [PunRPC]
    public void JoinMatchNow()
    {
        StartCoroutine(DelayToStartGame());
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined tournament Room: {PhotonNetwork.CurrentRoom.Name}");
        if (ui.isTournament)
        {
            if (mymatchroom != "")
            {
                if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    PhotonNetwork.CurrentRoom.IsVisible = false;
                    //MainUI.playerNames[0] = PhotonNetwork.NickName;
                    // MainUI.playerNames[1] = newPlayer.NickName;
                    ui.isMatched = true;
                    Debug.Log("joining game");


                    photonView.RPC("JoinMatchNow", RpcTarget.All);
                }
                else
                {
                    Debug.Log("waiting for other player");
                }
            }
            else
            {
                if (PhotonNetwork.CurrentRoom.Name == TournamentRoomName)
                {
                    tournamentPanel.SetActive(true);
                    normalPanel.SetActive(false);
                    if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
                    {
                        isTournamentOwner = true;
                        // startTournamentButton.gameObject.SetActive(true);
                    }
                    else
                    {
                        startTournamentButton.gameObject.SetActive(false);
                    }

                    photonView.RPC("SyncTournamentsSkins", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber,
                        PlayerPrefs.GetInt("SKIN1"));
                    //Hashtable properties = new Hashtable
                    //{
                    //    { $"Skin{PhotonNetwork.LocalPlayer.ActorNumber}", PlayerPrefs.GetInt("SKIN1",0) } // Store the integer with a key
                    //};
                    //  PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
                    //  PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
                    UpdateTournamentPlayerList();
                }
                else
                {
                    tournamentPanel.SetActive(false);
                    normalPanel.SetActive(true);
                }
            }
        }
        else
        {
            if (isInTournament())
            {
                photonView.RPC("SyncTournamentsSkins", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber,
                    PlayerPrefs.GetInt("SKIN1"));
            }

            tournamentPanel.SetActive(false);
            normalPanel.SetActive(true);
        }
    }

    IEnumerator DelayToStartGame()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Multi");
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        //if (ui.isTournament)
        //{


        //    for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        //    {
        //        if (propertiesThatChanged.ContainsKey($"Skin{PhotonNetwork.PlayerList[i].ActorNumber}"))
        //        {
        //            int newval = (int)propertiesThatChanged[$"Skin{PhotonNetwork.PlayerList[i].ActorNumber}"];
        //            matchSlots.UpdateSlotSkin(ui.GetPLayerSkin(newval), i, PhotonNetwork.PlayerList[i].ActorNumber);


        //        }
        //    }
        //}
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //if (ui.isTournament)
        //{
        //    if (changedProps.ContainsKey("Skin"))
        //    {
        //        int newval = (int)changedProps["Skin"];

        //        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        //        {
        //            if (PhotonNetwork.PlayerList[i].ActorNumber == targetPlayer.ActorNumber)
        //            {
        //                matchSlots.UpdateSlotSkin(ui.GetPLayerSkin(newval), i, targetPlayer.ActorNumber);

        //            }

        //        }
        //    }
        //}
    }
    //[PunRPC]
    //public void ClearTournamentsSkin(int actornum)
    //{

    //    for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
    //    {
    //        if (PhotonNetwork.PlayerList[i].ActorNumber == actornum)
    //        {
    //            matchSlots.clearSkinSlot(  i, PhotonNetwork.PlayerList[i].ActorNumber);

    //        }

    //    }
    //}
    [PunRPC]
    public void SyncTournamentsSkins(int actornum, int skin)
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].ActorNumber == actornum)
            {
                matchSlots.UpdateSlotSkin(ui.GetPLayerSkin(skin), i, PhotonNetwork.PlayerList[i].ActorNumber);
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (ui.isTournament)
        {
            if (mymatchroom != "")
            {
                //if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
                //{
                //    PhotonNetwork.CurrentRoom.IsOpen = false;
                //    PhotonNetwork.CurrentRoom.IsVisible = false;
                //    //MainUI.playerNames[0] = PhotonNetwork.NickName;
                //    // MainUI.playerNames[1] = newPlayer.NickName;
                //    //  ui.isMatched = true;

                //    if (PhotonNetwork.IsMasterClient)
                //    {
                //        StartCoroutine(DelayToStartGame());
                //    }
                //}
            }
            else
            {
                Debug.Log($"New Player Joined: {newPlayer.NickName}");
                UpdateTournamentPlayerList();
                photonView.RPC("UpdateSkinValues", RpcTarget.All);
            }
        }
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
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
                Debug.Log($"Player Left: {otherPlayer.NickName}");
                UpdateTournamentPlayerList();

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

    private void UpdateTournamentPlayerList()
    {
        tournamentPlayers.Clear();
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            tournamentPlayers.Add(player.NickName);


            //    if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey($"Skin{player.ActorNumber}"))
            //    {
            //        Debug.Log("haveskin");
            //        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            //        {
            //            if (PhotonNetwork.PlayerList[i].ActorNumber == player.ActorNumber)
            //            {
            //                Debug.Log("haveskin 2");
            //                int skinNum = (int)PhotonNetwork.CurrentRoom.CustomProperties[$"Skin{player.ActorNumber}"];
            //                matchSlots.UpdateSlotSkin(ui.GetPLayerSkin(skinNum), i, PhotonNetwork.PlayerList[i].ActorNumber);

            //            }

            //        }
            //    }
        }

        photonView.RPC("SyncTournamentPlayerList", RpcTarget.All, string.Join("\n", tournamentPlayers));


        //    photonView.RPC("SyncTournamentPlayerList", RpcTarget.All, tournamentPlayers.ToArray());
    }

    [PunRPC]
    private void SyncTournamentPlayerList(string pl)
    {
//        playerListText.text = "Players in Tournament:\n" + pl;


        //    List<string> playerList = new List<string>(pl);

        Debug.Log("Received Player List:");

        matchSlots.clearslots();
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            matchSlots.PopulateSlot(PhotonNetwork.PlayerList[i].NickName, i, PhotonNetwork.PlayerList[i].ActorNumber);
        }
    }


    [PunRPC]
    public void AddAiplayer(int pos, string ainame)
    {
        matchSlots.PopulateSlot(ainame, pos, -1);
    }

    public void StartTournament()
    {
        if (isTournamentOwner && PhotonNetwork.CurrentRoom.PlayerCount >= 1)
        {
            int roundcount = PlayerPrefs.GetInt(Prefs.TournamentRoundCount, 0);

            if (roundcount <= 0)
            {
                fillRemainingPlayers();
            }

            Invoke("AssignPlayersToMatches", 3);
            //  photonView.RPC("StartTournamentRPC", RpcTarget.All);
        }
    }
    
    bool hasRoomFilled = false;

    [PunRPC]
    private void StartTournamentRPC()
    {
        Debug.Log("Tournament started! Assigning players to matches.");
    }

    List<string> namelist = new List<string>();

    public void fillRemainingPlayers()
    {
        int remcount = 8 - PhotonNetwork.PlayerList.ToList().Count;

        namelist.Clear();
        int pos = PhotonNetwork.PlayerList.ToList().Count;
        if (remcount > 0)
        {
            for (int i = 0; i < 8; i++)
            {
                namelist.Add(GetRandomName());
                if (i >= pos)
                {
                    photonView.RPC("AddAiplayer", RpcTarget.All, i, namelist[i]);
                }
            }
        }

        photonView.RPC("FillRPCNamesList", RpcTarget.Others, namelist[0], namelist[1], namelist[2], namelist[3],
            namelist[4], namelist[5],
            namelist[6], namelist[7]);
    }

    [PunRPC]
    public void FillRPCNamesList(string n1, string n2, string n3, string n4, string n5, string n6, string n7, string n8)
    {
        namelist.Clear();
        namelist.Add(n1);
        namelist.Add(n2);
        namelist.Add(n3);
        namelist.Add(n4);
        namelist.Add(n5);
        namelist.Add(n6);
        namelist.Add(n7);
        namelist.Add(n8);

        int remcount = 8 - PhotonNetwork.PlayerList.ToList().Count;


        int pos = PhotonNetwork.PlayerList.ToList().Count;
        if (remcount > 0)
        {
            for (int i = 0; i < 8; i++)
            {
                if (i >= pos)
                {
                    AddAiplayer(i, namelist[i]);
                }
            }
        }
    }

    private void AssignPlayersToMatches()
    {
        List<Player> players = new List<Player>(PhotonNetwork.PlayerList);
        int matchNumber = 1;

        int totalplayers = players.Count;
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
                    player1.NickName, player2.NickName, false);
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
            photonView.RPC("MovePlayersToMatch", RpcTarget.All, player1.ActorNumber, -1, matchRoom, player1.NickName,
                namelist[totalplayers], true);
        }
    }

    bool canmakeroom = false;
    public static bool isAiPLayingTournament;
    public List<string> randomFNames;
    public List<string> randomLNames;

    public string GetRandomName()
    {
        string name = randomFNames[Random.Range(0, randomFNames.Count)];
        // name += " " + randomLNames[Random.Range(0, randomLNames.Count)];
        return name;
    }

    [PunRPC]
    private void MovePlayersToMatch(int player1ID, int player2ID, string matchRoom, string plName1, string plName2,
        bool isai)
    {
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
        else
        {
        }
    }

    public string mymatchroom = "";

    //public override void OnJoinedRoom()
    //{
    //    Debug.Log($"Joined Room: {PhotonNetwork.CurrentRoom.Name}");

    //    if (PhotonNetwork.CurrentRoom.Name.StartsWith("Match_"))
    //    {
    //        Debug.Log("Match started. Play against opponent!");
    //    }
    //}

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
}