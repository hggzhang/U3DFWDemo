using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMgr
{
    void Init();
    void Begin();
    void End();
    void Shutdown();
}

public class MgrBase<T> : SingleBase<T>, IMgr where T : class, new()
{
    public void Begin()
    {
        OnBegin();
    }

    public void End()
    {
        OnEnd();
    }

    public void Init()
    {
        OnInit();
    }

    public void Shutdown()
    {
        OnShutdown();
    }

    virtual protected void OnBegin()
    {
        //
    }

    virtual protected void OnInit()
    {
        //
    }

    virtual protected void OnEnd()
    {
        //
    }

    virtual protected void OnShutdown()
    {
        //
    }
}

public class MonoMgrBase<T> : MonoBehaviour, IMgr
{
    protected static T instance;

    public MonoMgrBase() { }
    
    public static T Inst
    {
        get { return instance; }
    }

    public void Begin()
    {
        OnBegin();
    }

    public void End()
    {
        OnEnd();
    }

    public void Init()
    {
        OnInit();
    }

    public void Shutdown()
    {
        OnShutdown();
    }

    virtual protected void OnBegin()
    {
        //
    }

    virtual protected void OnInit()
    {
        //
    }

    virtual protected void OnEnd()
    {
        //
    }

    virtual protected void OnShutdown()
    {
        //
    }
}

