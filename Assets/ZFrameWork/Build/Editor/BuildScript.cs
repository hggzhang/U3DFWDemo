using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using LitJson;
using ZFrameWork;

namespace ZFrameWork.Build
{
    public class BuildScript
    {
        static BundleBuildContext context;
        static public void Publish()
        {
            InitContext();
            BundleBuilder builder = new BundleBuilder(context);
            builder.Start();
            if (!context.isSuccess)
            {
                ZLogger.Log(context.error);
                return;
            }
            Clear();
            AssetDatabase.Refresh();
        }

        static void InitContext()
        {
            context = new BundleBuildContext();
            context.outPutPath = FileConst.AssetBundleLocalPath;
            context.md5FilePath = FileConst.AssetMd5Path;
            context.bundlePaths = BuildUtil.GetBuildBundlePaths();
        }

        static void Clear()
        {
            context = null;
        }

        /*static void CreateMd5File()
        {
            var jsonStr = JsonMapper.ToJson(context.md5Dict);
            var json = JsonMapper.ToObject(jsonStr);
            ZFileUtil.WriteJsonFile(FileConst.AssetMd5Path, json);
        }*/

        /*static void GatherLuaBundles(List<AssetBundleBuild> inOutBundles)
        {
            var paths = BuildConst.LuaBuildPaths;
            var copyList = new List<string>();
            foreach (var path in paths)
            {
                var key = path.Substring(path.IndexOf("Assets"));
                key = $"{key.Replace("\\","_")}.bundle";
                var files = ZFileUtil.GetFiles(path, null, (s) => { return s.EndsWith(".lua"); });
                var assets = new List<string>();
                foreach (var file in files)
                {
                    var byteFile = Path.ChangeExtension(file, "bytes");
                    File.Copy(file, byteFile);
                    assets.Add(ZFileUtil.SubPathFromAssets(byteFile));
                    copyList.Add(byteFile);
                }
                AssetDatabase.Refresh();
                BuildUtil.GatherAssetBundleBuilds(inOutBundles, key, assets.ToArray());
            }

            foreach (var copy in copyList)
            {
                File.Delete(copy);
                File.Delete(Path.ChangeExtension(copy,"meta"));
            }
        }*/
    }
}