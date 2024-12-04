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
/// 人物移动及其动画播放控制器
/// </summary>
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    public float testRaid;
    public float maxDistance;

    [Header("锁定视角相关(长宽高)")]
    public Vector3 ViewBoxRange;
    public float ViewBoxDist = 5.0f;

    [Header("最大锁定距离")]
    public float MaxLockDist = 100.0f;

    [Header("摄像机相关")]
    public float CameraRotateSpeed = 1.0f;
    public float DefaultCameraDist = 3.0f;
    public float CameraFollowRate = 0.05f;
    public float CameraFollowRotateRate = 0.05f;
    public Transform ViewPos;
    public Transform ViewTarget;

    [Header("人物相关")]
    public float TransformRotateRate = 1.0f;
    public float BaseSpeed = 1.0f; //基础移动速度，基于Run的速度计算
    public float RunSpeedRate = 1.0f;
    public float SprintSpeedRate = 2.0f; //冲刺速度比率
    public float WalkSpeedRate = 0.5f;
    public float MaxCommonLandingHeight = 2.0f; //最大落地不受影响高度

    //位移旋转相关
    //存储对应按键要使用的旋转角度,目标方向向量就是摄像机的朝向旋转以下的角度
    //第一个键是Z轴方向，第二个键是X轴方向
    private Dictionary<int, Dictionary<int, float>> RotationDir;
    private CharacterController characterController;
    private float LastSpeed; //记录位移衰减上一帧位移的速度
    private Vector3 MoveDir; //记录当前帧应当位移的方向

    //人物动画相关
    private Animator animator;
    private string CurrentLeftHandAnimatorLayer;
    private string CurrentRightHandAnimatorLayer;

    //视角锁定相关
    private bool IsViewLocked = false;
    private Transform TargetLocked;

    //摄像机自动避障相关
    private float CameraDist;

    //动作锁定状态(前后摇，发生时人物无法通过控制取消动作)
    private E_Player_Action_Type PlayerActionType;
    private Vector3 BodyLockDir; //发生锁定时，人物位移要锁定的方向
    private float LockSpeedDecreaseRate; //该动作是否开启速度衰减
    private float LastLandHeight; //记录落下前的高度

    //装备相关
    private AttackableBody attackBody;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        //获取各个组件
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        attackBody = GetComponent<AttackableBody>();

        //设置人物动画状态机初始层级
        CurrentLeftHandAnimatorLayer = "UnarmedLeftHand";
        CurrentRightHandAnimatorLayer = "UnarmedRightHand";
        StartCoroutine(ChangeHandAnimateLayer(CurrentLeftHandAnimatorLayer, false));
        StartCoroutine(ChangeHandAnimateLayer(CurrentRightHandAnimatorLayer, true));

        //测试剑层级
        attackBody.EquipWeapon(true, E_Type_PlayerEquipmentType.Sword);

        //重置位移速度
        LastSpeed = 0;

        //重置摄像机位置和旋转
        var newPos = ViewPos.position;
        newPos.z -= CameraDist;
        Camera.main.transform.position = newPos;
        CameraDist = DefaultCameraDist;

        //设置初始的旋转变量
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
    /// 向外部提供切换人物动画层级的方法
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
            //如果目标物体在一帧中瞬间移出摄像机(疯狗速度很快)
            //或距离玩家过远，(跑路了)
            //或目标物体和玩家之间有遮挡(藏起来了)
            //则丢失锁定目标
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
            //创建视野检测碰撞盒
            Quaternion boxRotation = Camera.main.transform.rotation;
            Vector3 boxCenter = Camera.main.transform.position
                + Camera.main.transform.forward * (CameraDist + ViewBoxDist);
            Collider[] enemys = Physics.OverlapBox(boxCenter, ViewBoxRange, boxRotation
                , 1 << LayerMask.NameToLayer("Enemy"));

            //计算获取下一个目标
            TargetLocked = ViewLockTargetMgr.Instance.GetNextTarget(enemys);
            if (TargetLocked != null)
            {
                IsViewLocked = true;
                //更换动画层级
                StartCoroutine(UpdateTargetLockAnimateLayer());
            }
        }
        else if (IsViewLocked && InputController.Instance.MouseScroll != 0)
        {
            //创建视野检测碰撞盒
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

            //如果获取不到目标，就解除视角锁定状态
            if (TargetLocked == null)
            {
                UnlockTarget();
            }
        }
        else if (InputController.Instance.TabDown && IsViewLocked)
        {
            UnlockTarget();
        }

        //设置目标锁定点UI元素
        if (IsViewLocked)
        {
            OnGameUIManager.Instance.GetPanel<CombatPanel>().ShowTargetKnob(TargetLocked.transform);

        }
    }

    private void UnlockTarget()
    {
        print("解除锁定");
        IsViewLocked = false;
        TargetLocked = null;
        OnGameUIManager.Instance.GetPanel<CombatPanel>().HideTargetKnob();
        StartCoroutine(UpdateTargetLockAnimateLayer());
    }

    /// <summary>
    /// 自动更新摄像机的逻辑
    /// 在人物眼睛处摆放一个ViewPos，旋转就是让这个ViewPos旋转
    /// ViewPos不能跟随人物的旋转而旋转，因此ViewPos不能是人物的子物体
    /// 不断更新ViewPos的坐标等于ViewTarget的坐标
    /// 定义摄像机与ViewPos的距离Dist，
    /// 则摄像机永远会出现在ViewPos正后方Dist处并看向ViewPos
    /// </summary>
    private void AutoUpdateCamera()
    {
        RaycastHit hitInfo;

        //射线检测玩家是否被遮挡
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
            //通知UI绘制锁定点
            ViewPos.LookAt(TargetLocked.position);
        }
        else
        {
            //控制摄像机看向目标的旋转
            float HorizontalCamRotate =
                Input.GetAxis("Mouse X") * CameraRotateSpeed * Time.deltaTime;
            float VerticalCamRotate = //向上看减小夹角(负)，向下看增大夹角(正)
                Input.GetAxis("Mouse Y") * CameraRotateSpeed * Time.deltaTime * -1;

            //水平旋转一定是绕世界的Z轴
            ViewPos.Rotate(Vector3.up
                , HorizontalCamRotate, Space.World);

            //垂直旋转一定是绕看向方向的local X轴在水平面上的投影
            Vector3 refVerticalAxis = Vector3.ProjectOnPlane(ViewPos.right, Vector3.up);

            //限制摄像机的水平面与世界水平面夹角不能超过60度
            //直接计算两个平面法向量的夹角
            //参考轴就是垂直旋转的轴，因为上下是反向的，所以*-1
            float verticalAngle = -1 *
                Vector3.SignedAngle(ViewPos.up, Vector3.up, refVerticalAxis);
            if (Mathf.Abs(verticalAngle + VerticalCamRotate) < 60)
                ViewPos.Rotate(refVerticalAxis, VerticalCamRotate, Space.World);
        }


        //插值更新摄像机的新位置并实时更新摄像机旋转，达到平滑过度效果
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position,
            ViewPos.position - ViewPos.forward * CameraDist, CameraFollowRate);

        Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation,
            ViewPos.rotation, CameraFollowRotateRate);
    }

    /// <summary>
    /// 该函数统一处理这一帧人物最终的位移
    /// 函数会根据奔跑来消耗耐力
    /// </summary>
    private void BodyMove()
    {
        //计算位移方向
        if (PlayerActionType != E_Player_Action_Type.Lock
            && PlayerActionType != E_Player_Action_Type.InputUnlock)
        {
            //只有人物处于可移动状态下才进行计算
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

        //判断角色模型朝向
        if ((!IsViewLocked && (PlayerActionType == E_Player_Action_Type.Run || 
            PlayerActionType == E_Player_Action_Type.Sprint)) ||
            (IsViewLocked && PlayerActionType == E_Player_Action_Type.Sprint))
        {
            //非锁定状态下的移动或冲刺才会使用比例球形插值来旋转人物方向与输入相同
            //默认只使用前进的动画
            transform.rotation =
                Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(MoveDir)
                , TransformRotateRate * Time.timeScale);
        }
        else if(IsViewLocked && PlayerActionType != E_Player_Action_Type.Lock
            && PlayerActionType != E_Player_Action_Type.InputUnlock
            && PlayerActionType != E_Player_Action_Type.Sprint)
        {
            //锁定状态下人物始终看向目标物体
            //除非人物在其他动作或在冲刺
            //需要使用混合树动画控制人物看向目标不变，但身体移动动画是变的
            Vector3 lookdir = Vector3.ProjectOnPlane(
                TargetLocked.position - transform.position,
                Vector3.up);
            transform.rotation = Quaternion.LookRotation(lookdir);
        }
        else if(PlayerActionType == E_Player_Action_Type.Lock || 
            PlayerActionType == E_Player_Action_Type.InputUnlock)
        {
            //人物模型朝向应当和位移方向相同
            transform.rotation = Quaternion.LookRotation(MoveDir);
        }

        //print("最终位移方向"+MoveDir);

        float SpeedChangeRate = 0.05f; //速度变化的插值比率，让速度过度更平滑，达到惯性效果
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
    /// 对角色是否在地面进行射线检测，方便滑坡下落不会误判
    /// </summary>
    private void Fall()
    {
        bool isGrounded = false;
        //使用稍大的球形检测
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
                print("出发重落地");
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
            
        //更新人物移动动画
        if (InputController.Instance.verticalDirRaw != 0 
            || InputController.Instance.horizontalDirRaw != 0)
        {
            //判断是否为奔跑
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
        //设置动画和禁用其他普通位移
        if (InputController.Instance.verticalDirRaw != 0 
            || InputController.Instance.horizontalDirRaw != 0)
        {
            //计算翻滚的方向，和正常移动的算法相同
            Vector3 cameraForwardProj =
            Vector3.ProjectOnPlane(ViewPos.forward, Vector3.up);
            BodyLockDir = Quaternion.AngleAxis(
                RotationDir[InputController.Instance.verticalDirRaw]
                [InputController.Instance.horizontalDirRaw]
                , Vector3.up) * cameraForwardProj;
            print("激活翻滚");
            animator.SetTrigger("Dodge");
        }
    }

    private void LeftHandAttack()
    {
        if (InputController.Instance.LeftHandPress)
        {
            print("玩家左手防御");
        }
        else if(InputController.Instance.LeftAltPress &&
            InputController.Instance.LeftHandDown)
        {
            //方向修正
            BodyLockDir = transform.forward;
            print("玩家左手重攻击");
            animator.SetTrigger("LeftHandHeavyAttack");
        }
        else if(InputController.Instance.LeftHandDown)
        {
            //方向修正
            BodyLockDir = transform.forward;
            print("玩家左手攻击");
            animator.SetTrigger("LeftHandAttack");
        }
    }

    private void RightHandAttack()
    {
        if (InputController.Instance.RightHandPress && 
            animator.GetBool("RightHandDefend"))
        {
            //只有当左手持有武器时才能防御
            print("玩家右手防御");
        }
        else if (InputController.Instance.LeftAltPress &&
            InputController.Instance.RightHandDown)
        {
            //方向修正
            BodyLockDir = transform.forward;
            print("玩家右手重攻击");
            animator.SetTrigger("RightHandHeavyAttack");
        }
        else if(InputController.Instance.RightHandDown)
        {
            //方向修正
            BodyLockDir = transform.forward;
            print("玩家右手攻击");
            animator.SetTrigger("RightHandAttack");
        }
    }

    /// <summary>
    /// 进入下一个动作时，清空所有其他动作的预输入
    /// </summary>
    private void ResetAllTrigger()
    {
        //print("清空所有触发器");
        animator.ResetTrigger("Dodge");
        animator.ResetTrigger("LeftHandAttack");
        animator.ResetTrigger("LeftHandHeavyAttack");
        animator.ResetTrigger("RightHandAttack");
        animator.ResetTrigger("RightHandHeavyAttack");
    }

    void Update()
    {
        //print("新一帧开始");
        InputController.Instance.UpdateInputState();

        //每帧先更新视角锁定逻辑
        LockTarget();

        //更新摄像机
        AutoUpdateCamera();

        //判断人物是否落下
        Fall();

        //计算人物方向
        //计算当前人物朝向和摄像机朝向的夹角(右正左负)
        

        //只有当人物不被锁定时，才可接受输入位移
        if (PlayerActionType != E_Player_Action_Type.Lock
            && PlayerActionType != E_Player_Action_Type.InputUnlock)
        {
            Move();
        }

        //人物翻滚
        if (InputController.Instance.SpaceDown
            && PlayerActionType != E_Player_Action_Type.Lock
            && InputController.Instance.IsInputGetMove)
        {
            Dodge();
        }

        //玩家攻击和防御动作，左手和右手应当是分离的
        if (PlayerActionType != E_Player_Action_Type.Lock)
        {
            LeftHandAttack();
        }
        if (PlayerActionType != E_Player_Action_Type.Lock)
        {
            RightHandAttack();
        }

        //人物位移
        BodyMove();
    }

    /// <summary>
    /// 动画的回调函数，用于设置本次锁定状态时的位移和方向，以及锁定输入
    /// </summary>
    /// <param name="MoveLockSpeed">本次锁定位移速度</param>
    public void StartBodyLock(float MoveLockSpeed)
    {
        //锁定动作
        PlayerActionType = E_Player_Action_Type.Lock;
        //清空其他动作
        ResetAllTrigger();
        //设置速度
        print("动作锁定，动作设置速度为" + MoveLockSpeed);
        LockSpeedDecreaseRate = 0.015f; //默认有衰减
        LastSpeed = MoveLockSpeed; //设置速度瞬间变化
    }

    /// <summary>
    /// 动画的回调函数用于开启输入检测，检测是否有连续动作输入
    /// </summary>
    public void OpenInput()
    {
        PlayerActionType = E_Player_Action_Type.InputUnlock;
    }

    /// <summary>
    /// 动画的回调函数，用于关闭动作的输入检测，禁止动作连段
    /// </summary>
    public void EndInput()
    {
        PlayerActionType = E_Player_Action_Type.Lock;
    }

    /// <summary>
    /// 开启位移衰减
    /// </summary>
    /// <param name="DecreaseRate"></param>
    public void StartLockSpeedDecrease(float DecreaseRate)
    {
        print("前摇衰减开始");
        LockSpeedDecreaseRate = DecreaseRate;
    }

    /// <summary>
    /// 动画的回调函数，用于结束状态和位移锁定
    /// </summary>
    public void EndBodyLock()
    {
        print("后摇结束");
        LastSpeed = 0;
        PlayerActionType = E_Player_Action_Type.Idle;
    }
}
