using UnityEngine;
using System;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class TabFocusHandler : MonoBehaviourPunCallbacks
{
    private Queue<string> actionQueue = new Queue<string>();
    private bool isOpponentPaused = false;
    public MultiEngine multi;

   
    void Start()
    {
        // Keep Unity WebGL app running in the background
        Application.runInBackground = true;

        // Hook up JavaScript events for tab focus

    }
    private bool wasFocused = true;

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !wasFocused)
        {
            Debug.Log("WebGL tab regained focus. Synchronizing Photon gameplay.");
            OnTabGainedFocus();
        }else if (!hasFocus && wasFocused) 
        {
        OnTabLostFocus();
        
        }
        wasFocused = hasFocus;
    }
    // Method called when focus is lost (JavaScript event triggers this)
    public void OnTabLostFocus()
    {
        Debug.Log("Tab lost focus.");
        photonView.RPC("NotifyOpponentPaused", RpcTarget.Others, true);
    }

    // Method called when focus is regained
    public void OnTabGainedFocus()
    {
        Debug.Log("Tab gained focus.");
        photonView.RPC("NotifyOpponentPaused", RpcTarget.Others, false);

        
        SendRecordedActions();
        // Attempt to upload collected logs to the remote server when focus is regained.
        // Configure the endpoint on your Laravel backend (example: https://yourdomain.com/api/unity-logs)
        try
        {
            if (RemoteLogger.Instance != null)
            {
                // NOTE: replace this URL with your Laravel endpoint that accepts POST JSON with CORS enabled.
                RemoteLogger.Instance.UploadLogsNow("https://compasshero.com/api/unity-logs");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Failed to trigger RemoteLogger upload: " + ex.Message);
        }
    }

 
    public void RecordAction(string rpcData)
    {
        if (isOpponentPaused)
        {
            Debug.Log("Recording action: " + rpcData);
            actionQueue.Enqueue(rpcData);
        }
        else
        {
            Debug.Log("Sending action immediately: " + rpcData);
            photonView.RPC("ReceiveAction", RpcTarget.Others, rpcData);
        }
    }

 
    private void SendRecordedActions()
    {
        while (actionQueue.Count > 0)
        {
            string rpcData = actionQueue.Dequeue();
            photonView.RPC("ReceiveAction", RpcTarget.Others, rpcData);
        }
    }

    // RPC to notify the opponent about the pause state
    [PunRPC]
    private void NotifyOpponentPaused(bool paused)
    {
        Debug.Log("Opponent pause state: " + paused);
        isOpponentPaused = paused;



        if(isOpponentPaused) // start recording all actions
        {

        }
        else // sync opponent data with mine
        {


           
               Debug.Log("whose turn in opponenet unpaused " + multi.whoseTurn);

            if (multi.players[multi.whoseTurn].GetComponent<PlayerInfo>().playTimes <= 0)
            {
                // Prepare authoritative current turn and remaining playTimes to resync clients.
                object[] payload = new object[] { multi.whoseTurn, multi.players[multi.whoseTurn].GetComponent<PlayerInfo>().playTimes };
                Debug.Log("TabFocusHandler: prepared SyncTurn payload on focus regain: " + payload[0] + ", playTimes=" + payload[1]);

                // Only the master client should broadcast authoritative turn-syncs. Non-master clients must not call SyncTurn()
                // because that can cause transient, non-authoritative flips. If this client is master, send the sync now.
                if (PhotonNetwork.IsMasterClient)
                {
                    if (multi.IsWaitingForServerTurnChange)
                    {
                        Debug.Log("TabFocusHandler: master is already waiting for server turn change; skipping SyncTurn.");
                    }
                    else
                    {
                        Debug.Log("TabFocusHandler: master sending SyncTurn payload on focus regain.");
                        multi.SyncTurn(payload);
                    }
                }
                else
                {
                    // Non-master: do not broadcast authoritative SyncTurn. Optionally notify master to resend state.
                    Debug.Log("TabFocusHandler: non-master regained focus — not sending SyncTurn to avoid non-authoritative turn changes.");
                }

            }
            photonView.RPC("SyncTime", RpcTarget.All, multi.bootingObject.GetComponent<GameSettingsApplier>().getCurrentval());


            object[] objs = new object[multi.playerslist.Count];

            for (int i = 0; i < multi.playerslist.Count; i++)
            {
                if (multi.playerslist[i] != null)
                {
                    objs[i] = multi.playerslist[i].position;
                }
                else
                {
                    objs[i] = null;
                }
            }

            multi.SyncPlayersPositions(objs);
        }
    }

    [PunRPC]
    public void SyncTime(float currentval)
    {
        multi.bootingObject.GetComponent<GameSettingsApplier>().updatetime(currentval);

    }
 
    // RPC to receive and apply actions
    [PunRPC]
    private void ReceiveAction(string rpcData)
    {
        Debug.Log("Received action: " + rpcData);
        ApplyAction(rpcData);
    }

    // Apply an action (implement your logic here)
    private void ApplyAction(string rpcData)
    {
        // Parse and execute the action (e.g., move or fire)
        Debug.Log("Applying action: " + rpcData);
        // Example: Move player or fire weapon based on rpcData
    }


}
