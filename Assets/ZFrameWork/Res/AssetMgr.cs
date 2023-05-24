//#undef UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using ZFrameWork;
using UnityEditor;


using UObj = UnityEngine.Object;
using ZFileUtil = ZFrameWork.ZFileUtil;

public class AssetUtil
{
    static public string DirPath2BundleKey(string path)
    {
        var key = path.ToLower();
        return $"{key.Substring(key.IndexOf("assets")).Replace("\\", "_").Replace("/", "_")}.bundle";
    }

    static public string Path2BundleKey(string path)
    {
        var key = path.ToLower();
        return key.Substring(key.IndexOf("assets")).Replace("\\", "_").Replace("/", "_");
    }

    static public List<string> LoadSchemaBundleKeys()
    {
        List<string> ret = new List<string>();
        var json = ZFileUtil.ReadFileToJson(FileConst.BuildBundleKeyFilePath);
        if (json != null && json.IsArray)
            for (int i = 0; i < json.Count; i++)
                ret.Add(DirPath2BundleKey((string)json[i]));
        return ret;
    }

    static public List<string> LoadSchemaBundlePaths()
    {
        List<string> ret = new List<string>();
        var json = ZFileUtil.ReadFileToJson(FileConst.BuildBundleKeyFilePath);
        if (json != null && json.IsArray)
            for (int i = 0; i < json.Count; i++)
                ret.Add((string)json[i]);
        return ret;
    }
}

namespace ZFrameWork
{
    public class ObjPoolBase<T> where T : new()
    {
        Queue<T> objs = new Queue<T>();
        protected T Pop()
        {
            if (Count > 0)
                return objs.Dequeue();
            return default(T);
        }

        protected void Push(T obj)
        {
            objs.Enqueue(obj);
        }

        public void Shrank(int desired)
        {
            var cnt = Count;
            if (cnt > desired && cnt != 0)
            {
                var del = cnt - desired;
                while(del-- > 0)
                    objs.Dequeue();
            }
        }

        public void Clear()
        {
            objs.Clear();
        }

        public void Preload(int desired)
        {
            var cnt = Count;
            if (desired > cnt)
            {
                var del = desired - cnt;
                while (del-- > 0)
                    objs.Enqueue(new T());
            }
        }

        public int Count { get { return objs.Count; } }
    }

    public class GarbageQue<TK, TV>
    {
        LinkedList<TK> que = new LinkedList<TK>();
        Dictionary<TK, TV> dict = new Dictionary<TK, TV>();
        Action<TV> onRelease;
        uint size;
        public int Count { get { return que.Count; } }
        public GarbageQue(uint size, Action<TV> onRelease)
        {
            this.size = size;
            this.onRelease = onRelease;
        }

        public void Push(TK key, TV val)
        {
            if (!dict.ContainsKey(key))
            {
                dict[key] = val;
                que.AddFirst(key);
                Evalute();
            }
        }

        void Evalute()
        {
            if (que.Count > size)
            {
                var key = que.Last.Value;
                var val = dict[key];
                dict.Remove(key);
                que.RemoveLast();
                onRelease?.Invoke(val);
            }
        }

        public bool TryPopVal(TK key, out TV outVal)
        {
            outVal = default(TV);
            if (dict.ContainsKey(key))
            {
                outVal = dict[key];
                dict.Remove(key);
                que.Remove(key);
                return true;
            }
            return false;
        }

        public void ReleaseAll()
        {
            while (Count > 0)
            {
                var vals = new List<TV>();
                foreach (var key in que)
                    vals.Add(dict[key]);

                que.Clear();
                dict.Clear();

                foreach (var val in vals)
                    onRelease?.Invoke(val);
            }
        }
    }

    public class AssetOp
    {
        bool isRelease = false;
        public bool IsValid { get; private set; }
        public string AssetKey { get; private set; }
        public Action<UObj> Callback { get; private set; }
        public void Release()
        {
            isRelease = true;
        }

        public void Init(string assetKey, Action<UObj> cb)
        {
            AssetKey = assetKey;
            Callback = cb;
            IsValid = true;
            isRelease = false;
        }

        public void Clear()
        {
            IsValid = false;
            Callback = null;
            isRelease = true;
            AssetKey = null;
        }

        public void OnLoadComplete(string assetKey, UObj asset)
        {
            if (AssetKey == assetKey && isRelease == false && IsValid == true)
                Callback(asset);
        }

        ~AssetOp()
        {
            if (isRelease == false && IsValid == true)
                AssetFacade.ReleaseAsset(this);
        }
    }

    public class AssetLoader
    {
        Dictionary<object, AssetOp> cacheDict = new Dictionary<object, AssetOp>();
        List<AssetOp> cacheList = new List<AssetOp>();
        public void LoadAsset<T>(object key, string assetKey, Action<T> cb) where T : UObj
        {
            Release(key);
            cacheDict[key] = AssetFacade.LoadAsset(assetKey, cb);
        }

        public void LoadAsset<T>(string assetKey, Action<T> cb) where T : UObj
        {
            cacheList.Add(AssetFacade.LoadAsset(assetKey, cb));
        }

        public void Release(object key)
        {
            AssetOp op;
            if (cacheDict.TryGetValue(key, out op))
            {
                AssetFacade.ReleaseAsset(op);
                cacheDict.Remove(key);
            }
        }

        public void ReleaseAll()
        {
            foreach (var pair in cacheDict)
                AssetFacade.ReleaseAsset(pair.Value);
            cacheDict.Clear();

            foreach (var op in cacheList)
                AssetFacade.ReleaseAsset(op);
            cacheList.Clear();
        }

        ~AssetLoader()
        {
            ReleaseAll();
        }
    }

    public class AssetFacade
    {
        class BundleLocaMgr : MgrBase<BundleLocaMgr>
        {
            class FindNode
            {
                string data;
                List<FindNode> children = new List<FindNode>();
                public FindNode(string data)
                {
                    this.data = data;
                }

                public void Add(string key)
                {
                    key = key.Substring(0, key.LastIndexOf("."));
                    var keys = key.Split("_");
                    int deep = 0;
                    int len = keys.Length;
                    FindNode node = Search(keys, len, ref deep);
                    deep++;
                    while (deep < len)
                    {
                        var n = new FindNode(keys[deep]);
                        node.children.Add(n);
                        node = n;
                        deep++;
                    }
                }

                public bool TryGetValue(string key, out string bundleKey)
                {
                    key = key.Substring(0, key.LastIndexOf("."));
                    var keys = key.Split("_");
                    int deep = 0;
                    int len = keys.Length;

                    Search(keys, len, ref deep);
                    if (deep == 0)
                    {
                        bundleKey = null;
                        return false;
                    }

                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < deep; i++)
                    {
                        builder.Append(keys[i]);
                        builder.Append("_");
                    }
                    builder.Append(keys[deep]);
                    bundleKey = builder.ToString() + ".bundle";
                    return true;
                }

                FindNode Search(string[] keys, int len, ref int deep)
                {
                    if (deep == len)
                        return this;
                    foreach (var child in children)
                    {
                        if (child.data == keys[deep + 1])
                        {
                            deep++;
                            return child.Search(keys, len, ref deep);
                        }
                    }
                    return this;
                }
            }

            HashSet<string> remoteBundleKeys = new HashSet<string>();
            FindNode searchRoot;
            AssetBundleManifest localMani;
            AssetBundleManifest remoteMani;

            protected override void OnInit()
            {
                searchRoot = new FindNode("assets");
                var localBundleKeys = new HashSet<string>();
#if UNITY_EDITOR
                // �ӱ����ļ��ж�ȡ
                var schemaKeys = AssetUtil.LoadSchemaBundleKeys();
                foreach (var key in schemaKeys)
                    localBundleKeys.Add(key);
#else
                LoadManifest(ref localMani, FileConst.LocalManifestBundlePath);
                LoadManifest(ref remoteMani, FileConst.RemoteManifestBundlePath);
                GatherBundleKeys(localMani, localBundleKeys);
                GatherBundleKeys(remoteMani, remoteBundleKeys);
#endif
                localBundleKeys.UnionWith(remoteBundleKeys);
                var totalBundleKeys = new string[localBundleKeys.Count];
                localBundleKeys.CopyTo(totalBundleKeys);
                foreach (var key in totalBundleKeys)
                    searchRoot.Add(key);
            }

            protected override void OnShutdown()
            {
                remoteBundleKeys = null;
                searchRoot = null;
                localMani = null;
                remoteMani = null;
            }

            public List<string> GetDepands(string bundleKey)
            {
                List<string> ret = new List<string>();
                var mani = IsRemoteBundle(bundleKey) ? remoteMani : localMani;
                var deps = mani.GetAllDependencies(bundleKey);
                foreach (var dep in deps)
                {
                    var depKey = GetBundleKey(dep);
                    ret.Add(depKey);
                    ret.AddRange(GetDepands(depKey));
                }
                return ret;
            }

            void LoadManifest(ref AssetBundleManifest mani, string path)
            {
                if (!ZFileUtil.CheckFile(path))
                    return;
                var bundle = AssetBundle.LoadFromFile(path);
                if (bundle != null)
                {
                    mani = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                    bundle.Unload(false);
                }
            }

            void GatherBundleKeys(AssetBundleManifest mani, HashSet<string> bundleKeys)
            {
                if (mani != null)
                {
                    var keys = mani.GetAllAssetBundles();
                    foreach (var key in keys)
                        bundleKeys.Add(key);
                }
            }

            bool IsRemoteBundle(string bundleKey)
            {
                return remoteBundleKeys.Contains(bundleKey);
            }

            public string GetBundleKey(string assetKey)
            {
                var key = assetKey.ToLower();
                key = key.Substring(key.IndexOf("assets")).Replace("/", "_");
                string ret = null;
                searchRoot.TryGetValue(key, out ret);
                return ret;
            }

            public string GetBundlePath(string bundleKey)
            {
                if (IsRemoteBundle(bundleKey))
                    return Path.Combine(FileConst.AssetBundleRemotePath, bundleKey);
                return Path.Combine(FileConst.AssetBundleLocalPath, bundleKey);
            }

        }

        class BundleMgr : MgrBase<BundleMgr>
        {
            Dictionary<string, BundleInfo> cache = new Dictionary<string, BundleInfo>();
            GarbageQue<string, BundleInfo> garbage;

            protected override void OnInit()
            {
                garbage = new GarbageQue<string, BundleInfo>(32, Unload);
            }

            public void Release(string bundleKey)
            {
                BundleInfo bundle;
                if (cache.TryGetValue(bundleKey, out bundle))
                {
                    bundle.Release();
                    if (bundle.RefCnt == 0)
                    {
                        cache.Remove(bundleKey);
                        garbage.Push(bundleKey, bundle);
                    }
                }
            }

            public void Unload(BundleInfo bundle)
            {
                bundle.Unload();
            }

            public AssetBundle LoadBundle(string bundleKey)
            {
                var key = bundleKey;
                BundleInfo bundle;

                if (cache.TryGetValue(key, out bundle))
                {
                    return bundle.Load();
                }

                if (garbage.TryPopVal(key, out bundle))
                {
                    cache[key] = bundle;
                    return bundle.Load();
                }

                bundle = new BundleInfo(key);
                cache[key] = bundle;
                return bundle.Load();
            }

            public void GC()
            {
                garbage.ReleaseAll();
            }

            protected override void OnShutdown()
            {
                foreach (var pair in cache)
                    Unload(pair.Value);
                GC();
            }
        }

        class AssetMgr : MgrBase<AssetMgr>
        {
            class AssetOpPool : ObjPoolBase<AssetOp>
            {
                public AssetOp Get(string assetKey, Action<UObj> cb)
                {
                    AssetOp op = Pop();
                    if (op == null)
                        op = new AssetOp();

                    op.Init(assetKey, cb);
                    return op;
                }

                public void recycle(AssetOp op)
                {
                    op.Clear();
                    Push(op);
                }
            }

            Dictionary<string, AssetInfo> cache = new Dictionary<string, AssetInfo>();
            GarbageQue<string, AssetInfo> garbage;
            AssetOpPool opPool = new AssetOpPool();

            protected override void OnInit()
            {
                garbage = new GarbageQue<string, AssetInfo>(128, Unload);
            }

            public AssetOp LoadAsset<T>(string assetkey, Action<T> cb) where T : UObj
            {
                var key = assetkey.Replace("\\","/");
                AssetInfo asset;
                var op = opPool.Get(key, (obj) => { cb(obj as T); });
                if (cache.TryGetValue(key, out asset))
                {
                    asset.Load<T>(op);
                    return op;
                }

                if (garbage.TryPopVal(key, out asset))
                {
                    asset.Load<T>(op);
                    cache[key] = asset;
                    return op;
                }

                asset = new AssetInfo(key);
                cache[key] = asset;
                asset.Load<T>(op);
                return op;
            }

            public void ReleaseAsset(AssetOp op)
            {
                op.Release();
                if (!op.IsValid) return;
                var key = op.AssetKey;
                AssetInfo asset;
                if (cache.TryGetValue(key, out asset))
                {
                    asset.Release();
                    if (asset.RefCnt == 0)
                    {
                        cache.Remove(key);
                        garbage.Push(key, asset);
                    }
                }
                opPool.recycle(op);
            }

            public void Unload(AssetInfo asset)
            {
                asset.Unload();
            }

            public void GC()
            {
                garbage.ReleaseAll();
                opPool.Shrank(32);
            }

            protected override void OnShutdown()
            {
                foreach (var pair in cache)
                    Unload(pair.Value);
                GC();
            }
        }

        class BundleInfo
        {
            public AssetBundle bundle;
            public uint RefCnt { get; private set; }
            public string BundleKey { get; private set; }
            public List<string> Depends { get; private set; }
            public BundleInfo(string bundleKey)
            {
                RefCnt = 0;
                BundleKey = bundleKey;
                Depends = new List<string>();
                var depPaths = BundleLocaMgr.Inst.GetDepands(bundleKey);
                foreach (var depPath in depPaths)
                    Depends.Add(AssetUtil.Path2BundleKey(depPath));
                bundle = AssetBundle.LoadFromFile(BundleLocaMgr.Inst.GetBundlePath(bundleKey));
                // Debug.Log($"key = {bundleKey}, path = {BundleLocaMgr.Inst.GetBundlePath(bundleKey)}");
                foreach (var dep in Depends)
                    BundleMgr.Inst.LoadBundle(dep);
            }

            public void Unload()
            {
                bundle.Unload(true);
                foreach (var dep in Depends)
                    BundleMgr.Inst.Release(dep);
            }

            public void Release()
            {
                RefCnt--;
            }

            public AssetBundle Load()
            {
                RefCnt++;
                return bundle;
            }
        }

        class AssetInfo
        {
            UObj asset;
            public UObj Asset { get { return asset; } }
            AssetBundleRequest req;
            public uint RefCnt { get; private set; }
            public string AssetKey { get; private set; }
            public string BundleKey { get; set; }
            public AssetInfo(string assetKey)
            {
                RefCnt = 0;
                AssetKey = assetKey;
                BundleKey = "";
            }
            public void Load<T>(AssetOp op) where T:UObj
            {
#if UNITY_EDITOR
                asset = AssetDatabase.LoadAssetAtPath(AssetKey,typeof(T));
                op.OnLoadComplete(AssetKey, Asset);
#else
            if (asset != null)
                op.OnLoadComplete(AssetKey, Asset);
            else
            {

                if (req == null)
                {
                    BundleKey = BundleLocaMgr.Inst.GetBundleKey(AssetKey);
                    var bundle = BundleMgr.Inst.LoadBundle(BundleKey);
                    req = bundle.LoadAssetAsync<T>(AssetKey);
                    req.completed += (h) =>
                    {
                        asset = req.asset;
                        req = null;
                    };
                }
                req.completed += (h) => { op.OnLoadComplete(AssetKey, Asset); }; 
            }
#endif
                RefCnt++;
            }

            public void Release()
            {
                RefCnt--;
            }

            public void Unload()
            {
#if UNITY_EDITOR
                if (!(asset is GameObject))
                    Resources.UnloadAsset(asset);
#else
            BundleMgr.Inst.Release(BundleKey);
#endif
            }
        }

        static public void Init()
        {
            BundleLocaMgr.Inst.Init();
            BundleMgr.Inst.Init();
            AssetMgr.Inst.Init();
        }

        static public AssetOp LoadAsset<T>(string assetkey, Action<T> cb) where T : UObj
        {
            return AssetMgr.Inst.LoadAsset<T>(assetkey, cb);
        }

        static public byte[] LoadLuaBuffer(string path)
        {
            path = ZFileUtil.NormPath(path);
            var isEdit = false;
#if UNITY_EDITOR
            isEdit = true;
#endif      
            if (isEdit)
            {
                if (!ZFileUtil.CheckFile(path))
                    return null;
                return File.ReadAllBytes(path);
            }

            path = Path.ChangeExtension(path, FileConst.TxtFileExtension);
            var bundleKey = GetBundleKey(path);
            var bundle = BundleMgr.Inst.LoadBundle(bundleKey);
            if (bundle != null)
            {
                var asset = bundle.LoadAsset<TextAsset>(path);
                Resources.UnloadAsset(asset);
                return asset.bytes;
            }
            return null;
        }

        static public void ReleaseAsset(AssetOp op)
        {
            AssetMgr.Inst.ReleaseAsset(op);
        }

        static public string GetBundleKey(string assetKey)
        {
            return BundleLocaMgr.Inst.GetBundleKey(assetKey);
        }

        static public void GC()
        {
            AssetMgr.Inst.GC();
            BundleMgr.Inst.GC();
        }

        static public void Flush()
        {
            BundleLocaMgr.Inst.Init();
        }

        static public void Shutdown()
        {
            BundleLocaMgr.Inst.Shutdown();
            BundleMgr.Inst.Shutdown();
            AssetMgr.Inst.Shutdown();
        }

        static public void HotReload()
        {
            Shutdown();
            BundleLocaMgr.Inst.Init();
        }
    }
}