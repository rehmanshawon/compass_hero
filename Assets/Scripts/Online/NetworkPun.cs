using Photon.Pun;
using Photon.Realtime;

public class NetworkPun : MonoBehaviourPunCallbacks
{    
    void Start()
    {
        // Set the client timeout to 65 seconds
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout = 65000;
    
 
    }


}