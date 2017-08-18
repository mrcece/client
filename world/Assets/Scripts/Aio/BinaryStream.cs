using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Aio
{
    public sealed class BinaryStream
    {
        private byte[] bytes;
        private int readIndex;
        private int writeIndex;
        private int capacity;

        public BinaryStream() : this(16)
        {

        }

        public BinaryStream(int capacity)
        {
            this.capacity = capacity;
            bytes = new byte[capacity];
            readIndex = 0;
            writeIndex = 0;
        }

        public BinaryStream(byte[] bytes)
        {
            capacity = bytes.Length;
            this.bytes = bytes;
            readIndex = 0;
            writeIndex = capacity;
        }

        public BinaryStream(byte[] bytes, int readIndex, int writeIndex)
        {
            capacity = bytes.Length;
            this.bytes = bytes;
            this.readIndex = readIndex;
            this.writeIndex = writeIndex;
        }


        
        public int ReadIndex {  get { return readIndex; } }
        public int WriteIndex {  get { return writeIndex; } }
        public void AddWriteIndex(int add)
        {
            writeIndex += add;
        }

        public void AddReadIndex(int add)
        {
            readIndex += add;
        }

        public byte[] Bytes { get { return bytes; } }

        public byte[] toArray()
        {
            var n = Remaining;
            var arr = new byte[n];
            Buffer.BlockCopy(bytes, readIndex, arr, 0, n);
            return arr;
        }

        public int Remaining { get { return writeIndex - readIndex; } }
        public int Capacity { get { return capacity; } }

        public void CompactBuffer()
        {
            WriteSure(capacity + readIndex - WriteIndex);
        }

        public int NotCompactWritable { get { return capacity - writeIndex; } }

        public void WriteBytes(byte[] bs, int offset, int  len)
        {
            WriteSure(len);
            Buffer.BlockCopy(bs, offset, bytes, writeIndex, len);
            writeIndex += len;
        }

        public void Clear()
        {
            readIndex = writeIndex = 0;
        }


        private int PropSize(int initSize, int needSize)
        {
            for(int i = Math.Max(initSize, 16); ; i <<= 1)
            {
                if (i >= needSize) return i;
            }
        }
        
        public void WriteSure(int size)
        {
            if(writeIndex + size > capacity)
            {
                var needSize = writeIndex + size - readIndex;
                if (needSize < capacity)
                {
                    writeIndex -= ReadIndex;
                    Array.Copy(bytes, readIndex, bytes, 0, writeIndex);
                    readIndex = 0;
                }
                else
                {
                    capacity = PropSize(capacity, needSize);
                    var newBytes = new byte[capacity];
                    writeIndex -= readIndex;
                    Buffer.BlockCopy(bytes, ReadIndex, newBytes, 0, writeIndex);
                    readIndex = 0;
                    bytes = newBytes;
                }
            }
        }

        private static readonly Exception UNMARSHAL_NOT_ENOUGH_EXCEPTION = new Exception("unmarshal not enough exception");
        private static readonly Exception UNMARSHAL_SIZE_EXCEPTION = new Exception("unmarshal size < 0 exception");

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReadSure(int size)
        {
            if (readIndex + size > writeIndex)
                throw UNMARSHAL_NOT_ENOUGH_EXCEPTION;
        }

        public void Append(byte x)
        {
            WriteSure(1);
            bytes[writeIndex++] = x;
        }

        public void MarshalBool(bool b)
        {
            WriteSure(1);
            bytes[writeIndex++] = (byte)(b ? 1 : 0);
        }

        public bool UnmarshalBool()
        {
            ReadSure(1);
            return bytes[readIndex++] != 0;
        }

        public void MarshalByte(byte x)
        {
            WriteSure(1);
            bytes[writeIndex++] = x;
        }

        public byte UnmarshalByte()
        {
            ReadSure(1);
            return bytes[readIndex++];
        }

        // marshal int 
        // n -> (n << 1) ^ (n >> 31)
        // unmarshal
        // (x >>> 1) ^ ((x << 31) >> 31)
        // (x >>> 1) ^ -(n&1)

        // marshal long
        // n -> (n << 1) ^ (n >> 63)
        // unmarshal
        // (x >>> 1) ^((x << 63) >> 63)
        // (x >>> 1) ^ -(n&1L)

        public void MarshalInt(int x)
        {
            MarshalUint((uint)x);
        }

        public int UnmarshalInt()
        {
            return (int)UnmarshalUint();
        }

        //private static ByteFloat bytefloat = new ByteFloat();
        [StructLayout(LayoutKind.Explicit)]
        struct ByteFloat
        {
            [FieldOffset(0)] public float vf;
            [FieldOffset(0)] public double vd;
            [FieldOffset(0)] public int vi;
            [FieldOffset(0)] public long vl;

            [FieldOffset(0)] public byte b1;
            [FieldOffset(1)] public byte b2;
            [FieldOffset(2)] public byte b3;
            [FieldOffset(3)] public byte b4;
            [FieldOffset(0)] public byte b5;
            [FieldOffset(1)] public byte b6;
            [FieldOffset(2)] public byte b7;
            [FieldOffset(3)] public byte b8;
        }

        private unsafe void MarshalUint(uint x)
        {
            // 0 111 1111
            if(x < 0x80)
            {
                WriteSure(1);
                bytes[writeIndex++] = (byte)x;
            } else if(x < 0x4000) // 10 11 1111, -
            {            
                WriteSure(2);
                bytes[writeIndex + 1] = (byte)x;
                bytes[writeIndex    ] = (byte)((x >> 8) | 0x80);
                writeIndex += 2;
            } else if(x < 0x200000) // 110 1 1111, -,-
            {
                WriteSure(3);
                bytes[writeIndex + 2] = (byte)x;
                bytes[writeIndex + 1] = (byte)(x >> 8);
                bytes[writeIndex    ] = (byte)((x >> 16) | 0xc0);
                writeIndex += 3;
            } else if(x < 0x10000000) // 1110 1111,-,-,-
            {
                WriteSure(4);
                bytes[writeIndex + 3] = (byte)x;
                bytes[writeIndex + 2] = (byte)(x >> 8);
                bytes[writeIndex + 1] = (byte)(x >> 16);
                bytes[writeIndex    ] = (byte)((x >> 24) | 0xe0);
                writeIndex += 4;
            } else
            {
                WriteSure(5);
                bytes[writeIndex] = 0xf0;
                bytes[writeIndex + 4] = (byte)x;
                bytes[writeIndex + 3] = (byte)(x >> 8);
                bytes[writeIndex + 2] = (byte)(x >> 16);
                bytes[writeIndex + 1] = (byte)(x >> 24);
                writeIndex += 5;
            }
        }

        public uint UnmarshalUint()
        {
            ReadSure(1);
            uint h = bytes[readIndex];
            if(h < 0x80)
            {
                readIndex++;
                return h;
            } else if(h < 0xc0)
            {
                ReadSure(2);
                uint x = ((h & 0x3f) << 8) | bytes[readIndex + 1];
                readIndex += 2;
                return x;
            } else if(h < 0xe0)
            {
                ReadSure(3);
                uint x = ((h & 0x1f) << 16) | ((uint)bytes[readIndex + 1] << 8) | bytes[readIndex + 2];
                readIndex += 3;
                return x;
            } else if(h < 0xf0)
            {

                ReadSure(4);
                uint x = ((h & 0x0f) << 24) | ((uint)bytes[readIndex + 1] << 16) | ((uint)bytes[readIndex + 2] << 8) | bytes[readIndex + 3];
                readIndex += 4;
                return x;
            } else
            {
                ReadSure(5);
                uint x = ((uint)bytes[readIndex + 1] << 24) | ((uint)(bytes[readIndex + 2] << 16)) | ((uint)bytes[readIndex + 3] << 8) | ((uint)bytes[readIndex + 4]);
                readIndex += 5;
                return x;
            }
        }


        public void MarshalLong(long x)
        {
            MarshalUlong((ulong)x);
        }

        public long UnmarshalLong()
        {
            return (long)UnmarshalUlong();
        }

        private void MarshalUlong(ulong x)
        {
            // 0 111 1111
            if (x < 0x80)
            {
                WriteSure(1);
                bytes[writeIndex++] = (byte)x;
            }
            else if (x < 0x4000) // 10 11 1111, -
            {
                WriteSure(2);
                bytes[writeIndex + 1] = (byte)x;
                bytes[writeIndex] = (byte)((x >> 8) | 0x80);
                writeIndex += 2;
            }
            else if (x < 0x200000) // 110 1 1111, -,-
            {
                WriteSure(3);
                bytes[writeIndex + 2] = (byte)x;
                bytes[writeIndex + 1] = (byte)(x >> 8);
                bytes[writeIndex] = (byte)((x >> 16) | 0xc0);
                writeIndex += 3;
            }
            else if (x < 0x10000000) // 1110 1111,-,-,-
            {
                WriteSure(4);
                bytes[writeIndex + 3] = (byte)x;
                bytes[writeIndex + 2] = (byte)(x >> 8);
                bytes[writeIndex + 1] = (byte)(x >> 16);
                bytes[writeIndex] = (byte)((x >> 24) | 0xe0);
                writeIndex += 4;
            }
            else if (x < 0x800000000L) // 1111 0xxx,-,-,-,-
            {
                WriteSure(5);
                bytes[writeIndex + 4] = (byte)x;
                bytes[writeIndex + 3] = (byte)(x >> 8);
                bytes[writeIndex + 2] = (byte)(x >> 16);
                bytes[writeIndex + 1] = (byte)(x >> 24);
                bytes[writeIndex    ] = (byte)((x >> 32) | 0xf0);
                writeIndex += 5;
            } else if(x < 0x40000000000L) // 1111 10xx, 
            {
                WriteSure(6);
                bytes[writeIndex + 5] = (byte)x;
                bytes[writeIndex + 4] = (byte)(x >> 8);
                bytes[writeIndex + 3] = (byte)(x >> 16);
                bytes[writeIndex + 2] = (byte)(x >> 24);
                bytes[writeIndex + 1] = (byte)(x >> 32);
                bytes[writeIndex    ] = (byte)((x >> 40) | 0xf8);
                writeIndex += 6;
            } else if(x < 0x200000000000L) // 1111 110x,
            {
                WriteSure(7);
                bytes[writeIndex + 6] = (byte)x;
                bytes[writeIndex + 5] = (byte)(x >> 8);
                bytes[writeIndex + 4] = (byte)(x >> 16);
                bytes[writeIndex + 3] = (byte)(x >> 24);
                bytes[writeIndex + 2] = (byte)(x >> 32);
                bytes[writeIndex + 1] = (byte)(x >> 40);
                bytes[writeIndex    ] = (byte)((x >> 48) | 0xfc);
                writeIndex += 7;
            } else if(x < 0x100000000000000L) // 1111 1110
            {
                WriteSure(8);
                bytes[writeIndex + 7] = (byte)x;
                bytes[writeIndex + 6] = (byte)(x >> 8);
                bytes[writeIndex + 5] = (byte)(x >> 16);
                bytes[writeIndex + 4] = (byte)(x >> 24);
                bytes[writeIndex + 3] = (byte)(x >> 32);
                bytes[writeIndex + 2] = (byte)(x >> 40);
                bytes[writeIndex + 1] = (byte)(x >> 48);
                bytes[writeIndex    ] = 0xfe;
                writeIndex += 8;
            } else // 1111 1111
            {
                WriteSure(9);
                bytes[writeIndex] = 0xff;
                bytes[writeIndex + 8] = (byte)x;
                bytes[writeIndex + 7] = (byte)(x >> 8);
                bytes[writeIndex + 6] = (byte)(x >> 16);
                bytes[writeIndex + 5] = (byte)(x >> 24);
                bytes[writeIndex + 4] = (byte)(x >> 32);
                bytes[writeIndex + 3] = (byte)(x >> 40);
                bytes[writeIndex + 2] = (byte)(x >> 48);
                bytes[writeIndex + 1] = (byte)(x >> 56);
                writeIndex += 9;
            }
        }

        public ulong UnmarshalUlong()
        {
            ReadSure(1);
            uint h = bytes[readIndex];
            if (h < 0x80)
            {
                readIndex++;
                return h;
            }
            else if (h < 0xc0)
            {
                ReadSure(2);
                uint x = ((h & 0x3f) << 8) | bytes[readIndex + 1];
                readIndex += 2;
                return x;
            }
            else if (h < 0xe0)
            {
                ReadSure(3);
                uint x = ((h & 0x1f) << 16) | ((uint)bytes[readIndex + 1] << 8) | bytes[readIndex + 2];
                readIndex += 3;
                return x;
            }
            else if (h < 0xf0)
            {
                ReadSure(4);
                uint x = ((h & 0x0f) << 24) | ((uint)bytes[readIndex + 1] << 16) | ((uint)bytes[readIndex + 2] << 8) | bytes[readIndex + 3];
                readIndex += 4;
                return x;
            }
            else if(h < 0xf8)
            {
                ReadSure(5);
                uint xl = ((uint)bytes[readIndex + 1] << 24) | ((uint)(bytes[readIndex + 2] << 16)) | ((uint)bytes[readIndex + 3] << 8) | (bytes[readIndex + 4]);
                uint xh = h & 0x07;
                readIndex += 5;
                return ((ulong)xh << 32) | xl;
            }
            else if (h < 0xfc)
            {
                ReadSure(6);
                uint xl = ((uint)bytes[readIndex + 2] << 24) | ((uint)(bytes[readIndex + 3] << 16)) | ((uint)bytes[readIndex + 4] << 8) | (bytes[readIndex + 5]);
                uint xh = ((h & 0x03) << 8) | bytes[readIndex + 1];
                readIndex += 6;
                return ((ulong)xh << 32) | xl;
            }
            else if (h < 0xfe)
            {
                ReadSure(7);
                uint xl = ((uint)bytes[readIndex + 3] << 24) | ((uint)(bytes[readIndex + 4] << 16)) | ((uint)bytes[readIndex + 5] << 8) | (bytes[readIndex + 6]);
                uint xh = ((h & 0x01) << 16) | ((uint)bytes[readIndex + 1] << 8) | bytes[readIndex + 2];
                readIndex += 7;
                return ((ulong)xh << 32) | xl;
            }
            else if (h < 0xff)
            {
                ReadSure(8);
                uint xl = ((uint)bytes[readIndex + 4] << 24) | ((uint)(bytes[readIndex + 5] << 16)) | ((uint)bytes[readIndex + 6] << 8) | (bytes[readIndex + 7]);
                uint xh = /*((h & 0x01) << 24) |*/ ((uint)bytes[readIndex + 1] << 16) | ((uint)bytes[readIndex + 2] << 8) | bytes[readIndex + 3];
                readIndex += 8;
                return ((ulong)xh << 32) | xl;
            }
            else 
            {
                ReadSure(9);
                uint xl = ((uint)bytes[readIndex + 5] << 24) | ((uint)(bytes[readIndex + 6] << 16)) | ((uint)bytes[readIndex + 7] << 8) | (bytes[readIndex + 8]);
                uint xh = ((uint)bytes[readIndex + 1] << 24) | ((uint)bytes[readIndex + 2] << 16) | ((uint)bytes[readIndex + 3] << 8) | bytes[readIndex + 4];
                readIndex += 9;
                return ((ulong)xh << 32) | xl;
            }
        }

        private static readonly bool isLittleEndian = BitConverter.IsLittleEndian;

        //const bool isLittleEndian = true;
        public unsafe void MarshalFloat(float x)
        {
            WriteSure(4);

            fixed(byte* b = bytes)
            {
                *(float*)(b + writeIndex) = x;
            }
            if(!isLittleEndian)
            {
                Array.Reverse(bytes, writeIndex, 4);
            }
            writeIndex += 4;
        }

        public float UnmarshalFloat()
        {
            ReadSure(4);
            if(!isLittleEndian)
            {
                Array.Reverse(bytes, readIndex, 4);
            }
            float x = BitConverter.ToSingle(bytes, readIndex);
            readIndex += 4;
            return x;
        }

        public unsafe void MarshalDouble(double x)
        {
            WriteSure(8);
            fixed(byte* b = bytes)
            {
                *(double*)(b + writeIndex) = x;
            }
            if (!isLittleEndian)
            {
                Array.Reverse(bytes, writeIndex, 8);
            }
            writeIndex += 8;
        }

        public unsafe double UnmarshalDouble()
        {
            ReadSure(8);
            if (!isLittleEndian)
            {
                Array.Reverse(bytes, readIndex, 8);
            }
            double x;
            fixed (byte* b = bytes)
            {
                x = *(double*)(b + readIndex);
            }
            //UnityEngine.Debug.LogFormat("unmarshal double. u:{0}", u);
            readIndex += 8;
            return x;
        }

        public void MarshalSize(int n)
        {
            uint x = (uint)n;
            // 0 111 1111
            if (x < 0x80)
            {
                WriteSure(1);
                bytes[writeIndex++] = (byte)x;
            }
            else if (x < 0x4000) // 10 11 1111, -
            {
                WriteSure(2);
                bytes[writeIndex + 1] = (byte)x;
                bytes[writeIndex] = (byte)((x >> 8) | 0x80);
                writeIndex += 2;
            }
            else if (x < 0x200000) // 110 1 1111, -,-
            {
                WriteSure(3);
                bytes[writeIndex + 2] = (byte)x;
                bytes[writeIndex + 1] = (byte)(x >> 8);
                bytes[writeIndex] = (byte)((x >> 16) | 0xc0);
                writeIndex += 3;
            }
            else if (x < 0x10000000) // 1110 1111,-,-,-
            {
                WriteSure(4);
                bytes[writeIndex + 3] = (byte)x;
                bytes[writeIndex + 2] = (byte)(x >> 8);
                bytes[writeIndex + 1] = (byte)(x >> 16);
                bytes[writeIndex] = (byte)((x >> 24) | 0xe0);
                writeIndex += 4;
            }
            else
            {
                throw UNMARSHAL_SIZE_EXCEPTION;
            }
        }

        public int UnmarshalSize()
        {
            ReadSure(1);
            int h = bytes[readIndex];
            if (h < 0x80)
            {
                readIndex++;
                return h;
            }
            else if (h < 0xc0)
            {
                ReadSure(2);
                int x = ((h & 0x3f) << 8) | bytes[readIndex + 1];
                readIndex += 2;
                return x;
            }
            else if (h < 0xe0)
            {
                ReadSure(3);
                int x = ((h & 0x1f) << 16) | (bytes[readIndex + 1] << 8) | bytes[readIndex + 2];
                readIndex += 3;
                return x;
            }
            else if (h < 0xf0)
            {

                ReadSure(4);
                int x = ((h & 0x0f) << 24) | (bytes[readIndex + 1] << 16) | (bytes[readIndex + 2] << 8) | bytes[readIndex + 3];
                readIndex += 4;
                return x;
            }
            else
            {
                throw UNMARSHAL_SIZE_EXCEPTION;
            }
        }

        public void MarshalString(string x)
        {
            var n = Encoding.UTF8.GetByteCount(x);
            MarshalSize(n);
            WriteSure(n);
            Encoding.UTF8.GetBytes(x, 0, x.Length, bytes, writeIndex);
            writeIndex += n;
        }

        public string UnmarshalString()
        {
            var n = UnmarshalSize();
            ReadSure(n);
            var s = Encoding.UTF8.GetString(bytes, readIndex, n);
            readIndex += n;
            return s;
        }

        [LuaInterface.LuaByteBuffer]
        public void MarshalBytes(byte[] x)
        {
            var n = x.Length;
            MarshalSize(n);
            WriteSure(n);
            x.CopyTo(bytes, writeIndex);
            writeIndex += n;
        }

        [LuaInterface.LuaByteBuffer]
        public byte[] UnmarshalBytes()
        {
            var n = UnmarshalSize();
            ReadSure(n);
            var x = new byte[n];
            Buffer.BlockCopy(bytes, readIndex, x, 0, n);
            readIndex += n;
            return x;
        }

        public class Msg
        {
            public int maxSize;
            public BinaryStream body = new BinaryStream();
        }

        public enum UnmarshalError
        {
            OK,
            NOT_ENOUGH,
            EXCEED_SIZE,
            UNMARSHAL_ERR,
        }

        public UnmarshalError TryUnmarshalBytes(Msg msg)
        {
            var oldReadIndex = readIndex;
            int n;
            try
            {
                n = UnmarshalSize();
            } catch (Exception e)
            {
                if(e == UNMARSHAL_NOT_ENOUGH_EXCEPTION)
                {
                    readIndex = oldReadIndex;
                    return UnmarshalError.NOT_ENOUGH;
                } else
                {
                    throw e;
                }
            }
            //UnityEngine.Debug.Log("unmarshal size:" + n);
            if (n > msg.maxSize) throw UNMARSHAL_SIZE_EXCEPTION;
            if (Remaining < n)
            {
                readIndex = oldReadIndex;
                return UnmarshalError.NOT_ENOUGH;
            }

            // 每提取一个消息后直接送达反序列化
            // 故可以复用BinaryStream
            // 确保使用的时候是立即反序列化
            var body = msg.body;
            body.bytes = bytes;
            body.readIndex = readIndex;
            readIndex += n;
            body.writeIndex = readIndex;

            return UnmarshalError.OK;
        }

        public void MarshalBinaryStream(BinaryStream o)
        {
            var n = o.Remaining;
            MarshalSize(n);
            WriteBytes(o.bytes, o.readIndex, n);
        }

        public override string ToString()
        {
            string[] datas = new string[writeIndex - ReadIndex];
            for (var i = readIndex; i < writeIndex; i++)
            {
                datas[i - readIndex] = bytes[i].ToString();
            }
            return string.Join(".", datas);
        }
    }
}
