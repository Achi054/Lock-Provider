namespace LockProvider
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    /// <summary>
    /// Represents the concurrency handler with lock mechanism.
    /// </summary>
    /// <typeparam name="T">Type of Key.</typeparam>
    public class LockProvider<T> : IDisposable
    {
        /* For concurrent requests, mainly on Off-Site Location or Visit Documents from Mobile. We currently use lock which is not thread safe.
           Instead we use Semaphores slim.
           We want to avoid race conditions for parallel calls.
           With this approach we can make sure that only one operation is performed against a specific key-value. */

        private static ConcurrentDictionary<T, SemaphoreSlim> lockers = new ConcurrentDictionary<T, SemaphoreSlim>();

        private readonly SemaphoreSlim currentLock;
        private readonly T key;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="LockProvider{T}"/> class.
        /// </summary>
        /// <param name="key">Key of the locker.</param>
        public LockProvider(T key)
        {
            this.key = key;
            this.currentLock = TryGetLock(key);

            // Wait until the current lock to be release.
            this.currentLock.Wait();
        }

        /// <summary>
        /// Gets the lock key.
        /// </summary>
        public T Key => this.key;

        /// <summary>
        /// Gets the current lock object.
        /// </summary>
        public SemaphoreSlim CurrentLock => this.currentLock;

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose unmanaged resources.
        /// </summary>
        /// <param name="disposing">Flag to enable garbage collection.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    // Release the current lock (semaphore slim)
                    this.currentLock.Release();
                }

                this.isDisposed = true;
            }
        }

        /// <summary>
        /// Get <see cref="SemaphoreSlim"/> instance for the <paramref name="key"/>.
        /// Creates new lock if not already exists.
        /// </summary>
        /// <param name="key">Key to get the lock.</param>
        /// <returns>Locker of the <paramref name="key"/>.</returns>
        private static SemaphoreSlim TryGetLock(T key)
        {
            if (!lockers.ContainsKey(key))
            {
                // If we don't already have a semaphore for this key, create it. It should only have one slot.
                lockers.TryAdd(key, new SemaphoreSlim(1, 1));
            }

            return lockers[key];
        }
    }
}