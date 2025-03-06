using UnityEngine; // NameSpace : �Ҽ�

public class PlayerManager : MonoBehaviour
{
    public float moveSpeed = 5.0f; // �÷��̾� �̵� �ӵ�
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

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        currentDistance = thirdPersonDistance;
        targetDistance = thirdPersonDistance;
        targetFOV = defaultFov;
        mainCamera = cameraTransform.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov;

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
            FirstPersonMovement();
        }
        else
        {
            ThirdPersonMovement();
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
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

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
}
