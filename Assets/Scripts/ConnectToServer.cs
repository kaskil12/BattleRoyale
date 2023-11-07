using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class Launcher : MonoBehaviourPunCallbacks
{
    void Start()
    {
        // Connect to Photon Master Server when the game starts.
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster(){
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby(){
        SceneManager.LoadScene("Lobby");
    }
}
