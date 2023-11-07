using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraScript : MonoBehaviour
{
    public void CamShake(){
        gameObject.transform.rotation = Quaternion.Euler(+10, 0, 0);
    }

}
