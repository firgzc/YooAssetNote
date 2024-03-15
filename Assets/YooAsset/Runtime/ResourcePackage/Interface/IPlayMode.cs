
namespace YooAsset
{
    /// <summary>
    /// 运行相关
    /// </summary>
    internal interface IPlayMode
    {
        /// <summary>
        /// 激活的清单
        /// </summary>
        PackageManifest ActiveManifest { set; get; }

        /// <summary>
        /// 保存清单版本文件到沙盒
        /// </summary>
        void FlushManifestVersionFile();

        /// <summary>
        /// 向网络端请求最新的资源版本
        /// </summary>
        UpdatePackageVersionOperation UpdatePackageVersionAsync(bool appendTimeTicks, int timeout);

        /// <summary>
        /// 向网络端请求并更新清单
        /// </summary>
        UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageVersion, bool autoSaveVersion, int timeout);

        /// <summary>
        /// 预下载指定版本的包裹内容
        /// </summary>
        PreDownloadContentOperation PreDownloadContentAsync(string packageVersion, int timeout);

        // 下载相关

        /// <summary>
        /// 创建下载所有资源的下载器
        /// </summary>
        /// <param name="downloadingMaxNumber">下载最大数量</param>
        /// <param name="failedTryAgain">下载失败后重复次数</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        ResourceDownloaderOperation CreateResourceDownloaderByAll(int downloadingMaxNumber, int failedTryAgain, int timeout);

        /// <summary>
        /// 通过标签下载资源的下载器
        /// </summary>
        /// <param name="tags">包标签</param>
        /// <param name="downloadingMaxNumber">下载最大数量</param>
        /// <param name="failedTryAgain">下载失败后重复次数</param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        ResourceDownloaderOperation CreateResourceDownloaderByTags(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout);

        /// <summary>
        /// 通过路径下载资源的下载器
        /// </summary>
        /// <param name="assetInfos">资源信息-数组</param>
        /// <param name="downloadingMaxNumber"></param>
        /// <param name="failedTryAgain"></param>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        ResourceDownloaderOperation CreateResourceDownloaderByPaths(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout);

        // 解压相关

        /// <summary>
        /// 创建所有资源解压器
        /// </summary>
        /// <param name="upackingMaxNumber">解压最大数量</param>
        /// <param name="failedTryAgain"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        ResourceUnpackerOperation CreateResourceUnpackerByAll(int upackingMaxNumber, int failedTryAgain, int timeout);
        
        /// <summary>
        /// 创建通过标签解压资源
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="upackingMaxNumber">解压最大数量</param>
        /// <param name="failedTryAgain"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        ResourceUnpackerOperation CreateResourceUnpackerByTags(string[] tags, int upackingMaxNumber, int failedTryAgain, int timeout);

        // 导入相关

        /// <summary>
        /// 创建通过文件路径导入资源
        /// </summary>
        /// <param name="filePaths">文件路径</param>
        /// <param name="importerMaxNumber">导入资源最大数量</param>
        /// <param name="failedTryAgain"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        ResourceImporterOperation CreateResourceImporterByFilePaths(string[] filePaths, int importerMaxNumber, int failedTryAgain, int timeout);
    }
}