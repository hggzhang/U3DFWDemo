using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using LitJson;
using UObj = UnityEngine.Object;

namespace ZFrameWork.Build
{
    public class UpdateBuildScript
    {
        static BundleBuildContext context;
        static string ver = "";
        static string upgradeVer = "";


        static public void HotUpdate(List<UObj> updateResObjs)
        {
            InitContext(updateResObjs);
            BundleBuilder builder = new BundleBuilder(context);
            builder.Start();
            if (!context.isSuccess)
            {
                ZLogger.Log(context.error);
                return;
            }
            VersionUtil.SaveResVersionToLocal(upgradeVer);
            CreateZip();
            CreateUpdateJson();
            Clear();
        }

        static void InitContext(List<UObj> updateResObjs)
        {
            ver = VersionUtil.GetLocalResVersion();
            upgradeVer = VersionUtil.GetUpgradeUpdateVersion(ver);

            context = new BundleBuildContext();
            var files = new List<string>();
            foreach (var Obj in updateResObjs)
                files.Add(AssetDatabase.GetAssetPath(Obj));
            context.bundlePaths = GatherUpdateBundles(files);
            context.outPutPath = Path.Combine(FileConst.UpdateResPath);
            context.md5FilePath = FileConst.UpdateAssetMd5Path;
        }

        static void Clear()
        {
            context = null;
            ver = "";
            upgradeVer = "";
        }

        static public List<string> GatherUpdateFile()
        {
            var ret = new List<string>();
            var dict = new Dictionary<string, string>();

            var jsonText = ZFileUtil.ReadFileToText(FileConst.AssetMd5Path);
            if (jsonText != null)
            {
                var json = JsonMapper.ToObject(jsonText);
                foreach (var key in json.Keys)
                    dict[key] = json[key].ToString();
            }

            jsonText = ZFileUtil.ReadFileToText(FileConst.UpdateAssetMd5Path);
            if (jsonText != null)
            {
                var json = JsonMapper.ToObject(jsonText);
                foreach (var key in json.Keys)
                    dict[key] = json[key].ToString();
            }

            var bundlePaths = BuildUtil.GetBuildBundlePaths();
            var files = ZFileUtil.GetFiles(bundlePaths, ZFileUtil.SubPathFromAssets, (s) => { return !s.EndsWith(".meta"); });
            var newJson = ZFileUtil.GenFilesMd5Json(files);

            foreach (var key in newJson.Keys)
            {
                if (!dict.ContainsKey(key) || dict[key] != newJson[key].ToString())
                    ret.Add(key);
            }

            return ret;
        }

        static List<string> GatherUpdateBundles(List<string> updateFiles)
        {
            AssetFacade.Flush();
            HashSet<string> set = new HashSet<string>();
            foreach (var file in updateFiles)
            {
                var bundlePath = GetAssetBundlePath(file);
                if (bundlePath != null)
                    set.Add(bundlePath);
            }
            var bundles = new string[set.Count];
            set.CopyTo(bundles);
            return new List<string>(bundles);
        }

        static string GetAssetBundlePath(string fileName)
        {
            var bundlePaths = BuildUtil.GetBuildBundlePaths();
            var bundleKey = AssetFacade.GetBundleKey(fileName);
            foreach (var bundlePath in bundlePaths)
            {
                var key = AssetUtil.DirPath2BundleKey(bundlePath);
                if (key == bundleKey)
                    return bundlePath;
            }
            return null;
        }

        static void CreateZip()
        {
            var outPath = context.outPutPath;
            var name = $"{BuildUtil.GetUpdateBuildDirName(upgradeVer)}.zip";
            var files = new List<string>();
            foreach (var bundle in context.bundles)
            {
                var bundleName = Path.Combine(outPath, bundle.assetBundleName);
                files.Add(bundleName);
                files.Add($"{bundleName}.manifest");
            }

            var manifestBundle = Path.Combine(outPath, FileConst.UpdateResDir);
            files.Add(manifestBundle);
            files.Add($"{manifestBundle}.manifest");

            var path = FileConst.ServiceDataPath;
            ZFileUtil.ZipFiles(path, name, files.ToArray());
        }

        static void CreateUpdateJson()
        {
            var json = new JsonData();
            json["res_version"] = upgradeVer;
            ZFileUtil.WriteJsonFile(Path.Combine(FileConst.ServiceDataPath, FileConst.HotUpdateJsonFileName), json);
        }

        /*static void CreateAssetMd5()
        {
            var json = ZFileUtil.ReadFileToJson(FileConst.UpdateAssetMd5Path);
            if (json == null)
                json = new JsonData();

            var md5Dict = context.md5Dict;
            foreach (var pair in md5Dict)
                json[pair.Key] = pair.Value;

            ZFileUtil.WriteJsonFile(FileConst.UpdateAssetMd5Path, json);
        }*/

    }
}
