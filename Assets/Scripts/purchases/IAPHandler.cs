using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class IAPHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator PurchaseNow()
    {
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
 

            }
            else
            {
                Debug.Log("Turn: Received: " + request.downloadHandler.text);




                Root parsedJson = JsonConvert.DeserializeObject<Root>(request.downloadHandler.text);
 



            }

        }
        else
        {
            Debug.Log("Turn: User Not ID Found");
 

        }
    }
}
