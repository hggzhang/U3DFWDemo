using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZFrameWork;
using LuaInterface;
public class LuaCallCS
{
    public static void Call(string code, object param)
    {
        switch (code)
        {
            case "CheckHotUpdate":
                HotUpdateMgr.Inst.CheckUpdate();
                break;
            case "StartHotUpdate":
                HotUpdateMgr.Inst.StartDownLoad();
                break;
            case "SendNetMsg":
                var p = param as LuaTable;
                if (p != null)
                {
                    var msgName = p["msg"] as string;
                    if (msgName != null)
                    {
                        if (msgName == "MsgLogin")
                        {
                            var msgBody = p["msgBody"] as LuaTable;
                            if (msgBody != null)
                            {
                                var user = (string)msgBody["user"];
                                var pw = (string)msgBody["password"];
                                var msg = new MsgLogin();
                                msg.user = user;
                                msg.password = pw;
                                NetMgr.Inst.Send(msg);
                                //LuaEvent.NetMsg(msg);
                            }
                        }
                    }
                }
                break;
            default:
                break;
        }
    }
}
