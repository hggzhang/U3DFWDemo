
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ZFrameWork.Build;
using ZFrameWork;
using System.IO;
using LitJson;

using ZFileUtil = ZFrameWork.ZFileUtil;

using UObj = UnityEngine.Object;

public class BundleSchemaWnd : EditorWindow
{
    List<UObj> editDirs = new List<UObj>();
    List<string> editPreviewDirs = new List<string>();
    HashSet<string> bundleKeys = new HashSet<string>();

    bool isSelf = true;
    bool isSub = false;
    bool isShowBundleKey = false;
    bool isAdd = false;

    [MenuItem("构建/分包")]
    public static void ShowWin()
    {
        var win = GetWindow<BundleSchemaWnd>();
        win.minSize = new Vector2(800,600);
        win.FlushBundleKeys();
        win.isAdd = true;
        win.isSub = true;
        win.Show();
    }

    private void Awake()
    {

    }

    private void OnGUI()
    {
        DrawWnd();
    }

    void DrawWnd()
    {
        GUILayout.BeginHorizontal(GUILayout.MinWidth(500f));
        DrawEditSchameWnd();
        GUILayout.Space(10);
        DrawPreviewWnd();
        GUILayout.EndHorizontal();
    }

    GUIStyle editPreviewStyle = new GUIStyle();

    void DrawEditSchameWnd()
    {
        GUILayout.BeginVertical();
        GUILayout.Label("设置");
        isSelf = GUILayout.Toggle(isSelf, "仅自己");
        isSub = GUILayout.Toggle(isSub, "子文件夹");
        isAdd = GUILayout.Toggle(isAdd, "添加/删除");

        GUILayout.BeginScrollView(Vector2.zero);
        if (GUILayout.Button("+"))
        {
            editDirs.Add(null);
        }
        for (int i = 0; i < editDirs.Count; i++)
        {
            GUILayout.BeginHorizontal();
            editDirs[i] = EditorGUILayout.ObjectField(editDirs[i], typeof(UObj), false);
            if (GUILayout.Button("-"))
            {
                editDirs.RemoveAt(i);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();

        GUILayout.Label("新增列表");
        GUILayout.BeginScrollView(Vector2.zero);
        GatherEditPreview();
        editPreviewStyle.normal.textColor = isAdd ? Color.green : Color.red;
        for (int i = 0; i < editPreviewDirs.Count; i++)
        {
            GUILayout.Label(editPreviewDirs[i], editPreviewStyle);
        }
        GUILayout.EndScrollView();


        if (GUILayout.Button("保存"))
        {
            SaveChangeKeys();
            FlushBundleKeys();
        }
        GUILayout.EndVertical();
    }

    void GatherEditPreview()
    {
        editPreviewDirs.Clear();
        foreach (var editDir in editDirs)
        {
            if (editDir == null)
                continue;
            var path = AssetDatabase.GetAssetPath(editDir);
            if (isSelf)
            {
                editPreviewDirs.Add(path);
            }
            if (isSub)
            {
                var subDirs = Directory.GetDirectories(path.Replace("Assets", Application.dataPath));
                foreach (var subDir in subDirs)
                {
                    editPreviewDirs.Add(ZFrameWork.ZFileUtil.SubPathFromAssets(subDir).Replace("\\","/"));
                }
            }
        }
    }

    void DrawPreviewWnd()
    {
        GUILayout.BeginVertical();
        GUILayout.Label("列表");
        if (GUILayout.Button("刷新"))
        {
            FlushBundleKeys();
        }
        isShowBundleKey = GUILayout.Toggle(isShowBundleKey, "显示BundleKey");

        GUILayout.BeginScrollView(Vector2.zero);
        var keys = new string[bundleKeys.Count];
        bundleKeys.CopyTo(keys);
        for (int i = 0; i < keys.Length; i++)
        {
            var key = isShowBundleKey ? AssetUtil.DirPath2BundleKey(keys[i]) : keys[i];
            GUILayout.Label(key);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    void FlushBundleKeys()
    {
        if (!ZFileUtil.CheckFile(FileConst.BuildBundleKeyFilePath))
        {
            ZFileUtil.WriteJsonFile(FileConst.BuildBundleKeyFilePath, new JsonData());
        }
        bundleKeys.Clear();
        var json = ZFileUtil.ReadFileToJson(FileConst.BuildBundleKeyFilePath);
        if (json != null && json.IsArray)
        {
            for (int i = 0; i < json.Count; i++)
            {
                bundleKeys.Add((string)json[i]);
            }
        }
    }

    void SaveChangeKeys()
    {
        if (isAdd)
            foreach (var key in editPreviewDirs)
                bundleKeys.Add(key);
        else
            foreach (var key in editPreviewDirs)
                bundleKeys.Remove(key);
        
        editDirs.Clear();
        editPreviewDirs.Clear();

        var keys = new string[bundleKeys.Count];
        bundleKeys.CopyTo(keys);

        var jsonStr = JsonMapper.ToJson(keys);
        var json = JsonMapper.ToObject(jsonStr);
        ZFileUtil.WriteJsonFile(FileConst.BuildBundleKeyFilePath, json);
    }

}
