using PM.Core;
using PM.Proxies;
using PM.Factories;
using System.Reflection;
using PM.Configs;
using PM.Managers;
using PM.CastleHelpers;

namespace PM.Transactions
{
    public class TransactionManager<T> : IInterceptorRedirect
         where T : class, new()
    {
        public ObjectPropertiesInfoMapper? ObjectMapper { get; private set; }
        public TransactionState State { get; private set; } = TransactionState.NotStarted;

        private readonly T _obj;
        public LogFile LogFile { get; set; }
        public const string RootExtension = ".root";

        private readonly IPmInterceptor _interceptor;
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
                var logFilePm = FileHandlerManager.CreateHandler(file);
                var logFile = new LogFile(new PmCSharpDefinedTypes(logFilePm.FileBasedStream));
                if (logFile.IsCommitedLogFile)
                {
                    var originalFilename = logFile.ReadOriginalFileName();
                    var pm = FileHandlerManager.CreateHandler(originalFilename);
                    foreach (var item in logFile.LogFileContent)
                    {
                        pm.FileBasedStream.Seek(item.Item1, SeekOrigin.Begin);
                        pm.FileBasedStream.Write(item.Item3);
                    }
                }
                logFile.DeleteFile();
            }
        }

        public TransactionManager(T obj)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));

            _obj = obj;
            _transactionID = Guid.NewGuid().ToString() + RootExtension;

            var filename = Path.Combine(PM.Configs.PmGlobalConfiguration.PmTransactionFolder, _transactionID);
            var pm = FileHandlerManager.CreateHandler(filename);
            LogFile = new LogFile(new PmCSharpDefinedTypes(pm.FileBasedStream));


            if (CastleManager.TryGetInterceptor(_obj, out var interceptor))
            {
                _interceptor = interceptor;
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
            var pm = FileHandlerManager.CreateHandler(_interceptor.PmMemoryMappedFile.FilePath);
            foreach (var item in LogFile.LogFileContent)
            {
                pm.FileBasedStream.Seek(item.Item1, SeekOrigin.Begin);
                pm.FileBasedStream.Write(item.Item3);
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
            else if (value is sbyte valueSByte) LogFile.WriteSByte(offset, valueSByte);
            else if (value is short valueShort) LogFile.WriteShort(offset, valueShort);
            else if (value is ushort valueUShort) LogFile.WriteUShort(offset, valueUShort);
            else if (value is uint valueUInt) LogFile.WriteUInt(offset, valueUInt);
            else if (value is int valueInt) LogFile.WriteInt(offset, valueInt);
            else if (value is long valueLong) LogFile.WriteLong(offset, valueLong);
            else if (value is ulong valueULong) LogFile.WriteULong(offset, valueULong);
            else if (value is float valueFloat) LogFile.WriteFloat(offset, valueFloat);
            else if (value is double valueDouble) LogFile.WriteDouble(offset, valueDouble);
            else if (value is decimal valueDecimal) LogFile.WriteDecimal(offset, valueDecimal);
            else if (value is char valueChar) LogFile.WriteChar(offset, valueChar);
            else if (value is bool valueBool) LogFile.WriteBool(offset, valueBool);
            else if (value is string valueStr) LogFile.WriteString(offset, valueStr);
            else
            {
                throw new ArgumentException($"value of type {value.GetType()} invalid in transaction");
            }

            _propertiesValues[property] = value;
        }
    }
}