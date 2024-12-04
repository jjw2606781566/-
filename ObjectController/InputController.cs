using System.Collections;
using UnityEngine;

enum E_Input_Delay_Type
{
    MouseLeft,
    MouseRight
}

public class InputController : MonoBehaviour
{
    public static InputController Instance;

    public float PressDelaySecond = 0.3f;//按压延迟多久才会决定

    //操控按键
    [HideInInspector]
    public bool TabDown;
    [HideInInspector]
    public bool LeftShift;
    [HideInInspector]
    public int horizontalDirRaw;
    [HideInInspector]
    public int verticalDirRaw;
    [HideInInspector]
    public float horizontalDir;
    [HideInInspector]
    public float verticalDir;
    [HideInInspector]
    public bool LeftCtrlPress;
    [HideInInspector]
    public bool LeftAltPress;
    [HideInInspector]
    public bool SpaceDown;
    [HideInInspector]
    public float MouseScroll;

    //特殊操控按键
    [HideInInspector]
    public bool LeftHandDown;
    [HideInInspector]
    public bool LeftHandPress;
    [HideInInspector]
    public bool RightHandDown;
    [HideInInspector]
    public bool RightHandPress;

    private Coroutine LeftHandDelayCortine;
    private Coroutine RightHandDelayCortine;
    private bool isInputLocked = false;

    /// <summary>
    /// 判断是否接受了位移输入
    /// </summary>
    public bool IsInputGetMove
    {
        get
        {
            if(horizontalDirRaw != 0 || verticalDirRaw != 0)
                return true;
            else
                return false;
        }
    }

    private void Awake()
    {
        Instance = this;

        LeftHandDelayCortine = null;
        RightHandDelayCortine = null;
    }

    /// <summary>
    /// 开启动作输入，用于UI关闭后开启
    /// </summary>
    public void OpenInput()
    {
        isInputLocked = false;
    }

    /// <summary>
    /// 关闭动作输入，用于UI开启后锁定输入，防止动作误触
    /// </summary>
    public void CloseInput()
    {
        StopAllCoroutines();
        //消除所有按键
        MouseScroll = 0;
        TabDown = false;
        LeftShift = false;
        horizontalDirRaw = 0;
        verticalDirRaw = 0;
        horizontalDir = 0;
        verticalDir = 0;
        LeftCtrlPress = false;
        LeftAltPress = false;
        SpaceDown = false;
        LeftHandDown = false;
        LeftHandPress = false;
        RightHandDown = false;
        RightHandPress = false;

        LeftHandDelayCortine = null;
        RightHandDelayCortine = null;

        isInputLocked = true;
    }

    /// <summary>
    /// 更新这帧输入的情况
    /// </summary>
    public void UpdateInputState()
    {
        //锁定状态下不接受玩家输入
        if (isInputLocked) 
        {
            return;
        }

        //接受玩家输入
        MouseScroll = Input.GetAxisRaw("Mouse ScrollWheel");
        TabDown = Input.GetKeyDown(KeyCode.Tab);
        LeftShift = Input.GetKey(KeyCode.LeftShift);
        horizontalDirRaw = (int)Input.GetAxisRaw("Horizontal");
        verticalDirRaw = (int)Input.GetAxisRaw("Vertical");
        horizontalDir = Input.GetAxis("Horizontal");
        verticalDir = Input.GetAxis("Vertical");
        LeftCtrlPress = Input.GetKey(KeyCode.LeftControl);
        LeftAltPress = Input.GetKey(KeyCode.LeftAlt);
        SpaceDown = Input.GetKeyDown(KeyCode.Space);

        //接收到输入就开始判断
        if (LeftHandPress) //一旦进入了按压状态，就等玩家抬起时再结束
        {
            LeftHandPress = Input.GetMouseButton(0);
        }
        else if (Input.GetMouseButton(0) && LeftHandDelayCortine == null)
        {
            LeftHandDelayCortine = 
                StartCoroutine(DelayMouseButtonPress(E_Input_Delay_Type.MouseLeft));
        }
        else if(LeftHandDelayCortine == null) //当不存在检测携程时，才会将两个都变空
        {
            LeftHandDown = false;
            LeftHandPress = false;
        }

        if (RightHandPress) //一旦进入了按压状态，就等玩家抬起时再结束
        {
            RightHandPress = Input.GetMouseButton(1);
        }
        else if (Input.GetMouseButton(1) && RightHandDelayCortine == null)
        {
            RightHandDelayCortine =
                StartCoroutine(DelayMouseButtonPress(E_Input_Delay_Type.MouseRight));
        }
        else if(RightHandDelayCortine == null) //当不存在检测携程时，才会将两个都变空
        {
            RightHandDown = false;
            RightHandPress = false;
        }

        //print($"LeftHandDown{LeftHandDown}");
        //print($"RightHandDown{RightHandDown}");
        //print($"LeftHandPress{LeftHandPress}");
        //print($"RightHandPress{RightHandPress}");
    }

    /// <summary>
    /// 延迟多久才会检测下一帧
    /// </summary>
    /// <param name="DelaySecond"></param>
    /// <returns></returns>
    private IEnumerator DelayMouseButtonPress(E_Input_Delay_Type type)
    {
        //print("检测长按开始");
        yield return new WaitForSecondsRealtime(PressDelaySecond);
        switch (type)
        {
            case E_Input_Delay_Type.MouseLeft:
                if (Input.GetMouseButton(0))
                {
                    LeftHandDown = false;
                    LeftHandPress = true;
                }
                else
                {
                    LeftHandDown = true;
                    LeftHandPress = false;
                }
                yield return null; //延迟一帧，等待玩家处理输入
                LeftHandDelayCortine = null;
                break;
            case E_Input_Delay_Type.MouseRight:
                if (Input.GetMouseButton(1))
                {
                    //print("鼠标右键长按");
                    RightHandDown = false;
                    RightHandPress = true;
                }
                else
                {
                    //print("鼠标右键点击");
                    RightHandDown = true;
                    RightHandPress = false;
                }
                yield return null; //延迟一帧，等待玩家处理输入
                RightHandDelayCortine = null;
                break;
        }
        //print("检测长按结束");
    } 
}
