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
    public bool Hit;
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
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, HitRange);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.tag == "Player")
                {
                    PhotonView hitView = hitCollider.transform.GetComponent<PhotonView>();
                    hitView.RPC("TakeDamage", RpcTarget.AllBuffered, ProjectileDamage);
                    if (Physics.Raycast(transform.position, hitCollider.transform.position, out RaycastHit ProjectileHit, HitRange))
                    {
                        Vector3 launchDirection = (transform.position - ProjectileHit.point).normalized;
                        ProjectileHit.transform.GetComponent<Rigidbody>().AddForce(launchDirection * Time.deltaTime * ProjectileForce, ForceMode.Impulse);
                    }
                }
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
