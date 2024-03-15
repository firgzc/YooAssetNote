using System.Collections;
using UnityEngine;
using UniFramework.Machine;
using YooAsset;

/// <summary>
/// 下载更新文件
/// </summary>
public class FsmDownloadPackageFiles : IStateNode
{
    private StateMachine _machine;

    void IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }
    void IStateNode.OnEnter()
    {
        Debug.Log($"<color=yellow>开始下载补丁文件 !!!!！</color>");
        PatchEventDefine.PatchStatesChange.SendEventMessage("开始下载补丁文件！");
        GameManager.Instance.StartCoroutine(BeginDownload());
    }
    void IStateNode.OnUpdate()
    {
        Debug.Log($"<color=yellow>当前的处在的节点状态: {_machine.CurrentNode} </color>");
    }
    void IStateNode.OnExit()
    {
        Debug.Log($"<color=yellow>退出当前节点状态: {_machine.CurrentNode} </color>");
    }

    private IEnumerator BeginDownload()
    {
        var downloader = (ResourceDownloaderOperation)_machine.GetBlackboardValue("Downloader");
        downloader.OnDownloadErrorCallback = PatchEventDefine.WebFileDownloadFailed.SendEventMessage;
        downloader.OnDownloadProgressCallback = PatchEventDefine.DownloadProgressUpdate.SendEventMessage;
        downloader.BeginDownload();
        yield return downloader;

        // 检测下载结果
        if (downloader.Status != EOperationStatus.Succeed)
            yield break;

        _machine.ChangeState<FsmDownloadPackageOver>();
    }
}