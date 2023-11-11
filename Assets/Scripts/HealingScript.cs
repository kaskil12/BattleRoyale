using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; // Import Photon.Pun namespace

public class HealingScript : MonoBehaviourPunCallbacks // Inherit from MonoBehaviourPunCallbacks
{
    private PhotonView photonView; // Add PhotonView component
    bool CanHeal;

    void Start()
    {
        photonView = GetComponent<PhotonView>(); // Initialize PhotonView component
        CanHeal = true;
    }

   void OnTriggerEnter(Collider other)
{
    if (!photonView.IsMine) return; // Only the owner can interact with the healing object

    if (other.gameObject.CompareTag("Player") && CanHeal)
    {
        PlayerMovement playerMovement = other.gameObject.transform.root.GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement component not found on the player object.");
            return;
        }
        CanHeal = false;
        photonView.RPC("HealPlayer", RpcTarget.All, playerMovement.photonView.ViewID); // Use RPC to heal player
    }
}

    [PunRPC]
    void HealPlayer(int playerID)
    {
        PlayerMovement playerMovement = PhotonView.Find(playerID).GetComponent<PlayerMovement>();
        playerMovement.Health += 20;
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
            StartCoroutine(EnableMeshRendererAfterDelay(meshRenderer, 20f));
        }
    }

    private IEnumerator EnableMeshRendererAfterDelay(MeshRenderer meshRenderer, float delay)
    {
        yield return new WaitForSeconds(delay);
        CanHeal = true;
        meshRenderer.enabled = true;
    }
}