using Aio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;


public class NetManager : IConnectionEvent
{
    public readonly static NetManager Ins = new NetManager();

    public void Start()
    {
        Main.Ins.Updater += Update;
    }

    public void Update()
    {
        if (link != null)
            link.Read();
        lock(actions)
        {
            foreach(var action in actions)
            {
                try
                {
                    action();
                } catch(Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
            actions.Clear();
        }
    }

    private readonly List<Action> actions = new List<Action>();

    private Connection link = null;
    public void Connect(string host, int port)
    {
        if (link != null)
            link.Close(false);

        var b = new Connection.Builder()
        {
            host = host,
            port = port,
            sendBufferSize = 8192,
            recvBufferSize = 8192,
            coder = new LuaCoder(),
            callback = this,
        };

        link = Connection.NewConnection(b);
        link.Connect();
    }

    private void async(Action action)
    {
        lock (actions)
        {
            actions.Add(action);
        }
    }

    public void onClose(Connection c, bool isAbort)
    {
        UnityEngine.Debug.Log("OnClose");
        async(() =>
            {
                if (link != c) return;
                LuaManager.OnClose();
            });
    }

    public void onConnected(Connection c)
    {
        async(() =>
        {
            UnityEngine.Debug.Log("onConnect");
            if (link != c) return;
            LuaManager.OnConnect();
        });
    }

    // 这个Read时触发的,不用async
    public void onRecv(Connection c, object msg)
    {
        UnityEngine.Debug.Log("OnRecv");
        LuaManager.RecvMsg(msg);
    }

    public void Send(object msg)
    {
        if (link != null)
            link.Write(msg);
    }
}

