using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ProjectileScript : MonoBehaviourPunCallbacks
{
    public GameObject Explosion;
    public float HitRange = 100;
    public float ProjectileDamage = 20;
    public float ProjectileForce = 1000;
    public bool Hit;
    public LayerMask PlayerMask;
    // Start is called before the first frame update
    void Start()
    {
        Hit = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Hit)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, HitRange, PlayerMask);

            foreach (Collider hitCollider in colliders)
            {
                // Apply force to rigidbodies
                if (hitCollider.transform.root.TryGetComponent(out Rigidbody rb))
                {
                    rb.AddExplosionForce(ProjectileForce, transform.position, HitRange);
                }

                PhotonView hitView = hitCollider.transform.root.GetComponent<PhotonView>();
                hitView.RPC("TakeDamage", RpcTarget.AllBuffered, ProjectileDamage);

                Debug.Log("Object affected: " + hitCollider.gameObject.name);
            }
        }
    }
    [PunRPC]
    void OnCollisionEnter(Collision ProjectileBox){
        Hit = true;
        GameObject ExplosionClone = PhotonNetwork.Instantiate(Explosion.name, gameObject.transform.position, gameObject.transform.rotation);
        Destroy(ExplosionClone, 2);
        Destroy(gameObject);
    }
}
