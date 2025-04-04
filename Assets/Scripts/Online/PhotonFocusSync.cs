using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonFocusSync : MonoBehaviourPunCallbacks
{
    private bool wasFocused = true;

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !wasFocused)
        {
            Debug.Log("WebGL tab regained focus. Synchronizing Photon gameplay.");
            SyncGameplay();
        }
        wasFocused = hasFocus;
    }

    private void SyncGameplay()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            // Request synchronization for all objects
            photonView.RPC("RequestSync", RpcTarget.Others);
        }
    }

    [PunRPC]
    public void RequestSync()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Sync all gameplay objects
            foreach (var obj in FindObjectsOfType<PhotonView>())
            {
                if (obj.IsMine)
                {
                    var position = obj.transform.position;
                    var rotation = obj.transform.rotation;

                    // Broadcast current positions and rotations to all clients
                    obj.RPC("SyncObject", RpcTarget.Others, obj.ViewID, position, rotation);
                }
            }
        }
    }

    [PunRPC]
    public void SyncObject(int viewID, Vector3 position, Quaternion rotation)
    {
        // Update the position and rotation of the specific object
        PhotonView obj = PhotonView.Find(viewID);
        if (obj != null && !obj.IsMine)
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
        }
    }
}
