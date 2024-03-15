using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using YooAsset;

/// <summary>
/// 清理未使用的缓存文件
/// </summary>
internal class FsmClearPackageCache : IStateNode
{
    private StateMachine _machine;

    void IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }
    void IStateNode.OnEnter()
    {
        Debug.Log($"<color=yellow>清理未使用的缓存文件 !!!!!！</color>");
        PatchEventDefine.PatchStatesChange.SendEventMessage("清理未使用的缓存文件！");
        var packageName = (string)_machine.GetBlackboardValue("PackageName");
        var package = YooAssets.GetPackage(packageName);
        var operation = package.ClearUnusedCacheFilesAsync();
        operation.Completed += Operation_Completed;
    }
    void IStateNode.OnUpdate()
    {
        Debug.Log($"<color=yellow>当前的处在的节点状态: {_machine.CurrentNode} </color>");
    }


    void IStateNode.OnExit()
    {
        Debug.Log($"<color=yellow>退出当前节点状态: {_machine.CurrentNode} </color>");
    }


    private void Operation_Completed(YooAsset.AsyncOperationBase obj)
    {
        _machine.ChangeState<FsmUpdaterDone>();
    }
}