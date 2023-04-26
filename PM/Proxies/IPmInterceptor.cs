using Castle.DynamicProxy;
using PM.Core;

namespace PM.Proxies
{
    public interface IPmInterceptor : IInterceptor
    {
        /// <summary>
        /// If a transaction is running, this redirector is called
        /// </summary>
        AsyncLocal<IInterceptorRedirect> TransactionInterceptorRedirect { get; }
        FileBasedStream PmMemoryMappedFile { get; }
        IInterceptorRedirect OriginalFileInterceptorRedirect { get; }

        public string FilePointer { get; }
        public ulong? PmPointer { get; }
    }
}
