using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMonoMgr : MonoMgrBase<TestMonoMgr>
{
    void Awake()
    {
        instance = this;    
    }
}