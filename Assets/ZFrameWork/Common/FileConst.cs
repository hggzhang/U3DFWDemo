using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace ZFrameWork
{
    public class FileConst
    {

        public static string ResourcesPath { get { return Path.Combine(Application.dataPath, "Resources"); } }

        // Version
        public static string LocalVersionPath { get { return Path.Combine(ResourcesPath, "version.txt"); } }
        public static string RemoteVersionPath { get { return Path.Combine(Application.persistentDataPath, "version.txt"); } }

        //Res
        public static string AssetBundleLocalDir = "Res";
        public static string AssetBundleLocalPath { get { return Path.Combine(Application.streamingAssetsPath, AssetBundleLocalDir); } }
        public static string LocalManifestBundlePath { get { return Path.Combine(AssetBundleLocalPath, AssetBundleLocalDir); } }

        public static string AssetBundleRemoteDir = "UpdateRes";
        public static string AssetBundleRemotePath { get { return Path.Combine(Application.persistentDataPath, AssetBundleRemoteDir); } }
        public static string RemoteManifestBundlePath { get { return Path.Combine(AssetBundleRemotePath, AssetBundleRemoteDir); } }

        public static string AssetMd5Path { get { return Path.Combine(Application.dataPath, "AssetMd5.txt"); } }
        public static string UpdateAssetMd5Path { get { return Path.Combine(ServiceDataPath, "AssetMd5.txt"); } }

        public static string ServiceDataPath { get { return Path.Combine(Application.dataPath, "..", "ServiceData"); } }
        public static string BuildBundleKeyFilePath { get { return Path.Combine(Application.dataPath, "BuildBundleKeys.txt"); } }

        static public string HotUpdateJsonFileName { get { return "update_json.txt"; } }
        public static string UpdateResDir = "UpdateRes";
        public static string UpdateResPath { get { return Path.Combine(Application.dataPath, "..", "UpdateRes"); } }

        public static string TxtFileExtension = ".txt";

        public static string GetAppExtension(bool isAbb)
        {
#if UNITY_STANDALONE
            return ".exe";
#elif UNITY_ANDROID
        if(useAAB)
        {
            return ".aab";
        }
        else
        {
            return ".apk";
        }
#else
        return ".ipa";
#endif
        }
    }
}
