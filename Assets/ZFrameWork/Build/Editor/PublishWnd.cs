using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ZFrameWork.Build;
using ZFrameWork;
using System.IO;

using UObj = UnityEngine.Object;

public class PublishWnd : EditorWindow
{
    [MenuItem("构建/构建")]
    public static void ShowWin()
    {
        var win = GetWindow<PublishWnd>();
        win.Show();
    }

    private void Awake()
    {

    }

    private void OnGUI()
    {
        if (GUILayout.Button("Publish"))
        {
            BuildScript.Publish();
        }

        if (GUILayout.Button("ClearBuild"))
        {
            ClearBuild();
        }

        if (GUILayout.Button("Debug"))
        {
            // Debug.Log(Application.persistentDataPath);
            // var list = new List<string>() { $"Lua/LuaFramework/ToLua/Lua" };
            var list = new List<string>() { $"Lua/Game", $"Lua/Core" };


            foreach (var file in list)
            {
                var p = $"{Application.dataPath}/{file}";
                Debug.Log(p);
                var ps = ZFileUtil.GetFiles(p, null, s => s.EndsWith(".txt"));
                foreach (var s in ps)
                {
                    Debug.Log(s);
                    var c = Path.ChangeExtension(s, "lua");
                    Debug.Log(c);
                    ZFileUtil.Move(s, c);

                }
            }
        }
    }

    void ClearBuild()
    {
        ZFileUtil.DelFile(FileConst.LocalVersionPath);
        ZFileUtil.DelFile(FileConst.AssetMd5Path);
        ZFileUtil.CheckDir(Application.persistentDataPath, true, true);
        ZFileUtil.CheckDir(FileConst.ServiceDataPath, true, true);
        ZFileUtil.CheckDir(FileConst.UpdateResPath, true, true);
        ZFileUtil.CheckDir(Application.streamingAssetsPath, true, true);
        AssetDatabase.Refresh();
    }
}
