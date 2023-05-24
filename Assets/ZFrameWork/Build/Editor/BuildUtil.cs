using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Security.Cryptography;
using UnityEditor;
using System.Linq;
using System.Text;
using LitJson;

namespace ZFrameWork.Build
{
    public class BuildUtil
    {
        static public List<string> GetBuildBundlePaths()
        {
            List<string> ret = new List<string>();
            var json = ZFileUtil.ReadFileToJson(FileConst.BuildBundleKeyFilePath);
            if (json != null && json.IsArray)
            {
                for (int i = 0; i < json.Count; i++)
                {
                    ret.Add((string)json[i]);
                }
            }

            return ret;
        }


        static public string GetUpdateBuildDirName(string resVersion)
        {
            return $"Res_{resVersion}";
        }
    }
}
