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
        bool devMode = false;
        try { devMode = DevAutoLogin.DevMode; } catch { }

        if (sceneName == "Multi")
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (devMode)
                {
                    durationInSeconds = durationInSeconds > 0 ? durationInSeconds : 120f;
                    SetWaves(string.IsNullOrEmpty(waveType) ? "none" : waveType);
                }
                else
                {
                    if (PlayerPrefs.HasKey("USER_ID"))
                    {
                        WWWForm form = new WWWForm();
                        form.AddField("user_id", PlayerPrefs.GetInt("USER_ID"));
                        UnityWebRequest request = UnityWebRequest.Post("https://compasshero.com/api/getUserSetting", form);
                        yield return request.SendWebRequest();

                        if (request.result == UnityWebRequest.Result.ConnectionError)
                        {
                            durationInSeconds = 120f;
                            SetWaves(string.IsNullOrEmpty(waveType) ? "none" : waveType);
                        }
                        else
                        {
                            try
                            {
                                Root parsedJson = JsonConvert.DeserializeObject<Root>(request.downloadHandler.text);
                                if (parsedJson != null && parsedJson.success && parsedJson.setting != null)
                                {
                                    durationInSeconds = parsedJson.setting.duration * 60f;
                                    string waves = parsedJson.setting.wave_option;
                                    SetWaves(waves);
                                }
                                else
                                {
                                    durationInSeconds = 120f;
                                    SetWaves("none");
                                }
                            }
                            catch
                            {
                                durationInSeconds = 120f;
                                SetWaves("none");
                            }
                        }
                    }
                    else
                    {
                        durationInSeconds = 120f;
                        SetWaves("none");
                    }
                }

                ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable
                {
                    { "AnimationDuration", durationInSeconds },
                    { "WaveType", waveType }
                };
                PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
            }
            else
            {
                object value;
                if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("AnimationDuration", out value))
                {
                    if (value is float) durationInSeconds = (float)value;
                    else if (value is double) durationInSeconds = (float)(double)value;
                    else if (value is int) durationInSeconds = (int)value;
                    else float.TryParse(value.ToString(), out durationInSeconds);
                }

                if (!devMode)
                {
                    if (PlayerPrefs.HasKey("USER_ID"))
                    {
                        WWWForm form = new WWWForm();
                        form.AddField("user_id", PlayerPrefs.GetInt("USER_ID"));
                        UnityWebRequest request = UnityWebRequest.Post("https://compasshero.com/api/getUserSetting", form);
                        yield return request.SendWebRequest();

                        if (request.result != UnityWebRequest.Result.ConnectionError)
                        {
                            try
                            {
                                Root parsedJson = JsonConvert.DeserializeObject<Root>(request.downloadHandler.text);
                                if (parsedJson != null && parsedJson.success && parsedJson.setting != null)
                                {
                                    string waves = parsedJson.setting.wave_option;
                                    SetWaves(waves);
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            t = 0;
        }
        else if (sceneName == "Single")
        {
            if (InteractiveTutorial.Instance == null || InteractiveTutorial.TutorialActive == false)
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
        // AI games always use 2 minutes (120 seconds) regardless of user settings
        durationInSeconds = 120f;
        
        // Still get wave settings from user preferences
        WWWForm form = new WWWForm();
        form.AddField("user_id", PlayerPrefs.GetInt("USER_ID"));
        UnityWebRequest request = UnityWebRequest.Post("https://compasshero.com/api/getUserSetting", form);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            waveType = "none";
        }
        else
        {
            try
            {
                Root parsedJson = JsonConvert.DeserializeObject<Root>(request.downloadHandler.text);
                string waves = parsedJson.setting.wave_option;
                waveType = waves;
            }
            catch
            {
                waveType = "none";
            }
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
            if (t >= durationInSeconds)
            {
                // timerActive = false;
            }
            else
            {
                t += Time.deltaTime;
            }

            t = Mathf.Clamp(t, 0, durationInSeconds);
            if (img_fill != null && durationInSeconds > 0)
            {
                img_fill.fillAmount = 1 - (t / durationInSeconds);
            }
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

    private float GetTurnTimeInSeconds()
    {
        if (durationInSeconds < 120)
        {
            durationInSeconds = 120;
        }
        return durationInSeconds;
    }

    private IEnumerator SendRequestToGetDuration()
    {
#if UNITY_EDITOR
        var parsedJson = new Root
        {
            success = true,
            setting = new Setting
            {
                id = 6,
                userId = 51,
                duration = 2,
                created_at = "2024-12-16T07:19:18.000000Z",
                updated_at = "2024-12-16T10:04:06.000000Z",
                map_shot = 1,
                sub_toggle = 1,
                wave_option = "none"
            }
        };
        durationInSeconds = parsedJson.setting.duration * 60f;
        SetWaves(parsedJson.setting.wave_option);
#else
        if (PlayerPrefs.HasKey("USER_ID"))
        {
            WWWForm form = new WWWForm();
            form.AddField("user_id", PlayerPrefs.GetInt("USER_ID"));
            UnityWebRequest request = UnityWebRequest.Post("https://compasshero.com/api/getUserSetting", form);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                durationInSeconds = 120f;
            }
            else
            {
                Root parsedJson = JsonConvert.DeserializeObject<Root>(request.downloadHandler.text);
                durationInSeconds = parsedJson.setting.duration * 60f;
                string waves = parsedJson.setting.wave_option;
                SetWaves(waves);
            }
        }
        else
        {
            durationInSeconds = 120f;
        }
#endif
        yield return null;
    }

    private void SetWaves(string waves)
    {
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
