using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ŀ������������
/// ��������Ŀ������Ҿ�������
/// </summary>
public class ViewLockTargetMgr
{
    private static ViewLockTargetMgr instance;
    public static ViewLockTargetMgr Instance
    {
        get
        {
            if (instance == null)
                instance = new ViewLockTargetMgr();
            return instance;
        }
    }
    private ViewLockTargetMgr() {}

    private float ComputeDist(Transform target)
    {
        return Vector3.Distance(target.position,
            PlayerController.Instance.ViewPos.position);
    }

    /// <summary>
    /// ��ȡ�������Ŀ��������current����һ��Ŀ��
    /// </summary>
    /// <param name="colliders"></param>
    /// <returns></returns>
    public Transform GetNextTarget(Collider[] colliders, Transform current = null)
    {
        float refDist;
        Transform result = null;
        float resultDist = float.MaxValue;

        float MinDist = float.MaxValue;
        Transform MinResult = null;
        if (current == null)
            refDist = 0.0f;
        else
            refDist = ComputeDist(current);

        foreach (Collider c in colliders)
        {
            if (c.transform == current || 
                Physics.Linecast(PlayerController.Instance.ViewPos.position,
                c.transform.position, 1 << LayerMask.NameToLayer("Maskable")))
                //�����ǰ����������current��
                //������߼��֮���п��ڵ����ߵ��ڵ��������������
                continue;
            
            float newDist = ComputeDist(c.transform);

            //Ѱ����������������
            if (newDist < MinDist) 
            {
                MinDist = newDist;
                MinResult = c.transform;
            }

            //���c��������Ҿ�����ڵ�ǰ���������壬��С��֮ǰѡ������ľ��룬��ѡ���������
            if (newDist >= refDist && newDist < resultDist)
            {
                resultDist = newDist;
                result = c.transform;
            }
        }
        //���û����һ������(��ǰ�����Ѿ�����Զ�ģ��򷵻ص�һ������)
        //�����Ұ��û�����壬�򷵻ؿ�
        return result == null ? MinResult : result;
    }

    /// <summary>
    /// ��ȡ�������Ŀ�����С��current����һ��Ŀ��
    /// </summary>
    /// <param name="colliders"></param>
    /// <returns></returns>
    public Transform GetLastTarget(Collider[] colliders, Transform current = null)
    {
        float refDist;
        Transform result = null;
        float resultDist = 0;

        float MaxDist = 0;
        Transform MaxResult = null;
        if (current == null)
            refDist = float.MaxValue;
        else
            refDist = ComputeDist(current);

        foreach (Collider c in colliders)
        {
            if (c.transform == current ||
                Physics.Linecast(PlayerController.Instance.ViewPos.position,
                c.transform.position, 1 << LayerMask.NameToLayer("Maskable")))
                //�����ǰ����������current��
                //������߼��֮���п��ڵ����ߵ��ڵ��������������
                continue;

            float newDist = ComputeDist(c.transform);

            //Ѱ���������Զ������
            if (newDist > MaxDist)
            {
                MaxDist = newDist;
                MaxResult = c.transform;
            }

            //���c��������Ҿ���С�ڵ�ǰ���������壬�Ҵ���֮ǰѡ������ľ��룬��ѡ���������
            if (newDist <= refDist && newDist > resultDist)
            {
                resultDist = newDist;
                result = c.transform;
            }
        }
        //���û����һ������(��ǰ�����Ѿ�����Զ�ģ��򷵻ص�һ������)
        //�����Ұ��û�����壬�򷵻ؿ�
        return result == null ? MaxResult : result;
    }
}
