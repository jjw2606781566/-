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

    public float PressDelaySecond = 0.3f;//��ѹ�ӳٶ�òŻ����

    //�ٿذ���
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

    //����ٿذ���
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
    /// �ж��Ƿ������λ������
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
    /// �����������룬����UI�رպ���
    /// </summary>
    public void OpenInput()
    {
        isInputLocked = false;
    }

    /// <summary>
    /// �رն������룬����UI�������������룬��ֹ������
    /// </summary>
    public void CloseInput()
    {
        StopAllCoroutines();
        //�������а���
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
    /// ������֡��������
    /// </summary>
    public void UpdateInputState()
    {
        //����״̬�²������������
        if (isInputLocked) 
        {
            return;
        }

        //�����������
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

        //���յ�����Ϳ�ʼ�ж�
        if (LeftHandPress) //һ�������˰�ѹ״̬���͵����̧��ʱ�ٽ���
        {
            LeftHandPress = Input.GetMouseButton(0);
        }
        else if (Input.GetMouseButton(0) && LeftHandDelayCortine == null)
        {
            LeftHandDelayCortine = 
                StartCoroutine(DelayMouseButtonPress(E_Input_Delay_Type.MouseLeft));
        }
        else if(LeftHandDelayCortine == null) //�������ڼ��Я��ʱ���ŻὫ���������
        {
            LeftHandDown = false;
            LeftHandPress = false;
        }

        if (RightHandPress) //һ�������˰�ѹ״̬���͵����̧��ʱ�ٽ���
        {
            RightHandPress = Input.GetMouseButton(1);
        }
        else if (Input.GetMouseButton(1) && RightHandDelayCortine == null)
        {
            RightHandDelayCortine =
                StartCoroutine(DelayMouseButtonPress(E_Input_Delay_Type.MouseRight));
        }
        else if(RightHandDelayCortine == null) //�������ڼ��Я��ʱ���ŻὫ���������
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
    /// �ӳٶ�òŻ�����һ֡
    /// </summary>
    /// <param name="DelaySecond"></param>
    /// <returns></returns>
    private IEnumerator DelayMouseButtonPress(E_Input_Delay_Type type)
    {
        //print("��ⳤ����ʼ");
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
                yield return null; //�ӳ�һ֡���ȴ���Ҵ�������
                LeftHandDelayCortine = null;
                break;
            case E_Input_Delay_Type.MouseRight:
                if (Input.GetMouseButton(1))
                {
                    //print("����Ҽ�����");
                    RightHandDown = false;
                    RightHandPress = true;
                }
                else
                {
                    //print("����Ҽ����");
                    RightHandDown = true;
                    RightHandPress = false;
                }
                yield return null; //�ӳ�һ֡���ȴ���Ҵ�������
                RightHandDelayCortine = null;
                break;
        }
        //print("��ⳤ������");
    } 
}
