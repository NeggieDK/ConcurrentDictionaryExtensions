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

    [MemoryDiagnoser]
    public class Benchy
    {
        private static ConcurrentDictionary<string, string> _dict1 = new ConcurrentDictionary<string, string>();
        private static SafeConcurrentDictionary<string, string> _dict2 = new SafeConcurrentDictionary<string, string>();


        [Benchmark]
        public void Regular()
        {
            for (var i = 0; i < 1000; i++)
            {
                _dict1.GetOrAdd(i.ToString(), (key) => key);
            }
        }
        
        [Benchmark]
        public void SafeWithSpinLock()
        {
            for (var i = 0; i < 1000; i++)
            {
                _dict2.GetOrAddStampedeProtection(i.ToString(), (key) => key);
            }
        }
    }
}