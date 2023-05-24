using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // Start is called before the first frame update
    static MonoSingleton<T> inst;
    public static T Inst { get { return inst as T; } }

    void Awake()
    {
        inst = this;
        OnAwake();
    }

    virtual protected void OnAwake()
    {

    }
}
