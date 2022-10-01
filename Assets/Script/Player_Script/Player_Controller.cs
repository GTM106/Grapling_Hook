using System.Collections;
using UnityEngine;
public class Player_Controller : MonoBehaviour
{
    //PlayerPrefab�̎w��
    [SerializeField] GameObject player;
    [SerializeField] float moveSpeed = 1;�@ //�ړ����x
    [SerializeField] float limitSpeed = 5f; //�������x
    [SerializeField] float dowSpeed = 0.9f; //����
    Rigidbody rigidbody;
    void Start()
    {
        rigidbody = gameObject.GetComponent<Rigidbody>();
    }
    void Update()
    {
        Player_Move();
    }
    void Player_Move()
    {
        //���E�̃L�[�̓��͂��擾
        float x = Input.GetAxis("Horizontal");

        // �㉺�̃L�[�̓��͂��擾
        float z = Input.GetAxis("Vertical");

        // �J�����̕�������AX-Z���ʂ̒P�ʃx�N�g�����擾
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;

        // �����L�[�̓��͒l�ƃJ�����̌�������A�ړ�����������
        Vector3 moveForward = cameraForward * z + Camera.main.transform.right * x;

        // �ړ������ɃX�s�[�h���|����B�W�����v�◎��������ꍇ�́A�ʓrY�������̑��x�x�N�g���𑫂��B
        rigidbody.velocity = moveForward * moveSpeed + new Vector3(0, rigidbody.velocity.y, 0);

        // �L�����N�^�[�̌�����i�s������
        if (moveForward != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveForward);
        }
    }
}