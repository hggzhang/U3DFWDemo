using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZFrameWork;
using LuaInterface;

public class LuaUtil
{
    static public void LoadUIView(string res, LuaFunction func)
    {
        var op = AssetFacade.LoadAsset<GameObject>(res, (prefab) => {
            var go = GameObject.Instantiate(prefab);
            go.transform.SetParent(GameManager.Inst.transform);
            var view = go.GetComponent<UIView>();
            func.Call(view);
            func.Dispose();
        });
    }
}
