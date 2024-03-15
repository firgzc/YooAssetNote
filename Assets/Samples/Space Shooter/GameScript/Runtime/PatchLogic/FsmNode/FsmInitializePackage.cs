using System.IO;
using System.Collections;
using UnityEngine;
using UniFramework.Machine;
using YooAsset;

/// <summary>
/// 初始化资源包
/// </summary>
internal class FsmInitializePackage : IStateNode
{
    private StateMachine _machine;
    /// <summary>
    /// 包裹名
    /// </summary>
    private string _packageName;

    void IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }
    void IStateNode.OnEnter()
    {
        Debug.Log($"<color=blue>初始化资源包！</color>");
        PatchEventDefine.PatchStatesChange.SendEventMessage("初始化资源包！");
        GameManager.Instance.StartCoroutine(InitPackage());
    }


    void IStateNode.OnUpdate()
    {
        Debug.Log($"<color=blue>当前的处在的节点状态:  {_machine.CurrentNode}</color>");
    }


    void IStateNode.OnExit()
    {
        Debug.Log($"<color=blue>退出当前节点状态:  {_machine.CurrentNode}</color>");
    }



    private IEnumerator InitPackage()
    {
        //获取运行模式数据
        var playMode = (EPlayMode)_machine.GetBlackboardValue("PlayMode");
        //获取包裹名
        var packageName = (string)_machine.GetBlackboardValue("PackageName");
        //获取渲染管线
        var buildPipeline = (string)_machine.GetBlackboardValue("BuildPipeline");

        // 创建资源包裹类
        var package = YooAssets.TryGetPackage(packageName);
        if (package == null)
            package = YooAssets.CreatePackage(packageName);

        _packageName = packageName;
        // 编辑器下的模拟模式
        InitializationOperation initializationOperation = null;
        if (playMode == EPlayMode.EditorSimulateMode)
        {
            var createParameters = new EditorSimulateModeParameters();
            createParameters.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(buildPipeline, packageName);
            initializationOperation = package.InitializeAsync(createParameters);
        }

        // 单机运行模式
        if (playMode == EPlayMode.OfflinePlayMode)
        {
            var createParameters = new OfflinePlayModeParameters();
            createParameters.DecryptionServices = new FileStreamDecryption();
            initializationOperation = package.InitializeAsync(createParameters);
        }

        // 联机运行模式
        if (playMode == EPlayMode.HostPlayMode)
        {
            Debug.Log("输出包裹名： " + _packageName);
            string defaultHostServer = GetHostServerURL();
            string fallbackHostServer = GetHostServerURL();
            var createParameters = new HostPlayModeParameters();
            createParameters.DecryptionServices = new FileStreamDecryption();
            createParameters.BuildinQueryServices = new GameQueryServices();
            createParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            initializationOperation = package.InitializeAsync(createParameters);
        }

        // WebGL运行模式
        if (playMode == EPlayMode.WebPlayMode)
        {
            Debug.Log("输出包裹名： " + _packageName );

            string defaultHostServer = GetHostServerURL();
            string fallbackHostServer = GetHostServerURL();
            var createParameters = new WebPlayModeParameters();
            createParameters.DecryptionServices = new FileStreamDecryption();
            createParameters.BuildinQueryServices = new GameQueryServices();
            createParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            initializationOperation = package.InitializeAsync(createParameters);
        }

        yield return initializationOperation;

        // 如果初始化失败弹出提示界面
        if (initializationOperation.Status != EOperationStatus.Succeed)
        {
            Debug.LogError($"{initializationOperation.Error}");
            PatchEventDefine.InitializeFailed.SendEventMessage();
        }
        else
        {
            //初始化成功后，获取版本号--即日期那个
            //此处，PackageVersion为空
            var version = initializationOperation.PackageVersion;
            Debug.Log($"Init resource package version : {version}");
            _machine.ChangeState<FsmUpdatePackageVersion>();
        }
    }

    /// <summary>
    /// 获取资源服务器地址
    /// </summary>
    private string GetHostServerURL()
    {
        //string hostServerIP = "http://10.0.2.2"; //安卓模拟器地址
        //string hostServerIP = "http://192.168.1.21:7000";
        string hostServerIP = "http://82.156.217.244:10021";
        //版本号可以自定义此处变量仅为示意
        string appVersion = "v1.0"; 
        //此处不需要日期版本号、包裹名，直接将日期下的包体整体拷贝到平台下即可
        return $"{hostServerIP}/CDN/{PlatformUtility.GetPlatformName()}/{appVersion}"; 

        //下面代码不需要，已被修改
#if UNITY_EDITOR

        //if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
        //    return $"{hostServerIP}/CDN/Android/{appVersion}";
        //else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
        //    return $"{hostServerIP}/CDN/IPhone/{appVersion}";
        //else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
        //    return $"{hostServerIP}/CDN/WebGL/{appVersion}";
        //else
        //{
        //    Debug.Log("输出当前平台：" + UnityEditor.EditorUserBuildSettings.activeBuildTarget);
        //    return $"{hostServerIP}/PC/";           
        //}
#else
        //if (Application.platform == RuntimePlatform.Android)
        //    return $"{hostServerIP}/CDN/Android/{appVersion}";
        //else if (Application.platform == RuntimePlatform.IPhonePlayer)
        //    return $"{hostServerIP}/CDN/IPhone/{appVersion}";
        //else if (Application.platform == RuntimePlatform.WebGLPlayer)
        //    return $"{hostServerIP}/CDN/WebGL/{appVersion}";
        //else
        //{
        //     return $"{hostServerIP}/StandaloneWindows64/DefaultPackage/{appVersion}";  
        //    //return $"{hostServerIP}/CDN/PC/{appVersion}";
        //}
#endif
    } 

    /// <summary>
    /// 远端资源地址查询服务类
    /// </summary>
    private class RemoteServices : IRemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }
        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }
        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }
    }

    /// <summary>
    /// 资源文件流加载解密类
    /// </summary>
    private class FileStreamDecryption : IDecryptionServices
    {
        /// <summary>
        /// 同步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        AssetBundle IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
        {
            BundleStream bundleStream = new BundleStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            managedStream = bundleStream;
            return AssetBundle.LoadFromStream(bundleStream, fileInfo.ConentCRC, GetManagedReadBufferSize());
        }

        /// <summary>
        /// 异步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        AssetBundleCreateRequest IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo, out Stream managedStream)
        {
            BundleStream bundleStream = new BundleStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            managedStream = bundleStream;
            return AssetBundle.LoadFromStreamAsync(bundleStream, fileInfo.ConentCRC, GetManagedReadBufferSize());
        }
   
        private static uint GetManagedReadBufferSize()
        {
            return 1024;
        }
    }

    /// <summary>
    /// 资源文件偏移加载解密类
    /// </summary>
    private class FileOffsetDecryption : IDecryptionServices
    {
        /// <summary>
        /// 同步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        AssetBundle IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
        {
            managedStream = null;
            return AssetBundle.LoadFromFile(fileInfo.FileLoadPath, fileInfo.ConentCRC, GetFileOffset());
        }

        /// <summary>
        /// 异步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        AssetBundleCreateRequest IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo, out Stream managedStream)
        {
            managedStream = null;
            return AssetBundle.LoadFromFileAsync(fileInfo.FileLoadPath, fileInfo.ConentCRC, GetFileOffset());
        }

        private static ulong GetFileOffset()
        {
            return 32;
        }
    }
}

/// <summary>
/// 资源文件解密流
/// </summary>
public class BundleStream : FileStream
{
    public const byte KEY = 64;

    public BundleStream(string path, FileMode mode, FileAccess access, FileShare share) : base(path, mode, access, share)
    {
    }

    public BundleStream(string path, FileMode mode) : base(path, mode)
    {
    }

    public override int Read(byte[] array, int offset, int count)
    {
        var index = base.Read(array, offset, count);
        for (int i = 0; i < array.Length; i++)
        {
            array[i] ^= KEY;
        }
        return index;
    }
}