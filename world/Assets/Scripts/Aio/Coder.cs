using System;
using System.Collections.Generic;

namespace Aio
{
    public interface ICoder
    {
        void Encode(BinaryStream bs, object msg);
        void Decode(BinaryStream bs, Action<object> onMsg);
    }
  /*
    public sealed class Coder
    {
        
        internal void Decode(List<object> msgs, OctetsStream os)
        {
          
            while (os.Remaining > 0)
            {
                int tranpos = os.Begin();
                try
                {
                    int type = os.UnmarshalSize();
                    int size = os.UnmarshalSize();

                    Stub stub;
                    if (_stubmap.TryGetValue(type, out stub))
                    {
                        if (size > os.Remaining)
                        {
                            //m_Log.Debug("decode rollback " + type + ", " + size + ", " + os.Data.ToHexString());
                            os.Rollback(tranpos);
                            break; // not enough
                        }
                        //m_Log.Debug("decode " + type + ", " + size);
                        int startpos = os.Position;
                        IProtocol p = stub.CreateProtocol();
                        try
                        {
                            p.Unmarshal(os);
                        }
                        catch (MarshalException e)
                        {
                            throw new CodecException("State.decode (" + type + ", " + size + ")", e);
                        }
                        if (p is SPing)
                        {
                            var ping = p as SPing;
                            ping.recvclienttime = Utils.CurrentTimeMillis();
                            var data = new OctetsStream().Marshal(p);
                            protocols.Add(LuaProtocol.Create(p.ProtocolType, data.Remaining, data));
                        }
                        else
                        {
                            protocols.Add(p);
                        }

                        if ((os.Position - startpos) != size)
                            throw new CodecException("State.decode(" + type + ", " + size + ")=" + (os.Position - startpos));
                    }
                    else
                    {
                        //throw new CodecException("unknown protocol (" + type + ", " + size + ")");
                        // 未知协议由lua来处理
                        if (size > os.Remaining)
                        {
                            //m_Log.Debug("decode rollback " + type + ", " + size + ", " + os.Data.ToHexString());
                            os.Rollback(tranpos);
                            break; // not enough
                        }
                        protocols.Add(LuaProtocol.Create(type, size, os));
                    }
                }
                catch (MarshalException)
                {
                    //m_Log.Debug("decode rollback " + os.Data.ToHexString());
                    os.Rollback(tranpos);
                    break;
                }
            }
        }
       
    } */
}
