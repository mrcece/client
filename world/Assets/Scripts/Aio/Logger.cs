using Aio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Aio
{
    /*
    public delegate void LoggerExceptionHandler(string msg, Exception e);

    public delegate void UdpQueueOverflowHandler(long rolename, String msg);

    public sealed class Logger
    {
        private UdpClient _sender;
        private IPEndPoint _remoteEp;
        private ConcurrentQueue<Aio.Action> _actions = new ConcurrentQueue<Aio.Action>();
        private readonly Stopwatch _frameWatcher = new Stopwatch();
        private List<string> _secondChanceList = new List<string>();
 
        private int _maxUdpQueueCount = 512;
        private int _secondChanceCapacity = 512;
        private LoggerExceptionHandler _exceptionHandler;
        private UdpQueueOverflowHandler _udpQueueOverflowHandler;

        public Logger(String remoteIp, int remotePort)
        {
            _sender = new UdpClient();
            _remoteEp = new IPEndPoint(IPAddress.Parse(remoteIp), remotePort);
        }

        public void setMaxUdpQueueCount(int maxUdpQueueCount)
        {
            _maxUdpQueueCount = maxUdpQueueCount;
        }

        public void setSecondChanceCapacity(int secondChanceCapacity)
        {
            _secondChanceCapacity = secondChanceCapacity;
        }

        public void setLoggerExceptionHandler(LoggerExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler;
        }

        public void setUdpQueueOverflowHandler(UdpQueueOverflowHandler udpQueueOverflowHandler)
        {
            _udpQueueOverflowHandler = udpQueueOverflowHandler;
        }

        public void process(long maxMilliseconds)
        {
            _frameWatcher.Reset();

            while (_frameWatcher.ElapsedMilliseconds < maxMilliseconds)
            {
                Aio.Action action;
                if (_actions.TryDequeue(out action))
                {
                    action();
                }
                else
                {
                    break;
                }
            }

        }

        public void Log(long rolename, string s)
        {
            Log(rolename, s, null);
        }

        public void Log(long rolename, string s, Exception e)
        {
            var sb = new StringBuilder();
            if (s != null)
            {
                sb.Append(s);
            }
            if (e != null)
            {
                if (sb.Length > 0)
                {
                    sb.Append(" ");
                }
                sb.Append(e);
            }
            DoLog(rolename, sb.ToString());
        }


        private void DoLog(long rolename, String msg)
        {
            if (_actions.Count > _maxUdpQueueCount)
            {
                if (_udpQueueOverflowHandler != null)
                {
                    _udpQueueOverflowHandler(rolename, msg);
                }
            }
            else
            {
                _actions.Enqueue(() => DoSend(rolename + "@" + msg + "@" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), true));
            }
        }

        private void DoSend(string str, bool firstTime)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                _sender.Connect(_remoteEp);
                _sender.BeginSend(bytes, bytes.Length, ar =>
                {
                    try
                    {
                        _sender.EndSend(ar);

                        foreach (var s in _secondChanceList)
                        {
                            var second = s;
                             _actions.Enqueue(() => DoSend(second, false));
                        }
                        _secondChanceList.Clear();
                    }
                    catch (Exception e)
                    {
                        GiveSecondChance(str, e, firstTime);
                    }
                }, null);
            }
            catch (Exception e)
            {
                GiveSecondChance(str, e, firstTime);
            }
        }

        private void GiveSecondChance(string str, Exception cause, bool firstTime)
        {
            if (firstTime)
            {
                _secondChanceList.Add(str);
                if (_secondChanceList.Count > _secondChanceCapacity)
                {
                    _secondChanceList.RemoveAt(0);
                }
            }
            else
            {
                if (_exceptionHandler != null)
                {
                    _exceptionHandler(str, cause);
                }
            }
        }


        public void Close() 
        {
            _sender.Close();
        }
    }
    */

}
