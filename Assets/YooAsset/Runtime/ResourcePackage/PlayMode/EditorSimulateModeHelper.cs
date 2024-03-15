#if UNITY_EDITOR
using System.Reflection;

namespace YooAsset
{
    /// <summary>
    /// 编辑器下模拟构建
    /// </summary>
    public static class EditorSimulateModeHelper
    {
        private static System.Type _classType;

        /// <summary>
        /// 编辑器下模拟构建清单
        /// </summary>
        public static string SimulateBuild(string buildPipelineName, string packageName)
        {
            //通过反射加载对应的dll，获取里面的类，进而可以获取到对应的变量、属性、方法
            if (_classType == null)
                _classType = Assembly.Load("YooAsset.Editor").GetType("YooAsset.Editor.AssetBundleSimulateBuilder");            
            string manifestFilePath = (string)InvokePublicStaticMethod(_classType, "SimulateBuild", buildPipelineName, packageName);
            return manifestFilePath;
        }

        /// <summary>
        /// 编辑器下模拟构建清单
        /// </summary>
        public static string SimulateBuild(EDefaultBuildPipeline buildPipeline, string packageName)
        {
            return SimulateBuild(buildPipeline.ToString(), packageName);
        }

        /// <summary>
        /// 执行公共的静态方法
        /// </summary>
        /// <param name="type">类</param>
        /// <param name="method">方法</param>
        /// <param name="parameters">方法参数</param>
        private static object InvokePublicStaticMethod(System.Type type, string method, params object[] parameters)
        {
            var methodInfo = type.GetMethod(method, BindingFlags.Public | BindingFlags.Static);
            if (methodInfo == null)
            {
                UnityEngine.Debug.LogError($"{type.FullName} not found method : {method}");
                return null;
            }
            return methodInfo.Invoke(null, parameters);
        }
    }
}
#else
namespace YooAsset
{ 
    public static class EditorSimulateModeHelper
    {
        public static string SimulateBuild(string buildPipelineName, string packageName) 
        {
            throw new System.Exception("Only support in unity editor !");
        }

        public static string SimulateBuild(EDefaultBuildPipeline buildPipeline, string packageName)
        {
            throw new System.Exception("Only support in unity editor !");
        }
    }
}
#endif