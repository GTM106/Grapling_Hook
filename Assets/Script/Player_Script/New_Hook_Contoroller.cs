using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
���}�E�X�N���b�N�Ń��C���[����
�@  ���˒��ɍ��}�E�X�𗣂��ƃ��C���[���k�ށ��@
�@�@���˒��ɍ��}�E�X���N���b�N����ƒ�~���A
�@�@���˒��ɉE�N���b�N����ƃ��C���[���������B
 */
public class New_Hook_Contoroller : MonoBehaviour
{
    enum HookShotState
    {
        //�����˂̏��
        wireDefault,
        //���˂��ꂽ���
        wireShot,
        //���C���[���k�ޏ��
        wireReel,
        //���C���[�̏k���~�܂������
        wireReelStop,
    }

    [SerializeField] GameObject player;
    [SerializeField] GameObject playerArm;
    [SerializeField] bool playerSpeed;
    [SerializeField] float maxDistance = 30.0f;
    [SerializeField] private float spring = 5.0f;
    [SerializeField] private float damper = 1.0f;
    [SerializeField] private Texture targetMarker;
    [SerializeField] private Texture noTargetMarker;
    [SerializeField] private RawImage targetCurrent;
    [SerializeField] private LayerMask wireJudgmentLayers;//���C���[��ڒ��\�����f���郌�C���[
    [SerializeField] private Vector3 anchorPosition = new Vector3(0.0f, 0.5f, 0.0f);//���C���[�̓��B�n�_

    const float shrinkSpeed = 8.5f;
    private float wireDistance;
    private bool wireCurrent;//�ˏo���Ă��邩�̊m�F
    private bool springJointUpdate;//springJoint�̍X�V���K�v���ǂ����̊m�F
    private Vector3 worldAnchorPosition;
    private readonly Vector3[] wireEnd = new Vector3[2];//Player���Ɛڒ��_���̖��[
    private Transform playerArmTransform;
    private SpringJoint hookShotJoint;
    private LineRenderer wireRenderer;

    HookShotState keyHookCurrent;
    void Awake()
    {
        playerArmTransform = Camera.main.transform;
        wireRenderer = GetComponent<LineRenderer>();
        targetCurrent.texture = noTargetMarker;
        keyHookCurrent = HookShotState.wireDefault;
        worldAnchorPosition = transform.TransformPoint(anchorPosition);
    }
    void Update()
    {
        HookRay();
        WireJoint();
    }
    private void HookRay()
    {
        var hookForwrad = this.playerArmTransform.forward;

        //�r����Ray�𔭎�
        Ray armRay = new Ray(playerArmTransform.position, hookForwrad);

        //���C�̏Փ˓_�Ɍ��������C�𔭎�
        Ray aimRay = new Ray(worldAnchorPosition, 
            Physics.Raycast(armRay, out var target, float.PositiveInfinity, wireJudgmentLayers) ? target.point - worldAnchorPosition : hookForwrad);

        if (Physics.Raycast(aimRay, out var targetObj, maxDistance))
        {
            targetCurrent.texture = targetMarker;

            if (Input.GetMouseButton(0))
            {
                //���݂̏�Ԃ̍X�V
                keyHookCurrent = HookShotState.wireShot;

                wireCurrent = true;
                springJointUpdate = true;

                //���C���[�̖��[�ݒ�
                wireEnd[1] = targetObj.transform.position;

                //���C���[�̒����ݒ�
                wireDistance = Vector3.Distance(playerArm.transform.position, targetObj.transform.position);
            }
            //�@
            if (Input.GetMouseButtonUp(0))
            {
                keyHookCurrent = HookShotState.wireReel;

                anchorPosition = new Vector3(0.0f, 0.5f, 0.0f);
                //anchorPosition = (targetObj.transform.localPosition + anchorPosition);
                if(hookShotJoint!=null)
                hookShotJoint.anchor = anchorPosition;

                KeyHookShot();
            }

            Debug.DrawRay(aimRay.origin, aimRay.direction, Color.red);
        }
        else
        {
            wireCurrent = false;

            targetCurrent.texture = noTargetMarker;

            //���݂̏�Ԃ̍X�V
            keyHookCurrent = HookShotState.wireDefault;
        }

        if (Input.GetMouseButtonDown(1))
        {
            keyHookCurrent = HookShotState.wireReelStop;
            KeyHookShot();
        }
    }
    private void KeyHookShot()
    {
        //���˒��ɍ��}�E�X�𗣂��ƃ��C���[���k��
        if (keyHookCurrent == HookShotState.wireReel)
        {
            //�񓯊�����
            if (hookShotJoint != null)
                hookShotJoint.connectedAnchor = wireEnd[1];

            StartCoroutine(WireShrink());
            springJointUpdate = true;
        }

        //���˒��ɍ��}�E�X���N���b�N����ƒ�~
        if (keyHookCurrent == HookShotState.wireReelStop)
        {
            StopCoroutine(WireShrink());
            Destroy(hookShotJoint);
            hookShotJoint = null;
        }
        WireUpdate();
    }
    private IEnumerator WireShrink()
    {
        while (0 < wireDistance)
        {
            //Time.deltaTime : 1 = wireDistance : shrinkSpeed
            wireDistance -= shrinkSpeed * Time.deltaTime;
            wireDistance = Mathf.Max(wireDistance, 0);
            //�ő勗�������݂̋����ɐݒ�
            if (hookShotJoint != null)
                hookShotJoint.maxDistance = wireDistance;
            yield return null;
        }
    }
    private void WireUpdate()
    {
        if (wireRenderer.enabled == wireCurrent)
        {
            //���[��player�ɐݒ�
            wireEnd[0] = playerArmTransform.position;

            //�n�_�ƏI�_�Ԃɐ��������A�R���C�_�[���q�b�g�����ꍇ
            if (Physics.Linecast(wireEnd[0], wireEnd[1], out var obstacle, wireJudgmentLayers))
            {
                //��Q��������΁A�ڒ��_����Q���ɕύX����
                wireEnd[1] = obstacle.transform.position;
                //(�v���C���[�����C���[�̖��[�ƃ��C���[�̐�[�̋���)�A���݂̃��C���[�̒����̒l����ŏ��l������Ԃ��B
                wireDistance = Mathf.Min(Vector3.Distance(wireEnd[0], wireEnd[1]), wireDistance);

                if (gameObject.TryGetComponent<SpringJoint>(out var sj))
                {
                    hookShotJoint = sj;
                }
                else
                {
                    hookShotJoint = gameObject.AddComponent<SpringJoint>();
                    //�ڑ���̃A���J�[�̈ʒu�������I�Ɍv�Z����ׂ���
                    hookShotJoint.autoConfigureConnectedAnchor = false;
                    //�ŏI�n�_�̕ύX
                    hookShotJoint.anchor = anchorPosition;
                    //�o�l�̋�����ݒ�
                    hookShotJoint.spring = spring;
                    //�o�l�̋�����}����ݒ�
                    hookShotJoint.damper = damper;
                    //�ő勗�������݂̋����ɐݒ�
                    hookShotJoint.maxDistance = wireDistance;
                    //���S�_�����C���[�̖��[�ɕύX
                    hookShotJoint.connectedAnchor = wireEnd[1];

                }

                hookShotJoint.connectedBody = obstacle.collider.GetComponent<Rigidbody>();
                springJointUpdate = true;
            }
        }
        wireRenderer.SetPositions(this.wireEnd);

        wireRenderer.startColor = Color.black;
        wireRenderer.endColor = Color.yellow;


    }
    private void WireJoint()
    {
        if (!springJointUpdate)
        {
            return;
        }

        if (wireCurrent == true)
        {
            //�ˏo������hookShotJoint�������Ĉȓ��ꍇ
            if (hookShotJoint != null)
            {
            }

        }
        else
        {
            if (hookShotJoint != null)
            {
            }
        }

        springJointUpdate = false;
    }
}
