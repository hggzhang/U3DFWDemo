using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaFramework;
using LuaInterface;
using LitJson;

public class LuaEvent
{
    public static void Call(string e)
    {
        LuaMgr.Inst.CallFunction("CSCallEve", e);
    }

    public static void Call<P1>(string e, P1 p1)
    {
        LuaMgr.Inst.CallFunction("CSCallEve", e, p1);
    }

    public static void Call<P1, P2>(string e, P1 p1, P2 p2)
    {
        LuaMgr.Inst.CallFunction("CSCallEve", e, p1, p2);
    }

    public static void NetMsg(MsgBase msg)
    {
        var code = JsonMapper.ToJson(msg);
        LuaMgr.Inst.CallFunction("CSNetMsg", code);
    }
}
