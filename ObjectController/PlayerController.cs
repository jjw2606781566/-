using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum E_Player_Action_Type
{
    Idle,
    Walk,
    Run,
    Sprint,
    Lock,
    InputUnlock
}

/// <summary>
/// �����ƶ����䶯�����ſ�����
/// </summary>
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    public float testRaid;
    public float maxDistance;

    [Header("�����ӽ����(�����)")]
    public Vector3 ViewBoxRange;
    public float ViewBoxDist = 5.0f;

    [Header("�����������")]
    public float MaxLockDist = 100.0f;

    [Header("��������")]
    public float CameraRotateSpeed = 1.0f;
    public float DefaultCameraDist = 3.0f;
    public float CameraFollowRate = 0.05f;
    public float CameraFollowRotateRate = 0.05f;
    public Transform ViewPos;
    public Transform ViewTarget;

    [Header("�������")]
    public float TransformRotateRate = 1.0f;
    public float BaseSpeed = 1.0f; //�����ƶ��ٶȣ�����Run���ٶȼ���
    public float RunSpeedRate = 1.0f;
    public float SprintSpeedRate = 2.0f; //����ٶȱ���
    public float WalkSpeedRate = 0.5f;
    public float MaxCommonLandingHeight = 2.0f; //�����ز���Ӱ��߶�

    //λ����ת���
    //�洢��Ӧ����Ҫʹ�õ���ת�Ƕ�,Ŀ�귽����������������ĳ�����ת���µĽǶ�
    //��һ������Z�᷽�򣬵ڶ�������X�᷽��
    private Dictionary<int, Dictionary<int, float>> RotationDir;
    private CharacterController characterController;
    private float LastSpeed; //��¼λ��˥����һ֡λ�Ƶ��ٶ�
    private Vector3 MoveDir; //��¼��ǰ֡Ӧ��λ�Ƶķ���

    //���ﶯ�����
    private Animator animator;
    private string CurrentLeftHandAnimatorLayer;
    private string CurrentRightHandAnimatorLayer;

    //�ӽ��������
    private bool IsViewLocked = false;
    private Transform TargetLocked;

    //������Զ��������
    private float CameraDist;

    //��������״̬(ǰ��ҡ������ʱ�����޷�ͨ������ȡ������)
    private E_Player_Action_Type PlayerActionType;
    private Vector3 BodyLockDir; //��������ʱ������λ��Ҫ�����ķ���
    private float LockSpeedDecreaseRate; //�ö����Ƿ����ٶ�˥��
    private float LastLandHeight; //��¼����ǰ�ĸ߶�

    //װ�����
    private AttackableBody attackBody;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        //��ȡ�������
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        attackBody = GetComponent<AttackableBody>();

        //�������ﶯ��״̬����ʼ�㼶
        CurrentLeftHandAnimatorLayer = "UnarmedLeftHand";
        CurrentRightHandAnimatorLayer = "UnarmedRightHand";
        StartCoroutine(ChangeHandAnimateLayer(CurrentLeftHandAnimatorLayer, false));
        StartCoroutine(ChangeHandAnimateLayer(CurrentRightHandAnimatorLayer, true));

        //���Խ��㼶
        attackBody.EquipWeapon(true, E_Type_PlayerEquipmentType.Sword);

        //����λ���ٶ�
        LastSpeed = 0;

        //���������λ�ú���ת
        var newPos = ViewPos.position;
        newPos.z -= CameraDist;
        Camera.main.transform.position = newPos;
        CameraDist = DefaultCameraDist;

        //���ó�ʼ����ת����
        RotationDir = new Dictionary<int, Dictionary<int, float>>();
        RotationDir[1] = new Dictionary<int, float>();
        RotationDir[-1] = new Dictionary<int, float>();
        RotationDir[0] = new Dictionary<int, float>();

        RotationDir[1][1] = 45;
        RotationDir[1][0] = 0;
        RotationDir[1][-1] = -45;
        RotationDir[0][0] = 0;
        RotationDir[0][1] = 90;
        RotationDir[0][-1] = -90;
        RotationDir[-1][1] = 135;
        RotationDir[-1][0] = 180;
        RotationDir[-1][-1] = -135;
    }

    /// <summary>
    /// ���ⲿ�ṩ�л����ﶯ���㼶�ķ���
    /// </summary>
    /// <param name="layerName"></param>
    public IEnumerator ChangeHandAnimateLayer(string layerName, bool IsRight)
    {
        //print(PlayerActionType);
        //print(CurrentAnimatorLayer);
        while (PlayerActionType == E_Player_Action_Type.Lock ||
            PlayerActionType == E_Player_Action_Type.InputUnlock)
            yield return null;
        if (IsRight)
        {
            animator.SetLayerWeight(
                animator.GetLayerIndex(CurrentRightHandAnimatorLayer), 0.0f);
            animator.SetLayerWeight(animator.GetLayerIndex(layerName), 1.0f);
            CurrentRightHandAnimatorLayer = layerName;
        }
        else
        {
            animator.SetLayerWeight(
                animator.GetLayerIndex(CurrentLeftHandAnimatorLayer), 0.0f);
            animator.SetLayerWeight(animator.GetLayerIndex(layerName), 1.0f);
            CurrentLeftHandAnimatorLayer = layerName;
        }
        
    }

    public IEnumerator UpdateTargetLockAnimateLayer()
    {
        while (PlayerActionType == E_Player_Action_Type.Lock ||
            PlayerActionType == E_Player_Action_Type.InputUnlock)
            yield return null;
        if (IsViewLocked)
        {
            animator.SetLayerWeight(
                animator.GetLayerIndex("BaseLayer"), 0.0f);
            animator.SetLayerWeight(
                animator.GetLayerIndex("BaseLayerTL"), 1.0f);
            animator.SetLayerWeight(
                animator.GetLayerIndex(CurrentRightHandAnimatorLayer), 0.0f);
            animator.SetLayerWeight(
                animator.GetLayerIndex(CurrentRightHandAnimatorLayer + "TL"), 1.0f);
            animator.SetLayerWeight(
                animator.GetLayerIndex(CurrentLeftHandAnimatorLayer), 0.0f);
            animator.SetLayerWeight(
                animator.GetLayerIndex(CurrentLeftHandAnimatorLayer + "TL"), 1.0f);
        }
        else
        {
            animator.SetLayerWeight(
                animator.GetLayerIndex("BaseLayer"), 1.0f);
            animator.SetLayerWeight(
                animator.GetLayerIndex("BaseLayerTL"), 0.0f);
            animator.SetLayerWeight(
                animator.GetLayerIndex(CurrentRightHandAnimatorLayer), 1.0f);
            animator.SetLayerWeight(
                animator.GetLayerIndex(CurrentRightHandAnimatorLayer + "TL"), 0.0f);
            animator.SetLayerWeight(
                animator.GetLayerIndex(CurrentLeftHandAnimatorLayer), 1.0f);
            animator.SetLayerWeight(
                animator.GetLayerIndex(CurrentLeftHandAnimatorLayer + "TL"), 0.0f);
        }
    }


    private void LockTarget()
    {
        if (TargetLocked)
        {
            //���Ŀ��������һ֡��˲���Ƴ������(�蹷�ٶȺܿ�)
            //�������ҹ�Զ��(��·��)
            //��Ŀ����������֮�����ڵ�(��������)
            //��ʧ����Ŀ��
            Vector3 viewPortPos = Camera.main.WorldToViewportPoint(TargetLocked.position);
            if (Mathf.Abs(viewPortPos.x) > 1 || Mathf.Abs(viewPortPos.y) > 1
                || viewPortPos.z < 0 || viewPortPos.z > (MaxLockDist) ||
                Physics.Linecast(Camera.main.transform.position, TargetLocked.position
                , 1 << LayerMask.NameToLayer("Maskable") |
                1 << LayerMask.NameToLayer("Ground")))
            {
                UnlockTarget();
            }
        }

        if (InputController.Instance.TabDown && !IsViewLocked)
        {
            //������Ұ�����ײ��
            Quaternion boxRotation = Camera.main.transform.rotation;
            Vector3 boxCenter = Camera.main.transform.position
                + Camera.main.transform.forward * (CameraDist + ViewBoxDist);
            Collider[] enemys = Physics.OverlapBox(boxCenter, ViewBoxRange, boxRotation
                , 1 << LayerMask.NameToLayer("Enemy"));

            //�����ȡ��һ��Ŀ��
            TargetLocked = ViewLockTargetMgr.Instance.GetNextTarget(enemys);
            if (TargetLocked != null)
            {
                IsViewLocked = true;
                //���������㼶
                StartCoroutine(UpdateTargetLockAnimateLayer());
            }
        }
        else if (IsViewLocked && InputController.Instance.MouseScroll != 0)
        {
            //������Ұ�����ײ��
            Quaternion boxRotation = Camera.main.transform.rotation;
            Vector3 boxCenter = Camera.main.transform.position
                + Camera.main.transform.forward * (CameraDist + ViewBoxDist);
            Collider[] enemys = Physics.OverlapBox(boxCenter, ViewBoxRange, boxRotation
                , 1 << LayerMask.NameToLayer("Enemy"));
            //foreach (Collider c in enemys)
            //{
            //    print(c.name);
            //}
            if (InputController.Instance.MouseScroll < 0)
                TargetLocked = ViewLockTargetMgr.Instance.GetNextTarget(enemys, TargetLocked);
            else
                TargetLocked = ViewLockTargetMgr.Instance.GetLastTarget(enemys, TargetLocked);

            //�����ȡ����Ŀ�꣬�ͽ���ӽ�����״̬
            if (TargetLocked == null)
            {
                UnlockTarget();
            }
        }
        else if (InputController.Instance.TabDown && IsViewLocked)
        {
            UnlockTarget();
        }

        //����Ŀ��������UIԪ��
        if (IsViewLocked)
        {
            OnGameUIManager.Instance.GetPanel<CombatPanel>().ShowTargetKnob(TargetLocked.transform);

        }
    }

    private void UnlockTarget()
    {
        print("�������");
        IsViewLocked = false;
        TargetLocked = null;
        OnGameUIManager.Instance.GetPanel<CombatPanel>().HideTargetKnob();
        StartCoroutine(UpdateTargetLockAnimateLayer());
    }

    /// <summary>
    /// �Զ�������������߼�
    /// �������۾����ڷ�һ��ViewPos����ת���������ViewPos��ת
    /// ViewPos���ܸ����������ת����ת�����ViewPos�����������������
    /// ���ϸ���ViewPos���������ViewTarget������
    /// �����������ViewPos�ľ���Dist��
    /// ���������Զ�������ViewPos����Dist��������ViewPos
    /// </summary>
    private void AutoUpdateCamera()
    {
        RaycastHit hitInfo;

        //���߼������Ƿ��ڵ�
        if (Physics.Raycast(ViewPos.position,
            Camera.main.transform.position - ViewPos.position,
            out hitInfo,
            DefaultCameraDist,
            1 << LayerMask.NameToLayer("Maskable") |
                1 << LayerMask.NameToLayer("Ground")))
        {
            CameraDist = hitInfo.distance;
        }
        else
        {
            CameraDist = DefaultCameraDist;
        }

        ViewPos.position = ViewTarget.position;

        if (IsViewLocked)
        {
            //֪ͨUI����������
            ViewPos.LookAt(TargetLocked.position);
        }
        else
        {
            //�������������Ŀ�����ת
            float HorizontalCamRotate =
                Input.GetAxis("Mouse X") * CameraRotateSpeed * Time.deltaTime;
            float VerticalCamRotate = //���Ͽ���С�н�(��)�����¿�����н�(��)
                Input.GetAxis("Mouse Y") * CameraRotateSpeed * Time.deltaTime * -1;

            //ˮƽ��תһ�����������Z��
            ViewPos.Rotate(Vector3.up
                , HorizontalCamRotate, Space.World);

            //��ֱ��תһ�����ƿ������local X����ˮƽ���ϵ�ͶӰ
            Vector3 refVerticalAxis = Vector3.ProjectOnPlane(ViewPos.right, Vector3.up);

            //�����������ˮƽ��������ˮƽ��нǲ��ܳ���60��
            //ֱ�Ӽ�������ƽ�淨�����ļн�
            //�ο�����Ǵ�ֱ��ת���ᣬ��Ϊ�����Ƿ���ģ�����*-1
            float verticalAngle = -1 *
                Vector3.SignedAngle(ViewPos.up, Vector3.up, refVerticalAxis);
            if (Mathf.Abs(verticalAngle + VerticalCamRotate) < 60)
                ViewPos.Rotate(refVerticalAxis, VerticalCamRotate, Space.World);
        }


        //��ֵ�������������λ�ò�ʵʱ�����������ת���ﵽƽ������Ч��
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position,
            ViewPos.position - ViewPos.forward * CameraDist, CameraFollowRate);

        Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation,
            ViewPos.rotation, CameraFollowRotateRate);
    }

    /// <summary>
    /// �ú���ͳһ������һ֡�������յ�λ��
    /// ��������ݱ�������������
    /// </summary>
    private void BodyMove()
    {
        //����λ�Ʒ���
        if (PlayerActionType != E_Player_Action_Type.Lock
            && PlayerActionType != E_Player_Action_Type.InputUnlock)
        {
            //ֻ�����ﴦ�ڿ��ƶ�״̬�²Ž��м���
            Vector3 cameraForwardProj =
            Vector3.ProjectOnPlane(ViewPos.forward, Vector3.up);
            MoveDir = Quaternion.AngleAxis(
                RotationDir[InputController.Instance.verticalDirRaw]
                [InputController.Instance.horizontalDirRaw]
                , Vector3.up) * cameraForwardProj;
        }
        else
        {
            MoveDir = BodyLockDir;
        }
        if (MoveDir == Vector3.zero)
            MoveDir = transform.forward;

        //�жϽ�ɫģ�ͳ���
        if ((!IsViewLocked && (PlayerActionType == E_Player_Action_Type.Run || 
            PlayerActionType == E_Player_Action_Type.Sprint)) ||
            (IsViewLocked && PlayerActionType == E_Player_Action_Type.Sprint))
        {
            //������״̬�µ��ƶ����̲Ż�ʹ�ñ������β�ֵ����ת���﷽����������ͬ
            //Ĭ��ֻʹ��ǰ���Ķ���
            transform.rotation =
                Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(MoveDir)
                , TransformRotateRate * Time.timeScale);
        }
        else if(IsViewLocked && PlayerActionType != E_Player_Action_Type.Lock
            && PlayerActionType != E_Player_Action_Type.InputUnlock
            && PlayerActionType != E_Player_Action_Type.Sprint)
        {
            //����״̬������ʼ�տ���Ŀ������
            //���������������������ڳ��
            //��Ҫʹ�û���������������￴��Ŀ�겻�䣬�������ƶ������Ǳ��
            Vector3 lookdir = Vector3.ProjectOnPlane(
                TargetLocked.position - transform.position,
                Vector3.up);
            transform.rotation = Quaternion.LookRotation(lookdir);
        }
        else if(PlayerActionType == E_Player_Action_Type.Lock || 
            PlayerActionType == E_Player_Action_Type.InputUnlock)
        {
            //����ģ�ͳ���Ӧ����λ�Ʒ�����ͬ
            transform.rotation = Quaternion.LookRotation(MoveDir);
        }

        //print("����λ�Ʒ���"+MoveDir);

        float SpeedChangeRate = 0.05f; //�ٶȱ仯�Ĳ�ֵ���ʣ����ٶȹ��ȸ�ƽ�����ﵽ����Ч��
        float MoveRate = 1.0f;
        switch (PlayerActionType)
        {
            case E_Player_Action_Type.Idle:
                MoveRate = Mathf.Lerp(LastSpeed, 0, SpeedChangeRate);
                break;
            case E_Player_Action_Type.Walk:
                MoveRate = Mathf.Lerp(LastSpeed, WalkSpeedRate, SpeedChangeRate);
                break;
            case E_Player_Action_Type.Run:
                MoveRate = Mathf.Lerp(LastSpeed, RunSpeedRate, SpeedChangeRate);
                break;
            case E_Player_Action_Type.Sprint:
                MoveRate = Mathf.Lerp(LastSpeed, SprintSpeedRate, SpeedChangeRate);
                break;
            case E_Player_Action_Type.Lock:
                MoveRate = Mathf.Lerp(LastSpeed, 0, LockSpeedDecreaseRate);
                //print(MoveRate);
                break;
            case E_Player_Action_Type.InputUnlock:
                MoveRate = Mathf.Lerp(LastSpeed, 0, LockSpeedDecreaseRate);
                //print(MoveRate);
                break;
        }
        LastSpeed = MoveRate;
        
        
        characterController.SimpleMove(MoveDir * BaseSpeed * MoveRate);
    }

    /// <summary>
    /// �Խ�ɫ�Ƿ��ڵ���������߼�⣬���㻬�����䲻������
    /// </summary>
    private void Fall()
    {
        bool isGrounded = false;
        //ʹ���Դ�����μ��
        if (Physics.CheckSphere(transform.position, 0.5f,
            1 << LayerMask.NameToLayer("Ground")))
        {
            isGrounded = true;
        }

        if (isGrounded && animator.GetBool("Fall"))
        {
            animator.SetBool("Fall", false);
            if(LastLandHeight - transform.position.y > MaxCommonLandingHeight)
            {
                animator.SetTrigger("HeavyFall");
                print("���������");
            }
            else
            {
                PlayerActionType = E_Player_Action_Type.Idle;
            }
        }
        else if(!isGrounded && !animator.GetBool("Fall"))
        {
            PlayerActionType = E_Player_Action_Type.Lock;
            animator.SetBool("Fall", true);
            LastLandHeight = transform.position.y;
        }
    }

    private void Move()
    {
        animator.SetFloat("WalkY", InputController.Instance.verticalDir);
        animator.SetFloat("WalkX", InputController.Instance.horizontalDir);
            
        //���������ƶ�����
        if (InputController.Instance.verticalDirRaw != 0 
            || InputController.Instance.horizontalDirRaw != 0)
        {
            //�ж��Ƿ�Ϊ����
            if (InputController.Instance.LeftShift)
            {
                PlayerActionType = E_Player_Action_Type.Sprint;
                animator.SetBool("Run", true);
            }
            else
            {
                PlayerActionType = E_Player_Action_Type.Run;
                animator.SetBool("Run", false);
            }
        }
        else
        {
            animator.SetBool("Run", false);
            PlayerActionType = E_Player_Action_Type.Idle;
        }
    }

    private void Dodge()
    {
        //���ö����ͽ���������ͨλ��
        if (InputController.Instance.verticalDirRaw != 0 
            || InputController.Instance.horizontalDirRaw != 0)
        {
            //���㷭���ķ��򣬺������ƶ����㷨��ͬ
            Vector3 cameraForwardProj =
            Vector3.ProjectOnPlane(ViewPos.forward, Vector3.up);
            BodyLockDir = Quaternion.AngleAxis(
                RotationDir[InputController.Instance.verticalDirRaw]
                [InputController.Instance.horizontalDirRaw]
                , Vector3.up) * cameraForwardProj;
            print("�����");
            animator.SetTrigger("Dodge");
        }
    }

    private void LeftHandAttack()
    {
        if (InputController.Instance.LeftHandPress)
        {
            print("������ַ���");
        }
        else if(InputController.Instance.LeftAltPress &&
            InputController.Instance.LeftHandDown)
        {
            //��������
            BodyLockDir = transform.forward;
            print("��������ع���");
            animator.SetTrigger("LeftHandHeavyAttack");
        }
        else if(InputController.Instance.LeftHandDown)
        {
            //��������
            BodyLockDir = transform.forward;
            print("������ֹ���");
            animator.SetTrigger("LeftHandAttack");
        }
    }

    private void RightHandAttack()
    {
        if (InputController.Instance.RightHandPress && 
            animator.GetBool("RightHandDefend"))
        {
            //ֻ�е����ֳ�������ʱ���ܷ���
            print("������ַ���");
        }
        else if (InputController.Instance.LeftAltPress &&
            InputController.Instance.RightHandDown)
        {
            //��������
            BodyLockDir = transform.forward;
            print("��������ع���");
            animator.SetTrigger("RightHandHeavyAttack");
        }
        else if(InputController.Instance.RightHandDown)
        {
            //��������
            BodyLockDir = transform.forward;
            print("������ֹ���");
            animator.SetTrigger("RightHandAttack");
        }
    }

    /// <summary>
    /// ������һ������ʱ�������������������Ԥ����
    /// </summary>
    private void ResetAllTrigger()
    {
        //print("������д�����");
        animator.ResetTrigger("Dodge");
        animator.ResetTrigger("LeftHandAttack");
        animator.ResetTrigger("LeftHandHeavyAttack");
        animator.ResetTrigger("RightHandAttack");
        animator.ResetTrigger("RightHandHeavyAttack");
    }

    void Update()
    {
        //print("��һ֡��ʼ");
        InputController.Instance.UpdateInputState();

        //ÿ֡�ȸ����ӽ������߼�
        LockTarget();

        //���������
        AutoUpdateCamera();

        //�ж������Ƿ�����
        Fall();

        //�������﷽��
        //���㵱ǰ���ﳯ������������ļн�(������)
        

        //ֻ�е����ﲻ������ʱ���ſɽ�������λ��
        if (PlayerActionType != E_Player_Action_Type.Lock
            && PlayerActionType != E_Player_Action_Type.InputUnlock)
        {
            Move();
        }

        //���﷭��
        if (InputController.Instance.SpaceDown
            && PlayerActionType != E_Player_Action_Type.Lock
            && InputController.Instance.IsInputGetMove)
        {
            Dodge();
        }

        //��ҹ����ͷ������������ֺ�����Ӧ���Ƿ����
        if (PlayerActionType != E_Player_Action_Type.Lock)
        {
            LeftHandAttack();
        }
        if (PlayerActionType != E_Player_Action_Type.Lock)
        {
            RightHandAttack();
        }

        //����λ��
        BodyMove();
    }

    /// <summary>
    /// �����Ļص��������������ñ�������״̬ʱ��λ�ƺͷ����Լ���������
    /// </summary>
    /// <param name="MoveLockSpeed">��������λ���ٶ�</param>
    public void StartBodyLock(float MoveLockSpeed)
    {
        //��������
        PlayerActionType = E_Player_Action_Type.Lock;
        //�����������
        ResetAllTrigger();
        //�����ٶ�
        print("�������������������ٶ�Ϊ" + MoveLockSpeed);
        LockSpeedDecreaseRate = 0.015f; //Ĭ����˥��
        LastSpeed = MoveLockSpeed; //�����ٶ�˲��仯
    }

    /// <summary>
    /// �����Ļص��������ڿ��������⣬����Ƿ���������������
    /// </summary>
    public void OpenInput()
    {
        PlayerActionType = E_Player_Action_Type.InputUnlock;
    }

    /// <summary>
    /// �����Ļص����������ڹرն����������⣬��ֹ��������
    /// </summary>
    public void EndInput()
    {
        PlayerActionType = E_Player_Action_Type.Lock;
    }

    /// <summary>
    /// ����λ��˥��
    /// </summary>
    /// <param name="DecreaseRate"></param>
    public void StartLockSpeedDecrease(float DecreaseRate)
    {
        print("ǰҡ˥����ʼ");
        LockSpeedDecreaseRate = DecreaseRate;
    }

    /// <summary>
    /// �����Ļص����������ڽ���״̬��λ������
    /// </summary>
    public void EndBodyLock()
    {
        print("��ҡ����");
        LastSpeed = 0;
        PlayerActionType = E_Player_Action_Type.Idle;
    }
}
