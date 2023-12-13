using System.Collections;
using UnityEngine;
using Photon.Pun;
public class RPGScript : MonoBehaviourPunCallbacks
{
    public bool Enabled;
    bool Reloading = false;
    public bool Shoot;
    public AudioSource ShootSound;
    public GameObject BulletHoleM14;
    public ParticleSystem flash;
    public RaycastHit GunHit;
    public Animator GunAnim;
    public ParticleSystem tracer;
    public GameObject tracerObject;
    public Camera GunCam;
    public bool RunningPlayer;
    public bool LocalCheck;
    public bool Shooting;
    public bool GunRun;
    public bool WalkingPlayer;
    public GameObject Projectile;
    public GameObject ProjectileSpawnPos;
    bool isShooting = false;
    [Header("GunSpesifications")]
    public float EquipDelayAmount;
    public float ProjectileSpeed;
    public float AmmoAmount;
    public float RunningAccuracy;
    public float WalkingAccuracy;
    public float ShootDelayAmount;
    public float ReloadDelayAmount;
    public float FullClipAmount;
    public float DamageAmountGunBody;
    public float DamageAmountGunHead;
    public float GunRange;
    public float SwayMultiplier;
    public float SwaySmooth;
    public float GunRecoil;
    public float GunRecoilLerpSpeed;
    public float GunRecoilYRight;
    public float GunRecoilYLeft;
    public float GunRecoilYLerpSpeed;
    bool isLerp = false;
    float lerpTarget = 0;
    bool CamYisLerp = false;
    float CamYlerpTarget = 0;
    public float AimFov;
    public Vector3 GunOffset;



    // Start is called before the first frame update
    void Start()
    {
        GetComponent<BoxCollider>().enabled = false;
        AmmoAmount = FullClipAmount;
        Enabled = false;
        Shoot = true;
        StartCoroutine(EquipDelayPickup());
    }

    // Update is called once per frame
    void Update()
    {
        if(Enabled){
            Transform parentTransform = transform.parent;
            if(!Shooting && !Reloading && parentTransform.parent.parent.GetComponentInParent<PlayerMovement>().Running){
                GunAnim.SetBool("Run", true);
            }else{
                GunAnim.SetBool("Run", false);
            }
        LocalCheck = parentTransform.parent.parent.GetComponentInParent<PlayerMovement>().IsLocalPlayerOwner;
        }
        if(Enabled && LocalCheck){
            Transform parentTransform = transform.parent;
            GunCam = parentTransform.parent.GetComponentInParent<Camera>();
            WalkingPlayer = parentTransform.parent.parent.GetComponentInParent<PlayerMovement>().Walking;
            RunningPlayer = parentTransform.parent.parent.GetComponentInParent<PlayerMovement>().Running;
            Vector3 forward = GunCam.transform.TransformDirection(Vector3.forward) * 10;
            Debug.DrawRay(transform.position, forward, Color.green);
        if(!Equiping && !Reloading && Input.GetMouseButton(0) && Shoot && AmmoAmount > 0){
        CallGun();
        }
        if(!Equiping &&Input.GetKeyDown(KeyCode.R) && AmmoAmount < 30 && !Reloading){
            StartCoroutine(Reload());
        }
        float mouseX = Input.GetAxisRaw("Mouse X") * SwayMultiplier;
        float mouseY = Input.GetAxisRaw("Mouse Y") * SwayMultiplier;
        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);
        Quaternion targetRotation = rotationX * rotationY;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, SwaySmooth * Time.fixedDeltaTime);
        //cameraShake
            PlayerMovement playerMovement = transform.root.GetComponent<PlayerMovement>();
            playerMovement.camX = isLerp && isShooting ? Mathf.Lerp(playerMovement.camX, +lerpTarget, GunRecoilLerpSpeed) : playerMovement.camX;
            playerMovement.camY = CamYisLerp && isShooting ? Mathf.Lerp(playerMovement.camY, +CamYlerpTarget, GunRecoilYLerpSpeed) : playerMovement.camY;
            if(playerMovement.camY == CamYlerpTarget) CamYisLerp = false;
            if(playerMovement.camX == lerpTarget) isLerp = false;
        //Hiding the projectile when no ammo and shot
            if(AmmoAmount == 0){
                ProjectileSpawnPos.GetComponent<MeshRenderer>().enabled = false;
            }else{
                ProjectileSpawnPos.GetComponent<MeshRenderer>().enabled = true;

            }
        }else{
            if(!Enabled){
            GunAnim.SetBool("Run", false);
            }
        }
    }
    [PunRPC]
    void CallGun()
    { 
        Shooting = true;
        AmmoAmount -= 1;
        StartCoroutine(ShootDelay());
        StartCoroutine(LerpStop());
        ShootSound.Play();
        GunAnim.SetTrigger("Shoot");
        if (GunCam != null && Physics.Raycast(GunCam.transform.position, GunCam.transform.forward, out GunHit, GunRange))
        {
            //Instantiating rpg projectile
            Vector3 targetPoint = GunHit.point;
            GameObject ProjectileClone = PhotonNetwork.Instantiate(Projectile.name, ProjectileSpawnPos.transform.position, ProjectileSpawnPos.transform.rotation);
            ProjectileClone.GetComponent<Rigidbody>().AddForce(transform.forward * Time.fixedDeltaTime * ProjectileSpeed, ForceMode.VelocityChange);
        }
        lerpTarget = transform.root.GetComponent<PlayerMovement>().camX - GunRecoil;
        isLerp = true;
        CamYlerpTarget = transform.root.GetComponent<PlayerMovement>().camY - Random.Range(GunRecoilYLeft, GunRecoilYRight);
        CamYisLerp = true;
    }
    IEnumerator LerpStop(){
        isShooting = true;
        yield return new WaitForSeconds(0.2f);
        isShooting = false;
        isLerp = false;
        CamYisLerp = false;
    }
    IEnumerator ShootDelay(){
        Shoot = false;
        yield return new WaitForSeconds(ShootDelayAmount);
        Shooting = false;
        Shoot = true;
    }
     IEnumerator Reload(){
        Shoot = false;
        Reloading = true;
        GunAnim.SetTrigger("Reload");
        yield return new WaitForSeconds(ReloadDelayAmount);
        AmmoAmount = FullClipAmount;
        Reloading = false;
        Shoot = true;
    }
    
    [PunRPC]
    Vector3 CalculateBulletDirection()
    {
        Vector3 direction = GunCam.transform.forward;
        if (RunningPlayer)
        {
            direction += Random.insideUnitSphere * RunningAccuracy;
        }else if(!RunningPlayer && WalkingPlayer){
            direction += Random.insideUnitSphere * WalkingAccuracy;
        }
        return direction.normalized;
    }
    bool Equiping = false;
    public void Equip(){
        GunAnim.SetTrigger("EquipAnim");
        StartCoroutine(EquipDelay());
    }
    IEnumerator EquipDelay(){
        Equiping = true;
        yield return new WaitForSeconds(EquipDelayAmount);
        isLerp = false;
        CamYisLerp = false;
        Equiping = false;
        Shoot = true;
        Reloading = false;
    }
    IEnumerator EquipDelayPickup(){
        yield return new WaitForSeconds(1f);
        GetComponent<BoxCollider>().enabled = true;
    }
}
