using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class subAvailability : MonoBehaviour
{
    public static subAvailability Instance;


    public static event Action onUpdateSub;
    private void Awake()
    {
        Instance = this;
    }
 
   public bool available=false;
 
    
 
    public void GetUpdatedData()
    {


        StartCoroutine(getdata());
       
    }

    [ContextMenu("LoadScene")]
    public void SceneReseter()
    {
        SceneManager.LoadScene("Main");
    }

    private IEnumerator getdata()
    {
        if (PlayerPrefs.HasKey("USER_ID"))
        {
            
            WWWForm form = new WWWForm();
            form.AddField("user_id", PlayerPrefs.GetInt("USER_ID"));
            UnityWebRequest request = UnityWebRequest.Post("https://compasshero.com/api/getUserSetting", form);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {

                available = false;
            }
            else
            {
               



                Root parsedJson = JsonConvert.DeserializeObject<Root>(request.downloadHandler.text);

                available = parsedJson.setting.sub_toggle == 1 ? true : false;




            }

        }
        else
        {
            available = false;

        }
        if (onUpdateSub != null)
            onUpdateSub.Invoke();
    }

   
}
