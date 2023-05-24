using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace ZFrameWork.Build
{
    public class BuildConst
    {

        static public BuildTarget BuildTarget
        {
            get
            {
#if UNITY_STANDALONE
                return BuildTarget.StandaloneWindows;
#elif UNITY_ANDROID
        return BuildTarget.Android;
#else
        return BuildTarget.iOS;
#endif
            }
        }
    }


}
