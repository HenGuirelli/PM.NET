using Castle.DynamicProxy;
using PM.Common;
using PM.Configs;
using PM.Core;
using PM.Managers;
using PM.PmContent;
using PM.Proxies;
using PM.Startup;
using Serilog;

namespace PM
{
    public interface IPersistentFactory
    {
        private static readonly PointersToPersistentObjects _pointersToPersistentObjects = new();
        private static readonly PmProxyGenerator _generator = new(proxyCacheCount: PmGlobalConfiguration.ProxyCacheCount);
        private static readonly IPmFolderCleaner _pmPointerCounter = new PmFolderCleaner();
        private static readonly ClassHashManager _classHashManager;

        private static readonly AsyncLocal<int> _recursionCount = new();

        private static readonly Thread _thread;
        private static IDictionary<ulong, ulong>? _pointers;

        static IPersistentFactory()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var exception = (Exception)args.ExceptionObject;
                Log.Error(exception, "Unhandled Exception: {Message}", exception.Message);
                Log.CloseAndFlush();
            };

            if (PmGlobalConfiguration.PersistentGCEnable)
            {
                _classHashManager = ClassHashManager.Instance;
                try
                {
                    _thread = new Thread(() =>
                    {
                        while (true)
                        {
                            Thread.Sleep(PmGlobalConfiguration.CollectFileInterval);

                            try
                            {
                                Log.Information("Collect starting");
                                _pointers =
                                    _pmPointerCounter.Collect(PmGlobalConfiguration.PmInternalsFolder);
                                Log.Information("Collect ending successfully");
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "Error on collect files");
                            }
                        }
                    });
                    Log.Information("First collect starting");
                    _pointers = _pmPointerCounter.Collect(PmGlobalConfiguration.PmInternalsFolder);
                    Log.Information("First collect ending successfully");
                    _generator = new(_pointers);

                    _thread.Start();
                }
                catch (Exception ex)
                {
                    Log.Error($"Error on initial collect", ex);
                }
            }
        }

        object CreateInternalObjectInList(object obj, ulong pmPointer, int fileSizeBytes = 4096)
        {
            return CreateInternalObjectByObject(
            obj,
                PmExtensions.AddExtension(pmPointer.ToString(), PmExtensions.PmListItem),
                pmPointer,
                fileSizeBytes);
        }

        object CreateInternalObjectByObject(object obj, ulong pmPointer, int fileSizeBytes = 4096)
        {
            return CreateInternalObjectByObject(
                obj,
                PmExtensions.AddExtension(pmPointer.ToString(), PmExtensions.PmInternalFile),
                pmPointer,
                fileSizeBytes);
        }

        object CreateInternalObjectByObject(object obj, string pmFilename, ulong pmPointer, int fileSizeBytes = 4096)
        {
            if (obj is ICustomPmClass customObj)
            {
                throw new ApplicationException($"object {obj} is a {nameof(ICustomPmClass)}");
            }

            var pmFilePath = Path.Combine(PmGlobalConfiguration.PmInternalsFolder, pmFilename);
            var objType = obj.GetType();

            if (obj is IProxyTargetAccessor innerInterceptor)
            {
                return obj;
            }

            var isRoot = IsRootObj(pmFilePath);

            var proxyObj = CreatePersistentProxy(
                objType,
                pmFilePath,
                isRootObject: isRoot,
                isListObject: pmFilePath.EndsWith(PmExtensions.PmListItem),
                pmPointer,
                fileSizeBytes);

            foreach (var prop in objType.GetProperties())
            {
                var propType = prop.PropertyType;
                if (propType.IsPrimitive ||
                    propType == typeof(decimal) ||
                    propType == typeof(string))
                {
                    var val = prop.GetValue(obj);
                    if (prop.CanWrite)
                    {
                        prop.SetValue(proxyObj, val);
                    }
                }
                else
                {
                    var innerObj = prop.GetValue(obj);
                    if (innerObj != null && !(innerObj is ICustomPmClass))
                    {
                        var pointer = _pointersToPersistentObjects.GetNext();
                        var proxyInnerObj = CreateInternalObjectByObject(
                            innerObj,
                            pointer);
                        var interceptor =
                            (IPmInterceptor)((IProxyTargetAccessor)proxyObj)
                                .GetInterceptors()
                                .Single(x => x is IPmInterceptor);
                        if (interceptor.OriginalFileInterceptorRedirect is PmManager pmManager)
                        {
                            pmManager.UserDefinedObjectsByProperty[prop] = proxyInnerObj;
                        }
                    }
                }
            }
            return proxyObj;
        }

        object CreatePersistentProxy(
            Type type,
            string filename,
            bool isRootObject,
            bool isListObject,
            ulong pmPointer,
            int fileSizeBytes = 4096,
            bool isLoad = false)
        {
            FileHandlerItem pm;
            if (isListObject)
            {
                pm = FileHandlerManager.CreateListHandler(filename);
            }
            else
            {
                pm = isRootObject ?
                    FileHandlerManager.CreateRootHandler(filename) :
                    FileHandlerManager.CreateInternalObjectHandler(filename);
            }

            var pmContentGenerator = new PmContentGenerator(
                new PmCSharpDefinedTypes(pm.FileBasedStream),
                type);
            var header = pmContentGenerator.CreateHeader(isRootObject);

            var objectPropertiesInfoMapper = new ObjectPropertiesInfoMapper(type, header);
            var interceptor = new PersistentInterceptor(
                new PmManager(
                    new PmUserDefinedTypes(pm.FileBasedStream, objectPropertiesInfoMapper),
                    objectPropertiesInfoMapper
                ),
                pm,
                type,
                filename,
                pmPointer);

            if (_pointers?.TryGetValue(pmPointer, out var pointerCount) ?? false)
            {
                interceptor.FilePointerCount = pointerCount;
            }


            if (_recursionCount.Value == 0 &&
                !isLoad &&
                _classHashManager != null &&
                _classHashManager.GetHashFile(type) == null)
            {
                _recursionCount.Value++;
                _classHashManager.AddHashFile(type);
                _recursionCount.Value--;
            }

            return _generator.CreateClassProxy(type, interceptor);
        }

        object CreateRootObject(Type type, string pmSymbolicLink, int fileSizeBytes = 4096)
        {
            string? pointerStr;
            ulong pointerULong;
            if (!PmFileSystem.FileExists(pmSymbolicLink))
            {
                pointerULong = _pointersToPersistentObjects.GetNext();
                pointerStr = pointerULong.ToString();
                pointerStr = PmFileSystem.CreateSymbolicLinkInInternalsFolder(
                    pmSymbolicLink,
                    PmExtensions.AddExtension(pointerStr, PmExtensions.PmRootFile));
            }
            else if (PmFileSystem.FileIsSymbolicLink(pmSymbolicLink))
            {
                pointerStr = PmFileSystem.GetTargetOfSymbolicLink(pmSymbolicLink);
                pointerULong = PmFileSystem.GetPointerFromSymbolicLink(pmSymbolicLink);
            }
            else
            {
                pointerStr = pmSymbolicLink;
                pointerULong = PmFileSystem.ParseAbsoluteStrPathToULongPointer(pmSymbolicLink);
            }
            return CreatePersistentProxy(type,
                pointerStr,
                isRootObject: true,
                isListObject: pointerStr.EndsWith(PmExtensions.PmListItem),
                pointerULong,
                fileSizeBytes);
        }

        T CreateRootObject<T>(string pmFilename, int fileSizeBytes = 4096)
            where T : class, new()
        {
            return (T)CreateRootObject(typeof(T), pmFilename, fileSizeBytes);
        }

        object LoadFromFile(Type type, string filename, ulong pointer)
        {
            if (!filename.StartsWith(PmGlobalConfiguration.PmInternalsFolder))
            {
                filename = Path.Combine(PmGlobalConfiguration.PmInternalsFolder, filename);
            }

            var isRoot = IsRootObj(filename);

            return CreatePersistentProxy(
                type,
                filename,
                isRoot,
                isListObject: filename.EndsWith(PmExtensions.PmListItem),
                pointer,
                isLoad: true);
        }

        private static bool IsRootObj(string filepath)
        {
            var pm = FileHandlerManager.CreateHandler(filepath);
            var pmCSharpDefinedTypes = new PmCSharpDefinedTypes(pm.FileBasedStream);
            var isRoot = pmCSharpDefinedTypes.ReadBool(offset: sizeof(int));
            return isRoot;
        }
    }
}
