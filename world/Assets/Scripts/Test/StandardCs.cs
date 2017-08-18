using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class TestBase
{
    public class BenchRecord
    {
        public string name;
        public long maxTime;
        public long minTime;
        public List<long> times = new List<long>();
    }

    private static Dictionary<string, BenchRecord> baseRecord;
    private Dictionary<string, BenchRecord> records = new Dictionary<string, BenchRecord>();

    public void run()
    {
        for(int i = 0; i < 2; i++)
        {
            test1();
        }
        for (int i = 0; i < 10; i++)
        {
            Benchmark("test1", test1);
            //Benchmark("test2", test2);
            //Benchmark("test3", test3);
            //Benchmark("test4", test4);
        }

        if (baseRecord == null)
        {
            baseRecord = records;
        }
        foreach (var e in records)
        {
            var name = e.Key;
            var record = e.Value;
            var aver = record.times.Average();
            var br = baseRecord[name];
            UnityEngine.Debug.LogFormat("{4} - {0} mintime:{1} maxtime:{2} avertime:{3}  self/base{{min:{6:#.##}, max:{7:#.##}, aver:{5:#.##}}}",
                name, record.minTime, record.maxTime, aver, GetType().Name, (float)aver / br.times.Average(), (float)record.minTime / br.minTime, (float)record.maxTime / br.maxTime);
        }
    }

    public void Benchmark(string name, Action fun)
    {
        GC.Collect();
        Stopwatch sw = new Stopwatch();
        sw.Start();
        fun();
        sw.Stop();
        var time = sw.ElapsedMilliseconds;
        if (!records.ContainsKey(name))
        {
            records.Add(name, new BenchRecord() { name = name, maxTime = int.MinValue, minTime = int.MaxValue });
        }
        var re = records[name];
        re.maxTime = Math.Max(re.maxTime, time);
        re.minTime = Math.Min(re.minTime, time);
        re.times.Add(time);
    }

    public int x;
    public float y;

    public const int N1 = 1000000;
    public const int N2 = 10000;
    public const int N3 = 100000;
    public abstract void test1();
    public abstract void test2();
    public abstract void test3();
    public abstract void test4();
}


public class StarndCs : TestBase
{
    public int _a;
    public int _b;
    public int _c;
    public override void test1()
    {
        int sum = 0;
        int a = 1;
        int b = 2;
        int c = 3;
        for(int i = 0; i < N1; i++)
        {
            sum += i;

            b = a + c;
            c = a + b;
            a = c + b;
            b = a + c;
            c = a + b;
            a = c + b;
            b = a + c;
            c = a + b;
            a = c + b;
            b = a + c;
        }
        x = sum;
        _a = a;
        _b = b;
    }

    public override void test2()
    {
        for(int i = 0; i < N2; i++)
        {
            new GameObject();
        }
    }

    public override void test3()
    {
        Transform t = new GameObject().transform;
        Vector3 v = new Vector3(1, 2, 3);
        for(int i = 0; i < N3; i++)
        {
            t.position = v;
            t.position = v;
            t.position = v;
            t.position = v;
            t.position = v;
            t.position = v;
            t.position = v;
            t.position = v;
            t.position = v;
            t.position = v;
        }
    }

    public override void test4() 
    {
        float time = 0;
        for(int i = 0; i < 100000; i++)
        {
            time = Time.time;
            time = Time.time;
            time = Time.time;
            time = Time.time;
            time = Time.time;
            time = Time.time;
            time = Time.time;
            time = Time.time;
        }
        y = time;
    }
}

