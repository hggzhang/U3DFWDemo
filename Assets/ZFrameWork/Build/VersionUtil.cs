using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;


namespace ZFrameWork
{
    public class VersionUtil 
    {
        static public string GetLocalResVersion()
        {
            var json = ZFileUtil.ReadFileToJson(FileConst.LocalVersionPath);
            if (json != null)
                return json["res_version"].ToString();
            return "0.0.0.0";
        }

        static public string GetRemoteResVersion()
        {
            var json = ZFileUtil.ReadFileToJson(FileConst.RemoteVersionPath);
            if (json != null)
                return json["res_version"].ToString();
            return "0.0.0.0";
        }

        static string GetUpgradeVersion(string inVersion, int idx)
        {
            var versions = inVersion.Split(".");
            int versionNum = int.Parse(versions[idx]);
            ++versionNum;
            versions[idx] = versionNum.ToString();
            return $"{versions[0]}.{versions[1]}.{versions[2]}.{versions[3]}";
        }


        static public string GetUpgradeUpdateVersion(string inVersion)
        {
            return GetUpgradeVersion(inVersion, 3);
        }

        static int GetVersionNum(string version, int idx)
        {
            return int.Parse(version.Split(".")[idx]);
        }

        static int CompareResVersion(string lowVersion, string highVersion)
        {
            var low = GetVersionNum(lowVersion, 3);
            var high = GetVersionNum(highVersion, 3);
            if (low == high) return 0;
            return high > low ? 1 : -1;
        }

        static public List<string> GetUpdateVersions(string localVer, string serviceVer)
        {
            List<string> ret = new List<string>();
            var ver = GetUpgradeVersion(localVer, 3);
            while (CompareResVersion(ver, serviceVer) >= 0)
            {
                ret.Add(ver);
                ver = GetUpgradeVersion(ver, 3);
            }

            return ret;
        }

        static public void SaveResVersionToLocal(string ver)
        {
            var json = new JsonData();
            json["res_version"] = ver;
            ZFileUtil.WriteJsonFile(FileConst.LocalVersionPath, json);
        }

        static public void SaveResVersionToRemote(string ver)
        {
            var json = new JsonData();
            json["res_version"] = ver;
            ZFileUtil.WriteJsonFile(FileConst.RemoteVersionPath, json);
        }
    }
}