using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aio
{
    class LuaCoder : ICoder
    {
        public LuaCoder()
        {
            message.maxSize = 512 * 1024;
        }

        private readonly BinaryStream.Msg message = new BinaryStream.Msg();
        public void Decode(BinaryStream bs, Action<object> onMsg)
        {
            if(bs.TryUnmarshalBytes(message) == BinaryStream.UnmarshalError.OK)
            {
                onMsg(message.body);
            }
        }

        public void Encode(BinaryStream bs, object msg)
        {
            bs.MarshalBinaryStream((BinaryStream)msg);
        }
    }
}
