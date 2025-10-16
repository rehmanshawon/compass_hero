using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

public class FirebaseManager : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void Initialize(string config, string callbackObject, string successCallbackMethod,
        string failCallbackMethod);

    [DllImport("__Internal")]
    private static extern void SignUp(string email, string password, string callbackObject, string callbackMethod);

    [DllImport("__Internal")]
    private static extern void SignIn(string email, string password, string callbackObject, string callbackMethod);

    [DllImport("__Internal")]
    private static extern void ReadData(string userId, string callbackObject, string callbackMethod);

    [DllImport("__Internal")]
    private static extern void WriteData(string userId, string jsonString, string callbackObject,
        string callbackMethod);
#endif

    public static string currentUID;


    public UnityEvent OnFirebaseInitializedSucceeded;
    public UnityEvent OnFirebaseInitializationFailed;
    public UnityEvent OnSignUpSuccess;
    public UnityEvent OnSignUpFailed;
    public UnityEvent OnSignInSuccess;
    public UnityEvent OnSignInFailed;
    public UnityEvent<string> OnDataReadComplete;
    public UnityEvent OnDataReadFailed;
    public UnityEvent OnDataWriteComplete;
    public UnityEvent OnDataWriteFailed;


    #region CallbackMethods

    private void SignInComplete(string result)
    {
        Debug.Log($"Fireabase Sign in Complete {result}");
        currentUID = result;
        OnSignInSuccess.Invoke();
    }

    private void SignInFailed(string result)
    {
        Debug.Log($"Fireabase Sign In failed: {result}");
        OnSignInFailed.Invoke();
    }

    private void SignUpComplete(string result)
    {
        Debug.Log($"Firebase Sign Up Complete: {result}");
        OnSignUpSuccess.Invoke();
    }

    private void SignUpFailed(string result)
    {
        Debug.Log($"Firebase SignUp Failed: {result}");
        OnSignUpFailed.Invoke();
    }

    private void FirebaseInitialized(string result)
    {
        Debug.Log($"Firebase Initialized {result}");
        OnFirebaseInitializedSucceeded.Invoke();
    }

    public void FirebaseInitializationFailed(string result)
    {
        Debug.Log($"Firebase Initialization Failed: {result}");
        OnFirebaseInitializationFailed.Invoke();
    }


    private void DataReadFailed(string result)
    {
        Debug.Log($"Data Read Failed: {result}");
        OnDataReadFailed.Invoke();
    }


    private void DataWriteComplete(string result)
    {
        Debug.Log($"Fireabse Data Write Complete: {result}");
        OnDataWriteComplete.Invoke();
    }

    private void DataReadComplete(string result)
    {
        Debug.Log($"Data Read Complete: {result}");
        OnDataReadComplete.Invoke(result);
    }

    private void DataWriteFailed(string result)
    {
        Debug.Log($"Data Write Failed{result}");
        OnDataWriteFailed.Invoke();
    }

    #endregion


    public static void WriteDataToDatabase(string jsonString)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WriteData(currentUID, jsonString, "FirebaseManager", "DataWriteComplete");
#else
        Debug.Log("Firebase WriteDataToDatabase skipped (not WebGL).");
#endif
    }

    //
    public static void ReadDataFromDataBase()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ReadData(currentUID, "FirebaseManager", "DataReadComplete");
#else
        Debug.Log("Firebase ReadDataFromDataBase skipped (not WebGL).");
#endif
    }

    public static void SignInFirebase(string email, string password)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SignIn(email, password, "FirebaseManager", "SignInComplete");
#else
        Debug.Log("Firebase SignIn skipped (not WebGL).");
#endif
    }

    public static void SignUpFirebase(string email, string password)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SignUp(email, password, "FirebaseManager", "SignUpComplete");
#else
        Debug.Log("Firebase SignUp skipped (not WebGL).");
#endif
    }

    public static void InitializeFirebase(string firebaseConfig)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Initialize(firebaseConfig, "FirebaseManager", "FirebaseInitialized", "FirebaseInitializationFailed");
#else
        Debug.Log("Firebase Initialize skipped (not WebGL).");
#endif
    }
}