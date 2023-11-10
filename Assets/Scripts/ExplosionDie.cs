using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
public class ExplosionDie : MonoBehaviourPunCallbacks {

void Start()
{
    Debug.Log("Start method called");
    StartCoroutine(DestroyAfterTwoSeconds());
}
IEnumerator DestroyAfterTwoSeconds()
{
    Debug.Log("Coroutine started");
    yield return new WaitForSeconds(2f);
    Debug.Log("Coroutine finished waiting");
    if (PhotonNetwork.IsConnected)
    {
        Debug.Log("Connected to PhotonNetwork");
        photonView.RPC("DestroyObject", RpcTarget.AllBuffered);
    }
    else
    {
        Debug.Log("Not connected to PhotonNetwork");
        DestroyObject();
    }
}

[PunRPC]
void DestroyObject()
{
    Debug.Log("DestroyObject method called");
    Destroy(gameObject);
}
}
