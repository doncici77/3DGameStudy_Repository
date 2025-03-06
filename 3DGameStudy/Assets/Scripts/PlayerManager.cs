using UnityEngine; // NameSpace : �Ҽ�

public class PlayerManager : MonoBehaviour
{
    public float moveSpeed = 5.0f; // �÷��̾� �̵� �ӵ�
    public float mouseSensitivity = 100.0f; // ���콺 ����
    public Transform cameraTransform; // ī�޶��� Transform
    public CharacterController characterController;
    public Transform playerHead; // �÷��̾� �Ӹ���ġ (1��Ī ��带 ���ؼ�)
    public float thirdPersonDistance = 3.0f; // 3��Ī ��忡�� �÷��̾�� ī�޶��� �Ÿ�
    public Vector3 thirdPersonOffset = new Vector3(0, 1.5f, 0f); // 3��Ī ��忡�� ī�޶� ������
    public Transform playerLookObj; // �÷��̾� �þ� ��ġ

    public float zoomDistance = 1.0f; // ī�޶� Ȯ��ɶ��� �Ÿ�(3��Ī ��忡�� ���)
    public float zoomSpeed = 5.0f; // Ȯ����Ұ� �Ǵ� �ӵ�
    public float defaultFov = 60.0f; // �⺻ ī�޶� �þ߰�
    public float zoomFov = 30.0f; // Ȯ�� �� ī�޶� �þ߰�(1��Ī ��忡�� ���)

    void Start()
    {

    }

    void Update()
    {
        
    }
}
