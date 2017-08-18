using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



    public class Platform
    {
        public static string PlatformName { get { return "ios"; } }
        public static int AppVersion {  get { return 200; } }
        public static int MinorVersion {  get { return 0; } }
        //        public static string PersistentDataPath { get { return UnityEngine.Application.persistentDataPath; } }
#if CLIENT
        public static string PersistentDataPath { get { return UnityEngine.Application.dataPath + "/../../data"; } }
        public static bool IgnoreVersionCheck { get { return true; } }
#else
        public static string PersistentDataPath { get { return UnityEngine.Application.persistentDataPath; } }
        public static bool IgnoreVersionCheck { get { return false; } }
#endif

    }


