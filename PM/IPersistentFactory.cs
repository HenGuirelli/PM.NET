using Castle.DynamicProxy;
using PM.Configs;
using PM.Core;
using PM.Factories;
using PM.Managers;
using PM.PmContent;
using PM.Proxies;
using System.IO;

namespace PM
{
    public interface IPersistentFactory
    {
        private static readonly PointersToPersistentObjects _pointersToPersistentObjects = new();

        object CreateRootObjectByObject(object obj, string pmFilename, int fileSizeBytes = 4096)
        {
            var objType = obj.GetType();

            if (obj is IProxyTargetAccessor innerInterceptor)
            {
                throw new ApplicationException($"object of type {objType} already has a proxy");
            }

            var proxyObj = CreateRootObject(objType, pmFilename, fileSizeBytes);

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
                        var proxyInnerObj = CreateRootObjectByObject(
                            innerObj,
                            pointer.ToString());
                        var interceptor =
                            (PersistentInterceptor)((IProxyTargetAccessor)proxyObj)
                                .GetInterceptors()
                                .Single(x => x is PersistentInterceptor);
                        if (interceptor.OriginalFileInterceptorRedirect is PmManager pmManager)
                        {
                            pmManager.UserDefinedObjectsByProperty[prop] = proxyInnerObj;
                        }
                    }
                }
            }
            return proxyObj;
        }

        object CreateRootObject(Type type, string pmSymbolicLink, int fileSizeBytes = 4096)
        {
            var generator = new ProxyGenerator();

            var pointer = default(string);
            var pmFile = default(string);
            if (!File.Exists(pmSymbolicLink))
            {
                pointer = _pointersToPersistentObjects.GetNext().ToString();
                pmFile = Path.Combine(PmGlobalConfiguration.PmInternalsFolder, pointer);
                File.CreateSymbolicLink(pmSymbolicLink, pmFile);
            }
            else if ((File.GetAttributes(pmSymbolicLink) & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
            {
                var fi = new FileInfo(pmSymbolicLink);
                pointer = fi.LinkTarget;
                pmFile = Path.Combine(PmGlobalConfiguration.PmInternalsFolder, pointer);
            }
            var pm = PmFactory.CreatePm(new PmMemoryMappedFileConfig(pmFile, fileSizeBytes));

            var pmContentGenerator = new PmContentGenerator(
                new PmCSharpDefinedTypes(pm),
                type);
            var header = pmContentGenerator.CreateHeader();

            var objectPropertiesInfoMapper = new ObjectPropertiesInfoMapper(type, header);
            var interceptor = new PersistentInterceptor(
                new PmManager(
                    new PmUserDefinedTypes(pm, objectPropertiesInfoMapper),
                    objectPropertiesInfoMapper),
                type,
                pointer);

            return generator.CreateClassProxy(type, interceptor);
        }

        T CreateRootObject<T>(string pmFilename, int fileSizeBytes = 4096)
            where T : class, new()
        {
            return (T)CreateRootObject(typeof(T), pmFilename, fileSizeBytes);
        }
    }
}
