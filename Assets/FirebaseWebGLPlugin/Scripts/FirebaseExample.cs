using System;
using UnityEngine;
using Newtonsoft.Json;
public class FirebaseExample : MonoBehaviour
{
    public void InitializeFirebase()
    {
        FirebaseManager.InitializeFirebase("firebaseConfig");
    }
    public void SignIn(string email, string password)
    {
        FirebaseManager.SignInFirebase(email, password);
    }
    public void SignUp(string email, string password)
    {
        FirebaseManager.SignUpFirebase(email, password);
    }
    public void ReadData()
    {
        FirebaseManager.ReadDataFromDataBase();
    }
    public void WriteData(string jsonString)
    {
        FirebaseManager.WriteDataToDatabase(jsonString);
    }

    #region Callbacks

    //This methods are assigned as listeners to the FirebaseManager events.

    public void OnInitializationSuccess()
    {
        SignIn("ibrahim.nasirdh@gmail.com", "12345679");
    }
    public void OnInitializationFailed()
    {

    }
   
    public void OnSignInSuccess()
    {

    }
    public void OnSignInFailed()
    {

    }
    public void OnSignUpSuccess()
    {

    }
    public void OnSignUpFailed()
    {

    }

    public void OnDataRead(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            Debug.LogError("❌ Firebase JSON string is null or empty.");
            return;
        }

        Debug.Log("📥 Loaded data from Firebase:\n" + data);

        try
        {
            FirebaseDataWrapper wrapper = JsonConvert.DeserializeObject<FirebaseDataWrapper>(data);
            
            if (wrapper != null)
            {
                FireBaseDataManager.Instance.DataLoader.playersRoomData = wrapper.playersRoomData;
                FireBaseDataManager.Instance.DataLoader.AllMatchPlayers = wrapper.allMatchPlayers;
                FireBaseDataManager.Instance.DataLoader.AllAiPlayers = wrapper.allaiPlayers;
                FireBaseDataManager.Instance.DataLoader.Round0List = wrapper.round0List;
                FireBaseDataManager.Instance.DataLoader.Round1List = wrapper.round1List;
                FireBaseDataManager.Instance.DataLoader.Round2List = wrapper.round2List;
                FireBaseDataManager.Instance.DataLoader.localMatchData = wrapper.match;

                Debug.Log("✅ Firebase data parsed and assigned successfully.");
            }
            else
            {
                Debug.LogError("❌ Failed to parse Firebase JSON: deserialized wrapper is null.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("❌ Exception during Firebase JSON deserialization:\n" + ex.Message);
        }
    }
    public void OnDataReadFailed()
    {

    }
    public void OnDataWriteComplete()
    {

    }
    public void OnDataWriteFailed()
    {

    }

    #endregion


    private void Start()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        InitializeFirebase();
        #else
        // Skip Firebase WebGL initialization on non-WebGL builds
        #endif
    }
}
