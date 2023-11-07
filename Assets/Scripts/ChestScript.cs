using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ChestScript : MonoBehaviourPunCallbacks
{
    public GameObject[] gunPrefabs;
    public Transform[] spawnPoints;
    public bool Spawned;
    public Animator ChestAnim;
    

    private void Start()
    {
        Spawned = false;
    }

    [PunRPC]
    public void Opened(int numberOfGunsToSpawn)
    {
        if (!Spawned)
        {
            Spawned = true;
            ChestAnim.SetBool("Spawned", true);
            if (spawnPoints.Length < numberOfGunsToSpawn && photonView.IsMine)
            {
                Debug.LogError("Not enough spawn points assigned. Please assign at least " + numberOfGunsToSpawn + " spawn points.");
                return;
            }
            if(photonView.IsMine){
            for (int i = 0; i < numberOfGunsToSpawn; i++)
            {
                int randomIndex = Random.Range(0, gunPrefabs.Length); 
                GameObject randomGunPrefab = gunPrefabs[randomIndex]; 

                int spawnPointIndex = i % spawnPoints.Length;
                Vector3 spawnPosition = spawnPoints[spawnPointIndex].position;

                PhotonNetwork.Instantiate(randomGunPrefab.name, spawnPosition, spawnPoints[1].rotation);
            }
            }
        }
    }
}
