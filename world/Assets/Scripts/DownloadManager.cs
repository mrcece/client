using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


    public class DownloadManager
    {
        public readonly static DownloadManager Ins = new DownloadManager();

        public class Config
        {
            public string tmpDirectory;
            public int maxDownloadingTaskNum;
        }

        private string tmpDirectory;
        private int maxDownloadingTaskNum;

        public void Init(Config conf)
        {
            maxDownloadingTaskNum = conf.maxDownloadingTaskNum;
            tmpDirectory = conf.tmpDirectory;
            Main.Ins.Updater += Update;

            Reset(true);
        }

        public int MaxDownloadingTaskNum { set { maxDownloadingTaskNum = value; } }

        public long DownloadingBytes { get { return downloadingTasks.Select(t => t.BytesDownloaded).Sum(); } }

        public void Reset(bool removeTmpDirectory)
        {
            downloadingTasks.Clear();
            failTasks.Clear();
            asyncWorks.Clear();
            if (removeTmpDirectory)
                Utils.RemoveAndCreateDirectory(tmpDirectory);
        }

        private ConcurrentQueue<Action> asyncWorks = new ConcurrentQueue<Action>();

        public void Update()
        {
            Action action;
            while(asyncWorks.TryDequeue(out action))
            {
                try
                {
                    action();
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public enum TaskStatus
        {
            WAIT,
            DOWNLOADING,
            SUCC,
            FAIL,
            MD5_ERROR,
        }

        public class Task : IComparable<Task>
        {
            public const int DEFAULT_PRIORITY = 10000;
            public readonly List<string> alternativeSourceUrls;
            public readonly string relateDownloadFile;
            public readonly string localSaveFile;
            public readonly string expectMd5;

            public int pririty;

            private bool done;
            private TaskStatus status;

            public volatile int BytesDownloaded;

            public bool InWait { get { return status == TaskStatus.WAIT; } }
            public bool Done { get { return done; } set { done = value; } }
            public TaskStatus Status { get { return status; } set { status = value; } }

            public event Action<Task> OnDoneCallback;
            

            public Task(List<string> alternativeSourceUrls, string relateDownloadFile, string localSaveFile, string expectMd5, int priority)
            {
                this.alternativeSourceUrls = alternativeSourceUrls;
                this.relateDownloadFile = relateDownloadFile;
                this.localSaveFile = localSaveFile;
                this.expectMd5 = expectMd5;
                this.pririty = priority;
                this.done = false;
                this.status = TaskStatus.WAIT;
            }

            public void Start()
            {
                Start(null);
            }

            public void Start(Action<Task> onDoneCallback)
            {
                //Boot.Ins.StartCoroutine(StartAsCoroutineWaitDone(onDone));
                if(onDoneCallback != null)
                    OnDoneCallback += onDoneCallback;
                Ins.Add(this);
            }

            public int CompareTo(Task other)
            {
                return pririty.CompareTo(other.pririty);
            }

            public void DoDone()
            {
                if (OnDoneCallback != null)
                {
                    try
                    {
                        OnDoneCallback(this);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }

        public Task createTask(List<string> alternativeSourceUrls, string relateDownloadFile, string localSaveFile, string expectMd5, int priority)
        {
            return new Task(alternativeSourceUrls, relateDownloadFile, localSaveFile, expectMd5, priority);
        }

        public Task createTask(List<string> alternativeSourceUrls, string relateDownloadFile, string localSaveFile, string expectMd5)
        {
            return new Task(alternativeSourceUrls, relateDownloadFile, localSaveFile, expectMd5, Task.DEFAULT_PRIORITY);
        }


        private Priority_Queue.SimplePriorityQueue<Task> tasks = new Priority_Queue.SimplePriorityQueue<Task>();
        private void Add(Task task)
        {
            tasks.Enqueue(task, task.pririty);
            CheckNewDownload();
        }

        public void UpdatePriority(Task task, int priority)
        {
            if(tasks.Contains(task))
            {
                task.pririty = priority;
                tasks.UpdatePriority(task, priority);
            }
        }

        private List<Task> downloadingTasks = new List<Task>();
        private List<Task> failTasks = new List<Task>();


        public IEnumerator waitAllDownload()
        {
            while (tasks.Count > 0 || downloadingTasks.Count > 0)
                yield return null;
        }

        private void CheckNewDownload()
        {
            while(downloadingTasks.Count < maxDownloadingTaskNum && tasks.Count > 0)
            {
                var newTask = tasks.Dequeue();
                downloadingTasks.Add(newTask);
                Main.Ins.StartCoroutine(download(newTask));
            }
        }

        private IEnumerator download(Task task)
        {
            yield return Main.Ins.StartCoroutine(doDownload(task));
            downloadingTasks.Remove(task);
            if(task.Status != TaskStatus.SUCC)
            {
                failTasks.Add(task);
            }
            task.DoDone();
            CheckNewDownload();
        }


        int tempFileId = 0;

        private static readonly Exception MD5_NOT_MATCH = new Exception("md5 not match");
        private IEnumerator doDownload(Task task)
        {
            var saveFile = task.localSaveFile;
            var tmpFile = tmpDirectory + "/" + (++tempFileId).ToString();
            Utils.CreateParentDirectory(saveFile); 
            //UnityEngine.Debug.Log("download start :" + task.relateDownloadFile);
            task.Status = TaskStatus.DOWNLOADING;
            foreach(var srcUrl in task.alternativeSourceUrls)
            {
                var fileUrl = srcUrl + "/" + task.relateDownloadFile;
                Debug.Log("download try " + fileUrl);
                var client = Utils.NewWebClient();
                task.Done = false;
                client.DownloadProgressChanged += (a, b) =>
                {
                        task.BytesDownloaded = (int)b.BytesReceived;                    
                };
                client.DownloadFileCompleted += (a, b) =>
                {
                    var err = b.Error;
                    var newMd5Str = "";
                    client.Dispose();
                    if (err == null)
                    {
                        try
                        {
                            newMd5Str = Utils.Md5sum(tmpFile);
                            
                            if (newMd5Str == null || (task.expectMd5 != null && task.expectMd5 != newMd5Str))
                            {
                                Debug.LogErrorFormat("download file:{0} expect md5:{1} actuallymd5:{2}", task.localSaveFile, task.expectMd5, newMd5Str);
                                err = MD5_NOT_MATCH;
                            }
                            else
                            {
                                Utils.Move(tmpFile, saveFile);
                            }
                        } catch (Exception e)
                        {
                            err = e;
                        }
                    }       
                    asyncWorks.Enqueue(() =>
                    {
                        task.Done = true;
                        if (err == null)
                        {
                            task.Status = TaskStatus.SUCC;
                            task.BytesDownloaded = (int)(new FileInfo(saveFile).Length);
                        }
                        else
                        {
                            Debug.LogErrorFormat("download fail. url:{0} save:{1}", fileUrl, saveFile);
                            if(err != MD5_NOT_MATCH)
                                Debug.LogException(err);
                        }
                    });
                };
                client.DownloadFileAsync(new Uri(fileUrl), tmpFile);              
                while (!task.Done)
                    yield return null;
                if (task.Status == TaskStatus.SUCC)
                    yield break;
            }
            task.Status = TaskStatus.FAIL;
        }

    }

