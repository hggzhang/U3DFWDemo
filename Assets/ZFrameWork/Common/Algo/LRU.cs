using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LRUBase<TK, TV>
{
    protected ulong size = 0;
    protected ulong count = 0;
    protected List<TK> list = new List<TK>();
    protected Dictionary<TK, TV> dict = new Dictionary<TK, TV>();

    public LRUBase(ulong size)
    {
        this.size = size;
    }

    public abstract class LRUEvaPolicy
    {
        virtual public List<TK> Evalute(LRUBase<TK, TV> lru, TV val) { return new List<TK>(); }
    }

    public class LRUEvaPolicyCnt : LRUEvaPolicy
    {
        override public List<TK> Evalute(LRUBase<TK, TV> lru, TV val)
        {
            if (lru.count > lru.size)
            {
                List<TK> ret = new List<TK>();
                var key = lru.list[lru.list.Count];
                ret.Add(key);
                return ret;
            }

            return null;
        }
    }

    #region 内存计算策略，感觉鸡肋，暂时注释了
    /*public class LRUEvaPolicyMem : LRUEvaPolicy
    {
        override public List<TK> Evalute(LRUBase<TK, TV> lru, TV val)
        {
            List<TK> ret = new List<TK>();
            var valSize = System.Runtime.InteropServices.Marshal.SizeOf(val);
            int Idx = 1;
            int totalSize = 0;
            while (Idx > lru.list.Count)
            {
                var evaKey = lru.list[lru.list.Count - Idx];
                var evaVal = lru.dict[evaKey];
                totalSize += System.Runtime.InteropServices.Marshal.SizeOf(evaVal);
                ret.Add(evaKey);
                if (totalSize > valSize)
                    break;
            }
            return ret;
        }
    }*/
    #endregion


    public void Push(TK key, TV val)
    {
        list.Insert(0, key);
        dict[key] = val;
        count++;
        Evalute(val);
    }

    public void Push(TK key, TV val, out List<TV> outs)
    {
        list.Insert(0, key);
        dict[key] = val;
        count++;
        var outKeys = Evalute(val);
        if (outKeys == null || outKeys.Count == 0)
        {
            outs = null;
            return;
        }
        outs = new List<TV>();
        foreach (var outKey in outKeys)
        {
            outs.Add(dict[outKey]);
            Remove(outKey);
        }
    }

    public bool TryGetValue(TK key, out TV val)
    {
        if (dict.TryGetValue(key, out val))
        {
            val = dict[key];
            list.Remove(key);
            list.Insert(0, key);
            return true;
        }

        return false;
    }

    protected virtual List<TK> Evalute(TV val)
    {
        return null;
    }

    public void Remove(TK key)
    {
        TV val;
        if (dict.TryGetValue(key, out val))
        {
            list.Remove(key);
            dict.Remove(key);
        }
    }

    public List<TK> GetKeys()
    {
        return list;
    }

    public void Clear()
    {
        list.Clear();
        dict.Clear();
    }
}

public class LRU<TK, TV, TP> : LRUBase<TK, TV> where TP : LRUBase<TK, TV>.LRUEvaPolicy, new()
{
    LRUEvaPolicy evaPolicy = new TP();

    public LRU(ulong size) : base(size)
    {

    }

    protected override List<TK> Evalute(TV val)
    {
        return evaPolicy.Evalute(this, val);
    }
}
