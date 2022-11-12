using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Animator�ARigidbody�ALineRenderer��K�{�Ƃ��Ă���
// Animator...�����ˏo���Ă��邩�ǂ����ɉ����ĉE���O�ɏo������߂����肷��̂Ɏg�p
// Rigidbody...�X�N���v�g���Œ��ڑ��삵�Ă͂��Ȃ����ASpringJoint�̓���ɕK�v
// LineRenderer...������ʏ�ɕ`�悷�邽�߂Ɏg�p
[RequireComponent(typeof(Animator), typeof(Rigidbody), typeof(LineRenderer))]
public class PlayerData : MonoBehaviour
{
    #region �ϐ�
    [SerializeField, Tooltip("����L�΂���ő勗��")] private float maxDis = 100.0f;
    [SerializeField] private LayerMask interactiveLayers; // �������������郌�C���[
    [SerializeField] private Vector3 casterCenter = new Vector3(0.0f, 0.5f, 0.0f); // �I�u�W�F�N�g�̃��[�J�����W�ŕ\�������̎ˏo�ʒu
    [SerializeField, Tooltip("SpringJoint��spring")] private float spring = 50.0f; // ���̕����I������S������SpringJoint��spring
    [SerializeField, Tooltip("SpringJoint��damper")] private float damper = 20.0f; // ���̕����I������S������SpringJoint��damper
    [SerializeField, Tooltip("�����k�߂����̎��R��")] private float equilibriumLength = 1.0f; // �����k�߂����̎��R��
    [SerializeField, Tooltip("�r�ʒu�̑J�ڎ���")] private float ikTransitionTime = 0f; // ���̎ˏo���ɉE���O�ɐL�΂�����A�����O�������ɉE���߂����肷�鎞�̘r�ʒu�̑J�ڎ���
    [SerializeField, Tooltip("�Ə��}�[�N�E�֎~�}�[�N�ɐ؂�ւ���")] private RawImage reticle; // ���𒣂�邩�ǂ����̏󋵂ɍ��킹�āA����RawImage�̕\�����Ə��}�[�N�E�֎~�}�[�N�ɐ؂�ւ���
    [SerializeField, Tooltip("�Ə��}�[�N")] private Texture reticleImageValid; // �Ə��}�[�N
    [SerializeField, Tooltip("�֎~�}�[�N")] private Texture reticleImageInvalid; // �֎~�}�[�N

    // �e��R���|�[�l���g�ւ̎Q��
    private Animator animator;
    private Transform cameraTransform;
    private LineRenderer lineRenderer;
    private SpringJoint springJoint;

    // �E���L�΂��E�߂�����̃X���[�W���O�̂��߂�...
    private float currentIkWeight; // ���݂̃E�F�C�g
    private float targetIkWeight; // �ڕW�E�F�C�g
    private float ikWeightVelocity; // �E�F�C�g�ω���

    private bool casting; // �����ˏo�����ǂ�����\���t���O
    private bool needsUpdateSpring; // FixedUpdate����SpringJoint�̏�ԍX�V���K�v���ǂ�����\���t���O
    private float stringLength; // ���݂̎��̒���...���̒l��FixedUpdate����SpringJoint��maxDistance�ɃZ�b�g����
    private readonly Vector3[] stringAnchor = new Vector3[2]; // SpringJoint�̃L�����N�^�[���Ɛڒ��_���̖��[
    private Vector3 worldCasterCenter; // casterCenter�����[���h���W�ɕϊ���������


    #endregion

    private void Awake()
    {
        // �X�N���v�g��Ŏg�p����R���|�[�l���g�ւ̎Q�Ƃ��擾����
        animator = this.GetComponent<Animator>();
        cameraTransform = Camera.main.transform;
        lineRenderer = this.GetComponent<LineRenderer>();

        // worldCasterCenter��Update���ł�����X�V���Ă��邪�AAwake���ɂ�����X�V���s����
        // ���Ȃ݂ɍ���̃L�����N�^�[�̏ꍇ�́A�L�����N�^�[��CapsuleCollider���S�ƈ�v����悤�ɂ��Ă���
        worldCasterCenter = this.transform.TransformPoint(this.casterCenter);
    }

    private void Update()
    {
        // �܂���ʒ��S����^�����ʂɐL�т�Ray�����߁A�����worldCasterCenter����
        // ����Ray�̏Փ˓_�Ɍ�����Ray�����߂�...��������̎ˏo�����Ƃ���
        this.worldCasterCenter = this.transform.TransformPoint(this.casterCenter);
        var cameraForward = this.cameraTransform.forward;
        var cameraRay = new Ray(this.cameraTransform.position, cameraForward);
        var aimingRay = new Ray(
            this.worldCasterCenter,
            Physics.Raycast(cameraRay, out var focus, float.PositiveInfinity, this.interactiveLayers)
                ? focus.point - this.worldCasterCenter
                : cameraForward);

        // �ˏo������maximumDistance�ȓ��̋����Ɏ��ڒ��\�ȕ��̂�����΁A�����ˏo�ł���Ɣ��f����
        if (Physics.Raycast(aimingRay, out var aimingTarget, this.maxDis, this.interactiveLayers))
        {
            // reticle�̕\�����Ə��}�[�N�ɕς�...
            this.reticle.texture = this.reticleImageValid;

            // ���̏�ԂŎ����˃{�^���������ꂽ��...
            if (Input.GetMouseButtonDown(0))
            {
                this.stringAnchor[1] = aimingTarget.point; // ���̐ڒ��_���[��ݒ�
                this.casting = true; // �u�����ˏo���v�t���O�𗧂Ă�
                this.targetIkWeight = 1.0f; // IK�ڕW�E�F�C�g��1�ɂ���...�܂�E����ˏo�����ɐL�΂����Ƃ���
                this.stringLength = Vector3.Distance(this.worldCasterCenter, aimingTarget.point); // ���̒�����ݒ�
                this.needsUpdateSpring = true; // �uSpringJoint�v�X�V�v�t���O�𗧂Ă�
            }
        }
        else
        {
            // ���ڒ��s�\�Ȃ�Areticle�̕\�����֎~�}�[�N�ɕς���
            this.reticle.texture = this.reticleImageInvalid;
        }

        // �����ˏo���̏�ԂŎ����k�{�^���������ꂽ��A���̒�����equilibriumLength�܂ŏk�߂�����
        if (this.casting && Input.GetMouseButtonDown(1))
        {
            this.stringLength = this.equilibriumLength;
            this.needsUpdateSpring = true;
        }

        // �����˃{�^���������ꂽ��...
        if (Input.GetMouseButtonDown(0))
        {
            this.casting = false; // �u�����ˏo���v�t���O��܂�
            this.targetIkWeight = 0.0f; // IK�ڕW�E�F�C�g��0�ɂ���...�܂�E������R�p���ɖ߂����Ƃ���
            this.needsUpdateSpring = true; // �uSpringJoint�v�X�V�v�t���O�𗧂Ă�
        }

        // �E�r��IK�E�F�C�g���Ȃ߂炩�ɕω�������
        this.currentIkWeight = Mathf.SmoothDamp(
            this.currentIkWeight,
            this.targetIkWeight,
            ref this.ikWeightVelocity,
            this.ikTransitionTime);

        // ���̏�Ԃ��X�V����
        this.UpdateString();
    }

    private void UpdateString()
    {
        // �����ˏo���Ȃ�lineRenderer���A�N�e�B�u�ɂ��Ď���`�悳���A�����Ȃ���Δ�\���ɂ���
        if (this.lineRenderer.enabled = this.casting)
        {
            // �����ˏo���̏ꍇ�̂ݏ������s��
            // ���̃L�����N�^�[�����[��ݒ肵...
            this.stringAnchor[0] = this.worldCasterCenter;

            // �L�����N�^�[�Ɛڒ��_�̊Ԃɏ�Q�������邩���`�F�b�N��...
            if (Physics.Linecast(
                this.stringAnchor[0],
                this.stringAnchor[1],
                out var obstacle,
                this.interactiveLayers))
            {
                // ��Q��������΁A�ڒ��_����Q���ɕύX����
                // ����ɂ��A���������ɐG���΂����ɂ������悤�ɂȂ�̂�
                // ���S�̂��S���������邩�̂悤�ɐU�镑��
                this.stringAnchor[1] = obstacle.point;
                this.stringLength = Mathf.Min(
                    Vector3.Distance(this.stringAnchor[0], this.stringAnchor[1]),
                    this.stringLength);
                this.needsUpdateSpring = true;
            }

            // ���̕`��ݒ���s��
            // ���̒[�_���m�̋�����stringLength�Ƃ̘�����ɂ���Ď���Ԃ��h��
            // �܂莅���Ԃ��Ȃ��Ă���΁ASpringJoint���k�����Ƃ��Ă��邱�Ƃ�����
            this.lineRenderer.SetPositions(this.stringAnchor);
            var gbValue = Mathf.Exp(
                this.springJoint != null
                    ? -Mathf.Max(Vector3.Distance(this.stringAnchor[0], this.stringAnchor[1]) - this.stringLength, 0.0f)
                    : 0.0f);
            var stringColor = new Color(1.0f, gbValue, gbValue);
            this.lineRenderer.startColor = stringColor;
            this.lineRenderer.endColor = stringColor;
        }
    }

    // �E�r�̎p����ݒ肵�A�E�r���玅���o���Ă���悤�Ɍ�����
    private void OnAnimatorIK(int layerIndex)
    {
        this.animator.SetIKPosition(AvatarIKGoal.RightHand, this.stringAnchor[1]);
        this.animator.SetIKPositionWeight(AvatarIKGoal.RightHand, this.currentIkWeight);
    }

    // SpringJoint�̏�Ԃ��X�V����
    private void FixedUpdate()
    {
        // �X�V�s�v�Ȃ牽�����Ȃ�
        if (!this.needsUpdateSpring)
        {
            return;
        }

        // ���ˏo�����ǂ����𔻒肵...
        if (this.casting)
        {
            // �ˏo���ŁA���܂�SpringJoint�������Ă��Ȃ���Β���...
            if (this.springJoint == null)
            {
                this.springJoint = this.gameObject.AddComponent<SpringJoint>();
                this.springJoint.autoConfigureConnectedAnchor = false;
                this.springJoint.anchor = this.casterCenter;
                this.springJoint.spring = this.spring;
                this.springJoint.damper = this.damper;
            }

            // SpringJoint�̎��R���Ɛڑ����ݒ肷��
            this.springJoint.maxDistance = this.stringLength;
            this.springJoint.connectedAnchor = this.stringAnchor[1];
        }
        else
        {
            // �ˏo���łȂ����SpringJoint���폜���A���ɂ������ς���N����Ȃ�����
            Destroy(this.springJoint);
            this.springJoint = null;
        }

        // �X�V���I������̂ŁA�uSpringJoint�v�X�V�v�t���O��܂�
        this.needsUpdateSpring = false;
    }
}
