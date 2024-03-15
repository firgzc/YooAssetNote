using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using YooAsset;

/// <summary>
/// 更新资源版本号
/// </summary>
internal class FsmUpdatePackageVersion : IStateNode
{
    private StateMachine _machine;

    void IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }
    void IStateNode.OnEnter()
    {
        Debug.Log($"<color=yellow>获取最新的资源版本 !！</color>");
        PatchEventDefine.PatchStatesChange.SendEventMessage("获取最新的资源版本 !");
        GameManager.Instance.StartCoroutine(UpdatePackageVersion());
    }
    void IStateNode.OnUpdate()
    {
        Debug.Log($"<color=yellow>当前的处在的节点状态:  {_machine.CurrentNode}</color>");
    }
    void IStateNode.OnExit()
    {
        Debug.Log($"<color=yellow>退出当前节点状态:  {_machine.CurrentNode}</color>");
    }

    private IEnumerator UpdatePackageVersion()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        var packageName = (string)_machine.GetBlackboardValue("PackageName");
        var package = YooAssets.GetPackage(packageName);
        var operation = package.UpdatePackageVersionAsync(false);
        yield return operation;

        //获取新的版本号后，直接从清单文件中获取对应的版本号
        if (operation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning(operation.Error);
            PatchEventDefine.PackageVersionUpdateFailed.SendEventMessage();
        }
        else
        {
            Debug.Log($"<color=yellow>更新版本： 新的版本号为: {operation.PackageVersion}</color>");
            _machine.SetBlackboardValue("PackageVersion", operation.PackageVersion);
            _machine.ChangeState<FsmUpdatePackageManifest>();
        }
    }
}