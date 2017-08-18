using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LuaMonitor))]
public class LuaMonitorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LuaMonitor myScript = (LuaMonitor)target;
        if (GUILayout.Button("gc"))
        {
            var luaState = LuaClient.GetMainState();
            if (luaState != null)
            {
                var cg = luaState.GetFunction("collectgarbage");
                cg.Call("collect");
            }
        }
    }
}