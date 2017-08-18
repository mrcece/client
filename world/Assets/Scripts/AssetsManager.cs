using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;


    public class AssetsManager
    {
        public readonly static AssetsManager Ins = new AssetsManager();

        public class Config 
        {
            public int localAppVersion;

            public string md5File;
            public string manifestFile;
            public string resourceRootDirectory;
            public List<string> urls;
            public Main.Source source;
        }

        private Dictionary<string, ResourceInfo> coreResourceInfos;
        private Dictionary<string, ResourceInfo> notCoreResourceInfos;

        class ResourceInfo
        {
            public string path;
            public string md5;
            public bool core;
            public long size;
        }

        private Config conf;

        private List<string> rootUrls = new List<string>();
        public void Init(Config conf)
        {
            this.conf = conf;

            if(conf.source != null)
            {
                Main.Ins.Updater += Update;
                foreach(var url in conf.urls)
                {
                    rootUrls.Add(url + "/" + conf.source.rootRelatePath);
                }
            }
        }

        public bool Ready { get; private set; }


        private IEnumerator LoadMd5File(string md5File, Dictionary<string, ResourceInfo> resources)
        {
            bool done = false;

            System.Threading.ThreadPool.QueueUserWorkItem((a) =>
            {
                try
                {
                    if (File.Exists(md5File))
                    {
                        foreach(var line in File.ReadAllLines(md5File, Encoding.UTF8))
                        {
                            var args = line.Split(',');
                            if (args.Length < 2) continue;
                            var file = args[0];
                            var ri = new ResourceInfo() { path = file, md5 = args[1], core = args[2].Length > 0, size = int.Parse(args[3])};
                            resources.Add(file, ri);
                        }
                    }
                    else
                    {
                        Debug.LogFormat("resourceinfofile:{0} not exist", md5File);
                    }
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    lock(resources)
                    {
                        done = true;
                    }
                }
            });

            while(true)
            {
                lock(resources)
                {
                    if (done)
                        break;
                }
                yield break;
            }

            Debug.LogFormat("LoadLocalMd5File resources:{0} file:{1}", resources.Count, md5File);
            yield break;
        }

        void GenNeedUpdateFilesAndRemoveUselessFiles(Dictionary<string, ResourceInfo> oldResources, Dictionary<string, ResourceInfo> newResources, Dictionary<string, ResourceInfo> needUpdates)
        {
            foreach(var e in oldResources)
            {
                var file = e.Key;
                var oldRi = e.Value;
                ResourceInfo newRi;
                if (!newResources.TryGetValue(file, out newRi))
                {
                    Utils.Delete(Utils.GetDataPath(oldRi.path));
                    Debug.LogFormat("remove not need oldfile:{0}", file);
                }
                else if (newRi.md5 != oldRi.md5)
                {
                    Debug.LogFormat("remove md5(old:{1} new:{2}) not match oldfile:{0}", file, oldRi.md5, newRi.md5);
                    Utils.Delete(Utils.GetDataPath(oldRi.path));
                    needUpdates[file] = newRi;
                }
            }
            foreach(var e in newResources)
            {
                ResourceInfo oldRi;
                if(!oldResources.TryGetValue(e.Key, out oldRi) || !File.Exists(Utils.GetDataPath(e.Value.path)))
                {
                    Debug.LogFormat("add newfile:{0} md5:{1}", e.Key, e.Value.md5);
                    needUpdates[e.Key] = e.Value;
                }

            }
        }

        private int localResourceVersion;
        private string localMd5Md5;

        public IEnumerator NormalUpdate()
        {
            // 比较本地app版本和最新的app版的版本号
            // 主版号 local < remote,触发 强更
            // 版本号 local < remote,不更

            var remote = conf.source;

            if (remote == null)
            {
                Debug.LogErrorFormat("NormalUpdate can't find avaliable source.");
                Main.Error = Main.ErrorCode.NOT_AVALIABLE_SOURCE;
                yield break;
            }

            {

                Debug.LogFormat("appversion  local:{0} remote:{1}", conf.localAppVersion,  remote.appVersion);

                if (conf.localAppVersion < remote.requireMinAppVersion)
                {
                    Debug.LogFormat("NormalUpdate major local:{0} < remote.require:{1}, need upgrade", conf.localAppVersion, remote.requireMinAppVersion);
                    Main.Error = Main.ErrorCode.NEED_UPGRADE_APP;
                    yield break;
                }

                if (conf.localAppVersion > remote.appVersion)
                {
                    Ready = true;
                    Debug.LogFormat("NormalUpdate fullversion local:{0} > remote:{1}, not need update", conf.localAppVersion, remote.appVersion);
                    yield break;
                }

            }

            // 读取本地资源号,如果 local > remote 则不更
            var localManifestFile = Utils.GetPath(conf.manifestFile);
            {
                try
                {
                    var line = Utils.ReadFile(localManifestFile);
                    if (line != null)
                    {
                        var args = line.Split(',');
                        localResourceVersion = int.Parse(args[0]);
                        localMd5Md5 = args[1];
                    }
                    else
                    {
                        localResourceVersion = -1;
                        localMd5Md5 = null;
                    }
                    Debug.LogFormat("resourceversion resource:{2} version:{0} md5md5:{1}", localResourceVersion, localMd5Md5, localManifestFile);

                    if (localResourceVersion > remote.resourceVersion)
                    {
                        Ready = true;
                        Debug.LogFormat("resourceversion local:{0} > remote:{1}, not need update", localResourceVersion, remote.resourceVersion);
                        yield break;
                    }
                } catch(Exception e)
                {
                    Debug.LogException(e);
                    Main.Error = Main.ErrorCode.READ_LOCAL_MANIFEST_FAIL;
                    Debug.LogErrorFormat("read local manifest:{0} fail.", localManifestFile);
                    yield break;
                }
                if(localMd5Md5 == null)
                {
                    Main.Error = Main.ErrorCode.LOCAL_MANIFEST_CORRUPT;
                    yield break;
                }
            }

            {
                var localResources = new Dictionary<string, ResourceInfo>();
                var remoteResources = new Dictionary<string, ResourceInfo>();
                var needUpdateResources = new Dictionary<string, ResourceInfo>();

                var localMd5FilePath = Utils.GetPath(conf.md5File);
                if (localResourceVersion == remote.resourceVersion)
                {
                    var localMd5Md5 = Utils.Md5sum(localMd5FilePath);
                    if (localMd5Md5 != remote.md5OfMd5File)
                    {
                        Debug.LogErrorFormat("local md5md5:{0} not match remote md5md5:{1}. corrupt", localMd5Md5, remote.md5OfMd5File);
                        Main.Error = Main.ErrorCode.LOCAL_MANIFEST_CORRUPT;
                        yield break;
                    }
                    yield return Utils.StartCoroutine(LoadMd5File(localMd5FilePath, localResources));
                    remoteResources = localResources;

                    GenNeedUpdateFilesAndRemoveUselessFiles(localResources, remoteResources, needUpdateResources);
                }
                else
                {
                    var remoteMd5FilePath = localMd5FilePath + ".tmp";
                    yield return Utils.StartCoroutine(DownloadMd5File(remoteMd5FilePath, remote.md5OfMd5File));
                    if (Main.Error != Main.ErrorCode.OK)
                        yield break;

                    yield return Utils.StartCoroutine(LoadMd5File(localMd5FilePath, localResources));
                    yield return Utils.StartCoroutine(LoadMd5File(remoteMd5FilePath, remoteResources));

                    GenNeedUpdateFilesAndRemoveUselessFiles(localResources, remoteResources, needUpdateResources);
                    Utils.Move(remoteMd5FilePath, localMd5FilePath);
                }
                var tmpManifestFile = localManifestFile + ".tmp";
                Utils.WriteFile(tmpManifestFile, string.Format("{0},{1}", remote.resourceVersion, remote.md5OfMd5File));
                Utils.Move(tmpManifestFile, localManifestFile);

                this.coreResourceInfos = needUpdateResources.Values.Where(ri => ri.core).ToDictionary(ri => ri.path);
                this.notCoreResourceInfos = needUpdateResources.Values.Where(ri => !ri.core).ToDictionary(ri => ri.path);
            }


            {
                if (this.coreResourceInfos.Count > 0)
                {
                    yield return DownloadFiles(this.coreResourceInfos);
                    if (Main.Error != Main.ErrorCode.OK)
                        yield break;
                }
            }
            Ready = true;
            yield break;
        }

        private void Walk(string dir, string root, Dictionary<string, ResourceInfo> resources)
        {
            //Debug.LogFormat("walk dir:{0}", dir);
            foreach (string file in Directory.GetFiles(dir))
            {
                var path = file.Substring(root.Length + 1).Replace('\\', '/');
                //Debug.LogFormat("== file:{1}", dir, path);
                var ri = new ResourceInfo();
                ri.path = path;
                ri.core = true;
                ri.size = new FileInfo(file).Length;
                resources.Add(path, ri);
            }

            foreach (string d in Directory.GetDirectories(dir))
            {
                Walk(d, root, resources);
            }
        }

        private IEnumerator RebuildLocalManifest()
        {
            Main.Error = Main.ErrorCode.OK;
            var root = Utils.GetDataPath(".");
            var resources = new Dictionary<string, ResourceInfo>();
            Walk(root, root, resources);

            int remainWorkNum = resources.Count;
            long totalBytes = 0;
            long finishBytes = 0;
            foreach(var ri in resources.Values)
            {
                //var ri = r;
                totalBytes += ri.size;
                var fullPath = Utils.GetDataPath(ri.path);
                System.Threading.ThreadPool.QueueUserWorkItem((a) =>
                {
                    ri.md5 = Utils.Md5sum(fullPath);
                    lock(resources)
                    {
                        --remainWorkNum;
                        finishBytes += ri.size;
                    }
                });
            }
            Debug.LogFormat("RebuildLocalManifest begin file:{0} total:{1} k", resources.Count, totalBytes / 1000);

            var finish = false;
            while (!finish)
            {
                lock(resources)
                {
                    finish = (remainWorkNum == 0);
                }
                yield return null;
            }

            try
            {
                var lines = resources.Values.Select(r => string.Format("{0},{1},1,{2}", r.path, r.md5, r.size)).ToArray();
                var md5File = Utils.GetPath(conf.md5File);
                File.WriteAllLines(md5File, lines, Encoding.UTF8);
                File.WriteAllText(Utils.GetPath(conf.manifestFile), "-1," + Utils.Md5sum(md5File), Encoding.UTF8);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.LogErrorFormat("RebuildLocalMd5s fail.");
                Main.Error = Main.ErrorCode.REPAIR_LOCAL_MANIFEST_FAIL;
            }
            Debug.LogFormat("RebuildLocalManifest end.");
        }

        private IEnumerator DownloadFiles(Dictionary<string, ResourceInfo> resources)
        {
            var dm = DownloadManager.Ins;
            dm.Reset(true);
            foreach (var e in resources)
            {
                var file = e.Key;
                var ri = e.Value;
                Debug.LogFormat("DownloadFiles:{0}", file);
                var task = dm.createTask(rootUrls, "data/" + file, Utils.GetDataPath(file), ri.md5);
                task.Start((t) =>
                {
                    if (task.Status == DownloadManager.TaskStatus.SUCC)
                    {
                        resources.Remove(file);
                    } 
                });
            }
            yield return Utils.StartCoroutine(dm.waitAllDownload());

            if(resources.Count > 0)
            {
                foreach(var file in resources.Keys)
                {
                    Debug.LogErrorFormat("DownloadFiles. file:{0} fail", file);
                }
                Main.Error = Main.ErrorCode.SOME_FILE_DOWNLOAD_FAIL;
            }
        }

        private IEnumerator DownloadMd5File(string localFile, string expectMd5)
        {         
            var dm = DownloadManager.Ins;
            var task = dm.createTask(rootUrls, conf.md5File, localFile, expectMd5);
            task.Start();
            while (!task.Done)
                yield return null;
            if (task.Status != DownloadManager.TaskStatus.SUCC)
            {
                Main.Error = Main.ErrorCode.DOWNLOAD_MD5_FAIL;
                yield break;
            }
        }

        public IEnumerator RepaireUpdate()
        {
            yield return Utils.StartCoroutine(RebuildLocalManifest());
            if (Main.Error != Main.ErrorCode.OK)
                yield break;
            yield return Utils.StartCoroutine(NormalUpdate());
        }


        public IEnumerator StartRemainUpdate()
        {
            Debug.Log("StartRemainUpdate begin");
            if (this.notCoreResourceInfos != null && this.notCoreResourceInfos.Count > 0)
            {
                yield return Utils.StartCoroutine(DownloadFiles(this.notCoreResourceInfos));
            }
            Debug.Log("StartRemainUpdate end");
        }

        public void Update()
        {

        }
    }

