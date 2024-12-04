using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 目标锁定管理器
/// 根据锁定目标与玩家距离排序
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
    /// 获取距离玩家目标距离大于current的下一个目标
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
                //如果当前这个物体就是current或
                //如果射线检测之间有可遮挡视线的遮挡物，则跳过该物体
                continue;
            
            float newDist = ComputeDist(c.transform);

            //寻找离玩家最近的物体
            if (newDist < MinDist) 
            {
                MinDist = newDist;
                MinResult = c.transform;
            }

            //如果c物体利玩家距离大于当前锁定的物体，且小于之前选择物体的距离，则选择这个物体
            if (newDist >= refDist && newDist < resultDist)
            {
                resultDist = newDist;
                result = c.transform;
            }
        }
        //如果没有下一个物体(当前物体已经是最远的，则返回第一个物体)
        //如果视野中没有物体，则返回空
        return result == null ? MinResult : result;
    }

    /// <summary>
    /// 获取距离玩家目标距离小于current的下一个目标
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
                //如果当前这个物体就是current或
                //如果射线检测之间有可遮挡视线的遮挡物，则跳过该物体
                continue;

            float newDist = ComputeDist(c.transform);

            //寻找离玩家最远的物体
            if (newDist > MaxDist)
            {
                MaxDist = newDist;
                MaxResult = c.transform;
            }

            //如果c物体利玩家距离小于当前锁定的物体，且大于之前选择物体的距离，则选择这个物体
            if (newDist <= refDist && newDist > resultDist)
            {
                resultDist = newDist;
                result = c.transform;
            }
        }
        //如果没有下一个物体(当前物体已经是最远的，则返回第一个物体)
        //如果视野中没有物体，则返回空
        return result == null ? MaxResult : result;
    }
}
