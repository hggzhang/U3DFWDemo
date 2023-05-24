using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PropertyBinder<T>
{
    public virtual void OnValChanged(T newVal, T oldVal)
    {

    }
}

public class BindableProperty<T>
{
    protected T val;
    protected List<PropertyBinder<T>> binders;

    public BindableProperty(T val)
    {
        this.val = val;
        binders = new List<PropertyBinder<T>>();
    }

    public void Modify(T val)
    {
        var oldVal = this.val;
        this.val = val;
        OnValChanged(val, oldVal);
    }

    void OnValChanged(T newVal, T oldVal)
    {
        foreach (var binder in binders)
            binder.OnValChanged(newVal, oldVal);
    }
}

public class Attr : BindableProperty<int>
{
    public Attr(int val = 0) : base(val)
    {
        //
    }

    public int GetVal()
    {
        return val;
    }
}

public enum EAttr : int
{
    HP = 1,
    MP = 2,
    ATK = 3,
    DEF = 4,
    Max = 5,
}

public class AttrComp
{
    Dictionary<EAttr, Attr> attrDict = new Dictionary<EAttr, Attr>();

    void InitAttr()
    {
        for (int i = 1; i < (int)EAttr.Max; i++)
            attrDict[(EAttr)i] = new Attr();

        SetAttr(EAttr.HP, 100);
        SetAttr(EAttr.MP, 50);
        SetAttr(EAttr.ATK, 20);
        SetAttr(EAttr.DEF, 10);
    }

    public void AddAttr(EAttr e, int val = 0)
    {
        //
    }

    public void RemoveAttr(EAttr e, int val = 0)
    {
        //
    }

    void SetAttr(EAttr e, int val)
    {
        attrDict[e].Modify(val);
    }

    public void SetBool(EAttr e, bool val)
    {
        int v = val ? 1 : 0;
        SetAttr(e, v);
    }

    public void SetFloat(EAttr e, float val)
    {
        int v = (int)(val * 100);
        SetAttr(e, v);
    }

    public void SetInt(EAttr e, int val)
    {
        SetAttr(e, val);
    }

    public int GetAttr(EAttr e)
    {
        if (attrDict.ContainsKey(e))
            return attrDict[e].GetVal();

        return 0;
    }

    public bool GetBool(EAttr e)
    {
        return GetAttr(e) == 1;
    }

    public float GetFloat(EAttr e)
    {
        return GetAttr(e) / 100.0f;
    }

    public int GetInt(EAttr e)
    {
        return GetAttr(e);
    }

}


public class ACS : MonoBehaviour
{
    //
}

public enum ENumericMod : int
{
    Inc = 1,
    More = 2,
}

abstract public class NumericMod
{
    protected Numeric numeric;
    public ENumericMod ModTy { get; private set; }
    public abstract void Modify(ref int val);
    public void SetNumeric(Numeric numeric)
    {
        this.numeric = numeric;
    }

    public void UpdateNumeric()
    {
        numeric?.UpdValWithNtf();
    }
}

abstract public class NumericValMod : NumericMod
{
    protected int val;
    public virtual void SetVal(int val)
    {
        if (val == this.val)
            return;
        this.val = val;
        UpdateNumeric();
    }
}

public class NumericModInc : NumericValMod
{
    public override void Modify(ref int val)
    {
        val += val;
    }
}

public class NumericModValSyncDec<T> : NumericValMod where T : NumericValMod
{
    T mod;
    
    public override void Modify(ref int val)
    {
        mod.Modify(ref val);
    }

    public override void SetVal(int val)
    {
        mod.SetVal(val);
    }

    public void SetSyncSourceNumeric(Numeric numeric)
    {
        SetNumeric(numeric);
    }

    public void Bind()
    {
        if (numeric == null)
            return;
        numeric.AddValChangedAction(OnValChanged);
    }

    public void UnBind()
    {
        if (numeric == null)
            return;
        numeric.RemoveValChangedAction(OnValChanged);
    }

    public void OnValChanged(int newVal, int oldVal)
    {
        SetVal(newVal);
    }
}

public class Numeric
{
    public const int floatFactor = 100;
    protected int val;
    protected List<NumericMod> mods = new List<NumericMod>();
    protected List<Action<int, int>> valChangedActions = new List<Action<int, int>>();

    public Numeric(int val = 0, Action<int, int> onValChanged = null)
    {
        this.val = val;
        if (onValChanged != null)
            valChangedActions.Add(onValChanged);
    }

    public void AddValChangedAction(Action<int, int> onValChanged)
    {
        if (onValChanged != null)
            valChangedActions.Add(onValChanged);
    }

    public void RemoveValChangedAction(Action<int, int> onValChanged)
    {
        if (onValChanged != null)
            valChangedActions.Remove(onValChanged);
    }

    public void SetVal(int val)
    {
        var oldVal = this.val;
        this.val = val;
        UpdVal();
        OnValChanged(this.val, oldVal);
    }

    public void OnValChanged(int newVal, int oldVal)
    {
        foreach (var action in valChangedActions)
            action(newVal, oldVal);
    }

    void UpdVal()
    {
        foreach (var mod in mods)
            mod.Modify(ref val);
    }

    public void UpdValWithNtf()
    {
        int oldVal = val;
        UpdVal();
        if (oldVal == val)
            return;
        OnValChanged(val, oldVal);
    }

    public void SetInt(int val)
    {
        SetVal(val);
    }

    public void SetBool(bool val)
    {
        var v = val ? 1 : 0;
        SetVal(v);
    }

    public void SetFloat(float val)
    {
        var v = (int)(val * floatFactor);
        SetVal(v);
    }

    public int GetVal()
    {
        return val;
    }

    public bool GetBool()
    {
        return val == 1;
    }

    public int GetInt()
    {
        return val;
    }

    public float GetFloat()
    {
        return (float)(val * floatFactor);
    }

    public void AddMod(NumericMod mod)
    {
        var oldVal = val;
        mods.Add(mod);
        mod.SetNumeric(this);
        mods.Sort((a, b) => { return (int)(a.ModTy) < (int)(b.ModTy) ? 1 : -1; });
        UpdVal();
        if (val == oldVal)
            return;

        OnValChanged(val, oldVal);
    }

    public void RemoveMod(NumericMod mod)
    {
        var oldVal = val;
        mods.Remove(mod);
        mod.SetNumeric(null);
        UpdVal();
        if (val == oldVal)
            return;
        OnValChanged(val, oldVal);
    }
}

public class NumericMgr<TK>
{
    Dictionary<TK, Numeric> numericDict;
    Dictionary<TK, NumericModInc> modDict;
    List<NumericMgr<TK>> childern = new List<NumericMgr<TK>>();
    NumericMgr<TK> parent;

    public void AddNumeric(TK k, int v)
    {
        if (!numericDict.ContainsKey(k))
        {
            var n = new Numeric(v);
            numericDict[k] = n;

            if (parent != null) ;
        }
    }

    void AddNumericToParent(TK k)
    {
        if (numericDict.ContainsKey(k))
        {
            //
        }
    }

    public void RemoveNumeric(TK k)
    {
        if (numericDict.ContainsKey(k))
        {
            numericDict.Remove(k);
        }
    }

    public void AddValChangedAction(TK k, Action<int, int> action)
    {
        if (numericDict.ContainsKey(k))
        {
            numericDict[k].AddValChangedAction(action);
        }
    }

    public void RemoveValChangedAction(TK k, Action<int, int> action)
    {
        if (numericDict.ContainsKey(k))
        {
            numericDict[k].RemoveValChangedAction(action);
        }
    }

    public void AddMod(TK k, NumericMod mod)
    {
        if (numericDict.ContainsKey(k))
        {
            numericDict[k].AddMod(mod);
        }
    }

    public void RemoveMod(TK k, NumericMod mod)
    {
        if (numericDict.ContainsKey(k))
        {
            numericDict[k].RemoveMod(mod);
        }
    }

    public void AddChild(NumericMgr<TK> child)
    {
        
    }


}

