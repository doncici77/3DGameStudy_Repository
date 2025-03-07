using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Android; // NameSpace : 소속

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
    public AudioClip audioClipWeaponChanage;
    public GameObject RifleAKobj;

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
    }

    void Update()
    {
        // 마우스 입력을 받아 카메라와 플레이어 회전 처리
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -45, 45);

        isGround = characterController.isGrounded;

        if(isGround && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if(Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
            Debug.Log(isRotaterAroundPlayer ? "1인칭 모드" : "3인칭 모드");
        }

        if(Input.GetKeyDown(KeyCode.F))
        {
            isRotaterAroundPlayer = !isRotaterAroundPlayer;
            Debug.Log(isRotaterAroundPlayer ? "카메라가 주위를 회전합니다." : "플레이어가 시야에 따라서 회전합니다");
        }

        if(isFirstPerson)
        {
            FirstPersonMovement(); // 1인칭 카메라 세팅
        }
        else
        {
            ThirdPersonMovement(); // 3인칭 카메라 세팅
        }

        SettingZoom(); // 줌 상태 변경 함수

        SetAnimator(); // 에니메이션 세팅

        Debug.Log("MoveSpeed: " + moveSpeed);

        if (isAim)
        {
            moveSpeed = 0;
        }
        else
        {
            moveSpeed = isRunnig ? runSpeed : walkSpeed;
        }
    }

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

    void ThirdPersonMovement()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        characterController.Move(move * moveSpeed * Time.deltaTime);

        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        if(isRotaterAroundPlayer)
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
    }

    public void SetTargetDistance(float distance)
    {
        targetDistance  = distance;
    }

    public void SetTargetFov(float fov)
    {
        targetFOV = fov;
    }

    IEnumerator ZoomCamera(float targetDistance)
    {
        while(Mathf.Abs(currentDistance - targetDistance) > 0.01f) // 현재 거리에서 목표 거리로 부드럽게 이동
        {
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        currentDistance = targetDistance; // 목표거리에 도달한 후 값을 고정
    }

    IEnumerator ZoomFieldOfView(float tatgetFov)
    {
        while(Mathf.Abs(mainCamera.fieldOfView - targetFOV) > 0.01f)
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
            if(isAim)
            {
                isFire = true;
                audioSource.PlayOneShot(audioClipFire);
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            isFire = false;
        }

        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            audioSource.PlayOneShot(audioClipWeaponChanage);
            RifleAKobj.SetActive(true);
        }

        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
        animator.SetBool("IsRunnig", isRunnig);
        animator.SetBool("IsAim", isAim);
        animator.SetBool("IsFire", isFire);
    }
}
