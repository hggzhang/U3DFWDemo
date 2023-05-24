using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListView : MonoBehaviour
{
    [SerializeField]
    GameObject childProfab;

    ScrollRect scrollRect;
    RectTransform content;

    List<GameObject> childern = new List<GameObject>();

    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content;
    }
    // Start is called before the first frame update
    void Start()
    {
        //
    }

    GameObject FindPool()
    {
        GameObject go = GameObjPoolMgr.Inst.FindItem(childProfab);
        if (go == null)
            go = Instantiate(childProfab);
        go.transform.SetParent(content);
        return go;
    }

    void FreePool(GameObject go)
    {
        GameObjPoolMgr.Inst.FreeItem(childProfab, go);
    }

    public void UpdateView(int cnt)
    {
        int delta = cnt - childern.Count;
        if (delta > 0)
        {
            for (int i = 0; i < delta; i++)
            {
                var go = FindPool();
                childern.Add(go);
            }
        }
        else
        {
            delta = -delta;
            for (int i = 0; i < delta; i++)
            {
                var go = childern[childern.Count - 1];
                FreePool(go);
                childern.RemoveAt(childern.Count - 1);
            }
        }

        /*List<UIView> ret = new List<UIView>();
        foreach (var child in childern)
            ret.Add(child.GetComponent<UIView>());
        return ret;*/
    }

    public List<GameObject> GetChildern()
    {
        return childern;
    }

    public List<UIView> GetChildViews()
    {
        List<UIView> ret = new List<UIView>();
        for (int i = 0; i < childern.Count; i++)
            ret.Add(childern[i].GetComponent<UIView>());

        return ret;
    }

    void OnDestroy()
    {
        GameObjPoolMgr.Inst.ClearItems(childProfab);       
    }
}
