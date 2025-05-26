using System.Collections.Concurrent;

namespace Agenix.ReqnrollPlugin;

/// <summary>
///     A helper class that provides thread-safe methods for managing lock objects.
/// </summary>
internal static class LockHelper
{
    private static readonly ConcurrentDictionary<int, object> Repository = new();

    private static readonly object GetLockLock = new();

    public static object GetLock(int hashCode)
    {
        lock (GetLockLock)
        {
            if (Repository.ContainsKey(hashCode)) return Repository[hashCode];

            var lockObj = new object();
            Repository.AddOrUpdate(hashCode, lockObj, (key, oldValue) => oldValue);

            return lockObj;
        }
    }
}