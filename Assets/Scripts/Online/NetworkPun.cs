using System.Net.Mime;
using Photon.Pun;
using UnityEngine;

public class NetworkPun : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout = 90000000;
    }
}