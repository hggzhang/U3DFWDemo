using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;


public enum EDownloadErr
{
    ContentEmpty = 0,
    TempFileMiss = 1,
    UnZipFileMiss = 2,
    ReqUpdateFileFailed = 3,
}

public class DownloadBreakResumeHandle : DownloadHandlerScript
{
    string savePath;
    string tempPath;
    string url;
    long doneLen;
    long lastLen;
    long totalLen;

    Action<EDownloadErr, string> onError;
    Action<string> onComplete;
    Action<long, long> onProgress;

    FileStream fs;

    public long DoneLen { get { return doneLen; } }

    public DownloadBreakResumeHandle(string url, string savePath, Action<EDownloadErr, string> Err, Action<string> Comp, Action<long, long> Prog)
    {
        this.url = url;
        this.savePath = savePath.Replace("\\","/");
        tempPath = $"{savePath}.temp";
        onError = Err;
        onComplete = Comp;
        onProgress = Prog;
        fs = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write);
        doneLen = fs.Length;
        fs.Position = doneLen;
    }


    protected override void ReceiveContentLengthHeader(ulong contentLength)
    {
        lastLen = (long)contentLength;
        totalLen = lastLen + doneLen;
    }

    protected override bool ReceiveData(byte[] data, int dataLength)
    {
        if (lastLen < 0 || data == null || data.Length <= 0) return false;
        fs.Write(data, 0, dataLength);
        doneLen += dataLength;
        onProgress?.Invoke(doneLen, totalLen);
        return true;
    }

    protected override void CompleteContent()
    {
        Close();
        if (doneLen <= 0)
        {
            onError?.Invoke(EDownloadErr.ContentEmpty, "Download len is 0");
            return;
        }

        if (!File.Exists(tempPath))
        {
            onError?.Invoke(EDownloadErr.TempFileMiss, "Temp file miss");
            return;
        }

        if (File.Exists(savePath)) File.Delete(savePath);
        File.Move(tempPath, savePath);
        onComplete?.Invoke("Download complete");
    }

    public void Close()
    {
        if (fs == null) return;
        fs.Dispose();
        fs.Close();
        fs = null;
    }
}

public class Downloader
{
    string url;
    string savePath;

    Action<EDownloadErr, string> onError;
    Action<string> onComplete;
    Action<long, long> onProgress;

    UnityWebRequest req;

    public Downloader(string url, string savePath, Action<EDownloadErr, string> Err, Action<string> Comp, Action<long, long> Prog)
    {
        this.url = url;
        this.savePath = savePath;
        onError = Err;
        onComplete = Comp;
        onProgress = Prog;
    }

    public void Start(int timeOut)
    {
        req = UnityWebRequest.Get(url);
        req.timeout = timeOut;
        req.disposeDownloadHandlerOnDispose = true;
        var handle = new DownloadBreakResumeHandle(url, savePath, onError, onComplete, onProgress);
        req.SetRequestHeader("range", $"bytes={handle.DoneLen}-");
        req.downloadHandler = handle;
        req.SendWebRequest();
    }

    public void Dispose()
    {
        onError = null;
        onComplete = null;
        onProgress = null;
        if (req != null)
        {
            if (!req.isDone) req.Abort();
            req.Dispose();
            req = null;
        }
    }
}


