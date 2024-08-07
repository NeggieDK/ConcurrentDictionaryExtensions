using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ConcurrentDictionaryExtensionsPoC
{
    public static class ConcurrentDictionaryExtensions
    {
        public static ConcurrentDictionary<object, SemaphoreSlim> _semaphorePerKey =
            new ConcurrentDictionary<object, SemaphoreSlim>();

        private static SpinLock _spinLock = new SpinLock();
        private static Object _lock = new Object();

        private static SemaphoreSlim GetSemaphoreSpinLock(object key)
        {
            var taken = false;
            _spinLock.Enter(ref taken);
            var semaphore = _semaphorePerKey.GetOrAdd(key, new SemaphoreSlim(1));
            _spinLock.Exit();
            return semaphore;
        }

        private static SemaphoreSlim GetSemaphoreLock(object key)
        {
            lock (_lock)
            {
                var semaphore = _semaphorePerKey.GetOrAdd(key, new SemaphoreSlim(1));
                return semaphore;    
            }
        }
        
        public static TValue GetOrAddStampedeProtectionSpinLock<TKey, TValue>(
            this ConcurrentDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TKey, TValue> valueFactory)
        {
            if (dictionary.TryGetValue(key, out var result))
            {
                return result;
            }
                

            var semaphore = GetSemaphoreSpinLock(key);
            semaphore.Wait();
            if (dictionary.TryGetValue(key, out var value))
            {
                _semaphorePerKey.TryRemove(key, out _);
                semaphore.Release();
                return value;
            }
            
            value = valueFactory.Invoke(key);
            dictionary.TryAdd(key, value);
            _semaphorePerKey.TryRemove(key, out _);
            semaphore.Release();

            return value;
        }
        
        public static TValue GetOrAddStampedeProtectionLock<TKey, TValue>(
            this ConcurrentDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TKey, TValue> valueFactory)
        {
            if (dictionary.TryGetValue(key, out var result))
            {
                return result;
            }
                

            var semaphore = GetSemaphoreLock(key);
            semaphore.Wait();
            if (dictionary.TryGetValue(key, out var value))
            {
                _semaphorePerKey.TryRemove(key, out _);
                semaphore.Release();
                return value;
            }
            
            value = valueFactory.Invoke(key);
            dictionary.TryAdd(key, value);
            _semaphorePerKey.TryRemove(key, out _);
            semaphore.Release();

            return value;
        }
    }
}