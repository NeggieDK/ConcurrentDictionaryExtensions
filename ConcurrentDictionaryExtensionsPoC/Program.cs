using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace ConcurrentDictionaryExtensionsPoC
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchy>();
        }
    }

    public class Benchy
    {
        private static ConcurrentDictionary<string, string> _dict = new ConcurrentDictionary<string, string>();

        [Benchmark]
        public void Spinlock()
        {
            _dict.Clear();
            for (var i = 0; i < 1000; i++)
            {
                _dict.GetOrAddStampedeProtectionSpinLock(i.ToString(), (key) => key);
            }
        }
        
        [Benchmark]
        public void Lock()
        {
            _dict.Clear();
            for (var i = 0; i < 1000; i++)
            {
                _dict.GetOrAddStampedeProtectionLock(i.ToString(), (key) => key);
            }
        }
    }
}