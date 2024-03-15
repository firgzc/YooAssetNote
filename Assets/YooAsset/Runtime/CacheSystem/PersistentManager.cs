﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 持久化文件管理器
    /// </summary>
    internal class PersistentManager
    {
        /// <summary>
        /// 缓存数据文件路径列表
        /// </summary>
        private readonly Dictionary<string, string> _cachedDataFilePaths = new Dictionary<string, string>(10000);

        /// <summary>
        /// 缓存信息文件路径列表
        /// </summary>
        private readonly Dictionary<string, string> _cachedInfoFilePaths = new Dictionary<string, string>(10000);

        /// <summary>
        /// 临时数据文件路径列表
        /// </summary>
        private readonly Dictionary<string, string> _tempDataFilePaths = new Dictionary<string, string>(10000);

        /// <summary>
        /// 内置文件路径列表
        /// </summary>
        private readonly Dictionary<string, string> _buildinFilePaths = new Dictionary<string, string>(10000);

        /// <summary>
        /// 所属包裹
        /// </summary>
        public readonly string PackageName;

        /// <summary>
        /// 内置根路径
        /// </summary>
        public string BuildinRoot { private set; get; }

        /// <summary>
        /// 内置包裹根路径
        /// </summary>
        public string BuildinPackageRoot { private set; get; }

        /// <summary>
        /// 沙盒根路径
        /// </summary>
        public string SandboxRoot { private set; get; }

        /// <summary>
        /// 沙盒报告根路径
        /// </summary>
        public string SandboxPackageRoot { private set; get; }

        /// <summary>
        /// 沙盒缓存文件的根路径
        /// </summary>
        public string SandboxCacheFilesRoot { private set; get; }

        /// <summary>
        /// 沙盒清单文件根路径
        /// </summary>
        public string SandboxManifestFilesRoot { private set; get; }

        /// <summary>
        /// 沙盒程序输出文件根路径
        /// </summary>
        public string SandboxAppFootPrintFilePath { private set; get; }

        /// <summary>
        /// 文件扩展
        /// </summary>
        public bool AppendFileExtension { private set; get; }


        public PersistentManager(string packageName)
        {
            PackageName = packageName;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(string buildinRoot, string sandboxRoot, bool appendFileExtension)
        {
            if (string.IsNullOrEmpty(buildinRoot))
                BuildinRoot = CreateDefaultBuildinRoot();
            else
                BuildinRoot = buildinRoot;

            if (string.IsNullOrEmpty(sandboxRoot))
                SandboxRoot = CreateDefaultSandboxRoot();
            else
                SandboxRoot = sandboxRoot;

            BuildinPackageRoot = PathUtility.Combine(BuildinRoot, PackageName);
            SandboxPackageRoot = PathUtility.Combine(SandboxRoot, PackageName);
            SandboxCacheFilesRoot = PathUtility.Combine(SandboxPackageRoot, YooAssetSettings.CacheFilesFolderName);
            SandboxManifestFilesRoot = PathUtility.Combine(SandboxPackageRoot, YooAssetSettings.ManifestFolderName);
            SandboxAppFootPrintFilePath = PathUtility.Combine(SandboxPackageRoot, YooAssetSettings.AppFootPrintFileName);
            AppendFileExtension = appendFileExtension;
        }

        /// <summary>
        /// 创建默认内置文件根路径
        /// </summary>
        /// <returns></returns>
        private static string CreateDefaultBuildinRoot()
        {
            //返回streamingAsset的内置包文件路径
            return PathUtility.Combine(UnityEngine.Application.streamingAssetsPath, YooAssetSettingsData.Setting.DefaultYooFolderName);
        }

        /// <summary>
        /// 创建默认
        /// </summary>
        /// <returns></returns>
        private static string CreateDefaultSandboxRoot()
        {
#if UNITY_EDITOR
            // 注意：为了方便调试查看，编辑器下把存储目录放到项目里。
            string projectPath = Path.GetDirectoryName(UnityEngine.Application.dataPath);
            projectPath = PathUtility.RegularPath(projectPath);
            return PathUtility.Combine(projectPath, YooAssetSettingsData.Setting.DefaultYooFolderName);
#elif UNITY_STANDALONE
            return PathUtility.Combine(UnityEngine.Application.dataPath, YooAssetSettingsData.Setting.DefaultYooFolderName);
#else
            return PathUtility.Combine(UnityEngine.Application.persistentDataPath, YooAssetSettingsData.Setting.DefaultYooFolderName);	
#endif
        }

        /// <summary>
        /// 获取缓存数据文件路径
        /// </summary>
        /// <param name="bundle">包</param>
        public string GetCachedDataFilePath(PackageBundle bundle)
        {
            if (_cachedDataFilePaths.TryGetValue(bundle.CacheGUID, out string filePath) == false)
            {
                string folderName = bundle.FileHash.Substring(0, 2);
                filePath = PathUtility.Combine(SandboxCacheFilesRoot, folderName, bundle.CacheGUID, YooAssetSettings.CacheBundleDataFileName);
                if (AppendFileExtension)
                    filePath += bundle.FileExtension;
                _cachedDataFilePaths.Add(bundle.CacheGUID, filePath);
            }
            return filePath;
        }

        /// <summary>
        /// 获取缓存信息文件路径
        /// </summary>
        public string GetCachedInfoFilePath(PackageBundle bundle)
        {
            if (_cachedInfoFilePaths.TryGetValue(bundle.CacheGUID, out string filePath) == false)
            {
                string folderName = bundle.FileHash.Substring(0, 2);
                filePath = PathUtility.Combine(SandboxCacheFilesRoot, folderName, bundle.CacheGUID, YooAssetSettings.CacheBundleInfoFileName);
                _cachedInfoFilePaths.Add(bundle.CacheGUID, filePath);
            }
            return filePath;
        }

        /// <summary>
        /// 获取临时数据文件路径
        /// </summary>
        public string GetTempDataFilePath(PackageBundle bundle)
        {
            if (_tempDataFilePaths.TryGetValue(bundle.CacheGUID, out string filePath) == false)
            {
                string cachedDataFilePath = GetCachedDataFilePath(bundle);
                filePath = $"{cachedDataFilePath}.temp";
                _tempDataFilePaths.Add(bundle.CacheGUID, filePath);
            }
            return filePath;
        }

        /// <summary>
        /// 获取内置文件路径
        /// </summary>
        public string GetBuildinFilePath(PackageBundle bundle)
        {
            if (_buildinFilePaths.TryGetValue(bundle.CacheGUID, out string filePath) == false)
            {
                filePath = PathUtility.Combine(BuildinPackageRoot, bundle.FileName);
                _buildinFilePaths.Add(bundle.CacheGUID, filePath);
            }
            return filePath;
        }

        /// <summary>
        /// 删除沙盒里的包裹目录
        /// </summary>
        public void DeleteSandboxPackageFolder()
        {
            if (Directory.Exists(SandboxPackageRoot))
                Directory.Delete(SandboxPackageRoot, true);
        }

        /// <summary>
        /// 删除沙盒内的缓存文件夹
        /// </summary>
        public void DeleteSandboxCacheFilesFolder()
        {
            if (Directory.Exists(SandboxCacheFilesRoot))
                Directory.Delete(SandboxCacheFilesRoot, true);
        }

        /// <summary>
        /// 删除沙盒内的清单文件夹
        /// </summary>
        public void DeleteSandboxManifestFilesFolder()
        {
            if (Directory.Exists(SandboxManifestFilesRoot))
                Directory.Delete(SandboxManifestFilesRoot, true);
        }


        /// <summary>
        /// 获取沙盒内包裹的清单文件的路径
        /// </summary>
        public string GetSandboxPackageManifestFilePath(string packageVersion)
        {
            string fileName = YooAssetSettingsData.GetManifestBinaryFileName(PackageName, packageVersion);
            return PathUtility.Combine(SandboxManifestFilesRoot, fileName);
        }

        /// <summary>
        /// 获取沙盒内包裹的哈希文件的路径
        /// </summary>
        public string GetSandboxPackageHashFilePath(string packageVersion)
        {
            string fileName = YooAssetSettingsData.GetPackageHashFileName(PackageName, packageVersion);
            return PathUtility.Combine(SandboxManifestFilesRoot, fileName);
        }

        /// <summary>
        /// 获取沙盒内包裹的版本文件的路径
        /// </summary>
        public string GetSandboxPackageVersionFilePath()
        {
            string fileName = YooAssetSettingsData.GetPackageVersionFileName(PackageName);
            return PathUtility.Combine(SandboxManifestFilesRoot, fileName);
        }

        /// <summary>
        /// 保存沙盒内默认的包裹版本
        /// </summary>
        public void SaveSandboxPackageVersionFile(string version)
        {
            string filePath = GetSandboxPackageVersionFilePath();
            FileUtility.WriteAllText(filePath, version);
        }


        /// <summary>
        /// 获取APP内包裹的清单文件的路径
        /// </summary>
        public string GetBuildinPackageManifestFilePath(string packageVersion)
        {
            string fileName = YooAssetSettingsData.GetManifestBinaryFileName(PackageName, packageVersion);
            return PathUtility.Combine(BuildinPackageRoot, fileName);
        }

        /// <summary>
        /// 获取APP内包裹的哈希文件的路径
        /// </summary>
        public string GetBuildinPackageHashFilePath(string packageVersion)
        {
            string fileName = YooAssetSettingsData.GetPackageHashFileName(PackageName, packageVersion);
            return PathUtility.Combine(BuildinPackageRoot, fileName);
        }

        /// <summary>
        /// 获取APP内包裹的版本文件的路径
        /// </summary>
        public string GetBuildinPackageVersionFilePath()
        {
            string fileName = YooAssetSettingsData.GetPackageVersionFileName(PackageName);
            return PathUtility.Combine(BuildinPackageRoot, fileName);
        }
    }
}