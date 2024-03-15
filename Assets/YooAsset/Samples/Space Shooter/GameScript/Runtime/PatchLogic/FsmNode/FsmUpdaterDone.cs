using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;

/// <summary>
/// 流程更新完毕
/// </summary>
internal class FsmUpdaterDone : IStateNode
{
    void IStateNode.OnCreate(StateMachine machine)
    {
        //todo:此处用于创建状态机
    }
    void IStateNode.OnEnter()
    {
        Debug.Log($"<color=yellow>流程更新完毕 !!!！</color>");
    }
    void IStateNode.OnUpdate()
    {
        Debug.Log($"<color=yellow>当前没有节点 </color>");
    }
    void IStateNode.OnExit()
    {
        Debug.Log($"<color=yellow>当前没有节点 </color>");
    }
}