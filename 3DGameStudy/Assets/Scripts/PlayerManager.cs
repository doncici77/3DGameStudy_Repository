using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI; // NameSpace : 소속

public enum WeaponMode
{
    Pistol,
    Shotgun,
    Rifle
}

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance
    {
        get; 
        private set; // 외부에서 값변경 불가능하게 private 사용
    }

    private float moveSpeed = 5.0f; // 플레이어 이동 속도
    public float mouseSensitivity = 100.0f; // 마우스 감도
    public Transform cameraTransform; // 카메라의 Transform
    public CharacterController characterController;
    public Transform playerHead; // 플레이어 머리위치 (1인칭 모드를 위해서)
    public float thirdPersonDistance = 3.0f; // 3인칭 모드에서 플레이어와 카메라의 거리
    public Vector3 thirdPersonOffset = new Vector3(0f, 1.5f, 0f); // 3인칭 모드에서 카메라 오프셋
    public Transform playerLookObj; // 플레이어 시야 위치

    public float zoomDistance = 1.0f; // 카메라가 확대될때의 거리(3인칭 모드에서 사용)
    public float zoomSpeed = 5.0f; // 확대축소가 되는 속도
    public float defaultFov = 60.0f; // 기본 카메라 시야각
    public float zoomFov = 30.0f; // 확대 시 카메라 시야각(1인칭 모드에서 사용)

    private float currentDistance; // 현재 카메라의 거리(3인칭 모드)
    private float targetDistance; // 목표카메라의 거리
    private float targetFOV; // 목표 Fov
    private bool isZoomed = false; // 확대 여부 확인
    private Coroutine zoomCorutine; // 코루틴을 사용하여 확대 축소 처리
    private Camera mainCamera; // 카메라 컴포넌트

    private float pitch = 0.0f; // 위아래 회전 값
    private float yaw = 0.0f; // 좌우 회전 값
    private bool isFirstPerson = false; // 1인칭 모드 여부
    private bool isRotaterAroundPlayer = true; // 카메라가 플레이어 주위를 회전하는지 여부

    // 중력 관련 변수
    public float gravity = -9.81f;
    public float jumpHeight = 2.0f;
    private Vector3 velocity;
    private bool isGround;

    private Animator animator;
    private float horizontal;
    private float vertical;
    private bool isRunnig = false;
    public float walkSpeed = 5.0f;
    public float runSpeed = 10.0f;
    private bool isAim = false;
    private bool isFire = false;

    public GameObject RifleAKobj;
    private int animationSpeed = 1;

    private bool isCanMove = true;

    public Transform aimTarget;

    private float weaponMaxDistance = 100.0f;
    public LayerMask targetLayerMask;

    public MultiAimConstraint multiAimConstraint;

    public Vector3 boxSize = Vector3.one;
    public float castDistance = 5.0f;
    public LayerMask itemLayer;
    public Transform itemGetPos;

    public GameObject crosshairObj;
    public GameObject itemIcon;

    private bool isHasItemRifle = false;
    private bool isCanAim = false;

    public ParticleSystem rifleEffect;
    public Transform rifleEffectPos;
    private float fireDelay = 0.5f;

    public ParticleSystem damageParticleSystem;

    public Text bulletText;
    private int firebulletCount = 30;
    private int savebulletCount = 120;

    public GameObject flashLightObj;
    private bool isFlashLightOn = false;
    private int playerHp = 100;

    private bool isTakingDamage = false;

    private bool isDead = false;

    public Text playerHpText;
    public GameObject PauseObj;
    private bool isPaused = false;

    private Vector3 lastPosition;
    private Coroutine walkSoundCoroutine = null;
    private Coroutine runSoundCoroutine = null;

    private float currentMoveSpeed;

    private WeaponMode currentWeaponMode= WeaponMode.Rifle;
    private int ShotgunRayCount = 5;
    private float shotgunSpreadAngle = 10.0f;
    private float recoilStrength = 10.0f;
    private float maxRecoilAngle = 10.0f;
    private float currentRecoil = 0;
    private float shakeDuration = 0.03f;
    private float shakeMegnitude = 0.02f;
    private Vector3 originalCameraPosition;
    private Coroutine cameraShakeCoroutine;

    private bool lastOpenedForward = false;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            if(Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        currentDistance = thirdPersonDistance;
        targetDistance = thirdPersonDistance;
        targetFOV = defaultFov;
        mainCamera = cameraTransform.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov;
        animator = GetComponent<Animator>();
        RifleAKobj.SetActive(false);
        crosshairObj.SetActive(false);
        itemIcon.SetActive(false);
        bulletText.text = $"{firebulletCount.ToString()}/{savebulletCount.ToString()}";
        playerHpText.text = $"HP:{playerHp}";
        bulletText.gameObject.SetActive(false);
        flashLightObj.SetActive(false);
        PauseObj.SetActive(false);

        SoundManager.Instance.PlayBGM("InGameBGMSound");
        SoundManager.Instance.SetSFXVolume(0.7f);

        //RenderSettings.fog = true; //안개 효과 활성화
        //RenderSettings.fogColor = Color.blue; //안개의 색 설정
        //RenderSettings.fogDensity = 1.0f; //안개의 밀도 설정
        //RenderSettings.fogStartDistance = 10.0f; //안개 시작 거리와 종료거리 설정(Linear모드에서 사용)
        //RenderSettings.fogEndDistance = 100.0f;
        //RenderSettings.fogMode = FogMode.Exponential; //(지수 함수 기반 안개)

        //if(mainCamera != null) //카메라의 Clear Flags를 Solid Color로 설정하고, 배경색을 안개색으로 설정
        //{
        //    mainCamera.clearFlags = CameraClearFlags.SolidColor;
        //    mainCamera.backgroundColor = RenderSettings.fogColor;
        //}
    }

    void Update()
    {
        if (!isDead)
        {
            SetMouseScope(); // 마우스 움직임 및 범위 처리

            CheckGround(); // 그라운드 체크?

            SetPersonShooter(); // 1인칭 or 3인칭 관리

            if (isHasItemRifle)
            {
                if (isCanAim)
                {
                    SettingZoom(); // 줌 상태 변경 함수
                }

                SetRifle(); // 총 꺼내는 애니메이션 함수

                Reload(); // 재장전 함수
            }

            if(Input.GetKeyUp(KeyCode.Escape))
            {
                isPaused = !isPaused;

                if(isPaused)
                {
                    Pause();
                }
                else
                {
                    ReGame();
                }
            }

            WalkSound(); // 발소리 재생 함수

            Fire(); // 총 발사 함수

            SetAnimator(); // 에니메이션 세팅

            SetMove(); // 움직임 상태 세팅

            if (!isAim)
            {
                SetPickUp(); // 픽업 행동 세팅
            }

            SetAnimationSpeed(); // 애니메이션 스피드 조절

            if (Input.GetKeyDown(KeyCode.Q))
            {
                ActionFlashLight();
            }

            if (currentRecoil > 0)
            {
                currentRecoil -= recoilStrength * Time.deltaTime;
                currentRecoil = Mathf.Clamp(currentRecoil, 0, maxRecoilAngle);
                Quaternion currentRotation = Camera.main.transform.rotation;
                Quaternion recoilRotation = Quaternion.Euler(-currentRecoil, 0, 0);
                Camera.main.transform.rotation = currentRotation * recoilRotation; // 카메라를 제어하는 코드를 꺼야 한다
            }
        }
    }


    void FireShotgun()
    {
        for (int i = 0; i < ShotgunRayCount; i++)
        {
            RaycastHit hit;

            Vector3 origin = Camera.main.transform.position;
            Vector3 spreadDirection = GetSpreadDirection(Camera.main.transform.forward, shotgunSpreadAngle);
            Debug.DrawRay(origin, spreadDirection * castDistance, Color.green, 2.0f);
            if(Physics.Raycast(origin, spreadDirection, out hit, castDistance, targetLayerMask))
            {
                Debug.Log("Shotgun Hit : " +  hit.collider.name);
            }
        }
    }

    Vector3 GetSpreadDirection(Vector3 forwardDirection, float spreadAngle)
    {
        float spreadX = UnityEngine.Random.Range(-spreadAngle, spreadAngle);
        float spreadY = UnityEngine.Random.Range(-spreadAngle, spreadAngle);
        Vector3 spreadDirection = Quaternion.Euler(spreadX, spreadY, 0) * forwardDirection;
        return spreadDirection;
    }

    void ApplyRecoil()
    {
        Quaternion currentRotation = Camera.main.transform.rotation; // 현재 카메라 월드 회전값 가져오기
        Quaternion recoilRotation = Quaternion.Euler(-currentRecoil, 0, 0); // 반동을 계산 하여 x축 상하회전에 추가
        Camera.main.transform.rotation = currentRotation * recoilRotation; // 현재 회전값에 반동을 곱하여 새로운 회전값
        currentRecoil += recoilStrength; // 반동값을 증가
        currentRecoil = Mathf.Clamp(currentRecoil, 0, maxRecoilAngle); // 반동값을 제한
    }

    void StartCameraShake()
    {
        if(cameraShakeCoroutine != null)
        {
            StopCoroutine(cameraShakeCoroutine);
        }
        cameraShakeCoroutine = StartCoroutine(CameraShake(shakeDuration, shakeMegnitude));
    }

    IEnumerator CameraShake(float duration, float magnitude)
    {
        float elapsed = 0;
        Vector3 originalPosition = Camera.main.transform.position;
        while(elapsed < duration)
        {
            float offsetX = UnityEngine.Random.Range(-1, 1) * magnitude;
            float offsetY = UnityEngine.Random.Range(-1, 1) * magnitude;

            Camera.main.transform.position = originalPosition + new Vector3(offsetX, offsetY, 0);

            elapsed += Time.deltaTime;

            yield return null;
        }

        Camera.main.transform.position = originalPosition;
    }

    public void ReGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        SoundManager.Instance.PlaySFX("MenuButtonClick", transform.position, false);
        PauseObj.SetActive(false);
        Time.timeScale = 1.0f; // 게임 시간 재게
    }

    void Pause()
    {
        Cursor.lockState = CursorLockMode.None;
        PauseObj.SetActive(true);
        Time.timeScale = 0; // 게임시간 정지
    }

    public void Restart(string restartSceneName)
    {
        Cursor.lockState = CursorLockMode.Locked;
        SoundManager.Instance.PlaySFX("MenuButtonClick" , transform.position, false);
        PauseObj.SetActive(false);
        Time.timeScale = 1.0f;
        SceneController.Instance.LoadScene(restartSceneName);
    }

    public void Exit()
    {
        SoundManager.Instance.PlaySFX("MenuButtonClick", transform.position, false);
        PauseObj.SetActive(false);
        Time.timeScale = 1.0f;
        Application.Quit();
    }

    void UpdateAimTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        aimTarget.position = ray.GetPoint(10.0f);
    }

    private void SetAnimationSpeed()
    {
        animator.speed = animationSpeed;

        AnimatorStateInfo stateInfo0 = animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo stateInfo1 = animator.GetCurrentAnimatorStateInfo(1);

        if ((stateInfo0.IsName("PickUp") || stateInfo0.IsName("Hit")) && stateInfo0.normalizedTime < 1.0f)
        {
            animationSpeed = 2;

            if (stateInfo0.IsName("PickUp"))
            {
                isCanMove = false;
            }
        }
        else if (stateInfo1.IsName("Hit") && stateInfo1.normalizedTime < 1.0f)
        {
            animationSpeed = 2;
        }
        else
        {
            animationSpeed = 1;
            isCanMove = true;
        }
    }

    private void SetPickUp()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.SetTrigger("PickUp");
        }
    }

    public void PlayPickUp()
    {
        Vector3 origin = itemGetPos.position;
        Vector3 direction = itemGetPos.forward;
        RaycastHit[] hits;
        hits = Physics.BoxCastAll(origin, boxSize / 2, direction, Quaternion.identity, castDistance, itemLayer);
        DebugBox(origin, direction);

        foreach (RaycastHit hit in hits)
        {
            Debug.Log("Item : " + hit.collider.name);
            SoundManager.Instance.PlaySFX("PickUpSound", transform.position, false);

            if (hit.collider.name == "Rifle")
            {
                hit.collider.gameObject.SetActive(false);
                isHasItemRifle = true;
                itemIcon.SetActive(true);
            }
            else if (hit.collider.name == "Ammo")
            {
                hit.collider.gameObject.SetActive(false);
                savebulletCount += 30;
                if(savebulletCount >= 120)
                {
                    savebulletCount = 120;
                }
                bulletText.text = $"{firebulletCount}/{savebulletCount}";
            }
            else if(hit.collider.name == "Door")
            {
                DoorManager doorManager = hit.collider.GetComponent<DoorManager>();

                if(doorManager != null)
                {
                    if (doorManager.isOpen) //doorManager 문이 열려있을 경우
                    {
                        if(lastOpenedForward)
                        {
                            doorManager.CloseForward(transform);
                        }
                        else
                        {
                            doorManager.CloseBackward(transform);
                        }
                    }
                    else //문이 닫혀 있을겨우
                    {
                        if(doorManager.Open(transform))
                        {
                            lastOpenedForward = doorManager.LastOpenedForward;
                        }
                    }

                    return;
                }
            }
        }
    }

    void Reload()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            animator.SetTrigger("Reload");
            SoundManager.Instance.PlaySFX("ReloadSound", transform.position, false);

            savebulletCount = savebulletCount - (30 - firebulletCount);
            firebulletCount = 30;
            bulletText.text = $"{firebulletCount}/{savebulletCount}";
        }
    }

    void ActionFlashLight()
    {
        SoundManager.Instance.PlaySFX("FlashLightOnSound", transform.position, false);
        isFlashLightOn = !isFlashLightOn;
        flashLightObj.SetActive(isFlashLightOn);
    }

    void SetMouseScope()
    {
        // 마우스 입력을 받아 카메라와 플레이어 회전 처리
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -45, 45);
    }

    void CheckGround()
    {
        isGround = characterController.isGrounded;

        if (isGround && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    void SetMove()
    {
        if (!isCanMove)
        {
            moveSpeed = 0.0f;
        }
        else
        {
            moveSpeed = isRunnig ? runSpeed : walkSpeed;
        }

        if (horizontal != 0 || vertical != 0)
        {
            if (moveSpeed == 2)
            {
                currentMoveSpeed = 2;
            }
            else if (moveSpeed == 3)
            {
                currentMoveSpeed = 3;
            }
        }
        else
        {
            currentMoveSpeed = 0;
        }
    }

    /// <summary>
    /// 인칭 변경 함수
    /// </summary>
    void SetPersonShooter()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
            Debug.Log(isRotaterAroundPlayer ? "1인칭 모드" : "3인칭 모드");
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            isRotaterAroundPlayer = !isRotaterAroundPlayer;
            Debug.Log(isRotaterAroundPlayer ? "카메라가 주위를 회전합니다." : "플레이어가 시야에 따라서 회전합니다");
        }

        if (isFirstPerson)
        {
            FirstPersonMovement(); // 1인칭 카메라 세팅
        }
        else
        {
            ThirdPersonMovement(); // 3인칭 카메라 세팅
        }
    }

    /// <summary>
    /// 1인칭 카메라 움직임
    /// </summary>
    void FirstPersonMovement()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        // 앞뒤좌우 누를때 카메라 위치 기준으로 변화값 moveDirection 저장
        Vector3 moveDirection = cameraTransform.forward * vertical + cameraTransform.right * horizontal;
        moveDirection.y = 0;
        characterController.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);

        cameraTransform.position = playerHead.position;
        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0);

        transform.rotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);

        UpdateAimTarget(); // 에임조정
    }

    /// <summary>
    /// 3인칭 움직임
    /// </summary>
    void ThirdPersonMovement()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        characterController.Move(move.normalized * moveSpeed * Time.deltaTime);
        
        UpdateCameraPosition();
    }

    /// <summary>
    /// 3인칭 카메라 회전
    /// </summary>
    void UpdateCameraPosition()
    {
        if (isRotaterAroundPlayer)
        {
            // 카메라가 플레이어 오른쪽에서 회전하도록 설정
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

            // 카메라를 플레이어의 오른쪽에서 고정된 위치로 이동
            cameraTransform.position = transform.position + thirdPersonOffset + rotation * direction;

            // 카메라가 플레이어의 위치를 따라가도록 설정
            cameraTransform.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
        }
        else
        {
            // 플레이어가 직접 회전하는 모드
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            cameraTransform.position = playerLookObj.position + thirdPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;
            cameraTransform.LookAt(playerLookObj.position + new Vector3(0, thirdPersonOffset.y, 0));
        }

        UpdateAimTarget(); // 에임조정
    }

    public void SetTargetDistance(float distance)
    {
        targetDistance = distance;
    }

    public void SetTargetFov(float fov)
    {
        targetFOV = fov;
    }

    IEnumerator ZoomCamera(float targetDistance)
    {
        while (Mathf.Abs(currentDistance - targetDistance) > 0.01f) // 현재 거리에서 목표 거리로 부드럽게 이동
        {
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        currentDistance = targetDistance; // 목표거리에 도달한 후 값을 고정
    }

    IEnumerator ZoomFieldOfView(float tatgetFov)
    {
        while (Mathf.Abs(mainCamera.fieldOfView - targetFOV) > 0.01f)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, tatgetFov, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        mainCamera.fieldOfView = tatgetFov;
    }

    void SettingZoom()
    {

        if (Input.GetMouseButtonDown(1)) // 1: 오른쪽 마우스 버튼 눌렀을때
        {
            isAim = true;

            crosshairObj.SetActive(true);

            // 캐릭터 회전
            multiAimConstraint.data.offset = new Vector3(-35.0f, 0f, 0f);
            flashLightObj.transform.localRotation = Quaternion.Euler(125, 0, 0);

            animator.SetLayerWeight(1, 1);

            if (zoomCorutine != null) // zoomCorutine에 값이 있으면 (중복 차단을 위함)
            {
                StopCoroutine(zoomCorutine); // zoomCorutine 값에 있는 코루틴을 종료한다.
            }

            if (isFirstPerson) // 1인칭일 경우
            {
                SetTargetFov(zoomFov); // targetFOV(목표 Fov)에 zoomFov(확대 시 카메라 시야각)값을 대입

                // targetFOV(목표 Fov)을 ZoomFieldOfView코루틴 함수에 매개변수로 넣어서 
                // StartCoroutine로 실행
                // 실행한 값을 zoomCorutine에 대입(코루틴 활성화 여부확인)
                zoomCorutine = StartCoroutine(ZoomFieldOfView(targetFOV));
            }
            else
            {
                SetTargetDistance(zoomDistance);
                zoomCorutine = StartCoroutine(ZoomCamera(targetDistance));
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            ZoomOut();
        }
    }

    void ZoomOut()
    {
        isAim = false;

        crosshairObj.SetActive(false);

        multiAimConstraint.data.offset = new Vector3(0f, 0f, 0f);
        flashLightObj.transform.localRotation = Quaternion.Euler(90, 0, 0);

        animator.SetLayerWeight(1, 0);

        if (zoomCorutine != null)
        {
            StopCoroutine(zoomCorutine);
        }

        if (isFirstPerson) // 1인칭일 경우
        {
            SetTargetFov(defaultFov);
            zoomCorutine = StartCoroutine(ZoomFieldOfView(targetFOV));
        }
        else
        {
            SetTargetDistance(thirdPersonDistance);
            zoomCorutine = StartCoroutine(ZoomCamera(targetDistance));
        }
    }

    /// <summary>
    /// 총꺼내는 애니메이션 함수
    /// </summary>
    void SetRifle()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            animator.SetTrigger("IsWeaponChange");
            RifleAKobj.SetActive(true);
            isCanAim = true;
            bulletText.gameObject.SetActive(true);
        }
    }

    void Fire()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isAim && !isFire)
            {
                if(currentWeaponMode == WeaponMode.Shotgun)
                {

                    FireShotgun();
                }
                else if(currentWeaponMode == WeaponMode.Rifle)
                {
                    recoilStrength = 0.01f;
                    maxRecoilAngle = 0.1f;
                }

                Debug.Log("recoilStrength : " + recoilStrength);

                if(firebulletCount > 0)
                {
                    //Weapon Type MaxDistance Set
                    weaponMaxDistance = 1000.0f;

                    firebulletCount--;
                    bulletText.text = $"{firebulletCount.ToString()}/{savebulletCount.ToString()}";
                    animator.SetTrigger("Fire");
                    isFire = true;
                    StartCoroutine(DelayFire());

                    ApplyRecoil();
                    StartCameraShake();

                    Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
                    RaycastHit[] hits = Physics.RaycastAll(ray, weaponMaxDistance, targetLayerMask);

                    if (hits.Length > 0)
                    {
                        // 거리를 기준으로 정렬
                        hits = hits.OrderBy(hit => hit.distance).ToArray();

                        if (hits.Length > 2)
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                Debug.Log("충돌 : " + hits[i].collider.name);
                                Debug.DrawLine(ray.origin, hits[i].point, Color.red, 2.0f);
                                hits[i].collider.GetComponent<ZombieManager>().TakeDamage(3.0f);

                                ParticleSystem particle = Instantiate(damageParticleSystem, hits[i].point, Quaternion.identity);
                                particle.Play();
                                SoundManager.Instance.PlaySFX("ZombieTakeDamageSound", hits[i].collider.transform.position, true);
                            }
                        }
                        else
                        {
                            Debug.Log("충돌 : " + hits[0].collider.name);
                            Debug.DrawLine(ray.origin, hits[0].point, Color.red, 2.0f);
                            hits[0].collider.GetComponent<ZombieManager>().TakeDamage(3.0f);

                            //ParticleSystem particle = Instantiate(damageParticleSystem, hits[0].point, Quaternion.identity);
                            //particle.Play();

                            ParticleManager.Instance.ParticlePlay(ParticleType.DamageExplosion, hits[0].transform, hits[0].transform.localScale);
                            SoundManager.Instance.PlaySFX("ZombieTakeDamageSound", hits[0].collider.transform.position, true);
                        }
                    }
                    else
                    {
                        Debug.DrawLine(ray.origin, ray.origin + ray.direction * weaponMaxDistance, Color.green, 2.0f);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 에임 상태, 애니메이션, 총 레이캐스트 함수
    /// </summary>
    void SetAnimator()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            isRunnig = true;
        }
        else
        {
            isRunnig = false;
        }

        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
        animator.SetBool("IsRunnig", isRunnig);
    }

    IEnumerator DelayFire()
    {
        yield return new WaitForSeconds(fireDelay);
        isFire = false;
    }

    public void WeaponChangeSoundOn()
    {
        SoundManager.Instance.PlaySFX("WeaponChangeSound", transform.position, false);
    }

    public void FireSoundOn()   
    {
        SoundManager.Instance.PlaySFX("FireSound", transform.position, false);
        //rifleEffect.Play();

        ParticleManager.Instance.ParticlePlay(ParticleType.WeaponFire, rifleEffectPos, new Vector3(1, 1, 1));
    }

    public void WalkSound()
    {
        // 현재 위치가 이전 위치와 다르면 소리 재생
        if (transform.position != lastPosition)
        {
            if (currentMoveSpeed == 2)
            {
                if (walkSoundCoroutine == null)
                {
                    walkSoundCoroutine = StartCoroutine(WalkSoundPlay());
                }
            }
            else if (moveSpeed == 3)
            {
                if (runSoundCoroutine == null)
                {
                    runSoundCoroutine = StartCoroutine(RunSoundPlay());
                }
            }
        }

        // 현재 위치를 다음 프레임에서 비교할 수 있도록 저장
        lastPosition = transform.position;
    }

    IEnumerator WalkSoundPlay()
    {
        while (currentMoveSpeed == 2) // 조건을 만족하는 동안 반복
        {
            Debug.Log("moveSpeed : " + moveSpeed);
            SoundManager.Instance.PlayWalkSound();
            yield return new WaitForSeconds(0.4f);
        }
        walkSoundCoroutine = null;
    }

    IEnumerator RunSoundPlay()
    {
        while (currentMoveSpeed == 3) // 조건을 만족하는 동안 반복
        {
            SoundManager.Instance.PlayWalkSound();
            yield return new WaitForSeconds(0.3f);
        }
        runSoundCoroutine = null;
    }

    /*// 걷는 사운드 예시
    public void FootStepSoundOn()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hit, 10.0f, itemLayer))
        {
            if (hit.ColliderHit.tag == "Wood")
            {
                audioSource.PlayOneShot(audioClipFire); //발소리재생
            }
            else if (hit.ColliderHit.tag == "Wood")
            {
                audioSource.PlayOneShot(audioClipFire); //발소리재생
            }
        }
    }*/

    private void OnTriggerEnter(Collider other)
    {
        if (!isDead)
        {
            if (isTakingDamage) return; // 이미 데미지를 받고 있으면 중복 방지

            if (other.gameObject.tag == "PlayerDamage")
            {
                Debug.Log("PlayerDamage" + other.gameObject.tag);
                animator.SetTrigger("Damage");
                SoundManager.Instance.PlaySFX("PlayerTakeDamageSound", transform.position, false);
                playerHp -= 30;

                if (playerHp < 0)
                {
                    playerHp = 0;
                    StartCoroutine(Dead());
                    return;
                }

                playerHpText.text = $"HP:{playerHp}";
                StartCoroutine(DelayTakeDamage()); // 일정 시간 후 다시 가능하도록
            }
        }
    }

    private IEnumerator Dead()
    {
        Debug.Log("플레이어 죽음");
        isDead = true;
        ZoomOut();
        animator.SetTrigger("Dead");

        yield return new WaitForSeconds(4.0f);
        gameObject.SetActive(false);
        Pause();
    }

    private IEnumerator DelayTakeDamage()
    {
        isTakingDamage = true;
        yield return new WaitForSeconds(1.0f); // 1초 동안 중복 방지
        isTakingDamage = false;
    }

    /// <summary>
    /// 박스 레이캐스트 디버깅 함수
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    void DebugBox(Vector3 origin, Vector3 direction)
    {
        Debug.Log("박스 레이케스트 디버그 생성");

        Vector3 endPoint = origin + direction * castDistance;

        Vector3[] corners = new Vector3[8];
        corners[0] = origin + new Vector3(-boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[1] = origin + new Vector3(boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[2] = origin + new Vector3(-boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[3] = origin + new Vector3(boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[4] = origin + new Vector3(-boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[5] = origin + new Vector3(boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[6] = origin + new Vector3(-boxSize.x, boxSize.y, boxSize.z) / 2;
        corners[7] = origin + new Vector3(boxSize.x, boxSize.y, boxSize.z) / 2;

        Debug.DrawLine(corners[0], corners[1], Color.green, 3.0f);
        Debug.DrawLine(corners[1], corners[3], Color.green, 3.0f);
        Debug.DrawLine(corners[3], corners[2], Color.green, 3.0f);
        Debug.DrawLine(corners[2], corners[0], Color.green, 3.0f);
        Debug.DrawLine(corners[4], corners[5], Color.green, 3.0f);
        Debug.DrawLine(corners[5], corners[7], Color.green, 3.0f);
        Debug.DrawLine(corners[7], corners[6], Color.green, 3.0f);
        Debug.DrawLine(corners[6], corners[4], Color.green, 3.0f);
        Debug.DrawLine(corners[0], corners[4], Color.green, 3.0f);
        Debug.DrawLine(corners[1], corners[5], Color.green, 3.0f);
        Debug.DrawLine(corners[2], corners[6], Color.green, 3.0f);
        Debug.DrawLine(corners[3], corners[7], Color.green, 3.0f);
        Debug.DrawRay(origin, direction * castDistance, Color.green);

    }

    /// <summary>
    /// 씬이 로드 될때 호출되는 함수
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    //void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    //{
    //    Debug.Log("Loaded Scene : " + scene.name);
    //    //플레이어, Ai, Item, Weapon
    //}
}
