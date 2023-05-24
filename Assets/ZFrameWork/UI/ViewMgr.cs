using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZFrameWork;
using UnityEditor;
using LuaFramework;
using LuaInterface;


public class ViewMgr : MgrBase<ViewMgr> 
{
    AssetLoader loader = new AssetLoader();

    public void LoadView(string res, LuaFunction func)
    {
        loader.LoadAsset<GameObject>(res, (prefab) => {
            var go = GameObject.Instantiate(prefab, GameManager.Inst.transform);
            var rectTrams = go.GetComponent<RectTransform>();
            rectTrams.localPosition = Vector3.zero;
            var view = go.GetComponent<UIView>();
            func.Call(view);
            func.Dispose();
        });
    }
}
