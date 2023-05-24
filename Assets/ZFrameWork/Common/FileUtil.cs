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
using System.IO.Compression;


namespace ZFrameWork
{
    public class ZFileUtil
    {
        static public string CombinePath(string s1, string s2, params string[] sn)
        {
            StringBuilder sb = new StringBuilder(s1);
            sb.Append("/");
            sb.Append(s2);
            for (int i = 0; i < sn.Length; i++)
            {
                sb.Append("/");
                sb.Append(sn[i]);
            }

            return sb.ToString();
        }

        static public string NormPath(string path)
        {
            return path.Replace("\\", "/");
        }

        static public string SubPathFromAssets(string path)
        {
            return path.Substring(path.IndexOf("Assets"));
        }

        public delegate string FileCorrFunc(string s);
        public delegate bool FilePredFunc(string s);
        #region Files Gather
        static public List<string> GetFiles(string path, FileCorrFunc corr = null, FilePredFunc pred = null)
        {
            var ret = new List<string>();
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                if (pred != null)
                    if (!pred(file))
                        continue;
                ret.Add((corr == null ? file : corr(file)).Replace("\\","/"));
            }
            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
                ret.AddRange(GetFiles(dir, corr, pred));
            return ret;
        }

        static public List<string> GetFiles(IEnumerable<string> paths, FileCorrFunc corr = null, FilePredFunc pred = null)
        {
            var ret = new List<string>();
            foreach (var path in paths)
                ret.AddRange(GetFiles(path, corr, pred));
            return ret;
        }

        static public string Md5file(string file)
        {
            try
            {
                FileStream fs = new FileStream(file, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(fs);
                fs.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("md5file() fail, error:" + ex.Message);
            }
        }

        // flie = Assets/xx/xx...
        static public JsonData GenFilesMd5Json(IEnumerable<string> files)
        {
            var json = new JsonData();
            foreach (var file in files)
            {
                var fullname = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.IndexOf("Assets")), file);
                json[file] = Md5file(fullname);
            }

            return json;
        }
        #endregion

        #region Dir File Operator
        public static bool CheckFile(string path, bool isCreate = false)
        {
            if (!File.Exists(path))
            {
                if (isCreate)
                {
                    File.Create(path);
                }
                return false;
            }

            return true;
        }

        public static void DelFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        public static bool CheckDir(string path, bool isCreate = false, bool isDeleteContent = false)
        {
            if (!Directory.Exists(path))
            {
                if (isCreate)
                {
                    Directory.CreateDirectory(path);
                }
                return false;
            }

            if (isDeleteContent)
            {
                Directory.Delete(path, true);
                Directory.CreateDirectory(path);
            }


            return true;
        }

        public static void DelDir(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        public static void CreateDir(string path, bool isDeleteContent)
        {
            if (Directory.Exists(path))
            {
                if (isDeleteContent)
                {
                    Directory.Delete(path, true);
                    Directory.CreateDirectory(path);
                }

                return;
            }

            Directory.CreateDirectory(path);
        }

        public static void WriteFile(string path, string content)
        {
            var dir = Path.GetDirectoryName(path);
            CheckDir(dir, true);
            using (var s = File.Open(path,FileMode.OpenOrCreate))
            {
                s.Seek(0, SeekOrigin.Begin);
                s.SetLength(0);
                var w = new StreamWriter(s);
                w.Write(content);
                w.Close();
            }
        }

        public static void WriteFile(string path, byte[] content)
        {
            var dir = Path.GetDirectoryName(path);
            CheckDir(dir, true);
            using (var s = File.Open(path, FileMode.OpenOrCreate))
            {
                s.Seek(0, SeekOrigin.Begin);
                s.SetLength(0);
                s.Write(content, 0, content.Length);
                s.Close();
            }
        }

        public static string ReadFileToText(string path)
        {
            if (!CheckFile(path))
                return null;
            using (var s = File.Open(path, FileMode.Open))
            {
                var r = new StreamReader(s);
                var str = r.ReadToEnd();
                r.Close();
                return str;
            }
        }

        public static void WriteJsonFile(string path, JsonData json)
        {
            if (File.Exists(path))
                File.Delete(path);
            WriteFile(path, json.ToJson());
        }

        public static JsonData ReadFileToJson(string path)
        {
            var content = ReadFileToText(path);
            if (content != null)
            {
                return JsonMapper.ToObject(content);
            }
            return null;
        }

        public static void ZipFiles(string path,string flieName, string[] files)
        {
            DelFile(Path.Combine(path, flieName));
            CheckDir(Path.Combine(path, "temp"), true);
            foreach (var file in files)
                File.Copy(file, Path.Combine(path, "temp", Path.GetFileName(file)));
            ZipFile.CreateFromDirectory(Path.Combine(path, "temp"), Path.Combine(path, flieName));
            DelDir(Path.Combine(path, "temp"));
        }

        public static void UnZipFile(string filePath, string savePath, bool isDeleteFile = false)
        {
            if (!CheckFile(filePath))
                return;
            CheckDir(savePath, true);
            ZipFile.ExtractToDirectory(filePath, savePath, true);
            if (isDeleteFile)
                DelFile(filePath);
        }

        public static string U3DAssetPath(string ap)
        {
            return Path.Combine(Application.dataPath.Substring(0, Application.dataPath.IndexOf("Assets")), ap);
        }

        public static void Move(string r, string d)
        {
            File.Move(r, d);
        }
        #endregion
    }
}
