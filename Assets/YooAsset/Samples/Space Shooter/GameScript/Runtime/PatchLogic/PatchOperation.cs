using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using UniFramework.Event;
using YooAsset;

/// <summary>
/// 补丁包操作逻辑
/// </summary>
public class PatchOperation : GameAsyncOperation
{
    /// <summary>
    ///  操作步骤
    /// </summary>
    private enum ESteps
    {
        None,
        Update,
        Done,
    }

    private readonly EventGroup _eventGroup = new EventGroup();
    private readonly StateMachine _machine;
    private ESteps _steps = ESteps.None;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="packageName">包裹名</param>
    /// <param name="buildPipeline">构建管线</param>
    /// <param name="playMode">运行模式</param>
    public PatchOperation(string packageName, string buildPipeline, EPlayMode playMode)
    {
        // 注册监听事件-- 事件组
        _eventGroup.AddListener<UserEventDefine.UserTryInitialize>(OnHandleEventMessage);
        _eventGroup.AddListener<UserEventDefine.UserBeginDownloadWebFiles>(OnHandleEventMessage);
        _eventGroup.AddListener<UserEventDefine.UserTryUpdatePackageVersion>(OnHandleEventMessage);
        _eventGroup.AddListener<UserEventDefine.UserTryUpdatePatchManifest>(OnHandleEventMessage);
        _eventGroup.AddListener<UserEventDefine.UserTryDownloadWebFiles>(OnHandleEventMessage);

        // 创建状态机
        _machine = new StateMachine(this);
        _machine.AddNode<FsmInitializePackage>();
        _machine.AddNode<FsmUpdatePackageVersion>();
        _machine.AddNode<FsmUpdatePackageManifest>();
        _machine.AddNode<FsmCreatePackageDownloader>();
        _machine.AddNode<FsmDownloadPackageFiles>();
        _machine.AddNode<FsmDownloadPackageOver>();
        _machine.AddNode<FsmClearPackageCache>();
        _machine.AddNode<FsmUpdaterDone>();

        _machine.SetBlackboardValue("PackageName", packageName);
        _machine.SetBlackboardValue("PlayMode", playMode);
        _machine.SetBlackboardValue("BuildPipeline", buildPipeline);
    }
    protected override void OnStart()
    {
        _steps = ESteps.Update;
        _machine.Run<FsmInitializePackage>();
    }
    protected override void OnUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if(_steps == ESteps.Update)
        {
            _machine.Update();
            Debug.Log($"<color=blue>输出当前正在进行的流程:  {_machine.CurrentNode}</color>");
            if(_machine.CurrentNode == typeof(FsmUpdaterDone).FullName)
            {
                Debug.Log($"<color=yellow>完成进行的流程:  {typeof(FsmUpdaterDone).FullName}</color>");

                _eventGroup.RemoveAllListener();
                Status = EOperationStatus.Succeed;
                _steps = ESteps.Done;
                Debug.Log($"<color=yellow>输出当前事件组中的数量:  {_eventGroup.GetListenerNums()}</color>");
            }
        }
    }


    protected override void OnAbort()
    {
    }

    /// <summary>
    /// 接收事件
    /// </summary>
    private void OnHandleEventMessage(IEventMessage message)
    {
        if (message is UserEventDefine.UserTryInitialize)
        {
            _machine.ChangeState<FsmInitializePackage>();
        }
        else if (message is UserEventDefine.UserBeginDownloadWebFiles)
        {
            _machine.ChangeState<FsmDownloadPackageFiles>();
        }
        else if (message is UserEventDefine.UserTryUpdatePackageVersion)
        {
            _machine.ChangeState<FsmUpdatePackageVersion>();
        }
        else if (message is UserEventDefine.UserTryUpdatePatchManifest)
        {
            _machine.ChangeState<FsmUpdatePackageManifest>();
        }
        else if (message is UserEventDefine.UserTryDownloadWebFiles)
        {
            _machine.ChangeState<FsmCreatePackageDownloader>();
        }
        else
        {
            throw new System.NotImplementedException($"{message.GetType()}");
        }
    }
}