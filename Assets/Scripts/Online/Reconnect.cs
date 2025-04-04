using System;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class Reconnect : MonoBehaviourPunCallbacks, IConnectionCallbacks
{
    private LoadBalancingClient loadBalancingClient;
    private AppSettings appSettings;

    //private bool testing = false;
    //public Text testText;

    void Start()
    {
        loadBalancingClient = PhotonNetwork.NetworkingClient;
        appSettings = PhotonNetwork.PhotonServerSettings.AppSettings;
        this.loadBalancingClient.AddCallbackTarget(this);
    }

    void OnDestroy()
    {
        if(this.loadBalancingClient == null)
        {
            return;
        }
        
        this.loadBalancingClient.RemoveCallbackTarget(this);
    }

    void IConnectionCallbacks.OnDisconnected(DisconnectCause cause)
    {
        if (this.CanRecoverFromDisconnect(cause))
        {
            print("Disconnection cause: " + cause);
            this.Recover();
        }

        //if (testing)
        //{
        //    this.Recover();
        //}
    }

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

    private void Recover()
    {
        if (!loadBalancingClient.ReconnectAndRejoin())
        {
            //Debug.LogError("ReconnectAndRejoin failed, trying Reconnect");
            if (!loadBalancingClient.ReconnectToMaster())
            {
                //Debug.LogError("Reconnect failed, trying ConnectUsingSettings");
                if (!loadBalancingClient.ConnectUsingSettings(appSettings))
                {
                    //Debug.LogError("ConnectUsingSettings failed");
                }
            }
            else
            {
                //Debug.Log("Reconnect success");
            }
        }
        else
        {
            //Debug.Log("ReconnectAndRejoin success");
        }
    } 
    
    public void TestBtnClick()
    {
        //testing = true;
        PhotonNetwork.Disconnect();
    }
}