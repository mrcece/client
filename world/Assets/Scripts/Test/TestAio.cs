using Aio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Assets.Scripts.Aio
{
    class TestAio
    {
        public static void Assert(bool con, string fmt, params object[] args)
        {
            if(!con)
            {
                UnityEngine.Debug.LogErrorFormat(fmt, args);
            }
        }

        public static void Assert(bool con, string fmt)
        {
            if (!con)
            {
                UnityEngine.Debug.LogError(fmt);
            }
        }

        public static void Test()
        {
            {
                var bs = new BinaryStream();
                var datas = new List<bool> {true,false,false,true };
                foreach (var d in datas)
                {
                    bs.MarshalBool(d);
                    var c = bs.UnmarshalBool();
                    Assert(bs.Remaining == 0 && d == c, "bool {0} : {1}, remain:{2}", d, c, bs.Remaining);
                }
            }

            {
                var bs = new BinaryStream();
                var datas = new List<byte> {0,3,60,255};
                foreach (var d in datas)
                {
                    bs.MarshalByte(d);
                    var c = bs.UnmarshalByte();
                    Assert(bs.Remaining == 0 && d == c, "byte {0} : {1}, remain:{2}", d, c, bs.Remaining);
                }
            }

            {
                var bs = new BinaryStream();
                for(long i = 0, n = 0; n < 200; i = (i + 1) * 3/2, n++)
                {
                    var d = (int)i;
                    //UnityEngine.Debug.LogFormat("n:{0} d:{1}", n, d);
                    bs.MarshalInt(d);
                    var c = bs.UnmarshalInt();
                    Assert(bs.Remaining == 0 && d == c, "int:{3} {0} : {1}, remain:{2}", d, c, bs.Remaining, n);
                }
            }

            {
                var bs = new BinaryStream();
                for (long i = 0, n = 0; n < 200; i = (i + 1) * 3 / 2, n++)
                {
                    var d = i;
                    bs.MarshalLong(d);
                    //UnityEngine.Debug.LogFormat("n:{0} d:{1} os:{2}", n, d, bs);
                    var c = bs.UnmarshalLong();
                    Assert(bs.Remaining == 0 && d == c, "long:{3} {0} : {1}, remain:{2}", d, c, bs.Remaining, n);
                }
            }
            
            {
                var bs = new BinaryStream();
                var f = 1f;
                for (int n = 0; n < 200; f = f * 1.5f, n++)
                {
                    var d = f;
                    bs.MarshalFloat(d);
                    //UnityEngine.Debug.LogFormat("n:{0} d:{1} os:{2}", n, d, bs);
                    var c = bs.UnmarshalFloat();
                    Assert(bs.Remaining == 0 && d == c, "float:{3} {0} : {1}, remain:{2}", d, c, bs.Remaining, n);
                }
            }

            {
                var bs = new BinaryStream();
                var f = 1.0;
                for (int n = 0; n < 200; f = f * 1.5, n++)
                {
                    var d = f;
                    bs.MarshalDouble(d);
                    //UnityEngine.Debug.LogFormat("n:{0} d:{1} os:{2}", n, d, bs);
                    var c = bs.UnmarshalDouble();
                    Assert(bs.Remaining == 0 && d == c, "double:{3} {0} : {1}, remain:{2}", d, c, bs.Remaining, n);
                }
            }

            /*
            {
                var bs = new BinaryStream();
                var datas = new List<string> { "江山如此多妖，引无数英雄", "abasdfasdfw" };
                foreach (var d in datas)
                {
                    bs.MarshalString(d);
                    var c = bs.UnmarshalString();
                    Assert(bs.Remaining == 0 && d == c, "string {0} : {1}, remain:{2}", d, c, bs.Remaining);
                }
            }

            {
                var bs = new BinaryStream();
                var datas = new List<Binary> {new Binary(Encoding.UTF8.GetBytes("江山如此多妖，引无数英雄")), new Binary(Encoding.UTF8.GetBytes("abasdfasdfw")) };
                foreach (var d in datas)
                {
                    bs.MarshalBinary(d);
                    var c = bs.UnmarshalBinary();
                    Assert(bs.Remaining == 0 && d == c, "binary {0} : {1}, remain:{2}", d, c, bs.Remaining);
                }
            }
            */
        }

        public static void Connect()
        {
            var sw = new List<string> { "aa", "bb", "晓N" };
            //var pattern = string.Join("|", sw.Select(w => "(" + Regex.Escape(w) + ")").ToArray());
            var pattern = "(" + string.Join("|", sw.Select(w => Regex.Escape(w)).ToArray()) + ")+";
            var re = new Regex(pattern, RegexOptions.IgnoreCase);
            var olds = "==aabb冯晓N,==";
            var news = re.Replace(olds, "妹子");
            UnityEngine.Debug.Log("=== " + news);
        }

        public static void Connect2()
        {
            var pattern = "(ab)+";
            var re = new Regex(pattern, RegexOptions.IgnoreCase);
            var olds = "==abab,cabab,==";
            var news = re.Replace(olds, "ab");
            UnityEngine.Debug.Log("=== " + news);
        }
    }
}
