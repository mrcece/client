using Aio;
using LuaInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class Main : MonoBehaviour
{

    public static GameObject main;
    public static Main Ins;

    public void Awake()
    {
        Ins = this;
        main = gameObject;
        DontDestroyOnLoad(main);
    }

    public void Start()
    {
        StartCoroutine(Init());
    }


    public class Source
    {
        public string name;
        public string platform; // ios,android,editor
        public string type; // dev,release,review
        public int appVersion;
        public int requireMinAppVersion;
        public int requireMaxAppVersion;
        public string rootRelatePath;
        public int resourceVersion;
        public string md5OfMd5File;
    }

    public class Config
    {
        public List<string> sourceRootUrls = new List<string>();
        public List<Source> sources = new List<Source>();
    }

    private Config config;
    private Source preferSource;

    public enum ErrorCode
    {
        OK = 0,
        DOWNLOAD_VERSION_FAIL = 1,
        NEED_UPGRADE_APP = 2,
        DOWNLOAD_MD5_FAIL = 3,
        DOWNLOAD_MD5_FILE_MD5_ERROR = 4,
        DO_VERSION_LUA_FAIL = 5,
        READ_LOCAL_MANIFEST_FAIL = 6,
        LOCAL_MANIFEST_CORRUPT = 7,
        LOCAL_MD5_CORRUPT = 8,
        SOME_FILE_DOWNLOAD_FAIL = 9,
        NOT_AVALIABLE_SOURCE = 10,
        REPAIR_LOCAL_MANIFEST_FAIL = 11,
    }

    public static ErrorCode Error { get; set; }


    /*
     *  从 versionUrl 里取到的数据大致类型于
     * return {
            urls = {
                "http://www.baidu.com",
                "http://www.wanmei.com",
            },

            sources = 
            {
                {
                    name = "ios_debug",
                    platform="ios",
                    version = { major=1, minor=12},
                    path = "ios_debug",
                    manifest = { resource=500, md5md5="abcdefg"},
                },
                {
                    name = "ios_release",
                    platform="ios",
                    version = { major=1, minor=12},
                    path = "ios",
                    manifest = { resource=500, md5md5="abcdefg"},
                },
            }
        }
     * 
     * */
    private void ParseConfig(Config conf, LuaTable table)
    {
        var rootUrlsTable = table["urls"] as LuaTable;
        for (int i = 1, n = rootUrlsTable.Length; i <= n; i++)
        {
            var sourcRootUrl = rootUrlsTable[i] as string;
            conf.sourceRootUrls.Add(sourcRootUrl);
            Debug.LogFormat("== config.url : {0}", sourcRootUrl);
        }
        var sourcesTable = table["sources"] as LuaTable;
        for (int i = 1, n = sourcesTable.Length; i <= n; i++)
        {
            var stable = sourcesTable[i] as LuaTable;
            var s = new Source();
            s.name = stable["name"] as string;
            s.platform = stable["platform"] as string;
            s.type = stable["type"] as string;

            s.appVersion = (int)(double)stable["app_version"];
            var compatibleVersion = stable["compatible_app_version"] as LuaTable;
            s.requireMinAppVersion = (int)(double)compatibleVersion[1];
            s.requireMaxAppVersion = (int)(double)compatibleVersion[2];
            s.rootRelatePath = stable["path"] as string;

            var manifest = stable["manifest"] as LuaTable;
            s.resourceVersion = (int)(double)manifest["resource_version"];
            s.md5OfMd5File = manifest["md5md5"] as string;
            Debug.LogFormat("== config.resource name:{0} type:{7} platform:{1} app_version:{2} require_app_version:{3} path:{4} resource:{5} md5md5:{6}",
                s.name, s.platform, s.appVersion, s.requireMinAppVersion, s.rootRelatePath, s.resourceVersion, s.md5OfMd5File, s.type);
            if (s.name == null || s.type == null || s.platform == null || s.rootRelatePath == null || s.md5OfMd5File == null)
                throw new Exception("incorrect config");
            conf.sources.Add(s);
        }
    }

    private IEnumerator LoadAppConfig(Config conf)
    {
        Error = ErrorCode.OK;
        if (Platform.IgnoreVersionCheck) yield break;
        var luaState = new LuaState();
        /**
         *    从 Resources/config.lua.txt里取的数据大致类似于
         *    return {
                    versionurls = {
                        "file:///d:/update/editor",
                        "file:///e:/update/editor",
                    },

                }
         * 
         * */
        try
        {
            var res = luaState.DoString<LuaTable>(Encoding.UTF8.GetString(Resources.Load<TextAsset>(string.Format("config.{0}.lua", Platform.PlatformName)).bytes));
            var versionUrls = res["versionurls"] as LuaTable;
            for (int i = 1, n = versionUrls.Length; i <= n; i++)
            {
                var versionFullUrl = (versionUrls[i] as string) + "/" + "version.lua";
                Debug.LogFormat("== version url:{0}", versionFullUrl);
                var www = new WWW(versionFullUrl);
                yield return www;
                if (www.error == null)
                {
                    var luaText = Encoding.UTF8.GetString(www.bytes);
                    try
                    {
                        var result = luaState.DoString<LuaTable>(luaText);
                        ParseConfig(conf, result);
                    }
                    catch (Exception e)
                    {
                        Error = ErrorCode.DO_VERSION_LUA_FAIL;
                        Debug.LogException(e);
                    }
                    yield break;
                }
            }
            Error = ErrorCode.DOWNLOAD_VERSION_FAIL;
        }
        finally
        {
            luaState.Dispose();
        }
    }

    private DownloadManager.Config GetDownloadCfg()
    {
        var dcfg = new DownloadManager.Config();
        dcfg.tmpDirectory = Platform.PersistentDataPath + "/tmp";
        dcfg.maxDownloadingTaskNum = 15;
        return dcfg;
    }

    private AssetsManager.Config GetAssetsCfg()
    {
        var acfg = new AssetsManager.Config();
        acfg.localAppVersion = Platform.AppVersion;

        acfg.urls = config.sourceRootUrls;
        acfg.manifestFile = "manifest.txt";
        acfg.md5File = "md5.txt";
        acfg.resourceRootDirectory = "data";

        if (!Platform.IgnoreVersionCheck)
        {
            if (preferSource != null)
            {
                acfg.source = preferSource;
            }
            else
            {
                acfg.source = config.sources.Where(s => s.platform == Platform.PlatformName
                    && acfg.localAppVersion >= s.requireMinAppVersion
                    && acfg.localAppVersion <= s.requireMaxAppVersion
                    && (s.type == "release" || s.type == "review")).FirstOrDefault();
                if (acfg.source == null)
                {
                    Debug.LogWarningFormat("GetAssetsCfg. can't find compatible source. try incompatible");
                    acfg.source = config.sources.Where(s => s.platform == Platform.PlatformName && s.type == "release").FirstOrDefault();
                }
            }
        }
        else
        {
            acfg.source = null;
        }
        return acfg;
    }


    private IEnumerator Init()
    {
        preferSource = null;
        config = new Config();
        yield return StartCoroutine(LoadAppConfig(config));

        if (Error != ErrorCode.OK)
        {
            Debug.LogErrorFormat("LoadAppConfig fail. err:{0}", Error);
            yield break;
        }
        // todo init sdks
        var dm = DownloadManager.Ins;
        dm.Init(GetDownloadCfg());

        var um = AssetsManager.Ins;
        um.Init(GetAssetsCfg());

        if (!Platform.IgnoreVersionCheck)
        {
            yield return StartCoroutine(um.NormalUpdate());

            if (Error == ErrorCode.LOCAL_MANIFEST_CORRUPT || Error == ErrorCode.LOCAL_MD5_CORRUPT)
            {
                yield return StartCoroutine(um.RepaireUpdate());
            }
            if (Error != ErrorCode.OK)
            {
                Debug.LogErrorFormat("Boot.Init fail. err:{0}", Error);
            }

            while (!um.Ready)
                yield return null;
        }

        new Test().test();
        while (!GameStart)
            yield return null;

        OnReady();
    }

    public bool GameStart { get; set; }

    private void OnReady()
    {
        if (!Platform.IgnoreVersionCheck)
            StartCoroutine(AssetsManager.Ins.StartRemainUpdate());
        NetManager.Ins.Start();
        ResourceManager.Ins.Start();
        main.AddComponent<LuaManager>();
    }

    public event Action Updater;
    public event Action LateUpdater;
    public event Action FixedUpdater;

    public void Update()
    {
        if (Updater != null)
            Updater();
    }

    public void LateUpdate()
    {
        if (LateUpdater != null)
            LateUpdater();
    }

    public void FixedUpdate()
    {
        if (FixedUpdater != null)
            FixedUpdater();
    }
}
