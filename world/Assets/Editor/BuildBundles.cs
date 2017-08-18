using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

class BuildBundles
{
    static string baseAssetBundlePath = Application.dataPath + "/../../scripts/bundles";
    [MenuItem("Bundle/Build Win")]
    static void BuildWinAssetBundles()
    {
        BuildAllAssetBundles("win", BuildTarget.StandaloneWindows64);
    }

    [MenuItem("Bundle/Build Android")]
    static void BuildAndroidAssetBundles()
    {
        BuildAllAssetBundles("android", BuildTarget.Android);

    }

    static void BuildAllAssetBundles(string platform, BuildTarget target)
    {
        string assetBundleDirectory = baseAssetBundlePath + "/" + platform;
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, 
            BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle,
            target);
      
    }
}

