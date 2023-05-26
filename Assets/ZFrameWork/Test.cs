using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using ZFrameWork;
using UnityEngine.Profiling;
using UnityEngine.UI;

using UObj = UnityEngine.Object;

public class Test : MonoBehaviour
{
    private void Start()
    {
        AssetFacade.Init();
    }

    AssetOp spOp;
    AssetOp objOp;
    GameObject go;
    AssetLoader loader = new AssetLoader();

    bool isLogin = false;
    private void OnGUI()
    {
        /*if (GUI.Button(new Rect(100,100,200,50),"op gc"))
        {
            //go.transform.GetChild(0).GetComponent<Image>().sprite = null;

            AssetFacade.ReleaseAsset(spOp);
            AssetFacade.ReleaseAsset(objOp);

            AssetFacade.GC();
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
        }

        if (GUI.Button(new Rect(100, 300, 200, 50), "op load"))
        {
            var goPath = @"Assets\GameRes\UI\MainUI.prefab";
            *//*AssetMgr.Inst.LoadAsset<GameObject>(goPath, (text) =>
            {
                var go = GameObject.Instantiate(text, transform);
            });*//*

            objOp = AssetFacade.LoadAsset<GameObject>(goPath, (prefab) =>
            {
                go = GameObject.Instantiate(prefab, transform);

                spOp = AssetFacade.LoadAsset<Sprite>(@"Assets\GameRes\Image\touxiang.png", (sp) =>
                {
                    go.transform.GetChild(0).GetComponent<Image>().sprite = sp;
                });

            });
        }

        if (GUI.Button(new Rect(100, 500, 200, 50), "loader gc"))
        {
            loader.ReleaseAll();
            AssetFacade.GC();
            //System.GC.Collect();
        }

        if (GUI.Button(new Rect(100, 700, 200, 50), "loader load"))
        {
            var goPath = @"Assets\GameRes\UI\MainUI.prefab";

            loader.LoadAsset<GameObject>(goPath, (prefab) =>
            {
                go = GameObject.Instantiate(prefab, transform);
                var image = go.transform.GetChild(0).GetComponent<Image>();
                loader.LoadAsset<Sprite>(image, @"Assets\GameRes\Image\touxiang2.png", (sp) =>
                {
                    image.sprite = sp;
                });

            });
        }

        if (GUI.Button(new Rect(500, 500, 200, 50), "�����Դ����"))
        {
            HotUpdateMgr.Inst.Init();
            HotUpdateMgr.Inst.CheckUpdate();
        }

        if (GUI.Button(new Rect(500, 700, 200, 50), "��ʼ����"))
        {
            HotUpdateMgr.Inst.StartDownLoad();
        }*/


        // if (GUI.Button(new Rect(100, 500, 200, 50), "��ʼ�� Net"))
        // {
        //     NetMgr.Inst.Init();
        //     NetMgr.Inst.Begin();
        // }

        // if (!isLogin)
        // {
        //     if (GUI.Button(new Rect(100, 700, 200, 50), "������Ϣ����"))
        //     {
        //         var msg = new MsgLogin();
        //         var str = JsonUtility.ToJson(msg);
        //         Debug.Log("Send = " + str);
        //         NetMgr.Inst.AddNetMsgCB("MsgLoginNtf", (MsgBase msg) =>
        //         {
        //             var ntfMsg = (MsgLoginNtf)msg;
        //             if (ntfMsg != null)
        //             {
        //                 var code = ntfMsg.code;
        //                 ZLogger.Log("msg handle: code = " + code);
        //                 isLogin = code == 1;
        //             }
        //         });
        //         NetMgr.Inst.Send(msg);
        //     }
        // }
        // else
        //     GUI.Label(new Rect(100, 700, 200, 50), "��¼�ɹ�");
    }


}
