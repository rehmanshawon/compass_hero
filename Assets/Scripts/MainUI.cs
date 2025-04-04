using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using Warzoom.MapShot;
using Newtonsoft.Json;
using SimpleJSON;

public class MainUI : MonoBehaviourPunCallbacks, IOnEventCallback
{
    [System.Serializable]
    public class UserInfo
    {
        public string username;
        public int skin1;
        public int skin2;
        public int wincount;
        public int losecount;
        public int level;
        public int playingCounter;
        public int round;
        public string options;
        public string time;
        public string day;
        public string skins;
        public string purchased_options;
    }

    struct EventParameters
    {
        public int hour;
        public int minute;
        public int second;
        public int round;
        public int eventRound;
        public string day;
    }

    [DllImport("__Internal")]
    private static extern string GetURLFromPage();

    public string ReadURL()
    {
        return GetURLFromPage();
    }

    public static MainUI Instance;
    public GameObject mainUI;
    public GameObject playUI;
    public GameObject loadingUI;
    //public GameObject skinUI;
    public GameObject onlineUI;
    public GameObject roomUI;
    public GameObject aiSelectionUI;
    public GameObject skinUI;
    public GameObject gameOptionsUI;
    public GameObject player1Avatar;
    public GameObject player2Avatar;
    public Sprite[] player1Skins;
    public Sprite[] player2Skins;
    public GameObject alert;
    public GameObject roomNameBack;
    public GameObject roomListBoard;
    public GameObject multiStartBtn;
    public GameObject logoInRoom;
    public GameObject prepareText;
    public GameObject warningText;
    public GameObject[] roomList;
    public Image[] roomBack;
    public GameObject[] skinList;
    public InputField playerNameCreate;
    public Text alertText;
    public Text[] roomName;
    public Text player1Name;
    public Text player2Name;
    public Text playingCount;
    [Header("--------------- AI ----------------")]
    public static int AIlevelIndex;
    public static string playerNameForAI;
    public static string nameAI;
    public static bool isPlayingNormal = false;

    [Header("--------------- Event ----------------")]
    public GameObject eventUI;
    public GameObject waitingUI;
    public GameObject[] playerNamesE;
    public GameObject backEventBtn;
    public GameObject trophy;
    public string[] playerNamesR;
    public float startingTime;
    int roundIndices;
    int indexPlus;
    int delayTimeRound;
    bool isAvailableEvent;
    bool isJoinedEvent;
    bool isAvailableRound;
    public static bool isListedRound;
    bool isAvailableRoom;
    [HideInInspector]public bool isMatched;
    bool isEventTime;
    public bool isEndEvent;
    public bool isTimeRefreshed;
    EventParameters eventParameter;

    [Header("--------- Skins for players ----------")]
    public Sprite[] skinSprites0;
    public Sprite[] skinSprites1;
    public Sprite[] skinSprites2;
    public Sprite[] skinSprites3;
    public Sprite[] skinSprites4;
    public Sprite[] skinSprites5;
    public Sprite[] skinSprites6;
    public Sprite[] skinSprites7;
    public Sprite[] skinSprites8;
    public Sprite[] skinSprites9;
    public Sprite[] skinSprites10;
    public Sprite[] skinSprites11;
    public Sprite[] skinSprites12;
    public Sprite[] skinSprites13;

    [Header("---------- Skins & Game options ----------")]
    public GameObject skinDialog;
    public GameObject skinDetailDialog;
    public GameObject skinsContent;
    public GameObject skinsContentOriginal;
    public GameObject skinPurchaseDescription;
    public GameObject optionsDetailDialog;
    public Image gameOptionsImage;
    public Image gameOptionsDetailImage;
    public GameObject optionsPurchaseDescription;
    public Image skinsImage1;
    public Image skinsImage2;
    public Image[] skinDetails;
    public Sprite[] optionsSprite;
    public Toggle[] skinsSelect;
    public Toggle[] optionsSelect;
    public Text skinDetailsTitle;
    //public Text onlineLevelText;
    public Text onlineOptoinsText;
    public Text optionDetailsTitle;
    public Text[] optionDetailsText;
    public Button[] skinPurchaseBtns;
    public Button[] optionPurchaseBtns;
    public string selectedOptions;
    private bool isSelectingSkin = false;

   public List<RoomInfo> rooms = new List<RoomInfo>();
    public static string[] playerNames = new string[2];
    public static string[] playerNamesTurn = new string[2];
    [HideInInspector]public string selectedRoom = "";
   public string gameVersion = "1";
    public static string gameOptions = "";
    bool isConnecting;
    bool isConnectedFirst;
    public static int skinIndex1;
    public static int skinIndex2;
    public static int skinIndex3;
    public static int skinIndex4;
    //public int selectedSkin;

    // Double click detect
    private float lastClickTime;
    private float doubleClickTimeThreshold = 0.3f;
    private bool isDoubleClick = false;

    // test
    public InputField testText;
    public Text testOutput;

    public static string frontURL = "";
    public bool isTournament;

    public GameObject tournamnetloading;
    public GameObject TournamentWinPanel;
    #region MonobehaviourCallbacks

    private void Awake()
    {
        Instance = this;
        PhotonNetwork.AutomaticallySyncScene = true;
        // Keep Unity WebGL app running in the background
        Application.runInBackground = true;
        //Application.runInBackground = true;
        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = 30;     
        TournamentWinPanel.SetActive(false);
    }

    void Start()
    {
        mainUI.transform.localScale = new Vector3(Screen.width / 1366f, Screen.height / 768f, 1f);
        Debug.Log("lobby available " +PhotonNetwork.InLobby);
        Debug.Log("was in tournament " + TournamentLobbyCreator.isInTournament());

        StartCoroutine(ClearCacheRoutine());
        //  if (!PhotonNetwork.InLobby)
        {
            //   TournamentLobbyCreator.SetIsinTournamanet(false);
        }
        if (!TournamentLobbyCreator.isInTournament())
        {
            PhotonNetwork.Disconnect();
            tournamnetloading.SetActive(false);
        }
        else
        {



            playerNameCreate.text = TournamentLobbyCreator.GetPlayerName();

            Invoke("Connect", 3);
            Connect();
            tournamnetloading.SetActive(true);

           // Invoke(nameof(ReconnetToTournament), 5);
        }
    
        selectButtonInit();
    }
    public void WonTournament()
    {
        TournamentWinPanel.SetActive(true);

        TournamentLobbyCreator.setroundCount(0);

    }
    public void ContinueFromTournament()
    {
        TournamentLobbyCreator.SetIsinTournament(false);
        SceneManager.LoadScene("Main");
        PhotonNetwork.Disconnect();
    }
    public void ReconnetToTournament()
    {
        JoinBtnClick(true, TournamentLobbyCreator.TournamentRoomName);
    }

    IEnumerator ClearCacheRoutine()
    {
        Debug.Log("Clear Cache Routine Called");
        // Clear the cache
        Caching.ClearCache();
        Debug.Log("Cache Cleared");
        yield return new WaitForSeconds(0f);

        playUI.SetActive(true);
        isConnecting = false;
        isConnectedFirst = true;
        isJoinedEvent = false;
        isListedRound = false;
        isAvailableRoom = false;
        isMatched = false;
        isEndEvent = false;
        isTimeRefreshed = false;
        isPlayingNormal = false;
        delayTimeRound = 0;
        indexPlus = 0;
        startingTime = 0;

        isAvailableEvent = false;
        isEventTime = false;
        roundIndices = 0;
        selectedOptions = "0000";

        Debug.Log("Getting URL");


        // Real parameters
        PlayerPrefs.SetInt("USER_ID", GetUserId(ReadURL()));
        Debug.Log($"User ID being set {GetUserId(ReadURL())}");
        frontURL = ReadURL().Split("/")[0] + "//" + ReadURL().Split("/")[2];

        // ------------------------ Test parameters -------------------------
        //gameOptions = "@^*";
        //PlayerPrefs.SetInt("USER_ID", 1);        
        //PlayerPrefs.SetInt("SKIN1", 13);
        //PlayerPrefs.SetInt("SKIN2", 10);
        //PlayerPrefs.SetInt("LEVEL", 1);

        //PlayerPrefs.SetInt("USER_ID", 4);
        //frontURL = "https://wargrids.com";

        StartCoroutine(GetUserInfos(frontURL + "/api/getUserData"));
    }    

    void Update()
    {
        mainUI.transform.localScale = new Vector3(Screen.width / 1366f, Screen.height / 768f, 1f);

        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - lastClickTime < doubleClickTimeThreshold)
            {
                isDoubleClick = true;
            }
            else
            {
                isDoubleClick = false;
            }

            lastClickTime = Time.time;
        }

        if (isEndEvent)
        {
            LeaveRound();
            isEndEvent = false;
            StartCoroutine(DelayToShowAlert("Sorry, not enough players, please join again next Friday and tell your friends!"));
        }
    }

    #region AI game mode
    public void PlaySoloBtnClick()
    {
        isTournament = false;
        if (playerNameCreate.text == "")
        {
            StartCoroutine(DelayToShowAlert("Please enter the name."));
        }
        else
        {
            //aiSelectionUI.SetActive(true);

       
                playerNameForAI = playerNameCreate.text;
                playerNames[0] = playerNameForAI;
         
            
            //   StartCoroutine(setCustomDataforAI());
            SelectAIPlayer(0);

        }
    }
    public string getAiName()
    {
        return TournamentLobbyCreator.isAiPLayingTournament ? PlayerPrefs.GetString(Prefs.otherplayername, "PEE WEE") : "PEE WEE";
    }
    public void SelectAIPlayer(int levelIndex)
    {
        AIlevelIndex = levelIndex;
        skinIndex1 = PlayerPrefs.GetInt("SKIN1");
        skinIndex2 = PlayerPrefs.GetInt("SKIN2");
        isPlayingNormal = true;

        switch (AIlevelIndex)
        {
            case 0:
                {
                    nameAI = "Pee Wee";                    
                    skinIndex3 = 6;
                    skinIndex4 = 6;
                }
                break;
            case 1:
                {
                    nameAI = "Iron Man";
                    skinIndex3 = 0;
                    skinIndex4 = 9;
                }
                break;
            case 2:
                {
                    nameAI = "Hercules";
                    skinIndex3 = 0;
                    skinIndex4 = 13;
                }
                break;
        }
        if (TournamentLobbyCreator.isAiPLayingTournament)
        {
            nameAI = getAiName();

        }
        
        playerNames[1] = nameAI;
        SceneManager.LoadScene("Single");
    }
    #endregion

    public void OpenMyCheese()
    {
        Application.OpenURL("https://subgrids.com/mycheese#");
    }
   void ConnectToRegion()
    {
        AppSettings regionSettings = new()
        {
            UseNameServer = true,
            //FixedRegion = "usw",
            FixedRegion = "ussc",
            AppIdRealtime = "9601d8d2-a991-441d-8735-713a47011d9e",
            AppVersion = gameVersion,
        };
        PhotonNetwork.ConnectUsingSettings(regionSettings);

        //PhotonNetwork.ConnectUsingSettings();
        //PhotonNetwork.GameVersion = gameVersion;
    }

    // Get user_id from gameURL.
    public int GetUserId(string gameUrl)
    {
        string temp = "";

        for (int i = 0; i < gameUrl.Length; i++)
        {
            temp += gameUrl[i];

            if (gameUrl[i] == '=')
            {
                temp = "";
            }
        }

        var id = MapShotAvailabilityChecker.GetUrlParameter("id");
        return int.Parse(id);
    }

    public void RefreshRoomList()
    {
        print("Refresh room");

        List<int> removedRooms = new List<int>();
        
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            for (int j = i + 1; j < rooms.Count; j++)
            {
                if (rooms[i].Name == rooms[j].Name)
                {
                    removedRooms.Add(j);
                }                
            }            
        }

        for(int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].MaxPlayers == roundIndices)
            {
                removedRooms.Add(i);
            }
        }

        for(int i = 0; i < removedRooms.Count; i++)
        {
            rooms.RemoveAt(removedRooms[i]);
        }

        for (int i = 0; i < roomList.Length; i++)
        {
            roomList[i].SetActive(false);
        }

       

        for (int i = 0; i < rooms.Count; i++)
        {
         //   if (!rooms[i].Name.Equals(TournamentLobbyCreator.TournamentRoomName))
            {
                roomList[i].SetActive(true);
                roomName[i].text = rooms[i].Name;

                if (string.Equals(selectedRoom, roomName[i].text))
                {
                    roomBack[i].color = Color.red;
                    roomName[i].color = Color.red;
                }
                else
                {
                    roomBack[i].color = Color.black;
                    roomName[i].color = Color.white;
                }
            }
        }
    }

    public void RefreshRound(int roundIndex)
    {
        for (int i = 0; i < roundIndex; i++)
        {
            playerNamesE[i].transform.GetChild(0).GetComponent<Text>().text = "Open";
        }

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            playerNamesE[i + indexPlus].transform.GetChild(0).GetComponent<Text>().text = PhotonNetwork.PlayerList[i].NickName;
        }
    }

    IEnumerator DelayToShowAlert(string str)
    {
        alertText.text = str;
        alert.SetActive(true);
        
        yield return new WaitForSeconds(2f);

        alert.SetActive(false);
    }

#endregion

    #region PhotonMonobehaviourCallbacks
    public void Connect()
    {
        // event logic implemented
        //if (!isEventTime)
        //{
        //    isConnectedFirst = true;

        //    if (PhotonNetwork.IsConnected)
        //    {
        //        playUI.SetActive(false);
        //        onlineUI.SetActive(true);

        //        isJoinedEvent = false;
        //        isConnecting = true;
        //        PhotonNetwork.JoinLobby(TypedLobby.Default);
        //    }
        //    else
        //    {
        //        isConnecting = true;
        //        isJoinedEvent = false;
        //        ConnectToRegion();

        //        playUI.SetActive(false);
        //        loadingUI.SetActive(true);
        //    }
        //}
        //else
        //{
        //    StartCoroutine(DelayToShowAlert("Event time (7:00 ~ 9:00 pm)"));
        //}                     

        isConnectedFirst = true;

        if (PhotonNetwork.IsConnected)
        {
            playUI.SetActive(false);
            onlineUI.SetActive(true);

            isJoinedEvent = false;
            isConnecting = true;

            if(!PhotonNetwork.InLobby)
                PhotonNetwork.JoinLobby(TypedLobby.Default);
            

           
        }
        else
        {
            isConnecting = true;
            isJoinedEvent = false;
            ConnectToRegion();

            playUI.SetActive(false);
            loadingUI.SetActive(true);
        }
        if (PhotonNetwork.InLobby)
        {
            if (TournamentLobbyCreator.isInTournament())
            {
                JoinBtnClick(true, TournamentLobbyCreator.TournamentRoomName);
            }
        }
    }

    public void JoinEvent()
    {
        isAvailableEvent = true;
        
        if (isAvailableEvent)
        {
            isConnectedFirst = true;
            playUI.SetActive(false);
            waitingUI.SetActive(true);
            isJoinedEvent = true;
            isConnecting = false;

            if (PhotonNetwork.IsConnected)
            {
                if (!PhotonNetwork.InLobby)
                    PhotonNetwork.JoinLobby(TypedLobby.Default);
            }
            else
            {
                ConnectToRegion();
            }                                                        
        }
        else
        {
            if((eventParameter.hour == 19 || eventParameter.hour == 20) && eventParameter.day == "Friday")
            {
                StartCoroutine(DelayToShowAlert("Sorry! Pee Wee Event is full."));
            }
            else
            {
                StartCoroutine(DelayToShowAlert("PEE WEE EVENT will start at Friday 7pm."));
            }                        
        }                
    }

    public void CreateRound()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = (byte)roundIndices;  

        PhotonNetwork.CreateRoom("Round1", roomOptions, TypedLobby.Default);
    }

    public void LeaveRound()
    {
        print("Leave round");
        StartCoroutine(GetUserInfos(frontURL + "/api/getUserData"));
        //StartCoroutine(GetUserInfos("http://192.168.106.133/api/getUserData"));

        PhotonNetwork.LeaveRoom();
        if (!PhotonNetwork.InLobby)
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        isJoinedEvent = false;
        isListedRound = false;
        playUI.SetActive(true);
        eventUI.SetActive(false);  
    }

    public void LeaveRoundForEvent()
    {
        print("Leave round for event");
        PhotonNetwork.LeaveRoom();
        if (!PhotonNetwork.InLobby)
            PhotonNetwork.JoinLobby(TypedLobby.Default);        
    }

    public void CreateGame()
    {        
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = (byte)2;
        gameOptions = "@^*";

        // disconnect issue
        roomOptions.CleanupCacheOnLeave = false;
        roomOptions.PlayerTtl = 500;
        roomOptions.EmptyRoomTtl = 600;
        PhotonNetwork.KeepAliveInBackground = 60000;

        PhotonNetwork.CreateRoom(PhotonNetwork.NickName, roomOptions, TypedLobby.Default);
    }

    IEnumerator DelayForJoinning()
    {
        yield return new WaitForSeconds(3f);

        if (isAvailableRound)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].MaxPlayers == roundIndices)
                {
                    if (rooms[i].IsOpen && rooms[i].IsVisible)
                    {
                        PhotonNetwork.JoinRoom("Round1");
                        print("Random joining...");
                        break;
                    }          
                }
            }

            if (PhotonNetwork.NetworkClientState != ClientState.Joining
                && PhotonNetwork.NetworkClientState != ClientState.Joined)
            {
                print("Random creating...");
                CreateRound();
            }
        }
        else
        {
            CreateRound();
        }
    }

    IEnumerator DelaytoMatch()
    {   
        yield return new WaitForSeconds(3f + delayTimeRound);        

        if (isAvailableRoom)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].IsOpen && rooms[i].IsVisible)
                {
                    print("Random joining...");
                    print(rooms[i].Name);          
                    PhotonNetwork.JoinRoom(rooms[i].Name);                    
                    break;
                } 
            }

            if (PhotonNetwork.NetworkClientState != ClientState.Joining
                && PhotonNetwork.NetworkClientState != ClientState.Joined)
            {
                print("Random creating...");
                CreateGame();
            }
        }
        else
        {
            print("Create random game");
            CreateGame();
        }
    }

    IEnumerator DelayToLeaveRound()
    {
        yield return new WaitForSeconds(3f);

        for (int i = 0; i < roundIndices; i++)
        {
            if (PhotonNetwork.NickName == playerNamesR[i])
            {
                delayTimeRound = (i + 1) * 2;
            }
        }

        LeaveRoundForEvent();
    }

    IEnumerator DelayToStartGame()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Multi");
    }

    IEnumerator DelayToStopCreate()
    {
        yield return new WaitForSeconds(5f);

        if (!isMatched)
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.JoinLobby(TypedLobby.Default);
            testOutput.text = "leave room";
        }
    }

    public override void OnConnectedToMaster()
    {
        if (isTournament)
        {

        }
        else
        {
            if (isConnecting)
            {
                loadingUI.SetActive(false);
                onlineUI.SetActive(true);
                if (!PhotonNetwork.InLobby)
                    PhotonNetwork.JoinLobby(TypedLobby.Default);
            }
            else if (isJoinedEvent)
            {
                if (!PhotonNetwork.InLobby)
                    PhotonNetwork.JoinLobby(TypedLobby.Default);
            }
            else if (isListedRound)
            {
                if (!PhotonNetwork.InLobby)
                    PhotonNetwork.JoinLobby(TypedLobby.Default);
            }
        }
    }    

    public override void OnDisconnected(DisconnectCause cause)
    {
        SceneManager.LoadScene("Main");
      //  print("Disconnect.");
    }

    public override void OnJoinedLobby()
    {
        print("Joined Lobby");

        if (!isConnectedFirst && isConnecting)
        {
            RefreshRoomList();
        }
        else if (isJoinedEvent)
        {
            //PhotonNetwork.NickName = PlayerPrefs.GetString("PLAYER_NAME") + " L" + PlayerPrefs.GetInt("LEVEL");
            
            PhotonNetwork.NickName = PlayerPrefs.GetString("PLAYER_NAME");
            
            StartCoroutine(DelayForJoinning());
        }
        else if (isListedRound)
        {
            //PhotonNetwork.NickName = PlayerPrefs.GetString("PLAYER_NAME") + " L" + PlayerPrefs.GetInt("LEVEL");

            PhotonNetwork.NickName = PlayerPrefs.GetString("PLAYER_NAME");   
            
            waitingUI.SetActive(true);
            StartCoroutine(DelaytoMatch());
        }

        bool shouldstartTournament=true;
        if (shouldstartTournament)
        {

            //RoomOptions roomOptions = new RoomOptions();
            //roomOptions.MaxPlayers = (byte)TournamentLobbyCreator.MaxPlayersInTournament;
            //roomOptions.CleanupCacheOnLeave = false;
            //roomOptions.PlayerTtl = 500;
            //roomOptions.EmptyRoomTtl = 600;

            //PhotonNetwork.KeepAliveInBackground = 60000;
            //PhotonNetwork.CreateRoom(TournamentLobbyCreator.TournamentRoomName, roomOptions, TypedLobby.Default);
        }
        if(TournamentLobbyCreator.isInTournament())
        {
            JoinBtnClick(true, TournamentLobbyCreator.TournamentRoomName);
        }


    }
    private IEnumerator setTimerTime()
    {
        float durationInSeconds = 120f;
        WWWForm form = new WWWForm();
        form.AddField("user_id", PlayerPrefs.GetInt("USER_ID"));
        UnityWebRequest request = UnityWebRequest.Post("https://subgrids.com/api/getUserSetting", form);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("Turn: Error While Sending: " + request.error);
            durationInSeconds = 120f;

        }
        else
        {
            Debug.LogError("Turn: Received: " + request.downloadHandler.text);




            Root parsedJson = JsonConvert.DeserializeObject<Root>(request.downloadHandler.text);
            durationInSeconds = parsedJson.setting.duration * 60f;


            if (durationInSeconds <= 120)
            {
                durationInSeconds = 120f;
            }
        }

        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable
            {
                 { "AnimationDuration", durationInSeconds}
            };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
    }
    private IEnumerator setCustomDataforAI()
    {
        Debug.LogError("Setting ai data");
        PlayerPrefs.SetInt("USER_ID",54);
        PlayerPrefs.Save();
        float durationInSeconds = 120f;
        string waveType = "";
        WWWForm form = new WWWForm();
        form.AddField("user_id", PlayerPrefs.GetInt("USER_ID"));
        UnityWebRequest request = UnityWebRequest.Post("https://subgrids.com/api/getUserSetting", form);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError("Turn: Error While Sending: " + request.error);
            durationInSeconds = 120f;

        }
        else
        {
            Debug.LogError("Turn: Received: " + request.downloadHandler.text);




            Root parsedJson = JsonConvert.DeserializeObject<Root>(request.downloadHandler.text);
            durationInSeconds = parsedJson.setting.duration * 60f;
            string waves = parsedJson.setting.wave_option;
            waveType = waves;

            if (durationInSeconds <= 120)
            {
                durationInSeconds = 120f;
            }
            Debug.LogError("waveinit : " + waveType);
        }

        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable
            {
                 { "Ai_AnimationDuration", durationInSeconds},
            {"Ai_WaveType",waveType }
            };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
    }

    [PunRPC()]
    public void RPC_AddToTOurnamentRoomlist()
    {

    }
    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom.Name == TournamentLobbyCreator.TournamentRoomName)
        {
            tournamnetloading.SetActive(false);
        }

        if (isTournament)
        {
            Debug.Log($">>> total room players {PhotonNetwork.CurrentRoom.MaxPlayers}  :    {PhotonNetwork.CurrentRoom.IsOpen}");
        }
        else
        {
            print("Join Room: " + PhotonNetwork.CurrentRoom.Name);
            testOutput.text = "Join Room: " + PhotonNetwork.CurrentRoom.Name;
            rooms = new List<RoomInfo>();

            if (isConnecting)
            {
                playerNames[0] = player1Name.text;
                playerNames[1] = player2Name.text;

                if (!PhotonNetwork.IsMasterClient)
                {
                    SyncSkin(new object[] { PlayerPrefs.GetInt("SKIN1"), PlayerPrefs.GetInt("SKIN2"), 1 });
                    Debug.LogError("Setting ai data2");
                    //    StartCoroutine(setCustomDataforAI());
                }
                else
                {
                    Debug.LogError("Setting ai data1");
                    StartCoroutine(setTimerTime());


                }
            }
            else if (isJoinedEvent)
            {
                waitingUI.SetActive(false);
                eventUI.SetActive(true);

                if (roundIndices == 4)
                {
                    playerNamesE[0].SetActive(false);
                    playerNamesE[1].SetActive(false);
                    playerNamesE[6].SetActive(false);
                    playerNamesE[7].SetActive(false);
                    indexPlus = 2;
                }
                else if (roundIndices == 2)
                {
                    playerNamesE[0].SetActive(false);
                    playerNamesE[1].SetActive(false);
                    playerNamesE[2].SetActive(false);
                    playerNamesE[3].SetActive(false);
                    playerNamesE[6].SetActive(false);
                    playerNamesE[7].SetActive(false);
                    trophy.SetActive(true);
                    indexPlus = 4;
                }

                if (PhotonNetwork.IsMasterClient)
                {
                    playerNamesE[0 + indexPlus].transform.GetChild(0).GetComponent<Text>().text = PhotonNetwork.NickName;
                }
                else
                {
                    RefreshRound(roundIndices);
                }
            }
            else if (isListedRound)
            {
                playerNames[0] = PhotonNetwork.CurrentRoom.Name;
                playerNames[1] = PhotonNetwork.NickName;

                if (PhotonNetwork.IsMasterClient)
                {
                    StartCoroutine(DelayToStopCreate());
                }
            }
        }
    }

    public override void OnCreatedRoom()
    {
        print("Create Room.");              
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (isTournament)
        {

        }
        else
        {
            if (isConnecting)
            {

                
                player2Name.text = newPlayer.NickName;
                playerNames[1] = player2Name.text;
                multiStartBtn.SetActive(true);
                logoInRoom.SetActive(false);
                prepareText.SetActive(true);
                warningText.SetActive(false);

                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;

                SyncSkin(new object[] { PlayerPrefs.GetInt("SKIN1"), PlayerPrefs.GetInt("SKIN2"), 0 });
                SyncGameOptions(new object[] { gameOptions });
            }
            else if (isJoinedEvent)
            {
                if (PhotonNetwork.PlayerList.Length >= roundIndices)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    PhotonNetwork.CurrentRoom.IsVisible = false;
                    print("Room is full");

                    if (PhotonNetwork.IsMasterClient)
                    {
                        for (int i = 0; i < roundIndices; i++)
                        {
                            SyncRoundInfos(new object[] { i, PhotonNetwork.PlayerList[i].NickName });
                        }

                        SyncRoundStart(new object[] { });
                    }
                }

                RefreshRound(roundIndices);
            }
            else if (isListedRound)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
                playerNames[0] = PhotonNetwork.NickName;
                playerNames[1] = newPlayer.NickName;
                isMatched = true;

                if (PhotonNetwork.IsMasterClient)
                {
                    StartCoroutine(DelayToStartGame());
                }
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (isTournament)
        {

        }
        else
        {


            if (isConnecting)
            {
                player2Name.text = "Open";
                player2Avatar.GetComponent<Image>().sprite = player2Skins[0];
                multiStartBtn.SetActive(false);
                logoInRoom.SetActive(true);
                prepareText.SetActive(false);
                warningText.SetActive(true);

                PhotonNetwork.CurrentRoom.IsOpen = true;
                PhotonNetwork.CurrentRoom.IsVisible = true;
            }
            else if (isJoinedEvent)
            {
                PhotonNetwork.CurrentRoom.IsOpen = true;
                PhotonNetwork.CurrentRoom.IsVisible = true;
                RefreshRound(roundIndices);
            }
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        print("Failed to create room" + message);

        if (isJoinedEvent)
        {
            waitingUI.SetActive(false);
            playUI.SetActive(true);
            isJoinedEvent = false; 
            
            if (!PhotonNetwork.InLobby)
                PhotonNetwork.JoinLobby(TypedLobby.Default);
            StartCoroutine(DelayToShowAlert("Sorry! Pee Wee Event is full."));
        }
        else if (isListedRound)
        {
            if (!PhotonNetwork.InLobby)
                PhotonNetwork.JoinLobby(TypedLobby.Default);
        }
        else
        {
            waitingUI.SetActive(false);
            onlineUI.SetActive(true);
            roomUI.SetActive(false);
            if (!PhotonNetwork.InLobby)
                PhotonNetwork.JoinLobby(TypedLobby.Default);
            StartCoroutine(DelayToShowAlert("Please create room again."));
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {      
        Debug.LogError($"Failed to join room:   {returnCode}" + message);      

        if (isJoinedEvent)
        {
            waitingUI.SetActive(false);
            playUI.SetActive(true);
            isJoinedEvent = false;
            if (!PhotonNetwork.InLobby)
                PhotonNetwork.JoinLobby(TypedLobby.Default);
            StartCoroutine(DelayToShowAlert("Sorry! Pee Wee Event is full."));
        }
        else if (isListedRound)
        {
            if (!PhotonNetwork.InLobby)
                PhotonNetwork.JoinLobby(TypedLobby.Default);
        }
        else
        {
            waitingUI.SetActive(false);
            onlineUI.SetActive(true);
            roomUI.SetActive(false);
            if (!PhotonNetwork.InLobby)
                PhotonNetwork.JoinLobby(TypedLobby.Default);
            StartCoroutine(DelayToShowAlert("Please create room again."));
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (isConnecting)
        {
            if(!TournamentLobbyCreator.isInTournament())
            LeaveRoomBtnClick();
        }                
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomLists)
    {
        base.OnRoomListUpdate(roomLists);
        Debug.Log("in main room list update");
        testOutput.text = "updated";

        if (isConnecting)
        {
            if (isConnectedFirst)
            {
                rooms = roomLists;
                isConnectedFirst = false;
                RefreshRoomList();
            }
            else
            {
                foreach (RoomInfo info in roomLists)
                {
                    if (info.MaxPlayers == 2)
                    {
                        if (info.RemovedFromList)
                        {
                            rooms.Remove(info);
                            print("Removed 2");
                            RefreshRoomList();
                        }
                        else
                        {
                            rooms.Add(info);
                            print("Added 2");
                            RefreshRoomList();
                        }
                    }
                }
            }
        }
        else if (isJoinedEvent)
        {
            if (isConnectedFirst)
            {
                rooms = roomLists;
                isConnectedFirst = false;
            }
            else
            {
                foreach (RoomInfo info in roomLists)
                {
                    if (info.MaxPlayers == roundIndices)
                    {
                        if (info.RemovedFromList)
                        {
                            rooms.Remove(info);
                        }
                        else
                        {
                            rooms.Add(info);
                        }
                    }
                }
            }

            isAvailableRound = false;

            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].MaxPlayers == roundIndices)
                {
                    isAvailableRound = true;
                    break;
                }
            }
        }
        else if (isListedRound)
        {
            if (isConnectedFirst)
            {
                rooms = roomLists;
                isConnectedFirst = false;                
            }
            else
            {
                foreach (RoomInfo info in roomLists)
                {
                    if (info.RemovedFromList)
                    {
                        rooms.Remove(info);
                        print("Removed");                     
                    }
                    else
                    {
                        rooms.Add(info);
                        print("Added");                 
                    }
                }
            }

            isAvailableRoom = false;

            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].MaxPlayers == 2)
                {
                    isAvailableRoom = true;
                    print("Available: " + isAvailableRoom);
                    break;
                }
            }
        }
    }

    public void SyncSkin(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(100, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncGameOptions(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(101, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncRoundStart(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(102, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncRoundInfos(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(103, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if(eventCode == 100)
        {
            object[] infos = (object[])photonEvent.CustomData;

            if ((int)infos[2] == 0)
            {
                skinIndex1 = (int)infos[0];
                skinIndex2 = (int)infos[1];

                player1Avatar.GetComponent<Image>().sprite = player1Skins[skinIndex1];
                player2Avatar.GetComponent<Image>().sprite = player2Skins[skinIndex4];
            } 
            else if ((int)infos[2] == 1)
            {
                skinIndex3 = (int)infos[0];
                skinIndex4 = (int)infos[1];

                player2Avatar.GetComponent<Image>().sprite = player2Skins[skinIndex4];
            }
        }  
        else if(eventCode == 101)
        {
            object[] infos = (object[])photonEvent.CustomData;

            gameOptions = (string)infos[0];
        }
        else if(eventCode == 102)
        {
            isListedRound = true;
            isConnectedFirst = true;
            isJoinedEvent = false;
            backEventBtn.SetActive(false);
            StartCoroutine(DelayToLeaveRound());
        }
        else if(eventCode == 103)
        {
            object[] infos = (object[])photonEvent.CustomData;

            playerNamesR[(int)infos[0]] = (string)infos[1];
        }
    }

    #endregion

    #region UIFunctions    
    public void TutorialBtnClick()
    {
        SceneManager.LoadScene("Tutorial");
    }
    public Sprite GetPLayerSkin(int n)
    {
        return player1Skins[n];
    }
    public void TournamentCreateBtnClick(string roomname)
    {
        isTournament = false;
        if (playerNameCreate.text == "")
        {
            StartCoroutine(DelayToShowAlert("Please enter the name."));
        }
        else
        {
            onlineUI.SetActive(false);
            roomUI.SetActive(true);
            player1Avatar.GetComponent<Image>().sprite = player1Skins[PlayerPrefs.GetInt("SKIN1")];
            //PlayerPrefs.SetString("PLAYER_NAME", playerNameCreate.text);

            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsOpen = true;
            roomOptions.IsVisible = roomname!=""?false: true;
            roomOptions.MaxPlayers = (byte)2;

            // disconnect issue
            roomOptions.CleanupCacheOnLeave = false;
            roomOptions.PlayerTtl = 1500;
            roomOptions.EmptyRoomTtl = 2000;
            PhotonNetwork.KeepAliveInBackground = 60000;

            string playerCreateName = "";

            //playerCreateName += playerNameCreate.text + " L" + PlayerPrefs.GetInt("LEVEL");
            playerCreateName += playerNameCreate.text;

            if (gameOptions.Length >= 1)
            {
                playerCreateName += ", ";
            }

            for (int i = 0; i < gameOptions.Length; i++)
            {
                if (i != gameOptions.Length - 1)
                {
                    if (gameOptions[i] == '@')
                    {
                        playerCreateName += "UFO, ";
                    }

                    if (gameOptions[i] == '^')
                    {
                        playerCreateName += "BT, ";
                    }

                    if (gameOptions[i] == '*')
                    {
                        playerCreateName += "AM, ";
                    }

                    if (gameOptions[i] == '&')
                    {
                        playerCreateName += "MS, ";
                    }
                }
                else
                {
                    if (gameOptions[i] == '@')
                    {
                        playerCreateName += "UFO";
                    }

                    if (gameOptions[i] == '^')
                    {
                        playerCreateName += "BT";
                    }

                    if (gameOptions[i] == '*')
                    {
                        playerCreateName += "AM";
                    }

                    if (gameOptions[i] == '&')
                    {
                        playerCreateName += "MS";
                    }
                }
            }

            PhotonNetwork.CreateRoom(roomname, roomOptions, TypedLobby.Default);
            player1Name.text = playerNameCreate.text;
            playerNames[0] = player1Name.text;
            PhotonNetwork.NickName = player1Name.text;
        }
    }
    public void CreateBtnClick()
    {
        isTournament = false;
        if(playerNameCreate.text == "")
        {
            StartCoroutine(DelayToShowAlert("Please enter the name."));
        }
        else
        {
            onlineUI.SetActive(false);
            roomUI.SetActive(true);
            player1Avatar.GetComponent<Image>().sprite = player1Skins[PlayerPrefs.GetInt("SKIN1")];
            //PlayerPrefs.SetString("PLAYER_NAME", playerNameCreate.text);

            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsOpen = true;
            roomOptions.IsVisible = true;
            roomOptions.MaxPlayers = (byte)2;

            // disconnect issue
            roomOptions.CleanupCacheOnLeave = false;
            roomOptions.PlayerTtl = 500;
            roomOptions.EmptyRoomTtl = 600;
            PhotonNetwork.KeepAliveInBackground = 60000;

            string playerCreateName = "";

            //playerCreateName += playerNameCreate.text + " L" + PlayerPrefs.GetInt("LEVEL");
            playerCreateName += playerNameCreate.text;
            
            if(gameOptions.Length >= 1)
            {
                playerCreateName += ", ";
            } 

            for(int i = 0; i < gameOptions.Length; i++)
            {
                if (i != gameOptions.Length - 1)
                {
                    if (gameOptions[i] == '@')
                    {
                        playerCreateName += "UFO, ";
                    }

                    if (gameOptions[i] == '^')
                    {
                        playerCreateName += "BT, ";
                    }

                    if (gameOptions[i] == '*')
                    {
                        playerCreateName += "AM, ";
                    }

                    if (gameOptions[i] == '&')
                    {
                        playerCreateName += "MS, ";
                    }
                }
                else
                {
                    if (gameOptions[i] == '@')
                    {
                        playerCreateName += "UFO";
                    }

                    if (gameOptions[i] == '^')
                    {
                        playerCreateName += "BT";
                    }

                    if (gameOptions[i] == '*')
                    {
                        playerCreateName += "AM";
                    }

                    if (gameOptions[i] == '&')
                    {
                        playerCreateName += "MS";
                    }
                }                                
            }

            PhotonNetwork.CreateRoom(playerCreateName, roomOptions, TypedLobby.Default);
            player1Name.text = playerCreateName;
            playerNames[0] = player1Name.text;
            PhotonNetwork.NickName = playerCreateName;



        }                
    }
    public void JoinBtnClick( string roomname="")
    {
        isTournament = false;

        if(roomname!="")
        {
            selectedRoom = roomname;
            TournamentLobbyCreator.SetIsinTournament( true);


        }
        else
        {
            Debug.Log("join btn click null");
            TournamentLobbyCreator.SetIsinTournament(false);
        }
        if (TournamentLobbyCreator.Instance!= null)
        {
            TournamentLobbyCreator.Instance.mymatchroom = "";
        }
            if (selectedRoom == "")
            {
                StartCoroutine(DelayToShowAlert("Please select the room."));
            }
            else if (playerNameCreate.text == "")
            {
                StartCoroutine(DelayToShowAlert("Please enter the name."));
            }
            else
            {
                if (selectedRoom != "Pee Wee (AI)")
                {
                    PhotonNetwork.JoinRoom(selectedRoom);
                if (TournamentLobbyCreator.isInTournament())
                {

                    player1Name.text = PlayerPrefs.GetString(Prefs.otherplayername, selectedRoom);
                }
                else
                {
                    player1Name.text = selectedRoom;
                }
                
                onlineUI.SetActive(false);
                    roomUI.SetActive(true);

                    //PhotonNetwork.NickName = playerNameCreate.text + " L" + PlayerPrefs.GetInt("LEVEL");     
                    PhotonNetwork.NickName = playerNameCreate.text;

                    //player2Name.text = playerNameCreate.text + " L" + PlayerPrefs.GetInt("LEVEL");
                    player2Name.text = playerNameCreate.text;
                    playerNames[0] = player1Name.text;
                    playerNames[1] = player2Name.text;
                }
                else
                {
                    PlaySoloBtnClick();
                }
            }
        
    }
    public void JoinTournament()
    {
        SceneManager.LoadScene("TournamentManager");
    }
    public void JoinBtnClick( bool istournament,string roomname="")
    {
        // Debug.LogError("join btn 2  : " + istournament);
        TournamentLobbyCreator.SetIsinTournament(istournament);

        TournamentLobbyCreator.SavePlayerName(playerNameCreate.text);
        
        this.isTournament = istournament;
        if (TournamentLobbyCreator.Instance != null)
        {
            TournamentLobbyCreator.Instance.mymatchroom = "";
        }
        if (istournament)
        {
            if (roomname == "")
            {
                Debug.LogError("PLease add room name");
            }
            else if (playerNameCreate.text == "")
            {
                StartCoroutine(DelayToShowAlert("Please enter the name."));
            }
            else
            {
          

                RoomOptions roomOptions = new RoomOptions();
                roomOptions.MaxPlayers = (byte)TournamentLobbyCreator.MaxPlayersInTournament;
                roomOptions.CleanupCacheOnLeave = false;
                roomOptions.PlayerTtl = 500;
                roomOptions.EmptyRoomTtl = 600;

                PhotonNetwork.KeepAliveInBackground = 60000;
                PhotonNetwork.JoinOrCreateRoom(roomname, roomOptions, TypedLobby.Default);
                player1Name.text = selectedRoom;
                onlineUI.SetActive(false);
                roomUI.SetActive(true);

                //PhotonNetwork.NickName = playerNameCreate.text + " L" + PlayerPrefs.GetInt("LEVEL");     
                PhotonNetwork.NickName = playerNameCreate.text;

                //player2Name.text = playerNameCreate.text + " L" + PlayerPrefs.GetInt("LEVEL");
                //player2Name.text = playerNameCreate.text;
                //playerNames[0] = player1Name.text;
                //playerNames[1] = player2Name.text;
            }
        }
        else
        {
            if (selectedRoom == "")
            {
                StartCoroutine(DelayToShowAlert("Please select the room."));
            }
            else if (playerNameCreate.text == "")
            {
                StartCoroutine(DelayToShowAlert("Please enter the name."));
            }
            else
            {
                if (selectedRoom != "Pee Wee (AI)")
                {
                    PhotonNetwork.JoinRoom(selectedRoom);
                    player1Name.text = selectedRoom;
                    onlineUI.SetActive(false);
                    roomUI.SetActive(true);

                    //PhotonNetwork.NickName = playerNameCreate.text + " L" + PlayerPrefs.GetInt("LEVEL");     
                    PhotonNetwork.NickName = playerNameCreate.text;

                    //player2Name.text = playerNameCreate.text + " L" + PlayerPrefs.GetInt("LEVEL");
                    player2Name.text = playerNameCreate.text;
                    playerNames[0] = player1Name.text;
                    playerNames[1] = player2Name.text;
                }
                else
                {
                    PlaySoloBtnClick();
                }
            }
        }
    }

    public void RoomListClick()
    {
        for(int i = 0; i < roomList.Length; i++)
        {
            roomBack[i].color = Color.black;
            roomName[i].color = Color.white;
        }

        selectedRoom = EventSystem.current.currentSelectedGameObject.GetComponent<Text>().text;

        EventSystem.current.currentSelectedGameObject.GetComponent<Text>().color = Color.red;
        EventSystem.current.currentSelectedGameObject.GetComponent<Text>().GetComponentInParent<Image>().color = Color.red;

        if (isDoubleClick)
        {
            if (selectedRoom == "Pee Wee (AI)")
            {
                PlaySoloBtnClick();
            }
            else if (selectedRoom == TournamentLobbyCreator.TournamentRoomName)
            {
                JoinBtnClick(TournamentLobbyCreator.TournamentRoomName); //not tournament

            }
            else
            {
                JoinBtnClick(); //not tournament
            }
        }
        
        
                 
    }
    

    public void LeaveRoomBtnClick()
    {
        roomUI.SetActive(false);
        onlineUI.SetActive(true);
        player2Name.text = "Open";
        player2Avatar.GetComponent<Image>().sprite = player2Skins[0];
        multiStartBtn.SetActive(false);
        logoInRoom.SetActive(true);
        prepareText.SetActive(false);
        warningText.SetActive(true);

        StartCoroutine(GetUserInfos(frontURL + "/api/getUserData"));
        //StartCoroutine(GetUserInfos("http://192.168.106.133/api/getUserData"));

        PhotonNetwork.LeaveRoom();
        if (!PhotonNetwork.InLobby)
            PhotonNetwork.JoinLobby(TypedLobby.Default);        
    }

    public void ClickAvatar()
    {
        skinUI.SetActive(true);
        isSelectingSkin = true;
    }

    public void SkinSelectBtnClick()
    {
        skinUI.SetActive(false);
        isSelectingSkin = false;
        int selectedSkins = 0;

        if (skinsSelect[12].isOn)
        {
            PlayerPrefs.SetInt("SKIN1", 12);
            selectedSkins++;
        }

        for (int i = 1; i < skinsSelect.Length; i++)
        {
            if (skinsSelect[i].isOn && i != 12)
            {
                if (selectedSkins == 0)
                {
                    selectedSkins++;
                    PlayerPrefs.SetInt("SKIN1", i);
                }
                else
                {
                    selectedSkins++;
                    PlayerPrefs.SetInt("SKIN2", i);
                    break;
                }
            }
        }

        if (selectedSkins == 0)
        {
            PlayerPrefs.SetInt("SKIN1", 12);
            PlayerPrefs.SetInt("SKIN2", 12);
        }
        else if (selectedSkins == 1)
        {
            PlayerPrefs.SetInt("SKIN2", 12);
        }

        if (PlayerPrefs.GetInt("SKIN1") == 12)
        {
            skinsImage1.sprite = player1Skins[PlayerPrefs.GetInt("SKIN2")];
            PlayerPrefs.SetInt("SKIN1", PlayerPrefs.GetInt("SKIN2"));
        }
        else
        {
            skinsImage1.sprite = player1Skins[PlayerPrefs.GetInt("SKIN1")];
        }

        if (PlayerPrefs.GetInt("SKIN2") == 12)
        {
            skinsImage2.sprite = player1Skins[PlayerPrefs.GetInt("SKIN1")];
            PlayerPrefs.SetInt("SKIN2", PlayerPrefs.GetInt("SKIN1"));
        }
        else
        {
            skinsImage2.sprite = player1Skins[PlayerPrefs.GetInt("SKIN2")];
        }

        StartCoroutine(UpdateSkinSelection(frontURL + "/api/updateSkinSelection"));
    }

    public void SkinListsClick()
    {
        skinDetailDialog.SetActive(true);
        skinsContent.transform.position = skinsContentOriginal.transform.position;
        int skinIndex = int.Parse(EventSystem.current.currentSelectedGameObject.name.Split(" ")[1]);

        for (int i = 0; i < skinDetails.Length; i++)
        {
            skinDetails[i].gameObject.SetActive(false);
        }

        switch (skinIndex)
        {
            case 0:
                {
                    for (int i = 0; i < skinSprites0.Length; i++)
                    {
                        skinDetails[i].gameObject.SetActive(true);
                        skinDetails[i].sprite = skinSprites0[i];
                        skinDetailsTitle.text = "Green Army";
                    }
                    break;
                }
            case 1:
                {
                    for (int i = 0; i < skinSprites1.Length; i++)
                    {
                        skinDetails[i].gameObject.SetActive(true);
                        skinDetails[i].sprite = skinSprites1[i];
                        skinDetailsTitle.text = "Sea Aliens";
                    }
                    break;
                }
            case 2:
                {
                    for (int i = 0; i < skinSprites2.Length; i++)
                    {
                        skinDetails[i].gameObject.SetActive(true);
                        skinDetails[i].sprite = skinSprites2[i];
                        skinDetailsTitle.text = "Golden Army";
                    }
                    break;
                }
            case 3:
                {
                    for (int i = 0; i < skinSprites3.Length; i++)
                    {
                        skinDetails[i].gameObject.SetActive(true);
                        skinDetails[i].sprite = skinSprites3[i];
                        skinDetailsTitle.text = "Centaur Army";
                    }
                    break;
                }
            case 4:
                {
                    for (int i = 0; i < skinSprites4.Length; i++)
                    {
                        skinDetails[i].gameObject.SetActive(true);
                        skinDetails[i].sprite = skinSprites4[i];
                        skinDetailsTitle.text = "Viking Army";
                    }
                    break;
                }
            case 5:
                {
                    for (int i = 0; i < skinSprites5.Length; i++)
                    {
                        skinDetails[i].gameObject.SetActive(true);
                        skinDetails[i].sprite = skinSprites5[i];
                        skinDetailsTitle.text = "Strawberry Army";
                    }
                    break;
                }
            case 6:
                {
                    for (int i = 0; i < skinSprites6.Length; i++)
                    {
                        skinDetails[i].gameObject.SetActive(true);
                        skinDetails[i].sprite = skinSprites6[i];
                        skinDetailsTitle.text = "Fire Engine";
                    }
                    break;
                }
            case 7:
                {
                    for (int i = 0; i < skinSprites7.Length; i++)
                    {
                        skinDetails[i].gameObject.SetActive(true);
                        skinDetails[i].sprite = skinSprites7[i];
                        skinDetailsTitle.text = "Vegas Aliens";
                    }
                    break;
                }
            case 8:
                {
                    for (int i = 0; i < skinSprites8.Length; i++)
                    {
                        skinDetails[i].gameObject.SetActive(true);
                        skinDetails[i].sprite = skinSprites8[i];
                        skinDetailsTitle.text = "Water Army";
                    }
                    break;
                }
            case 9:
                {
                    for (int i = 0; i < skinSprites9.Length; i++)
                    {
                        skinDetails[i].gameObject.SetActive(true);
                        skinDetails[i].sprite = skinSprites9[i];
                        skinDetailsTitle.text = "Future Army";
                    }
                    break;
                }
            case 10:
                {
                    for (int i = 0; i < skinSprites10.Length; i++)
                    {
                        skinDetails[i].gameObject.SetActive(true);
                        skinDetails[i].sprite = skinSprites10[i];
                        skinDetailsTitle.text = "Minotaur Army";
                    }
                    break;
                }
            case 11:
                {
                    for (int i = 0; i < skinSprites11.Length; i++)
                    {
                        skinDetails[i].gameObject.SetActive(true);
                        skinDetails[i].sprite = skinSprites11[i];
                        skinDetailsTitle.text = "Peace Army";
                    }
                    break;
                }
            case 13:
                {
                    for (int i = 0; i < skinSprites12.Length; i++)
                    {
                        skinDetails[i].gameObject.SetActive(true);
                        skinDetails[i].sprite = skinSprites12[i];
                        skinDetailsTitle.text = "Patriot Army";
                    }
                    break;
                }
            case 14:
                {
                    for (int i = 0; i < skinSprites13.Length; i++)
                    {
                        skinDetails[i].gameObject.SetActive(true);
                        skinDetails[i].sprite = skinSprites13[i];
                        skinDetailsTitle.text = "Swashbuckler Army";
                    }
                    break;
                }
        }
    }

    public void SkinsCheckClick()
    {
        if (isSelectingSkin)
        {
            int checkedIndex = int.Parse(EventSystem.current.currentSelectedGameObject.name.Split(" ")[1]);
            List<int> checkedSkins = new List<int>();

            for (int i = 0; i < skinsSelect.Length; i++)
            {
                if (skinsSelect[i].isOn && i != 12)
                {
                    checkedSkins.Add(i);
                }
            }

            if (checkedSkins.Count == 3)
            {
                for (int i = 0; i < skinsSelect.Length; i++)
                {
                    if (skinsSelect[i].isOn && i != 12 && i != checkedIndex)
                    {
                        skinsSelect[i].isOn = false;
                        break;
                    }
                }
            }
        }
    }

    public void OptionSelectBtnClick()
    {
        gameOptionsUI.SetActive(false);
        gameOptions = "";
        selectedOptions = "0000";

        if (optionsSelect[0].isOn)
        {
            gameOptions += "@";
            gameOptionsImage.sprite = optionsSprite[0];
            selectedOptions = "1" + selectedOptions[1].ToString() + selectedOptions[2].ToString() + selectedOptions[3].ToString();
        }

        if (optionsSelect[1].isOn)
        {
            gameOptions += "^";
            gameOptionsImage.sprite = optionsSprite[1];
            selectedOptions = selectedOptions[0].ToString() + "1" + selectedOptions[2].ToString() + selectedOptions[3].ToString();
            //print(selectedOptions);
        }

        if (optionsSelect[2].isOn)
        {
            gameOptions += "*";
            gameOptionsImage.sprite = optionsSprite[2];
            selectedOptions = selectedOptions[0].ToString() + selectedOptions[1].ToString() + "1" + selectedOptions[3].ToString();
            //print(selectedOptions);
        }

        if (optionsSelect[3].isOn)
        {
            gameOptions += "&";
            gameOptionsImage.sprite = optionsSprite[3];
            selectedOptions = selectedOptions[0].ToString() + selectedOptions[1].ToString() + selectedOptions[2].ToString() + "1";
            //print(selectedOptions);
        }

        if (gameOptions == "")
        {
            onlineOptoinsText.text = "None";
            gameOptionsImage.sprite = optionsSprite[0];
        }
        else
        {
            onlineOptoinsText.text = AddGameoptionsInfo();
        }

        PlayerPrefs.SetString("GAME_OPTION", gameOptions);        
        StartCoroutine(UpdateOptionSelection(frontURL + "/api/updateOptionSelection", selectedOptions));
    }

    public void OptionListsClick()
    {
        optionsDetailDialog.SetActive(true);
        int optionIndex = int.Parse(EventSystem.current.currentSelectedGameObject.name.Split(" ")[1]);

        for (int i = 0; i < optionDetailsText.Length; i++)
        {
            optionDetailsText[i].gameObject.SetActive(false);
        }

        if (optionIndex == 0)
        {
            optionDetailsTitle.text = "Project: UFO";
            optionDetailsText[0].gameObject.SetActive(true);
            gameOptionsDetailImage.sprite = optionsSprite[0];
        }
        else if (optionIndex == 1)
        {
            optionDetailsTitle.text = "Bermuda Triangle";
            optionDetailsText[1].gameObject.SetActive(true);
            gameOptionsDetailImage.sprite = optionsSprite[1];
        }
        else if (optionIndex == 2)
        {
            optionDetailsTitle.text = "Atomic Mine";
            optionDetailsText[2].gameObject.SetActive(true);
            gameOptionsDetailImage.sprite = optionsSprite[2];
        }
        else if (optionIndex == 3)
        {
            optionDetailsTitle.text = "Map Shot";
            optionDetailsText[3].gameObject.SetActive(true);
            gameOptionsDetailImage.sprite = optionsSprite[3];
        }
    }
    public string AddGameoptionsInfo()
    {
        string tempOptoinsInfo = "";

        for (int i = 0; i < gameOptions.Length; i++)
        {
            if (i != gameOptions.Length - 1)
            {
                if (gameOptions[i] == '@')
                {
                    tempOptoinsInfo += "UFO, ";
                }

                if (gameOptions[i] == '^')
                {
                    tempOptoinsInfo += "BT, ";
                }

                if (gameOptions[i] == '*')
                {
                    tempOptoinsInfo += "AM, ";
                }

                if (gameOptions[i] == '&')
                {
                    tempOptoinsInfo += "MS, ";
                }
            }
            else
            {
                if (gameOptions[i] == '@')
                {
                    tempOptoinsInfo += "UFO";
                }

                if (gameOptions[i] == '^')
                {
                    tempOptoinsInfo += "BT";
                }

                if (gameOptions[i] == '*')
                {
                    tempOptoinsInfo += "AM";
                }

                if (gameOptions[i] == '&')
                {
                    tempOptoinsInfo += "MS";
                }
            }
        }

        return tempOptoinsInfo;
    }

    public void SkinPurchaseBtnClick()
    {
        StartCoroutine(DelayToShowSkinsDescription());
    }

    public void OptionsPurchaseBtnClick()
    {
        StartCoroutine(DelayToShowOptionsDescription());
    }

    IEnumerator DelayToShowSkinsDescription()
    {
        skinPurchaseDescription.SetActive(true);
        yield return new WaitForSeconds(3f);
        skinPurchaseDescription.SetActive(false);
    }

    IEnumerator DelayToShowOptionsDescription()
    {
        optionsPurchaseDescription.SetActive(true);
        yield return new WaitForSeconds(3f);
        optionsPurchaseDescription.SetActive(false);
    }

    public void StartMultiplayer()
    {                
        PhotonNetwork.CurrentRoom.IsVisible = false;
        SceneManager.LoadScene("Multi");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void GoBack()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
            Application.ExternalEval("history.back()");
#endif
    }

    public int CalculateCurrentRound(string timeNow, string dayNow)
    {
        int tempRound = 0;
        startingTime = 0;     
        
        eventParameter.hour = int.Parse(timeNow.Split(':')[0]);
        eventParameter.minute = int.Parse(timeNow.Split(':')[1]);
        eventParameter.second = int.Parse(timeNow.Split(':')[2]);

        if (eventParameter.hour == 19 && (eventParameter.minute >= 0 && eventParameter.minute < 5))
        {
            tempRound = 1;
            roundIndices = 8;
            startingTime = (300 - eventParameter.minute * 60 - eventParameter.second);
        }
        else if(eventParameter.hour == 19 && (eventParameter.minute >= 50 && eventParameter.minute < 55))
        {
            tempRound = 2;
            roundIndices = 4;
            startingTime = (300 - (eventParameter.minute - 50) * 60 - eventParameter.second);
        }
        else if(eventParameter.hour == 20 && (eventParameter.minute >= 40 && eventParameter.minute < 45))
        {
            tempRound = 3;
            roundIndices = 2;
            startingTime = (300 - (eventParameter.minute - 40) * 60 - eventParameter.second);
        }

        if((eventParameter.hour == 19 || eventParameter.hour == 20) && string.Equals(dayNow, "Friday"))
        {
            isEventTime = true;
        }
        else
        {
            isEventTime = false;
        }
  
        isTimeRefreshed = true;
        return tempRound;
    }

    public void setUserinfos(string skinsStr, string optionsStr)
    {
        // check purchased skins & game options
        for (int i = 0; i < skinPurchaseBtns.Length; i++)
        {
            if (int.Parse(skinsStr[i].ToString()) == 1)
            {
                skinPurchaseBtns[i].gameObject.SetActive(false);
            }
        }

        skinPurchaseBtns[11].gameObject.SetActive(false);

        for (int i = 0; i < optionPurchaseBtns.Length; i++)
        {
            if (int.Parse(optionsStr[i].ToString()) == 1)
            {
                optionPurchaseBtns[i].gameObject.SetActive(false);
            }
        }

        // Set game options
        gameOptions = PlayerPrefs.GetString("GAME_OPTION");

        if (gameOptions == "")
        {
            onlineOptoinsText.text = "None";
        }
        else
        {
            onlineOptoinsText.text = AddGameoptionsInfo();

            for (int i = 0; i < gameOptions.Length; i++)
            {
                if (gameOptions[i] == '@')
                {
                    optionsSelect[0].isOn = true;
                    gameOptionsImage.sprite = optionsSprite[0];
                }
                else if (gameOptions[i] == '^')
                {
                    optionsSelect[1].isOn = true;
                    gameOptionsImage.sprite = optionsSprite[1];
                }
                else if (gameOptions[i] == '*')
                {
                    optionsSelect[2].isOn = true;
                    gameOptionsImage.sprite = optionsSprite[2];
                }
                else if (gameOptions[i] == '&')
                {
                    optionsSelect[3].isOn = true;
                    gameOptionsImage.sprite = optionsSprite[3];
                }
            }
        }

        // Set skins
        skinsSelect[PlayerPrefs.GetInt("SKIN1")].isOn = true;
        skinsSelect[PlayerPrefs.GetInt("SKIN2")].isOn = true;

        if (PlayerPrefs.GetInt("SKIN1") == 12)
        {
            skinsImage1.sprite = player1Skins[PlayerPrefs.GetInt("SKIN2")];
            PlayerPrefs.SetInt("SKIN1", PlayerPrefs.GetInt("SKIN2"));
        }
        else
        {
            skinsImage1.sprite = player1Skins[PlayerPrefs.GetInt("SKIN1")];
        }

        if (PlayerPrefs.GetInt("SKIN2") == 12)
        {
            skinsImage2.sprite = player1Skins[PlayerPrefs.GetInt("SKIN1")];
            PlayerPrefs.SetInt("SKIN2", PlayerPrefs.GetInt("SKIN1"));
        }
        else
        {
            skinsImage2.sprite = player1Skins[PlayerPrefs.GetInt("SKIN2")];
        }
    }

    IEnumerator GetUserInfos(string url)
    {
        Debug.Log("Get user Infos Called");
        WWWForm form = new WWWForm();
        form.AddField("user_id", PlayerPrefs.GetInt("USER_ID"));       

        UnityWebRequest uwr = UnityWebRequest.Post(url, form);

        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
            UserInfo loadData = JsonUtility.FromJson<UserInfo>(uwr.downloadHandler.text);            

            playerNameCreate.text = loadData.username;
            PlayerPrefs.SetString("PLAYER_NAME", loadData.username);
            PlayerPrefs.SetInt("SKIN1", loadData.skin1);
            PlayerPrefs.SetInt("SKIN2", loadData.skin2);
            PlayerPrefs.SetInt("WIN_COUNT", loadData.wincount);
            PlayerPrefs.SetInt("LOSE_COUNT", loadData.losecount);
            PlayerPrefs.SetInt("LEVEL", loadData.level);
            PlayerPrefs.SetInt("ROUND", loadData.round);
            playingCount.text = loadData.playingCounter.ToString();
            eventParameter.round = loadData.round;
            eventParameter.day = loadData.day;
            eventParameter.eventRound = CalculateCurrentRound(loadData.time, loadData.day);            

            if (loadData.skin1 == 0)
            {
                PlayerPrefs.SetInt("SKIN1", 12);
            }

            if(loadData.skin2 == 0)
            {
                PlayerPrefs.SetInt("SKIN2", 12);
            }

            if (PlayerPrefs.GetInt("SKIN1") == 12)
            {         
                PlayerPrefs.SetInt("SKIN1", PlayerPrefs.GetInt("SKIN2"));
            }

            if (PlayerPrefs.GetInt("SKIN2") == 12)
            { 
                PlayerPrefs.SetInt("SKIN2", PlayerPrefs.GetInt("SKIN1"));
            }

            // check game options
            gameOptions = "";

            if (loadData.options[0] == '1')
            {
                gameOptions += "@";
                selectedOptions = "1" + selectedOptions[1].ToString() + selectedOptions[2].ToString() + selectedOptions[3].ToString();             
            }
            
            if (loadData.options[1] == '1')
            {
                gameOptions += "^";
                selectedOptions = selectedOptions[0].ToString() + "1" + selectedOptions[2].ToString() + selectedOptions[3].ToString();
            }
            
            if (loadData.options[2] == '1')
            {
                gameOptions += "*";
                selectedOptions = selectedOptions[0].ToString() + selectedOptions[1].ToString() + "1" + selectedOptions[3].ToString();
            }

            if (loadData.options[3] == '1')
            {
                gameOptions += "&";
                selectedOptions = selectedOptions[0].ToString() + selectedOptions[1].ToString() + selectedOptions[2].ToString() + "1";
            }

            PlayerPrefs.SetString("GAME_OPTION", gameOptions);
            setUserinfos(loadData.skins, loadData.purchased_options);

            if (isEventTime && eventParameter.round == eventParameter.eventRound)
            {
                isAvailableEvent = true;
            }
            else
            {
                isAvailableEvent = false;
            }
        }
    }

    IEnumerator UpdateSkinSelection(string url)
    {
        WWWForm form = new WWWForm();
        form.AddField("userid", PlayerPrefs.GetInt("USER_ID"));
        form.AddField("skin1", PlayerPrefs.GetInt("SKIN1") - 1);
        form.AddField("skin2", PlayerPrefs.GetInt("SKIN2") - 1);

        UnityWebRequest uwr = UnityWebRequest.Post(url, form);

        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);                                    
        }
    }

    IEnumerator UpdateOptionSelection(string url, string optionsStr)
    {
        WWWForm form = new WWWForm();
        form.AddField("userid", PlayerPrefs.GetInt("USER_ID"));
        form.AddField("optionids", optionsStr);      

        UnityWebRequest uwr = UnityWebRequest.Post(url, form);

        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
        }
    }

    #endregion






    public List<customButtonLobby> btns;


    public void selectButton(customButtonLobby btn)
    {

        foreach (var item in btns)
        {
            if(item!=btn)
            item.SetSelected(false);
        }

        btn.SetSelected(true);
    }
    public void selectButtonInit()
    {
        for (int i = 0; i < btns.Count; i++)
        {
            btns[i].SetSelected(i == 0);
        }
    }
}
