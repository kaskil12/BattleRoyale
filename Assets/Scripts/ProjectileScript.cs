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

    // Update is called once per frame
    void Update()
    {
        // No need for Update method
    }

    void OnCollisionStay(Collision collision)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, HitRange, PlayerMask);

        foreach (Collider hitCollider in colliders)
        {
            // Apply force to rigidbodies
            if (hitCollider.transform.root.TryGetComponent(out Rigidbody rb))
            {
                rb.AddExplosionForce(ProjectileForce * Time.deltaTime, transform.position, HitRange);
            }

            PhotonView hitView = hitCollider.transform.root.GetComponent<PhotonView>();
            hitView.RPC("TakeDamage", RpcTarget.AllBuffered, ProjectileDamage);

            Debug.Log("Object affected: " + hitCollider.gameObject.name);
        }

        GameObject ExplosionClone = PhotonNetwork.Instantiate(Explosion.name, gameObject.transform.position, gameObject.transform.rotation);
        Destroy(ExplosionClone, 2);
        Destroy(gameObject);
    }
}
