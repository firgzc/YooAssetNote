using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using YooAsset;

/// <summary>
/// 创建文件下载器
/// </summary>
public class FsmCreatePackageDownloader : IStateNode
{
    private StateMachine _machine;

    void IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }
    void IStateNode.OnEnter()
    {
        Debug.Log($"<color=yellow>创建补丁下载器 !!!！</color>");
        PatchEventDefine.PatchStatesChange.SendEventMessage("创建补丁下载器！");
        GameManager.Instance.StartCoroutine(CreateDownloader());
    }
    void IStateNode.OnUpdate()
    {
        Debug.Log($"<color=yellow>当前的处在的节点状态: {_machine.CurrentNode} </color>");
    }
    void IStateNode.OnExit()
    {
        Debug.Log($"<color=yellow>退出当前节点状态: {_machine.CurrentNode} </color>");
    }

    IEnumerator CreateDownloader()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        var packageName = (string)_machine.GetBlackboardValue("PackageName");
        var package = YooAssets.GetPackage(packageName);
        int downloadingMaxNum = 10;
        int failedTryAgain = 3;
        var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
        _machine.SetBlackboardValue("Downloader", downloader);

        if (downloader.TotalDownloadCount == 0)
        {
            Debug.Log("Not found any download files !");
            _machine.ChangeState<FsmUpdaterDone>();
        }
        else
        {
            // 发现新更新文件后，挂起流程系统
            // 注意：开发者需要在下载前检测磁盘空间不足
            //todo：此处需要加上当前系统磁盘空间大小
            //todo: PC平台(Windows、macOS) --> 先检测当前程序的磁盘空间目录是否足够
            //todo: Android/iOS平台 --> 检测当前手机存储空间
            //todo: webgl平台 --> 先检测当前程序的磁盘空间目录是否足够
            //todo：若空间均不够时对话框提示当前的空间不足，并点击对话框后结束当前程序
            //todo: 先判断空间是否足够，足够执行下面语句
            int totalDownloadCount = downloader.TotalDownloadCount;
            long totalDownloadBytes = downloader.TotalDownloadBytes;
            PatchEventDefine.FoundUpdateFiles.SendEventMessage(totalDownloadCount, totalDownloadBytes);

            //todo: 空间不足时，发送空间检测的事件
            //xxx.xxx.SendEventMessage(xx, xx);
        }
    }
}