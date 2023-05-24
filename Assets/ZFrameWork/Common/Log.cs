using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class ZLogger
{
    static public void Log(params object[] objs)
    {
        var s = "";
        foreach (var obj in objs)
            s = $"{s}{obj}";
        Debug.Log(s);
    }

    static public void Err(params object[] objs)
    {
        var s = "";
        foreach (var obj in objs)
            s = $"{s}{obj}";
        Debug.Log(s);
    }

    static public void Warn(params object[] objs)
    {
        var s = "";
        foreach (var obj in objs)
            s = $"{s}{obj}";
        Debug.Log(s);
    }
}

