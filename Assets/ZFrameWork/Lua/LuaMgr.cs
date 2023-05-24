using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;
using LuaFramework;
using ZFrameWork;
using System.IO;

/*
Tolua的LuaFrameWork主要需要改下资源加载管理
Lua资源存放在统一的路径Lua下,不区分多个搜索目录，避免资源浪费
-Lua
 -/Core
 -/UI
    -/Main
        -/View
 -/Game
一、LuaManager->LuaMgr 
1.不管理Lua的bundle包
2.资源的加载路径唯一，不使用多个路径加载
3.接入Mgr的生命周期管理

二、LuaLoader
1.LuaLoader不再继承自LuaFileUtils，而继承自SingleBase
2.LuaLoader加载资源从AssetMgr中加载

 */

public class LuaMgr : MgrBase<LuaMgr>
{
    LuaState lua;
    LuaLooper loop = null;

    protected override void OnInit()
    {
        lua = new LuaState();
        this.OpenLibs();
        lua.LuaSetTop(0);

        LuaBinder.Bind(lua);
        DelegateFactory.Init();
        LuaCoroutine.Register(lua, GameManager.Inst);
        InitStart();
    }

    public void InitStart()
    {
        InitLuaPath();
        this.lua.Start();    //启动LUAVM
        this.StartMain();
        this.StartLooper();
    }

    void StartLooper()
    {
        loop = GameManager.Inst.gameObject.AddComponent<LuaLooper>();
        loop.luaState = lua;
    }

    //cjson 比较特殊，只new了一个table，没有注册库，这里注册一下
    protected void OpenCJson()
    {
        lua.LuaGetField(LuaIndexes.LUA_REGISTRYINDEX, "_LOADED");
        lua.OpenLibs(LuaDLL.luaopen_cjson);
        lua.LuaSetField(-2, "cjson");

        lua.OpenLibs(LuaDLL.luaopen_cjson_safe);
        lua.LuaSetField(-2, "cjson.safe");
    }

    void StartMain()
    {
        Debug.Log("-- StartMain");
        lua.DoFile("Core/Main.lua");
        LuaFunction main = lua.GetFunction("Main");
        main.Call();
        main.Dispose();
        main = null;
    }

    /// <summary>
    /// 初始化加载第三方库
    /// </summary>
    void OpenLibs()
    {
        lua.OpenLibs(LuaDLL.luaopen_pb);
        lua.OpenLibs(LuaDLL.luaopen_sproto_core);
        lua.OpenLibs(LuaDLL.luaopen_protobuf_c);
        lua.OpenLibs(LuaDLL.luaopen_lpeg);
        lua.OpenLibs(LuaDLL.luaopen_bit);
        lua.OpenLibs(LuaDLL.luaopen_socket_core);

        this.OpenCJson();
    }
    
    void InitLuaPath()
    {
    }

    public void DoFile(string filename)
    {
        lua.DoFile(filename);
    }

    // Update is called once per frame
    public object[] CallFunction(string funcName, params object[] args)
    {
        LuaFunction func = lua.GetFunction(funcName);
        if (func != null)
        {
            return func.LazyCall(args);
        }
        return null;
    }

    public void GC()
    {
        lua.LuaGC(LuaGCOptions.LUA_GCCOLLECT);
    }

    protected override void OnShutdown()
    {
        loop.Destroy();
        loop = null;
        lua.Dispose();
        lua = null;
    }
}
