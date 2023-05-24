using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZFrameWork;
using UnityEditor;
using LuaFramework;
using LuaInterface;

public class UIView : MonoBehaviour
{
    Dictionary<string, Transform> cache = new Dictionary<string, Transform>();
    Dictionary<string, LuaFunction> luaFuncDict = new Dictionary<string, LuaFunction>();
    AssetLoader loader = new AssetLoader();
    List<UIView> childViews;
    public string luaFile = "";
    LuaTable luaView;
    public void BindLuaView(LuaTable luaView)
    {
        UnBindLuaView();
        this.luaView = luaView;
        this.childViews = GetChildViews();
        foreach (var childView in childViews)
        {
            var luaFile = childView.GetLuaFile();
            luaView.Call("AddChildView", luaView, childView, luaFile);
        }
    }

    public void UnBindLuaView()
    {
        if (luaView == null)
            return;
        luaView.Call("RemoveAllChildView");
        luaView.Dispose();
        luaView = null;
    }

    public string GetLuaFile()
    {
        return luaFile;
    }

    public LuaTable GetLuaView()
    {
        return luaView;
    }
    public List<UIView> GetChildViews()
    {
        childViews = new List<UIView>(GetComponentsInChildren<UIView>());
        childViews.Remove(this);
        return childViews;
    }

    public void GC()
    {
        // loader.ReleaseAll();
        /*foreach (var pair in luaFuncDict)
            pair.Value.Dispose();
        luaFuncDict.Clear();*/
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public T Find<T>(string widget)
    {
        Transform trans = null;
        if (gameObject.name == widget)
            trans = transform;
        else
            trans = FindChild(widget);
        if (trans == null)
            return default(T);
        return trans.GetComponent<T>();
    }

    public Transform FindChild(string name)
    {
        if (cache.ContainsKey(name))
            return cache[name];

        var trans = transform;
        var paths = name.Split("/");
        for (int i = 0; i < paths.Length; i++)
        {
            var path = paths[i];
            if (trans != null)
                trans = trans.Find(path);
        }
        if (trans != null)
            cache[name] = trans;

        return trans;
    }

    /*Operation*/
    public void AddClick(string widget, LuaFunction luaFunc)
    {
        var key = widget + "Click";
        if (luaFuncDict.ContainsKey(key))
            return;
        var btn = Find<Button>(widget);
        if (btn == null)
            return;
        btn.onClick.AddListener(() => { luaFunc.Call(); });
        luaFuncDict[key] = luaFunc;
    }

    public void RemoveClick(string widget)
    {
        var key = widget + "Click";
        if (luaFuncDict.ContainsKey(key))
        {
            luaFuncDict[key].Dispose();
            luaFuncDict[key] = null;
        }
    }

    public void SetImage(string widget, string res)
    {
        var image = Find<Image>(widget);
        if (image == null)
            return;

        var key = widget + "Image";
        loader.LoadAsset<Sprite>(key, res, (sp) =>
        {
            image.sprite = sp;
        });
    }

    public void SetText(string widget, string str)
    {
        var text = Find<Text>(widget);
        if (text != null)
            text.text = str;
    }

    public string GetText(string widget)
    {
        var text = Find<Text>(widget);
        if (text != null)
        {
            return text.text;
        }

        var inf = Find<InputField>(widget);
        if (inf != null)
        {
            return inf.text;
        }

        return "";
    }

    public void SetVisible(string widget, bool v)
    {
        var c = FindChild(widget);
        c?.gameObject.SetActive(v);
    }
}
