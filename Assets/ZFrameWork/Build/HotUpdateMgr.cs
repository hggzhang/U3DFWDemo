using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LitJson;
using System.IO;
using UnityEditor;
using System;


// 检查下载了但是没有解压的文件
namespace ZFrameWork
{

    public class HotUpdateMgr : MgrBase<HotUpdateMgr>
    {
        class UpdateTask
        {
            string ver;
            string url;
            string savePath;
            Action<EDownloadErr, string> onErr;
            Action<long, long> onProgress;
            Action<string> onComp;
            Downloader du;

            public UpdateTask(string url, string ver, Action<EDownloadErr, string> onErr, Action<string> onComp, Action<long, long> onProgress)
            {
                this.url = url;
                this.ver = ver;
                savePath = Path.Combine(Application.persistentDataPath, $"Res_{ver}.zip");
                this.onErr = onErr;
                this.onComp = onComp;
                this.onProgress = onProgress;
                du = new Downloader(url, savePath, Err, Comp, Prog);
            }

            public void Start()
            {
                du.Start(10);
            }

            public void Clear()
            {
                du.Dispose();
                du = null;
            }

            void Err(EDownloadErr e, string code)
            {
                onErr?.Invoke(e, code);
            }

            void Prog(long done, long total)
            {
                onProgress?.Invoke(done, total);
            }

            void Comp(string code)
            {
                if (!UnZip())
                    return;
                VersionUtil.SaveResVersionToRemote(ver);
                Clear();
                onComp?.Invoke(code);
            }

            bool UnZip()
            {
                Prog(0, 0);
                if (!ZFileUtil.CheckFile(savePath))
                {
                    Err(EDownloadErr.UnZipFileMiss, "Unzip file miss");
                    return false;
                }
                ZFileUtil.UnZipFile(savePath, FileConst.AssetBundleRemotePath, true);
                return true;
            }
        }

        Queue<UpdateTask> downloadQue;
        string savePath;
        Downloader du;
        UpdateTask workTask;

        // Start is called before the first frame update
        public void CheckUpdate()
        {
            var json = ReqHotUpdateJson();
            if (json == null)
            {
                // OnError(EDownloadErr.ReqUpdateFileFailed, "Req service json failed");
                LuaEvent.Call("HotUpdate Completed");
                return;
            }

            if (!json.ContainsKey("res_version"))
            {
                OnError(EDownloadErr.ReqUpdateFileFailed, "json res_version field not exist");
                return;
            }

            var resVersion = json["res_version"].ToString();
            var ver = VersionUtil.GetRemoteResVersion();
            var updateVersions = VersionUtil.GetUpdateVersions(ver, resVersion);

            foreach (var updateVersion in updateVersions)
            {
                var url = Path.Combine(FileConst.ServiceDataPath, $"Res_{updateVersion}.zip");
                var task = new UpdateTask(url,updateVersion,OnError,OnComplete,OnProgress);
                downloadQue.Enqueue(task);
            }

            // Debug.Log($"检查到{downloadQue.Count}个资源");
            LuaEvent.Call("HotUpdate Check", downloadQue.Count);
        }

        public void StartDownLoad()
        {
            if (downloadQue.Count > 0)
            {
                workTask = downloadQue.Dequeue();
                workTask.Start();
            }    
        }

        protected override void OnInit()
        {
            downloadQue = new Queue<UpdateTask>();
        }

        void Clear()
        {
            while(downloadQue.Count > 0)
            {
                var task = downloadQue.Dequeue();
                task.Clear();
            }
            workTask = null;
        }

        JsonData ReqHotUpdateJson()
        {
            UnityWebRequest uwr = UnityWebRequest.Get(Path.Combine(FileConst.ServiceDataPath, FileConst.HotUpdateJsonFileName));
            var request = uwr.SendWebRequest();
            while (!request.isDone) { }
            if (!string.IsNullOrEmpty(uwr.error))
            {
                Debug.LogError(uwr.error);
                return null;
            }

            var json = JsonMapper.ToObject(uwr.downloadHandler.text);
            return json;
        }

        void OnError(EDownloadErr e, string code)
        {
            Clear();
            Debug.LogError(code);
        }

        void OnProgress(long done, long total)
        {
            LuaEvent.Call("HotUpdate Update", (int)done, (int)total);
        }

        void OnComplete(string code)
        {
            if (downloadQue.Count == 0)
            {
                LuaEvent.Call("HotUpdate Completed");
                Clear();
            }
            else
            {
                workTask = downloadQue.Dequeue();
                workTask.Start();
            }
        }
    }
}


