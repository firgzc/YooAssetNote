using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;

/// <summary>
/// 下载完毕
/// </summary>
internal class FsmDownloadPackageOver : IStateNode
{
    private StateMachine _machine;

    void IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }
    void IStateNode.OnEnter()
    {
        Debug.Log($"<color=yellow>下载完毕 !!!!!！</color>");
        _machine.ChangeState<FsmClearPackageCache>();
    }

    void IStateNode.OnUpdate()
    {
        Debug.Log($"<color=yellow>当前的处在的节点状态: {_machine.CurrentNode} </color>");
    }

    void IStateNode.OnExit()
    {
        Debug.Log($"<color=yellow>退出当前节点状态: {_machine.CurrentNode} </color>");
    }
}