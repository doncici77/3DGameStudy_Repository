using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Android; // NameSpace : �Ҽ�

public class PlayerManager : MonoBehaviour
{
    private float moveSpeed = 5.0f; // �÷��̾� �̵� �ӵ�
    public float mouseSensitivity = 100.0f; // ���콺 ����
    public Transform cameraTransform; // ī�޶��� Transform
    public CharacterController characterController;
    public Transform playerHead; // �÷��̾� �Ӹ���ġ (1��Ī ��带 ���ؼ�)
    public float thirdPersonDistance = 3.0f; // 3��Ī ��忡�� �÷��̾�� ī�޶��� �Ÿ�
    public Vector3 thirdPersonOffset = new Vector3(0f, 1.5f, 0f); // 3��Ī ��忡�� ī�޶� ������
    public Transform playerLookObj; // �÷��̾� �þ� ��ġ

    public float zoomDistance = 1.0f; // ī�޶� Ȯ��ɶ��� �Ÿ�(3��Ī ��忡�� ���)
    public float zoomSpeed = 5.0f; // Ȯ����Ұ� �Ǵ� �ӵ�
    public float defaultFov = 60.0f; // �⺻ ī�޶� �þ߰�
    public float zoomFov = 30.0f; // Ȯ�� �� ī�޶� �þ߰�(1��Ī ��忡�� ���)

    private float currentDistance; // ���� ī�޶��� �Ÿ�(3��Ī ���)
    private float targetDistance; // ��ǥī�޶��� �Ÿ�
    private float targetFOV; // ��ǥ Fov
    private bool isZoomed = false; // Ȯ�� ���� Ȯ��
    private Coroutine zoomCorutine; // �ڷ�ƾ�� ����Ͽ� Ȯ�� ��� ó��
    private Camera mainCamera; // ī�޶� ������Ʈ

    private float pitch = 0.0f; // ���Ʒ� ȸ�� ��
    private float yaw = 0.0f; // �¿� ȸ�� ��
    private bool isFirstPerson = false; // 1��Ī ��� ����
    private bool isRotaterAroundPlayer = true; // ī�޶� �÷��̾� ������ ȸ���ϴ��� ����

    // �߷� ���� ����
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
        // ���콺 �Է��� �޾� ī�޶�� �÷��̾� ȸ�� ó��
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
            Debug.Log(isRotaterAroundPlayer ? "1��Ī ���" : "3��Ī ���");
        }

        if(Input.GetKeyDown(KeyCode.F))
        {
            isRotaterAroundPlayer = !isRotaterAroundPlayer;
            Debug.Log(isRotaterAroundPlayer ? "ī�޶� ������ ȸ���մϴ�." : "�÷��̾ �þ߿� ���� ȸ���մϴ�");
        }

        if(isFirstPerson)
        {
            FirstPersonMovement(); // 1��Ī ī�޶� ����
        }
        else
        {
            ThirdPersonMovement(); // 3��Ī ī�޶� ����
        }

        SettingZoom(); // �� ���� ���� �Լ�

        SetAnimator(); // ���ϸ��̼� ����

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

        // �յ��¿� ������ ī�޶� ��ġ �������� ��ȭ�� moveDirection ����
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
            // ī�޶� �÷��̾� �����ʿ��� ȸ���ϵ��� ����
            Vector3 direction = new Vector3(0, 0, -currentDistance);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

            // ī�޶� �÷��̾��� �����ʿ��� ������ ��ġ�� �̵�
            cameraTransform.position = transform.position + thirdPersonOffset + rotation * direction;

            // ī�޶� �÷��̾��� ��ġ�� ���󰡵��� ����
            cameraTransform.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
        }
        else
        {
            // �÷��̾ ���� ȸ���ϴ� ���
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
        while(Mathf.Abs(currentDistance - targetDistance) > 0.01f) // ���� �Ÿ����� ��ǥ �Ÿ��� �ε巴�� �̵�
        {
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSpeed);
            yield return null;
        }

        currentDistance = targetDistance; // ��ǥ�Ÿ��� ������ �� ���� ����
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
        if (Input.GetMouseButtonDown(1)) // 1: ������ ���콺 ��ư ��������
        {
            isAim = true;

            if (zoomCorutine != null) // zoomCorutine�� ���� ������ (�ߺ� ������ ����)
            {
                StopCoroutine(zoomCorutine); // zoomCorutine ���� �ִ� �ڷ�ƾ�� �����Ѵ�.
            }

            if (isFirstPerson) // 1��Ī�� ���
            {
                SetTargetFov(zoomFov); // targetFOV(��ǥ Fov)�� zoomFov(Ȯ�� �� ī�޶� �þ߰�)���� ����

                // targetFOV(��ǥ Fov)�� ZoomFieldOfView�ڷ�ƾ �Լ��� �Ű������� �־ 
                // StartCoroutine�� ����
                // ������ ���� zoomCorutine�� ����(�ڷ�ƾ Ȱ��ȭ ����Ȯ��)
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

            if (isFirstPerson) // 1��Ī�� ���
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
