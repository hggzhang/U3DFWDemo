using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;
using LuaFramework;
using ZFrameWork;
using System.IO;


public class LuaLoader : SingleBase<LuaLoader>
{
    public byte[] LoadLuaBuffer(string file)
    {
        if (!file.EndsWith(".lua"))
        {
            file = file + ".lua";
        }
        var path = FindPath(file);
        return AssetFacade.LoadLuaBuffer(path);
    }

    public string FindPath(string file)
    {
        return ZFileUtil.CombinePath("Assets/Lua", file);
    }
}
