using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine.Networking;
using System;
using Unity.Burst.CompilerServices;

public class MultiEngine : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public GameObject gameUI;
    enum TeamIndex
    {
        Player1 = 0,
        Player2 = 1
    }

    enum ForceType
    {
        General = 0,
        Plane = 1,
        Sub = 2,
        UFO = 3,
        Tank = 4,
        Mine = 5
    }

    enum CellType
    {
        Land = 0,
        Sea = 1,
        Mountain = 2,
        Tunnel = 3,
        Bridge = 4,
        TunnelEntrance = 5
    }

    enum SpecialType
    {
        Normal = 0,
        Bunker = 1
    }

    enum PositionType
    {
        Normal = 0,
        Corner = 1,
        Start = 2
    }

    enum TurnIndex
    {
        Player1 = 0,
        Player2 = 1
    }

    //Pathfinding
    public List<GameObject> pathTiles;
    bool isPath;
    public int pathIndex;

    public void PathFind(GameObject origin)
    {
        int x = 0;
        int y = 0;
        int x2 = 0;
        int y2 = 0;

        for (int i = 0; i < tiles.Length; i++)
        {
            //foreach tile check if the origin position is equal to the tile position meaning the origin of gameObject is on the tile
            if (Vector3.Distance(origin.transform.position, tiles[i].transform.position) < 0.1f)
            {
                x = i % 30;
                //There are total i%30
                y = i / 30;
            }

            if (Vector3.Distance(targetObject.transform.position, tiles[i].transform.position) < 0.1f)
            {
                x2 = i % 30;
                y2 = i / 30;
            }
        }

        bool[] vst = new bool[1000];
        int[] prevPoint = new int[1000];
        int[] dx = { 0, 1, 0, -1, 1, 1, -1, -1 };
        int[] dy = { -1, 0, 1, 0, -1, 1, 1, -1 };
        int[] queue = new int[2000];
        int qFront = 0;
        int qBack = 2;

        queue[0] = x;
        queue[1] = y;

        for (int i = 0; i < 1000; i++)
        {
            vst[i] = false;
            prevPoint[i] = -1;
        }

        vst[x + y * 30] = true;
        int targetPosition = x2 + y2 * 30;
        int originPosition = x + y * 30;

        while (qFront < qBack)
        {
            int currentX = queue[qFront++];
            int currentY = queue[qFront++];
            int currentPosition = currentX + currentY * 30;

            if (currentPosition == targetPosition)
            {
                break;
            }

            for (int i = 0; i < 8; i++)
            {
                int nextX = (currentX + dx[i] + 30) % 30;
                int nextY = (currentY + dy[i] + 30) % 30;

                if (nextX < 0 || nextY < 0 || nextX >= 30 || nextY >= 30)
                {
                    continue;
                }

                int nextPosition = nextY * 30 + nextX;

                if (tiles[nextPosition].GetComponent<SpriteRenderer>().enabled == true && vst[nextPosition] == false)
                {
                    vst[nextPosition] = true;
                    queue[qBack++] = nextX;
                    queue[qBack++] = nextY;
                    prevPoint[nextPosition] = currentPosition;
                }
            }
        }

        if (prevPoint[targetPosition] < 0)
        {
            print("Impossible");
            //IncreasePlayTimes();
        }

        while (prevPoint[targetPosition] > 0)
        {
            pathTiles.Add(tiles[targetPosition]);

            if (originPosition == targetPosition)
            {
                break;
            }

            targetPosition = prevPoint[targetPosition];

            if (pathTiles.Count > 0)
            {
                isMoveAnimation = true;
                pathIndex = pathTiles.Count - 1;
            }
        }
    }

    public List<GameObject> CheckAvailablePath(GameObject origin, GameObject target)
    {
        pathTiles = new List<GameObject>();
        int x = 0;
        int y = 0;
        int x2 = 0;
        int y2 = 0;

        for (int i = 0; i < tiles.Length; i++)
        {
            if (Vector3.Distance(origin.transform.position, tiles[i].transform.position) < 0.1f)
            {
                x = i % 30;
                y = i / 30;
            }

            if (Vector3.Distance(target.transform.position, tiles[i].transform.position) < 0.1f)
            {
                x2 = i % 30;
                y2 = i / 30;
            }
        }

        bool[] vst = new bool[1000];
        int[] prevPoint = new int[1000];
        int[] dx = { 0, 1, 0, -1, 1, 1, -1, -1 };
        int[] dy = { -1, 0, 1, 0, -1, 1, 1, -1 };
        int[] queue = new int[2000];
        int qFront = 0;
        int qBack = 2;

        queue[0] = x;
        queue[1] = y;

        for (int i = 0; i < 1000; i++)
        {
            vst[i] = false;
            prevPoint[i] = -1;
        }

        vst[x + y * 30] = true;
        int targetPosition = x2 + y2 * 30;
        int originPosition = x + y * 30;

        while (qFront < qBack)
        {
            int currentX = queue[qFront++];
            int currentY = queue[qFront++];
            int currentPosition = currentX + currentY * 30;

            if (currentPosition == targetPosition)
            {
                break;
            }

            for (int i = 0; i < 8; i++)
            {
                int nextX = (currentX + dx[i] + 30) % 30;
                int nextY = (currentY + dy[i] + 30) % 30;

                if (nextX < 0 || nextY < 0 || nextX >= 30 || nextY >= 30)
                {
                    continue;
                }

                int nextPosition = nextY * 30 + nextX;

                if (tiles[nextPosition].GetComponent<SpriteRenderer>().enabled == true && vst[nextPosition] == false)
                {
                    vst[nextPosition] = true;
                    queue[qBack++] = nextX;
                    queue[qBack++] = nextY;
                    prevPoint[nextPosition] = currentPosition;
                }
            }
        }

        while (prevPoint[targetPosition] > 0)
        {
            pathTiles.Add(tiles[targetPosition]);

            if (originPosition == targetPosition)
            {
                break;
            }

            targetPosition = prevPoint[targetPosition];
        }

        return pathTiles;
    }

    //MTN Strategy
    public List<int> mountains;

    public bool IsMTNStrategy(GameObject tile, int rangeIndex, int selectedX, int selectedY, int tileX, int tileY)
    {
        int mountainX;
        int mountainY;
        bool isMTN = false;

        for (int i = 0; i < mountains.Count; i++)
        {
            mountainX = mountains[i] % 30;
            mountainY = mountains[i] / 30;

            // Strategy for panning
            if (selectedX >= 0 && selectedX <= 8)
            {
                if (tileX >= 22 && tileX <= 29)
                {
                    selectedX += 30;
                }

                if (mountainX >= 22 && mountainX <= 29)
                {
                    mountainX += 30;
                }
            }
            else if (selectedX >= 22 && selectedX <= 29)
            {
                if (tileX >= 0 && tileX <= 8)
                {
                    tileX += 30;
                }

                if (mountainX >= 0 && mountainX <= 8)
                {
                    mountainX += 30;
                }
            }

            if (selectedY >= 0 && selectedY <= 8)
            {
                if (tileY >= 22 && tileY <= 29)
                {
                    selectedY += 30;
                }

                if (mountainY >= 22 && mountainY <= 29)
                {
                    mountainY += 30;
                }
            }
            else if (selectedY >= 22 && selectedY <= 29)
            {
                if (tileY >= 0 && tileY <= 8)
                {
                    tileY += 30;
                }

                if (mountainY >= 0 && mountainY <= 8)
                {
                    mountainY += 30;
                }
            }

            // ---------------- Mtn logic for selected unit -------------------
            if (selectedY == mountainY)
            {
                // Mtn strategy 1
                if (selectedX == mountainX - 1)
                {
                    if (tileX > mountainX && (Mathf.Abs(tileX - selectedX) > Mathf.Abs(tileY - selectedY)))
                    {
                        isMTN = true;
                    }
                }
                else if (selectedX == mountainX + 1)
                {
                    if (tileX < mountainX && (Mathf.Abs(tileX - selectedX) > Mathf.Abs(tileY - selectedY)))
                    {
                        isMTN = true;
                    }
                }

                // Mtn strategy 2
                if (selectedX == mountainX - 2 && tileX > mountainX)
                {
                    if (selectedY == tileY)
                    {
                        isMTN = true;
                    }
                    else
                    {
                        if (Mathf.Abs(tileX - mountainX) < 2 && Mathf.Abs(tileY - mountainY) < 2)
                        {
                            isMTN = true;
                        }
                    }
                }
                else if (selectedX == mountainX + 2 && tileX < mountainX)
                {
                    if (selectedY == tileY)
                    {
                        isMTN = true;
                    }
                    else
                    {
                        if (Mathf.Abs(tileX - mountainX) < 2 && Mathf.Abs(tileY - mountainY) < 2)
                        {
                            isMTN = true;
                        }
                    }
                }

                // Same direction
                if (selectedX < mountainX && tileX > mountainX && tileY == mountainY)
                {
                    isMTN = true;
                }
                else if (selectedX > mountainX && tileX < mountainX && tileY == mountainY)
                {
                    isMTN = true;
                }
            }
            else if (selectedX == mountainX)
            {
                // Mtn strategy 1
                if (selectedY == mountainY - 1)
                {
                    if (tileY > mountainY && (Mathf.Abs(tileX - selectedX) < Mathf.Abs(tileY - selectedY)))
                    {
                        isMTN = true;
                    }
                }
                else if (selectedY == mountainY + 1)
                {
                    if (tileY < mountainY && (Mathf.Abs(tileX - selectedX) < Mathf.Abs(tileY - selectedY)))
                    {
                        isMTN = true;
                    }
                }

                // Mtn strategy 2
                if (selectedY == mountainY - 2 && tileY > mountainY)
                {
                    if (selectedX == tileX)
                    {
                        isMTN = true;
                    }
                    else
                    {
                        if (Mathf.Abs(tileX - mountainX) < 2 && Mathf.Abs(tileY - mountainY) < 2)
                        {
                            isMTN = true;
                        }
                    }
                }
                else if (selectedY == mountainY + 2 && tileY < mountainY)
                {
                    if (selectedX == tileX)
                    {
                        isMTN = true;
                    }
                    else
                    {
                        if (Mathf.Abs(tileX - mountainX) < 2 && Mathf.Abs(tileY - mountainY) < 2)
                        {
                            isMTN = true;
                        }
                    }
                }

                // Same direction
                if (selectedY < mountainY && tileY > mountainY && tileX == mountainX)
                {
                    isMTN = true;
                }
                else if (selectedY > mountainY && tileY < mountainY && tileX == mountainX)
                {
                    isMTN = true;
                }
            }

            // ---------------- Mtn logic for emeny unit -------------------
            if (tileY == mountainY)
            {
                // Mtn strategy 1
                if (tileX == mountainX - 1)
                {
                    if (selectedX > mountainX && (Mathf.Abs(tileX - selectedX) > Mathf.Abs(tileY - selectedY)))
                    {
                        isMTN = true;
                    }
                }
                else if (tileX == mountainX + 1)
                {
                    if (selectedX < mountainX && (Mathf.Abs(tileX - selectedX) > Mathf.Abs(tileY - selectedY)))
                    {
                        isMTN = true;
                    }
                }

                // Mtn strategy 2
                if (tileX == mountainX - 2 && selectedX > mountainX)
                {
                    if (selectedY == tileY)
                    {
                        isMTN = true;
                    }
                    else
                    {
                        if (Mathf.Abs(selectedX - mountainX) < 2 && Mathf.Abs(selectedY - mountainY) < 2)
                        {
                            isMTN = true;
                        }
                    }
                }
                else if (tileX == mountainX + 2 && selectedX < mountainX)
                {
                    if (selectedY == tileY)
                    {
                        isMTN = true;
                    }
                    else
                    {
                        if (Mathf.Abs(selectedX - mountainX) < 2 && Mathf.Abs(selectedY - mountainY) < 2)
                        {
                            isMTN = true;
                        }
                    }
                }

                // Same direction
                if (tileX < mountainX && selectedX > mountainX && selectedY == mountainY)
                {
                    isMTN = true;
                }
                else if (tileX > mountainX && selectedX < mountainX && selectedY == mountainY)
                {
                    isMTN = true;
                }
            }
            else if (tileX == mountainX)
            {
                // Mtn strategy 1
                if (tileY == mountainY - 1)
                {
                    if (selectedY > mountainY && (Mathf.Abs(tileX - selectedX) < Mathf.Abs(tileY - selectedY)))
                    {
                        isMTN = true;
                    }
                }
                else if (tileY == mountainY + 1)
                {
                    if (selectedY < mountainY && (Mathf.Abs(tileX - selectedX) < Mathf.Abs(tileY - selectedY)))
                    {
                        isMTN = true;
                    }
                }

                // Mtn strategy 2
                if (tileY == mountainY - 2 && selectedY > mountainY)
                {
                    if (selectedX == tileX)
                    {
                        isMTN = true;
                    }
                    else
                    {
                        if (Mathf.Abs(selectedX - mountainX) < 2 && Mathf.Abs(selectedY - mountainY) < 2)
                        {
                            isMTN = true;
                        }
                    }
                }
                else if (tileY == mountainY + 2 && selectedY < mountainY)
                {
                    if (selectedX == tileX)
                    {
                        isMTN = true;
                    }
                    else
                    {
                        if (Mathf.Abs(selectedX - mountainX) < 2 && Mathf.Abs(selectedY - mountainY) < 2)
                        {
                            isMTN = true;
                        }
                    }
                }

                // Same direction
                if (tileY < mountainY && selectedY > mountainY && selectedX == mountainX)
                {
                    isMTN = true;
                }
                else if (tileY > mountainY && selectedY < mountainY && selectedX == mountainX)
                {
                    isMTN = true;
                }
            }

            // --------------------- Diagonally same direction --------------------------
            if (selectedX < mountainX && tileX > mountainX)
            {
                if (selectedY < mountainY)
                {
                    if (selectedX - mountainX == selectedY - mountainY && tileX - mountainX == tileY - mountainY)
                    {
                        isMTN = true;
                    }
                }
                else if (selectedY > mountainY)
                {
                    if (selectedX - mountainX == mountainY - selectedY && tileX - mountainX == mountainY - tileY)
                    {
                        isMTN = true;
                    }
                }
            }
            else if (selectedX > mountainX && tileX < mountainX)
            {
                if (selectedY < mountainY)
                {
                    if (selectedX - mountainX == mountainY - selectedY && tileX - mountainX == mountainY - tileY)
                    {
                        isMTN = true;
                    }
                    else if (selectedX - mountainX == selectedY - mountainY && tileX - mountainX == tileY - mountainY)
                    {
                        isMTN = true;
                    }
                }
                else if (selectedY > mountainY)
                {
                    if (selectedX - mountainX == selectedY - mountainY && tileX - mountainX == tileY - mountainY)
                    {
                        isMTN = true;
                    }
                    else if (selectedX - mountainX == mountainY - selectedY && tileX - mountainX == mountainY - tileY)
                    {
                        isMTN = true;
                    }
                }
            }

            // Far away one space from mountain diagonally : selected units
            if (selectedX == mountainX - 1 && selectedY == mountainY - 1)
            {
                if (mountainX == tileX - 1 && mountainY == tileY || mountainX == tileX && mountainY == tileY - 1)
                {
                    isMTN = true;
                }

                if ((tileX - mountainX) >= 1 && (tileY - mountainY) >= 1)
                {
                    isMTN = true;
                }
            }
            else if (selectedX == mountainX - 1 && selectedY == mountainY + 1)
            {
                if (mountainX == tileX && mountainY == tileY + 1 || mountainX == tileX - 1 && mountainY == tileY)
                {
                    isMTN = true;
                }

                if ((tileX - mountainX) >= 1 && (mountainY - tileY) >= 1)
                {
                    isMTN = true;
                }
            }
            else if (selectedX == mountainX + 1 && selectedY == mountainY - 1)
            {
                if (mountainX == tileX + 1 && mountainY == tileY || mountainX == tileX && mountainY == tileY - 1)
                {
                    isMTN = true;
                }

                if ((mountainX - tileX) >= 1 && (tileY - mountainY) >= 1)
                {
                    isMTN = true;
                }
            }
            else if (selectedX == mountainX + 1 && selectedY == mountainY + 1)
            {
                if (mountainX == tileX + 1 && mountainY == tileY || mountainX == tileX && mountainY == tileY + 1)
                {
                    isMTN = true;
                }

                if ((mountainX - tileX) >= 1 && (mountainY - tileY) >= 1)
                {
                    isMTN = true;
                }
            }

            // Far away one space from mountain diagonally : opponent units
            if (tileX == mountainX - 1 && tileY == mountainY - 1)
            {
                if (mountainX == selectedX - 1 && mountainY == selectedY || mountainX == selectedX && mountainY == selectedY - 1)
                {
                    isMTN = true;
                }

                if ((selectedX - mountainX) >= 1 && (selectedY - mountainY) >= 1)
                {
                    isMTN = true;
                }
            }
            else if (tileX == mountainX - 1 && tileY == mountainY + 1)
            {
                if (mountainX == selectedX && mountainY == selectedY + 1 || mountainX == selectedX - 1 && mountainY == selectedY)
                {
                    isMTN = true;
                }

                if ((selectedX - mountainX) >= 1 && (mountainY - selectedY) >= 1)
                {
                    isMTN = true;
                }
            }
            else if (tileX == mountainX + 1 && tileY == mountainY - 1)
            {
                if (mountainX == selectedX + 1 && mountainY == selectedY || mountainX == selectedX && mountainY == selectedY - 1)
                {
                    isMTN = true;
                }

                if ((mountainX - selectedX) >= 1 && (selectedY - mountainY) >= 1)
                {
                    isMTN = true;
                }
            }
            else if (tileX == mountainX + 1 && tileY == mountainY + 1)
            {
                if (mountainX == selectedX + 1 && mountainY == selectedY || mountainX == selectedX && mountainY == selectedY + 1)
                {
                    isMTN = true;
                }

                if ((mountainX - selectedX) >= 1 && (mountainY - selectedY) >= 1)
                {
                    isMTN = true;
                }
            }
        }

        return isMTN;
    }
    
    [Header("---------- Game objects ----------")]
    public GameObject cameraView;
    public GameObject map;
    public GameObject volcano;
    public GameObject subPing;
    public GameObject ufo;
    public GameObject[] generalFaces;
    public GameObject[] bridges;
    public GameObject[] tiles;
    public GameObject[] force1ForCircle;
    public GameObject[] force1ForSquare;
    public GameObject[] force2ForCircle;
    public GameObject[] force2ForSquare;    
    public GameObject[] bermudaTriangles;
    public GameObject[] atomicMines;
    public GameObject[] target1ForCircle;
    public GameObject[] target1ForSquare;
    public GameObject[] target2ForCircle;
    public GameObject[] target2ForSquare;
    public GameObject[] players;
    public GameObject[] playerAvatars;
    public GameObject[] subBacks;
    public GameObject seaTile;
    public GameObject mineBTtile;
    public GameObject explosionTile;
    public GameObject[] explosionTileHuge;
    public List<GameObject> availableForce1;
    public List<GameObject> availableForce2;
    public List<GameObject> removeTilesForPathFind;
    public List<GameObject> totalAvailablePlaces;
    public List<GameObject> dedicatedAvailablePlaces;
    public List<GameObject> placedTiles;
    public List<GameObject> placesForBermuda;
    public List<GameObject> placesForMines;
    [Header("---------- Game Infos ----------")]
    public GameObject infoPanel;
    public GameObject infoWarning;
    public GameObject infoPanelForUFO;
    public GameObject infoGamePlayPanel;
    public GameObject infoSubAction;
    public GameObject infoBermudaAction;
    public GameObject infoAtomicMines;
    public GameObject infoBlackDragon;
    public GameObject infoBombs;    
    public GameObject turnDiceBottom;
    public GameObject turnDiceTop;
    public GameObject emojiBtn;
    public GameObject dice60;
    public GameObject dice61;
    public GameObject dice40;
    public GameObject dice41;
    public GameObject moveButton;
    public GameObject fireButton;
    public GameObject subButton;
    public GameObject sonarButton;
    public GameObject endTurnButton;
    public GameObject endPlayButton;
    public GameObject blackDragonButton;
    public GameObject pingButton;
    public GameObject subFireButton;
    public GameObject ufoFireButton;
    public GameObject ufoButton;
    public GameObject selectedUnit;
    public GameObject transportUnit;
    public GameObject targetObject;
    public GameObject convertedTile;
    public GameObject bootingObject;
    public GameObject startDialog;
    public GameObject ufoDialog;
    public GameObject blackDragonDialog;
    public GameObject winningDialog;
    public GameObject loseDialog;
    public GameObject videoPlayer;
    public GameObject[] videoPlayerBT;    
    public GameObject warningText;
    public GameObject reconnectingText;
    public GameObject mapshotBtn;
    public GameObject picturesBtn;
    public GameObject meleeRoundStartDialog;
    public GameObject operationSerpentDialog;
    public GameObject[] oSSubs;
    public Text bermudaText;
    public Text bermudaTextAfter;
    public Text playerNameText;
    public Text playsLeftText;
    public Text turnCountText;
    public Text currentPlayerNameText;
    public int currentTurnIndex;
    [Header("---------- Game sounds ----------")]
    public AudioClip[] generalAudios;
    public AudioClip[] airplaneAudios;
    public AudioClip[] subAudios;
    public AudioClip[] tankAudios;
    public AudioClip[] ufoAudios;
    public AudioClip[] transportAudios;
    public AudioSource diceAudio;
    public AudioSource soundEffect;        
    [Header("---------- Skins ----------")]
    public Sprite[] dice6Sprites;
    public Sprite[] dice4Sprites;
    public Sprite[] demoSkinsForLeft;
    public Sprite[] skins1ForLeft;
    public Sprite[] skins2ForLeft;
    public Sprite[] skins3ForLeft;
    public Sprite[] skins4ForLeft;
    public Sprite[] skins5ForLeft;
    public Sprite[] skins6ForLeft;
    public Sprite[] skins7ForLeft;
    public Sprite[] skins8ForLeft;
    public Sprite[] skins9ForLeft;
    public Sprite[] skins10ForLeft;
    public Sprite[] skins11ForLeft;
    public Sprite[] skins12ForLeft;
    public Sprite[] skins13ForLeft;
    public Sprite[] demoSkinsForRight;
    public Sprite[] skins1ForRight;
    public Sprite[] skins2ForRight;
    public Sprite[] skins3ForRight;
    public Sprite[] skins4ForRight;
    public Sprite[] skins5ForRight;
    public Sprite[] skins6ForRight;
    public Sprite[] skins7ForRight;
    public Sprite[] skins8ForRight;
    public Sprite[] skins9ForRight;
    public Sprite[] skins10ForRight;
    public Sprite[] skins11ForRight;
    public Sprite[] skins12ForRight;
    public Sprite[] skins13ForRight;
    public Sprite[] bridgeSprites;
    public Material outlineMat;
    public Material defaultMat;
    [Header("---------- Game Effects ----------")]
    public ParticleSystem firePrefab;
    public ParticleSystem volcSmokePrefab;
    public ParticleSystem volcFirePrefab;
    ParticleSystem fireParticle;
    ParticleSystem volcFireParticle;
    ParticleSystem volcSmokeParticle;
    Color moveColor;
    Color fireColor;
    Color sonarColor;
    Color tunnelColor;    
    private Vector3 ufoTarget = new(2.65f, -0.75f, 0f);
    private Vector3 ufoInitialPos = new(60f, 60f, 0f);
    [Header("---------- Game parameters ----------")]
    public List<int> placedTilesIndex;
    public List<int> minesIndicesInfo;
    public List<int> tileIndicesInfo;
    public List<int> startTiles;
    public List<int> bermudaTiles;
    public List<int> bermudaTilesCount;
    public int playersInRoom;
    public int expandExplosionIndex;
    public int chainExplosionIndex;
    public int[] bermudaInfos;
    public int[] minesInfos;
    public int remainingExplosion;
    public int blackDragonTime;
    public int loadedBombs;
    public int videoIndexBT;    
    public int whoseTurn;
    public int turnCount;
    public int nextCount;
    public int selectedX;
    public int selectedY;
    public int meleeRoundCount;
    public int subCount1;
    public int subCount2;
    int randomIndex;
    int indices;
    int previousIndex = 0;
    int previousSonarIndex = 0;
    int moveRange;
    int fireRange;
    int sonarRange;
    int subRange;
    int ufoDiceIndex1;
    int ufoDiceIndex2;
    int ufoActionIndex;
    int exceedCount = 0;   
    [Header("---------- bool ----------")]
    public bool isSmartMapEnabled = false;
    public bool isMasterInGame;
    public bool isFinished;
    public bool isMove;
    public bool isMoveAnimation;
    public bool isUFOMove;
    public bool isFire;
    public bool isEnemy;
    public bool isEmpty;
    public bool isSub;
    public bool isSonar;
    public bool isPing;
    public bool isAvailableAttack;
    public bool isUFO;
    public bool isUFOAction;
    public bool isSelected;
    public bool isSelectedSync;
    public bool isOnlyOncePlay;
    public bool isShowWinning;
    public bool isGamePlay;
    public bool isForceForMove;
    public bool isForceForFire;
    public bool isForceForSonar;
    public bool isForceForSub;
    public bool isUFOMoving;
    public bool isDiceRolling;
    public bool isChangingTurn;
    public bool isActivatedBermuda;
    public bool isActivatedBermudaBT;
    public bool isProgressAction;
    public bool isIncreasedTimes;
    public bool isIncreasedByBermuda;
    public bool isIncreasedByMines;
    public bool isEndedByMines;
    public bool isExplodedMines;
    public bool isClickExit;
    public bool isBlackDragon;
    public bool isBombLoading;
    public bool isWarning;
    private bool isDisableMouseClicks = false;
    public bool isOutlined;
    public bool isMeleeRound;



    public List<Transform> playerslist;
    //public bool isGameVisible;

    public void PlaceUnits()
    {
        totalAvailablePlaces = new List<GameObject>();
        placedTiles = new List<GameObject>();

        for (int i = 0; i < tiles.Length; i++)
        {
            totalAvailablePlaces.Add(tiles[i]);
        }

        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            dedicatedAvailablePlaces = new List<GameObject>();

            if (force1ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.General)
            {
                for (int j = 0; j < totalAvailablePlaces.Count; j++)
                {
                    if (totalAvailablePlaces[j].GetComponent<CellInfo>().isLeft && totalAvailablePlaces[j].GetComponent<CellInfo>().cellType == (int)CellType.Land)
                    {
                        dedicatedAvailablePlaces.Add(totalAvailablePlaces[j]);
                    }
                }
            }
            else if (force1ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Plane)
            {
                for (int j = 0; j < totalAvailablePlaces.Count; j++)
                {
                    if (totalAvailablePlaces[j].GetComponent<CellInfo>().isLeft)
                    {
                        dedicatedAvailablePlaces.Add(totalAvailablePlaces[j]);
                    }
                }
            }
            else if (force1ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Tank)
            {
                for (int j = 0; j < totalAvailablePlaces.Count; j++)
                {
                    if (totalAvailablePlaces[j].GetComponent<CellInfo>().isLeft && totalAvailablePlaces[j].GetComponent<CellInfo>().cellType == (int)CellType.Land)
                    {
                        dedicatedAvailablePlaces.Add(totalAvailablePlaces[j]);
                    }
                }
            }
            else if (force1ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                for (int j = 0; j < totalAvailablePlaces.Count; j++)
                {
                    if (totalAvailablePlaces[j].GetComponent<CellInfo>().isLeft && totalAvailablePlaces[j].GetComponent<CellInfo>().cellType == (int)CellType.Sea)
                    {
                        dedicatedAvailablePlaces.Add(totalAvailablePlaces[j]);
                    }
                }
            }

            randomIndex = UnityEngine.Random.Range(0, dedicatedAvailablePlaces.Count);
            placedTiles.Add(dedicatedAvailablePlaces[randomIndex]);
            totalAvailablePlaces.Remove(dedicatedAvailablePlaces[randomIndex]);
        }

        for (int i = 0; i < force1ForSquare.Length; i++)
        {
            dedicatedAvailablePlaces = new List<GameObject>();

            if (force1ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.General)
            {
                for (int j = 0; j < totalAvailablePlaces.Count; j++)
                {
                    if (totalAvailablePlaces[j].GetComponent<CellInfo>().isLeft && totalAvailablePlaces[j].GetComponent<CellInfo>().cellType == (int)CellType.Land)
                    {
                        dedicatedAvailablePlaces.Add(totalAvailablePlaces[j]);
                    }
                }
            }
            else if (force1ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Plane)
            {
                for (int j = 0; j < totalAvailablePlaces.Count; j++)
                {
                    if (totalAvailablePlaces[j].GetComponent<CellInfo>().isLeft)
                    {
                        dedicatedAvailablePlaces.Add(totalAvailablePlaces[j]);
                    }
                }
            }
            else if (force1ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Tank)
            {
                for (int j = 0; j < totalAvailablePlaces.Count; j++)
                {
                    if (totalAvailablePlaces[j].GetComponent<CellInfo>().isLeft && totalAvailablePlaces[j].GetComponent<CellInfo>().cellType == (int)CellType.Land)
                    {
                        dedicatedAvailablePlaces.Add(totalAvailablePlaces[j]);
                    }
                }
            }
            else if (force1ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                for (int j = 0; j < totalAvailablePlaces.Count; j++)
                {
                    if (totalAvailablePlaces[j].GetComponent<CellInfo>().isLeft && totalAvailablePlaces[j].GetComponent<CellInfo>().cellType == (int)CellType.Sea)
                    {
                        dedicatedAvailablePlaces.Add(totalAvailablePlaces[j]);
                    }
                }
            }

            randomIndex = UnityEngine.Random.Range(0, dedicatedAvailablePlaces.Count);
            placedTiles.Add(dedicatedAvailablePlaces[randomIndex]);
            totalAvailablePlaces.Remove(dedicatedAvailablePlaces[randomIndex]);
        }

        for (int i = 0; i < force2ForSquare.Length; i++)
        {
            dedicatedAvailablePlaces = new List<GameObject>();

            if (force2ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.General)
            {
                for (int j = 0; j < totalAvailablePlaces.Count; j++)
                {
                    if (!totalAvailablePlaces[j].GetComponent<CellInfo>().isLeft && totalAvailablePlaces[j].GetComponent<CellInfo>().cellType == (int)CellType.Land)
                    {
                        dedicatedAvailablePlaces.Add(totalAvailablePlaces[j]);
                    }
                }
            }
            else if (force2ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Plane)
            {
                for (int j = 0; j < totalAvailablePlaces.Count; j++)
                {
                    if (!totalAvailablePlaces[j].GetComponent<CellInfo>().isLeft)
                    {
                        dedicatedAvailablePlaces.Add(totalAvailablePlaces[j]);
                    }
                }
            }
            else if (force2ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Tank)
            {
                for (int j = 0; j < totalAvailablePlaces.Count; j++)
                {
                    if (!totalAvailablePlaces[j].GetComponent<CellInfo>().isLeft && totalAvailablePlaces[j].GetComponent<CellInfo>().cellType == (int)CellType.Land)
                    {
                        dedicatedAvailablePlaces.Add(totalAvailablePlaces[j]);
                    }
                }
            }
            else if (force2ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                for (int j = 0; j < totalAvailablePlaces.Count; j++)
                {
                    if (!totalAvailablePlaces[j].GetComponent<CellInfo>().isLeft && totalAvailablePlaces[j].GetComponent<CellInfo>().cellType == (int)CellType.Sea)
                    {
                        dedicatedAvailablePlaces.Add(totalAvailablePlaces[j]);
                    }
                }
            }

            randomIndex = UnityEngine.Random.Range(0, dedicatedAvailablePlaces.Count);
            placedTiles.Add(dedicatedAvailablePlaces[randomIndex]);
            totalAvailablePlaces.Remove(dedicatedAvailablePlaces[randomIndex]);
        }

        for (int i = 0; i < force2ForCircle.Length; i++)
        {
            dedicatedAvailablePlaces = new List<GameObject>();

            if (force2ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.General)
            {
                for (int j = 0; j < totalAvailablePlaces.Count; j++)
                {
                    if (!totalAvailablePlaces[j].GetComponent<CellInfo>().isLeft && totalAvailablePlaces[j].GetComponent<CellInfo>().cellType == (int)CellType.Land)
                    {
                        dedicatedAvailablePlaces.Add(totalAvailablePlaces[j]);
                    }
                }
            }
            else if (force2ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Plane)
            {
                for (int j = 0; j < totalAvailablePlaces.Count; j++)
                {
                    if (!totalAvailablePlaces[j].GetComponent<CellInfo>().isLeft)
                    {
                        dedicatedAvailablePlaces.Add(totalAvailablePlaces[j]);
                    }
                }
            }
            else if (force2ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Tank)
            {
                for (int j = 0; j < totalAvailablePlaces.Count; j++)
                {
                    if (!totalAvailablePlaces[j].GetComponent<CellInfo>().isLeft && totalAvailablePlaces[j].GetComponent<CellInfo>().cellType == (int)CellType.Land)
                    {
                        dedicatedAvailablePlaces.Add(totalAvailablePlaces[j]);
                    }
                }
            }
            else if (force2ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                for (int j = 0; j < totalAvailablePlaces.Count; j++)
                {
                    if (!totalAvailablePlaces[j].GetComponent<CellInfo>().isLeft && totalAvailablePlaces[j].GetComponent<CellInfo>().cellType == (int)CellType.Sea)
                    {
                        dedicatedAvailablePlaces.Add(totalAvailablePlaces[j]);
                    }
                }
            }

            randomIndex = UnityEngine.Random.Range(0, dedicatedAvailablePlaces.Count);
            placedTiles.Add(dedicatedAvailablePlaces[randomIndex]);
            totalAvailablePlaces.Remove(dedicatedAvailablePlaces[randomIndex]);
        }

        for (int i = 0; i < placedTiles.Count; i++)
        {
            SendPlacedTileIndex(placedTiles[i]);
        }
    }

    public void SendPlacedTileIndex(GameObject cell)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (Vector3.Distance(cell.transform.position, tiles[i].transform.position) < 0.1f)
            {
                SyncPlacement(new object[] { i });
            }
        }
    }

    public void PlaceTriangles()
    {
        bool isAvailable = true;

        for (int i = 0; i < tiles.Length; i++)
        {
            isAvailable = true;

            for (int j = 0; j < force1ForCircle.Length; j++)
            {
                if (Vector3.Distance(force1ForCircle[j].transform.position, tiles[i].transform.position) < 0.1f ||
                    Vector3.Distance(force1ForSquare[j].transform.position, tiles[i].transform.position) < 0.1f ||
                    Vector3.Distance(force2ForCircle[j].transform.position, tiles[i].transform.position) < 0.1f ||
                    Vector3.Distance(force2ForSquare[j].transform.position, tiles[i].transform.position) < 0.1f)
                {
                    isAvailable = false;
                    break;
                }
            }

            if (isAvailable)
            {
                if (tiles[i].GetComponent<CellInfo>().cellType == (int)CellType.Land || tiles[i].GetComponent<CellInfo>().cellType == (int)CellType.Sea)
                {
                    placesForBermuda.Add(tiles[i]);
                }
            }
        }

        for (int i = 0; i < bermudaTriangles.Length; i++)
        {
            int bermudaIndex = UnityEngine.Random.Range(0, placesForBermuda.Count);

            SyncTriangles(new object[] { i, placesForBermuda[bermudaIndex].transform.position });

            placesForBermuda.RemoveAt(bermudaIndex);
        }
    }

    public void PlaceMines()
    {
        bool isAvailable = true;

        for (int i = 0; i < tiles.Length; i++)
        {
            int indexX = i % 30;
            int indexY = i / 30;

            if (indexX >= 3 && indexX <= 26 && indexY >= 3 && indexY <= 26)
            {
                isAvailable = true;

                for (int j = 0; j < force1ForCircle.Length; j++)
                {
                    if (Vector3.Distance(force1ForCircle[j].transform.position, tiles[i].transform.position) < 0.1f ||
                        Vector3.Distance(force1ForSquare[j].transform.position, tiles[i].transform.position) < 0.1f ||
                        Vector3.Distance(force2ForCircle[j].transform.position, tiles[i].transform.position) < 0.1f ||
                        Vector3.Distance(force2ForSquare[j].transform.position, tiles[i].transform.position) < 0.1f)
                    {
                        isAvailable = false;
                        break;
                    }
                }

                for (int j = 0; j < bermudaTriangles.Length; j++)
                {
                    if (Vector3.Distance(bermudaTriangles[j].transform.position, tiles[i].transform.position) < 0.1f)
                    {
                        isAvailable = false;
                        break;
                    }
                }

                if (isAvailable)
                {
                    if (tiles[i].GetComponent<CellInfo>().cellType == (int)CellType.Land || tiles[i].GetComponent<CellInfo>().cellType == (int)CellType.Sea)
                    {
                        placesForMines.Add(tiles[i]);
                    }
                }
            }
        }

        //for (int i = 7; i < 21; i++)
        //{
        //    placesForMines.Add(tiles[i * 30 + 7]);
        //    placesForMines.Add(tiles[i * 30 + 8]);
        //    placesForMines.Add(tiles[i * 30 + 9]);
        //    placesForMines.Add(tiles[i * 30 + 10]);
        //}

        for (int i = 0; i < atomicMines.Length; i++)
        {
            int minesIndex = UnityEngine.Random.Range(0, placesForMines.Count);

            SyncMines(new object[] { i, placesForMines[minesIndex].transform.position });

            placesForMines.RemoveAt(minesIndex);
        }
    }

    public void TransportViaBermuda(int legion, int force, int bermuda)
    {
        List<GameObject> availableTiles;
        availableTiles = new List<GameObject>();

        // Find available tiles for transporting
        for (int i = 0; i < tiles.Length; i++)
        {
            bool isAvailableToTransport = true;

            for (int j = 0; j < bermudaTriangles.Length; j++)
            {
                if (Vector3.Distance(tiles[i].transform.position, bermudaTriangles[j].transform.position) < 0.1f)
                {
                    isAvailableToTransport = false;
                }
            }

            for (int j = 0; j < force1ForCircle.Length; j++)
            {
                if (Vector3.Distance(tiles[i].transform.position, force1ForCircle[j].transform.position) < 0.1f ||
                    Vector3.Distance(tiles[i].transform.position, force1ForSquare[j].transform.position) < 0.1f ||
                    Vector3.Distance(tiles[i].transform.position, force2ForCircle[j].transform.position) < 0.1f ||
                    Vector3.Distance(tiles[i].transform.position, force2ForSquare[j].transform.position) < 0.1f)
                {
                    isAvailableToTransport = false;
                }
            }

            if (isAvailableToTransport)
            {
                availableTiles.Add(tiles[i]);
            }
        }

        int moveIndex = UnityEngine.Random.Range(0, availableTiles.Count);

        for (int i = 0; i < tiles.Length; i++)
        {
            if (Vector3.Distance(tiles[i].transform.position, availableTiles[moveIndex].transform.position) < 0.1f)
            {
                SyncBermuda(new object[] { legion, force, bermuda, i });
                break;
            }
        }
    }

    public void ExplosionEffect(int tileIndexInfo, int minesIndexInfo)
    {
        chainExplosionIndex++;
        expandExplosionIndex = CalculateExpandIndex(chainExplosionIndex);

        if (isActivatedBermudaBT)
        {
            GameObject cloneExplosion = Instantiate(mineBTtile, tiles[tileIndexInfo].transform.position, Quaternion.identity);
            cloneExplosion.GetComponent<SpriteRenderer>().sortingOrder = 4;
            Destroy(cloneExplosion, 3f);
        }
        else
        {            
            GameObject cloneExplosion = Instantiate(explosionTile, tiles[tileIndexInfo].transform.position, Quaternion.identity);
            cloneExplosion.GetComponent<SpriteRenderer>().sortingOrder = 4;
            Destroy(cloneExplosion, 3f);
        }

        StartCoroutine(DelayToShowInfoAtomicMines(tileIndexInfo));        
    }

    public void ExplodeAction(int startTile)
    {
        List<int> tileIndices = new List<int>();        

        for (int i = 0; i < 3 + expandExplosionIndex * 2; i++)
        {
            for (int j = 0; j < 3 + expandExplosionIndex * 2; j++)
            {
                tileIndices.Add(startTile + i + 30 * j);
            }
        }

        // Explode units around mines
        for (int j = 0; j < tileIndices.Count; j++)
        {
            for (int i = 0; i < force1ForCircle.Length; i++)
            {
                if (Vector3.Distance(force1ForCircle[i].transform.position, tiles[tileIndices[j]].transform.position) < 0.1f)
                {
                    // Do Operation Serpent
                    if (force1ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
                    {
                        OperationSerpent(1);
                    }

                    force1ForCircle[i].transform.position = new Vector3(10000f, 10000f, -8f);
                    force1ForCircle[i].SetActive(false);              
                }

                if (Vector3.Distance(force1ForSquare[i].transform.position, tiles[tileIndices[j]].transform.position) < 0.1f)
                {
                    // Do Operation Serpent
                    if (force1ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
                    {
                        OperationSerpent(1);
                    }

                    force1ForSquare[i].transform.position = new Vector3(10000f, 10000f, -8f);
                    force1ForSquare[i].SetActive(false);                 
                }

                if (Vector3.Distance(force2ForCircle[i].transform.position, tiles[tileIndices[j]].transform.position) < 0.1f)
                {
                    // Do Operation Serpent
                    if (force2ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
                    {
                        OperationSerpent(2);
                    }

                    force2ForCircle[i].transform.position = new Vector3(10000f, 10000f, -8f);
                    force2ForCircle[i].SetActive(false);            
                }

                if (Vector3.Distance(force2ForSquare[i].transform.position, tiles[tileIndices[j]].transform.position) < 0.1f)
                {
                    // Do Operation Serpent
                    if (force2ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
                    {
                        OperationSerpent(2);
                    }

                    force2ForSquare[i].transform.position = new Vector3(10000f, 10000f, -8f);
                    force2ForSquare[i].SetActive(false);   
                }
            }
        }

        if (!isActivatedBermudaBT)
        {
            atomicMines[minesIndicesInfo[minesIndicesInfo.Count - remainingExplosion - 1]].transform.position = new Vector3(20000f, 20000f, 0);
        }        

        // bermuda triangles around mines
        for (int i = 0; i < tileIndices.Count; i++)
        {
            for (int j = 0; j < bermudaTriangles.Length; j++)
            {
                if (Vector3.Distance(tiles[tileIndices[i]].transform.position, bermudaTriangles[j].transform.position) < 0.1f)
                {
                    if (!bermudaTiles.Contains(j))
                    {
                        bermudaTiles.Add(j);
                        bermudaTilesCount.Add(0);
                    }

                    for (int k = 0; k < bermudaTiles.Count; k++)
                    {
                        if (bermudaTiles[k] == j)
                        {
                            bermudaTilesCount[k]++;
                            break;
                        }
                    }
                }
            }
        }        

        isActivatedBermudaBT = false;

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].GetComponent<SpriteRenderer>().enabled = false;
        }

        // Kill other units
        for (int i = 0; i < 4; i++)
        {
            if (GeneralNumbers(i) == 0)
            {
                SyncKillGeneralEnimies(new object[] { i });
            }
        }

        if (GeneralNumbers(0) == 0 && GeneralNumbers(1) == 0)
        {
            whoseTurn = 1;
            isFinished = true;
            StartCoroutine(ShowWinningDialog(1));            
        }
        else if (GeneralNumbers(2) == 0 && GeneralNumbers(3) == 0)
        {
            whoseTurn = 0;
            isFinished = true;
            StartCoroutine(ShowWinningDialog(1));
        }               
    }
    int myval;
    public void MakeGroundZero()
    {
        // make ground zero
        for (int i = 0; i < 3 + expandExplosionIndex * 2; i++)
        {
            for (int j = 0; j < 3 + expandExplosionIndex * 2; j++)
            {
                tiles[startTiles[minesIndicesInfo.Count - remainingExplosion - 1] + i + 30 * j].GetComponent<CellInfo>().cellType = (int)CellType.Sea;
                GameObject cloneSea = Instantiate(seaTile, tiles[startTiles[minesIndicesInfo.Count - remainingExplosion - 1] + i + 30 * j].transform.position, Quaternion.identity);
                cloneSea.GetComponent<SpriteRenderer>().sortingOrder = 0;
            }
        }

        // Check bermuda activation
        if (!(GeneralNumbers(0) == 0 && GeneralNumbers(1) == 0 ||
            GeneralNumbers(2) == 0 && GeneralNumbers(3) == 0))
        {
            if (remainingExplosion != 0)
            {
                ExplosionEffect(tileIndicesInfo[minesIndicesInfo.Count - remainingExplosion], minesIndicesInfo[minesIndicesInfo.Count - remainingExplosion - 1]);
            }
            else
            {
                if (bermudaTiles.Count > 0)
                {
                    StartCoroutine(DelaytoShowBermudaActivation());
                }
                else
                {
                    isProgressAction = false;

                    if (whoseTurn == (int)TurnIndex.Player1 && players[0].GetComponent<PlayerInfo>().playTimes == 0 ||
                        whoseTurn == (int)TurnIndex.Player2 && players[1].GetComponent<PlayerInfo>().playTimes == 0)
                    {
                        if (isIncreasedByMines || isEndedByMines)
                        {
                            StartCoroutine(DelayTurn());
                        }
                    }

                    if (bootingObject.GetComponent<Image>().fillAmount <= 0f && isShowWinning == false)
                    {
                        if ((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame))
                        {
                            if (isEndedByMines || isIncreasedByMines)
                            {
                                isShowWinning = true;
                                EndTurn();
                            }
                        }
                    }

                    isIncreasedByMines = false;
                    isEndedByMines = false;
                }
            }
        }
    }

    #region MonoBehaviourCallBacks
    //private void Awake()
    //{
    //    Application.runInBackground = true;
    //    QualitySettings.vSyncCount = 0;
    //    Application.targetFrameRate = 30;        
    //}
    private void OnDestroy()
    {
        if (subAvailability.Instance != null)
            subAvailability.onUpdateSub -= setSubNow;

    }
    void Start()
    {
        InteractiveTutorial.TutorialActive = false;
        // Subscribe to the application focus events
        //Application.focusChanged += OnApplicationFocusChange;
        if (subAvailability.Instance != null)
        {
            subAvailability.onUpdateSub += setSubNow;
        }
        PlayerPrefs.SetString("CREATOR", PhotonNetwork.CurrentRoom.Name);  
        startDialog.SetActive(true);
        playersInRoom = 2;
        placedTilesIndex = new List<int>();
        bermudaTiles = new List<int>();
        bermudaTilesCount = new List<int>();
        isGamePlay = true;
        isOutlined = true;
        isFinished = false;
        isMasterInGame = false;
        isActivatedBermuda = false;
        isActivatedBermudaBT = false;
        isIncreasedTimes = false;
        isIncreasedByMines = false;
        isIncreasedByBermuda = false;
        isEndedByMines = false;
        isClickExit = false;
        isExplodedMines = false;
        isBombLoading = false;
        isWarning = false;
        isProgressAction = false;
        isChangingTurn = false;
        isSelectedSync = false; 
        expandExplosionIndex = 0;
        chainExplosionIndex = 0;
        blackDragonTime = 1;
        loadedBombs = 0;
        videoIndexBT = 0;
        subCount1 = 0;
        subCount2 = 0;

        isMeleeRound = true;
        meleeRoundCount = 0;

        turnDiceBottom.SetActive(false);
        turnDiceTop.SetActive(false);

        smartMapText.color = isSmartMapEnabled ? smartMapEnabledColor : smartMapDisabledColor;
        ufo.transform.position = new Vector3(ufoInitialPos.x * UnityEngine.Random.Range(0, 2) == 0 ? 1f : -1f, ufoInitialPos.y * UnityEngine.Random.Range(0, 2) == 0 ? 1f : -1f, ufoInitialPos.z);
        ufo.SetActive(false);

        for (int i = 0; i < MainUI.gameOptions.Length; i++)
        {
            if (MainUI.gameOptions[i] == '&')
            {
                mapshotBtn.SetActive(true);
                //picturesBtn.SetActive(true);
            }
        }        

        if (PhotonNetwork.IsMasterClient)
        {
         
            for (int i = 0; i < 8; i++)
            {
                force1ForCircle[i].GetComponent<SpriteRenderer>().material = outlineMat;
                force1ForSquare[i].GetComponent<SpriteRenderer>().material = outlineMat;
            }

            isMasterInGame = true;
            //StartCoroutine(PostPlayingRoom(MainUI.frontURL + "/api/startPlaying"));

            playerNameText.text = PhotonNetwork.NickName;
            SyncTurnMap(new object[] { UnityEngine.Random.Range(0, 4) });
            placesForBermuda = new List<GameObject>();
            placesForMines = new List<GameObject>();

            PlaceUnits();

            StartCoroutine(DelayToPlaceSpecialThings());

            SyncSkin(new object[] { MainUI.skinIndex1, MainUI.skinIndex2, MainUI.skinIndex3, MainUI.skinIndex4 });

            int a = (System.DateTime.Now.Millisecond + UnityEngine.Random.Range(6, 100)) % 10;
            int b = (System.DateTime.Now.Millisecond + UnityEngine.Random.Range(5, 100)) % 10;

            if (PlayerPrefs.GetInt("Previous_A") >= PlayerPrefs.GetInt("Previous_B") && a >= b ||
                PlayerPrefs.GetInt("Previous_A") < PlayerPrefs.GetInt("Previous_B") && a < b)
            {
                a = (a + 5) % 10;
            }

            PlayerPrefs.SetInt("Previous_A", a);
            PlayerPrefs.SetInt("Previous_B", b);

            a += 2;
            b += 2;

            if (a == b)
            {
                if (UnityEngine.Random.Range(0, 2) == 0)
                {
                    if (a >= 6)
                    {
                        a -= 1;
                    }
                    else
                    {
                        a += 1;
                    }
                }
                else
                {
                    if (b >= 6)
                    {
                        b -= 1;
                    }
                    else
                    {
                        b += 1;
                    }
                }
            }

            int a1 = UnityEngine.Random.Range(1, Math.Min(a - 1, 6) + 1);
            int a2 = a - a1;
            if (a1 > 6)
            {
                a1 = 6;
                a2 = a - a1;
            }
            else if (a2 > 6)
            {
                a2 = 6;
                a1 = a - a2;
            }

            int b1 = UnityEngine.Random.Range(1, Math.Min(b - 1, 6) + 1);
            int b2 = b - b1;
            if (b1 > 6)
            {
                b1 = 6;
                b2 = b - b1;
            }
            else if (b2 > 6)
            {
                b2 = 6;
                b1 = b - b2;
            }

            SyncFirstRoll(new object[] { a1, a2, b1, b2 });
            cameraView.transform.position = new Vector3(force1ForCircle[0].transform.position.x, force1ForCircle[0].transform.position.y, cameraView.transform.position.z);
        }
        else
        {
            playerNameText.text = PhotonNetwork.NickName;
            cameraView.transform.position = new Vector3(force2ForCircle[0].transform.position.x, force2ForCircle[0].transform.position.y, cameraView.transform.position.z);

            for (int i = 0; i < 8; i++)
            {
                force2ForCircle[i].GetComponent<SpriteRenderer>().material = outlineMat;
                force2ForSquare[i].GetComponent<SpriteRenderer>().material = outlineMat;
            }
        }
    }
 
    void Update()
    {
        gameUI.transform.localScale = new Vector3(Screen.width / 1366f, Screen.height / 768f, 1);

        if (isGamePlay)
        {
            if (bootingObject.GetComponent<Image>().fillAmount <= 0.1275 &&
                bootingObject.GetComponent<Image>().fillAmount >= 0.125)
            {
                isWarning = true;
            }

            if (isWarning)
            {
                isWarning = false;
                StartCoroutine(DelayShowInfoWarning());
            }

            if (isDisableMouseClicks)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                {
                    return;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                // Creates a Ray from the mouse position
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit) && isMoveAnimation == false)
                {
                    //print(hit.transform.gameObject.tag);

                    if (hit.transform.gameObject.tag == "Force" && isFire == true && !isOnlyOncePlay && IsEnemy(hit.transform.gameObject) && IsAvailableAttack(hit.transform.gameObject))
                    {
                        if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
                        {
                            for (int i = 0; i < force1ForCircle.Length; i++)
                            {
                                if (hit.transform.gameObject == force1ForCircle[i])
                                {
                                    isOnlyOncePlay = true;
                                    SyncAttack(new object[] { i, 0 });
                                }
                                else if (hit.transform.gameObject == force1ForSquare[i])
                                {
                                    isOnlyOncePlay = true;
                                    SyncAttack(new object[] { i, 1 });
                                }
                                else if (hit.transform.gameObject == force2ForCircle[i])
                                {
                                    isOnlyOncePlay = true;
                                    SyncAttack(new object[] { i, 2 });
                                }
                                else if (hit.transform.gameObject == force2ForSquare[i])
                                {
                                    isOnlyOncePlay = true;
                                    SyncAttack(new object[] { i, 3 });
                                }
                            }
                        }
                    }
                    else if (hit.transform.gameObject.tag == "Force" && hit.transform.gameObject.GetComponent<Animator>().enabled == true && !isMove && !isFire && !isSub && !isSonar && isSelected == false && !isUFOMoving && !isBombLoading && !isProgressAction)
                    {
                        if (hit.transform.gameObject.GetComponent<ForceInfo>().teamIndex == whoseTurn && ((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
                        {
                            if (selectedUnit != hit.transform.gameObject)
                            {
                                isSelectedSync = false;
                            }

                            if (!isSelectedSync)
                            {
                                for (int i = 0; i < force1ForCircle.Length; i++)
                                {
                                    if (hit.transform.gameObject == force1ForCircle[i])
                                    {
                                        isSelectedSync = true;
                                        SyncSelectedUnit(new object[] { i, 0 });
                                    }
                                    else if (hit.transform.gameObject == force1ForSquare[i])
                                    {
                                        isSelectedSync = true;
                                        SyncSelectedUnit(new object[] { i, 1 });
                                    }
                                    else if (hit.transform.gameObject == force2ForCircle[i])
                                    {
                                        isSelectedSync = true;
                                        SyncSelectedUnit(new object[] { i, 2 });
                                    }
                                    else if (hit.transform.gameObject == force2ForSquare[i])
                                    {
                                        isSelectedSync = true;
                                        SyncSelectedUnit(new object[] { i, 3 });
                                    }
                                }
                            }
                        }
                    }
                    else if (hit.transform.gameObject.tag == "Force" && hit.transform.gameObject.GetComponent<Animator>().enabled == true && isSelected == true)
                    {
                        if (hit.transform.gameObject.GetComponent<ForceInfo>().teamIndex == whoseTurn && ((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
                        {
                            if (isMove)
                            {
                                if (selectedUnit.GetComponent<ForceInfo>().movementRange == hit.transform.gameObject.GetComponent<ForceInfo>().movementRange)
                                {
                                    for (int i = 0; i < force1ForCircle.Length; i++)
                                    {
                                        if (hit.transform.gameObject == force1ForCircle[i])
                                        {
                                            SyncDoubleSelectedUnit(new object[] { i, 0 });
                                        }
                                        else if (hit.transform.gameObject == force1ForSquare[i])
                                        {
                                            SyncDoubleSelectedUnit(new object[] { i, 1 });
                                        }
                                        else if (hit.transform.gameObject == force2ForCircle[i])
                                        {
                                            SyncDoubleSelectedUnit(new object[] { i, 2 });
                                        }
                                        else if (hit.transform.gameObject == force2ForSquare[i])
                                        {
                                            SyncDoubleSelectedUnit(new object[] { i, 3 });
                                        }
                                    }
                                }
                            }
                            else if (isFire || isSub)
                            {
                                if (selectedUnit.GetComponent<ForceInfo>().fireRange == hit.transform.gameObject.GetComponent<ForceInfo>().fireRange)
                                {
                                    for (int i = 0; i < force1ForCircle.Length; i++)
                                    {
                                        if (hit.transform.gameObject == force1ForCircle[i])
                                        {
                                            SyncDoubleSelectedUnit(new object[] { i, 0 });
                                        }
                                        else if (hit.transform.gameObject == force1ForSquare[i])
                                        {
                                            SyncDoubleSelectedUnit(new object[] { i, 1 });
                                        }
                                        else if (hit.transform.gameObject == force2ForCircle[i])
                                        {
                                            SyncDoubleSelectedUnit(new object[] { i, 2 });
                                        }
                                        else if (hit.transform.gameObject == force2ForSquare[i])
                                        {
                                            SyncDoubleSelectedUnit(new object[] { i, 3 });
                                        }
                                    }
                                }
                            }
                            else if (isSonar)
                            {
                                if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && hit.transform.gameObject.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
                                {
                                    for (int i = 0; i < force1ForCircle.Length; i++)
                                    {
                                        if (hit.transform.gameObject == force1ForCircle[i])
                                        {
                                            SyncDoubleSelectedUnit(new object[] { i, 0 });
                                        }
                                        else if (hit.transform.gameObject == force1ForSquare[i])
                                        {
                                            SyncDoubleSelectedUnit(new object[] { i, 1 });
                                        }
                                        else if (hit.transform.gameObject == force2ForCircle[i])
                                        {
                                            SyncDoubleSelectedUnit(new object[] { i, 2 });
                                        }
                                        else if (hit.transform.gameObject == force2ForSquare[i])
                                        {
                                            SyncDoubleSelectedUnit(new object[] { i, 3 });
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (isUFOAction == true && hit.transform.gameObject.tag == "Force")
                    {
                        if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
                        {
                            for (int i = 0; i < force1ForCircle.Length; i++)
                            {
                                if (hit.transform.gameObject.GetComponent<ForceInfo>().forceType == (int)ForceType.Plane && ConvertToTile(hit.transform.gameObject).GetComponent<CellInfo>().cellType == (int)CellType.Mountain)
                                {
                                    if (hit.transform.gameObject == force1ForCircle[i] &&
                                        (whoseTurn == (int)TurnIndex.Player1 && force1ForCircle[i].GetComponent<ForceInfo>().teamIndex == 1 ||
                                         whoseTurn == (int)TurnIndex.Player2 && force1ForCircle[i].GetComponent<ForceInfo>().teamIndex == 0))
                                    {
                                        SyncUFOAction(new object[] { i, 0 });
                                    }
                                    else if (hit.transform.gameObject == force1ForSquare[i] &&
                                        (whoseTurn == (int)TurnIndex.Player1 && force1ForSquare[i].GetComponent<ForceInfo>().teamIndex == 1 ||
                                         whoseTurn == (int)TurnIndex.Player2 && force1ForSquare[i].GetComponent<ForceInfo>().teamIndex == 0))
                                    {
                                        SyncUFOAction(new object[] { i, 1 });
                                    }
                                    else if (hit.transform.gameObject == force2ForCircle[i] &&
                                        (whoseTurn == (int)TurnIndex.Player1 && force2ForCircle[i].GetComponent<ForceInfo>().teamIndex == 1 ||
                                         whoseTurn == (int)TurnIndex.Player2 && force2ForCircle[i].GetComponent<ForceInfo>().teamIndex == 0))
                                    {
                                        SyncUFOAction(new object[] { i, 2 });
                                    }
                                    else if (hit.transform.gameObject == force2ForSquare[i] &&
                                        (whoseTurn == (int)TurnIndex.Player1 && force2ForSquare[i].GetComponent<ForceInfo>().teamIndex == 1 ||
                                         whoseTurn == (int)TurnIndex.Player2 && force2ForSquare[i].GetComponent<ForceInfo>().teamIndex == 0))
                                    {
                                        SyncUFOAction(new object[] { i, 3 });
                                    }
                                }
                            }
                        }
                    }
                    else if (hit.transform.gameObject.tag == "Force" && isBombLoading && IsAvailableToLoad(hit.transform.gameObject))
                    {
                        if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
                        {
                            int tileIndexForBomb = 0;

                            for (int i = 0; i < tiles.Length; i++)
                            {
                                if (Vector3.Distance(tiles[i].transform.position, ConvertToTile(hit.transform.gameObject).transform.position) < 0.1f)
                                {
                                    tileIndexForBomb = i;
                                    break;
                                }
                            }

                            if (hit.transform.gameObject.GetComponent<ForceInfo>().forceType == (int)ForceType.Tank || hit.transform.gameObject.GetComponent<ForceInfo>().forceType == (int)ForceType.Mine)
                            {
                                SyncLoadBombs(new object[] { 1, tileIndexForBomb });
                            }
                            else if (hit.transform.gameObject.GetComponent<ForceInfo>().forceType == (int)ForceType.Plane || hit.transform.gameObject.GetComponent<ForceInfo>().forceType == (int)ForceType.UFO)
                            {
                                SyncLoadBombs(new object[] { 2, tileIndexForBomb });
                            }
                            else if (hit.transform.gameObject.GetComponent<ForceInfo>().forceType == (int)ForceType.General || hit.transform.gameObject.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
                            {
                                SyncLoadBombs(new object[] { 3, tileIndexForBomb });
                            }
                        }
                    }
                    else if (hit.transform.gameObject.tag == "Tile" && isOnlyOncePlay == false)
                    {
                        if (isMove == true && hit.transform.gameObject.GetComponent<SpriteRenderer>().enabled == true)
                        {
                            if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
                            {
                                for (int i = 0; i < tiles.Length; i++)
                                {
                                    if (hit.transform.gameObject == tiles[i])
                                    {
                                        isOnlyOncePlay = true;
                                        SyncMoveAction(new object[] { i });
                                    }
                                }
                            }
                        }
                        else if (isFire == true && hit.transform.gameObject.GetComponent<SpriteRenderer>().enabled == true)
                        {
                            if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
                            {
                                int tileHit = 0;

                                for (int i = 0; i < tiles.Length; i++)
                                {
                                    if (hit.transform.gameObject == tiles[i])
                                    {
                                        isOnlyOncePlay = true;
                                        tileHit = i;
                                        SyncFireAction(new object[] { i });
                                        break;
                                    }
                                }

                                for (int i = 0; i < atomicMines.Length; i++)
                                {
                                    if (Vector3.Distance(atomicMines[i].transform.position, tiles[tileHit].transform.position) < 0.1f)
                                    {
                                        int randomIndexX = UnityEngine.Random.Range(7, 12);
                                        int randomIndexY = UnityEngine.Random.Range(7, 12);

                                        SyncMinesAction(new object[] { i, randomIndexX + randomIndexY * 30, 0 });
                                        break;
                                    }
                                }
                            }
                        }
                        else if (isUFO == true && !isUFOMove && hit.transform.gameObject.GetComponent<SpriteRenderer>().enabled == true)
                        {
                            if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
                            {
                                for (int i = 0; i < tiles.Length; i++)
                                {
                                    if (hit.transform.gameObject == tiles[i])
                                    {
                                        isOnlyOncePlay = true;
                                        SyncUFORange(new object[] { i });
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (isMoveAnimation == true  )
            {
                endPlayButton.SetActive(false);
                endTurnButton.SetActive(false);

                for (int i = 0; i < subBacks.Length; i++)
                {
                    subBacks[i].GetComponent<SpriteRenderer>().enabled = false;
                }

                if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
                {
                }
                else
                {
                    if (selectedUnit.GetComponent<ForceInfo>().forceType != (int)ForceType.Sub)
                    {
                        if (isSmartMapEnabled)
                            cameraView.transform.position = new Vector3(selectedUnit.transform.position.x, selectedUnit.transform.position.y, cameraView.transform.position.z);
                    }
                }

                if (pathIndex < 0)
                {
                    pathIndex = pathTiles.Count + pathIndex;
                }

                if (Vector3.Distance(selectedUnit.transform.position, pathTiles[pathIndex].transform.position) > 10f)
                {
                    //print("&&&" + Vector3.Distance(selectedUnit.transform.position, pathTiles[pathIndex].transform.position));
                    selectedUnit.transform.position = pathTiles[pathIndex].transform.position;
                    bool isTransported = false;

                    for (int i = 0; i < bermudaTriangles.Length; i++)
                    {
                        if (Vector3.Distance(bermudaTriangles[i].transform.position, selectedUnit.transform.position) < 0.1f)
                        {
                            if (whoseTurn == (int)TurnIndex.Player1 && i >= 10 || whoseTurn == (int)TurnIndex.Player2 && i < 10)
                            {
                                isTransported = true;
                                isIncreasedByBermuda = true;
                                IncreasePlayTimes();
                                break;
                            }                                
                        }
                        else if (Vector3.Distance(atomicMines[i].transform.position, selectedUnit.transform.position) < 0.1f)
                        {
                            if (whoseTurn == (int)TurnIndex.Player1 && i >= 10 || whoseTurn == (int)TurnIndex.Player2 && i < 10)
                            {
                                isIncreasedByMines = true;
                                isTransported = true;
                                IncreasePlayTimes();
                                break;
                            }
                        }
                    }

                    if (!isTransported)
                    {
                        if (pathIndex - 1 >= 0)
                        {
                            //exceedCount++;
                        }
                        else
                        {
                            pathIndex--;
                            exceedCount++;
                        }

                        if (exceedCount > 10)
                        {
                            print("Freeze!!!!!!");
                            selectedUnit.transform.position = targetObject.transform.position;
                            pathIndex = 0;
                            IncreasePlayTimes();
                        }
                    }
                }
                else
                {
                    selectedUnit.transform.position = Vector3.MoveTowards(selectedUnit.transform.position, pathTiles[pathIndex].transform.position, 1.5f * Time.deltaTime);                  
                    bool isTransported = false;

                    for (int i = 0; i < bermudaTriangles.Length; i++)
                    {
                        if (Vector3.Distance(bermudaTriangles[i].transform.position, selectedUnit.transform.position) < 0.1f)
                        {
                            if (whoseTurn == (int)TurnIndex.Player1 && i >= 10 || whoseTurn == (int)TurnIndex.Player2 && i < 10)
                            {
                                isTransported = true;
                                isIncreasedByBermuda = true;
                                IncreasePlayTimes();
                                break;
                            }                                
                        }
                        else if (Vector3.Distance(atomicMines[i].transform.position, selectedUnit.transform.position) < 0.1f)
                        {
                            if (whoseTurn == (int)TurnIndex.Player1 && i >= 10 || whoseTurn == (int)TurnIndex.Player2 && i < 10)
                            {
                                isIncreasedByMines = true;
                                isTransported = true;
                                IncreasePlayTimes();
                                break;
                            }
                        }
                    }

                    if (Vector3.Distance(selectedUnit.transform.position, pathTiles[pathIndex].transform.position) < 0.1f && !isTransported)
                    {
                        if (pathIndex == 0)
                        {
                            for (int i = 0; i < atomicMines.Length; i++)
                            {
                                if (Vector3.Distance(atomicMines[i].transform.position, selectedUnit.transform.position) < 0.1f)
                                {
                                    isIncreasedByMines = true;
                                    break;
                                }
                                else if(Vector3.Distance(bermudaTriangles[i].transform.position, selectedUnit.transform.position) < 0.1f)
                                {
                                    isIncreasedByBermuda = true;
                                    break;
                                }
                            }

                            IncreasePlayTimes();
                        }
                        else
                        {
                            if (Vector3.Distance(selectedUnit.transform.position, pathTiles[pathIndex - 1].transform.position) > 10f)
                            {
                                selectedUnit.transform.position = pathTiles[pathIndex - 1].transform.position;

                                if (pathIndex - 2 >= 0)
                                {
                                    pathIndex -= 2;
                                }
                                else
                                {
                                    pathIndex--;
                                    exceedCount++;
                                }
                            }
                            else
                            {
                                pathIndex--;
                                exceedCount++;
                            }

                            if (exceedCount > 10)
                            {
                                selectedUnit.transform.position = targetObject.transform.position;
                                pathIndex = 0;
                                IncreasePlayTimes();
                            }
                        }
                    }
                }
                //print("ExceedCount: " + exceedCount);
            }
            else
            {
                if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
                {
                    //endPlayButton.SetActive(true);
                    if (!isDiceRolling)
                    {
                        endTurnButton.SetActive(true);
                    }
                    else
                    {
                        endTurnButton.SetActive(false);
                    }

                    if (isChangingTurn)
                    {
                        endPlayButton.SetActive(false);
                    }                 
                }
            }

            if (isUFOMove)
            {
                ufo.transform.position = Vector3.MoveTowards(ufo.transform.position, ufoTarget, 10 * Time.deltaTime);

                if (Vector3.Distance(ufo.transform.position, ufoTarget) < 0.1f)
                {
                    for (int i = 0; i < tiles.Length; i++)
                    {
                        tiles[i].GetComponent<SpriteRenderer>().enabled = false;
                    }

                    isUFOMove = false;
                    dice40.SetActive(true);
                    dice41.SetActive(false);
                    dice60.SetActive(false);
                    dice40.GetComponent<Animator>().enabled = true;
                    dice61.SetActive(true);
                    dice61.GetComponent<Animator>().enabled = true;
                    diceAudio.Play();

                    if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
                    {
                        SyncUFODiceAnimation(new object[] { UnityEngine.Random.Range(0, 4), UnityEngine.Random.Range(0, 4) });
                    }
                }
            }

            if (isUFOMoving)
            {
                endPlayButton.SetActive(false);
                endTurnButton.SetActive(false);
            }
            else
            {
                if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
                {
                    //endPlayButton.SetActive(true);
                    //endTurnButton.SetActive(true);
                }
            }

            if (isActivatedBermuda && isIncreasedTimes)
            {
                isActivatedBermuda = false;
                isIncreasedTimes = false;
                isProgressAction = true;
                bermudaTriangles[bermudaInfos[2]].SetActive(true);

                switch (bermudaInfos[0])
                {
                    case 0:
                        {
                            transportUnit = force1ForCircle[bermudaInfos[1]];
                            break;
                        }
                    case 1:
                        {
                            transportUnit = force1ForSquare[bermudaInfos[1]];
                            break;
                        }
                    case 2:
                        {
                            transportUnit = force2ForCircle[bermudaInfos[1]];
                            break;
                        }
                    case 3:
                        {
                            transportUnit = force2ForSquare[bermudaInfos[1]];
                            break;
                        }
                }

                // show bermuda text
                if (transportUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Tank)
                {
                    bermudaText.text = "Tank has entered into a Bermuda Triangle.";
                    videoIndexBT = 1;
                }
                else if (transportUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.General)
                {
                    bermudaText.text = "General has entered into a Bermuda Triangle.";
                    videoIndexBT = 2;
                }
                else if (transportUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
                {
                    bermudaText.text = "Sub has entered into a Bermuda Triangle.";
                    videoIndexBT = 3;
                }
                else if (transportUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Plane)
                {
                    bermudaText.text = "Jet has entered into a Bermuda Triangle.";
                    videoIndexBT = 4;
                }

                StartCoroutine(DelayToShowInfoBermuda());
            }

            if (isExplodedMines)
            {
                isExplodedMines = false;
                isProgressAction = true;
                atomicMines[minesInfos[0]].SetActive(true);
                atomicMines[minesInfos[0]].transform.GetChild(0).gameObject.SetActive(true);

                for (int i = 0; i < tiles.Length; i++)
                {
                    if (Vector3.Distance(tiles[i].transform.position, atomicMines[minesInfos[0]].transform.position) < 0.1f)
                    {
                        if (minesInfos[2] == 0)
                        {
                            StartCoroutine(DelayToFinishBermuda(i, 0));
                        }
                        else
                        {
                            StartCoroutine(DelayToFinishBermuda(i, 3f));
                        }

                        break;
                    }
                }
            }
 
            // print("???" + bootingObject.GetComponent<Image>().fillAmount);
            if (bootingObject.GetComponent<Image>().fillAmount <= 0f && isShowWinning == false && waitingforserverTurnChange==false)
            {
                if ((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame))
                {
                    endPlayButton.SetActive(false);
                    
                    if (!isEndedByMines && !isIncreasedByMines && !isMoveAnimation && !isIncreasedByBermuda && !isDiceRolling)
                    {
                        isShowWinning = true;
                        EndTurn();
                    }
                }
            }
        }
    }
    bool waitingforserverTurnChange=false;
    // Tab change detect
    //private void OnApplicationFocusChange(bool hasFocus)
    //{
    //    isGameVisible = hasFocus;

    //    if (isGameVisible)
    //    {
    //        // Game tab is visible, handle resuming game logic
    //    }
    //    else
    //    {
    //        // Game tab is hidden, handle pausing game logic
    //    }
    //}

    IEnumerator ShowWinningDialog(int index)
    {
        SyncGameEnd(new object[] { index });

        yield return new WaitForSeconds(1f);
    }

    IEnumerator DelayToEndGame()
    {
        //StartCoroutine(PostEndedRoom(MainUI.frontURL + "/api/endPlaying"));
        yield return new WaitForSeconds(10f);
        
        SceneManager.LoadScene("Main");
        if (TournamentLobbyCreator.isInTournament())
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            PhotonNetwork.Disconnect();

        }        
    }

    public void ExitBtnClick()
    {        
        SceneManager.LoadScene("Main");
        if (TournamentLobbyCreator.isInTournament())
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            PhotonNetwork.Disconnect();

        }
    }

    public int GeneralNumberOfEnemy()
    {
        indices = 0;

        if (whoseTurn == (int)TurnIndex.Player1)
        {
            for (int i = 0; i < 2; i++)
            {
                if (force2ForCircle[i].transform.position != new Vector3(10000f, 10000f, -8f))
                {
                    indices++;
                }

                if (force2ForSquare[i].transform.position != new Vector3(10000f, 10000f, -8f))
                {
                    indices++;
                }
            }
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                if (force1ForCircle[i].transform.position != new Vector3(10000f, 10000f, -8f))
                {
                    indices++;
                }

                if (force1ForSquare[i].transform.position != new Vector3(10000f, 10000f, -8f))
                {
                    indices++;
                }
            }
        }

        return indices;
    }

    public int GeneralNumberofLegion(int legionIndex)
    {
        indices = 0;

        if (whoseTurn == (int)TurnIndex.Player1)
        {
            for (int i = 0; i < 2; i++)
            {
                if (legionIndex == 0 && force2ForCircle[i].transform.position != new Vector3(10000f, 10000f, -8f))
                {
                    indices++;
                }

                if (legionIndex == 1 && force2ForSquare[i].transform.position != new Vector3(10000f, 10000f, -8f))
                {
                    indices++;
                }
            }
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                if (legionIndex == 0 && force1ForCircle[i].transform.position != new Vector3(10000f, 10000f, -8f))
                {
                    indices++;
                }

                if (legionIndex == 1 && force1ForSquare[i].transform.position != new Vector3(10000f, 10000f, -8f))
                {
                    indices++;
                }
            }
        }

        return indices;
    }

    public int GeneralNumbers(int teamInfo)
    {
        indices = 0;

        switch (teamInfo)
        {
            case 0:
                {
                    if (force1ForCircle[0].transform.position != new Vector3(10000f, 10000f, -8f) ||
                        force1ForCircle[1].transform.position != new Vector3(10000f, 10000f, -8f))
                    {
                        indices++;
                    }

                    break;
                }
            case 1:
                {
                    if (force1ForSquare[0].transform.position != new Vector3(10000f, 10000f, -8f) ||
                        force1ForSquare[1].transform.position != new Vector3(10000f, 10000f, -8f))
                    {
                        indices++;
                    }

                    break;
                }
            case 2:
                {
                    if (force2ForCircle[0].transform.position != new Vector3(10000f, 10000f, -8f) ||
                        force2ForCircle[1].transform.position != new Vector3(10000f, 10000f, -8f))
                    {
                        indices++;
                    }

                    break;
                }
            case 3:
                {
                    if (force2ForSquare[0].transform.position != new Vector3(10000f, 10000f, -8f) ||
                        force2ForSquare[1].transform.position != new Vector3(10000f, 10000f, -8f))
                    {
                        indices++;
                    }

                    break;
                }

        }

        return indices;
    }

    public int NumbersOfAvailableUnits()
    {
        indices = 0;

        if (whoseTurn == (int)TurnIndex.Player1)
        {
            for (int i = 0; i < force1ForCircle.Length; i++)
            {
                if (force1ForCircle[i].transform.position != new Vector3(10000f, 10000f, -8f))
                {
                    indices++;
                }

                if (force1ForSquare[i].transform.position != new Vector3(10000f, 10000f, -8f))
                {
                    indices++;
                }
            }
        }
        else
        {
            for (int i = 0; i < force2ForCircle.Length; i++)
            {
                if (force2ForCircle[i].transform.position != new Vector3(10000f, 10000f, -8f))
                {
                    indices++;
                }

                if (force2ForSquare[i].transform.position != new Vector3(10000f, 10000f, -8f))
                {
                    indices++;
                }
            }
        }

        return indices;
    }

    public void SetSkins(int a, int b, int c, int d)
    {
        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            switch (a)
            {
                case 0:
                    {
                        force1ForCircle[i].GetComponent<SpriteRenderer>().sprite = demoSkinsForLeft[i];
                        break;
                    }
                case 1:
                    {
                        force1ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins1ForLeft[i];
                        break;
                    }
                case 2:
                    {
                        force1ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins2ForLeft[i];
                        break;
                    }
                case 3:
                    {
                        force1ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins3ForLeft[i];
                        break;
                    }
                case 4:
                    {
                        force1ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins4ForLeft[i];
                        break;
                    }
                case 5:
                    {
                        force1ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins5ForLeft[i];
                        break;
                    }
                case 6:
                    {
                        force1ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins6ForLeft[i];
                        break;
                    }
                case 7:
                    {
                        force1ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins7ForLeft[i];
                        break;
                    }
                case 8:
                    {
                        force1ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins8ForLeft[i];
                        break;
                    }
                case 9:
                    {
                        force1ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins9ForLeft[i];
                        break;
                    }
                case 10:
                    {
                        force1ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins10ForLeft[i];
                        break;
                    }
                case 11:
                    {
                        force1ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins11ForLeft[i];
                        break;
                    }
                case 12:
                    {
                        force1ForCircle[i].GetComponent<SpriteRenderer>().sprite = demoSkinsForLeft[i];
                        break;
                    }
                case 13:
                    {
                        force1ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins12ForLeft[i];
                        break;
                    }
                case 14:
                    {
                        force1ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins13ForLeft[i];
                        break;
                    }
            }
        }

        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            switch (b)
            {
                case 0:
                    {
                        force1ForSquare[i].GetComponent<SpriteRenderer>().sprite = demoSkinsForLeft[i];
                        break;
                    }
                case 1:
                    {
                        force1ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins1ForLeft[i];
                        break;
                    }
                case 2:
                    {
                        force1ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins2ForLeft[i];
                        break;
                    }
                case 3:
                    {
                        force1ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins3ForLeft[i];
                        break;
                    }
                case 4:
                    {
                        force1ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins4ForLeft[i];
                        break;
                    }
                case 5:
                    {
                        force1ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins5ForLeft[i];
                        break;
                    }
                case 6:
                    {
                        force1ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins6ForLeft[i];
                        break;
                    }
                case 7:
                    {
                        force1ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins7ForLeft[i];
                        break;
                    }
                case 8:
                    {
                        force1ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins8ForLeft[i];
                        break;
                    }
                case 9:
                    {
                        force1ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins9ForLeft[i];
                        break;
                    }
                case 10:
                    {
                        force1ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins10ForLeft[i];
                        break;
                    }
                case 11:
                    {
                        force1ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins11ForLeft[i];
                        break;
                    }
                case 12:
                    {
                        force1ForSquare[i].GetComponent<SpriteRenderer>().sprite = demoSkinsForLeft[i];
                        break;
                    }
                case 13:
                    {
                        force1ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins12ForLeft[i];
                        break;
                    }
                case 14:
                    {
                        force1ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins13ForLeft[i];
                        break;
                    }
            }
        }

        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            switch (c)
            {
                case 0:
                    {
                        force2ForCircle[i].GetComponent<SpriteRenderer>().sprite = demoSkinsForRight[i];
                        break;
                    }
                case 1:
                    {
                        force2ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins1ForRight[i];
                        break;
                    }
                case 2:
                    {
                        force2ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins2ForRight[i];
                        break;
                    }
                case 3:
                    {
                        force2ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins3ForRight[i];
                        break;
                    }
                case 4:
                    {
                        force2ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins4ForRight[i];
                        break;
                    }
                case 5:
                    {
                        force2ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins5ForRight[i];
                        break;
                    }
                case 6:
                    {
                        force2ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins6ForRight[i];
                        break;
                    }
                case 7:
                    {
                        force2ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins7ForRight[i];
                        break;
                    }
                case 8:
                    {
                        force2ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins8ForRight[i];
                        break;
                    }
                case 9:
                    {
                        force2ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins9ForRight[i];
                        break;
                    }
                case 10:
                    {
                        force2ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins10ForRight[i];
                        break;
                    }
                case 11:
                    {
                        force2ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins11ForRight[i];
                        break;
                    }
                case 12:
                    {
                        force2ForCircle[i].GetComponent<SpriteRenderer>().sprite = demoSkinsForRight[i];
                        break;
                    }
                case 13:
                    {
                        force2ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins12ForRight[i];
                        break;
                    }
                case 14:
                    {
                        force2ForCircle[i].GetComponent<SpriteRenderer>().sprite = skins13ForRight[i];
                        break;
                    }
            }
        }

        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            switch (d)
            {
                case 0:
                    {
                        force2ForSquare[i].GetComponent<SpriteRenderer>().sprite = demoSkinsForRight[i];
                        break;
                    }
                case 1:
                    {
                        force2ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins1ForRight[i];
                        break;
                    }
                case 2:
                    {
                        force2ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins2ForRight[i];
                        break;
                    }
                case 3:
                    {
                        force2ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins3ForRight[i];
                        break;
                    }
                case 4:
                    {
                        force2ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins4ForRight[i];
                        break;
                    }
                case 5:
                    {
                        force2ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins5ForRight[i];
                        break;
                    }
                case 6:
                    {
                        force2ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins6ForRight[i];
                        break;
                    }
                case 7:
                    {
                        force2ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins7ForRight[i];
                        break;
                    }
                case 8:
                    {
                        force2ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins8ForRight[i];
                        break;
                    }
                case 9:
                    {
                        force2ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins9ForRight[i];
                        break;
                    }
                case 10:
                    {
                        force2ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins10ForRight[i];
                        break;
                    }
                case 11:
                    {
                        force2ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins11ForRight[i];
                        break;
                    }
                case 12:
                    {
                        force2ForSquare[i].GetComponent<SpriteRenderer>().sprite = demoSkinsForRight[i];
                        break;
                    }
                case 13:
                    {
                        force2ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins12ForRight[i];
                        break;
                    }
                case 14:
                    {
                        force2ForSquare[i].GetComponent<SpriteRenderer>().sprite = skins13ForRight[i];
                        break;
                    }
            }
        }

        //if (a == b)
        //{
        //    for (int i = 0; i < force1ForCircle.Length; i++)
        //    {
        //        print(i);
        //        force1ForCircle[i].GetComponentsInChildren<SpriteRenderer>()[1].enabled = true;
        //        force1ForSquare[i].GetComponentsInChildren<SpriteRenderer>()[1].enabled = true;
        //    }
        //}

        //if (a == c)
        //{
        //    for (int i = 0; i < force1ForCircle.Length; i++)
        //    {
        //        print(i);
        //        force1ForCircle[i].GetComponentsInChildren<SpriteRenderer>()[1].enabled = true;                
        //        force2ForCircle[i].GetComponentsInChildren<SpriteRenderer>()[1].enabled = true;
        //    }
        //}

        //if (a == d)
        //{
        //    for (int i = 0; i < force1ForCircle.Length; i++)
        //    {
        //        print(i);
        //        force1ForCircle[i].GetComponentsInChildren<SpriteRenderer>()[1].enabled = true;
        //        force2ForSquare[i].GetComponentsInChildren<SpriteRenderer>()[1].enabled = true;
        //    }
        //}

        //if (b == c)
        //{
        //    for (int i = 0; i < force1ForCircle.Length; i++)
        //    {
        //        print(i);
        //        force1ForSquare[i].GetComponentsInChildren<SpriteRenderer>()[1].enabled = true;
        //        force2ForCircle[i].GetComponentsInChildren<SpriteRenderer>()[1].enabled = true;
        //    }
        //}

        //if (b == d)
        //{
        //    for (int i = 0; i < force1ForCircle.Length; i++)
        //    {
        //        print(i);
        //        force1ForSquare[i].GetComponentsInChildren<SpriteRenderer>()[1].enabled = true;
        //        force2ForSquare[i].GetComponentsInChildren<SpriteRenderer>()[1].enabled = true;
        //    }
        //}

        //if (c == d)
        //{
        //    for (int i = 0; i < force1ForCircle.Length; i++)
        //    {
        //        print(i);
        //        force2ForCircle[i].GetComponentsInChildren<SpriteRenderer>()[1].enabled = true;
        //        force2ForSquare[i].GetComponentsInChildren<SpriteRenderer>()[1].enabled = true;
        //    }
        //}
    }

    public void SetAudio(int index)
    {
        if (index == 0)
        {
            if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.General)
            {
                selectedUnit.GetComponent<AudioSource>().clip = generalAudios[0];
            }
            else if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Plane)
            {
                selectedUnit.GetComponent<AudioSource>().clip = airplaneAudios[0];
            }
            else if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                selectedUnit.GetComponent<AudioSource>().clip = subAudios[0];
            }
            else if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Tank)
            {
                selectedUnit.GetComponent<AudioSource>().clip = tankAudios[0];
            }
            else if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.UFO)
            {
                selectedUnit.GetComponent<AudioSource>().clip = ufoAudios[0];
            }
        }
        else if (index == 1)
        {
            if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.General)
            {
                selectedUnit.GetComponent<AudioSource>().clip = generalAudios[1];
            }
            else if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Plane)
            {
                selectedUnit.GetComponent<AudioSource>().clip = airplaneAudios[1];
            }
            else if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                selectedUnit.GetComponent<AudioSource>().clip = subAudios[1];
            }
            else if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Tank)
            {
                selectedUnit.GetComponent<AudioSource>().clip = tankAudios[1];
            }
            else if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.UFO)
            {
                selectedUnit.GetComponent<AudioSource>().clip = ufoAudios[1];
            }
        }
        else if (index == 2)
        {
            selectedUnit.GetComponent<AudioSource>().clip = subAudios[2];
        }

        selectedUnit.GetComponent<AudioSource>().volume = 0.5f;
    }
 
    public void setSubButton(bool val)
    {
        subButton.SetActive(val);
 

    }
    // After moving action done, game parameters will be set
    public void IncreasePlayTimes()
    {
        isSelected = false;
        isMoveAnimation = false;
        isOnlyOncePlay = false;
        isUFO = false;
        isUFOMove = false;
        isUFOMoving = false;
        isProgressAction = false;
        isBlackDragon = false;
        moveButton.SetActive(false);
        fireButton.SetActive(false);
        setSubButton(false);
        sonarButton.SetActive(false);
        endPlayButton.SetActive(false);
        pingButton.SetActive(false);


        subFireButton.SetActive(false);
        ufoFireButton.SetActive(false);
        ufoButton.SetActive(false);
        ufoDialog.SetActive(false);
        blackDragonButton.SetActive(false);
        //exceedCount = 0;
        isIncreasedTimes = true;

        if (whoseTurn == (int)TurnIndex.Player1)
        {
            if (!InteractiveTutorial.TutorialActive)
                players[0].GetComponent<PlayerInfo>().playTimes--;
            playsLeftText.text = "" + players[0].GetComponent<PlayerInfo>().playTimes;
        }
        else if (whoseTurn == (int)TurnIndex.Player2)
        {
            if (!InteractiveTutorial.TutorialActive)
                players[1].GetComponent<PlayerInfo>().playTimes--;
            playsLeftText.text = "" + players[1].GetComponent<PlayerInfo>().playTimes;
        }

        // Check bermuda actions
        if ((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame))
        {
            for (int i = 0; i < force1ForCircle.Length; i++)
            {
                for (int j = 0; j < bermudaTriangles.Length; j++)
                {
                    if (Vector3.Distance(force1ForCircle[i].transform.position, bermudaTriangles[j].transform.position) < 0.1f)
                    {
                        isProgressAction = true;
                        TransportViaBermuda(0, i, j);
                    }

                    if (Vector3.Distance(force1ForSquare[i].transform.position, bermudaTriangles[j].transform.position) < 0.1f)
                    {
                        isProgressAction = true;
                        TransportViaBermuda(1, i, j);
                    }

                    if (Vector3.Distance(force2ForCircle[i].transform.position, bermudaTriangles[j].transform.position) < 0.1f)
                    {
                        isProgressAction = true;
                        TransportViaBermuda(2, i, j);
                    }

                    if (Vector3.Distance(force2ForSquare[i].transform.position, bermudaTriangles[j].transform.position) < 0.1f)
                    {
                        isProgressAction = true;
                        TransportViaBermuda(3, i, j);
                    }
                }
            }
        }

        // Check mines actions
        CheckMinesAction(0);

        if ((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame))
        {
            for (int i = 0; i < MainUI.gameOptions.Length; i++)
            {
                if (MainUI.gameOptions[i] == '@')
                {
                    ufoButton.SetActive(true);
                }

                //if (MainUI.gameOptions[i] == '&')
                //{
                //    blackDragonButton.SetActive(true);
                //}
            }
        }

        ufo.transform.position = new Vector3(ufoInitialPos.x * UnityEngine.Random.Range(0, 2) == 0 ? 1f : -1f, ufoInitialPos.y * UnityEngine.Random.Range(0, 2) == 0 ? 1f : -1f, ufoInitialPos.z);
        ufo.GetComponent<AudioSource>().Stop();
        ufo.SetActive(false);

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].GetComponent<SpriteRenderer>().enabled = false;
        }

        // Show sub background
        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            if (force1ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                if (ConvertToTile(force1ForCircle[i]).GetComponent<CellInfo>().cellType == (int)CellType.Tunnel ||
                    ConvertToTile(force1ForCircle[i]).GetComponent<CellInfo>().cellType == (int)CellType.TunnelEntrance)
                {
                    tunnelColor = new Color32(69, 130, 255, 1);
                    tunnelColor.a = 0.7f;

                    if (i == 4)
                    {
                        subBacks[0].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[0].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                    else
                    {
                        subBacks[1].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[1].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                }
                else
                {
                    if (i == 4)
                    {
                        subBacks[0].GetComponent<SpriteRenderer>().enabled = false;
                    }
                    else
                    {
                        subBacks[1].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }

            if (force1ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                if (ConvertToTile(force1ForSquare[i]).GetComponent<CellInfo>().cellType == (int)CellType.Tunnel ||
                    ConvertToTile(force1ForSquare[i]).GetComponent<CellInfo>().cellType == (int)CellType.TunnelEntrance)
                {
                    tunnelColor = new Color32(69, 130, 255, 1);
                    tunnelColor.a = 0.7f;

                    if (i == 4)
                    {
                        subBacks[2].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[2].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                    else
                    {
                        subBacks[3].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[3].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                }
                else
                {
                    if (i == 4)
                    {
                        subBacks[2].GetComponent<SpriteRenderer>().enabled = false;
                    }
                    else
                    {
                        subBacks[3].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }

            if (force2ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                if (ConvertToTile(force2ForCircle[i]).GetComponent<CellInfo>().cellType == (int)CellType.Tunnel ||
                    ConvertToTile(force2ForCircle[i]).GetComponent<CellInfo>().cellType == (int)CellType.TunnelEntrance)
                {
                    tunnelColor = new Color32(69, 130, 255, 1);
                    tunnelColor.a = 0.7f;

                    if (i == 4)
                    {
                        subBacks[4].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[4].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                    else
                    {
                        subBacks[5].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[5].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                }
                else
                {
                    if (i == 4)
                    {
                        subBacks[4].GetComponent<SpriteRenderer>().enabled = false;
                    }
                    else
                    {
                        subBacks[5].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }

            if (force2ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                if (ConvertToTile(force2ForSquare[i]).GetComponent<CellInfo>().cellType == (int)CellType.Tunnel ||
                    ConvertToTile(force2ForSquare[i]).GetComponent<CellInfo>().cellType == (int)CellType.TunnelEntrance)
                {
                    tunnelColor = new Color32(69, 130, 255, 1);
                    tunnelColor.a = 0.7f;

                    if (i == 4)
                    {
                        subBacks[6].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[6].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                    else
                    {
                        subBacks[7].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[7].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                }
                else
                {
                    if (i == 4)
                    {
                        subBacks[6].GetComponent<SpriteRenderer>().enabled = false;
                    }
                    else
                    {
                        subBacks[7].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }
        }

        selectedUnit.GetComponent<AudioSource>().Stop();
        selectedUnit.GetComponent<ForceInfo>().controlTimes++;

        if (selectedUnit.GetComponent<ForceInfo>().controlTimes == 2)
        {
            selectedUnit.GetComponent<Animator>().enabled = false;
        }

        if (whoseTurn == (int)TurnIndex.Player1 && players[0].GetComponent<PlayerInfo>().playTimes == 0 ||
            whoseTurn == (int)TurnIndex.Player2 && players[1].GetComponent<PlayerInfo>().playTimes == 0)
        {
            for (int i = 0; i < force1ForCircle.Length; i++)
            {
                force1ForCircle[i].GetComponent<Animator>().enabled = false;
                force1ForSquare[i].GetComponent<Animator>().enabled = false;
                force2ForCircle[i].GetComponent<Animator>().enabled = false;
                force2ForSquare[i].GetComponent<Animator>().enabled = false;
            }

            ufoButton.SetActive(false);

            if (!isIncreasedByMines && !isIncreasedByBermuda)
            {
                StartCoroutine(DelayTurn());
            }
        }
    }

    IEnumerator DelayTurn()
    {
        if (whoseTurn == (int)TurnIndex.Player1 && players[0].GetComponent<PlayerInfo>().playTimes == 0 ||
            whoseTurn == (int)TurnIndex.Player2 && players[1].GetComponent<PlayerInfo>().playTimes == 0)
        {
            for (int i = 0; i < force1ForCircle.Length; i++)
            {
                force1ForCircle[i].GetComponent<Animator>().enabled = false;
                force1ForSquare[i].GetComponent<Animator>().enabled = false;
                force2ForCircle[i].GetComponent<Animator>().enabled = false;
                force2ForSquare[i].GetComponent<Animator>().enabled = false;
            }
        }

        yield return new WaitForSeconds(5f);

        if (whoseTurn == (int)TurnIndex.Player1 && players[0].GetComponent<PlayerInfo>().playTimes == 0)
        {
            if (!isMasterInGame)
            {
                Debug.Log("Delay Turn called 0 ");
                SyncTurn(new object[] { 1 });
            }
        }
        else if (whoseTurn == (int)TurnIndex.Player2 && players[1].GetComponent<PlayerInfo>().playTimes == 0)
        {
            if (isMasterInGame)
            {
                Debug.Log("Delay Turn called 1");

                SyncTurn(new object[] { 0 });
            }
        }
    }

    // Convert from unit to tile
    public GameObject ConvertToTile(GameObject force)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (Vector3.Distance(tiles[i].transform.position, force.transform.position) < 0.1f)
            {
                convertedTile = tiles[i];
            }
        }

        return convertedTile;
    }

    // Get bool whether this unit will be available on the map
    public bool IsAvailableAttack(GameObject cell)
    {
        isAvailableAttack = false;

        for (int i = 0; i < tiles.Length; i++)
        {
            if (Vector3.Distance(tiles[i].transform.position, cell.transform.position) < 0.1f)
            {
                isAvailableAttack = true;
            }
        }

        return isAvailableAttack;
    }

    public bool IsEnemy(GameObject cell)
    {
        isEnemy = false;

        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            if (ConvertToTile(cell).gameObject.GetComponent<SpriteRenderer>().enabled ||
                cell.transform.GetChild(1).gameObject.activeSelf)
            {
                if (whoseTurn == (int)TurnIndex.Player1)
                {
                    if (Vector3.Distance(cell.transform.position, force2ForCircle[i].transform.position) < 0.1f ||
                    Vector3.Distance(cell.transform.position, force2ForSquare[i].transform.position) < 0.1f
                    )
                    {
                        isEnemy = true;
                    }
                }
                else if (whoseTurn == (int)TurnIndex.Player2)
                {
                    if (Vector3.Distance(cell.transform.position, force1ForCircle[i].transform.position) < 0.1f ||
                        Vector3.Distance(cell.transform.position, force1ForSquare[i].transform.position) < 0.1f
                        )
                    {
                        isEnemy = true;
                    }
                }
            }
        }

        return isEnemy;
    }

    public bool IsAvailableToLoad(GameObject force)
    {
        bool isAvailableToLoad = false;

        // click units to load
        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            if (whoseTurn == (int)TurnIndex.Player1)
            {
                if (Vector3.Distance(force.transform.position, force1ForCircle[i].transform.position) < 0.1f ||
                    Vector3.Distance(force.transform.position, force1ForSquare[i].transform.position) < 0.1f
                    )
                {
                    isAvailableToLoad = true;
                }
            }
            else if (whoseTurn == (int)TurnIndex.Player2)
            {
                if (Vector3.Distance(force.transform.position, force2ForCircle[i].transform.position) < 0.1f ||
                    Vector3.Distance(force.transform.position, force2ForSquare[i].transform.position) < 0.1f
                    )
                {
                    isAvailableToLoad = true;
                }
            }
        }

        // click mines to load
        for (int i = 0; i < atomicMines.Length / 2; i++)
        {
            if (whoseTurn == (int)TurnIndex.Player1)
            {
                if (Vector3.Distance(force.transform.position, atomicMines[i].transform.position) < 0.1f)
                {
                    isAvailableToLoad = true;
                }
            }
            else if (whoseTurn == (int)TurnIndex.Player2)
            {
                if (Vector3.Distance(force.transform.position, atomicMines[i + 10].transform.position) < 0.1f)
                {
                    isAvailableToLoad = true;
                }
            }
        }

        return isAvailableToLoad;
    }

    public bool IsEmpty(GameObject cell)
    {
        isEmpty = true;

        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            if (Vector3.Distance(cell.transform.position, force2ForCircle[i].transform.position) < 0.1f ||
                Vector3.Distance(cell.transform.position, force2ForSquare[i].transform.position) < 0.1f ||
                Vector3.Distance(cell.transform.position, force1ForCircle[i].transform.position) < 0.1f ||
                Vector3.Distance(cell.transform.position, force1ForSquare[i].transform.position) < 0.1f
                )
            {
                isEmpty = false;
            }
        }

        return isEmpty;
    }

    public void MoveButtonClick()
    {
        isSelectedSync = false;
        moveButton.SetActive(false);
        fireButton.SetActive(false);
        setSubButton(false);
        sonarButton.SetActive(false);
        SyncMoveClick(new object[] { });
    }

    public void FireButtonClick()
    {
        isSelectedSync = false;
        moveButton.SetActive(false);
        fireButton.SetActive(false);
        setSubButton(false);
        sonarButton.SetActive(false);
        SyncFireClick(new object[] { });
    }

    public void SonarButtonClick()
    {
        isSonar = true;
        isSelectedSync = false;
        moveButton.SetActive(false);
        fireButton.SetActive(false);
        setSubButton(false);
        sonarButton.SetActive(false);
        SyncSonarClick(new object[] { });
    }

    public void EndPlayButtonClick()
    {
        isSelectedSync = false;
        SyncEndPlay(new object[] { });
    }

    public void SubClick()
    {
        isSelectedSync = false;
        moveButton.SetActive(false);
        fireButton.SetActive(false);
        setSubButton(false);
        sonarButton.SetActive(false);
        SyncSubClick(new object[] { });
    }

    public void PingClick()
    {
        isSelectedSync = false;
        pingButton.SetActive(false);
        SyncPingClick(new object[] { });
    }

    public void UFOClick()
    {
        isSelectedSync = false;

        if (ufo.GetComponent<ForceInfo>().controlTimes < 2)
        {
            ufoDialog.SetActive(true);
        }
    }

    public void UFOCall()
    {
        currentTurnIndex = whoseTurn;
        
        if (ufo.GetComponent<ForceInfo>().controlTimes < 2 && !isProgressAction)
        {
            if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
            {
                isUFO = true;
                ufoButton.SetActive(false);
                endPlayButton.SetActive(true);
                SyncUFOCall(new object[] { });
            }
        }
    }

    public void BlackDragonCall()
    {
        isSelectedSync = false;

        if (blackDragonTime == 1)
        {
            if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
            {
                endPlayButton.SetActive(true);
                isBombLoading = true;
                SyncBlackDragonCall(new object[] { });
            }
        }
    }

    public void SubFireClick()
    {
        subFireButton.SetActive(false);

        SyncSubAction(new object[] { });
    }

    public void UFOFireClick()
    {
        //SyncUFOAction(new object[] { });
    }

    IEnumerator DiceAnimationForMove(int moveRange)
    {
        switch (selectedUnit.GetComponent<ForceInfo>().forceType)
        {
            case (int)ForceType.General:
                {
                    dice60.SetActive(false);
                    dice61.SetActive(false);
                    dice40.SetActive(true);
                    dice41.SetActive(false);
                    dice40.GetComponent<Animator>().enabled = true;
                    break;
                }
            case (int)ForceType.Plane:
                {
                    dice60.SetActive(true);
                    dice61.SetActive(false);
                    dice40.SetActive(false);
                    dice41.SetActive(false);
                    dice60.GetComponent<Animator>().enabled = true;
                    break;
                }
            case (int)ForceType.Sub:
                {
                    dice60.SetActive(false);
                    dice61.SetActive(false);
                    dice40.SetActive(true);
                    dice41.SetActive(false);
                    dice40.GetComponent<Animator>().enabled = true;
                    break;
                }
            case (int)ForceType.Tank:
                {
                    dice60.SetActive(false);
                    dice61.SetActive(false);
                    dice40.SetActive(true);
                    dice41.SetActive(false);
                    dice40.GetComponent<Animator>().enabled = true;
                    break;
                }
        }

        diceAudio.Play();
        selectedUnit.GetComponent<AudioSource>().Stop();
        moveButton.SetActive(false);
        fireButton.SetActive(false);
        setSubButton(false);
        sonarButton.SetActive(false);
        isDiceRolling = true;

        yield return new WaitForSeconds(2f);

        diceAudio.Stop();
        isDiceRolling = false;

        int index = 0;

        if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
        {
            if (ConvertToTile(selectedUnit).GetComponent<PositionInfo>().positionType == (int)PositionType.Corner)
            {
                UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
                index = UnityEngine.Random.Range(2, 4);

                if (index == previousIndex)
                {
                    index = UnityEngine.Random.Range(2, 4);
                }

                previousIndex = index;
            }
            else if (ConvertToTile(selectedUnit).GetComponent<PositionInfo>().positionType == (int)PositionType.Start)
            {
                index = 1;
            }
            else
            {
                UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
                index = (System.DateTime.Now.Millisecond * UnityEngine.Random.Range(2, 8)) % moveRange;

                if (index == previousIndex || index == 0)
                {
                    if (UnityEngine.Random.Range(0, 100) % 2 == 0)
                    {
                        index = (index + 1) % moveRange;
                    }
                    else
                    {
                        index = (index + 2) % moveRange;
                    }
                }

                previousIndex = index;
            }

            SyncDiceAnimation(new object[] { index, 0 });
        }
    }

    IEnumerator DiceAnimationForFire(int fireRange)
    {
        switch (selectedUnit.GetComponent<ForceInfo>().forceType)
        {
            case (int)ForceType.General:
                {
                    dice60.SetActive(false);
                    dice61.SetActive(false);
                    dice40.SetActive(true);
                    dice41.SetActive(false);
                    dice40.GetComponent<Animator>().enabled = true;
                    break;
                }
            case (int)ForceType.Plane:
                {
                    dice60.SetActive(false);
                    dice61.SetActive(false);
                    dice40.SetActive(true);
                    dice41.SetActive(false);
                    dice40.GetComponent<Animator>().enabled = true;
                    break;
                }
            case (int)ForceType.Sub:
                {
                    dice60.SetActive(false);
                    dice61.SetActive(false);
                    dice40.SetActive(true);
                    dice41.SetActive(false);
                    dice40.GetComponent<Animator>().enabled = true;
                    break;
                }
            case (int)ForceType.Tank:
                {
                    dice60.SetActive(true);
                    dice61.SetActive(false);
                    dice40.SetActive(false);
                    dice41.SetActive(false);
                    dice60.GetComponent<Animator>().enabled = true;
                    break;
                }
        }

        diceAudio.Play();
        selectedUnit.GetComponent<AudioSource>().Stop();
        isDiceRolling = true;

        moveButton.SetActive(false);
        fireButton.SetActive(false);
        setSubButton(false);
        sonarButton.SetActive(false);

        yield return new WaitForSeconds(2f);

        diceAudio.Stop();
        isDiceRolling = false;

        int index = 0;

        if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
        {
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
            index = (System.DateTime.Now.Millisecond * UnityEngine.Random.Range(2, 8)) % fireRange;

            if (index == previousIndex || index == 0)
            {
                if (UnityEngine.Random.Range(0, 100) % 2 == 0)
                {
                    index = (index + 1) % fireRange;
                }
                else
                {
                    index = (index + 2) % fireRange;
                }
            }

            previousIndex = index;

            SyncDiceAnimation(new object[] { index, 1 });
        }
    }

    IEnumerator DiceAnimationForSub(int fireRange)
    {
        switch (selectedUnit.GetComponent<ForceInfo>().forceType)
        {
            case (int)ForceType.General:
                {
                    dice60.SetActive(false);
                    dice61.SetActive(false);
                    dice40.SetActive(true);
                    dice41.SetActive(false);
                    dice40.GetComponent<Animator>().enabled = true;
                    break;
                }
            case (int)ForceType.Plane:
                {
                    dice60.SetActive(false);
                    dice61.SetActive(false);
                    dice40.SetActive(true);
                    dice41.SetActive(false);
                    dice40.GetComponent<Animator>().enabled = true;
                    break;
                }
            case (int)ForceType.Sub:
                {
                    dice60.SetActive(false);
                    dice61.SetActive(false);
                    dice40.SetActive(true);
                    dice41.SetActive(false);
                    dice40.GetComponent<Animator>().enabled = true;
                    break;
                }
            case (int)ForceType.Tank:
                {
                    dice60.SetActive(true);
                    dice61.SetActive(false);
                    dice40.SetActive(false);
                    dice41.SetActive(false);
                    dice60.GetComponent<Animator>().enabled = true;
                    break;
                }
        }
        moveButton.SetActive(false);
        fireButton.SetActive(false);
        setSubButton(false);
        sonarButton.SetActive(false);
        diceAudio.Play();
        selectedUnit.GetComponent<AudioSource>().Stop();
        isDiceRolling = true;

        yield return new WaitForSeconds(2f);

        diceAudio.Stop();
        isDiceRolling = false;

        int index = 0;

        if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
        {
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
            index = (System.DateTime.Now.Millisecond * UnityEngine.Random.Range(2, 8)) % fireRange;

            if (index == previousIndex || index == 0)
            {
                if (UnityEngine.Random.Range(0, 100) % 2 == 0)
                {
                    index = (index + 1) % fireRange;
                }
                else
                {
                    index = (index + 2) % fireRange;
                }
            }

            previousIndex = index;

            SyncDiceAnimation(new object[] { index, 2 });
        }
    }

    IEnumerator DiceAnimationForSonar()
    {
        dice60.SetActive(false);
        dice61.SetActive(false);
        dice40.SetActive(true);
        dice41.SetActive(true);
        dice40.GetComponent<Animator>().enabled = true;
        dice41.GetComponent<Animator>().enabled = true;
        moveButton.SetActive(false);
        fireButton.SetActive(false);
        setSubButton(false);
        sonarButton.SetActive(false);
        diceAudio.Play();
        selectedUnit.GetComponent<AudioSource>().Stop();
        isDiceRolling = true;

        yield return new WaitForSeconds(2f);

        diceAudio.Stop();
        isDiceRolling = false;

        int index0 = 0, index1 = 0;

        if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
        {
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
            int index = (System.DateTime.Now.Millisecond * UnityEngine.Random.Range(3, 8)) % 4;

            if (index == previousSonarIndex)
            {
                if (UnityEngine.Random.Range(0, 100) % 3 == 0)
                {
                    index = (index + 1) % 4;
                }
                else if (UnityEngine.Random.Range(0, 100) % 3 == 1)
                {
                    index = (index + 2) % 4;
                }
                else
                {
                    index = (index + 3) % 4;
                }
            }

            previousSonarIndex = index;
            index += 4;

            int x = UnityEngine.Random.Range(1, Math.Min(index - 1, 4) + 1);
            int y = index - x;
            if (x > 4)
            {
                x = 4;
                y = index - x;
            }
            else if (y > 4)
            {
                y = 4;
                x = index - y;
            }
            index0 = x;
            index1 = y;

            SyncDiceAnimation(new object[] { index0, 3, index1 });
        }
    }

    IEnumerator DiceAnimationForUFO(int a, int b)
    {
        diceAudio.Play();
        selectedUnit.GetComponent<AudioSource>().Stop();
        isDiceRolling = true;

        yield return new WaitForSeconds(2f);

        dice40.GetComponent<Animator>().enabled = false;
        dice40.GetComponent<Image>().sprite = dice4Sprites[a];
        dice61.GetComponent<Animator>().enabled = false;
        dice61.GetComponent<Image>().sprite = dice6Sprites[b];
        ufoDiceIndex1 = a;
        ufoDiceIndex2 = b;
        diceAudio.Stop();
        isDiceRolling = false;

        if (ufoDiceIndex1 == ufoDiceIndex2 && ShowUFOFireRange())
        {
            isUFOAction = true;
        }
        else
        {
            StartCoroutine(DelayShowInfoUFO());
            StartCoroutine(DelayShowInfoUFO());
            EndPlayAction();
        }
    }

    IEnumerator DelayToSink(GameObject selected)
    {
        isProgressAction = true;  
        selected.GetComponent<AudioSource>().clip = subAudios[0];
        selected.GetComponent<AudioSource>().loop = false;
        selected.GetComponent<AudioSource>().Play();
        selected.GetComponent<Animator>().enabled = false;
        selected.GetComponent<ForceInfo>().controlTimes = 2;

        yield return new WaitForSeconds(3f);

        selected.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

        yield return new WaitForSeconds(1f);

        ConvertToTile(selected).GetComponent<SpriteRenderer>().enabled = false;
        selected.transform.position = new Vector3(10000f, 10000f, -8f);
        selected.SetActive(false);
        isProgressAction = false;
    }

    IEnumerator DelayForShowingPinkBorder(GameObject transportedUnit)
    {
        transportedUnit.transform.GetChild(2).gameObject.SetActive(true);
        transportedUnit.GetComponent<Animator>().enabled = false;
        transportedUnit.transform.localScale = new Vector3(0.15f, 0.15f, 1f);
        yield return new WaitForSeconds(3f);
        transportedUnit.transform.localScale = new Vector3(0.1f, 0.1f, 1f);

        if (transportedUnit.GetComponent<ForceInfo>().controlTimes != 2
            && (whoseTurn == (int)TurnIndex.Player1 && players[0].GetComponent<PlayerInfo>().playTimes != 4 ||
                whoseTurn == (int)TurnIndex.Player2 && players[1].GetComponent<PlayerInfo>().playTimes != 4))
        {
            transportedUnit.GetComponent<Animator>().enabled = true;
        }
             
        yield return new WaitForSeconds(7f);
        transportedUnit.transform.GetChild(2).gameObject.SetActive(false);
    }

    IEnumerator DelayShowInfoWarning()
    {
        infoWarning.SetActive(true);

        yield return new WaitForSeconds(5f);

        infoWarning.SetActive(false);
    }

    IEnumerator DelayShowInfoUFO()
    {
        infoPanelForUFO.SetActive(true);

        yield return new WaitForSeconds(1.8f);

        infoPanelForUFO.SetActive(false);
        isUFOMoving = false;
    }

    IEnumerator DelayToShowInfoBermuda()
    {
        infoBermudaAction.SetActive(true);

        yield return new WaitForSeconds(3f);

        infoBermudaAction.SetActive(false);

        StartCoroutine(DelayToPauseTimer());
        // triangle video
        videoPlayerBT[videoIndexBT - 1].SetActive(true);
        RenderTexture.active = videoPlayerBT[videoIndexBT - 1].GetComponent<VideoPlayer>().targetTexture;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;
        videoPlayerBT[videoIndexBT - 1].GetComponent<VideoPlayer>().Play();

        yield return new WaitForSeconds(8f);

        videoPlayerBT[videoIndexBT - 1].GetComponent<VideoPlayer>().Stop();
        videoPlayerBT[videoIndexBT - 1].SetActive(false);

        // move transported unit and triangle
        switch (bermudaInfos[0])
        {
            case 0:
                {
                    force1ForCircle[bermudaInfos[1]].transform.position = tiles[bermudaInfos[3]].transform.position;
                    bermudaTriangles[bermudaInfos[2]].transform.position = new Vector3(20000f, 20000f, 0);
                    break;
                }
            case 1:
                {
                    force1ForSquare[bermudaInfos[1]].transform.position = tiles[bermudaInfos[3]].transform.position;
                    bermudaTriangles[bermudaInfos[2]].transform.position = new Vector3(20000f, 20000f, 0);
                    break;
                }
            case 2:
                {
                    force2ForCircle[bermudaInfos[1]].transform.position = tiles[bermudaInfos[3]].transform.position;
                    bermudaTriangles[bermudaInfos[2]].transform.position = new Vector3(20000f, 20000f, 0);
                    break;
                }
            case 3:
                {
                    force2ForSquare[bermudaInfos[1]].transform.position = tiles[bermudaInfos[3]].transform.position;
                    bermudaTriangles[bermudaInfos[2]].transform.position = new Vector3(20000f, 20000f, 0);
                    break;
                }
        }
    
        StartCoroutine(DelayForShowingPinkBorder(transportUnit));

        // Special effects after transporting
        if (transportUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Tank)
        {
            cameraView.transform.position = new Vector3(transportUnit.transform.position.x, transportUnit.transform.position.y, cameraView.transform.position.z);

            if (ConvertToTile(transportUnit).GetComponent<CellInfo>().cellType == (int)CellType.Mountain)
            {
                transportUnit.GetComponent<SpeciaInfo>().specialType = (int)SpecialType.Bunker;
                transportUnit.GetComponent<SpeciaInfo>().bunkerCount = 2;
                transportUnit.GetComponent<AudioSource>().clip = transportAudios[0];
                transportUnit.GetComponent<AudioSource>().loop = false;
                transportUnit.GetComponent<AudioSource>().Play();
                bermudaTextAfter.text = "Tank crashed on mountain";
            }
            else if (ConvertToTile(transportUnit).GetComponent<CellInfo>().cellType == (int)CellType.Sea)
            {
                // Disappear
                transportUnit.GetComponent<AudioSource>().clip = transportAudios[1];
                transportUnit.GetComponent<AudioSource>().loop = false;
                transportUnit.GetComponent<AudioSource>().Play();
                StartCoroutine(DelayToSink(transportUnit));
                bermudaTextAfter.text = "Tank relocated in water and sank";
            }
            else
            {
                bermudaTextAfter.text = "Tank relocated on land";
            }    
        }
        else if (transportUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.General)
        {
            cameraView.transform.position = new Vector3(transportUnit.transform.position.x, transportUnit.transform.position.y, cameraView.transform.position.z);

            if (ConvertToTile(transportUnit).GetComponent<CellInfo>().cellType == (int)CellType.Mountain)
            {
                transportUnit.GetComponent<SpeciaInfo>().specialType = (int)SpecialType.Bunker;
                transportUnit.GetComponent<SpeciaInfo>().bunkerCount = 2;
                transportUnit.GetComponent<AudioSource>().clip = transportAudios[0];
                transportUnit.GetComponent<AudioSource>().loop = false;
                transportUnit.GetComponent<AudioSource>().Play();
                bermudaTextAfter.text = "General crash landed on mountain";
            }
            else if (ConvertToTile(transportUnit).GetComponent<CellInfo>().cellType == (int)CellType.Sea)
            {
                bermudaTextAfter.text = "General relocated on water";
            }
            else
            {
                bermudaTextAfter.text = "General relocated on land";
            }
        }
        else if (transportUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
        {
            if (ConvertToTile(transportUnit).GetComponent<CellInfo>().cellType == (int)CellType.Mountain ||
                ConvertToTile(transportUnit).GetComponent<CellInfo>().cellType == (int)CellType.Land)
            {
                transportUnit.GetComponent<SpeciaInfo>().specialType = (int)SpecialType.Bunker;
                transportUnit.GetComponent<SpeciaInfo>().bunkerCount = 2;
                transportUnit.SetActive(true);

                cameraView.transform.position = new Vector3(transportUnit.transform.position.x, transportUnit.transform.position.y, cameraView.transform.position.z);

                transportUnit.GetComponent<AudioSource>().clip = transportAudios[0];
                transportUnit.GetComponent<AudioSource>().loop = false;
                transportUnit.GetComponent<AudioSource>().Play();

                if (ConvertToTile(transportUnit).GetComponent<CellInfo>().cellType == (int)CellType.Mountain)
                {
                    bermudaTextAfter.text = "Sub crashed on mountain";
                }
                else
                {
                    bermudaTextAfter.text = "Sub crashed on land";
                }
            }
            else
            {
                if((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame))
                {
                    cameraView.transform.position = new Vector3(transportUnit.transform.position.x, transportUnit.transform.position.y, cameraView.transform.position.z);
                }

                transportUnit.GetComponent<AudioSource>().clip = transportAudios[1];
                transportUnit.GetComponent<AudioSource>().loop = false;
                transportUnit.GetComponent<AudioSource>().Play();
                bermudaTextAfter.text = "Sub relocated in water";
            }
        }
        else if (transportUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Plane)
        {
            cameraView.transform.position = new Vector3(transportUnit.transform.position.x, transportUnit.transform.position.y, cameraView.transform.position.z);

            if (ConvertToTile(transportUnit).GetComponent<CellInfo>().cellType == (int)CellType.Mountain)
            {
                bermudaTextAfter.text = "Jet relocated over mountain";
            }
            else if (ConvertToTile(transportUnit).GetComponent<CellInfo>().cellType == (int)CellType.Sea)
            {
                bermudaTextAfter.text = "Jet relocated over water";
            }
            else
            {
                bermudaTextAfter.text = "Jet relocated over land";
            }
        }

        bermudaTextAfter.gameObject.SetActive(true);

        yield return new WaitForSeconds(3f);

        bermudaTextAfter.gameObject.SetActive(false);        

        // Check mines actions
        CheckMinesAction(1);
        isProgressAction = false;

        if (whoseTurn == (int)TurnIndex.Player1 && players[0].GetComponent<PlayerInfo>().playTimes == 0 ||
            whoseTurn == (int)TurnIndex.Player2 && players[1].GetComponent<PlayerInfo>().playTimes == 0)
        {
            if (isIncreasedByBermuda)
            {                
                StartCoroutine(DelayTurn());
            }
        }

        if (bootingObject.GetComponent<Image>().fillAmount <= 0f && isShowWinning == false)
        {
            if ((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame))
            {
                if (isIncreasedByBermuda)
                {
                    isShowWinning = true;                 
                    EndTurn();
                }
            }
        }

        isIncreasedByBermuda = false;
    }

    IEnumerator DelayToFinishBermuda(int tileMineIndex, float bermudaTime)
    {
        yield return new WaitForSeconds(bermudaTime);

        infoAtomicMines.SetActive(true);

        yield return new WaitForSeconds(3f);

        infoAtomicMines.SetActive(false);

        StartMinesExplosion(tileMineIndex, minesInfos[0]);
    }

    IEnumerator DelayToShowInfoAtomicMines(int indexTile)
    {
        yield return new WaitForSeconds(3f);
        
        // Show huge explosion image
        GameObject cloneExplosionHuge = new GameObject();

        if (expandExplosionIndex == 0)
        {
            cloneExplosionHuge = Instantiate(explosionTileHuge[0], tiles[indexTile].transform.position, Quaternion.identity);
        }
        else if (expandExplosionIndex == 1)
        {
            cloneExplosionHuge = Instantiate(explosionTileHuge[1], tiles[indexTile].transform.position, Quaternion.identity);
        }
        else if (expandExplosionIndex == 2)
        {
            cloneExplosionHuge = Instantiate(explosionTileHuge[2], tiles[indexTile].transform.position, Quaternion.identity);
        }

        cloneExplosionHuge.GetComponent<SpriteRenderer>().sortingOrder = 4;
        Destroy(cloneExplosionHuge, 3f);

        yield return new WaitForSeconds(3f);
        remainingExplosion--;

        ExplodeAction(indexTile - 31 * (expandExplosionIndex + 1));
        fireParticle = Instantiate(firePrefab, tiles[indexTile].transform.position - new Vector3(0, 0, 1f), Quaternion.identity);
        fireParticle.Play();
        soundEffect.PlayOneShot(soundEffect.clip);

        yield return new WaitForSeconds(0.5f);

        // Show explosion video
        if (chainExplosionIndex >= 2 && !isFinished)
        {
            StartCoroutine(DelayToPauseTimer());
            videoPlayer.SetActive(true);        
            RenderTexture.active = videoPlayer.GetComponent<VideoPlayer>().targetTexture;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = null;
            videoPlayer.GetComponent<VideoPlayer>().Play();

            yield return new WaitForSeconds(1f);
            warningText.SetActive(true);

            yield return new WaitForSeconds(8f);

            warningText.SetActive(false);
            videoPlayer.GetComponent<VideoPlayer>().Stop();
            videoPlayer.SetActive(false);            
        }

        MakeGroundZero();
    }

    IEnumerator DelaytoShowBermudaActivation()
    {
        // show bermuda info
        bermudaText.text = "Atomic mine has entered into a Bermuda Triangle.";
        infoBermudaAction.SetActive(true);
        isProgressAction = true;
        isActivatedBermudaBT = true;

        yield return new WaitForSeconds(3f); 

        infoBermudaAction.SetActive(false);

        // bermuda mines video
        StartCoroutine(DelayToPauseTimer());
        videoPlayerBT[4].SetActive(true);    
        RenderTexture.active = videoPlayerBT[4].GetComponent<VideoPlayer>().targetTexture;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;
        videoPlayerBT[4].GetComponent<VideoPlayer>().Play();

        yield return new WaitForSeconds(7f);

        videoPlayerBT[4].SetActive(false);
        videoPlayerBT[4].GetComponent<VideoPlayer>().Stop();

        // show bermuda text after video
        //bermudaTextAfter.text = "ATOMIC MINE EXPLOSION ACTIVATED A TRIANGLE!";
        //bermudaTextAfter.gameObject.SetActive(true);        

        // Explosion again
        startTiles = new List<int>();

        for (int i = 0; i < bermudaTiles.Count; i++)
        {
            bermudaTriangles[bermudaTiles[i]].transform.position = new Vector3(20000f, 20000f, 0);
        }

        if (bermudaTiles.Count >= 1)
            cameraView.transform.position = new Vector3(tiles[minesInfos[1] + 6].transform.position.x, tiles[minesInfos[1] + 6].transform.position.y, cameraView.transform.position.z);

        //yield return new WaitForSeconds(3f);
        //bermudaTextAfter.gameObject.SetActive(false);

        bermudaTiles = new List<int>();
        bermudaTilesCount = new List<int>();

        StartMinesExplosion(minesInfos[1], 30);

        if (minesInfos[1] % 30 > 20)
        {
            minesInfos[1] -= 63;
        }
        else if(minesInfos[1] % 30 <= 10)
        {
            minesInfos[1] += 63;
        }
    }

    IEnumerator DelayToPlaceSpecialThings()
    {
        yield return new WaitForSeconds(5f);

        for (int i = 0; i < MainUI.gameOptions.Length; i++)
        {
            if (MainUI.gameOptions[i] == '^')
            {
                PlaceTriangles();
                break;
            }
        }

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < MainUI.gameOptions.Length; i++)
        {
            if (MainUI.gameOptions[i] == '*')
            {
                PlaceMines();
                break;
            }
        }
    }

    IEnumerator DelayToSubHit(int index1, int index2)
    {
        if (index2 == 0)
        {
            force1ForCircle[index1].SetActive(true);
            force1ForCircle[index1].transform.localScale = new Vector3(.15f, .15f, .15f);
            yield return new WaitForSeconds(3f);

            // Do Operation Serpent
            if (force1ForCircle[index1].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                OperationSerpent(1);
            }

            fireParticle = Instantiate(firePrefab, force1ForCircle[index1].transform.position, Quaternion.identity);
            fireParticle.Play();
            force1ForCircle[index1].transform.position = new Vector3(10000f, 10000f, -8f);
            force1ForCircle[index1].SetActive(false);
        }
        else if (index2 == 1)
        {
            force1ForSquare[index1].SetActive(true);
            force1ForSquare[index1].transform.localScale = new Vector3(.15f, .15f, .15f);
            yield return new WaitForSeconds(3f);

            // Do Operation Serpent
            if (force1ForSquare[index1].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                OperationSerpent(1);
            }

            fireParticle = Instantiate(firePrefab, force1ForSquare[index1].transform.position, Quaternion.identity);
            fireParticle.Play();
            force1ForSquare[index1].transform.position = new Vector3(10000f, 10000f, -8f);
            force1ForSquare[index1].SetActive(false);
        }
        else if (index2 == 2)
        {
            force2ForCircle[index1].SetActive(true);
            force2ForCircle[index1].transform.localScale = new Vector3(.15f, .15f, .15f);
            yield return new WaitForSeconds(3f);

            // Do Operation Serpent
            if (force2ForCircle[index1].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                OperationSerpent(2);
            }

            fireParticle = Instantiate(firePrefab, force2ForCircle[index1].transform.position, Quaternion.identity);
            fireParticle.Play();
            force2ForCircle[index1].transform.position = new Vector3(10000f, 10000f, -8f);
            force2ForCircle[index1].SetActive(false);
        }
        else if (index2 == 3)
        {
            force2ForSquare[index1].SetActive(true);
            force2ForSquare[index1].transform.localScale = new Vector3(.15f, .15f, .15f);
            yield return new WaitForSeconds(3f);

            // Do Operation Serpent
            if (force2ForSquare[index1].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                OperationSerpent(2);
            }

            fireParticle = Instantiate(firePrefab, force2ForSquare[index1].transform.position, Quaternion.identity);
            fireParticle.Play();
            force2ForSquare[index1].transform.position = new Vector3(10000f, 10000f, -8f);
            force2ForSquare[index1].SetActive(false);
        }
    }

    IEnumerator DelayShowInfoGamePlay(int index)
    {
        if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
        {
            if (((whoseTurn == (int)TurnIndex.Player1 && !isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && isMasterInGame)))
            {
                if (index == 0)
                {
                    infoSubAction.SetActive(true);
                    infoSubAction.GetComponent<AudioSource>().clip = subAudios[0];
                    infoSubAction.GetComponent<AudioSource>().loop = false;
                    infoSubAction.GetComponent<AudioSource>().Play();
                }
                else if (index == 1)
                {

                    infoSubAction.SetActive(true);
                    infoSubAction.GetComponent<AudioSource>().clip = subAudios[1];
                    infoSubAction.GetComponent<AudioSource>().loop = false;
                    infoSubAction.GetComponent<AudioSource>().Play();
                }
                else if (index == 2)
                {

                    infoSubAction.SetActive(true);
                    infoSubAction.GetComponent<AudioSource>().clip = subAudios[1];
                    infoSubAction.GetComponent<AudioSource>().loop = false;
                    infoSubAction.GetComponent<AudioSource>().Play();
                }
            }
        }

        infoGamePlayPanel.SetActive(true);

        yield return new WaitForSeconds(3f);

        infoGamePlayPanel.SetActive(false);
        infoSubAction.SetActive(false);
        subPing.SetActive(false);
        isPing = false;
    }

    IEnumerator DelayToShowEndplay()
    {
        infoGamePlayPanel.SetActive(true);
        infoGamePlayPanel.GetComponentInChildren<Text>().color = Color.yellow;
        infoGamePlayPanel.GetComponentInChildren<Text>().text = "END PLAY";
        yield return new WaitForSeconds(2f);
        infoGamePlayPanel.SetActive(false);
    }

    IEnumerator DelayToPauseTimer()
    {
        bootingObject.GetComponent<GameSettingsApplier>().setTimerActive(false);
        yield return new WaitForSeconds(7f);
        bootingObject.GetComponent<GameSettingsApplier>().setTimerActive(true);

    }

    IEnumerator DelayToCheckPlayerLeftRoom()
    {
        yield return new WaitForSeconds(2f);
        reconnectingText.SetActive(false);
        isDisableMouseClicks = false;

        if (playersInRoom < 2)
        {
            int winCount = PlayerPrefs.GetInt("WIN_COUNT");
            winCount++;
            PlayerPrefs.SetInt("WIN_COUNT", winCount);

            if (MainUI.isListedRound)
            {
                PlayerPrefs.SetInt("ROUND", PlayerPrefs.GetInt("ROUND") + 1);
            }

            StartCoroutine(PostUserInfos(MainUI.frontURL + "/api/savePlaycount"));
            winningDialog.SetActive(true);

            StartCoroutine(DelayToEndGame());
        }
    }

    IEnumerator FirstRollAnimation(int a1, int a2, int b1, int b2)
    {
        //print($"{a1}, {a2}, {b1}, {b2}");
        diceAudio.Play();
        diceAudio.GetComponent<AudioSource>().loop = true;
        infoPanel.SetActive(true);
        infoPanel.GetComponentInChildren<Text>().text = "Rolling to see who goes first!\r\nFIRST ROLL!";

        turnDiceTop.transform.GetChild(3).GetComponent<Text>().text = MainUI.playerNames[0];
        turnDiceBottom.transform.GetChild(3).GetComponent<Text>().text = MainUI.playerNames[1];

        turnDiceTop.SetActive(true);
        turnDiceBottom.SetActive(true);

        yield return new WaitForEndOfFrame();

        turnDiceBottom.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
        turnDiceBottom.transform.GetChild(0).GetChild(0).GetComponent<Animator>().enabled = true;
        yield return new WaitForEndOfFrame();
        turnDiceBottom.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
        turnDiceBottom.transform.GetChild(1).GetChild(0).GetComponent<Animator>().enabled = true;
        yield return new WaitForEndOfFrame();

        turnDiceTop.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
        turnDiceTop.transform.GetChild(0).GetChild(0).GetComponent<Animator>().enabled = true;
        yield return new WaitForEndOfFrame();
        turnDiceTop.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
        turnDiceTop.transform.GetChild(1).GetChild(0).GetComponent<Animator>().enabled = true;

        yield return new WaitForSeconds(4f);

        diceAudio.GetComponent<AudioSource>().loop = false;
        diceAudio.Stop();
        turnDiceTop.transform.GetChild(0).GetChild(0).GetComponent<Animator>().enabled = false;
        turnDiceTop.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = dice6Sprites[a1 - 1];
        yield return new WaitForEndOfFrame();
        turnDiceTop.transform.GetChild(1).GetChild(0).GetComponent<Animator>().enabled = false;
        turnDiceTop.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = dice6Sprites[a2 - 1];
        yield return new WaitForEndOfFrame();

        turnDiceBottom.transform.GetChild(0).GetChild(0).GetComponent<Animator>().enabled = false;
        turnDiceBottom.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = dice6Sprites[b1 - 1];
        yield return new WaitForEndOfFrame();
        turnDiceBottom.transform.GetChild(1).GetChild(0).GetComponent<Animator>().enabled = false;
        turnDiceBottom.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = dice6Sprites[b2 - 1];
        yield return new WaitForEndOfFrame();

        if (a1 + a2 > b1 + b2)
        {
            infoPanel.GetComponentInChildren<Text>().text = MainUI.playerNames[0] + " FIRST ROLL!";
        }
        else
        {
            infoPanel.GetComponentInChildren<Text>().text = MainUI.playerNames[1] + " FIRST ROLL!";
        }

        // show bermuda triangles
        if (isMasterInGame)
        {
            for (int i = 0; i < bermudaTriangles.Length / 2; i++)
            {
                bermudaTriangles[i + 10].SetActive(false);
                atomicMines[i + 10].SetActive(false);
            }
        }
        else if (!isMasterInGame)
        {
            for (int i = 0; i < bermudaTriangles.Length / 2; i++)
            {
                bermudaTriangles[i].SetActive(false);
                atomicMines[i].SetActive(false);
            }
        }

        for (int i = 0; i < bermudaTriangles.Length; i++)
        {
            atomicMines[i].transform.GetChild(0).gameObject.SetActive(false);
            bermudaTriangles[i].transform.GetChild(0).gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(2.5f);

        turnDiceBottom.SetActive(false);
        turnDiceTop.SetActive(false);

        infoPanel.SetActive(false);
        startDialog.SetActive(false);
        emojiBtn.SetActive(true);

        if (isMasterInGame)
        {
            if (a1 + a2 > b1 + b2)
            {
                Debug.Log("First roll animation turn 0 ");
                SyncTurn(new object[] { 0 });
            }
            else
            {
                Debug.Log("First roll animation turn 1");

                SyncTurn(new object[] { 1 });
            }
        }
    }

    int CalculateTileIndex(int j, int k)
    {
        int tileIndex;

        if (j < 0)
        {
            tileIndex = (30 * (30 + j));
        }
        else if (j >= 30)
        {
            tileIndex = (30 * (j - 30));
        }
        else
        {
            tileIndex = 30 * j;
        }

        if (k < 0)
        {
            tileIndex += (30 + k);
        }
        else if (k >= 30)
        {
            tileIndex += (k - 30);
        }
        else
        {
            tileIndex += k;
        }

        return tileIndex;
    }

    public void ShowFireRange(int rangeIndex)
    {
        mountains = new();

        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            target1ForCircle[i].SetActive(false);
            target1ForSquare[i].SetActive(false);
            target2ForCircle[i].SetActive(false);
            target2ForSquare[i].SetActive(false);
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            if (Vector3.Distance(selectedUnit.transform.position, tiles[i].transform.position) < 0.1f)
            {
                int selectedX = i % 30;
                int selectedY = i / 30;

                int xMinRange = selectedX - rangeIndex - 1, xMaxRange = selectedX + rangeIndex + 1;
                int yMinRange = selectedY - rangeIndex - 1, yMaxRange = selectedY + rangeIndex + 1;

                for (int j = yMinRange; j <= yMaxRange; j++)
                {
                    for (int k = xMinRange; k <= xMaxRange; k++)
                    {
                        int tileIndex = CalculateTileIndex(j, k);

                        if (tiles[tileIndex].GetComponent<CellInfo>().cellType == (int)CellType.Mountain)
                        {
                            mountains.Add(tileIndex);
                        }
                    }
                }

                for (int j = yMinRange; j <= yMaxRange; j++)
                {
                    for (int k = xMinRange; k <= xMaxRange; k++)
                    {
                        int tileIndex = CalculateTileIndex(j, k);

                        if ((selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.General) ||
                               (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && (tiles[tileIndex].GetComponent<CellInfo>().cellType != (int)CellType.Mountain)) ||
                               (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Tank && (tiles[tileIndex].GetComponent<CellInfo>().cellType != (int)CellType.Mountain)) ||
                               (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.UFO) ||
                               (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Plane)
                               )
                        {
                            int tileX = tileIndex % 30;
                            int tileY = tileIndex / 30;

                            if (IsForceForFire(tiles[tileIndex]) == false)
                            {
                                if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Plane || selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.UFO || selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.General)
                                {
                                    fireColor = Color.red;
                                    fireColor.a = 0.7f;
                                    tiles[tileIndex].GetComponent<SpriteRenderer>().enabled = true;
                                    tiles[tileIndex].GetComponent<SpriteRenderer>().color = fireColor;
                                    FindFireEnemy(tileIndex);
                                }
                                else
                                {
                                    if (j >= 0 && j < 30 && k >= 0 && k < 30)
                                    {
                                        if (IsMTNStrategy(tiles[tileIndex], rangeIndex, selectedX, selectedY, tileX, tileY) == false)
                                        {
                                            if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
                                            {
                                                if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
                                                {
                                                    fireColor = Color.red;
                                                    fireColor.a = 0.7f;
                                                    tiles[tileIndex].GetComponent<SpriteRenderer>().enabled = true;
                                                    tiles[tileIndex].GetComponent<SpriteRenderer>().color = fireColor;
                                                    FindFireEnemy(tileIndex);
                                                }
                                                else
                                                {
                                                    fireColor = Color.red;
                                                    fireColor.a = 0f;
                                                    tiles[tileIndex].GetComponent<SpriteRenderer>().enabled = true;
                                                    tiles[tileIndex].GetComponent<SpriteRenderer>().color = fireColor;
                                                    FindFireEnemy(tileIndex);
                                                }
                                            }
                                            else
                                            {
                                                fireColor = Color.red;
                                                fireColor.a = 0.7f;
                                                tiles[tileIndex].GetComponent<SpriteRenderer>().enabled = true;
                                                tiles[tileIndex].GetComponent<SpriteRenderer>().color = fireColor;
                                                FindFireEnemy(tileIndex);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (IsMTNStrategy(tiles[tileIndex], rangeIndex, selectedX, selectedY, tileX, tileY) == false)
                                        {
                                            if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
                                            {
                                                if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
                                                {
                                                    fireColor = Color.red;
                                                    fireColor.a = 0.7f;
                                                    tiles[tileIndex].GetComponent<SpriteRenderer>().enabled = true;
                                                    tiles[tileIndex].GetComponent<SpriteRenderer>().color = fireColor;
                                                    FindFireEnemy(tileIndex);
                                                }
                                                else
                                                {
                                                    fireColor = Color.red;
                                                    fireColor.a = 0f;
                                                    tiles[tileIndex].GetComponent<SpriteRenderer>().enabled = true;
                                                    tiles[tileIndex].GetComponent<SpriteRenderer>().color = fireColor;
                                                    FindFireEnemy(tileIndex);
                                                }
                                            }
                                            else
                                            {
                                                fireColor = Color.red;
                                                fireColor.a = 0.7f;
                                                tiles[tileIndex].GetComponent<SpriteRenderer>().enabled = true;
                                                tiles[tileIndex].GetComponent<SpriteRenderer>().color = fireColor;
                                                FindFireEnemy(tileIndex);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (currentTurnIndex != whoseTurn)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].GetComponent<SpriteRenderer>().enabled = false;
            }

            endPlayButton.SetActive(false);

            for (int i = 0; i < target1ForCircle.Length; i++)
            {
                target1ForCircle[i].SetActive(false);
                target1ForSquare[i].SetActive(false);
                target2ForCircle[i].SetActive(false);
                target2ForSquare[i].SetActive(false);
            }
        }
    }

    public bool ShowUFOFireRange()
    {
        bool isAnythingInRange = false;

        for (int i = 0; i < tiles.Length; i++)
        {
            if (whoseTurn == (int)TurnIndex.Player1)
            {
                for (int j = 0; j < force2ForCircle.Length; j++)
                {
                    if (Vector3.Distance(force2ForCircle[j].transform.position, tiles[i].transform.position) < 0.1f)
                    {
                        if (force2ForCircle[j].GetComponent<ForceInfo>().forceType == (int)ForceType.Plane && tiles[i].GetComponent<CellInfo>().cellType == (int)CellType.Mountain)
                        {
                            isAnythingInRange = true;
                            fireColor = Color.red;
                            fireColor.a = 0.7f;
                            tiles[i].GetComponent<SpriteRenderer>().enabled = true;
                            tiles[i].GetComponent<SpriteRenderer>().color = fireColor;
                        }
                    }

                    if (Vector3.Distance(force2ForSquare[j].transform.position, tiles[i].transform.position) < 0.1f)
                    {
                        if (force2ForSquare[j].GetComponent<ForceInfo>().forceType == (int)ForceType.Plane && tiles[i].GetComponent<CellInfo>().cellType == (int)CellType.Mountain)
                        {
                            isAnythingInRange = true;
                            fireColor = Color.red;
                            fireColor.a = 0.7f;
                            tiles[i].GetComponent<SpriteRenderer>().enabled = true;
                            tiles[i].GetComponent<SpriteRenderer>().color = fireColor;
                        }
                    }
                }
            }
            else if (whoseTurn == (int)TurnIndex.Player2)
            {
                for (int j = 0; j < force1ForCircle.Length; j++)
                {
                    if (Vector3.Distance(force1ForCircle[j].transform.position, tiles[i].transform.position) < 0.1f)
                    {
                        if (force1ForCircle[j].GetComponent<ForceInfo>().forceType == (int)ForceType.Plane && tiles[i].GetComponent<CellInfo>().cellType == (int)CellType.Mountain)
                        {
                            isAnythingInRange = true;
                            fireColor = Color.red;
                            fireColor.a = 0.7f;
                            tiles[i].GetComponent<SpriteRenderer>().enabled = true;
                            tiles[i].GetComponent<SpriteRenderer>().color = fireColor;
                        }
                    }

                    if (Vector3.Distance(force1ForSquare[j].transform.position, tiles[i].transform.position) < 0.1f)
                    {
                        if (force1ForSquare[j].GetComponent<ForceInfo>().forceType == (int)ForceType.Plane && tiles[i].GetComponent<CellInfo>().cellType == (int)CellType.Mountain)
                        {
                            isAnythingInRange = true;
                            fireColor = Color.red;
                            fireColor.a = 0.7f;
                            tiles[i].GetComponent<SpriteRenderer>().enabled = true;
                            tiles[i].GetComponent<SpriteRenderer>().color = fireColor;
                        }
                    }
                }
            }
        }

        return isAnythingInRange;
    }

    public bool IsForceForMove(GameObject cell)
    {
        isForceForMove = false;

        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            if ((Vector3.Distance(cell.transform.position, force1ForCircle[i].transform.position) < 0.1f) ||
                (Vector3.Distance(cell.transform.position, force2ForCircle[i].transform.position) < 0.1f) ||
                (Vector3.Distance(cell.transform.position, force1ForSquare[i].transform.position) < 0.1f) ||
                (Vector3.Distance(cell.transform.position, force2ForSquare[i].transform.position) < 0.1f)
                )
            {
                isForceForMove = true;
            }
        }

        return isForceForMove;
    }

    public bool IsForceForSub(GameObject cell)
    {
        isForceForSub = false;

        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            if ((Vector3.Distance(cell.transform.position, force1ForCircle[i].transform.position) < 0.1f && force1ForCircle[i].activeSelf == true) ||
                (Vector3.Distance(cell.transform.position, force2ForCircle[i].transform.position) < 0.1f && force2ForCircle[i].activeSelf == true) ||
                (Vector3.Distance(cell.transform.position, force1ForSquare[i].transform.position) < 0.1f && force1ForSquare[i].activeSelf == true) ||
                (Vector3.Distance(cell.transform.position, force2ForSquare[i].transform.position) < 0.1f && force2ForSquare[i].activeSelf == true)
                )
            {
                isForceForSub = true;

                if (whoseTurn == (int)TurnIndex.Player1)
                {
                    if (force2ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub || force2ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
                    {
                        isForceForSub = false;
                    }
                }
                else
                {
                    if (force1ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub || force1ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
                    {
                        isForceForSub = false;
                    }
                }
            }
        }

        return isForceForSub;
    }

    public bool IsForceForFire(GameObject cell)
    {
        isForceForFire = false;

        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            if (whoseTurn == (int)TurnIndex.Player1)
            {
                if (Vector3.Distance(cell.transform.position, force1ForCircle[i].transform.position) < 0.1f
                    && force1ForCircle[i].activeSelf == true
                )
                {
                    isForceForFire = true;
                }

                if (
                Vector3.Distance(cell.transform.position, force1ForSquare[i].transform.position) < 0.1f
                 && force1ForSquare[i].activeSelf == true
                )
                {
                    isForceForFire = true;
                }

                if (
                Vector3.Distance(cell.transform.position, force2ForCircle[i].transform.position) < 0.1f
                && force2ForCircle[i].activeSelf == true
                )
                {
                    //target2ForCircle[i].SetActive(true);
                    cell.GetComponent<SpriteRenderer>().enabled = false;
                }

                if (
                Vector3.Distance(cell.transform.position, force2ForSquare[i].transform.position) < 0.1f
                && force2ForSquare[i].activeSelf == true
                )
                {
                    //target2ForSquare[i].SetActive(true);
                    cell.GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            else if (whoseTurn == (int)TurnIndex.Player2)
            {
                if (Vector3.Distance(cell.transform.position, force2ForCircle[i].transform.position) < 0.1f
                    && force2ForCircle[i].activeSelf == true
                    )
                {
                    isForceForFire = true;
                }

                if (
                    Vector3.Distance(cell.transform.position, force2ForSquare[i].transform.position) < 0.1f
                    && force2ForSquare[i].activeSelf == true
                    )
                {
                    isForceForFire = true;
                }

                if (
                Vector3.Distance(cell.transform.position, force1ForCircle[i].transform.position) < 0.1f
                && force1ForCircle[i].activeSelf == true
                )
                {
                    //target1ForCircle[i].SetActive(true);
                    cell.GetComponent<SpriteRenderer>().enabled = false;
                }

                if (
                Vector3.Distance(cell.transform.position, force1ForSquare[i].transform.position) < 0.1f
                && force1ForSquare[i].activeSelf == true
                )
                {
                    //target1ForSquare[i].SetActive(true);
                    cell.GetComponent<SpriteRenderer>().enabled = false;
                }
            }
        }

        return isForceForFire;
    }

    public bool IsForceForSonar(GameObject cell)
    {
        isForceForSonar = false;

        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            if (whoseTurn == (int)TurnIndex.Player1)
            {
                if (
                Vector3.Distance(cell.transform.position, force2ForCircle[i].transform.position) < 0.1f
                && force2ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub
                && cell.GetComponent<SpriteRenderer>().enabled == true
                )
                {
                    isForceForSonar = true;
                    force2ForCircle[i].SetActive(true);
                }

                if (
                Vector3.Distance(cell.transform.position, force2ForSquare[i].transform.position) < 0.1f
                && force2ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub
                && cell.GetComponent<SpriteRenderer>().enabled == true
                )
                {
                    isForceForSonar = true;
                    force2ForSquare[i].SetActive(true);
                }
            }
            else if (whoseTurn == (int)TurnIndex.Player2)
            {
                if (
                Vector3.Distance(cell.transform.position, force1ForCircle[i].transform.position) < 0.1f
                && force1ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub
                && cell.GetComponent<SpriteRenderer>().enabled == true
                )
                {
                    isForceForSonar = true;
                    force1ForCircle[i].SetActive(true);
                }

                if (
                Vector3.Distance(cell.transform.position, force1ForSquare[i].transform.position) < 0.1f
                && force1ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub
                && cell.GetComponent<SpriteRenderer>().enabled == true
                )
                {
                    isForceForSonar = true;
                    force1ForSquare[i].SetActive(true);
                }
            }
        }

        cell.GetComponent<SpriteRenderer>().enabled = false;

        return isForceForFire;
    }

    void CheckAndEnableEnemy(int index, GameObject enemy, GameObject target)
    {
        if (Vector3.Distance(tiles[index].transform.position, enemy.transform.position) < 0.1f && enemy.activeSelf == true)
        {
            Color targetColor = new Color32(255, 255, 255, 1);
            targetColor.a = 1f;
            target.GetComponent<SpriteRenderer>().color = targetColor;

            if ((whoseTurn == (int)TurnIndex.Player1 && !isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && isMasterInGame))
            {
                if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub ||
                   enemy.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
                {
                    targetColor.a = 0f;
                    target.GetComponent<SpriteRenderer>().color = targetColor;
                }

                if (enemy.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub || enemy.GetComponent<SpeciaInfo>().specialType == (int)SpecialType.Bunker)
                {
                    targetColor.a = 1f;
                    target.GetComponent<SpriteRenderer>().color = targetColor;
                }
            }

            //Current is sub
            if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                //Currently underwater
                if (selectedX >= 13 && selectedX <= 16 && selectedY >= 13 && selectedY <= 16)
                {
                    int indexX = index % 30, indexY = index / 30;

                    //Other object is also under water
                    if (indexX >= 13 && indexX <= 16 && indexY >= 13 && indexY <= 16)
                    {
                        //Other object is sub
                        if (enemy.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
                        {
                            target.SetActive(true);
                            tiles[index].GetComponent<SpriteRenderer>().enabled = false;
                        }
                    }
                    //Other object is not under water
                    else
                    {
                        if (selectedX == 13 && indexY == 13 && indexX <= 12 ||
                            selectedY == 13 && indexX == 13 && indexY >= 17 ||
                            selectedX == 16 && indexX >= 17 && indexX == 16 ||
                            selectedY == 16 && indexX == 16 && indexY <= 12)
                        {
                            target.SetActive(true);
                            tiles[index].GetComponent<SpriteRenderer>().enabled = false;
                        }

                        if (selectedX == 13 && selectedY == 15)
                        {
                            if (indexX >= 16 && indexX <= 19 && indexX + indexY == 28)
                            {
                                target.SetActive(true);
                                tiles[index].GetComponent<SpriteRenderer>().enabled = false;
                            }
                        }

                        if (selectedX == 14 && selectedY == 13)
                        {
                            if (indexY == 12 && indexX == 11 || indexY == 13 && indexX == 12)
                            {
                                target.SetActive(true);
                                tiles[index].GetComponent<SpriteRenderer>().enabled = false;
                            }
                        }

                        if (selectedX == 16 && selectedY == 14)
                        {
                            if (indexY == 17 && indexX == 13 || indexY == 18 && indexX == 12)
                            {
                                target.SetActive(true);
                                tiles[index].GetComponent<SpriteRenderer>().enabled = false;
                            }
                        }

                        if (selectedX == 15 && selectedY == 16)
                        {
                            if (indexX >= 17 && indexX <= 20 && indexX - indexY == 1)
                            {
                                target.SetActive(true);
                                tiles[index].GetComponent<SpriteRenderer>().enabled = false;
                            }
                        }
                    }
                }
                else
                {
                    target.SetActive(true);
                    tiles[index].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            else
            {
                if (enemy.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
                {
                    int indexX = index % 30, indexY = index / 30;

                    //Other object is not under water
                    if (!(indexX >= 13 && indexX <= 16 && indexY >= 13 && indexY <= 16))
                    {
                        target.SetActive(true);
                        tiles[index].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
                else
                {
                    target.SetActive(true);
                    tiles[index].GetComponent<SpriteRenderer>().enabled = false;
                }

                // if jet is away more than 3 from tank
                if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Tank)
                {
                    int indexX = index % 30, indexY = index / 30;

                    if (enemy.GetComponent<ForceInfo>().forceType == (int)ForceType.Plane)
                    {
                        if ((Mathf.Abs(selectedX - indexY) > 3) && (MathF.Abs(selectedX - indexY) < 10) || (MathF.Abs(selectedX - indexY) > 10) && (30 - MathF.Abs(selectedX - indexY) > 3) ||
                           (Mathf.Abs(selectedY - indexX) > 3) && (MathF.Abs(selectedY - indexX) < 10) || (MathF.Abs(selectedY - indexX) > 10) && (30 - MathF.Abs(selectedY - indexX) > 3))
                        {
                            target.SetActive(false);
                            tiles[index].GetComponent<SpriteRenderer>().enabled = true;
                        }
                    }
                }
            }
        }
    }

    public void FindFireEnemy(int index)
    {
        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            if (whoseTurn == (int)TurnIndex.Player1)
            {
                CheckAndEnableEnemy(index, force2ForCircle[i], target2ForCircle[i]);

                CheckAndEnableEnemy(index, force2ForSquare[i], target2ForSquare[i]);
            }
            else if (whoseTurn == (int)TurnIndex.Player2)
            {
                CheckAndEnableEnemy(index, force1ForCircle[i], target1ForCircle[i]);

                CheckAndEnableEnemy(index, force1ForSquare[i], target1ForSquare[i]);
            }
        }
    }

    public void StartMinesExplosion(int tileIndexInfo, int minesIndexInfo)
    {
        int checkChainIndex = chainExplosionIndex + 1;
        int checkExpandIndex = CalculateExpandIndex(checkChainIndex);

        tileIndicesInfo = new List<int>();
        minesIndicesInfo = new List<int>();
        startTiles = new List<int>();
        bool isDoubled = false;

        tileIndicesInfo.Add(tileIndexInfo);
        minesIndicesInfo.Add(minesIndexInfo);
        startTiles.Add(tileIndexInfo - 31 * (checkExpandIndex + 1));

        do
        {
            isDoubled = false;

            for (int k = 0; k < tileIndicesInfo.Count; k++)
            {
                tileIndexInfo = tileIndicesInfo[k];
                List<int> tileIndices = new List<int>();

                for (int i = 0; i < 3 + checkExpandIndex * 2; i++)
                {
                    for (int j = 0; j < 3 + checkExpandIndex * 2; j++)
                    {
                        tileIndices.Add(tileIndexInfo + i + j * 30 - 31 * (checkExpandIndex + 1));
                    }
                }

                for (int j = 0; j < tileIndices.Count; j++)
                {
                    for (int i = 0; i < atomicMines.Length; i++)
                    {
                        if (Vector3.Distance(atomicMines[i].transform.position, tiles[tileIndices[j]].transform.position) < 0.1f)
                        {
                            if (!minesIndicesInfo.Contains(i))
                            {
                                checkChainIndex++;
                                checkExpandIndex = CalculateExpandIndex(checkChainIndex);

                                tileIndicesInfo.Add(tileIndices[j]);
                                minesIndicesInfo.Add(i);
                                startTiles.Add(tileIndices[j] - 31 * (checkExpandIndex + 1));
                                isDoubled = true;
                            }
                        }
                    }
                }
            }
        } while (isDoubled);

        remainingExplosion = minesIndicesInfo.Count;
        ExplosionEffect(tileIndicesInfo[0], minesIndicesInfo[0]);
    }

    public void CheckMinesAction(int actionIndex)
    {
        if ((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame))
        {
            int randomIndexX = UnityEngine.Random.Range(7, 12);
            int randomIndexY = UnityEngine.Random.Range(7, 12);

            for (int i = 0; i < force1ForCircle.Length; i++)
            {
                for (int j = 0; j < atomicMines.Length; j++)
                {
                    if (Vector3.Distance(force1ForCircle[i].transform.position, atomicMines[j].transform.position) < 0.1f)
                    {
                        isProgressAction = true;
                        SyncMinesAction(new object[] { j, randomIndexX + randomIndexY * 30, actionIndex });
                    }

                    if (Vector3.Distance(force1ForSquare[i].transform.position, atomicMines[j].transform.position) < 0.1f)
                    {
                        isProgressAction = true;
                        SyncMinesAction(new object[] { j, randomIndexX + randomIndexY * 30, actionIndex });
                    }

                    if (Vector3.Distance(force2ForCircle[i].transform.position, atomicMines[j].transform.position) < 0.1f)
                    {
                        isProgressAction = true;
                        SyncMinesAction(new object[] { j, randomIndexX + randomIndexY * 30, actionIndex });
                    }

                    if (Vector3.Distance(force2ForSquare[i].transform.position, atomicMines[j].transform.position) < 0.1f)
                    {
                        isProgressAction = true;
                        SyncMinesAction(new object[] { j, randomIndexX + randomIndexY * 30, actionIndex });
                    }
                }
            }
        }
    }

    public int CalculateExpandIndex(int chainIndex)
    {
        int expandIndex = 0;

        if (chainIndex < 3)
        {
            expandIndex = 0;
        }
        else if (chainIndex == 3)
        {
            expandIndex = 1;
        }
        else if (chainIndex >= 4)
        {
            expandIndex = 2;
        }

        return expandIndex;
    }

    public void ShowMoveRange(int rangeIndex)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (Vector3.Distance(selectedUnit.transform.position, tiles[i].transform.position) < 0.1f)
            {
                int selectedTileIndex = i;

                int x1 = i % 30;
                int y1 = i / 30;

                int xMinRange = x1 - rangeIndex - 1, xMaxRange = x1 + rangeIndex + 1;
                int yMinRange = y1 - rangeIndex - 1, yMaxRange = y1 + rangeIndex + 1;

                for (int j = yMinRange; j <= yMaxRange; j++)
                {
                    for (int k = xMinRange; k <= xMaxRange; k++)
                    {
                        int tileIndex = CalculateTileIndex(j, k);

                        if (tileIndex != selectedTileIndex)
                        {
                            if ((selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.General && tiles[tileIndex].GetComponent<CellInfo>().cellType != (int)CellType.Mountain) ||
                               (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && (tiles[tileIndex].GetComponent<CellInfo>().cellType == (int)CellType.Sea || tiles[tileIndex].GetComponent<CellInfo>().cellType == (int)CellType.Bridge || tiles[tileIndex].GetComponent<CellInfo>().cellType == (int)CellType.Tunnel || tiles[tileIndex].GetComponent<CellInfo>().cellType == (int)CellType.TunnelEntrance)) ||
                               (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Tank && (tiles[tileIndex].GetComponent<CellInfo>().cellType == (int)CellType.Land || tiles[tileIndex].GetComponent<CellInfo>().cellType == (int)CellType.Bridge || tiles[tileIndex].GetComponent<CellInfo>().cellType == (int)CellType.Tunnel || tiles[tileIndex].GetComponent<CellInfo>().cellType == (int)CellType.TunnelEntrance)) ||
                               (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.UFO) ||
                               (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Plane)
                               )
                            {
                                if (IsForceForMove(tiles[tileIndex]) == false)
                                {
                                    if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
                                    {
                                        if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
                                        {
                                            moveColor = Color.blue;
                                            moveColor.a = 0.7f;
                                            tiles[tileIndex].GetComponent<SpriteRenderer>().enabled = true;
                                            tiles[tileIndex].GetComponent<SpriteRenderer>().color = moveColor;
                                        }
                                        else
                                        {
                                            moveColor = Color.blue;
                                            moveColor.a = 0f;
                                            tiles[tileIndex].GetComponent<SpriteRenderer>().enabled = true;
                                            tiles[tileIndex].GetComponent<SpriteRenderer>().color = moveColor;
                                        }
                                    }
                                    else
                                    {
                                        //print("!!!!" + tileIndex);
                                        moveColor = Color.green;
                                        moveColor.a = 0.7f;
                                        tiles[tileIndex].GetComponent<SpriteRenderer>().enabled = true;
                                        tiles[tileIndex].GetComponent<SpriteRenderer>().color = moveColor;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        removeTilesForPathFind = new List<GameObject>();

        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i].GetComponent<SpriteRenderer>().enabled == true)
            {
                //print("" + i + "th Tile: " + CheckAvailablePath(selectedUnit, tiles[i]).Count);

                if (CheckAvailablePath(selectedUnit, tiles[i]).Count > rangeIndex + 2 || CheckAvailablePath(selectedUnit, tiles[i]).Count == 0)
                {
                    removeTilesForPathFind.Add(tiles[i]);
                }
            }
        }

        for (int i = 0; i < removeTilesForPathFind.Count; i++)
        {
            if (Vector3.Distance(selectedUnit.transform.position, tiles[0].transform.position) < 0.1f)
            {
                //if (removeTilesForPathFind[i] == tiles[1] || removeTilesForPathFind[i] == tiles[29] || removeTilesForPathFind[i] == tiles[30] ||
                //    removeTilesForPathFind[i] == tiles[31] || removeTilesForPathFind[i] == tiles[59] || removeTilesForPathFind[i] == tiles[89] ||
                //    removeTilesForPathFind[i] == tiles[870] || removeTilesForPathFind[i] == tiles[871] || removeTilesForPathFind[i] == tiles[899])
                //{
                //}
                //else
                //{
                //    removeTilesForPathFind[i].GetComponent<SpriteRenderer>().enabled = false;
                //}
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[1].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[2].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[3].transform.position) < 0.1f)
            {
                if (removeTilesForPathFind[i] == tiles[29] || removeTilesForPathFind[i] == tiles[59] || removeTilesForPathFind[i] == tiles[899])
                {
                }
                else
                {
                    removeTilesForPathFind[i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            if (Vector3.Distance(selectedUnit.transform.position, tiles[25].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[26].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[27].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[28].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[29].transform.position) < 0.1f)
            {
                if (removeTilesForPathFind[i] == tiles[1] || removeTilesForPathFind[i] == tiles[31] || removeTilesForPathFind[i] == tiles[835] ||
                    removeTilesForPathFind[i] == tiles[871])
                {
                }
                else
                {
                    removeTilesForPathFind[i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[30].transform.position) < 0.1f)
            {
                if (removeTilesForPathFind[i] == tiles[870] || removeTilesForPathFind[i] == tiles[871] || removeTilesForPathFind[i] == tiles[899])
                {
                }
                else
                {
                    removeTilesForPathFind[i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[31].transform.position) < 0.1f)
            {
                if (removeTilesForPathFind[i] == tiles[29] || removeTilesForPathFind[i] == tiles[899])
                {
                }
                else
                {
                    removeTilesForPathFind[i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[32].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[33].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[61].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[62].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[63].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[91].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[92].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[93].transform.position) < 0.1f)
            {
                if (removeTilesForPathFind[i] == tiles[899])
                {
                }
                else
                {
                    removeTilesForPathFind[i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[55].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[56].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[57].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[58].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[85].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[86].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[87].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[88].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[115].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[116].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[117].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[118].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[145].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[146].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[147].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[148].transform.position) < 0.1f)
            {
                if (removeTilesForPathFind[i] == tiles[871])
                {
                }
                else
                {
                    removeTilesForPathFind[i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[59].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[89].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[119].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[149].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[179].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[209].transform.position) < 0.1f)
            {
                if (removeTilesForPathFind[i] == tiles[870] || removeTilesForPathFind[i] == tiles[871] || removeTilesForPathFind[i] == tiles[899])
                {
                }
                else
                {
                    removeTilesForPathFind[i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[60].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[90].transform.position) < 0.1f)
            {
                if (removeTilesForPathFind[i] == tiles[870] || removeTilesForPathFind[i] == tiles[871] || removeTilesForPathFind[i] == tiles[899])
                {
                }
                else
                {
                    removeTilesForPathFind[i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[481].transform.position) < 0.1f)
            {
                if (removeTilesForPathFind[i] == tiles[536])
                {
                }
                else
                {
                    removeTilesForPathFind[i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[751].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[752].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[753].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[754].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[781].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[782].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[783].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[784].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[785].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[811].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[812].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[813].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[814].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[815].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[841].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[842].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[843].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[844].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[845].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[871].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[872].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[873].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[874].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[875].transform.position) < 0.1f)
            {
                if (removeTilesForPathFind[i] == tiles[29] || removeTilesForPathFind[i] == tiles[59] || removeTilesForPathFind[i] == tiles[899])
                {
                }
                else
                {
                    removeTilesForPathFind[i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[865].transform.position) < 0.1f)
            {
                if (removeTilesForPathFind[i] == tiles[31] || removeTilesForPathFind[i] == tiles[1])
                {
                }
                else
                {
                    removeTilesForPathFind[i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[866].transform.position) < 0.1f)
            {
                if (removeTilesForPathFind[i] == tiles[31] || removeTilesForPathFind[i] == tiles[32])
                {
                }
                else
                {
                    removeTilesForPathFind[i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[867].transform.position) < 0.1f
                  || Vector3.Distance(selectedUnit.transform.position, tiles[837].transform.position) < 0.1f
                  || Vector3.Distance(selectedUnit.transform.position, tiles[807].transform.position) < 0.1f
                  || Vector3.Distance(selectedUnit.transform.position, tiles[777].transform.position) < 0.1f)
            {
                if (removeTilesForPathFind[i] == tiles[31])
                {
                }
                else
                {
                    removeTilesForPathFind[i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[868].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[838].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[808].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[778].transform.position) < 0.1f)
            {
                if (removeTilesForPathFind[i] == tiles[30] || removeTilesForPathFind[i] == tiles[31])
                {
                }
                else
                {
                    removeTilesForPathFind[i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[840].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[869].transform.position) < 0.1f)
            {
                if (removeTilesForPathFind[i] == tiles[30] || removeTilesForPathFind[i] == tiles[31] || removeTilesForPathFind[i] == tiles[59])
                {
                }
                else
                {
                    removeTilesForPathFind[i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[870].transform.position) < 0.1f)
            {
                if (removeTilesForPathFind[i] == tiles[30] || removeTilesForPathFind[i] == tiles[31] || removeTilesForPathFind[i] == tiles[59])
                {
                }
                else
                {
                    removeTilesForPathFind[i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[871].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[872].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[873].transform.position) < 0.1f)
            {
                if (removeTilesForPathFind[i] == tiles[59])
                {
                }
                else
                {
                    removeTilesForPathFind[i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[897].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[898].transform.position) < 0.1f)
            {
                if (removeTilesForPathFind[i] == tiles[1] || removeTilesForPathFind[i] == tiles[31] || removeTilesForPathFind[i] == tiles[871])
                {
                }
                else
                {
                    removeTilesForPathFind[i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[899].transform.position) < 0.1f)
            {
                if (removeTilesForPathFind[i] == tiles[30] || removeTilesForPathFind[i] == tiles[31])
                {
                }
                else
                {
                    removeTilesForPathFind[i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
            else
            {
                removeTilesForPathFind[i].GetComponent<SpriteRenderer>().enabled = false;
            }
        }

        if (Vector3.Distance(selectedUnit.transform.position, tiles[0].transform.position) < 0.1f)
        {
            moveColor = Color.green;
            moveColor.a = 0.7f;

            int[] startValue = new int[] { 1, 29, 30, 31, 59, 870, 871, 899 };

            for (int i = 0; i < startValue.Length; i++)
            {
                tiles[startValue[i]].GetComponent<SpriteRenderer>().enabled = true;
                tiles[startValue[i]].GetComponent<SpriteRenderer>().color = moveColor;
            }
        }

        if (Vector3.Distance(selectedUnit.transform.position, tiles[1].transform.position) < 0.1f ||
            Vector3.Distance(selectedUnit.transform.position, tiles[2].transform.position) < 0.1f ||
            Vector3.Distance(selectedUnit.transform.position, tiles[3].transform.position) < 0.1f)
        {
            moveColor = Color.green;
            moveColor.a = 0.7f;

            int[] startValue = new int[] { 29, 59, 899 };

            for (int i = 0; i < startValue.Length; i++)
            {
                tiles[startValue[i]].GetComponent<SpriteRenderer>().enabled = true;
                tiles[startValue[i]].GetComponent<SpriteRenderer>().color = moveColor;
            }
        }

        // Yellow color for selected units        
        for (int i = 0; i < tiles.Length; i++)
        {
            if (Vector3.Distance(selectedUnit.transform.position, tiles[i].transform.position) < 0.1f)
            {
                if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && selectedUnit.GetComponent<SpeciaInfo>().specialType != (int)SpecialType.Bunker)
                {
                    if ((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame))
                    {
                        int selectedTileIndex = i;
                        Color selectedColor = Color.yellow;
                        selectedColor.a = 0.7f;
                        tiles[selectedTileIndex].GetComponent<SpriteRenderer>().enabled = true;
                        tiles[selectedTileIndex].GetComponent<SpriteRenderer>().color = selectedColor;
                    }
                }
                else
                {
                    int selectedTileIndex = i;
                    Color selectedColor = Color.yellow;
                    selectedColor.a = 0.7f;
                    tiles[selectedTileIndex].GetComponent<SpriteRenderer>().enabled = true;
                    tiles[selectedTileIndex].GetComponent<SpriteRenderer>().color = selectedColor;
                }
            }
        }

        if (currentTurnIndex != whoseTurn)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].GetComponent<SpriteRenderer>().enabled = false;
            }

            endPlayButton.SetActive(false);
        }
    }

    public void ShowSubRange(int rangeIndex)
    {
        mountains = new List<int>();
        int x1, y1 = 0;
        int x3, y3 = 0;

        for (int i = 0; i < tiles.Length; i++)
        {
            if (Vector3.Distance(selectedUnit.transform.position, tiles[i].transform.position) < 0.1f)
            {
                int selectedTileIndex = i;

                x1 = i % 30;
                y1 = i / 30;

                int xMinRange = x1 - rangeIndex - 1, xMaxRange = x1 + rangeIndex + 1;
                int yMinRange = y1 - rangeIndex - 1, yMaxRange = y1 + rangeIndex + 1;

                for (int j = yMinRange; j <= yMaxRange; j++)
                {
                    for (int k = xMinRange; k <= xMaxRange; k++)
                    {
                        int tileIndex = CalculateTileIndex(j, k);

                        if (tiles[tileIndex].GetComponent<CellInfo>().cellType == (int)CellType.Mountain)
                        {
                            mountains.Add(tileIndex);
                        }
                    }
                }

                for (int j = yMinRange; j <= yMaxRange; j++)
                {
                    for (int k = xMinRange; k <= xMaxRange; k++)
                    {
                        int tileIndex = CalculateTileIndex(j, k);

                        x3 = tileIndex % 30;
                        y3 = tileIndex / 30;

                        if (tiles[tileIndex].GetComponent<CellInfo>().cellType != (int)CellType.Mountain)
                        {
                            if (IsForceForSub(tiles[tileIndex]) == false)
                            {
                                if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Plane || selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.UFO || selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.General)
                                {
                                    fireColor = Color.red;
                                    fireColor.a = 0.7f;
                                    tiles[tileIndex].GetComponent<SpriteRenderer>().enabled = true;
                                    tiles[tileIndex].GetComponent<SpriteRenderer>().color = fireColor;
                                }
                                else if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
                                {
                                    if (j >= 0 && j < 30 && k >= 0 && k < 30)
                                    {
                                        if (IsMTNStrategy(tiles[tileIndex], rangeIndex, x1, y1, x3, y3) == false)
                                        {
                                            if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
                                            {
                                                fireColor = Color.red;
                                                fireColor.a = 0.7f;
                                                tiles[tileIndex].GetComponent<SpriteRenderer>().enabled = true;
                                                tiles[tileIndex].GetComponent<SpriteRenderer>().color = fireColor;
                                            }
                                            else
                                            {
                                                fireColor = Color.red;
                                                fireColor.a = 0f;
                                                tiles[tileIndex].GetComponent<SpriteRenderer>().enabled = true;
                                                tiles[tileIndex].GetComponent<SpriteRenderer>().color = fireColor;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (IsMTNStrategy(tiles[tileIndex], rangeIndex, x1, y1, x3, y3) == false)
                                        {
                                            if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
                                            {
                                                fireColor = Color.red;
                                                fireColor.a = 0.7f;
                                                tiles[tileIndex].GetComponent<SpriteRenderer>().enabled = true;
                                                tiles[tileIndex].GetComponent<SpriteRenderer>().color = fireColor;
                                            }
                                            else
                                            {
                                                fireColor = Color.red;
                                                fireColor.a = 0f;
                                                tiles[tileIndex].GetComponent<SpriteRenderer>().enabled = true;
                                                tiles[tileIndex].GetComponent<SpriteRenderer>().color = fireColor;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (j >= 0 && j < 30 && k >= 0 && k < 30)
                                    {
                                        if (IsMTNStrategy(tiles[tileIndex], rangeIndex, x1, y1, x3, y3) == false)
                                        {
                                            fireColor = Color.red;
                                            fireColor.a = 0.7f;
                                            tiles[tileIndex].GetComponent<SpriteRenderer>().enabled = true;
                                            tiles[tileIndex].GetComponent<SpriteRenderer>().color = fireColor;
                                        }
                                    }
                                    else
                                    {
                                        if (IsMTNStrategy(tiles[tileIndex], rangeIndex, x1, y1, x3, y3) == false)
                                        {
                                            fireColor = Color.red;
                                            fireColor.a = 0.7f;
                                            tiles[tileIndex].GetComponent<SpriteRenderer>().enabled = true;
                                            tiles[tileIndex].GetComponent<SpriteRenderer>().color = fireColor;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Yellow color for selected units        
        for (int i = 0; i < tiles.Length; i++)
        {
            if (Vector3.Distance(selectedUnit.transform.position, tiles[i].transform.position) < 0.1f)
            {
                if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && selectedUnit.GetComponent<SpeciaInfo>().specialType != (int)SpecialType.Bunker)
                {
                    if ((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame))
                    {
                        int selectedTileIndex = i;
                        Color selectedColor = Color.yellow;
                        selectedColor.a = 0.7f;
                        tiles[selectedTileIndex].GetComponent<SpriteRenderer>().enabled = true;
                        tiles[selectedTileIndex].GetComponent<SpriteRenderer>().color = selectedColor;
                    }
                }
                else
                {
                    int selectedTileIndex = i;
                    Color selectedColor = Color.yellow;
                    selectedColor.a = 0.7f;
                    tiles[selectedTileIndex].GetComponent<SpriteRenderer>().enabled = true;
                    tiles[selectedTileIndex].GetComponent<SpriteRenderer>().color = selectedColor;
                }
            }
        }

        if (currentTurnIndex != whoseTurn)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].GetComponent<SpriteRenderer>().enabled = false;
            }

            endPlayButton.SetActive(false);
            infoSubAction.SetActive(false);
            subFireButton.SetActive(false);

        }
    }

    public void ShowSonarRange(int rangeIndex)
    {
        mountains = new List<int>();
        int x1, y1 = 0;
        int x3, y3 = 0;

        for (int i = 0; i < tiles.Length; i++)
        {
            if (Vector3.Distance(selectedUnit.transform.position, tiles[i].transform.position) < 0.1f)
            {
                int selectedTileIndex = i;

                x1 = i % 30;
                y1 = i / 30;

                int xMinRange = x1 - rangeIndex - 1, xMaxRange = x1 + rangeIndex + 1;
                int yMinRange = y1 - rangeIndex - 1, yMaxRange = y1 + rangeIndex + 1;

                for (int j = yMinRange; j <= yMaxRange; j++)
                {
                    for (int k = xMinRange; k <= xMaxRange; k++)
                    {
                        int tileIndex = CalculateTileIndex(j, k);

                        if (tiles[tileIndex].GetComponent<CellInfo>().cellType == (int)CellType.Mountain)
                        {
                            mountains.Add(tileIndex);
                        }
                    }
                }

                for (int j = yMinRange; j <= yMaxRange; j++)
                {
                    for (int k = xMinRange; k <= xMaxRange; k++)
                    {
                        int tileIndex = CalculateTileIndex(j, k);

                        x3 = tileIndex % 30;
                        y3 = tileIndex / 30;

                        if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && tiles[tileIndex].GetComponent<CellInfo>().cellType != (int)CellType.Mountain)
                        {
                            if (j >= 0 && j < 30 && k >= 0 && k < 30)
                            {
                                if (IsMTNStrategy(tiles[tileIndex], rangeIndex, x1, y1, x3, y3) == false)
                                {
                                    if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
                                    {
                                        sonarColor = new Color32(143, 0, 254, 1);
                                        sonarColor.a = 0.7f;
                                        tiles[tileIndex].GetComponent<SpriteRenderer>().enabled = true;
                                        tiles[tileIndex].GetComponent<SpriteRenderer>().color = sonarColor;
                                        pingButton.SetActive(true);
                                    }
                                    else
                                    {
                                        sonarColor = new Color32(143, 0, 254, 1);
                                        sonarColor.a = 0f;
                                        tiles[tileIndex].GetComponent<SpriteRenderer>().enabled = true;
                                        tiles[tileIndex].GetComponent<SpriteRenderer>().color = sonarColor;
                                    }
                                }
                            }
                            else
                            {
                                if (IsMTNStrategy(tiles[tileIndex], rangeIndex, x1, y1, x3, y3) == false)
                                {
                                    if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
                                    {
                                        sonarColor = new Color32(143, 0, 254, 1);
                                        sonarColor.a = 0.7f;
                                        tiles[tileIndex].GetComponent<SpriteRenderer>().enabled = true;
                                        tiles[tileIndex].GetComponent<SpriteRenderer>().color = sonarColor;
                                        pingButton.SetActive(true);
                                    }
                                    else
                                    {
                                        sonarColor = new Color32(143, 0, 254, 1);
                                        sonarColor.a = 0f;
                                        tiles[tileIndex].GetComponent<SpriteRenderer>().enabled = true;
                                        tiles[tileIndex].GetComponent<SpriteRenderer>().color = sonarColor;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (((whoseTurn == (int)TurnIndex.Player1 && !isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && isMasterInGame)))
        {
            subPing.SetActive(true);
            //cameraView.transform.position = new Vector3(subPing.transform.position.x, subPing.transform.position.y, cameraView.transform.position.z);

            for (int i = 0; i < tiles.Length; i++)
            {
                sonarColor = new Color32(143, 0, 254, 1);
                sonarColor.a = 1f;
                tiles[i].GetComponent<SpriteRenderer>().enabled = true;
                tiles[i].GetComponent<SpriteRenderer>().color = sonarColor;
            }
        }
        else
        {
            SetAudio(2);
            selectedUnit.GetComponent<AudioSource>().loop = true;
            selectedUnit.GetComponent<AudioSource>().Play();
        }

        // Yellow color for selected units        
        for (int i = 0; i < tiles.Length; i++)
        {
            if (Vector3.Distance(selectedUnit.transform.position, tiles[i].transform.position) < 0.1f)
            {
                if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && selectedUnit.GetComponent<SpeciaInfo>().specialType != (int)SpecialType.Bunker)
                {
                    if ((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame))
                    {
                        int selectedTileIndex = i;
                        Color selectedColor = Color.yellow;
                        selectedColor.a = 0.7f;
                        tiles[selectedTileIndex].GetComponent<SpriteRenderer>().enabled = true;
                        tiles[selectedTileIndex].GetComponent<SpriteRenderer>().color = selectedColor;
                    }
                }
                else
                {
                    int selectedTileIndex = i;
                    Color selectedColor = Color.yellow;
                    selectedColor.a = 0.7f;
                    tiles[selectedTileIndex].GetComponent<SpriteRenderer>().enabled = true;
                    tiles[selectedTileIndex].GetComponent<SpriteRenderer>().color = selectedColor;
                }
            }
        }

        if (currentTurnIndex != whoseTurn)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].GetComponent<SpriteRenderer>().enabled = false;
            }

            endPlayButton.SetActive(false);
            subPing.SetActive(false);
            pingButton.SetActive(false);
        }
    }

    public void ShowUFORange()
    {
        isUFOMoving = true;

        for (int i = 0; i < tiles.Length; i++)
        {
            moveColor = Color.cyan;
            moveColor.a = 0.7f;
            tiles[i].GetComponent<SpriteRenderer>().enabled = true;
            tiles[i].GetComponent<SpriteRenderer>().color = moveColor;

            for (int j = 0; j < force1ForCircle.Length; j++)
            {
                if (Vector3.Distance(tiles[i].transform.position, force1ForCircle[j].transform.position) < 0.1f && force1ForCircle[j].activeSelf == true)
                {
                    tiles[i].GetComponent<SpriteRenderer>().enabled = false;
                }

                if (Vector3.Distance(tiles[i].transform.position, force1ForSquare[j].transform.position) < 0.1f && force1ForSquare[j].activeSelf == true)
                {
                    tiles[i].GetComponent<SpriteRenderer>().enabled = false;
                }

                if (Vector3.Distance(tiles[i].transform.position, force2ForCircle[j].transform.position) < 0.1f && force2ForCircle[j].activeSelf == true)
                {
                    tiles[i].GetComponent<SpriteRenderer>().enabled = false;
                }

                if (Vector3.Distance(tiles[i].transform.position, force2ForSquare[j].transform.position) < 0.1f && force2ForSquare[j].activeSelf == true)
                {
                    tiles[i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }
        }

        if (currentTurnIndex != whoseTurn)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].GetComponent<SpriteRenderer>().enabled = false;
            }
   
            endPlayButton.SetActive(false);
            ufo.SetActive(false);
        }
    }

    public void PingAction(int rangeIndex)
    {
        selectedUnit.GetComponent<AudioSource>().loop = false;

        if ((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame))
        {
            for (int j = 0; j < tiles.Length; j++)
            {
                if (tiles[j].GetComponent<SpriteRenderer>().enabled == true)
                {
                    GameObject cell = tiles[j];

                    // find hiden subs
                    for (int i = 0; i < force1ForCircle.Length; i++)
                    {
                        if (whoseTurn == (int)TurnIndex.Player1)
                        {
                            if (Vector3.Distance(cell.transform.position, force2ForCircle[i].transform.position) < 0.1f && force2ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
                            {
                                force2ForCircle[i].SetActive(true);
                                force2ForCircle[i].transform.GetChild(3).gameObject.SetActive(true);
                            }

                            if (Vector3.Distance(cell.transform.position, force2ForSquare[i].transform.position) < 0.1f && force2ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
                            {
                                force2ForSquare[i].SetActive(true);
                                force2ForSquare[i].transform.GetChild(3).gameObject.SetActive(true);
                            }
                        }
                        else if (whoseTurn == (int)TurnIndex.Player2)
                        {
                            if (Vector3.Distance(cell.transform.position, force1ForCircle[i].transform.position) < 0.1f && force1ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
                            {
                                force1ForCircle[i].SetActive(true);
                                force1ForCircle[i].transform.GetChild(3).gameObject.SetActive(true);
                            }

                            if (Vector3.Distance(cell.transform.position, force1ForSquare[i].transform.position) < 0.1f && force1ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
                            {
                                force1ForSquare[i].SetActive(true);
                                force1ForSquare[i].transform.GetChild(3).gameObject.SetActive(true);
                            }
                        }
                    }

                    // find hiden mines 
                    if (whoseTurn == (int)TurnIndex.Player1)
                    {
                        for (int i = 0; i < atomicMines.Length / 2; i++)
                        {
                            if (Vector3.Distance(cell.transform.position, atomicMines[i + 10].transform.position) < 0.1f)
                            {
                                atomicMines[i + 10].SetActive(true);
                                atomicMines[i + 10].transform.GetChild(0).gameObject.SetActive(true);
                            }
                        }
                    }
                    else if (whoseTurn == (int)TurnIndex.Player2)
                    {
                        for (int i = 0; i < atomicMines.Length / 2; i++)
                        {
                            if (Vector3.Distance(cell.transform.position, atomicMines[i].transform.position) < 0.1f)
                            {
                                atomicMines[i].SetActive(true);
                                atomicMines[i].transform.GetChild(0).gameObject.SetActive(true);
                            }
                        }
                    }

                    // find hiden triangles
                    if (whoseTurn == (int)TurnIndex.Player1)
                    {
                        for (int i = 0; i < bermudaTriangles.Length / 2; i++)
                        {
                            if (Vector3.Distance(cell.transform.position, bermudaTriangles[i + 10].transform.position) < 0.1f)
                            {
                                bermudaTriangles[i + 10].SetActive(true);
                                bermudaTriangles[i + 10].transform.GetChild(0).gameObject.SetActive(true);
                            }
                        }
                    }
                    else if (whoseTurn == (int)TurnIndex.Player2)
                    {
                        for (int i = 0; i < bermudaTriangles.Length / 2; i++)
                        {
                            if (Vector3.Distance(cell.transform.position, bermudaTriangles[i].transform.position) < 0.1f)
                            {
                                bermudaTriangles[i].SetActive(true);
                                bermudaTriangles[i].transform.GetChild(0).gameObject.SetActive(true);
                            }
                        }
                    }
                }
            }
        }

        SonarAction();
    }
    #endregion

    #region Extra

    public void ChangeTurn(int turnsrem=-1)
    {

        if(PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Change Turn called for master");
        }
        else
        {
            Debug.Log("Change turn called for client");
        }
        isChangingTurn = false;
        isUFOMove = false;
        isBlackDragon = false;
        isSelected = false;
        isFire = false;
        isMove = false;
        isSonar = false;
        isSub = false;
        isUFO = false;
        isUFOMoving = false;
        isProgressAction = false;
        isOnlyOncePlay = false;
        isIncreasedByMines = false;
        isIncreasedByBermuda = false;
        isEndedByMines = false;
        isBombLoading = false;
        moveButton.SetActive(false);
        fireButton.SetActive(false);
        setSubButton(false);
        sonarButton.SetActive(false);
        subFireButton.SetActive(false);

        pingButton.SetActive(false);
        //isMoveAnimation = false;
        exceedCount = 0;
        blackDragonTime = 1;

        ufo.transform.position = new Vector3(ufoInitialPos.x * UnityEngine.Random.Range(0, 2) == 0 ? 1f : -1f, ufoInitialPos.y * UnityEngine.Random.Range(0, 2) == 0 ? 1f : -1f, ufoInitialPos.z);
        ufo.SetActive(false);
        subPing.SetActive(false);
        infoSubAction.SetActive(false);
        infoBlackDragon.SetActive(false);
        infoBombs.SetActive(false);

        bootingObject.SetActive(true);
        if (turnsrem ==-1)
        {
            bootingObject.GetComponent<GameSettingsApplier>().resetTimer();
        }

        bootingObject.GetComponent<GameSettingsApplier>().setTimerActive(true);
        isShowWinning = false;

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].GetComponent<SpriteRenderer>().enabled = false;
        }

        if (isMasterInGame)
        {
            for (int i = 0; i < bermudaTriangles.Length / 2; i++)
            {
                bermudaTriangles[i + 10].SetActive(false);
                atomicMines[i + 10].SetActive(false);
            }
        }
        else if (!isMasterInGame)
        {
            for (int i = 0; i < bermudaTriangles.Length / 2; i++)
            {
                bermudaTriangles[i].SetActive(false);
                atomicMines[i].SetActive(false);
            }
        }

        for (int i = 0; i < bermudaTriangles.Length; i++)
        {
            atomicMines[i].transform.GetChild(0).gameObject.SetActive(false);
            bermudaTriangles[i].transform.GetChild(0).gameObject.SetActive(false);
        }

        for (int i = 4; i < 6; i++)
        {
            force1ForCircle[i].transform.GetChild(3).gameObject.SetActive(false);
            force1ForSquare[i].transform.GetChild(3).gameObject.SetActive(false);
            force2ForCircle[i].transform.GetChild(3).gameObject.SetActive(false);
            force2ForSquare[i].transform.GetChild(3).gameObject.SetActive(false);
        }

        for (int i = 0; i < force1ForCircle.Length; i++)
        {                        
            if (force1ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                if (ConvertToTile(force1ForCircle[i]).GetComponent<CellInfo>().cellType == (int)CellType.Tunnel ||
                    ConvertToTile(force1ForCircle[i]).GetComponent<CellInfo>().cellType == (int)CellType.TunnelEntrance)
                {
                    tunnelColor = new Color32(69, 130, 255, 1);
                    tunnelColor.a = 0.7f;

                    if (i == 4)
                    {
                        subBacks[0].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[0].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                    else
                    {
                        subBacks[1].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[1].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                }
                else
                {
                    if (i == 4)
                    {
                        subBacks[0].GetComponent<SpriteRenderer>().enabled = false;
                    }
                    else
                    {
                        subBacks[1].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }

            if (force1ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                if (ConvertToTile(force1ForSquare[i]).GetComponent<CellInfo>().cellType == (int)CellType.Tunnel ||
                    ConvertToTile(force1ForSquare[i]).GetComponent<CellInfo>().cellType == (int)CellType.TunnelEntrance)
                {
                    tunnelColor = new Color32(69, 130, 255, 1);
                    tunnelColor.a = 0.7f;

                    if (i == 4)
                    {
                        subBacks[2].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[2].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                    else
                    {
                        subBacks[3].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[3].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                }
                else
                {
                    if (i == 4)
                    {
                        subBacks[2].GetComponent<SpriteRenderer>().enabled = false;
                    }
                    else
                    {
                        subBacks[3].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }

            if (force2ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                if (ConvertToTile(force2ForCircle[i]).GetComponent<CellInfo>().cellType == (int)CellType.Tunnel ||
                    ConvertToTile(force2ForCircle[i]).GetComponent<CellInfo>().cellType == (int)CellType.TunnelEntrance)
                {
                    tunnelColor = new Color32(69, 130, 255, 1);
                    tunnelColor.a = 0.7f;

                    if (i == 4)
                    {
                        subBacks[4].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[4].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                    else
                    {
                        subBacks[5].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[5].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                }
                else
                {
                    if (i == 4)
                    {
                        subBacks[4].GetComponent<SpriteRenderer>().enabled = false;
                    }
                    else
                    {
                        subBacks[5].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }

            if (force2ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                if (ConvertToTile(force2ForSquare[i]).GetComponent<CellInfo>().cellType == (int)CellType.Tunnel
                    || ConvertToTile(force2ForSquare[i]).GetComponent<CellInfo>().cellType == (int)CellType.TunnelEntrance)
                {
                    tunnelColor = new Color32(69, 130, 255, 1);
                    tunnelColor.a = 0.7f;

                    if (i == 4)
                    {
                        subBacks[6].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[6].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                    else
                    {
                        subBacks[7].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[7].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                }
                else
                {
                    if (i == 4)
                    {
                        subBacks[6].GetComponent<SpriteRenderer>().enabled = false;
                    }
                    else
                    {
                        subBacks[7].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }
        }

        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            target1ForCircle[i].SetActive(false);
            target1ForSquare[i].SetActive(false);
            target2ForCircle[i].SetActive(false);
            target2ForSquare[i].SetActive(false);

            //Check bunker units
            if ((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame))
            {
                if (force1ForCircle[i].GetComponent<SpeciaInfo>().bunkerCount != 0)
                {
                    force1ForCircle[i].GetComponent<SpeciaInfo>().bunkerCount--;
                }

                if (force2ForCircle[i].GetComponent<SpeciaInfo>().bunkerCount != 0)
                {
                    force2ForCircle[i].GetComponent<SpeciaInfo>().bunkerCount--;
                }

                if (force1ForSquare[i].GetComponent<SpeciaInfo>().bunkerCount != 0)
                {
                    force1ForSquare[i].GetComponent<SpeciaInfo>().bunkerCount--;
                }

                if (force2ForSquare[i].GetComponent<SpeciaInfo>().bunkerCount != 0)
                {
                    force2ForSquare[i].GetComponent<SpeciaInfo>().bunkerCount--;
                }
            }                        
        }

        availableForce1 = new List<GameObject>();
        availableForce2 = new List<GameObject>();

        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            if (force1ForCircle[i].transform.position != new Vector3(10000f, 10000f, -8f))
            {
                availableForce1.Add(force1ForCircle[i]);
            }

            if (force1ForSquare[i].transform.position != new Vector3(10000f, 10000f, -8f))
            {
                availableForce1.Add(force1ForSquare[i]);
            }
        }

        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            if (force2ForCircle[i].transform.position != new Vector3(10000f, 10000f, -8f))
            {
                availableForce2.Add(force2ForCircle[i]);
            }

            if (force2ForSquare[i].transform.position != new Vector3(10000f, 10000f, -8f))
            {
                availableForce2.Add(force2ForSquare[i]);
            }
        }

        if (isMasterInGame)
        {
            force2ForCircle[4].SetActive(false);
            force2ForSquare[4].SetActive(false);
            force2ForCircle[5].SetActive(false);
            force2ForSquare[5].SetActive(false);
            force1ForCircle[4].SetActive(true);
            force1ForSquare[4].SetActive(true);
            force1ForCircle[5].SetActive(true);
            force1ForSquare[5].SetActive(true);
        }
        else
        {
            force1ForCircle[4].SetActive(false);
            force1ForSquare[4].SetActive(false);
            force1ForCircle[5].SetActive(false);
            force1ForSquare[5].SetActive(false);
            force2ForCircle[4].SetActive(true);
            force2ForSquare[4].SetActive(true);
            force2ForCircle[5].SetActive(true);
            force2ForSquare[5].SetActive(true);
        }

        if (whoseTurn == (int)TurnIndex.Player1)
        {
            for (int i = 0; i < force1ForCircle.Length; i++)
            {
                force1ForCircle[i].GetComponent<Animator>().enabled = true;
                force1ForSquare[i].GetComponent<Animator>().enabled = true;
                force2ForCircle[i].GetComponent<Animator>().enabled = false;
                force2ForSquare[i].GetComponent<Animator>().enabled = false;
                force1ForCircle[i].GetComponent<ForceInfo>().controlTimes = 0;
                force1ForSquare[i].GetComponent<ForceInfo>().controlTimes = 0;
                force2ForCircle[i].GetComponent<ForceInfo>().controlTimes = 0;
                force2ForSquare[i].GetComponent<ForceInfo>().controlTimes = 0;
                force1ForCircle[i].GetComponent<AudioSource>().Stop();
                force1ForSquare[i].GetComponent<AudioSource>().Stop();
                force2ForCircle[i].GetComponent<AudioSource>().Stop();
                force2ForSquare[i].GetComponent<AudioSource>().Stop();
                target1ForCircle[i].SetActive(false);
                target1ForSquare[i].SetActive(false);
                target2ForCircle[i].SetActive(false);
                target2ForSquare[i].SetActive(false);
            }

            playerAvatars[0].SetActive(true);
            playerAvatars[1].SetActive(false);
            currentPlayerNameText.text = MainUI.playerNames[0];
        }
        else if (whoseTurn == (int)TurnIndex.Player2)
        {
            for (int i = 0; i < force2ForCircle.Length; i++)
            {
                force1ForCircle[i].GetComponent<Animator>().enabled = false;
                force1ForSquare[i].GetComponent<Animator>().enabled = false;
                force2ForCircle[i].GetComponent<Animator>().enabled = true;
                force2ForSquare[i].GetComponent<Animator>().enabled = true;
                force1ForCircle[i].GetComponent<ForceInfo>().controlTimes = 0;
                force1ForSquare[i].GetComponent<ForceInfo>().controlTimes = 0;
                force2ForCircle[i].GetComponent<ForceInfo>().controlTimes = 0;
                force2ForSquare[i].GetComponent<ForceInfo>().controlTimes = 0;
                force1ForCircle[i].GetComponent<AudioSource>().Stop();
                force1ForSquare[i].GetComponent<AudioSource>().Stop();
                force2ForCircle[i].GetComponent<AudioSource>().Stop();
                force2ForSquare[i].GetComponent<AudioSource>().Stop();
                target1ForCircle[i].SetActive(false);
                target1ForSquare[i].SetActive(false);
                target2ForCircle[i].SetActive(false);
                target2ForSquare[i].SetActive(false);
            }

            playerAvatars[1].SetActive(true);
            playerAvatars[0].SetActive(false);
            currentPlayerNameText.text = MainUI.playerNames[1];
        }

        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            if (force1ForCircle[i].GetComponent<SpeciaInfo>().specialType == (int)SpecialType.Bunker)
            {
                force1ForCircle[i].SetActive(true);
            }

            if (force1ForSquare[i].GetComponent<SpeciaInfo>().specialType == (int)SpecialType.Bunker)
            {
                force1ForSquare[i].SetActive(true);
            }

            if (force2ForCircle[i].GetComponent<SpeciaInfo>().specialType == (int)SpecialType.Bunker)
            {
                force2ForCircle[i].SetActive(true);
            }

            if (force2ForSquare[i].GetComponent<SpeciaInfo>().specialType == (int)SpecialType.Bunker)
            {
                force2ForSquare[i].SetActive(true);
            }
        }

        for (int i = 0; i < generalFaces.Length; i++)
        {
            generalFaces[i].SetActive(false);
        }

        if (whoseTurn == (int)TurnIndex.Player1)
        {
            generalFaces[0].GetComponent<Image>().sprite = force1ForCircle[0].GetComponent<SpriteRenderer>().sprite;
            generalFaces[1].GetComponent<Image>().sprite = force1ForCircle[1].GetComponent<SpriteRenderer>().sprite;
            generalFaces[2].GetComponent<Image>().sprite = force1ForSquare[0].GetComponent<SpriteRenderer>().sprite;
            generalFaces[3].GetComponent<Image>().sprite = force1ForSquare[1].GetComponent<SpriteRenderer>().sprite;

            for (int i = 0; i < 2; i++)
            {
                if (force1ForCircle[i].transform.position != new Vector3(10000f, 10000f, -8f))
                {
                    generalFaces[i].SetActive(true);
                }

                if (force1ForSquare[i].transform.position != new Vector3(10000f, 10000f, -8f))
                {
                    generalFaces[i + 2].SetActive(true);
                }
            }
        }
        else
        {
            generalFaces[0].GetComponent<Image>().sprite = force2ForCircle[0].GetComponent<SpriteRenderer>().sprite;
            generalFaces[1].GetComponent<Image>().sprite = force2ForCircle[1].GetComponent<SpriteRenderer>().sprite;
            generalFaces[2].GetComponent<Image>().sprite = force2ForSquare[0].GetComponent<SpriteRenderer>().sprite;
            generalFaces[3].GetComponent<Image>().sprite = force2ForSquare[1].GetComponent<SpriteRenderer>().sprite;

            for (int i = 0; i < 2; i++)
            {
                if (force2ForCircle[i].transform.position != new Vector3(10000f, 10000f, -8f))
                {
                    generalFaces[i].SetActive(true);
                }

                if (force2ForSquare[i].transform.position != new Vector3(10000f, 10000f, -8f))
                {
                    generalFaces[i + 2].SetActive(true);
                }
            }
        }

        ufo.GetComponent<ForceInfo>().controlTimes = 0;
        ufo.GetComponent<AudioSource>().Stop();                
        
        if (whoseTurn == (int)TurnIndex.Player1)
        {
            if (NumbersOfAvailableUnits() == 1)
            {
                players[0].GetComponent<PlayerInfo>().playTimes = turnsrem != -1 ? turnsrem : 2;
                playsLeftText.text = "" + players[0].GetComponent<PlayerInfo>().playTimes;

                //if (isMeleeRound)
                //{
                //    if(meleeRoundCount == 0)
                //    {
                //        StartCoroutine(DelayToShowMeleeRound());
                //    }
                //    else if(meleeRoundCount == 6)
                //    {
                //        // Start melee round
                //    }
                    
                //    meleeRoundCount++;
                //}
            }
            else
            {
                players[0].GetComponent<PlayerInfo>().playTimes = turnsrem!=-1?turnsrem:4;
                playsLeftText.text = "" + players[0].GetComponent<PlayerInfo>().playTimes;
            }
            
            infoPanel.GetComponentInChildren<Text>().text = RemoveGameoptionName(MainUI.playerNames[0]) + "'s Turn!";
            StartCoroutine(DelayShowInfo(turnsrem==-1));
        }
        else if (whoseTurn == (int)TurnIndex.Player2)
        {
            if (NumbersOfAvailableUnits() == 1)
            {
                players[1].GetComponent<PlayerInfo>().playTimes = turnsrem != -1 ? turnsrem : 2;
                playsLeftText.text = "" + players[1].GetComponent<PlayerInfo>().playTimes;
            }
            else
            {
                players[1].GetComponent<PlayerInfo>().playTimes = turnsrem != -1 ? turnsrem : 4;
                playsLeftText.text = "" + players[1].GetComponent<PlayerInfo>().playTimes;
            }
                
            infoPanel.GetComponentInChildren<Text>().text = RemoveGameoptionName(MainUI.playerNames[1]) + "'s Turn!";
            StartCoroutine(DelayShowInfo(turnsrem == -1));
        }

        moveButton.SetActive(false);
        fireButton.SetActive(false);
        setSubButton(false);
        sonarButton.SetActive(false);
        endTurnButton.SetActive(false);
        endPlayButton.SetActive(false);
        pingButton.SetActive(false);
        subFireButton.SetActive(false);

        ufoFireButton.SetActive(false);
        ufoButton.SetActive(false);
        ufoDialog.SetActive(false);
        blackDragonButton.SetActive(false);

        //StartCoroutine(DelayDice61Show());

        if ((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame))
        {
            for (int i = 0; i < MainUI.gameOptions.Length; i++)
            {
                if (MainUI.gameOptions[i] == '@')
                {
                    ufoButton.SetActive(true);
                }

                //if (MainUI.gameOptions[i] == '&')
                //{
                //    blackDragonButton.SetActive(true);
                //}
            }
        }
        if(turnsrem<=-1)
        turnCount++;

        if (turnCount % 5 == 0 && turnCount >= 10)
        {
            volcFireParticle = Instantiate(volcFirePrefab, volcano.transform.position, Quaternion.identity);
            volcFireParticle.transform.rotation = Quaternion.Euler(180f, 0, 0);
            volcFireParticle.Play();

            volcSmokeParticle = Instantiate(volcSmokePrefab, volcano.transform.position, Quaternion.identity);
            volcSmokeParticle.transform.rotation = Quaternion.Euler(180f, 0, 0);
            volcSmokeParticle.Play();
        }

        nextCount = 0;
        turnCountText.text = "" + turnCount;
    }

    public string RemoveGameoptionName(string str)
    {
        string tempNames = "";
        //int tempNameCount = 0;

        if(str.Split(",").Length == 1)
        {
            tempNames = str;
        }
        else
        {
            tempNames = str.Split(",")[0];
        }        

        //while (true)
        //{
        //    // For warzoom.com
        //    //if (str.Split(" ")[tempNameCount].Substring(0, 1)[0] == 'L' &&
        //    //    str.Split(" ")[tempNameCount].Length > 1 &&
        //    //    char.IsDigit(str.Split(" ")[tempNameCount].Substring(1, 1)[0]))
        //    //{
        //    //    break;
        //    //}

        //    //tempNames += str.Split(" ")[tempNameCount];
        //    //tempNameCount++;
        //    //tempNames += " ";

        //    // wargrids.com
        //    if (str.Split(" ").Length >= 2)
        //    {
        //        if (str.Split(" ")[tempNameCount] == "UFO,"
        //            || str.Split(" ")[tempNameCount] == "BT,"
        //            || str.Split(" ")[tempNameCount] == "AM,"
        //            || str.Split(" ")[tempNameCount] == "MS,")
        //        {
        //            break;
        //        }

        //        tempNames += str.Split(" ")[tempNameCount];
        //        tempNameCount++;
        //        tempNames += " ";
        //    }
        //    else
        //    {
        //        tempNames += str.Split(" ")[tempNameCount];
        //        tempNames = tempNames.Split(",")[0];
        //        break;
        //    }

        //    //if (str.Split(" ")[tempNameCount] == "UFO,"
        //    //        || str.Split(" ")[tempNameCount] == "BT,"
        //    //        || str.Split(" ")[tempNameCount] == "AM,"
        //    //        || str.Split(" ")[tempNameCount] == "MS,")
        //    //{
        //    //    break;
        //    //}

        //    //tempNames += str.Split(" ")[tempNameCount];
        //    //tempNameCount++;
        //    //tempNames += " ";
        //}

        return tempNames;
    }

    public void EndPlayAction()
    {
        isSelected = false;
        isMoveAnimation = false;
        isOnlyOncePlay = false;
        isEnemy = false;
        isAvailableAttack = false;
        moveButton.SetActive(false);
        fireButton.SetActive(false);
        setSubButton(false);
        sonarButton.SetActive(false);
        subFireButton.SetActive(false);

        pingButton.SetActive(false);
        isUFO = false;
        isUFOMove = false;
        isUFOMoving = false;
        isProgressAction = false;
        isBlackDragon = false;
        isBombLoading = false;
        exceedCount = 0;
        loadedBombs = 0;
        ufo.transform.position = new Vector3(ufoInitialPos.x * UnityEngine.Random.Range(0, 2) == 0 ? 2f : 3f, ufoInitialPos.y * UnityEngine.Random.Range(0, 2) == 0 ? 2f : 3f, ufoInitialPos.z);
        ufo.GetComponent<AudioSource>().Stop();
        ufo.SetActive(false);
        infoBlackDragon.SetActive(false);
        infoBombs.SetActive(false);

        for (int i = 0; i < atomicMines.Length; i++)
        {
            if (Vector3.Distance(atomicMines[i].transform.position, tiles[indices].transform.position) < 0.1f)
            {
                isEndedByMines = true;
                break;
            }
        }

        for (int i = 0; i < 3; i++)
        {
            infoBombs.transform.GetChild(i).gameObject.SetActive(false);
        }

        for (int i = 0; i < atomicMines.Length; i++)
        {
            atomicMines[i].GetComponent<BoxCollider>().enabled = false;
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].GetComponent<SpriteRenderer>().enabled = false;
        }

        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            if (force1ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                if (ConvertToTile(force1ForCircle[i]).GetComponent<CellInfo>().cellType == (int)CellType.Tunnel ||
                    ConvertToTile(force1ForCircle[i]).GetComponent<CellInfo>().cellType == (int)CellType.TunnelEntrance)
                {
                    tunnelColor = new Color32(69, 130, 255, 1);
                    tunnelColor.a = 0.7f;

                    if (i == 4)
                    {
                        subBacks[0].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[0].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                    else
                    {
                        subBacks[1].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[1].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                }
                else
                {
                    if (i == 4)
                    {
                        subBacks[0].GetComponent<SpriteRenderer>().enabled = false;
                    }
                    else
                    {
                        subBacks[1].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }

            if (force1ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                if (ConvertToTile(force1ForSquare[i]).GetComponent<CellInfo>().cellType == (int)CellType.Tunnel ||
                    ConvertToTile(force1ForSquare[i]).GetComponent<CellInfo>().cellType == (int)CellType.TunnelEntrance)
                {
                    tunnelColor = new Color32(69, 130, 255, 1);
                    tunnelColor.a = 0.7f;

                    if (i == 4)
                    {
                        subBacks[2].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[2].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                    else
                    {
                        subBacks[3].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[3].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                }
                else
                {
                    if (i == 4)
                    {
                        subBacks[2].GetComponent<SpriteRenderer>().enabled = false;
                    }
                    else
                    {
                        subBacks[3].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }

            if (force2ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                if (ConvertToTile(force2ForCircle[i]).GetComponent<CellInfo>().cellType == (int)CellType.Tunnel ||
                    ConvertToTile(force2ForCircle[i]).GetComponent<CellInfo>().cellType == (int)CellType.TunnelEntrance)
                {
                    tunnelColor = new Color32(69, 130, 255, 1);
                    tunnelColor.a = 0.7f;

                    if (i == 4)
                    {
                        subBacks[4].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[4].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                    else
                    {
                        subBacks[5].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[5].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                }
                else
                {
                    if (i == 4)
                    {
                        subBacks[4].GetComponent<SpriteRenderer>().enabled = false;
                    }
                    else
                    {
                        subBacks[5].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }

            if (force2ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                if (ConvertToTile(force2ForSquare[i]).GetComponent<CellInfo>().cellType == (int)CellType.Tunnel ||
                    ConvertToTile(force2ForSquare[i]).GetComponent<CellInfo>().cellType == (int)CellType.TunnelEntrance)
                {
                    tunnelColor = new Color32(69, 130, 255, 1);
                    tunnelColor.a = 0.7f;

                    if (i == 4)
                    {
                        subBacks[6].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[6].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                    else
                    {
                        subBacks[7].GetComponent<SpriteRenderer>().enabled = true;
                        subBacks[7].GetComponent<SpriteRenderer>().color = tunnelColor;
                    }
                }
                else
                {
                    if (i == 4)
                    {
                        subBacks[6].GetComponent<SpriteRenderer>().enabled = false;
                    }
                    else
                    {
                        subBacks[7].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }
        }

        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            target1ForCircle[i].SetActive(false);
            target1ForSquare[i].SetActive(false);
            target2ForCircle[i].SetActive(false);
            target2ForSquare[i].SetActive(false);
        }

        if (whoseTurn == (int)TurnIndex.Player1)
        {
            if (!InteractiveTutorial.TutorialActive)
                players[0].GetComponent<PlayerInfo>().playTimes--;
            playsLeftText.text = "" + players[0].GetComponent<PlayerInfo>().playTimes;
        }
        else if (whoseTurn == (int)TurnIndex.Player2)
        {
            if (!InteractiveTutorial.TutorialActive)
                players[1].GetComponent<PlayerInfo>().playTimes--;
            playsLeftText.text = "" + players[1].GetComponent<PlayerInfo>().playTimes;
        }

        selectedUnit.GetComponent<ForceInfo>().controlTimes++;

        if (selectedUnit.GetComponent<ForceInfo>().controlTimes == 2)
        {
            selectedUnit.GetComponent<Animator>().enabled = false;
        }

        isMove = false;
        isFire = false;
        isSub = false;
        isSonar = false;
        endPlayButton.SetActive(false);
        pingButton.SetActive(false);
        subFireButton.SetActive(false);

        ufoFireButton.SetActive(false);
        ufoButton.SetActive(false);
        ufoDialog.SetActive(false);
        blackDragonButton.SetActive(false);
        //StartCoroutine(DelayDice61Show());

        if ((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame))
        {
            for (int i = 0; i < MainUI.gameOptions.Length; i++)
            {
                if (MainUI.gameOptions[i] == '@')
                {
                    ufoButton.SetActive(true);
                }

                //if (MainUI.gameOptions[i] == '&')
                //{
                //    blackDragonButton.SetActive(true);
                //}
            }
        }

        if (whoseTurn == (int)TurnIndex.Player1 && players[0].GetComponent<PlayerInfo>().playTimes == 0 ||
            whoseTurn == (int)TurnIndex.Player2 && players[1].GetComponent<PlayerInfo>().playTimes == 0)
        {
            for (int i = 0; i < force1ForCircle.Length; i++)
            {
                force1ForCircle[i].GetComponent<Animator>().enabled = false;
                force1ForSquare[i].GetComponent<Animator>().enabled = false;
                force2ForCircle[i].GetComponent<Animator>().enabled = false;
                force2ForSquare[i].GetComponent<Animator>().enabled = false;
            }

            ufoButton.SetActive(false);

            if (!isEndedByMines)
            {
                StartCoroutine(DelayTurn());
            }
        }
    }

    public void FireAction(GameObject cell)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (Vector3.Distance(tiles[i].transform.position, cell.transform.position) < 0.1f)
            {
                indices = i;
            }
        }

        if (IsEmpty(tiles[indices]))
        {
            if (tiles[indices].GetComponent<CellInfo>().cellType == (int)CellType.Bridge)
            {
                bridges[tiles[indices].GetComponent<CellInfo>().bridgeIndex].GetComponent<SpriteRenderer>().sprite = bridgeSprites[1];
                tiles[indices].GetComponent<CellInfo>().cellType = (int)CellType.Sea;
                fireParticle = Instantiate(firePrefab, tiles[indices].transform.position, Quaternion.identity);
                fireParticle.Play();
            }
        }

        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            if (whoseTurn == (int)TurnIndex.Player1)
            {
                if (Vector3.Distance(cell.transform.position, force2ForCircle[i].transform.position) < 0.1f)
                {
                    if (target2ForCircle[i].activeSelf == true)
                    {
                        fireParticle = Instantiate(firePrefab, targetObject.transform.position, Quaternion.identity);
                        fireParticle.Play();
                        force2ForCircle[i].transform.position = new Vector3(10000f, 10000f, -8f);
                        force2ForCircle[i].SetActive(false);

                        if (GeneralNumberOfEnemy() == 0)
                        {
                            if ((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame))
                            {
                                StartCoroutine(ShowWinningDialog(1));
                            }
                        }

                        if (GeneralNumberofLegion(force2ForCircle[i].GetComponent<ForceInfo>().legionIndex) == 0)
                        {
                            if (isMasterInGame)
                            {
                                SyncKillGeneralEnimies(new object[] { 2 });
                            }
                        }

                        break;
                    }
                    else
                    {
                        if (force2ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && cell.GetComponent<CellInfo>().cellType != (int)CellType.Tunnel)
                        {
                            // Do Operation Serpent
                            OperationSerpent(2);
                            
                            fireParticle = Instantiate(firePrefab, targetObject.transform.position, Quaternion.identity);
                            fireParticle.Play();
                            force2ForCircle[i].transform.position = new Vector3(10000f, 10000f, -8f);
                            force2ForCircle[i].SetActive(false);

                            break;
                        }
                    }
                }

                if (Vector3.Distance(cell.transform.position, force2ForSquare[i].transform.position) < 0.1f)
                {
                    if (target2ForSquare[i].activeSelf == true)
                    {
                        fireParticle = Instantiate(firePrefab, targetObject.transform.position, Quaternion.identity);
                        fireParticle.Play();
                        force2ForSquare[i].transform.position = new Vector3(10000f, 10000f, -8f);
                        force2ForSquare[i].SetActive(false);

                        if (GeneralNumberOfEnemy() == 0)
                        {
                            if ((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame))
                            {
                                StartCoroutine(ShowWinningDialog(1));
                            }
                        }

                        if (GeneralNumberofLegion(force2ForSquare[i].GetComponent<ForceInfo>().legionIndex) == 0)
                        {
                            if (isMasterInGame)
                            {
                                SyncKillGeneralEnimies(new object[] { 3 });
                            }
                        }

                        break;
                    }
                    else
                    {
                        if (force2ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && cell.GetComponent<CellInfo>().cellType != (int)CellType.Tunnel)
                        {
                            // Do Operation Serpent
                            OperationSerpent(2);

                            fireParticle = Instantiate(firePrefab, targetObject.transform.position, Quaternion.identity);
                            fireParticle.Play();
                            force2ForSquare[i].transform.position = new Vector3(10000f, 10000f, -8f);
                            force2ForSquare[i].SetActive(false);

                            break;
                        }
                    }
                }
            }
            else if (whoseTurn == (int)TurnIndex.Player2)
            {
                if (Vector3.Distance(cell.transform.position, force1ForCircle[i].transform.position) < 0.1f)
                {
                    if (target1ForCircle[i].activeSelf == true)
                    {
                        fireParticle = Instantiate(firePrefab, targetObject.transform.position, Quaternion.identity);
                        fireParticle.Play();
                        force1ForCircle[i].transform.position = new Vector3(10000f, 10000f, -8f);
                        force1ForCircle[i].SetActive(false);

                        if (GeneralNumberOfEnemy() == 0)
                        {
                            StartCoroutine(ShowWinningDialog(1));
                        }

                        if (GeneralNumberofLegion(force1ForCircle[i].GetComponent<ForceInfo>().legionIndex) == 0)
                        {
                            if (!isMasterInGame)
                            {
                                SyncKillGeneralEnimies(new object[] { 0 });
                            }
                        }

                        break;
                    }
                    else
                    {
                        if (force1ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && cell.GetComponent<CellInfo>().cellType != (int)CellType.Tunnel)
                        {
                            // Do Operation Serpent
                            OperationSerpent(1);

                            fireParticle = Instantiate(firePrefab, targetObject.transform.position, Quaternion.identity);
                            fireParticle.Play();
                            force1ForCircle[i].transform.position = new Vector3(10000f, 10000f, -8f);
                            force1ForCircle[i].SetActive(false);

                            break;
                        }
                    }
                }

                if (Vector3.Distance(cell.transform.position, force1ForSquare[i].transform.position) < 0.1f)
                {
                    if (target1ForSquare[i].activeSelf == true)
                    {
                        fireParticle = Instantiate(firePrefab, targetObject.transform.position, Quaternion.identity);
                        fireParticle.Play();
                        force1ForSquare[i].transform.position = new Vector3(10000f, 10000f, -8f);
                        force1ForSquare[i].SetActive(false);

                        if (GeneralNumberOfEnemy() == 0)
                        {
                            if ((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame))
                            {
                                StartCoroutine(ShowWinningDialog(1));
                            }
                        }

                        if (GeneralNumberofLegion(force1ForSquare[i].GetComponent<ForceInfo>().legionIndex) == 0)
                        {
                            if (!isMasterInGame)
                            {
                                SyncKillGeneralEnimies(new object[] { 1 });
                            }
                        }

                        break;
                    }
                    else
                    {
                        if (force1ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && cell.GetComponent<CellInfo>().cellType != (int)CellType.Tunnel)
                        {
                            // Do Operation Serpent
                            OperationSerpent(1);

                            fireParticle = Instantiate(firePrefab, targetObject.transform.position, Quaternion.identity);
                            fireParticle.Play();
                            force1ForSquare[i].transform.position = new Vector3(10000f, 10000f, -8f);
                            force1ForSquare[i].SetActive(false);

                            break;
                        }
                    }
                }
            }
        }

        if (whoseTurn == (int)TurnIndex.Player1)
        {
            for (int i = 0; i < 2; i++)
            {
                if (force1ForCircle[i].transform.position != new Vector3(10000f, 10000f, -8f))
                {
                    generalFaces[i].SetActive(true);
                }

                if (force1ForSquare[i].transform.position != new Vector3(10000f, 10000f, -8f))
                {
                    generalFaces[i + 2].SetActive(true);
                }
            }
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                if (force2ForCircle[i].transform.position != new Vector3(10000f, 10000f, -8f))
                {
                    generalFaces[i].SetActive(true);
                }

                if (force2ForSquare[i].transform.position != new Vector3(10000f, 10000f, -8f))
                {
                    generalFaces[i + 2].SetActive(true);
                }
            }
        }

        SetAudio(1);
        selectedUnit.GetComponent<AudioSource>().loop = false;
        selectedUnit.GetComponent<AudioSource>().Play();
        ShowGamePlayInfo(1);
        EndPlayAction();
    }

    public void SonarAction()
    {
        EndPlayAction();
    }

    public void SubAction(GameObject cell)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (Vector3.Distance(tiles[i].transform.position, cell.transform.position) < 0.1f)
            {
                indices = i;
                //print("" + i + "th tile's Sub attack action");      
            }
        }

        for (int i = 0; i < force1ForCircle.Length; i++)
        {
            if (whoseTurn == (int)TurnIndex.Player1)
            {
                if (Vector3.Distance(cell.transform.position, force2ForCircle[i].transform.position) < 0.1f && force2ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && cell.GetComponent<CellInfo>().cellType != (int)CellType.Tunnel)
                {
                    SyncSubFire(new object[] { i, 2 });

                    for (int j = 0; j < tiles.Length; j++)
                    {
                        tiles[j].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }

                if (Vector3.Distance(cell.transform.position, force2ForCircle[i].transform.position) < 0.1f && force2ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && ConvertToTile(selectedUnit).GetComponent<CellInfo>().cellType == (int)CellType.TunnelEntrance)
                {
                    SyncSubFire(new object[] { i, 2 });

                    for (int j = 0; j < tiles.Length; j++)
                    {
                        tiles[j].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }

                if (Vector3.Distance(cell.transform.position, force2ForSquare[i].transform.position) < 0.1f && force2ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && cell.GetComponent<CellInfo>().cellType != (int)CellType.Tunnel)
                {
                    SyncSubFire(new object[] { i, 3 });

                    for (int j = 0; j < tiles.Length; j++)
                    {
                        tiles[j].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }

                if (Vector3.Distance(cell.transform.position, force2ForSquare[i].transform.position) < 0.1f && force2ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && ConvertToTile(selectedUnit).GetComponent<CellInfo>().cellType == (int)CellType.TunnelEntrance)
                {
                    SyncSubFire(new object[] { i, 3 });

                    for (int j = 0; j < tiles.Length; j++)
                    {
                        tiles[j].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }
            else if (whoseTurn == (int)TurnIndex.Player2)
            {
                if (Vector3.Distance(cell.transform.position, force1ForCircle[i].transform.position) < 0.1f && force1ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && cell.GetComponent<CellInfo>().cellType != (int)CellType.Tunnel)
                {
                    SyncSubFire(new object[] { i, 0 });

                    for (int j = 0; j < tiles.Length; j++)
                    {
                        tiles[j].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }

                if (Vector3.Distance(cell.transform.position, force1ForCircle[i].transform.position) < 0.1f && force1ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && ConvertToTile(selectedUnit).GetComponent<CellInfo>().cellType == (int)CellType.TunnelEntrance)
                {
                    SyncSubFire(new object[] { i, 0 });

                    for (int j = 0; j < tiles.Length; j++)
                    {
                        tiles[j].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }

                if (Vector3.Distance(cell.transform.position, force1ForSquare[i].transform.position) < 0.1f && force1ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && cell.GetComponent<CellInfo>().cellType != (int)CellType.Tunnel)
                {
                    SyncSubFire(new object[] { i, 1 });

                    for (int j = 0; j < tiles.Length; j++)
                    {
                        tiles[j].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }

                if (Vector3.Distance(cell.transform.position, force1ForSquare[i].transform.position) < 0.1f && force1ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && ConvertToTile(selectedUnit).GetComponent<CellInfo>().cellType == (int)CellType.TunnelEntrance)
                {
                    SyncSubFire(new object[] { i, 1 });

                    for (int j = 0; j < tiles.Length; j++)
                    {
                        tiles[j].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }
        }
    }

    public void UFOAction(GameObject cell)
    {
        if (cell.GetComponent<ForceInfo>().forceType == (int)ForceType.Plane && ConvertToTile(cell).GetComponent<CellInfo>().cellType == (int)CellType.Mountain)
        {
            fireParticle = Instantiate(firePrefab, cell.transform.position, Quaternion.identity);
            fireParticle.Play();
            cell.transform.position = new Vector3(10000f, 10000f, -8f);
            cell.SetActive(false);

            SetAudio(1);
            selectedUnit.GetComponent<AudioSource>().loop = false;
            selectedUnit.GetComponent<AudioSource>().Play();
        }
    }

    public void setSubNow()
    {
        if (subAvailability.Instance != null)
        {
            Debug.Log("set2 a : " + subAvailability.Instance.available);
            operationSerpentDialog.SetActive(subAvailability.Instance.available);
        }
        else
        {
            Debug.Log("set2 b : " + false);
            operationSerpentDialog.SetActive(false);
        }
    }
    public void OperationSerpent(int destroyedTeam)
    {

        operationSerpentDialog.SetActive(false);

        if (isMasterInGame && destroyedTeam == 2)
        {                        
            subCount2++;

            for(int i = 0; i < subCount2; i++)
            {
               
                oSSubs[i].SetActive(true);
            }
        }
        else if(!isMasterInGame && destroyedTeam == 1)
        {
            subCount1++;

            for (int i = 0; i < subCount1; i++)
            {
               
                oSSubs[i].SetActive(true);
            }
        }
  
        if (subAvailability.Instance != null)
        {
            subAvailability.Instance.GetUpdatedData();

        }
    }

    public void EndTurn()
    {
       // if (waitingforserverTurnChange == false)
        {
            isSelectedSync = false;
            isChangingTurn = true;


     
            bootingObject.GetComponent<GameSettingsApplier>().resetTimer();
            bootingObject.GetComponent<GameSettingsApplier>().setTimerActive(false);
            waitingforserverTurnChange = true;

            if (whoseTurn == (int)TurnIndex.Player1 && isMasterInGame)
            {
                Debug.LogError("End Turn turn 1 ");

                SyncTurn(new object[] { 1 });
                //    whoseTurn = 1;
            }
            else if (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)
            {
                Debug.LogError("End Turn turn 0 ");

                SyncTurn(new object[] { 0 });
                //  whoseTurn = 0;
            }
        }
      
    }

    public void ExitGame()
    {
        //whoseTurn = (whoseTurn + 1) % 2;
        isClickExit = true;
        StartCoroutine(ShowWinningDialog(0));

        //StartCoroutine(DelayGoToMainScene(0.5f));
    }

    public void ShowGamePlayInfo(int index)
    {
        switch (index)
        {
            case 0:
                {
                    infoGamePlayPanel.GetComponentInChildren<Text>().color = Color.green;

                    switch (selectedUnit.GetComponent<ForceInfo>().forceType)
                    {
                        case (int)ForceType.General:
                            {
                                infoGamePlayPanel.GetComponentInChildren<Text>().text = "General is moving";
                                StartCoroutine(DelayShowInfoGamePlay(0));
                                break;
                            }
                        case (int)ForceType.Tank:
                            {
                                infoGamePlayPanel.GetComponentInChildren<Text>().text = "Tank is moving";
                                StartCoroutine(DelayShowInfoGamePlay(0));
                                break;
                            }
                        case (int)ForceType.Plane:
                            {
                                infoGamePlayPanel.GetComponentInChildren<Text>().text = "Jet is moving";
                                StartCoroutine(DelayShowInfoGamePlay(0));
                                break;
                            }
                        case (int)ForceType.Sub:
                            {
                                infoGamePlayPanel.GetComponentInChildren<Text>().text = "Sub is moving";
                                StartCoroutine(DelayShowInfoGamePlay(0));
                                break;
                            }
                    }
                    break;
                }
            case 1:
                {
                    infoGamePlayPanel.GetComponentInChildren<Text>().color = Color.red;


                    switch (selectedUnit.GetComponent<ForceInfo>().forceType)
                    {
                        case (int)ForceType.General:
                            {
                                infoGamePlayPanel.GetComponentInChildren<Text>().text = "General fired!";
                                StartCoroutine(DelayShowInfoGamePlay(1));
                                break;
                            }
                        case (int)ForceType.Tank:
                            {
                                infoGamePlayPanel.GetComponentInChildren<Text>().text = "Tank fired!";
                                StartCoroutine(DelayShowInfoGamePlay(1));
                                break;
                            }
                        case (int)ForceType.Plane:
                            {
                                infoGamePlayPanel.GetComponentInChildren<Text>().text = "Jet fired!";
                                StartCoroutine(DelayShowInfoGamePlay(1));
                                break;
                            }
                        case (int)ForceType.Sub:
                            {
                                infoGamePlayPanel.GetComponentInChildren<Text>().text = "Sub fired!";
                                StartCoroutine(DelayShowInfoGamePlay(1));
                                break;
                            }
                    }
                    break;
                }
            case 2:
                {
                    infoGamePlayPanel.GetComponentInChildren<Text>().color = Color.red;

                    switch (selectedUnit.GetComponent<ForceInfo>().forceType)
                    {
                        case (int)ForceType.General:
                            {
                                infoGamePlayPanel.GetComponentInChildren<Text>().text = "General fired at Sub!";
                                StartCoroutine(DelayShowInfoGamePlay(2));
                                break;
                            }
                        case (int)ForceType.Tank:
                            {
                                infoGamePlayPanel.GetComponentInChildren<Text>().text = "Tank fired at Sub!";
                                StartCoroutine(DelayShowInfoGamePlay(2));
                                break;
                            }
                        case (int)ForceType.Plane:
                            {
                                infoGamePlayPanel.GetComponentInChildren<Text>().text = "Jet fired at Sub!";
                                StartCoroutine(DelayShowInfoGamePlay(2));
                                break;
                            }
                        case (int)ForceType.Sub:
                            {
                                infoGamePlayPanel.GetComponentInChildren<Text>().text = "Sub fired at Sub!";
                                StartCoroutine(DelayShowInfoGamePlay(2));
                                break;
                            }
                    }
                    break;
                }
            case 3:
                {
                    infoGamePlayPanel.GetComponentInChildren<Text>().color = Color.magenta;

                    switch (selectedUnit.GetComponent<ForceInfo>().forceType)
                    {
                        case (int)ForceType.Sub:
                            {
                                infoGamePlayPanel.GetComponentInChildren<Text>().text = "Sub using Sonar!";
                                StartCoroutine(DelayShowInfoGamePlay(3));
                                break;
                            }
                    }
                    break;
                }
        }
    }

    IEnumerator DelayShowInfo(bool shouldshow)
    {
        if (shouldshow)
        {
            infoPanel.SetActive(true);

            yield return new WaitForSeconds(3f);

            infoPanel.SetActive(false);
        }
    }

    public void SyncTurn(object[] content)
    {
        Debug.Log("Raising Event 1 change turn event");
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(1, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncSelectedUnit(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(2, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncMoveClick(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(3, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncFireClick(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(4, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncMoveAction(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(5, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncFireAction(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(6, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncAttack(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(7, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncDiceAnimation(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(8, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncSonarClick(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(9, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncSubClick(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(10, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncSubAction(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(11, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncEndPlay(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(12, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncPingClick(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(13, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncSubFire(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(14, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncUFOCall(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(15, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncUFORange(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(16, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncUFOAction(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(17, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncUFODiceAnimation(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(18, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncDoubleSelectedUnit(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(19, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncSkin(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(20, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncPlacement(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(21, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncFirstRoll(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(22, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncGameEnd(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(23, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncTurnMap(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(24, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncKillGeneralEnimies(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(25, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncHitOrMiss(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(26, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncTriangles(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(27, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncBermuda(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(28, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncMines(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(29, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncMinesAction(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(30, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncBlackDragonCall(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(31, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncLoadBombs(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(32, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncPlayersPositions(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(33, content, raiseEventOptions, SendOptions.SendReliable);
    }
    #endregion

    #region MonoBehaviourPunCallbacks
    //public void DoSomething()
    //{
    //    if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
    //    {
    //        return;
    //    }

    //    PhotonNetwork.NetworkingClient.LoadBalancingPeer.DispatchIncomingCommands();
    //    PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();
    //}

    private bool CanRecoverFromDisconnect(DisconnectCause cause)
    {
        switch (cause)
        {
            // the list here may be non exhaustive and is subject to review
            case DisconnectCause.Exception:
            case DisconnectCause.ServerTimeout:
            case DisconnectCause.ClientTimeout:
            case DisconnectCause.DisconnectByServerLogic:
            case DisconnectCause.DisconnectByServerReasonUnknown:
                return true;
        }
        return false;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (this.CanRecoverFromDisconnect(cause))
        {

        }
        else
        {
            //StartCoroutine(PostEndedRoom(MainUI.frontURL + "/api/endPlaying"));
            SceneManager.LoadScene("Main");
            print("Disconnected.");
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        playersInRoom = 1;
        reconnectingText.SetActive(true);
        isDisableMouseClicks = true;  
        StartCoroutine(DelayToCheckPlayerLeftRoom());
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        playersInRoom = 2;
        isDisableMouseClicks = false;     
        reconnectingText.SetActive(false);
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
    bool firstturn = false;
    bool winset = false;
    public void OnEvent(EventData photonEvent)
    {

        byte eventCode = photonEvent.Code;
 
        if (eventCode == 1)
        {
            
            object[] infos = (object[])photonEvent.CustomData;

            int lastturn = (int)whoseTurn;  //1

            if (infos.Length > 0)
            {
                if ((int)infos[0] == 0)
                {
                    whoseTurn = (int)TurnIndex.Player1;
                    if (players[whoseTurn].GetComponent<PlayerInfo>().playTimes <= 0)
                    {
                        bootingObject.GetComponent<GameSettingsApplier>().resetTimer();
                    }
                }
                else
                {
                    whoseTurn = (int)TurnIndex.Player2;
                    if (players[whoseTurn].GetComponent<PlayerInfo>().playTimes <= 0)
                    {
                        bootingObject.GetComponent<GameSettingsApplier>().resetTimer();
                    }
                }


                // if (whoseTurn != lastturn)
                {





                    if (infos.Length >= 2)
                    {

                        bootingObject.GetComponent<GameSettingsApplier>().setTimerActive(false);

                        if ((int)infos[1] > 0)
                        {
                            ChangeTurn((int)infos[1]);
                        }
                    }
                    else
                    {
                        bootingObject.GetComponent<GameSettingsApplier>().setTimerActive(false);

                        bootingObject.GetComponent<GameSettingsApplier>().resetTimer();
                        ChangeTurn();
                    }

                    waitingforserverTurnChange = false;
                }
            }
       
        }
        else if (eventCode == 2)
        {
            object[] infos = (object[])photonEvent.CustomData;
            isIncreasedTimes = false;

            if ((int)infos[1] == 0)
            {
                selectedUnit = force1ForCircle[(int)infos[0]];
            }
            else if ((int)infos[1] == 1)
            {
                selectedUnit = force1ForSquare[(int)infos[0]];
            }
            else if ((int)infos[1] == 2)
            {
                selectedUnit = force2ForCircle[(int)infos[0]];
            }
            else if ((int)infos[1] == 3)
            {
                selectedUnit = force2ForSquare[(int)infos[0]];
            }

            for (int i = 0; i < tiles.Length; i++)
            {
                if (Vector3.Distance(selectedUnit.transform.position, tiles[i].transform.position) < 0.1f)
                {
                    selectedX = i / 30;
                    selectedY = i % 30;
                }
            }

            moveButton.SetActive(false);
            fireButton.SetActive(false);
            setSubButton(false);
            sonarButton.SetActive(false);

            for (int i = 0; i < force1ForCircle.Length; i++)
            {
                force1ForCircle[i].GetComponent<AudioSource>().Stop();
                force1ForSquare[i].GetComponent<AudioSource>().Stop();
                force2ForCircle[i].GetComponent<AudioSource>().Stop();
                force2ForSquare[i].GetComponent<AudioSource>().Stop();
            }

            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].GetComponent<SpriteRenderer>().enabled = false;
            }

            // Show yellow tiles
            if ((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame) ||
                (whoseTurn == (int)TurnIndex.Player1 && !isMasterInGame && selectedUnit.gameObject.GetComponent<ForceInfo>().forceType != (int)ForceType.Sub) ||
                (whoseTurn == (int)TurnIndex.Player2 && isMasterInGame && selectedUnit.gameObject.GetComponent<ForceInfo>().forceType != (int)ForceType.Sub) ||
                (selectedUnit.gameObject.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && selectedUnit.gameObject.GetComponent<SpeciaInfo>().specialType == (int)SpecialType.Bunker))
            {
                Color selectedColor = new Color32(254, 254, 0, 1);
                selectedColor.a = 1f;
                ConvertToTile(selectedUnit).GetComponent<SpriteRenderer>().enabled = true;
                ConvertToTile(selectedUnit).GetComponent<SpriteRenderer>().color = selectedColor;
            }

            SetAudio(0);
            selectedUnit.GetComponent<AudioSource>().loop = true;
            selectedUnit.GetComponent<AudioSource>().Play();

            if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
            {
                if (!isUFO)
                {
                    if (selectedUnit.gameObject.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
                    {
                        moveButton.SetActive(true);
                        fireButton.SetActive(true);
                        setSubButton(true);
                        sonarButton.SetActive(true);
                        ufoButton.SetActive(false);
                        blackDragonButton.SetActive(false);
                    }
                    else
                    {
                        moveButton.SetActive(true);
                        fireButton.SetActive(true);
                        setSubButton(true);
                        ufoButton.SetActive(false);
                        blackDragonButton.SetActive(false);
                    }
                }
            }
            else
            {
                if (selectedUnit.GetComponent<ForceInfo>().forceType != (int)ForceType.Sub)
                {
                    if (isSmartMapEnabled)
                        cameraView.transform.position = new Vector3(selectedUnit.transform.position.x, selectedUnit.transform.position.y, cameraView.transform.position.z);
                }
            }

            if (selectedUnit.GetComponent<SpeciaInfo>().specialType == (int)SpecialType.Bunker)
            {
                // Resume bunker sub                
                if (selectedUnit.GetComponent<SpeciaInfo>().bunkerCount != 0)
                {
                    moveButton.SetActive(false);
                }
                else
                {
                    //if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Tank)
                    //{
                    //    selectedUnit.GetComponent<SpeciaInfo>().specialType = (int)SpecialType.Normal;
                    //}
                    //else if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
                    //{
                    //    moveButton.SetActive(false);
                    //}

                    if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Tank)
                    {
                        moveButton.SetActive(false);
                    }

                    //selectedUnit.GetComponent<SpeciaInfo>().specialType = (int)SpecialType.Normal;
                }

                //moveButton.SetActive(false);
            }
        }
        else if (eventCode == 3)
        {
            isMove = true;
            isSelected = true;
            currentTurnIndex = whoseTurn;
            StartCoroutine(DiceAnimationForMove(selectedUnit.GetComponent<ForceInfo>().movementRange));
        }
        else if (eventCode == 4)
        {
            isFire = true;
            isSelected = true;
            currentTurnIndex = whoseTurn;
            StartCoroutine(DiceAnimationForFire(selectedUnit.GetComponent<ForceInfo>().fireRange));
        }
        else if (eventCode == 5)
        {
            object[] infos = (object[])photonEvent.CustomData;
            isMove = false;
            targetObject = tiles[(int)infos[0]];
            pathTiles = new List<GameObject>();
            isPath = false;

            selectedUnit.GetComponent<AudioSource>().loop = false;
            selectedUnit.GetComponent<AudioSource>().Play();

            #region blank spot
            if (Vector3.Distance(selectedUnit.transform.position, tiles[0].transform.position) < 0.1f)
            {
                if (!isPath)
                {
                    isPath = true;

                    if ((int)infos[0] == 2 || (int)infos[0] == 32 || (int)infos[0] == 60 || (int)infos[0] == 61 ||
                        (int)infos[0] == 62)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[31]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                    else if ((int)infos[0] == 28 || (int)infos[0] == 58 || (int)infos[0] == 88 || (int)infos[0] == 89)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[59]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                    else if ((int)infos[0] == 840 || (int)infos[0] == 841 || (int)infos[0] == 842 || (int)infos[0] == 872)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[871]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                    else if ((int)infos[0] == 868 || (int)infos[0] == 869 || (int)infos[0] == 888)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[899]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                    else
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[1].transform.position) < 0.1f &&
                ((int)infos[0] == 29 || (int)infos[0] == 59 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[0]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[2].transform.position) < 0.1f &&
                ((int)infos[0] == 29 || (int)infos[0] == 59 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[0]);
                    pathTiles.Add(tiles[1]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[3].transform.position) < 0.1f &&
                ((int)infos[0] == 29 || (int)infos[0] == 59 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;

                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[0]);
                    pathTiles.Add(tiles[1]);
                    pathTiles.Add(tiles[2]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[25].transform.position) < 0.1f &&
                ((int)infos[0] == 1 || (int)infos[0] == 31 || (int)infos[0] == 835 || (int)infos[0] == 871))
            {
                if (!isPath)
                {
                    isPath = true;

                    if ((int)infos[0] == 835)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[865]);
                        pathTiles.Add(tiles[895]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                    else
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[0]);
                        pathTiles.Add(tiles[29]);
                        pathTiles.Add(tiles[28]);
                        pathTiles.Add(tiles[27]);
                        pathTiles.Add(tiles[26]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[26].transform.position) < 0.1f &&
                ((int)infos[0] == 1 || (int)infos[0] == 31 || (int)infos[0] == 835 || (int)infos[0] == 871))
            {
                if (!isPath)
                {
                    isPath = true;

                    if ((int)infos[0] == 835)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[865]);
                        pathTiles.Add(tiles[895]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                    else
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[0]);
                        pathTiles.Add(tiles[29]);
                        pathTiles.Add(tiles[28]);
                        pathTiles.Add(tiles[27]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[27].transform.position) < 0.1f &&
                ((int)infos[0] == 1 || (int)infos[0] == 31 || (int)infos[0] == 835 || (int)infos[0] == 871))
            {
                if (!isPath)
                {
                    isPath = true;

                    if ((int)infos[0] == 835)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[865]);
                        pathTiles.Add(tiles[895]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                    else
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[0]);
                        pathTiles.Add(tiles[29]);
                        pathTiles.Add(tiles[28]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[28].transform.position) < 0.1f &&
                ((int)infos[0] == 1 || (int)infos[0] == 31 || (int)infos[0] == 835 || (int)infos[0] == 871))
            {
                if (!isPath)
                {
                    isPath = true;

                    if ((int)infos[0] == 835)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[865]);
                        pathTiles.Add(tiles[895]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                    else
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[0]);
                        pathTiles.Add(tiles[29]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[29].transform.position) < 0.1f &&
                ((int)infos[0] == 1 || (int)infos[0] == 31 || (int)infos[0] == 842 || (int)infos[0] == 871))
            {
                if (!isPath)
                {
                    isPath = true;

                    if ((int)infos[0] == 31)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[1]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                    if ((int)infos[0] == 842)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[871]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                    else
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[30].transform.position) < 0.1f &&
                ((int)infos[0] == 870 || (int)infos[0] == 871 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[0]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if ((Vector3.Distance(selectedUnit.transform.position, tiles[31].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[61].transform.position) < 0.1f) &&
                ((int)infos[0] == 29 || (int)infos[0] == 898 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[0]);
                    pathTiles.Add(tiles[30]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if ((Vector3.Distance(selectedUnit.transform.position, tiles[91].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[92].transform.position) < 0.1f) &&
                (int)infos[0] == 899)
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[0]);
                    pathTiles.Add(tiles[31]);
                    pathTiles.Add(tiles[61]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if ((Vector3.Distance(selectedUnit.transform.position, tiles[32].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[62].transform.position) < 0.1f) &&
                ((int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[0]);
                    pathTiles.Add(tiles[31]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if ((Vector3.Distance(selectedUnit.transform.position, tiles[33].transform.position) < 0.1f ||
                Vector3.Distance(selectedUnit.transform.position, tiles[63].transform.position) < 0.1f) &&
                ((int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[0]);
                    pathTiles.Add(tiles[31]);
                    pathTiles.Add(tiles[32]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[92].transform.position) < 0.1f &&
                (int)infos[0] == 899)
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[0]);
                    pathTiles.Add(tiles[31]);
                    pathTiles.Add(tiles[61]);
                    pathTiles.Add(tiles[92]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[55].transform.position) < 0.1f &&
                ((int)infos[0] == 871))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[25]);
                    pathTiles.Add(tiles[870]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[56].transform.position) < 0.1f &&
                ((int)infos[0] == 871))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[26]);
                    pathTiles.Add(tiles[870]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[57].transform.position) < 0.1f &&
                ((int)infos[0] == 871))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[27]);
                    pathTiles.Add(tiles[870]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[58].transform.position) < 0.1f &&
                ((int)infos[0] == 871))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[29]);
                    pathTiles.Add(tiles[870]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[59].transform.position) < 0.1f &&
                ((int)infos[0] == 870 || (int)infos[0] == 871 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[29]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[60].transform.position) < 0.1f &&
                ((int)infos[0] == 870 || (int)infos[0] == 871 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[0]);
                    pathTiles.Add(tiles[30]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[90].transform.position) < 0.1f &&
                ((int)infos[0] == 870 || (int)infos[0] == 871 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[0]);
                    pathTiles.Add(tiles[30]);
                    pathTiles.Add(tiles[60]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[85].transform.position) < 0.1f &&
                ((int)infos[0] == 871))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[25]);
                    pathTiles.Add(tiles[55]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[86].transform.position) < 0.1f &&
                ((int)infos[0] == 871))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[26]);
                    pathTiles.Add(tiles[56]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[87].transform.position) < 0.1f &&
                ((int)infos[0] == 871))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[27]);
                    pathTiles.Add(tiles[57]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[88].transform.position) < 0.1f &&
                ((int)infos[0] == 871))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[28]);
                    pathTiles.Add(tiles[58]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[89].transform.position) < 0.1f &&
                ((int)infos[0] == 871))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[29]);
                    pathTiles.Add(tiles[59]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[115].transform.position) < 0.1f &&
                ((int)infos[0] == 871))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[25]);
                    pathTiles.Add(tiles[55]);
                    pathTiles.Add(tiles[85]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[116].transform.position) < 0.1f &&
                ((int)infos[0] == 871))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[26]);
                    pathTiles.Add(tiles[56]);
                    pathTiles.Add(tiles[86]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[117].transform.position) < 0.1f &&
                ((int)infos[0] == 871))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[27]);
                    pathTiles.Add(tiles[57]);
                    pathTiles.Add(tiles[87]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[118].transform.position) < 0.1f &&
                ((int)infos[0] == 871))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[28]);
                    pathTiles.Add(tiles[58]);
                    pathTiles.Add(tiles[88]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[119].transform.position) < 0.1f &&
                ((int)infos[0] == 871))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[29]);
                    pathTiles.Add(tiles[59]);
                    pathTiles.Add(tiles[89]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[145].transform.position) < 0.1f &&
                ((int)infos[0] == 870 || (int)infos[0] == 871 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[25]);
                    pathTiles.Add(tiles[55]);
                    pathTiles.Add(tiles[85]);
                    pathTiles.Add(tiles[115]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[146].transform.position) < 0.1f &&
                ((int)infos[0] == 870 || (int)infos[0] == 871 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[26]);
                    pathTiles.Add(tiles[56]);
                    pathTiles.Add(tiles[86]);
                    pathTiles.Add(tiles[116]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[147].transform.position) < 0.1f &&
                ((int)infos[0] == 870 || (int)infos[0] == 871 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[27]);
                    pathTiles.Add(tiles[57]);
                    pathTiles.Add(tiles[87]);
                    pathTiles.Add(tiles[117]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[148].transform.position) < 0.1f &&
                ((int)infos[0] == 870 || (int)infos[0] == 871 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[28]);
                    pathTiles.Add(tiles[58]);
                    pathTiles.Add(tiles[88]);
                    pathTiles.Add(tiles[118]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[149].transform.position) < 0.1f &&
                ((int)infos[0] == 870 || (int)infos[0] == 871 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[29]);
                    pathTiles.Add(tiles[59]);
                    pathTiles.Add(tiles[89]);
                    pathTiles.Add(tiles[119]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[175].transform.position) < 0.1f &&
                ((int)infos[0] == 870 || (int)infos[0] == 871 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[25]);
                    pathTiles.Add(tiles[55]);
                    pathTiles.Add(tiles[85]);
                    pathTiles.Add(tiles[115]);
                    pathTiles.Add(tiles[145]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[176].transform.position) < 0.1f &&
                ((int)infos[0] == 870 || (int)infos[0] == 871 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[26]);
                    pathTiles.Add(tiles[56]);
                    pathTiles.Add(tiles[86]);
                    pathTiles.Add(tiles[116]);
                    pathTiles.Add(tiles[146]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[177].transform.position) < 0.1f &&
                ((int)infos[0] == 870 || (int)infos[0] == 871 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[27]);
                    pathTiles.Add(tiles[57]);
                    pathTiles.Add(tiles[87]);
                    pathTiles.Add(tiles[117]);
                    pathTiles.Add(tiles[147]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[178].transform.position) < 0.1f &&
                ((int)infos[0] == 870 || (int)infos[0] == 871 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[28]);
                    pathTiles.Add(tiles[58]);
                    pathTiles.Add(tiles[88]);
                    pathTiles.Add(tiles[118]);
                    pathTiles.Add(tiles[148]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[179].transform.position) < 0.1f &&
                ((int)infos[0] == 870 || (int)infos[0] == 871 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[29]);
                    pathTiles.Add(tiles[59]);
                    pathTiles.Add(tiles[89]);
                    pathTiles.Add(tiles[119]);
                    pathTiles.Add(tiles[149]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[205].transform.position) < 0.1f &&
                ((int)infos[0] == 870 || (int)infos[0] == 871 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[25]);
                    pathTiles.Add(tiles[55]);
                    pathTiles.Add(tiles[85]);
                    pathTiles.Add(tiles[115]);
                    pathTiles.Add(tiles[145]);
                    pathTiles.Add(tiles[175]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[206].transform.position) < 0.1f &&
                ((int)infos[0] == 870 || (int)infos[0] == 871 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[26]);
                    pathTiles.Add(tiles[56]);
                    pathTiles.Add(tiles[86]);
                    pathTiles.Add(tiles[116]);
                    pathTiles.Add(tiles[146]);
                    pathTiles.Add(tiles[176]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[207].transform.position) < 0.1f &&
                ((int)infos[0] == 870 || (int)infos[0] == 871 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[27]);
                    pathTiles.Add(tiles[57]);
                    pathTiles.Add(tiles[87]);
                    pathTiles.Add(tiles[117]);
                    pathTiles.Add(tiles[147]);
                    pathTiles.Add(tiles[177]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[208].transform.position) < 0.1f &&
                ((int)infos[0] == 870 || (int)infos[0] == 871 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[28]);
                    pathTiles.Add(tiles[58]);
                    pathTiles.Add(tiles[88]);
                    pathTiles.Add(tiles[118]);
                    pathTiles.Add(tiles[148]);
                    pathTiles.Add(tiles[178]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[209].transform.position) < 0.1f &&
                ((int)infos[0] == 870 || (int)infos[0] == 871 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[29]);
                    pathTiles.Add(tiles[59]);
                    pathTiles.Add(tiles[89]);
                    pathTiles.Add(tiles[119]);
                    pathTiles.Add(tiles[149]);
                    pathTiles.Add(tiles[179]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[481].transform.position) < 0.1f &&
                ((int)infos[0] == 536))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[537]);
                    pathTiles.Add(tiles[538]);
                    pathTiles.Add(tiles[539]);
                    pathTiles.Add(tiles[480]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if ((Vector3.Distance(selectedUnit.transform.position, tiles[751].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[752].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[753].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[781].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[782].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[783].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[811].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[812].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[813].transform.position) < 0.1f) &&
                ((int)infos[0] == 29 || (int)infos[0] == 59))
            {
                if (!isPath)
                {
                    isPath = true;

                    if ((int)infos[0] == 29)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[872]);
                        pathTiles.Add(tiles[843]);
                        pathTiles.Add(tiles[814]);
                        pathTiles.Add(tiles[785]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }

                    if ((int)infos[0] == 59)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[29]);
                        pathTiles.Add(tiles[872]);
                        pathTiles.Add(tiles[843]);
                        pathTiles.Add(tiles[814]);
                        pathTiles.Add(tiles[785]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                }
            }
            else if ((Vector3.Distance(selectedUnit.transform.position, tiles[754].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[755].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[784].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[785].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[814].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[815].transform.position) < 0.1f) &&
                ((int)infos[0] == 29 || (int)infos[0] == 59))
            {
                if (!isPath)
                {
                    isPath = true;

                    if ((int)infos[0] == 29)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[870]);
                        pathTiles.Add(tiles[841]);
                        pathTiles.Add(tiles[812]);
                        pathTiles.Add(tiles[782]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }

                    if ((int)infos[0] == 59)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[29]);
                        pathTiles.Add(tiles[870]);
                        pathTiles.Add(tiles[841]);
                        pathTiles.Add(tiles[812]);
                        pathTiles.Add(tiles[782]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                }
            }
            else if ((Vector3.Distance(selectedUnit.transform.position, tiles[841].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[842].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[843].transform.position) < 0.1f) &&
                ((int)infos[0] == 29 || (int)infos[0] == 59))
            {
                if (!isPath)
                {
                    isPath = true;

                    if ((int)infos[0] == 29)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[872]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                    else if ((int)infos[0] == 59)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[29]);
                        pathTiles.Add(tiles[872]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                }
            }
            else if ((Vector3.Distance(selectedUnit.transform.position, tiles[844].transform.position) < 0.1f ||
                     Vector3.Distance(selectedUnit.transform.position, tiles[845].transform.position) < 0.1f) &&
                ((int)infos[0] == 29 || (int)infos[0] == 59))
            {
                if (!isPath)
                {
                    isPath = true;

                    if ((int)infos[0] == 29)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[871]);
                        pathTiles.Add(tiles[872]);
                        pathTiles.Add(tiles[873]);
                        pathTiles.Add(tiles[874]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                    else if ((int)infos[0] == 59)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[29]);
                        pathTiles.Add(tiles[871]);
                        pathTiles.Add(tiles[872]);
                        pathTiles.Add(tiles[873]);
                        pathTiles.Add(tiles[874]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[840].transform.position) < 0.1f &&
                ((int)infos[0] == 30 || (int)infos[0] == 31 || (int)infos[0] == 59))
            {
                if (!isPath)
                {
                    isPath = true;

                    if ((int)infos[0] == 31)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[870]);
                        pathTiles.Add(tiles[30]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                    else
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[870]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[865].transform.position) < 0.1f &&
                ((int)infos[0] == 31 || (int)infos[0] == 1))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[0]);
                    pathTiles.Add(tiles[898]);
                    pathTiles.Add(tiles[897]);
                    pathTiles.Add(tiles[896]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[866].transform.position) < 0.1f &&
                ((int)infos[0] == 31))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[0]);
                    pathTiles.Add(tiles[899]);
                    pathTiles.Add(tiles[868]);
                    pathTiles.Add(tiles[867]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[777].transform.position) < 0.1f &&
                ((int)infos[0] == 31))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[0]);
                    pathTiles.Add(tiles[899]);
                    pathTiles.Add(tiles[868]);
                    pathTiles.Add(tiles[838]);
                    pathTiles.Add(tiles[808]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[807].transform.position) < 0.1f &&
                ((int)infos[0] == 31))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[0]);
                    pathTiles.Add(tiles[899]);
                    pathTiles.Add(tiles[868]);
                    pathTiles.Add(tiles[838]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[837].transform.position) < 0.1f &&
                ((int)infos[0] == 31))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[0]);
                    pathTiles.Add(tiles[899]);
                    pathTiles.Add(tiles[868]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[867].transform.position) < 0.1f &&
                ((int)infos[0] == 31))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[0]);
                    pathTiles.Add(tiles[899]);
                    pathTiles.Add(tiles[868]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[778].transform.position) < 0.1f &&
                ((int)infos[0] == 31))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[0]);
                    pathTiles.Add(tiles[899]);
                    pathTiles.Add(tiles[868]);
                    pathTiles.Add(tiles[838]);
                    pathTiles.Add(tiles[808]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[808].transform.position) < 0.1f &&
                ((int)infos[0] == 31))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[0]);
                    pathTiles.Add(tiles[899]);
                    pathTiles.Add(tiles[868]);
                    pathTiles.Add(tiles[838]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[838].transform.position) < 0.1f &&
                ((int)infos[0] == 31))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[0]);
                    pathTiles.Add(tiles[899]);
                    pathTiles.Add(tiles[868]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[868].transform.position) < 0.1f &&
                ((int)infos[0] == 31))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[0]);
                    pathTiles.Add(tiles[899]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[869].transform.position) < 0.1f &&
                ((int)infos[0] == 30 || (int)infos[0] == 31 || (int)infos[0] == 59))
            {
                if (!isPath)
                {
                    isPath = true;

                    if ((int)infos[0] == 31)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[30]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                    else
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[870].transform.position) < 0.1f &&
                ((int)infos[0] == 30 || (int)infos[0] == 31 || (int)infos[0] == 59 || (int)infos[0] == 88))
            {
                if (!isPath)
                {
                    isPath = true;

                    if ((int)infos[0] == 30 || (int)infos[0] == 31)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[0]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                    else if ((int)infos[0] == 59 || (int)infos[0] == 88)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[29]);
                        pathTiles.Add(tiles[0]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[871].transform.position) < 0.1f &&
                ((int)infos[0] == 59 || (int)infos[0] == 899))
            {
                if (!isPath)
                {
                    isPath = true;

                    if ((int)infos[0] == 59)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[29]);
                        pathTiles.Add(tiles[870]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                    else if ((int)infos[0] == 899)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[870]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[872].transform.position) < 0.1f &&
                ((int)infos[0] == 59))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[29]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[871]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[873].transform.position) < 0.1f &&
                ((int)infos[0] == 59))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[870]);
                    pathTiles.Add(tiles[871]);
                    pathTiles.Add(tiles[872]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else if ((Vector3.Distance(selectedUnit.transform.position, tiles[897].transform.position) < 0.1f ||
                      Vector3.Distance(selectedUnit.transform.position, tiles[898].transform.position) < 0.1f) &&
                ((int)infos[0] == 1) || ((int)infos[0] == 31) || ((int)infos[0] == 871))
            {
                if (!isPath)
                {
                    isPath = true;

                    if ((int)infos[0] == 871)
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[870]);
                        pathTiles.Add(tiles[899]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                    else
                    {
                        pathTiles.Add(tiles[(int)infos[0]]);
                        pathTiles.Add(tiles[0]);
                        pathTiles.Add(tiles[899]);
                        isMoveAnimation = true;
                        pathIndex = pathTiles.Count - 1;
                    }
                }
            }
            else if (Vector3.Distance(selectedUnit.transform.position, tiles[899].transform.position) < 0.1f &&
                ((int)infos[0] == 30 || (int)infos[0] == 31))
            {
                if (!isPath)
                {
                    isPath = true;
                    pathTiles.Add(tiles[(int)infos[0]]);
                    pathTiles.Add(tiles[0]);
                    isMoveAnimation = true;
                    pathIndex = pathTiles.Count - 1;
                }
            }
            else
            {
                if (isPath == false)
                {
                    isPath = true;
                    PathFind(selectedUnit);
                }
            }
            #endregion

            ShowGamePlayInfo(0);
        }
        else if (eventCode == 6)
        {
            object[] infos = (object[])photonEvent.CustomData;
            isFire = false;
            targetObject = tiles[(int)infos[0]];

            FireAction(targetObject);
        }
        else if (eventCode == 7)
        {
            object[] infos = (object[])photonEvent.CustomData;
            isFire = false;
            isOnlyOncePlay = false;

            if ((int)infos[1] == 0)
            {
                targetObject = force1ForCircle[(int)infos[0]];
            }
            else if ((int)infos[1] == 1)
            {
                targetObject = force1ForSquare[(int)infos[0]];
            }
            else if ((int)infos[1] == 2)
            {
                targetObject = force2ForCircle[(int)infos[0]];
            }
            else if ((int)infos[1] == 3)
            {
                targetObject = force2ForSquare[(int)infos[0]];
            }

            FireAction(targetObject);
        }
        else if (eventCode == 8)
        {
            object[] infos = (object[])photonEvent.CustomData;

            switch ((int)infos[1])
            {
                case 0:
                    {
                        switch (selectedUnit.GetComponent<ForceInfo>().forceType)
                        {
                            case (int)ForceType.General:
                                {
                                    dice40.GetComponent<Animator>().enabled = false;
                                    dice40.GetComponent<Image>().sprite = dice4Sprites[(int)infos[0]];
                                    break;
                                }
                            case (int)ForceType.Plane:
                                {
                                    dice60.GetComponent<Animator>().enabled = false;
                                    dice60.GetComponent<Image>().sprite = dice6Sprites[(int)infos[0]];
                                    break;
                                }
                            case (int)ForceType.Sub:
                                {
                                    dice40.GetComponent<Animator>().enabled = false;
                                    dice40.GetComponent<Image>().sprite = dice4Sprites[(int)infos[0]];
                                    break;
                                }
                            case (int)ForceType.Tank:
                                {
                                    dice40.GetComponent<Animator>().enabled = false;
                                    dice40.GetComponent<Image>().sprite = dice4Sprites[(int)infos[0]];
                                    break;
                                }
                        }

                        moveRange = (int)infos[0];
                        ShowMoveRange((int)infos[0]);
                    }
                    break;
                case 1:
                    {
                        switch (selectedUnit.GetComponent<ForceInfo>().forceType)
                        {
                            case (int)ForceType.General:
                                {
                                    dice40.GetComponent<Animator>().enabled = false;
                                    dice40.GetComponent<Image>().sprite = dice4Sprites[(int)infos[0]];
                                    break;
                                }
                            case (int)ForceType.Plane:
                                {
                                    dice40.GetComponent<Animator>().enabled = false;
                                    dice40.GetComponent<Image>().sprite = dice4Sprites[(int)infos[0]];
                                    break;
                                }
                            case (int)ForceType.Sub:
                                {
                                    dice40.GetComponent<Animator>().enabled = false;
                                    dice40.GetComponent<Image>().sprite = dice4Sprites[(int)infos[0]];
                                    break;
                                }
                            case (int)ForceType.Tank:
                                {
                                    dice60.GetComponent<Animator>().enabled = false;
                                    dice60.GetComponent<Image>().sprite = dice6Sprites[(int)infos[0]];
                                    break;
                                }
                        }
                        fireRange = (int)infos[0];
                        ShowFireRange((int)infos[0]);
                    }
                    break;
                case 2:
                    {
                        switch (selectedUnit.GetComponent<ForceInfo>().forceType)
                        {
                            case (int)ForceType.General:
                                {
                                    dice40.GetComponent<Animator>().enabled = false;
                                    dice40.GetComponent<Image>().sprite = dice4Sprites[(int)infos[0]];
                                    break;
                                }
                            case (int)ForceType.Plane:
                                {
                                    dice40.GetComponent<Animator>().enabled = false;
                                    dice40.GetComponent<Image>().sprite = dice4Sprites[(int)infos[0]];
                                    break;
                                }
                            case (int)ForceType.Sub:
                                {
                                    dice40.GetComponent<Animator>().enabled = false;
                                    dice40.GetComponent<Image>().sprite = dice4Sprites[(int)infos[0]];
                                    break;
                                }
                            case (int)ForceType.Tank:
                                {
                                    dice60.GetComponent<Animator>().enabled = false;
                                    dice60.GetComponent<Image>().sprite = dice6Sprites[(int)infos[0]];
                                    break;
                                }
                        }
                        subRange = (int)infos[0];
                        ShowSubRange((int)infos[0]);
                    }
                    break;
                case 3:
                    {
                        dice40.GetComponent<Animator>().enabled = false;
                        dice41.GetComponent<Animator>().enabled = false;

                        int index0 = (int)infos[0];
                        int index1 = (int)infos[2];
                        //print($"{index0}, {index1}");
                        dice40.GetComponent<Image>().sprite = dice4Sprites[index0 - 1];
                        dice41.GetComponent<Image>().sprite = dice4Sprites[index1 - 1];

                        isSelected = true;
                        sonarRange = index0 + index1 - 1;
                        ShowSonarRange(sonarRange);
                    }
                    break;
            }

            if ((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame))
            {
                endPlayButton.SetActive(true);

                if ((int)infos[1] == 2)
                {
                    subFireButton.SetActive(true);

                }
            }

            if (whoseTurn != currentTurnIndex)
            {
                endPlayButton.SetActive(false);
                subFireButton.SetActive(false);

            }
        }
        else if (eventCode == 9)
        {
            isSonar = true;
            isSelected = true;
            currentTurnIndex = whoseTurn;
            StartCoroutine(DiceAnimationForSonar());
        }
        else if (eventCode == 10)
        {
            isSub = true;
            isSelected = true;
            currentTurnIndex = whoseTurn;
            StartCoroutine(DiceAnimationForSub(selectedUnit.GetComponent<ForceInfo>().fireRange));
        }
        else if (eventCode == 11)
        {
            bool isHit = false;
            isSub = false;

            // Check hit or missed.

            if ((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame))
            {
                for (int j = 0; j < tiles.Length; j++)
                {
                    if (tiles[j].GetComponent<SpriteRenderer>().enabled == true)
                    {
                        GameObject cell = tiles[j];

                        for (int i = 0; i < force1ForCircle.Length; i++)
                        {
                            if (whoseTurn == (int)TurnIndex.Player1)
                            {
                                if (Vector3.Distance(cell.transform.position, force2ForCircle[i].transform.position) < 0.1f && force2ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && cell.GetComponent<CellInfo>().cellType != (int)CellType.Tunnel)
                                {
                                    isHit = true;
                                }

                                if (Vector3.Distance(cell.transform.position, force2ForCircle[i].transform.position) < 0.1f && force2ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && ConvertToTile(selectedUnit).GetComponent<CellInfo>().cellType == (int)CellType.TunnelEntrance)
                                {
                                    isHit = true;
                                }

                                if (Vector3.Distance(cell.transform.position, force2ForSquare[i].transform.position) < 0.1f && force2ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && cell.GetComponent<CellInfo>().cellType != (int)CellType.Tunnel)
                                {
                                    isHit = true;
                                }

                                if (Vector3.Distance(cell.transform.position, force2ForSquare[i].transform.position) < 0.1f && force2ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && ConvertToTile(selectedUnit).GetComponent<CellInfo>().cellType == (int)CellType.TunnelEntrance)
                                {
                                    isHit = true;
                                }
                            }
                            else if (whoseTurn == (int)TurnIndex.Player2)
                            {
                                if (Vector3.Distance(cell.transform.position, force1ForCircle[i].transform.position) < 0.1f && force1ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && cell.GetComponent<CellInfo>().cellType != (int)CellType.Tunnel)
                                {
                                    isHit = true;
                                }

                                if (Vector3.Distance(cell.transform.position, force1ForCircle[i].transform.position) < 0.1f && force1ForCircle[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && ConvertToTile(selectedUnit).GetComponent<CellInfo>().cellType == (int)CellType.TunnelEntrance)
                                {
                                    isHit = true;
                                }

                                if (Vector3.Distance(cell.transform.position, force1ForSquare[i].transform.position) < 0.1f && force1ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && cell.GetComponent<CellInfo>().cellType != (int)CellType.Tunnel)
                                {
                                    isHit = true;
                                }

                                if (Vector3.Distance(cell.transform.position, force1ForSquare[i].transform.position) < 0.1f && force1ForSquare[i].GetComponent<ForceInfo>().forceType == (int)ForceType.Sub && ConvertToTile(selectedUnit).GetComponent<CellInfo>().cellType == (int)CellType.TunnelEntrance)
                                {
                                    isHit = true;
                                }
                            }
                        }
                    }
                }

                SyncHitOrMiss(new object[] { isHit });
            }

            if ((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame))
            {
                for (int i = 0; i < tiles.Length; i++)
                {
                    if (tiles[i].GetComponent<SpriteRenderer>().enabled == true)
                    {
                        SubAction(tiles[i]);
                    }
                }
            }

            EndPlayAction();
            SetAudio(1);
            selectedUnit.GetComponent<AudioSource>().loop = false;
            selectedUnit.GetComponent<AudioSource>().Play();
        }
        else if (eventCode == 12)
        {
            StartCoroutine(DelayToShowEndplay());
            EndPlayAction();
        }
        else if (eventCode == 13)
        {
            isPing = true;
            PingAction(sonarRange);
            ShowGamePlayInfo(3);
        }
        else if (eventCode == 14)
        {
            object[] infos = (object[])photonEvent.CustomData;
            StartCoroutine(DelayToSubHit((int)infos[0], (int)infos[1]));
        }
        else if (eventCode == 15)
        {
            currentTurnIndex = whoseTurn;

            ufo.SetActive(true);
            ufo.GetComponent<Animator>().enabled = true;
            selectedUnit = ufo;
            SetAudio(0);
            selectedUnit.GetComponent<AudioSource>().loop = true;
            selectedUnit.GetComponent<AudioSource>().Play();
            ShowUFORange();
        }
        else if (eventCode == 16)
        {
            object[] infos = (object[])photonEvent.CustomData;
            targetObject = tiles[(int)infos[0]];
            isUFO = false;
            isUFOMove = true;
            currentTurnIndex = whoseTurn;
            //cameraView.transform.position = new Vector3(ufoTarget.x, ufoTarget.y, cameraView.transform.position.z);
        }
        else if (eventCode == 17)
        {
            object[] infos = (object[])photonEvent.CustomData;

            if ((int)infos[1] == 0)
            {
                UFOAction(force1ForCircle[(int)infos[0]]);
            }
            else if ((int)infos[1] == 1)
            {
                UFOAction(force1ForSquare[(int)infos[0]]);
            }
            else if ((int)infos[1] == 2)
            {
                UFOAction(force2ForCircle[(int)infos[0]]);
            }
            else if ((int)infos[1] == 3)
            {
                UFOAction(force2ForSquare[(int)infos[0]]);
            }

            ufoActionIndex++;

            if (ufoDiceIndex1 > 0 && ufoActionIndex == 1)
            {
                EndPlayAction();
                isUFOAction = false;
                ufoActionIndex = 0;
            }
            else if (ufoDiceIndex1 == 0)
            {
                if (ufoActionIndex == 2)
                {
                    EndPlayAction();
                    isUFOAction = false;
                    ufoActionIndex = 0;
                }
                else if (ufoActionIndex == 1 && !ShowUFOFireRange())
                {
                    EndPlayAction();
                    isUFOAction = false;
                    ufoActionIndex = 0;
                }
            }
        }
        else if (eventCode == 18)
        {
            object[] infos = (object[])photonEvent.CustomData;

            StartCoroutine(DiceAnimationForUFO((int)infos[0], (int)infos[1]));
        }
        else if (eventCode == 19)
        {
            object[] infos = (object[])photonEvent.CustomData;

            if ((int)infos[1] == 0)
            {
                selectedUnit = force1ForCircle[(int)infos[0]];
            }
            else if ((int)infos[1] == 1)
            {
                selectedUnit = force1ForSquare[(int)infos[0]];
            }
            else if ((int)infos[1] == 2)
            {
                selectedUnit = force2ForCircle[(int)infos[0]];
            }
            else if ((int)infos[1] == 3)
            {
                selectedUnit = force2ForSquare[(int)infos[0]];
            }

            for (int i = 0; i < tiles.Length; i++)
            {
                if (Vector3.Distance(selectedUnit.transform.position, tiles[i].transform.position) < 0.1f)
                {
                    selectedX = i / 30;
                    selectedY = i % 30;
                }
            }

            for (int i = 0; i < force1ForCircle.Length; i++)
            {
                force1ForCircle[i].GetComponent<AudioSource>().Stop();
                force1ForSquare[i].GetComponent<AudioSource>().Stop();
                force2ForCircle[i].GetComponent<AudioSource>().Stop();
                force2ForSquare[i].GetComponent<AudioSource>().Stop();
            }

            SetAudio(0);
            selectedUnit.GetComponent<AudioSource>().loop = true;
            selectedUnit.GetComponent<AudioSource>().Play();

            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].GetComponent<SpriteRenderer>().enabled = false;
            }

            if (isMove)
            {
                ShowMoveRange(moveRange);
            }
            else if (isFire)
            {
                ShowFireRange(fireRange);
            }
            else if (isSub)
            {
                ShowSubRange(subRange);
            }
            else if (isSonar)
            {
                ShowSonarRange(sonarRange);
            }

            if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
            {
            }
            else
            {
                if (selectedUnit.GetComponent<ForceInfo>().forceType != (int)ForceType.Sub)
                {
                    if (isSmartMapEnabled)
                        cameraView.transform.position = new Vector3(selectedUnit.transform.position.x, selectedUnit.transform.position.y, cameraView.transform.position.z);
                }
            }
        }
        else if (eventCode == 20)
        {
            object[] infos = (object[])photonEvent.CustomData;

            SetSkins((int)infos[0], (int)infos[1], (int)infos[2], (int)infos[3]);
        }
        else if (eventCode == 21)
        {
            object[] infos = (object[])photonEvent.CustomData;

            placedTilesIndex.Add((int)infos[0]);

            if (placedTilesIndex.Count == 32)
            {
                for (int i = 0; i < force1ForCircle.Length; i++)
                {
                    force1ForCircle[i].transform.position = tiles[placedTilesIndex[i]].transform.position;
                    force1ForSquare[i].transform.position = tiles[placedTilesIndex[8 + i]].transform.position;
                    force2ForSquare[i].transform.position = tiles[placedTilesIndex[16 + i]].transform.position;
                    force2ForCircle[i].transform.position = tiles[placedTilesIndex[24 + i]].transform.position;
                }
            }
        }
        else if (eventCode == 22)
        {
            object[] infos = (object[])photonEvent.CustomData;
            StartCoroutine(FirstRollAnimation((int)infos[0], (int)infos[1], (int)infos[2], (int)infos[3]));
        }
        else if (eventCode == 23)
        {
            object[] infos = (object[])photonEvent.CustomData;
            int winCount = PlayerPrefs.GetInt("WIN_COUNT");
            int loseCount = PlayerPrefs.GetInt("LOSE_COUNT");

            if ((int)infos[0] == 0)
            {
                if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
                {
                    if (isClickExit)
                    {
                        whoseTurn = (whoseTurn + 1) % 2;
                    }
                }
                else
                {
                    if (!isClickExit)
                    {
                        whoseTurn = (whoseTurn + 1) % 2;
                    }
                }

                if (isGamePlay)
                {
                    if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
                    {
                        winCount++;
                        PlayerPrefs.SetInt("WIN_COUNT", winCount);

                        if (MainUI.isListedRound)
                        {
                            PlayerPrefs.SetInt("ROUND", PlayerPrefs.GetInt("ROUND") + 1);
                        }

                        StartCoroutine(PostUserInfos(MainUI.frontURL + "/api/savePlaycount"));
                        winningDialog.SetActive(true);

                    }
                    else
                    {
                        TournamentLobbyCreator.SetIsinTournament(false);
                        loseCount++;
                        PlayerPrefs.SetInt("LOSE_COUNT", loseCount);

                        StartCoroutine(PostUserInfos(MainUI.frontURL + "/api/savePlaycount"));
                        loseDialog.SetActive(true);
                    }
                }

                isGamePlay = false;
            }
            else
            {
                if (isGamePlay)
                {
                    if (((whoseTurn == (int)TurnIndex.Player1 && isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && !isMasterInGame)))
                    {
                        winCount++;
                        PlayerPrefs.SetInt("WIN_COUNT", winCount);

                        if (MainUI.isListedRound)
                        {
                            PlayerPrefs.SetInt("ROUND", PlayerPrefs.GetInt("ROUND") + 1);
                        }

                        StartCoroutine(PostUserInfos(MainUI.frontURL + "/api/savePlaycount"));
                        winningDialog.SetActive(true);
                    }
                    else
                    {
                        TournamentLobbyCreator.SetIsinTournament(false);
                        loseCount++;
                        PlayerPrefs.SetInt("LOSE_COUNT", loseCount);

                        StartCoroutine(PostUserInfos(MainUI.frontURL + "/api/savePlaycount"));
                        loseDialog.SetActive(true);
                    }
                }

                isGamePlay = false;
            }

            StartCoroutine(DelayToEndGame());
        }
        else if (eventCode == 24)
        {
            object[] infos = (object[])photonEvent.CustomData;

            switch ((int)infos[0])
            {
                case 0:
                    {
                        map.transform.position = new Vector3(0, 0, 0);
                        map.transform.rotation = Quaternion.Euler(0, 0, 0);
                        break;
                    }
                case 1:
                    {
                        map.transform.position = new Vector3(8.942206f, -12.33977f, 0);
                        map.transform.rotation = Quaternion.Euler(0, 0, 90);
                        break;
                    }
                case 2:
                    {
                        map.transform.position = new Vector3(21.28913f, -3.354249f, 0);
                        map.transform.rotation = Quaternion.Euler(0, 0, 180);
                        break;
                    }
                case 3:
                    {
                        map.transform.position = new Vector3(12.35923f, 8.984041f, 0);
                        map.transform.rotation = Quaternion.Euler(0, 0, 270);
                        break;
                    }
            }

            if (isMasterInGame)
            {
                generalFaces[0].GetComponent<Image>().sprite = force1ForCircle[0].GetComponent<SpriteRenderer>().sprite;
                generalFaces[1].GetComponent<Image>().sprite = force1ForCircle[1].GetComponent<SpriteRenderer>().sprite;
                generalFaces[2].GetComponent<Image>().sprite = force1ForSquare[0].GetComponent<SpriteRenderer>().sprite;
                generalFaces[3].GetComponent<Image>().sprite = force1ForSquare[1].GetComponent<SpriteRenderer>().sprite;
            }
            else
            {
                generalFaces[0].GetComponent<Image>().sprite = force2ForCircle[0].GetComponent<SpriteRenderer>().sprite;
                generalFaces[1].GetComponent<Image>().sprite = force2ForCircle[1].GetComponent<SpriteRenderer>().sprite;
                generalFaces[2].GetComponent<Image>().sprite = force2ForSquare[0].GetComponent<SpriteRenderer>().sprite;
                generalFaces[3].GetComponent<Image>().sprite = force2ForSquare[1].GetComponent<SpriteRenderer>().sprite;
            }

        }
        else if (eventCode == 25)
        {
            object[] infos = (object[])photonEvent.CustomData;

            switch ((int)infos[0])
            {
                case 0:
                    {
                        for (int j = 0; j < force1ForCircle.Length; j++)
                        {
                            if (force1ForCircle[j].GetComponent<ForceInfo>().forceType != (int)ForceType.Sub)
                            {
                                fireParticle = Instantiate(firePrefab, force1ForCircle[j].transform.position, Quaternion.identity);
                                fireParticle.Play();
                                force1ForCircle[j].transform.position = new Vector3(10000f, 10000f, -8f);
                                force1ForCircle[j].SetActive(false);
                            }
                        }

                        break;
                    }
                case 1:
                    {
                        for (int j = 0; j < force1ForSquare.Length; j++)
                        {
                            if (force1ForSquare[j].GetComponent<ForceInfo>().forceType != (int)ForceType.Sub)
                            {
                                fireParticle = Instantiate(firePrefab, force1ForSquare[j].transform.position, Quaternion.identity);
                                fireParticle.Play();
                                force1ForSquare[j].transform.position = new Vector3(10000f, 10000f, -8f);
                                force1ForSquare[j].SetActive(false);
                            }
                        }

                        break;
                    }
                case 2:
                    {
                        for (int j = 0; j < force2ForCircle.Length; j++)
                        {
                            if (force2ForCircle[j].GetComponent<ForceInfo>().forceType != (int)ForceType.Sub)
                            {
                                fireParticle = Instantiate(firePrefab, force2ForCircle[j].transform.position, Quaternion.identity);
                                fireParticle.Play();
                                force2ForCircle[j].transform.position = new Vector3(10000f, 10000f, -8f);
                                force2ForCircle[j].SetActive(false);
                            }
                        }

                        break;
                    }
                case 3:
                    {
                        for (int j = 0; j < force2ForSquare.Length; j++)
                        {
                            if (force2ForSquare[j].GetComponent<ForceInfo>().forceType != (int)ForceType.Sub)
                            {
                                fireParticle = Instantiate(firePrefab, force2ForSquare[j].transform.position, Quaternion.identity);
                                fireParticle.Play();
                                force2ForSquare[j].transform.position = new Vector3(10000f, 10000f, -8f);
                                force2ForSquare[j].SetActive(false);
                            }
                        }

                        break;
                    }
            }
        }
        else if (eventCode == 26)
        {
            object[] infos = (object[])photonEvent.CustomData;
            bool isHit = (bool)infos[0];

            ShowGamePlayInfo(2);

            if (selectedUnit.GetComponent<ForceInfo>().forceType == (int)ForceType.Sub)
            {
                infoGamePlayPanel.GetComponentInChildren<Text>().color = Color.red;

                if (((whoseTurn == (int)TurnIndex.Player1 && !isMasterInGame) || (whoseTurn == (int)TurnIndex.Player2 && isMasterInGame)))
                {
                    if (isHit)
                    {
                        infoGamePlayPanel.GetComponentInChildren<Text>().text = "Sub Fired at Sub and Hit!";
                    }
                    else
                    {
                        infoGamePlayPanel.GetComponentInChildren<Text>().text = "Sub Fired at Sub and missed!";
                    }
                }
                else
                {
                    if (isHit)
                    {
                        infoGamePlayPanel.GetComponentInChildren<Text>().text = "Sub Fired at Sub and Hit!";
                    }
                    else
                    {
                        infoGamePlayPanel.GetComponentInChildren<Text>().text = "Sub Fired at Sub and missed!";
                    }
                }
            }
        }
        else if (eventCode == 27)
        {
            object[] infos = (object[])photonEvent.CustomData;

            bermudaTriangles[(int)infos[0]].transform.position = (Vector3)infos[1];
        }
        else if (eventCode == 28)
        {
            object[] infos = (object[])photonEvent.CustomData;
            isActivatedBermuda = true;

            for (int i = 0; i < 4; i++)
            {
                bermudaInfos[i] = (int)infos[i];
            }
        }
        else if (eventCode == 29)
        {
            object[] infos = (object[])photonEvent.CustomData;

            atomicMines[(int)infos[0]].transform.position = (Vector3)infos[1];
        }
        else if (eventCode == 30)
        {
            object[] infos = (object[])photonEvent.CustomData;

            isExplodedMines = true;
            expandExplosionIndex = 0;
            chainExplosionIndex = 0;
            minesInfos[0] = (int)infos[0];
            minesInfos[1] = (int)infos[1];
            minesInfos[2] = (int)infos[2];
        }
        else if (eventCode == 31)
        {
            for (int i = 0; i < atomicMines.Length; i++)
            {
                atomicMines[i].GetComponent<BoxCollider>().enabled = true;
            }

            infoBlackDragon.SetActive(true);
            infoBombs.SetActive(true);
        }
        else if (eventCode == 32)
        {
            object[] infos = (object[])photonEvent.CustomData;

            if ((int)infos[0] + loadedBombs <= 3)
            {
                if ((int)infos[0] == 1)
                {
                    loadedBombs += (int)infos[0];
                }
                else if ((int)infos[0] == 2)
                {
                    loadedBombs += (int)infos[0];
                }
                else if ((int)infos[0] == 3)
                {
                    loadedBombs += (int)infos[0];
                }

                for (int i = 0; i < loadedBombs; i++)
                {
                    infoBombs.transform.GetChild(i).gameObject.SetActive(true);
                }

                for (int i = 0; i < force1ForCircle.Length; i++)
                {
                    if (Vector3.Distance(force1ForCircle[i].transform.position, tiles[(int)infos[1]].transform.position) < 0.1f)
                    {
                        force1ForCircle[i].transform.position = new Vector3(10000f, 10000f, -8f);
                    }
                    else if (Vector3.Distance(force1ForSquare[i].transform.position, tiles[(int)infos[1]].transform.position) < 0.1f)
                    {
                        force1ForSquare[i].transform.position = new Vector3(10000f, 10000f, -8f);
                    }
                    else if (Vector3.Distance(force2ForCircle[i].transform.position, tiles[(int)infos[1]].transform.position) < 0.1f)
                    {
                        force2ForCircle[i].transform.position = new Vector3(10000f, 10000f, -8f);
                    }
                    else if (Vector3.Distance(force2ForSquare[i].transform.position, tiles[(int)infos[1]].transform.position) < 0.1f)
                    {
                        force2ForSquare[i].transform.position = new Vector3(10000f, 10000f, -8f);
                    }
                }

                for (int i = 0; i < atomicMines.Length; i++)
                {
                    if (Vector3.Distance(atomicMines[i].transform.position, tiles[(int)infos[1]].transform.position) < 0.1f)
                    {
                        atomicMines[i].transform.position = new Vector3(20000f, 20000f, 0);
                    }
                }
            }
            else
            {
                print("Can't load anymore ...");
            }
        }else if(eventCode == 33) // sync posistions and other data 
        {
            object[] infos = (object[])photonEvent.CustomData;

           
            for (int i = 0; i < playerslist.Count; i++)
            {
                if (infos[i] != null && playerslist[i].transform!=null)
                playerslist[i].transform.position = (Vector3)infos[i];

            }
 



        }
    }
    #endregion

    IEnumerator PostUserInfos(string url)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", PlayerPrefs.GetInt("USER_ID"));
        form.AddField("wincount", PlayerPrefs.GetInt("WIN_COUNT"));
        form.AddField("losecount", PlayerPrefs.GetInt("LOSE_COUNT"));
        form.AddField("round", PlayerPrefs.GetInt("ROUND"));

        UnityWebRequest uwr = UnityWebRequest.Post(url, form);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Request: Received: " + uwr.downloadHandler.text);
        }
    }

    IEnumerator PostPlayingRoom(string url)
    {
        WWWForm form = new WWWForm();
        form.AddField("creator", PlayerPrefs.GetString("CREATOR"));

        UnityWebRequest uwr = UnityWebRequest.Post(url, form);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Request: Received: " + uwr.downloadHandler.text);
        }
    }

    IEnumerator PostEndedRoom(string url)
    {
        WWWForm form = new WWWForm();
        form.AddField("creator", PlayerPrefs.GetString("CREATOR"));

        UnityWebRequest uwr = UnityWebRequest.Post(url, form);
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Request: Received: " + uwr.downloadHandler.text);
        }
    }

    IEnumerator DelayToShowMeleeRound()
    {
        meleeRoundStartDialog.SetActive(true);
        yield return new WaitForSeconds(5f);
        meleeRoundStartDialog.SetActive(false);
    }

    public Text smartMapText;
    public Color smartMapEnabledColor, smartMapDisabledColor;

    public void ToggleSmartMap()
    {
        isSmartMapEnabled = !isSmartMapEnabled;

        smartMapText.color = isSmartMapEnabled ? smartMapEnabledColor : smartMapDisabledColor;
    }
    
    public Text unitGlowText;
    public Color outlineEnabledColor, outlineDisabledColor;

    public void UnitGlowBtn()
    {
        isOutlined = !isOutlined;
        unitGlowText.color = isOutlined ? outlineEnabledColor : outlineDisabledColor;

        if (isOutlined)
        {
            if (isMasterInGame)
            {
                for (int i = 0; i < 8; i++)
                {
                    force1ForCircle[i].GetComponent<SpriteRenderer>().material = outlineMat;
                    force1ForSquare[i].GetComponent<SpriteRenderer>().material = outlineMat;
                }
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    force2ForCircle[i].GetComponent<SpriteRenderer>().material = outlineMat;
                    force2ForSquare[i].GetComponent<SpriteRenderer>().material = outlineMat;
                }
            }
        }
        else
        {
            if (isMasterInGame)
            {
                for (int i = 0; i < 8; i++)
                {
                    force1ForCircle[i].GetComponent<SpriteRenderer>().material = defaultMat;
                    force1ForSquare[i].GetComponent<SpriteRenderer>().material = defaultMat;
                }
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    force2ForCircle[i].GetComponent<SpriteRenderer>().material = defaultMat;
                    force2ForSquare[i].GetComponent<SpriteRenderer>().material = defaultMat;
                }
            }
        }
    }
}
