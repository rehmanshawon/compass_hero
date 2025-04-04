using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TournamentLobbyGlobalData : MonoBehaviour
{
    public static TournamentLobbyGlobalData instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        SetIsinTournamanet(false);
    }
    private void OnApplicationQuit()
    {
        SetIsinTournamanet(false);
    }
    public static void SetIsinTournamanet(bool val)
    {
        // Debug.LogError("setting intournament : " + val);
        PlayerPrefs.SetInt(Prefs.IsTournamanet, val ? 1 : 0);

        if (val == false)
        {
            PlayerPrefs.SetInt(Prefs.TournamentRoundCount, 0);

        }
        PlayerPrefs.Save();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
