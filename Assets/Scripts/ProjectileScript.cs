using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ProjectileScript : MonoBehaviourPunCallbacks
{
    public GameObject Explosion;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [PunRPC]
    void OnCollisionEnter(Collision ProjectileBox){
        GameObject ExplosionClone = PhotonNetwork.Instantiate(Explosion.name, gameObject.transform.position, gameObject.transform.rotation);
        Destroy(ExplosionClone, 2);
        Destroy(gameObject);
    }
}
