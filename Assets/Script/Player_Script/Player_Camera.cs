using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Camera : MonoBehaviour
{
    [SerializeField] Camera mainCamera;//���_
    [SerializeField] Vector2 rotationSpeed;//���_�̈ړ����x
    [SerializeField] GameObject player;
    public bool reverse;//�}�E�X�ړ������ƃJ������]�����𔽓]���锻��t���O
    private Vector2 MousePosition;//�}�E�X�̍��W
    private Vector2 cameraAngle = new Vector2(1, 1);//�J�����̊p�x
    void Start()
    {
        //���W�b�g�{�f�B�̎擾
        Rigidbody rigidbody = GetComponent<Rigidbody>();

    }
    void Update()
    {
        Camera_rotate();
        Camera_Follow();
    }
    public void Camera_rotate()
    {
        //���N���b�N��
        if (Input.GetMouseButtonDown(0))
        {
            //���_�p�x���擾��cameraAngle�Ɋi�[(transform.localEulerAngles�͉�]�p�̎擾���\)
            cameraAngle = mainCamera.transform.localEulerAngles;
            //�}�E�X���W���擾��MousePosition�Ɋi�[(Input.mousePositiom�Ō��݂̃}�E�X���W�̎擾���\)
            MousePosition = Input.mousePosition;
        }
        //���h���b�O���Ă����
        else 
        {
            if (reverse == false)
            {
                //���_�p�x��(�N���b�N���̍��W�ƃ}�E�X���W�̌��ݒl�̍����l)*��]���x����
                cameraAngle.y -= (MousePosition.x - Input.mousePosition.x) * rotationSpeed.y;
                cameraAngle.x -= (Input.mousePosition.y - MousePosition.y) * rotationSpeed.x;

                //cameraAngle�̊p�x���p�x�Ɋi�[
                mainCamera.transform.localEulerAngles = cameraAngle;

                //�}�E�X���W��ϐ�MousePosition�Ɋi�[
                MousePosition = Input.mousePosition;
            }
            else if (reverse == true)
            {
                //���_�p�x��(�N���b�N���̍��W�ƃ}�E�X���W�̌��ݒl�̍����l)*��]���x����
                cameraAngle.y -= (Input.mousePosition.x - MousePosition.x) * rotationSpeed.y;
                cameraAngle.x -= (MousePosition.y - Input.mousePosition.y) * rotationSpeed.x;
                //cameraAngle�̊p�x���p�x�Ɋi�[
                mainCamera.transform.localEulerAngles = cameraAngle;
                //�}�E�X���W��ϐ�MousePosition�Ɋi�[
                MousePosition = Input.mousePosition;
            }
        }
    }
    public void Camera_Follow()
    {
        //�v���C���[�̉�]
        player.transform.rotation = Quaternion.Euler(player.transform.rotation.x, mainCamera.transform.localEulerAngles.y, player.transform.rotation.z);
        //player.transform.localEulerAngles = new Vector3(player.transform.localEulerAngles.x, mainCamera.transform.localEulerAngles.y, player.transform.localEulerAngles.z); 
    }
    //�h���b�O�����Ǝ��_��]�����𔽓]���鏈��
    public void DirectionChange()
    {
        if (reverse == false)
        {
            reverse = true;
        }
        else
        {
            reverse = false;
        }
    }
}
