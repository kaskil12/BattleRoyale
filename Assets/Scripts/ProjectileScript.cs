using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ProjectileScript : MonoBehaviourPunCallbacks
{
    public GameObject Explosion;
    public float HitRange;
    public float ProjectileDamage;
    public float ProjectileForce;
    public LayerMask PlayerMask;
    private PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    [PunRPC]
    void OnCollisionStay(Collision collision)
    {
        if (!photonView.IsMine) return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, HitRange, PlayerMask);

        foreach (Collider hitCollider in colliders)
        {
            if (hitCollider.transform.root.TryGetComponent(out Rigidbody rb))
            {
                rb.AddExplosionForce(ProjectileForce * Time.fixedDeltaTime, transform.position, HitRange);
            }

            PhotonView hitView = hitCollider.transform.root.GetComponent<PhotonView>();
            hitView.RPC("TakeDamage", RpcTarget.AllBuffered, ProjectileDamage);

            Debug.Log("Object affected: " + hitCollider.gameObject.name);
        }

        GameObject ExplosionClone = PhotonNetwork.Instantiate(Explosion.name, gameObject.transform.position, gameObject.transform.rotation);
        PhotonNetwork.Destroy(gameObject);
    }
    

}
