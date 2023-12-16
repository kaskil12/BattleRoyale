using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using UnityEngine.Animations.Rigging;
using UnityEngine.Animations;


public class PlayerMovement : MonoBehaviourPunCallbacks
{
    [Header("Camera Movement")]
    public float Sensitivity;
    public float camX;
    public float camY;
    [Header("Rigidbody Movement")]
    public Rigidbody rb;
    public float speed;
    public bool Looks;
    public bool Sliding;
    public bool SlidAble;
    
    [Header("Jumping")]
    public GameObject JumpObject;
    public float jumprad;
    public float JumpPower;
    bool IsGrounded;
    public bool Jumpable = true;
    [Header("PlayerComponents")]
    public GameObject Unequip;
    public float Health = 100;
    public bool Walking;
    public bool Running;
    public bool IsLocalPlayerOwner;
    public float CapsuleHeight;
    public float WalkSpeed;
    public float RunningSpeed;
    public GameObject HipPosition;
    public GameObject AimPosition;
    public bool Aiming;
    public float HandLerpSpeed;
    public GameObject Body;
    //Show what weapon is active
    public GameObject[] ActiveWeapon;
    [Header("Weapons")]
    public GameObject HandObject;
    public int currentWeapon = 0;
    public GameObject[] weaponSlots;
    public Camera MyCamera;
    bool HasLerped;
    [Header("UI")]
    public GameObject PlayerUI;
    public Text AmmoText;
    public Slider HealthSlider;
    public GameObject PickUpText;
    public GameObject OpenChestText;
    public bool SettingsActive;
    public GameObject SettingsObject;
    public Slider SensetivitySlider;
    public Slider VolumeSlider;
    public Toggle fullscreenToggle;
    [Header("Gameplay")]
    public GameObject DeadPosition;
    public string tagName = "SpawnPoint";
    public GameObject[] SpawnPositions;
    private bool hasAppliedForce = false;
    bool SlideDelay;
    [Header("Arms")]
    public TwoBoneIKConstraint LeftArm;
    public TwoBoneIKConstraint RightArm;
    public Transform LeftHandIdle;
    public Transform RightHandIdle;
    public RigBuilder rigBuilder;
    

    
    // Start is called before the first frame update
    void Start()
    {
        Aiming = false;
        SlidAble = true;
        Jumpable = true;
        SettingsActive = false;
        Health = 100;
        DeadPosition = GameObject.Find("SpawnPositions/Dead");
        SpawnPositions = GameObject.FindGameObjectsWithTag(tagName);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Looks = true;
        speed = 50;
        MyCamera = GetComponentInChildren<Camera>();
        IsLocalPlayerOwner = photonView.IsMine;
        SettingsObject.SetActive(false);
        Application.targetFrameRate = 144;
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine){
            IsLocalPlayerOwner = photonView.IsMine;
            Debug.Log("Local player: Enabling camera");
            MyCamera.enabled = true;
            MyCamera.GetComponent<AudioListener>().enabled = true;
            MyCamera = GetComponentInChildren<Camera>();
            Body.GetComponent<MeshRenderer>().enabled = false;
            
            if(weaponSlots[currentWeapon] != null){
                Transform LeftHand = weaponSlots[currentWeapon].GetComponent<GunScript>().LeftHand;
                Transform RightHand = weaponSlots[currentWeapon].GetComponent<GunScript>().RightHand;
                LeftArm.data.target = LeftHand;
                RightArm.data.target = RightHand;
                rigBuilder.Build();
            }else{
                LeftArm.data.target = LeftHandIdle;
                RightArm.data.target = RightHandIdle;
                rigBuilder.Build();
            }
            if(weaponSlots[currentWeapon] != null){
                AmmoText.enabled = true;
                if(weaponSlots[currentWeapon].GetComponent<GunScript>() != null){
                    AmmoText.text =  weaponSlots[currentWeapon].GetComponent<GunScript>().AmmoAmount.ToString() + "/" + weaponSlots[currentWeapon].GetComponent<GunScript>().FullClipAmount.ToString();
                }else{
                    AmmoText.text =  weaponSlots[currentWeapon].GetComponent<RPGScript>().AmmoAmount.ToString() + "/" + weaponSlots[currentWeapon].GetComponent<RPGScript>().FullClipAmount.ToString();
                }
            }else{
                AmmoText.enabled = false;
            }
            HealthSlider.value = Health;
        if(Physics.Raycast(MyCamera.transform.position, MyCamera.transform.forward, out RaycastHit PickupHit, 5)){
            if (PickupHit.transform.CompareTag("Gun")){
                PickUpText.SetActive(true);
            }else{
                PickUpText.SetActive(false);
            }
            //Open Chest
            if (PickupHit.transform.CompareTag("Chest")){
                if(PickupHit.transform.GetComponent<ChestScript>().Spawned == false){
                OpenChestText.SetActive(true);
                }else if(PickupHit.transform.GetComponent<ChestScript>().Spawned == true){
                    OpenChestText.SetActive(false);
                }
                if(Input.GetKeyDown(KeyCode.E)){
                    PhotonView chestPhotonView = PickupHit.transform.gameObject.GetComponent<PhotonView>();
                    if (chestPhotonView != null)
                    {
                        chestPhotonView.RPC("Opened", RpcTarget.AllBuffered, 2);
                    }
                }
            }else{
                OpenChestText.SetActive(false);
            }
        }
        Debug.Log(rb.velocity.magnitude);
        if(rb.velocity.magnitude > 2){
            Walking = true;
        }else{
            Walking = false;
        }
      Look();
      Jumping();
      if(!Sliding){
      Movement();
      }
      if(!IsGrounded)
      {
        speed = 1;
      }
      if(Input.GetKeyDown(KeyCode.E)){
        photonView.RPC("Pickup", RpcTarget.AllBuffered);
        Pickup();
      }
      //Display what gun is active
            ActiveWeapon[0].SetActive(false);
            ActiveWeapon[1].SetActive(false);
            ActiveWeapon[2].SetActive(false);
            if(currentWeapon == 0){
                ActiveWeapon[0].SetActive(true);
            }else if(currentWeapon == 1){
                ActiveWeapon[1].SetActive(true);
            }else if(currentWeapon == 2){
                ActiveWeapon[2].SetActive(true);
            }
      //ScrollWheel
        if(Input.GetAxis("Mouse ScrollWheel") > 0f){
            if(currentWeapon >= weaponSlots.Length - 1){
                currentWeapon = 0;
            }else{
                currentWeapon += 1;
            }
            photonView.RPC("ActivateCurrentGun", RpcTarget.AllBuffered);
            if(weaponSlots[currentWeapon].GetComponent<GunScript>() != null){
                weaponSlots[currentWeapon].GetComponent<GunScript>().Equip();
                }else{
                    weaponSlots[currentWeapon].GetComponent<RPGScript>().Equip();
                }
        }
        //Downscroll wheel
        if(Input.GetAxis("Mouse ScrollWheel") < 0f){
            if(currentWeapon <= 0){
                currentWeapon = weaponSlots.Length - 1;
            }else{
                currentWeapon -= 1;
            }
            photonView.RPC("ActivateCurrentGun", RpcTarget.AllBuffered);
            if(weaponSlots[currentWeapon].GetComponent<GunScript>() != null){
                weaponSlots[currentWeapon].GetComponent<GunScript>().Equip();
                }else{
                    weaponSlots[currentWeapon].GetComponent<RPGScript>().Equip();
                }
        }
      //Set Current Weapon active all the time if weapon is not null
        if(weaponSlots[currentWeapon] != null){
            ActivateCurrentGun();
        }
      if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentWeapon = 0;
            photonView.RPC("GunSlot1", RpcTarget.AllBuffered);
            ActivateCurrentGun();
            photonView.RPC("ActivateCurrentGun", RpcTarget.AllBuffered);
            if(weaponSlots[currentWeapon].GetComponent<GunScript>() != null){
            weaponSlots[currentWeapon].GetComponent<GunScript>().Equip();
            }else{
                weaponSlots[currentWeapon].GetComponent<RPGScript>().Equip();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentWeapon = 1;
            photonView.RPC("GunSlot2", RpcTarget.AllBuffered);
            ActivateCurrentGun();
            photonView.RPC("ActivateCurrentGun", RpcTarget.AllBuffered);
            if(weaponSlots[currentWeapon].GetComponent<GunScript>() != null){
            weaponSlots[currentWeapon].GetComponent<GunScript>().Equip();
            }else{
                weaponSlots[currentWeapon].GetComponent<RPGScript>().Equip();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentWeapon = 2;
            photonView.RPC("GunSlot3", RpcTarget.AllBuffered);
            ActivateCurrentGun();
            photonView.RPC("ActivateCurrentGun", RpcTarget.AllBuffered);
            if(weaponSlots[currentWeapon].GetComponent<GunScript>() != null){
            weaponSlots[currentWeapon].GetComponent<GunScript>().Equip();
            }else{
                weaponSlots[currentWeapon].GetComponent<RPGScript>().Equip();
            }
        }
        if(Input.GetKeyDown(KeyCode.G) && weaponSlots[currentWeapon] != null){
            DropGun();
            photonView.RPC("DropGun", RpcTarget.AllBuffered);
        }
        DisableUnusedGun();
        photonView.RPC("DisableUnusedGun", RpcTarget.AllBuffered);
        //GamePlay like dying 
        if(Health <= 0){
            StartCoroutine(Dying());
        }
        if(Health > 100){
            Health = 100;
        }
        if(Input.GetKeyDown(KeyCode.Escape)){
            SettingsActive = !SettingsActive;
        }
        if(SettingsActive){
            SettingsObject.SetActive(true);
            Looks = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }else{
            SettingsObject.SetActive(false);
            Looks = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        AudioListener.volume = VolumeSlider.value;
        Sensitivity = SensetivitySlider.value;
        if (Input.GetKey(KeyCode.C) && IsGrounded || Input.GetKey(KeyCode.LeftControl) && IsGrounded) {
        if (Input.GetKeyDown(KeyCode.C) && !Sliding) {
            Sliding = true; // Set the sliding state to true
        }
            // Rest of your code
            Sliding = true;
            GetComponentInChildren<CapsuleCollider>().material.dynamicFriction = 0;
            GetComponentInChildren<CapsuleCollider>().material.staticFriction = 0f;
            CapsuleHeight = Mathf.Lerp(CapsuleHeight, 0.5f, 0.05f);
            rb.drag = 0.2f;
            rb.mass = 10;
        } else {
        // Restore any other properties as needed when not sliding
        CapsuleHeight = Mathf.Lerp(CapsuleHeight, 2, 0.05f);
        }
        if(Input.GetKeyUp(KeyCode.C) || Input.GetKeyUp(KeyCode.LeftControl)){
            StartCoroutine(AfterSlide());
        }
        
        GetComponentInChildren<CapsuleCollider>().height = CapsuleHeight;
            
        //Sprinting
        if(Input.GetKey(KeyCode.LeftShift) && IsGrounded){
            Running = true;
            speed = RunningSpeed;
        }else if(IsGrounded){
            speed = WalkSpeed;
            Running = false;
        }
        //Aiming
        if(Input.GetMouseButtonDown(1)){
            Aiming = !Aiming;
            HasLerped = false;
        }
        if(weaponSlots[currentWeapon].GetComponent<GunScript>() != null || weaponSlots[currentWeapon].GetComponent<RPGScript>() != null){
            if (Aiming && !HasLerped) {
                HandObject.transform.localPosition = Vector3.Lerp(HandObject.transform.localPosition, AimPosition.transform.localPosition, HandLerpSpeed * Time.fixedDeltaTime);
                if(weaponSlots[currentWeapon].GetComponent<GunScript>() != null){
                MyCamera.fieldOfView = Mathf.Lerp(MyCamera.fieldOfView, weaponSlots[currentWeapon].GetComponent<GunScript>().AimFov, 0.1f);
                }else{
                    MyCamera.fieldOfView = Mathf.Lerp(MyCamera.fieldOfView, weaponSlots[currentWeapon].GetComponent<RPGScript>().AimFov, 0.1f);
                }
                if (Vector3.Distance(HandObject.transform.localPosition, AimPosition.transform.localPosition) < 0.01f) {
                    HandObject.transform.localPosition = AimPosition.transform.localPosition;
                    HasLerped = true;
                }
            }

            if (!Aiming && !HasLerped) {
                HandObject.transform.localPosition = Vector3.Lerp(HandObject.transform.localPosition, HipPosition.transform.localPosition, HandLerpSpeed * Time.fixedDeltaTime);
                MyCamera.fieldOfView = Mathf.Lerp(MyCamera.fieldOfView, 60, 0.1f);
                if (Vector3.Distance(HandObject.transform.localPosition, HipPosition.transform.localPosition) < 0.01f) {
                    HandObject.transform.localPosition = HipPosition.transform.localPosition;
                    HasLerped = true;
                }
            }
        }else{
            MyCamera.fieldOfView = Mathf.Lerp(MyCamera.fieldOfView, 60, 0.1f);
        }
        
        
    }else{
        Destroy(PlayerUI.gameObject);
        MyCamera.enabled = false;
        MyCamera.GetComponent<AudioListener>().enabled = false;
    }
    }
    void FixedUpdate(){
        if (Sliding && !hasAppliedForce && SlidAble) {
            StartCoroutine(SlideForceDelay());
            rb.AddForce(transform.forward * 100 * Time.fixedDeltaTime, ForceMode.VelocityChange);
            hasAppliedForce = true;
        }
    }
    IEnumerator AfterSlide(){
        GetComponentInChildren<CapsuleCollider>().height = CapsuleHeight;
        yield return new WaitForSeconds(0.4f);
        GetComponentInChildren<CapsuleCollider>().material.dynamicFriction = 0.6f;
        GetComponentInChildren<CapsuleCollider>().material.staticFriction = 0.6f;
        rb.mass = 1;
        Sliding = false;
        hasAppliedForce = false;
    }
    IEnumerator SlideForceDelay(){
        SlidAble = false;
        yield return new WaitForSeconds(0.4f);
        SlidAble = true;
    }
    [PunRPC]
    public void DisableUnusedGun(){
        if (weaponSlots.Length > 0 && currentWeapon >= 0 && currentWeapon < weaponSlots.Length)
        {
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (i != currentWeapon)
                {
                    if(weaponSlots[i] != null){
                    weaponSlots[i].SetActive(false);
                    }
                }
            }
        }
    }

    [PunRPC]
    public void GunSlot1(){
        currentWeapon = 0;
    }
    [PunRPC]
    public void GunSlot2(){
        currentWeapon = 1;
    }
    [PunRPC]
    public void GunSlot3(){
        currentWeapon = 2;
    }
    [PunRPC]
    public void ActivateCurrentGun(){
        weaponSlots[currentWeapon].SetActive(true);
    }
    [PunRPC]
    public void DropGun(){
        weaponSlots[currentWeapon].transform.SetParent(null);
        weaponSlots[currentWeapon].GetComponent<Collider>().enabled = true;
        if(weaponSlots[currentWeapon].GetComponent<GunScript>() != null){
        weaponSlots[currentWeapon].GetComponent<GunScript>().Enabled = false;
        }else{
            weaponSlots[currentWeapon].GetComponent<RPGScript>().Enabled = false;
        }
        weaponSlots[currentWeapon].transform.position = Unequip.transform.position;
        weaponSlots[currentWeapon].transform.rotation = Unequip.transform.rotation;
        weaponSlots[currentWeapon] = null;
    }
    
    [PunRPC]
void Pickup()
{
    if (Physics.Raycast(MyCamera.transform.position, MyCamera.transform.forward, out RaycastHit PickupHit, 5))
    {
        if (PickupHit.transform.CompareTag("Gun"))
        {
            int nextAvailableSlot = -1; // Initialize as -1 to check if any slot is available

            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (weaponSlots[i] == null)
                {
                    nextAvailableSlot = i;
                    break; // Found an available slot, exit the loop
                }
            }

            if (nextAvailableSlot != -1)
            {
                // Notify all clients that the gun has been picked up
                photonView.RPC("SyncPickup", RpcTarget.All, PickupHit.transform.gameObject.GetPhotonView().ViewID, nextAvailableSlot);
            }
            else
            {
                // Handle the case when all slots are full (optional).
                Debug.Log("All weapon slots are full!");
            }
        }
    }
}

[PunRPC]
void SyncPickup(int gunViewID, int slotIndex)
{
    GameObject gun = PhotonView.Find(gunViewID).gameObject;

    weaponSlots[slotIndex] = gun;
    weaponSlots[slotIndex].transform.rotation = HandObject.transform.rotation;
    weaponSlots[slotIndex].transform.parent = HandObject.transform;
    if(weaponSlots[slotIndex].GetComponent<GunScript>() != null){
        weaponSlots[slotIndex].transform.localPosition = weaponSlots[slotIndex].GetComponent<GunScript>().GunOffset;
    }else if(weaponSlots[slotIndex].GetComponent<RPGScript>() != null){
        weaponSlots[slotIndex].transform.localPosition = weaponSlots[slotIndex].GetComponent<RPGScript>().GunOffset;
    }
    //Disable Guns Collider when picked up
        weaponSlots[slotIndex].GetComponent<Collider>().enabled = false;
    if(weaponSlots[slotIndex].GetComponent<GunScript>() != null){
        weaponSlots[slotIndex].GetComponent<GunScript>().Enabled = true;
    }else{
        weaponSlots[slotIndex].GetComponent<RPGScript>().Enabled = true;
    }
}

    IEnumerator Dying(){
        Health = 100;
        DeadPosition = GameObject.Find("SpawnPositions/Dead");
        transform.position = DeadPosition.transform.position;
        yield return new WaitForSeconds(3);
        Health = 100;
        GameObject randomSpawnPoint = SpawnPositions[Random.Range(0, SpawnPositions.Length)];
            
        transform.position = randomSpawnPoint.transform.position;
    }
    [PunRPC]
    public void TakeDamage(float Damage){
        Health -= Damage;
    }
    void Movement()
{
    float moveX = Input.GetAxisRaw("Horizontal");
    float moveZ = Input.GetAxisRaw("Vertical");
    Vector3 moveDirection = (moveX * transform.right + moveZ * transform.forward).normalized;
    Vector3 move = moveDirection * speed * Time.deltaTime;
    rb.AddForce(move, ForceMode.VelocityChange);
}

    void Look()
{
    if (Looks)
    {
        float mouseX = Input.GetAxis("Mouse X") * Sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * Sensitivity * Time.deltaTime;

        // Rotate the player character left/right (Y-axis rotation)
        transform.Rotate(transform.up * mouseX);

        // Rotate the camera up/down (X-axis rotation)
        camX -= mouseY;
        camX = Mathf.Clamp(camX, -70, 70);
        GetComponentsInChildren<Camera>()[0].transform.localRotation = Quaternion.Euler(camX, 0, 0);

        // Rotate the camera left/right (Y-axis rotation)
        camY += mouseX;
        GetComponentsInChildren<Camera>()[0].transform.parent.rotation = Quaternion.Euler(0, camY, 0);
    }
}




    void Jumping(){
        IsGrounded = false;
        foreach(Collider i in Physics.OverlapSphere(JumpObject.transform.position, jumprad)){
            if(i.transform.tag != "Player"){
                IsGrounded = true;
                break;
            }
        }
        if(IsGrounded){
            if(Input.GetKeyDown(KeyCode.Space) && Jumpable == true){
                StartCoroutine(JumpDelay());
                rb.AddForce(transform.up * JumpPower, ForceMode.VelocityChange);
            }
        }
        if(!Sliding){
        rb.drag = IsGrounded ? 15 : 0.1f;
        }
    }
    IEnumerator JumpDelay(){
        Jumpable = false;
        yield return new WaitForSeconds(0.1f);
        Jumpable = true;
    }
    public void ToggleFullscreen()
    {
        // Toggle the fullscreen mode.
        Screen.fullScreen = fullscreenToggle.isOn;
    }
}