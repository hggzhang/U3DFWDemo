using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZFrameWork;

public enum EMgrLife
{
    Game = 1,
    Player = 2,
}

public class MgrRegister
{
    public static void RegGameMgr()
    {
        EMgrLife e = EMgrLife.Game;
        GameManager.Inst.RegMgr(LuaMgr.Inst, e);
        GameManager.Inst.RegMgr(ViewMgr.Inst, e);
        GameManager.Inst.RegMgr(HotUpdateMgr.Inst, e);
        GameManager.Inst.RegMgr(NetMgr.Inst, e);

        GameManager.Inst.RegMonoMgr<TestMonoMgr>(e);
        GameManager.Inst.RegMonoMgr<GameObjPoolMgr>(e);
    }

    public static void RegPlayerMgr()
    {
        //
    }
}

public class GameManager : MonoBehaviour
{
    static GameManager inst = null;
    public static GameManager Inst { get { return inst; } }
    public Dictionary<EMgrLife, List<IMgr>> mgrDict = new Dictionary<EMgrLife, List<IMgr>>();

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        inst = this;
        Init();
    }

    public void RegMgr(IMgr mgr, EMgrLife e)
    {
        if (!mgrDict.ContainsKey(e))
            mgrDict[e] = new List<IMgr>();
        mgrDict[e].Add(mgr);
    }

    public void RegMonoMgr<T>(EMgrLife e) where T : MonoMgrBase<T>
    {
        gameObject.AddComponent<T>();
        RegMgr(MonoMgrBase<T>.Inst, e);
    }

    public void InitMgr(EMgrLife e)
    {
        if (mgrDict.ContainsKey(e))
        {
            foreach (var mgr in mgrDict[e])
                mgr.Init();
            foreach (var mgr in mgrDict[e])
                mgr.Begin();
        }
    }

    public void ShutDownMgr(EMgrLife e)
    {
        if (mgrDict.ContainsKey(e))
        {
            foreach (var mgr in mgrDict[e])
                mgr.End();
            foreach (var mgr in mgrDict[e])
                mgr.Shutdown();
        }
    }

    void Init()
    {
        AssetFacade.Init();
        MgrRegister.RegGameMgr();
        MgrRegister.RegPlayerMgr();
        InitMgr(EMgrLife.Game);
    }

    void Update()
    {
        NetMgr.Inst.Update();
    }

    void HotReload()
    {
        AssetFacade.HotReload();
        InitMgr(EMgrLife.Game);
    }

    void OnLogin()
    {
        InitMgr(EMgrLife.Player);
    }

    void OnLogout()
    {
        ShutDownMgr(EMgrLife.Player);
    }
    void Shutdown()
    {
        ShutDownMgr(EMgrLife.Game);
        AssetFacade.Shutdown();
    }
}

