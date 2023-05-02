using Castle.DynamicProxy;
using PM.Configs;
using PM.Core;
using PM.Factories;
using PM.Managers;
using PM.PmContent;
using PM.Proxies;
using PM.Startup;

namespace PM
{
    public interface IPersistentFactory
    {
        private static readonly PointersToPersistentObjects _pointersToPersistentObjects = new();
        private static readonly PmProxyGenerator _generator = new();
        private static readonly IPmPointerCounter _pmPointerCounter = new PmPointerCounter();

        static IPersistentFactory()
        {
            IDictionary<ulong, ulong> pointers = _pmPointerCounter.MapPointers(PmGlobalConfiguration.PmInternalsFolder);
        }

        object CreateInternalObjectByObject(object obj, ulong pmPointer, int fileSizeBytes = 4096)
        {
            if (obj is ICustomPmClass customObj)
            {
                throw new ApplicationException($"object {obj} is a {nameof(ICustomPmClass)}");
            }

            var pmFilename = $"{pmPointer}.pm";
            pmFilename = Path.Combine(PmGlobalConfiguration.PmInternalsFolder, pmFilename);
            var objType = obj.GetType();

            if (obj is IProxyTargetAccessor innerInterceptor)
            {
                throw new ApplicationException($"object of type {objType} already has a proxy");
            }

            var proxyObj = CreatePersistentProxy(objType, pmFilename, isRootObject: false, fileSizeBytes, pmPointer);

            foreach (var prop in objType.GetProperties())
            {
                var propType = prop.PropertyType;
                if (propType.IsPrimitive ||
                    propType == typeof(decimal) ||
                    propType == typeof(string))
                {
                    var val = prop.GetValue(obj);
                    prop.SetValue(proxyObj, val);
                }
                else
                {
                    var innerObj = prop.GetValue(obj);
                    if (innerObj != null)
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
            int fileSizeBytes = 4096,
            ulong? pmPointer = null)
        {
            var pm = PmFactory.CreatePm(filename, fileSizeBytes);

            var pmContentGenerator = new PmContentGenerator(
                new PmCSharpDefinedTypes(pm),
                type);
            var header = pmContentGenerator.CreateHeader(isRootObject);

            var objectPropertiesInfoMapper = new ObjectPropertiesInfoMapper(type, header);
            var interceptor = new PersistentInterceptor(
                new PmManager(
                    new PmUserDefinedTypes(pm, objectPropertiesInfoMapper),
                    objectPropertiesInfoMapper
                ),
                type,
                filename,
                pmPointer);

            return _generator.CreateClassProxy(type, interceptor);
        }

        object CreateRootObject(Type type, string pmSymbolicLink, int fileSizeBytes = 4096)
        {
            string? pointerStr;
            ulong? pointerULong;
            if (!PmFileSystem.FileExists(pmSymbolicLink))
            {
                pointerULong = _pointersToPersistentObjects.GetNext();
                pointerStr = pointerULong.ToString();
                pointerStr = PmFileSystem.CreateSymbolicLinkInInternalsFolder(pmSymbolicLink, pointerStr + ".pm");
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
            return CreatePersistentProxy(type, pointerStr, isRootObject: true, fileSizeBytes, pointerULong);
        }

        T CreateRootObject<T>(string pmFilename, int fileSizeBytes = 4096)
            where T : class, new()
        {
            return (T)CreateRootObject(typeof(T), pmFilename, fileSizeBytes);
        }

        object LoadFromFile(Type propertyType, string filename)
        {
            if (!filename.StartsWith(PmGlobalConfiguration.PmInternalsFolder))
            {
                filename = Path.Combine(PmGlobalConfiguration.PmInternalsFolder, filename);
            }
            var pm = PmFactory.CreatePm(filename, 4096);
            var pmCSharpDefinedTypes = new PmCSharpDefinedTypes(pm);
            var isRoot = pmCSharpDefinedTypes.ReadBool(offset: sizeof(int));

            return CreatePersistentProxy(
                propertyType,
                filename,
                isRoot);
        }
    }
}
