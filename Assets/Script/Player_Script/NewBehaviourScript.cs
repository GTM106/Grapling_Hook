using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//[RequireComponent(typeof(Animator), typeof(Rigidbody), typeof(LineRenderer))]
public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField] private float maxhook_shot_distance = 100.0f; //Hook_Shot��L�΂���ő勗��
    [SerializeField] private float wire_shorten = 1.0f;             //���C���[�̏k�߂��ۂ̎��R��
    [SerializeField] private float spring = 50.0f;                  //���C���[�̕����I����
    [SerializeField] private float damper = 20.0f;                  //���C���[�̕����I����
    [SerializeField] private Vector3 launch_portCenter = new Vector3(0.0f, 0.5f, 0.0f);//Hook_Shot��]���˒n�_
    [SerializeField] private LayerMask interactiveLayers;           //���C���[�����������郌�C���[   
    [SerializeField] private RawImage target_current;               //�\�����Ə��}�[�N�E�֎~�}�[�N�ɐ؂�ւ���
    [SerializeField] private Texture target_marker;                 //�Ə��}�[�N
    [SerializeField] private Texture target_no_marker;              //�֎~�}�[�N

    private bool hook_shot_joint_need_update;//FixedUpdate����hook_shot_joint�̏�ԍX�V���K�v���ǂ�����\���t���O
    private bool hook_shot_current;//���C���[���ˏo�����ǂ�����\���t���O
    private float wire_current;//���݂̃��C���[�̒���...���̒l��FixedUpdate����SpringJoint��maxDistance�ɃZ�b�g����
    private readonly Vector3[] wire_end = new Vector3[2];//Player���Ɛڒ��_���̖��[
    private Vector3 world_launch_port_center;//Hook_Shot�����[���h���W�ɕϊ���������

    //�X�N���v�g��ŃR���|�[�l���g���Q�Ƃ��邽�߂̐錾
    private Transform cameraTransform;
    private LineRenderer hook_shot_renderer;
    //Spring Joint�͓�̃I�u�W�F�N�g���o�l�̂悤�Ɍq��
    private SpringJoint hook_shot_joint;

    void Awake()
    {
        //���ꂼ��錾�����ϐ���Unity��̏����擾
        this.cameraTransform = Camera.main.transform;
        this.hook_shot_renderer = this.GetComponent<LineRenderer>();
        //launch_portCenter�̍��W��world_launch_port_Center�ɕϊ�
        this.world_launch_port_center = this.transform.TransformPoint(this.launch_portCenter);
    }
    void Update()
    {
        hook_shot_string();
        Wire_drawing();
    }
    void hook_shot_string()
    {
        //launch_portCenter�̍��W��world_launch_port_Center�ɕϊ�
        this.world_launch_port_center = transform.TransformPoint(this.launch_portCenter);

        //�J�����̑O�����擾
        var cameraForward = this.cameraTransform.forward;

        //�J�������烌�C���΂�
        var cameraRay = new Ray(this.cameraTransform.position, cameraForward);

        //���̃��C�̏Փ˓_�Ɍ��������C�����߂�B��������̎ˏo�����Ƃ���
        //{ ���_,Physics.Raycast (Vector3 origin(ray�̊J�n�n�_), Vector3 direction(ray�̌���),float distance(ray�̔��ˋ���), int layerMask(���C���}�X�N�̐ݒ�) ? ���C���R���C�_�[�Ƀq�b�g������ �] Hoo��_Shot�̃��[���h���W) : �J�����̑O�� }
        //PositiveInfinity�͖����@Physics.Raycast(cameraRay, out var focus, float.PositiveInfinity, this.interactiveLayers)�����݂���ꍇ�q�b�g�����R���C�_�[��Hook_Shot�̍��W�̃x�N�g�������߃��C���΂��Ă���B���݂��Ȃ��ꍇ�͌��Ă�������̑O���ɔ�΂��B
        var aimingRay = new Ray(this.world_launch_port_center, Physics.Raycast(cameraRay, out var focus, float.PositiveInfinity, this.interactiveLayers) ? focus.point - this.world_launch_port_center : cameraForward);

        //�ˏo������maxhook_shot_distance�ȓ��̋����ɐڒ��\�ȕ��̂�����΁A�����ˏo�ł���
        //(Vector3 origin(ray�̊J�n�n�_), Vector3 direction(ray�̌���),float distance(ray�̔��ˋ���), int layerMask(���C���}�X�N�̐ݒ�))
        if (Physics.Raycast(aimingRay, out var aimingTarget, this.maxhook_shot_distance, this.interactiveLayers))
        {
            //���C�̕\�����Ə��}�[�N�ɕύX
            //�摜�ύX�̍ۂɂ�texture��t����
            this.target_current.texture = this.target_marker;

            //���̏�Ԃō��N���b�N�������ꂽ�ꍇ
            if (Input.GetMouseButtonDown(0))
            {
                //���C���[�̐ڒ��_���[��ݒ�
                this.wire_end[1] = aimingTarget.point;

                //���C���[�ˏo���t���O�𗧂Ă�
                this.hook_shot_current = true;

                //���C���[�̒�����ݒ�
                this.wire_current = Vector3.Distance(this.world_launch_port_center, aimingTarget.point);

                //hook_shot_joint�̍X�V�t���O�𗧂Ă�
                this.hook_shot_joint_need_update = true;
            }
        }
        else
        {
            //���C�̕\�����֎~�}�[�N�ɕύX
            this.target_current.texture = this.target_no_marker;
        }

        //���C���[���ˏo���̏�ԂŃ}�E�X�������ꂽ��
        if (this.hook_shot_current)
        {
            //wire_shorten�̒������܂ŏk�߂�����
            this.wire_current = this.wire_shorten;
            //�t���O�𗧂Ă�
            this.hook_shot_joint_need_update = true;
        }

        //���˃{�^���������ꂽ�ꍇ
        if (Input.GetMouseButtonUp(0))
        {
            //���C���[���ˏo���t���O��false�ɕύX
            this.hook_shot_current = false;
            //hook_shot_joint�̍X�V�t���O�𗧂Ă�
            this.hook_shot_joint_need_update = true;
        }
        //���C���[�̏�Ԃ��X�V����
        this.WireUpdate();
    }
    private void WireUpdate()
    {
        //���C���[�����ˏo���Ȃ�hook_shot_Renderer���A�N�e�B�u�ɂ��ĕ`�悳���A�����Ȃ���Δ�\���ɂ���
        if (this.hook_shot_renderer.enabled = this.hook_shot_current)
        {
            //���C���[���ˏo���̏ꍇ�̂ݏ������s��
            //���C���[�̃L�����N�^�[�����[��ݒ�
            this.wire_end[0] = this.world_launch_port_center;

            //�L�����N�^�[�Ɛڒ��_�̊Ԃɏ�Q�������邩�ǂ���
            //Linecast�͎n�_�ƏI�_��ݒ肵�Ă����ɐ��������A�R���C�_�[���q�b�g�����ꍇ
            if (Physics.Linecast(this.wire_end[0], this.wire_end[1], out var obstacle, this.interactiveLayers))
            {
                //��Q��������΁A�ڒ��_����Q���ɕύX����
                this.wire_end[1] = obstacle.point;
                //(�v���C���[�����C���[�̖��[�ƃ��C���[�̐�[�̋���)�A���݂̃��C���[�̒����̒l����ŏ��l������Ԃ��B
                this.wire_current = Mathf.Min(Vector3.Distance(this.wire_end[0], this.wire_end[1]), this.wire_current);
                //hook_shot_joint�̍X�V�t���O�𗧂Ă�
                this.hook_shot_joint_need_update = true;
            }
            //���C���[�̕`��ݒ���s��
            this.hook_shot_renderer.SetPositions(this.wire_end);
            //hook_shot_joint�̃l�C�s�A�������߂Ă���@
            var gbValue = Mathf.Exp(this.hook_shot_joint != null ? -Mathf.Max(Vector3.Distance(this.wire_end[0], this.wire_end[1]) - this.wire_current, 0.0f) : 0.0f);
            //���C���[�̐F��錾
            var wire_color = new Color(1.0f, gbValue, gbValue);
            //
            this.hook_shot_renderer.startColor = wire_color;
            //
            this.hook_shot_renderer.endColor = wire_color;
        }
    }
    private void Wire_drawing()
    {
        //�X�V�s�v�Ȃ牽�����Ȃ�
        if (!this.hook_shot_joint_need_update)
        {
            return;
        }
        //�ˏo�����ǂ����𔻒�
        if (this.hook_shot_current)
        {
            //�ˏo���ŁA���܂�hook_shot_joint�������Ă��Ȃ���Β���
            if (this.hook_shot_joint == null)
            {
                this.hook_shot_joint = this.gameObject.AddComponent<SpringJoint>();
                this.hook_shot_joint.autoConfigureConnectedAnchor = false;
                this.hook_shot_joint.anchor = this.launch_portCenter;
                this.hook_shot_joint.spring = this.spring;
                this.hook_shot_joint.damper = this.damper;
            }
            //hook_shot_joint�̎��R���Ɛڑ����ݒ肷��
            this.hook_shot_joint.maxDistance = this.wire_current;
            this.hook_shot_joint.connectedAnchor = this.wire_end[1];
        }
        else
        {
            //�ˏo���łȂ����hook_shot_joint���폜���A�����ς���N����Ȃ�����
            Destroy(this.hook_shot_joint);
            this.hook_shot_joint = null;
        }
        //�X�V���I������̂ŁA�uSpringJoint�v�X�V�v�t���O��܂�
        this.hook_shot_joint_need_update = false;
    }
}
