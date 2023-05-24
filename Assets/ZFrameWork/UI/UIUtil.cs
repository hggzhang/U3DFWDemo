using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZFrameWork;
using UnityEditor;
using LuaFramework;
using LuaInterface;

public class UIUtil
{
    static public ListView GetListView(UIView view, string path)
    {
        return view.Find<ListView>(path);
    }
}
