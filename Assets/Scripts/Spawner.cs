using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Spawner : MonoBehaviour
{
    public GameObject SpawnPoint;
    public GameObject playerPrefab; // Reference to your player prefab

    // Start is called before the first frame update
    void Start()
    {
        // Instantiate the player prefab at the SpawnPoint's position with the default rotation and group 0.
        PhotonNetwork.Instantiate(playerPrefab.name, SpawnPoint.transform.position, Quaternion.identity, 0);
    }
}
