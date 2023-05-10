using Castle.DynamicProxy;
using PM.Core;
using PM.Managers;

namespace PM.Proxies
{
    public interface IPmInterceptor : IInterceptor
    {
        /// <summary>
        /// If a transaction is running, this redirector is called
        /// </summary>
        AsyncLocal<IInterceptorRedirect> TransactionInterceptorRedirect { get; }
        FileBasedStream PmMemoryMappedFile { get; }
        FileHandlerItem FileHandlerItem { get; }
        IInterceptorRedirect OriginalFileInterceptorRedirect { get; }

        ulong FilePointerCount { get; set; }
        string FilePointer { get; }
        ulong? PmPointer { get; }
    }
}
