using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine.UI;
using System;

[System.Serializable]
public class Setting
{

    public int id { get; set; }
    public int userId { get; set; }
    public int duration { get; set; }
    public string created_at { get; set; }
    public string updated_at { get; set; }
    public int map_shot { get; set; }
    public int sub_toggle { get; set; }
    public string wave_option { get; set; }
}
[System.Serializable]

public class Root
{
    public bool success { get; set; }
    public Setting setting { get; set; }
}

public class GameSettingsApplier : MonoBehaviourPun
{
    public Image img_fill;
   // public Animator bootingObjectAnim;
    public float durationInSeconds;
    public float animationSpeed;
    public string waveType;
    public GameObject noWaves;
    public GameObject crackedWaves;
    public GameObject simpleWaves;
 
    public bool timerActive = false;
    float t = 0;
    public IEnumerator init()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "Multi")
        {
 

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("Send Request To Get Duration Called");
                Debug.LogError("WEBGL");
                if (PlayerPrefs.HasKey("USER_ID"))
                {
                    Debug.Log("User ID Found");
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
                        Debug.Log($"Duration Found {durationInSeconds}");
                        string waves = parsedJson.setting.wave_option;
                        SetWaves(waves);



                    }

                }
                else
                {
                    Debug.LogError("Turn: User Not ID Found");
                    durationInSeconds = 120f;

                }


                yield return null;
               


            ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable
            {
                 { "AnimationSpeed", durationInSeconds },
                    {"WaveType", waveType }
            };
                PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
             

                Debug.Log("Animation Speed Set" + animationSpeed);
            }
           else
            {

                object value;
                if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("AnimationDuration", out value))
                {

                    durationInSeconds = (float)value;

                }
            
                Debug.LogError("Send Request To Get Duration Called");
                Debug.LogError("WEBGL");
                if (PlayerPrefs.HasKey("USER_ID"))
                {
                    Debug.Log("User ID Found");
                    WWWForm form = new WWWForm();
                    form.AddField("user_id", PlayerPrefs.GetInt("USER_ID"));
                    UnityWebRequest request = UnityWebRequest.Post("https://subgrids.com/api/getUserSetting", form);
                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.ConnectionError)
                    {
                        Debug.LogError("Turn: Error While Sending: " + request.error);
              

                    }
                    else
                    {
                        Debug.LogError("Turn: Received: " + request.downloadHandler.text);




                        Root parsedJson = JsonConvert.DeserializeObject<Root>(request.downloadHandler.text);
                         string waves = parsedJson.setting.wave_option;
                        SetWaves(waves);



                    }

                }
                else
                {
 

                }
        

 
            }
            t = 0;
        }
        else if (sceneName == "Single")
        {
       

            if(InteractiveTutorial.Instance==null || InteractiveTutorial.TutorialActive==false)
            {
                StartCoroutine(setCustomDataforAI());

 
            }
            else
            {
                durationInSeconds = 120f;
 
                SetWaves(waveType);
            }
            t = 0;
        }
    }
    private IEnumerator setCustomDataforAI()
    {
   
    
      
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

        t = 0;
        SetWaves(waveType);
    }
    private void Awake()
    {

        img_fill = GetComponent<Image>();
      StartCoroutine(init());
    }

    public void setTimer(float duration)
    {
        durationInSeconds = duration;
        timerActive = true;
    }
    public void resetTimer()
    {
        t = 0;
        timerActive = false;
    }
   public void setTimerActive(bool check)
    {
        timerActive = check;

    } 
    private void Update()
    {
       // if(timerActive)
        {


            if(t>=durationInSeconds) // turncomplete
            {

          //      timerActive = false;

            }
            else
            {
                t += Time.deltaTime;

            }

            t = Mathf.Clamp(t, 0, durationInSeconds);
            img_fill.fillAmount =1-( t / durationInSeconds);

        }
    }
    public float getCurrentval()
    {
        return t;
    }
    public void updatetime(float val)
    {
        t = val;
    } 
    //public void SetAnimationTimerSpeed(float animSpeed)
    //{
    //    bootingObjectAnim.speed = animSpeed;
    //}
    //private float GetTimerAnimationSpeed()
    //{
    //    return (120f / GetTurnTimeInSeconds());
    //}

    private float GetTurnTimeInSeconds()
    {
        Debug.Log("Get Time in seconds called");
       
        if(durationInSeconds<120)
        {
            durationInSeconds = 120;
        }
        //Get data from the api 
        return durationInSeconds;
    }

    private IEnumerator SendRequestToGetDuration()
    {


#if UNITY_EDITOR

        Debug.Log("EDITOR");


        string json = @"{
            ""success"": true,
            ""setting"": {
                ""id"": 6,
                ""userId"": 51,
                ""duration"": 3,
                ""created_at"": ""2024-12-16T07:19:18.000000Z"",
                ""updated_at"": ""2024-12-16T10:04:06.000000Z"",
                ""map_shot"": 1,
                ""sub_toggle"": 1,
                ""wave_option"": ""none""
            }
        }";

        Root parsedJson = JsonConvert.DeserializeObject<Root>(json);
        durationInSeconds = parsedJson.setting.duration * 60f;
        SetWaves(parsedJson.setting.wave_option);
        Debug.Log($"Duration Found {durationInSeconds}");

#else
        Debug.Log("Send Request To Get Duration Called");
        Debug.Log("WEBGL");
        if (PlayerPrefs.HasKey("USER_ID"))
        {
            Debug.Log("User ID Found");
            WWWForm form = new WWWForm();
            form.AddField("user_id", PlayerPrefs.GetInt("USER_ID"));
            UnityWebRequest request = UnityWebRequest.Post("https://subgrids.com/api/getUserSetting", form);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Turn: Error While Sending: " + request.error);
                durationInSeconds = 120f;

            }
            else
            {
                Debug.Log("Turn: Received: " + request.downloadHandler.text);


              

                Root parsedJson = JsonConvert.DeserializeObject<Root>(request.downloadHandler.text);
                durationInSeconds = parsedJson.setting.duration * 60f;
                Debug.Log($"Duration Found {durationInSeconds}");
                 string waves = parsedJson.setting.wave_option;
                 SetWaves(waves);



            }

        }
        else
        {
            Debug.Log("Turn: User Not ID Found");
            durationInSeconds = 120f;

        }
#endif

        yield return null;
    }

    private void SetWaves(string waves)
    {
        Debug.LogError("Waves set" + waves);
        switch (waves)
        {
            case "none":
                crackedWaves.SetActive(false);
                simpleWaves.SetActive(false);
                noWaves.SetActive(true);

                break;
            case "crack":
                crackedWaves.SetActive(true);
                simpleWaves.SetActive(false);
                noWaves.SetActive(false);
                break;
            case "water":
                crackedWaves.SetActive(false);
                simpleWaves.SetActive(true);
                noWaves.SetActive(false);
                break;
            default:
                crackedWaves.SetActive(false);
                simpleWaves.SetActive(false);
                noWaves.SetActive(true);
                break;
        }
        waveType = waves;

    }
}
