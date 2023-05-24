using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZFrameWork;
using UnityEditor;
using LuaFramework;
using LuaInterface;


public class GameObjPoolMgr : MonoMgrBase<GameObjPoolMgr>
{
    Dictionary<object, Queue<GameObject>> dict = new Dictionary<object, Queue<GameObject>>();
    Transform content;

    void Awake()
    {
        instance = this;
    }

    protected override void OnBegin()
    {
        
    }

    protected override void OnEnd()
    {
        
    }

    protected override void OnInit()
    {
        var go = new GameObject();
        content = go.transform;
        content.SetParent(GameManager.Inst.transform);
    }

    protected override void OnShutdown()
    {
        foreach (var pair in dict)
            foreach (var item in pair.Value)
                Destroy(item);
        Destroy(content.gameObject);
    }

    
    public void FreeItem(object k, GameObject item)
    {
        item.SetActive(false);
        if (!dict.ContainsKey(k))
            dict[k] = new Queue<GameObject>();
        
        dict[k].Enqueue(item);
    }

    public GameObject FindItem(object k)
    {
        if (!dict.ContainsKey(k))
            return null;
        if (dict[k].Count == 0)
            return null;
        var item = dict[k].Dequeue();
        item.SetActive(true);
        return item;
    }

    public void ClearItems(object k)
    {
        if (!dict.ContainsKey(k))
            return;
        var que = dict[k];
        foreach (var item in que)
            Destroy(item);
        dict.Remove(k);
    }
}
