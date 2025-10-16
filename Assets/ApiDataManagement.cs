using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ApiDataManagement : MonoBehaviour
{
    public List<String> WorkingIDs = new List<String>();
    public string ID;
    public string TrophyName = "Pee Wee Event";
    public string TrophyDescription = "1st Place Winner";

    [ContextMenu("Send Data")]
    public void SendApiData()
    {
        StartCoroutine(PostTrophy(ID));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SendApiData();
        }
    }
    private IEnumerator PostTrophy(string id)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", id);
        form.AddField("trophy_name", TrophyName);
        form.AddField("description", TrophyDescription);

        using (UnityWebRequest www = UnityWebRequest.Post("https://compasshero.com/api/trophies", form))
        {
            yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (www.result == UnityWebRequest.Result.Success)
#else
            if (!www.isNetworkError && !www.isHttpError)
#endif
            {
                Debug.Log("Trophy submitted successfully: " + www.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Trophy submission failed: " + www.error);
            }
        }
    }
}