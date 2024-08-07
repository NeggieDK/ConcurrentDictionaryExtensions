using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ConcurrentDictionaryExtensionsPoC
{
    public class SafeConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>
    {
        readonly ConcurrentDictionary<TKey, SemaphoreSlim> _semaphorePerKey =
            new ConcurrentDictionary<TKey, SemaphoreSlim>();

        SpinLock _spinLock = new SpinLock();
        
        private SemaphoreSlim GetSemaphoreSpinLock(TKey key)
        {
            var taken = false;
            _spinLock.Enter(ref taken);
            var semaphore = _semaphorePerKey.GetOrAdd(key, new SemaphoreSlim(1));
            _spinLock.Exit();
            return semaphore;
        }
        
        public TValue GetOrAddStampedeProtection(
            TKey key,
            Func<TKey, TValue> valueFactory)
        {
            if (TryGetValue(key, out var result))
            {
                return result;
            }

            var semaphore = GetSemaphoreSpinLock(key);
            semaphore.Wait();
            if (TryGetValue(key, out var value))
            {
                _semaphorePerKey.TryRemove(key, out _);
                semaphore.Release();
                return value;
            }
            
            value = valueFactory.Invoke(key);
            TryAdd(key, value);
            _semaphorePerKey.TryRemove(key, out _);
            semaphore.Release();

            return value;
        }
    }
}