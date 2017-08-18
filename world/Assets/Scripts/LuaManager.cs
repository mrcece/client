using LuaInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class LuaManager : LuaClient
{
    private static LuaFunction recvMsg;
    private static LuaFunction onClose;
    private static LuaFunction onConnect;


    protected override void StartMain()
    {
        base.StartMain();

        recvMsg = luaState.GetFunction("RecvMsg");
        onConnect = luaState.GetFunction("OnConnect");
        onClose = luaState.GetFunction("OnClose");

        gameObject.AddComponent<LuaMonitor>();
    }

    public static void Connect(string host, int port)
    {
        NetManager.Ins.Connect(host, port);
    }


    public static void RecvMsg(object msg)
    {
        recvMsg.Call(msg);
    }

    public static void SendMsg(object msg)
    {
        NetManager.Ins.Send(msg);
    }

    internal static void OnConnect()
    {
        onConnect.Call();
    }

    internal static void OnClose()
    {
        onClose.Call();
    }
}

