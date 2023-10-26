using PM.Core;
using PM.Proxies;
using PM.Factories;
using System.Reflection;
using PM.Configs;
using PM.Managers;
using PM.CastleHelpers;
using Castle.DynamicProxy;
using System;

namespace PM.Transactions
{
    public class TransactionManager<T> : IInterceptorRedirect
         where T : class, new()
    {
        public LogFile LogFile => _genericTransactionManager.LogFile;

        private readonly TransactionManager _genericTransactionManager;
        public TransactionState State => _genericTransactionManager.State;

        public TransactionManager(T obj, bool isRootObject)
        {
            _genericTransactionManager = new TransactionManager(obj, isRootObject);
        }

        public ObjectPropertiesInfoMapper ObjectMapper => _genericTransactionManager.ObjectMapper!;

        public static void ApplyPendingTransactions() => TransactionManager.ApplyPendingTransactions();

        public void Begin()
        {
            _genericTransactionManager.Begin();
        }

        public void Commit()
        {
            _genericTransactionManager.Commit();
        }

        public void RollBack()
        {
            _genericTransactionManager.RollBack();
        }

        public object? GetValuePm(PropertyInfo property)
        {
            return _genericTransactionManager.GetValuePm(property);
        }

        public void InsertValuePm(PropertyInfo property, object value)
        {
            _genericTransactionManager.InsertValuePm(property, value);
        }
    }

    public class TransactionManager : IInterceptorRedirect
    {
        public ObjectPropertiesInfoMapper? ObjectMapper { get; private set; }
        public TransactionState State { get; private set; } = TransactionState.NotStarted;

        private readonly object _obj;
        private readonly Type _type;
        private readonly bool _isRootObject;

        private readonly List<TransactionManager> _allInternalObjectsTransactionManagers = new();

        public LogFile LogFile { get; set; }
        public const string RootExtension = ".root";

        private readonly IPmInterceptor _interceptor;
        private readonly string _transactionID;
        private readonly Dictionary<PropertyInfo, object> _propertiesValues = new();
        private readonly PointersToPersistentObjects _pointersToPersistentObjects = new();

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

        public TransactionManager(object obj, bool isRootObject)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));

            _obj = obj;
            _type = _obj.GetType();
            _isRootObject = isRootObject;
            _transactionID = Guid.NewGuid().ToString() + (isRootObject ? RootExtension : string.Empty);

            var filename = Path.Combine(PmGlobalConfiguration.PmTransactionFolder, _transactionID);
            var pm = FileHandlerManager.CreateHandler(filename);
            LogFile = new LogFile(new PmCSharpDefinedTypes(pm.FileBasedStream));


            if (CastleManager.TryGetInterceptor(_obj, out var interceptor))
            {
                _interceptor = interceptor;
            }
            else
            {
                throw new PmTransactionException($"Object {obj.GetType().Name} is not a PersistentObject");
            }
        }

        public void Begin()
        {
            State = TransactionState.Running;
            ObjectMapper = _interceptor.OriginalFileInterceptorRedirect.ObjectMapper;
            _interceptor.TransactionInterceptorRedirect.Value = this;
            LogFile.WriteOriginalFileName(_interceptor.PmMemoryMappedFile.FilePath);
            // After this point, the sets will call this object on method 'InsertValuePm'
            // instead the original InterceptorRedirect
            
            if (_isRootObject) RedirectAllInternalsObjects(_obj);
        }

        private void RedirectAllInternalsObjects(object obj)
        {
            foreach (var prop in obj.GetType().GetProperties())
            {
                var propType = prop.PropertyType;
                if (!propType.IsPrimitive &&
                    propType != typeof(decimal) &&
                    propType != typeof(string))
                {
                    var innerObj = prop.GetValue(obj);
                    if (innerObj != null && !(innerObj is ICustomPmClass))
                    {
                        RedirectAllInternalsObjects(innerObj);
                        var transactionManager = new TransactionManager(innerObj, false);
                        _allInternalObjectsTransactionManagers.Add(transactionManager);
                        transactionManager.Begin();
                    }
                }
            }
        }

        public void Commit()
        {
            _interceptor.TransactionInterceptorRedirect.Value = null;
            LogFile.Commit();
            CopyLogToOriginal();
            LogFile.DeleteFile();
            State = TransactionState.Commited;

            if (_isRootObject) CommitAllInternalsObjects(_obj);
        }

        private void CommitAllInternalsObjects(object obj)
        {
            foreach(var transactionManager in _allInternalObjectsTransactionManagers)
            {
                transactionManager.Commit();
            }
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

            if (_isRootObject) CopyLogToOriginalAllInternalsObjects(_obj);
        }

        private void CopyLogToOriginalAllInternalsObjects(object obj)
        {
            foreach (var transactionManager in _allInternalObjectsTransactionManagers)
            {
                transactionManager.CopyLogToOriginal();
            }
        }

        public void RollBack()
        {
            _interceptor.TransactionInterceptorRedirect.Value = null;
            LogFile.RollBack();

            if (_isRootObject) RollBackAllInternalsObjects(_obj);

            State = TransactionState.RollBacked;
        }

        private void RollBackAllInternalsObjects(object obj)
        {
            foreach (var transactionManager in _allInternalObjectsTransactionManagers)
            {
                transactionManager.RollBack();
            }
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
                if (value is ICustomPmClass customObj)
                {
                    ulong pointer = customObj.PmPointer;
                    LogFile.WriteULong(offset, pointer);
                }
                else if (value is null)
                {
                    ulong nullPtr = 0;
                    LogFile.WriteULong(offset, nullPtr);
                }
                else
                {
                    ulong pointer = GetPointerIfExistsOrNew(property);
                    // User defined objects
                    IPersistentFactory persistentFactory = new PersistentFactory();
                    var proxy = persistentFactory.CreateInternalObjectByObject(
                        value,
                        pointer);
                    LogFile.WriteULong(offset, pointer);
                    _propertiesValues[property] = proxy;
                    return;
                }
            }

            _propertiesValues[property] = value;
        }


        private ulong GetPointerIfExistsOrNew(PropertyInfo property)
        {
            var pmManager = (PmManager)_interceptor.OriginalFileInterceptorRedirect;
            var pointer = pmManager._pm.GetULongPropertValue(property);
            var pointerAlreadyExists = pointer != 0;
            if (!pointerAlreadyExists)
            {
                pointer = _pointersToPersistentObjects.GetNext();
            }

            return pointer;
        }
    }
}