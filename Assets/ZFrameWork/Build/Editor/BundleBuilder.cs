using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System.IO;
using LitJson;

namespace ZFrameWork.Build
{
    public class BundleBuildContext
    {
        public List<string> bundlePaths = new List<string>();
        public List<AssetBundleBuild> bundles = new List<AssetBundleBuild>();
        public string outPutPath = "";
        public bool isGenMd5 = false;
        public bool isSuccess = true;
        public string error = "no error";
        public string md5FilePath = null;
        public JsonData md5;
    }

    public class BundleBuilder
    {
        BundleBuildContext context;

        Dictionary<string, string> txtTypeFileCache = new Dictionary<string, string>();
        static HashSet<string> txtFileTypeSet = new HashSet<string>()
        {
            ".json",
            ".lua",
            ".xml",
        };

        public BundleBuilder(BundleBuildContext context)
        {
            this.context = context;
        }

        public void Start()
        {
            BeginBuild();
            if (context.isSuccess == false) return;
            Build();
            if (context.isSuccess == false) return;
            EndBuild();
        }

        void BeginBuild()
        {
            if (string.IsNullOrEmpty(context.outPutPath))
            {
                context.isSuccess = false;
                context.error = "OutPutPath is Null Or Empty";
                return;
            }
            ZFileUtil.CheckDir(context.outPutPath, true, true);

            if (context.bundlePaths.Count == 0)
            {
                context.isSuccess = false;
                context.error = "BundlePaths Count = 0";
                return;
            }

            foreach (var path in context.bundlePaths)
            {
                if (!ZFileUtil.CheckDir(path))
                {
                    context.isSuccess = false;
                    context.error = $"BundlePaths path = {path} is not exist";
                    return;
                }
            }

            txtTypeFileCache.Clear();
            var files = ZFileUtil.GetFiles(context.bundlePaths, null, (s) => { return !s.EndsWith(".meta"); });
            context.md5 = ZFileUtil.GenFilesMd5Json(files);
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file);
                if (txtFileTypeSet.Contains(extension))
                {
                    var currFile = Path.ChangeExtension(file, FileConst.TxtFileExtension);
                    File.Move(file, currFile);
                    txtTypeFileCache[file] = currFile;
                }
            }
            AssetDatabase.Refresh();
        }

        void Build()
        {
            var bundlePaths = context.bundlePaths;
            var assets = new List<string>();
            foreach (var path in bundlePaths)
            {
                var key = path.Substring(path.IndexOf("Assets"));
                key = AssetUtil.DirPath2BundleKey(path);
                var files = ZFileUtil.GetFiles(path, ZFileUtil.SubPathFromAssets, (s) => { return !s.EndsWith(".meta"); });
                if (files.Count == 0) continue;
                foreach (var file in files)
                    assets.Add(file);
                context.bundles.Add(new AssetBundleBuild { assetNames = assets.ToArray(), assetBundleName = key });
                assets.Clear();
            }

            BuildPipeline.BuildAssetBundles(context.outPutPath, context.bundles.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, BuildConst.BuildTarget);
        }

        void EndBuild()
        {
            CreateMd5File();

            foreach (var pair in txtTypeFileCache)
                File.Move(pair.Value, pair.Key);
            txtTypeFileCache.Clear();
        }

        void CreateMd5File()
        {
            if (context.md5FilePath == null)
                return;
            var json = ZFileUtil.ReadFileToJson(FileConst.UpdateAssetMd5Path);
            if (json == null)
                json = new JsonData();

            foreach (var key in context.md5.Keys)
                json[key] = context.md5[key];

            ZFileUtil.WriteJsonFile(context.md5FilePath, json);
        }
    }
}
