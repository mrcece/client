using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;


    class Utils
    {
        private class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                var req = base.GetWebRequest(address);
                //UnityEngine.Debug.LogFormat("uri:{0} req:{1}", address, req);
                HttpWebRequest request =  req as HttpWebRequest;
                if (request != null)
                {
                    request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                }
                return req;
            }
        }

        public static WebClient NewWebClient()
        {
            return new MyWebClient();
        }

        public static string ToHexString(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte bt in bytes)
            {
                sb.Append(bt.ToString("x2"));
            }
            return sb.ToString();
        }

        public static string Md5sum(string file)
        {
            try
            {
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    using (var stream = File.OpenRead(file))
                    {
                        return ToHexString(md5.ComputeHash(stream));
                    }
                }
            } catch(Exception e)
            {
                UnityEngine.Debug.LogException(e);
                return null;
            }
        }

        public static string Md5sum(byte[] bytes)
        {
            return ToHexString(System.Security.Cryptography.MD5.Create().ComputeHash(bytes));
        }

        public static void CreateDirectory(string dir)
        {
            var directory = new DirectoryInfo(dir);
            if (!directory.Exists)
            {
                directory.Create();
            }
        }

        public static void DeleteDirectory(string toRemoveDir)
        {
            string[] files = Directory.GetFiles(toRemoveDir);
            string[] dirs = Directory.GetDirectories(toRemoveDir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(toRemoveDir, false);
        }

        public static void CreateParentDirectory(string file)
        {
            //Debug.LogFormat("download file:{0} bytes:{1}", savePath, bytes.Length);
            var directory = Directory.GetParent(file);
            if (!directory.Exists)
            {
                directory.Create();
            }
        }

        public static void RemoveAndCreateDirectory(string dir)
        {
            var directory = new DirectoryInfo(dir);
            if (directory.Exists)
            {
                DeleteDirectory(directory.FullName);
            }
            directory.Create();
        }

        public static void Replace(string srcFile, string dstFile)
        {
            UnityEngine.Debug.LogFormat("Repace src:{0} dst:{1}", srcFile, dstFile);
            if (!File.Exists(dstFile))
            {
                File.Move(srcFile, dstFile);
            }
            else
            {
                var backupFile = srcFile + ".tmp.2";
                File.Replace(srcFile, dstFile, backupFile);
                File.Delete(backupFile);
            }
        }

        public static void Move(string srcFile, string dstFile)
        {
            // 这么写是为了规避Sharing Violation
            // c#很淡疼的地方
            if (File.Exists(dstFile))
                File.Delete(dstFile);
            File.Move(srcFile, dstFile);
            //File.Delete(srcFile);
        }

        public static void Copy(string srcFile, string dstFile)
        {
            if (File.Exists(dstFile))
                File.Delete(dstFile);
            File.Copy(srcFile, dstFile);
        }

        public static void Delete(string dstFile)
        {
            UnityEngine.Debug.LogFormat("delete file:{0}", dstFile);
            if (File.Exists(dstFile))
            {
                File.Delete(dstFile);
            }
        }

        public static string ReadFile(string file)
        {
            try
            {
                return File.Exists(file) ? File.ReadAllText(file, Encoding.UTF8) : null;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogErrorFormat("ReadFile file:{0} fail", file);
                UnityEngine.Debug.LogException(e);
                return null;
            }
        }

        public static void WriteFile(string file, string content)
        {
            try
            {
                File.WriteAllText(file, content, Encoding.UTF8);
            } catch (Exception e)
            {
                UnityEngine.Debug.LogErrorFormat("WriteFile file:{0} fail", file);
                UnityEngine.Debug.LogException(e);
            }
        }

        public static UnityEngine.Coroutine StartCoroutine(System.Collections.IEnumerator cor)
        {
            return Main.Ins.StartCoroutine(cor);
        }

        public static string GetPath(string path)
        {
            return Platform.PersistentDataPath + "/" + path;
        }

        public static string GetDataPath(string path)
        {
            return Platform.PersistentDataPath + "/data/" + path;
        }

        public static string GetLuaPath(string path)
        {
            return Platform.PersistentDataPath + "/data/scripts/" + path.Replace('.', '/') + ".lua";
        }
    }

