using System;
using System.IO;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 缓存管理器
    /// </summary>
    internal class CacheManager
    {
        /// <summary>
        /// 信息存档
        /// </summary>
        internal class RecordWrapper
        {
            /// <summary>
            /// 信息文件路径 -- 暂定
            /// </summary>
            public string InfoFilePath { private set; get; }
            /// <summary>
            /// 数据文件路径 -- 暂定
            /// </summary>
            public string DataFilePath { private set; get; }
            /// <summary>
            /// 数据文件的crc校验
            /// </summary>
            public string DataFileCRC { private set; get; }
            /// <summary>
            /// 数据文件大小
            /// </summary>
            public long DataFileSize { private set; get; }

            public RecordWrapper(string infoFilePath, string dataFilePath, string dataFileCRC, long dataFileSize)
            {
                InfoFilePath = infoFilePath;
                DataFilePath = dataFilePath;
                DataFileCRC = dataFileCRC;
                DataFileSize = dataFileSize;
            }
        }

        /// <summary>
        /// 存档列表
        /// </summary>
        private readonly Dictionary<string, RecordWrapper> _wrappers = new Dictionary<string, RecordWrapper>();

        /// <summary>
        /// 所属包裹
        /// </summary>
        public readonly string PackageName;

        /// <summary>
        /// 验证级别
        /// </summary>
        public readonly EVerifyLevel BootVerifyLevel;


        public CacheManager(string packageName, EVerifyLevel bootVerifyLevel)
        {
            PackageName = packageName;
            BootVerifyLevel = bootVerifyLevel;
        }

        /// <summary>
        /// 清空所有数据
        /// </summary>
        public void ClearAll()
        {
            _wrappers.Clear();
        }

        /// <summary>
        /// 查询缓存记录
        /// </summary>
        public bool IsCached(string cacheGUID)
        {
            return _wrappers.ContainsKey(cacheGUID);
        }

        /// <summary>
        /// 记录验证结果
        /// </summary>
        public void Record(string cacheGUID, RecordWrapper wrapper)
        {
            if (_wrappers.ContainsKey(cacheGUID) == false)
            {
                _wrappers.Add(cacheGUID, wrapper);
            }
            else
            {
                throw new Exception("Should never get here !");
            }
        }

        /// <summary>
        /// 丢弃验证结果并删除缓存文件
        /// </summary>
        public void Discard(string cacheGUID)
        {
            var wrapper = TryGetWrapper(cacheGUID);
            if (wrapper != null)
            {
                try
                {
                    string dataFilePath = wrapper.DataFilePath;
                    FileInfo fileInfo = new FileInfo(dataFilePath);
                    if (fileInfo.Exists)
                        fileInfo.Directory.Delete(true);
                }
                catch (Exception e)
                {
                    YooLogger.Error($"Failed to delete cache file ! {e.Message}");
                }
            }

            if (_wrappers.ContainsKey(cacheGUID))
            {
                _wrappers.Remove(cacheGUID);
            }
        }

        /// <summary>
        /// 获取记录对象
        /// </summary>
        public RecordWrapper TryGetWrapper(string cacheGUID)
        {
            if (_wrappers.TryGetValue(cacheGUID, out RecordWrapper value))
                return value;
            else
                return null;
        }

        /// <summary>
        /// 获取缓存文件总数
        /// </summary>
        public int GetAllCachedFilesCount()
        {
            return _wrappers.Count;
        }

        /// <summary>
        /// 获取缓存GUID集合
        /// </summary>
        public List<string> GetAllCachedGUIDs()
        {
            List<string> keys = new List<string>(_wrappers.Keys.Count);
            var keyCollection = _wrappers.Keys;
            foreach (var key in keyCollection)
            {
                keys.Add(key);
            }
            return keys;
        }
    }
}