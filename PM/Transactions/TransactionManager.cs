using PM.Core;
using PM.Proxies;
using PM.Factories;
using Castle.DynamicProxy;
using System.Reflection;
using PM.Configs;
using PM.Managers;

namespace PM.Transactions
{
    public class TransactionManager<T> : IInterceptorRedirect
         where T : class, new()
    {
        public ObjectPropertiesInfoMapper? ObjectMapper { get; private set; }
        public TransactionState State { get; private set; } = TransactionState.NotStarted;

        private readonly T _obj;
        public LogFile LogFile { get; set; }
        private readonly PersistentInterceptor _interceptor;
        private readonly string _transactionID;
        private readonly Dictionary<PropertyInfo, object> _propertiesValues = new();

        static TransactionManager()
        {
            ApplyPendingTransactions();
        }

        public static void ApplyPendingTransactions()
        {
            var transactionFolder = TransactionFolderFactory.Create();
            var files = transactionFolder.GetLogFileNames();
            foreach (var file in files)
            {
                var logFilePm = PmFactory.CreatePm(new PmMemoryMappedFileConfig(file));
                var logFile = new LogFile(new PmCSharpDefinedTypes(logFilePm));
                if (logFile.IsCommitedLogFile)
                {
                    var originalFilename = logFile.ReadOriginalFileName();
                    var pm = PmFactory.CreatePm(new PmMemoryMappedFileConfig(originalFilename));
                    foreach (var item in logFile.LogFileContent)
                    {
                        pm.Store(item.Item3, offset: item.Item1);
                    }
                }
                logFile.DeleteFile();
            }
        }

        public TransactionManager(T obj)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));

            _obj = obj;
            _transactionID = Guid.NewGuid().ToString();

            var filename = Path.Combine(PmGlobalConfiguration.PmTransactionFolder, _transactionID);
            var pm = PmFactory.CreatePm(new PmMemoryMappedFileConfig(filename));
            LogFile = new LogFile(new PmCSharpDefinedTypes(pm));


            if (_obj is IProxyTargetAccessor proxyObj)
            {
                _interceptor =
                    (PersistentInterceptor)proxyObj
                        .GetInterceptors()
                        .Single(x => x is PersistentInterceptor);
            }
            else
            {
                throw new PmTransactionException($"Object {typeof(T)} is not a PersistentObject");
            }
        }

        public void Begin()
        {
            State = TransactionState.Running;
            ObjectMapper = _interceptor.OriginalFileInterceptorRedirect.ObjectMapper;
            _interceptor.TransactionInterceptorRedirect.Value = this;
            LogFile.WriteOriginalFileName(_interceptor.PmMemoryMappedFile.FilePath);
            // After this point, the sets will call this object instead the original InterceptorRedirect
        }

        public void Commit()
        {
            _interceptor.TransactionInterceptorRedirect.Value = null;
            LogFile.Commit();
            CopyLogToOriginal();
            LogFile.DeleteFile();
            State = TransactionState.Commited;
        }

        public void Lock()
        {
            if (_interceptor != null)
            {
                var pmManager = (PmManager)_interceptor.OriginalFileInterceptorRedirect;
                pmManager.Lock();
            }
        }

        public void Release()
        {
            if (_interceptor != null)
            {
                var pmManager = (PmManager)_interceptor.OriginalFileInterceptorRedirect;
                pmManager.Release();
            }
        }

        private void CopyLogToOriginal()
        {
            var pm = PmFactory.CreatePm(_interceptor.PmMemoryMappedFile);
            foreach (var item in LogFile.LogFileContent)
            {
                pm.Store(item.Item3, offset: item.Item1);
            }
        }

        public void RollBack()
        {
            _interceptor.TransactionInterceptorRedirect.Value = null;
            LogFile.RollBack();
            State = TransactionState.RollBacked;
        }

        public object? GetValuePm(PropertyInfo property)
        {
            if (_propertiesValues.TryGetValue(property, out var value))
            {
                return value;
            }
            return _interceptor.OriginalFileInterceptorRedirect.GetValuePm(property);
        }

        public void InsertValuePm(PropertyInfo property, object value)
        {
            if (ObjectMapper is null) throw new ApplicationException($"{ObjectMapper} is null");

            var offset = ObjectMapper.GetOffSet(property);
            if (value is byte valueByte) LogFile.WriteByte(offset, valueByte);
            if (value is sbyte valueSByte) LogFile.WriteSByte(offset, valueSByte);
            if (value is short valueShort) LogFile.WriteShort(offset, valueShort);
            if (value is ushort valueUShort) LogFile.WriteUShort(offset, valueUShort);
            if (value is uint valueUInt) LogFile.WriteUInt(offset, valueUInt);
            if (value is int valueInt) LogFile.WriteInt(offset, valueInt);
            if (value is long valueLong) LogFile.WriteLong(offset, valueLong);
            if (value is ulong valueULong) LogFile.WriteULong(offset, valueULong);
            if (value is float valueFloat) LogFile.WriteFloat(offset, valueFloat);
            if (value is double valueDouble) LogFile.WriteDouble(offset, valueDouble);
            if (value is decimal valueDecimal) LogFile.WriteDecimal(offset, valueDecimal);
            if (value is char valueChar) LogFile.WriteChar(offset, valueChar);
            if (value is bool valueBool) LogFile.WriteBool(offset, valueBool);
            if (value is string valueStr) LogFile.WriteString(offset, valueStr);

            _propertiesValues[property] = value;
        }
    }
}