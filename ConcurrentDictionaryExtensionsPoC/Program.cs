using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace ConcurrentDictionaryExtensionsPoC
{
    internal class Program
    {
        private static SafeConcurrentDictionary<string, string> _dict2 = new SafeConcurrentDictionary<string, string>();
        private static int ActiveThreads = 0;
        
        public static void Main(string[] args)
        {
            CheckSafety();
            //BenchmarkRunner.Run<Benchy>();
        }

        public static void CheckSafety()
        {
            var t1 = new Thread(() =>
            {
                var sw = Stopwatch.StartNew();
                _dict2.GetOrAdd("a", FactoryMethod);
                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds);
            });
            
            var t2 = new Thread(() =>
            {
                var sw = Stopwatch.StartNew();
                _dict2.GetOrAdd("a", FactoryMethod);
                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds);
            });
            
            var t3 = new Thread(() =>
            {
                var sw = Stopwatch.StartNew();
                _dict2.GetOrAdd("a", FactoryMethod);
                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds);
            });
            
            var t4 = new Thread(() =>
            {
                Thread.Sleep(50);
                var sw = Stopwatch.StartNew();
                _dict2.GetOrAdd("a", FactoryMethod);
                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds);
            });
            
            
            t1.Start();
            t2.Start();
            t3.Start();
            t4.Start();

            t1.Join();
            t2.Join();
            t3.Join();
            t4.Join();
        }

        static string FactoryMethod(string key)
        {
            Console.WriteLine("FactoryMethod");
            var amount = Interlocked.Increment(ref ActiveThreads);
            if (amount > 1)
            {
                Console.WriteLine("Race Condition!");
            }
            Thread.Sleep(100);
            Interlocked.Decrement(ref ActiveThreads);
            return "b";
        }
    }

    [MemoryDiagnoser]
    public class Benchy
    {
        private static ConcurrentDictionary<string, string> _dict1 = new ConcurrentDictionary<string, string>();
        private static SafeConcurrentDictionary<string, string> _dict2 = new SafeConcurrentDictionary<string, string>();


        [Benchmark]
        public void AddNew()
        {
            for (var i = 0; i < 1000; i++)
            {
                _dict1.GetOrAdd(i.ToString(), (key) => key);
            }
        }
        
        [Benchmark]
        public void AddNew_Safe()
        {
            for (var i = 0; i < 1000; i++)
            {
                _dict2.GetOrAddStampedeProtection(i.ToString(), (key) => key);
            }
        }
        
        [Benchmark]
        public async Task AddSame()
        {
            var tasks = new List<Task>();
            for (var i = 0; i < 1000; i++)
            {
                tasks.Add(Task.Run(() => _dict1.GetOrAdd("a", (key) => key)));
            }

            await Task.WhenAll(tasks);
        }
        
        [Benchmark]
        public async Task SafeWithSpinLock()
        {
            var tasks = new List<Task>();
            for (var i = 0; i < 1000; i++)
            {
                tasks.Add(Task.Run(() => _dict2.GetOrAddStampedeProtection("a", (key) => key)));
            }

            await Task.WhenAll(tasks);
        }
    }
}