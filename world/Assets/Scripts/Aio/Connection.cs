using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Aio
{

    public interface IConnectionEvent
    {
        void onClose(Connection c, bool isAbort);
        void onConnected(Connection c);
        void onRecv(Connection c, object msg);
    }

    public sealed class Connection
    {
        public sealed class Builder
        {
            public string host;
            public int port;
            public ICoder coder;
            public IConnectionEvent callback;
            public int recvBufferSize;
            public int sendBufferSize;
        }

        enum State
        {
            NOT_CONNECT,
            CONNECTING,
            CONNECTED,
            CLOSED,
        }

        private readonly Socket socket;

        private readonly IPAddress address;
        private readonly int port;

        private readonly ICoder coder;
        private readonly IConnectionEvent callback;

        private readonly BinaryStream inStream = new BinaryStream();
        private readonly BinaryStream outStream = new BinaryStream();

        private State state;


        public static Connection NewConnection(Builder b)
        {
            IPAddress address;
            if (!IPAddress.TryParse(b.host, out address))
            {
                IPAddress[] addresses = Dns.GetHostAddresses(b.host);
                if (addresses == null || addresses.Length == 0)
                {
                    UnityEngine.Debug.LogError("can not get ipaddress by host, host is " + b.host);
                    return null;
                }
                address = addresses[0];
            }
            var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                SendBufferSize = b.sendBufferSize,
                ReceiveBufferSize = b.recvBufferSize,
                NoDelay = true,
            };
            return new Connection(socket, address, b.port, b.coder, b.callback);
        }

        private Connection(Socket socket, IPAddress address, int port, ICoder coder, IConnectionEvent callback)
        {
            this.socket = socket;
            this.address = address;
            this.port = port;
            this.state = State.NOT_CONNECT;
            this.coder = coder;
            this.callback = callback;

            this.delegateOnRecv = this.OnRecv;
        }

        public void Connect()
        {
            lock(this)
            {
                if (state != State.NOT_CONNECT) return;
                state = State.CONNECTING;

                try
                {
                    socket.BeginConnect(address, port, ar =>
                    {
                        state = State.CONNECTED;
                        socket.EndConnect(ar);
                        callback.onConnected(this);
                        BeginRead();
                    }, null);
                } catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                    Close(true);
                }
            }
        }

        public void Close(bool abort)
        {
            lock (this)
            {
                if (state == State.CLOSED) return;
                try
                {
                    socket.Close();
                } catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
                finally
                {
                    state = State.CLOSED;
                    callback.onClose(this, abort);
                }
            }
        }

        private void BeginRead()
        {
            lock(this)
            {
                inStream.WriteSure(1024);
                UnityEngine.Debug.LogFormat("beginread writeindex:{0} writable:{1}", inStream.WriteIndex, inStream.NotCompactWritable);
                socket.BeginReceive(inStream.Bytes, inStream.WriteIndex, inStream.NotCompactWritable, SocketFlags.None, ar =>
                {
                    try
                    {
                        var n = socket.EndReceive(ar);
                        if (n > 0)
                        {

                            lock (this)
                            {
                                inStream.AddWriteIndex(n);
                            }
                            // 反正所有消息都要放到lua里解析,索性这儿不decode了
                            // coder.Decode(inStream, msgs);
                            BeginRead();
                        } else
                        {
                            Close(false);
                        }
                    } catch(Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                        Close(false);
                    }

                }, null);
            }
        }

        private readonly Action<object> delegateOnRecv;
        public void Read()
        {
            lock(this)
            {
                try
                {
                    if (inStream.Remaining == 0) return;
                    UnityEngine.Debug.LogFormat("== recv {0}", inStream.ToString());
                    coder.Decode(inStream, delegateOnRecv);
                } catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                    Close(false);
                    return;
                }
            }
            /*
            foreach (var msg in msgs)
            {
                try
                {
                    callback.onRecv(this, msg);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
            msgs.Clear();
            */
        }

        private void OnRecv(object msg)
        {
            callback.onRecv(this, msg);
        }

        public void Write(object msg)
        {
            lock (this)
            {
                if (state != State.CONNECTED) return;
                var needBeginWrite = outStream.Remaining == 0;
                coder.Encode(outStream, msg);
                if (needBeginWrite)
                    BeginWrite();
            }
        }

        public void BeginWrite()
        {
            lock(this)
            {
                try
                {
                    outStream.CompactBuffer();
                    socket.BeginSend(outStream.Bytes, outStream.ReadIndex, outStream.Remaining, SocketFlags.None, ar =>
                    {
                        try
                        {
                            var n = socket.EndSend(ar);
                            if (n > 0)
                            {
                                lock (this)
                                {
                                    outStream.AddReadIndex(n);
                                    if (outStream.Remaining > 0)
                                        BeginWrite();
                                }
                            }
                            else
                            {
                                Close(false);
                            }
                        } catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                            Close(false);
                        }
                    }, null);
                } catch(Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                    Close(false);
                }
            }
        }
    }

}
