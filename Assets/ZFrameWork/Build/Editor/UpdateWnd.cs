using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ZFrameWork.Build;
using ZFrameWork;

using UObj = UnityEngine.Object;
using ZFileUtil = ZFrameWork.ZFileUtil;

public class UpdateWnd : EditorWindow
{
    List<UObj> updateResObjs = new List<UObj>();
    [MenuItem("构建/更新包")]
    public static void ShowWin()
    {
        var win = GetWindow<UpdateWnd>();
        win.Show();
    }

    private void Awake()
    {

    }

    private void OnGUI()
    {
        if (GUILayout.Button("MakeTestUpdateAsset"))
        {
            var l = ZFileUtil.U3DAssetPath($"Assets/GameRes/Image/touxiang1.png");
            var r = ZFileUtil.U3DAssetPath($"Assets/GameRes/Image/touxiang2.png");
            var t = ZFileUtil.U3DAssetPath($"Assets/GameRes/Image/_temp");

            ZFileUtil.Move(l, t);
            ZFileUtil.Move(r, l);
            ZFileUtil.Move(t, r);
            AssetDatabase.Refresh();
        }

        if (GUILayout.Button("UpdatePacket"))
        {
            UpdateBuildScript.HotUpdate(updateResObjs);
        }

        if (GUILayout.Button("检测更新资产"))
        {
            CheckUpdateAsset();
        }

        DrawUpdateResObjs();
    }

    void CheckUpdateAsset()
    {
        updateResObjs.Clear();
        var updates = UpdateBuildScript.GatherUpdateFile();
        if (updates != null)
            for (int i = 0; i < updates.Count; i++)
                updateResObjs.Add(AssetDatabase.LoadAssetAtPath(updates[i], typeof(UObj)));
    }

    void DrawUpdateResObjs()
    {
        GUILayout.BeginScrollView(Vector2.zero);
        if (GUILayout.Button("+"))
            updateResObjs.Add(null);
        for (int i = 0; i < updateResObjs.Count; i++)
        {
            GUILayout.BeginHorizontal();
            updateResObjs[i] = EditorGUILayout.ObjectField(updateResObjs[i], typeof(UObj), false);
            if (GUILayout.Button("-"))
            {
                updateResObjs.RemoveAt(i);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
    }
}
