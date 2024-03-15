
namespace YooAsset
{
    /// <summary>
    /// 资源协作
    /// </summary>
    internal class ResourceAssist
    {
        public CacheManager Cache { set; get; }
        public PersistentManager Persistent { set; get; }
        public DownloadManager Download { set; get; }
        public ResourceLoader Loader { set; get; }
    }
}