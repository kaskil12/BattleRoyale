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
    //public ParticleSystem //flash;
    public RaycastHit GunHit;
    //public Animator //////GunAnim;
    //public ParticleSystem //tracer;
    //public GameObject //tracerObject;
    public Camera GunCam;
    public bool RunningPlayer;
    public bool LocalCheck;
    public bool Shooting;
    public bool GunRun;
    public bool WalkingPlayer;
    public GameObject Projectile;
    public GameObject ProjectileSpawnPos;
    [Header("GunSpesifications")]
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



    // Start is called before the first frame update
    void Start()
    {
        AmmoAmount = FullClipAmount;
        Enabled = false;
        Shoot = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Enabled){
            Transform parentTransform = transform.parent;
            if(!Shooting && !Reloading && parentTransform.parent.parent.GetComponentInParent<PlayerMovement>().Running){
                //////GunAnim.SetBool("Run", true);
            }else{
                //////GunAnim.SetBool("Run", false);
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
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, SwaySmooth * Time.deltaTime);
        //cameraShake
            PlayerMovement playerMovement = transform.root.GetComponent<PlayerMovement>();;
            playerMovement.camX = isLerp && Shooting ? Mathf.Lerp(playerMovement.camX, +lerpTarget, GunRecoilLerpSpeed) : playerMovement.camX;
            playerMovement.camY = CamYisLerp && Shooting ? Mathf.Lerp(playerMovement.camY, +CamYlerpTarget, GunRecoilYLerpSpeed) : playerMovement.camY;
            if(playerMovement.camY == CamYlerpTarget) CamYisLerp = false;
            if(playerMovement.camX == lerpTarget) isLerp = false;
        }else{
            if(!Enabled){
            //////GunAnim.SetBool("Run", false);
            }
        }
    }
    void CallGun()
    { 
        Shooting = true;
        AmmoAmount -= 1;
        StartCoroutine(ShootDelay());
        //flash.Play();
        ShootSound.Play();
        //////GunAnim.SetTrigger("Shoot");
        if (GunCam != null && Physics.Raycast(GunCam.transform.position, GunCam.transform.forward, out GunHit, GunRange))
        {
            //Instantiating rpg projectile
            Vector3 targetPoint = GunHit.point;
            GameObject ProjectileClone = PhotonNetwork.Instantiate(Projectile.name, ProjectileSpawnPos.transform.position, ProjectileSpawnPos.transform.rotation);
            ProjectileClone.GetComponent<Rigidbody>().AddForce(transform.forward * ProjectileSpeed * Time.deltaTime, ForceMode.VelocityChange);
            if (GunHit.collider.tag == "Player")
            {
            // PhotonView hitView = GunHit.transform.GetComponent<PhotonView>();
            // hitView.RPC("TakeDamage", RpcTarget.AllBuffered, DamageAmountGunBody);
            }
            if (GunHit.collider.tag == "Head")
            {
            // PhotonView hitView = GunHit.transform.GetComponent<PhotonView>();
            // hitView.RPC("TakeDamage", RpcTarget.AllBuffered, DamageAmountGunHead);
            }
        }
        lerpTarget = transform.root.GetComponent<PlayerMovement>().camX - GunRecoil;
        isLerp = true;
        CamYlerpTarget = transform.root.GetComponent<PlayerMovement>().camY - Random.Range(GunRecoilYLeft, GunRecoilYRight);
        CamYisLerp = true;
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
        //.SetTrigger("Reload");
        yield return new WaitForSeconds(ReloadDelayAmount);
        AmmoAmount = FullClipAmount;
        Reloading = false;
        Shoot = true;
    }
    

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
        ////////GunAnim.SetTrigger("EquipAnim");
        StartCoroutine(EquipDelay());
    }
    IEnumerator EquipDelay(){
        Equiping = true;
        yield return new WaitForSeconds(1);
        Equiping = false;
        Shoot = true;
        Reloading = false;
    }
}
