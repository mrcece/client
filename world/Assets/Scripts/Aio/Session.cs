using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;

namespace Aio
{

    public interface ICallback
    {
        void OnConnected();

        void OnAbort();

        void OnDisconnect();
    }

    // 如不特殊说明,
    // 所有接口只能unity主线程操作
    // 
    public sealed class Session //: ICallback
    {

    }
}

