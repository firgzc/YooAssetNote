using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// 资源包文件信息
    /// </summary>
    internal class BundleInfo
    {
        /// <summary>
        /// 资源包加载模式
        /// </summary>
        public enum ELoadMode
        {
            None, 
            LoadFromDelivery,  //分发加载
            LoadFromStreaming,  //流加载
            LoadFromCache,  //缓存加载
            LoadFromRemote, //远端加载
            LoadFromEditor, //编辑器加载
        }

        private readonly ResourceAssist _assist;

        /// <summary>
        /// 资源包对象
        /// </summary>
        public readonly PackageBundle Bundle;

        /// <summary>
        /// 资源包加载模式
        /// </summary>
        public readonly ELoadMode LoadMode;

        /// <summary>
        /// 远端下载地址
        /// </summary>
        public string RemoteMainURL { private set; get; }

        /// <summary>
        /// 远端下载备用地址
        /// </summary>
        public string RemoteFallbackURL { private set; get; }

        /// <summary>
        /// 分发资源文件路径
        /// </summary>
        public string DeliveryFilePath { private set; get; }

        /// <summary>
        /// 注意：该字段只用于帮助编辑器下的模拟模式。
        /// </summary>
        public string[] IncludeAssetsInEditor;

        private BundleInfo()
        {
        }


        public BundleInfo(ResourceAssist assist, PackageBundle bundle, ELoadMode loadMode, string mainURL, string fallbackURL)
        {
            _assist = assist;
            Bundle = bundle;
            LoadMode = loadMode;
            RemoteMainURL = mainURL;
            RemoteFallbackURL = fallbackURL;
            DeliveryFilePath = string.Empty;
        }


        public BundleInfo(ResourceAssist assist, PackageBundle bundle, ELoadMode loadMode, string deliveryFilePath)
        {
            _assist = assist;
            Bundle = bundle;
            LoadMode = loadMode;
            RemoteMainURL = string.Empty;
            RemoteFallbackURL = string.Empty;
            DeliveryFilePath = deliveryFilePath;
        }


        public BundleInfo(ResourceAssist assist, PackageBundle bundle, ELoadMode loadMode)
        {
            _assist = assist;
            Bundle = bundle;
            LoadMode = loadMode;
            RemoteMainURL = string.Empty;
            RemoteFallbackURL = string.Empty;
            DeliveryFilePath = string.Empty;
        }

        #region Cache

        public bool IsCached()
        {
            return _assist.Cache.IsCached(Bundle.CacheGUID);
        }

        /// <summary>
        /// 缓存记录
        /// </summary>
        public void CacheRecord()
        {
            string infoFilePath = CachedInfoFilePath;
            string dataFilePath = CachedDataFilePath;
            string dataFileCRC = Bundle.FileCRC;
            long dataFileSize = Bundle.FileSize;
            var wrapper = new CacheManager.RecordWrapper(infoFilePath, dataFilePath, dataFileCRC, dataFileSize);
            _assist.Cache.Record(Bundle.CacheGUID, wrapper);
        }

        /// <summary>
        /// 丢弃缓存
        /// </summary>
        public void CacheDiscard()
        {
            _assist.Cache.Discard(Bundle.CacheGUID);
        }

        /// <summary>
        /// 校验结果
        /// </summary>
        public EVerifyResult VerifySelf()
        {
            return CacheHelper.VerifyingRecordFile(_assist.Cache, Bundle.CacheGUID);
        }

        #endregion

        #region Persistent

        public string CachedDataFilePath
        {
            get
            {
                return _assist.Persistent.GetCachedDataFilePath(Bundle);
            }
        }

        public string CachedInfoFilePath
        {
            get
            {
                return _assist.Persistent.GetCachedInfoFilePath(Bundle);
            }
        }

        public string TempDataFilePath
        {
            get
            {
                return _assist.Persistent.GetTempDataFilePath(Bundle);
            }
        }

        public string BuildinFilePath
        {
            get
            {
                return _assist.Persistent.GetBuildinFilePath(Bundle);
            }
        }

        #endregion

        #region Download

        /// <summary>
        /// 创建下载器
        /// </summary>
        public DownloaderBase CreateDownloader(int failedTryAgain, int timeout = 60)
        {
            return _assist.Download.CreateDownload(this, failedTryAgain, timeout);
        }

        /// <summary>
        /// 创建解压器
        /// </summary>
        public DownloaderBase CreateUnpacker(int failedTryAgain, int timeout = 60)
        {
            var unpackBundleInfo = ConvertToUnpackInfo();
            return _assist.Download.CreateDownload(unpackBundleInfo, failedTryAgain, timeout);
        }

        #endregion

        #region AssetBundle

        /// <summary>
        /// 同步加载bundle
        /// </summary>
        /// <param name="fileLoadPath">文件加载路径</param>
        /// <param name="managedStream">流对象加载</param>
        /// <returns></returns>
        internal AssetBundle LoadAssetBundle(string fileLoadPath, out Stream managedStream)
        {
            return _assist.Loader.LoadAssetBundle(this, fileLoadPath, out managedStream);
        }

        /// <summary>
        /// 异步加载bundle
        /// </summary>
        /// <param name="fileLoadPath">文件加载路径</param>
        /// <param name="managedStream">流对象加载</param>
        /// <returns></returns>
        internal AssetBundleCreateRequest LoadAssetBundleAsync(string fileLoadPath, out Stream managedStream)
        {
            return _assist.Loader.LoadAssetBundleAsync(this, fileLoadPath, out managedStream);
        }

        /// <summary>
        /// 异步加载分发bundle
        /// </summary>
        /// <param name="fileLoadPath">文件加载路径</param>
        internal AssetBundle LoadDeliveryAssetBundle(string fileLoadPath)
        {
            return _assist.Loader.LoadDeliveryAssetBundle(this, fileLoadPath);
        }

        /// <summary>
        /// 异步加载分发bundle
        /// </summary>
        /// <param name="fileLoadPath">文件加载路径</param>
        internal AssetBundleCreateRequest LoadDeliveryAssetBundleAsync(string fileLoadPath)
        {
            return _assist.Loader.LoadDeliveryAssetBundleAsync(this, fileLoadPath);
        }


        #endregion

        /// <summary>
        /// 转换为解压BundleInfo
        /// </summary>
        private BundleInfo ConvertToUnpackInfo()
        {
            string streamingPath = PersistentHelper.ConvertToWWWPath(BuildinFilePath);
            BundleInfo newBundleInfo = new BundleInfo(_assist, Bundle, ELoadMode.LoadFromStreaming, streamingPath, streamingPath);
            return newBundleInfo;
        }

        /// <summary>
        /// 批量创建解压BundleInfo
        /// </summary>
        public static List<BundleInfo> CreateUnpackInfos(ResourceAssist assist, List<PackageBundle> unpackList)
        {
            List<BundleInfo> result = new List<BundleInfo>(unpackList.Count);
            foreach (var packageBundle in unpackList)
            {
                var bundleInfo = CreateUnpackInfo(assist, packageBundle);
                result.Add(bundleInfo);
            }
            return result;
        }
        private static BundleInfo CreateUnpackInfo(ResourceAssist assist, PackageBundle packageBundle)
        {
            string streamingPath = PersistentHelper.ConvertToWWWPath(assist.Persistent.GetBuildinFilePath(packageBundle));
            BundleInfo newBundleInfo = new BundleInfo(assist, packageBundle, ELoadMode.LoadFromStreaming, streamingPath, streamingPath);
            return newBundleInfo;
        }

        /// <summary>
        /// 创建导入BundleInfo
        /// </summary>
        public static BundleInfo CreateImportInfo(ResourceAssist assist, PackageBundle packageBundle, string filePath)
        {
            // 注意：我们把本地文件路径指定为远端下载地址
            string persistentPath = PersistentHelper.ConvertToWWWPath(filePath);
            BundleInfo bundleInfo = new BundleInfo(assist, packageBundle, BundleInfo.ELoadMode.None, persistentPath, persistentPath);
            return bundleInfo;
        }
    }
}