using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Animations.Rigging; // NameSpace : 소속

public class PlayerManager : MonoBehaviour
{
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

    public AudioClip audioClipFire;
    private AudioSource audioSource;
    public AudioClip audioClipWeaponChange;
    public AudioClip audioClipPickUp;
    public GameObject RifleAKobj;
    private int animationSpeed = 1;
    string currentAnimation;

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

    public int haveBullet = 30;

    public ParticleSystem rifleEffect;
    private float fireDelay = 0.5f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        currentDistance = thirdPersonDistance;
        targetDistance = thirdPersonDistance;
        targetFOV = defaultFov;
        mainCamera = cameraTransform.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        RifleAKobj.SetActive(false);
        crosshairObj.SetActive(false);
        itemIcon.SetActive(false);
    }

    void Update()
    {
        SetMouseScope(); // 마우스 움직임 및 범위 처리

        CheckGround(); // 그라운드 체크?

        SetPersonShooter(); // 1인칭 or 3인칭 관리

        if(isHasItemRifle)
        {
            if(isCanAim)
            {
                SettingZoom(); // 줌 상태 변경 함수
            }

            SetRifle(); // 총 꺼내는 애니메이션 함수
        }

        SetAnimator(); // 에니메이션 세팅

        SetMove(); // 움직임 상태 세팅

        if(!isAim)
        {
            SetPickUp(); // 픽업 행동 세팅
        }

        SetAnimationSpeed(); // 애니메이션 스피드 조절
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

    void PlayPickUp()
    {
        Vector3 origin = itemGetPos.position;
        Vector3 direction = itemGetPos.forward;
        RaycastHit[] hits;
        hits = Physics.BoxCastAll(origin, boxSize / 2, direction, Quaternion.identity, castDistance, itemLayer);
        DebugBox(origin, direction);

        foreach (RaycastHit hit in hits)
        {
            hit.collider.gameObject.SetActive(false);
            Debug.Log("Item : " + hit.collider.name);
            audioSource.PlayOneShot(audioClipPickUp);

            if (hit.collider.name == "Rifle")
            {
                isHasItemRifle = true;
                itemIcon.SetActive(true);
            }
        }
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
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 앞뒤좌우 누를때 카메라 위치 기준으로 변화값 moveDirection 저장
        Vector3 moveDirection = cameraTransform.forward * vertical + cameraTransform.right * horizontal;
        moveDirection.y = 0;
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

        cameraTransform.position = playerHead.position;
        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0);

        transform.rotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
    }

    /// <summary>
    /// 3인칭 움직임
    /// </summary>
    void ThirdPersonMovement()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        characterController.Move(move * moveSpeed * Time.deltaTime);

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
            isAim = false;

            crosshairObj.SetActive(false);

            multiAimConstraint.data.offset = new Vector3(0f, 0f, 0f);

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

        if (Input.GetMouseButtonDown(0))
        {
            if (isAim && haveBullet > 0 && !isFire)
            {
                //Weapon Type MaxDistance Set
                weaponMaxDistance = 1000.0f;

                animator.SetTrigger("Fire");
                isFire = true;
                StartCoroutine(DelayFire());

                haveBullet--; // 테스트용 코드

                Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
                RaycastHit[] hits = Physics.RaycastAll(ray, weaponMaxDistance, targetLayerMask);

                if (hits.Length > 0)
                {
                    int hitCount = 0;
                    foreach (RaycastHit hit in hits)
                    {
                        Debug.Log("충돌 : " + hit.collider.name + ", count : " + hitCount);
                        Debug.DrawLine(ray.origin, hit.point, Color.red, 2.0f);
                        hitCount++;
                    }

                    // 거리를 기준으로 정렬
                    hits = hits.OrderBy(hit => hit.distance).ToArray();

                    foreach (RaycastHit hit in hits)
                    {
                        Debug.Log(hit.collider.gameObject.name + " - 거리: " + hit.distance);
                    }
                }
                else
                {
                    Debug.DrawLine(ray.origin, ray.origin + ray.direction * weaponMaxDistance, Color.green, 2.0f);
                }
            }

            Debug.Log("남은 총알 : " + haveBullet);
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
        audioSource.PlayOneShot(audioClipWeaponChange);
    }

    public void FireSoundOn()   
    {
        audioSource.PlayOneShot(audioClipFire);
        rifleEffect.Play();
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
        if (other.gameObject.tag == "Zombie")
        {
            FireSoundOn();
            animator.SetTrigger("Damage");
            characterController.enabled = false;
            transform.position = Vector3.zero;
            characterController.enabled = true;
        }
    }

    /// <summary>
    /// 박스 레이캐스트 디버깅 함수
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    void DebugBox(Vector3 origin, Vector3 direction)
    {
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
}
