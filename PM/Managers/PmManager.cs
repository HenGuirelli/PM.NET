using PM.Core;
using PM.Factories;
using System.Reflection;
using PM.Proxies;
using PM.Configs;
using PM.CastleHelpers;
using PM.Collections;
using System.Globalization;

namespace PM.Managers
{
    internal class PmManager : IInterceptorRedirect
    {
        public ObjectPropertiesInfoMapper ObjectMapper { get; }
        public FileBasedStream PmMemoryMappedFile { get; }

        private readonly PmUserDefinedTypes _pm;

        internal readonly Dictionary<PropertyInfo, object> UserDefinedObjectsByProperty = new();

        private readonly PointersToPersistentObjects _pointersToPersistentObjects = new();

        public PmManager(
            PmUserDefinedTypes pm,
            ObjectPropertiesInfoMapper objectPropertiesSizeMapper)
        {
            _pm = pm ?? throw new ArgumentNullException(nameof(pm));
            PmMemoryMappedFile = pm.PmMemoryMappedFile;
            ObjectMapper = objectPropertiesSizeMapper ?? throw new ArgumentNullException(nameof(objectPropertiesSizeMapper));
        }

        public void InsertValuePm(PropertyInfo property, object value)
        {
            // If is not a property will be null
            if (property != null)
            {
                var valueType = property.PropertyType;
                if (valueType.IsPrimitive || valueType == typeof(decimal))
                {
                    // Start inserting on PM
                    if (value is byte valueByte) _pm.UpdateProperty(property, valueByte);
                    if (value is sbyte valueSByte) _pm.UpdateProperty(property, valueSByte);
                    if (value is short valueShort) _pm.UpdateProperty(property, valueShort);
                    if (value is ushort valueUShort) _pm.UpdateProperty(property, valueUShort);
                    if (value is uint valueUInt) _pm.UpdateProperty(property, valueUInt);
                    if (value is int valueInt) _pm.UpdateProperty(property, valueInt);
                    if (value is long valueLong) _pm.UpdateProperty(property, valueLong);
                    if (value is ulong valueULong) _pm.UpdateProperty(property, valueULong);
                    if (value is float valueFloat) _pm.UpdateProperty(property, valueFloat);
                    if (value is double valueDouble) _pm.UpdateProperty(property, valueDouble);
                    if (value is decimal valueDecimal) _pm.UpdateProperty(property, valueDecimal);
                    if (value is char valueChar) _pm.UpdateProperty(property, valueChar);
                    if (value is bool valueBool) _pm.UpdateProperty(property, valueBool);
                }
                else
                {
                    // User Defined Objects or reference types
                    if (value is string valuestr)
                    {
                        ulong pointer = GetPointerIfExistsOrNew(property);

                        var pm = FileHandlerManager.CreateInternalObjectHandler(
                            Path.Combine(PmGlobalConfiguration.PmInternalsFolder, pointer.ToString()));
                        var pmCSharpDefinedTypes = new PmCSharpDefinedTypes(pm.FileBasedStream);
                        pmCSharpDefinedTypes.WriteString(valuestr);

                        _pm.UpdateProperty(property, pointer);
                    }
                    else
                    {
                        if (value is ICustomPmClass customObj)
                        {
                            ulong pointer = customObj.PmPointer;
                            UserDefinedObjectsByProperty[property] = value;
                            _pm.UpdateProperty(property, pointer);
                        }
                        else
                        {
                            if (value is null)
                            {
                                ulong nullPtr = 0;
                                _pm.UpdateProperty(property, nullPtr);

                                SubtractReferencePointerFromOldObject(property);
                                return;
                            }

                            ulong pointer = GetPointerIfExistsOrNew(property);
                            // User defined objects
                            IPersistentFactory persistentFactory = new PersistentFactory();
                            var proxy = persistentFactory.CreateInternalObjectByObject(
                                value,
                                pointer);
                            _pm.UpdateProperty(property, pointer);

                            if (CastleManager.TryGetInterceptor(proxy, out var objInterceptor))
                            {
                                objInterceptor!.FilePointerCount++;
                            }


                            SubtractReferencePointerFromOldObject(property);


                            UserDefinedObjectsByProperty[property] = proxy;
                        }
                    }
                }
            }
        }

        private void SubtractReferencePointerFromOldObject(PropertyInfo property)
        {
            if (UserDefinedObjectsByProperty.TryGetValue(property, out var oldObj) &&
                CastleManager.TryGetInterceptor(oldObj, out var oldObjInterceptor))
            {
                oldObjInterceptor!.FilePointerCount--;
            }
        }

        private ulong GetPointerIfExistsOrNew(PropertyInfo property)
        {
            var pointer = _pm.GetULongPropertValue(property);
            var pointerAlreadyExists = pointer != 0;
            if (!pointerAlreadyExists)
            {
                pointer = _pointersToPersistentObjects.GetNext();
            }

            return pointer;
        }

        public object? GetValuePm(PropertyInfo property)
        {
            if (property != null)
            {
                var typePropertyID = ObjectMapper.GetPropetyID(property);
                // ComplexObjects
                if (typePropertyID == 14)
                {
                    if (UserDefinedObjectsByProperty.TryGetValue(property, out var @object))
                    {
                        return @object;
                    }
                    var pointer = _pm.GetULongPropertValue(property);
                    if (pointer != 0)
                    {
                        if (typeof(ICustomPmClass).IsAssignableFrom(property.PropertyType))
                        {
                            if (typeof(IPmList).IsAssignableFrom(property.PropertyType))
                            {
                                object[] parameterValues = new object[]
                                {
                                    Path.Combine(PmGlobalConfiguration.PmInternalsFolder, pointer.ToString() + ".pm"),
                                    pointer
                                };
                                object obj = Activator.CreateInstance(
                                    property.PropertyType,
                                    BindingFlags.NonPublic | BindingFlags.Instance,
                                    null,
                                    parameterValues,
                                    null) ?? throw new ApplicationException("Error on Activator.CreateInstance returning null");

                                UserDefinedObjectsByProperty[property] = obj;
                                return obj;
                            }
                            throw new ApplicationException("You cant use a custom pm class inside root object");
                        }

                        // User defined objects
                        IPersistentFactory persistentFactory = new PersistentFactory();
                        var proxy = persistentFactory.LoadFromFile(
                            property.PropertyType,
                            pointer.ToString() + ".pm",
                            pointer);
                        UserDefinedObjectsByProperty[property] = proxy;
                        return proxy;
                    }
                    return null;
                }

                var propType = property.PropertyType;
                if (propType.IsPrimitive || propType == typeof(decimal))
                {
                    // Start inserting on PM
                    if (propType == typeof(byte))
                    {
                        return _pm.GetBytePropertValue(property);
                    }
                    if (propType == typeof(sbyte))
                    {
                        return _pm.GetSBytePropertValue(property);
                    }
                    if (propType == typeof(short))
                    {
                        return _pm.GetShortPropertValue(property);
                    }
                    if (propType == typeof(ushort))
                    {
                        return _pm.GetUShortPropertValue(property);
                    }
                    if (propType == typeof(uint))
                    {
                        return _pm.GetUIntPropertValue(property);
                    }
                    if (propType == typeof(int))
                    {
                        return _pm.GetIntPropertValue(property);
                    }
                    if (propType == typeof(long))
                    {
                        return _pm.GetLongPropertValue(property);
                    }
                    if (propType == typeof(ulong))
                    {
                        return _pm.GetULongPropertValue(property);
                    }
                    if (propType == typeof(float))
                    {
                        return _pm.GetFloatPropertValue(property);
                    }
                    if (propType == typeof(double))
                    {
                        return _pm.GetDoublePropertValue(property);
                    }
                    if (propType == typeof(decimal))
                    {
                        return _pm.GetDecimalPropertValue(property);
                    }
                    if (propType == typeof(char))
                    {
                        return _pm.GetCharPropertValue(property);
                    }
                    if (propType == typeof(bool))
                    {
                        return _pm.GetBoolPropertValue(property);
                    }
                }
                else
                {
                    // User Defined Objects or reference types
                    if (propType == typeof(string))
                    {
                        var pointer = _pm.GetULongPropertValue(property);
                        if (pointer == 0)
                        {
                            return null;
                        }

                        try
                        {
                            var pm = FileHandlerManager.CreateInternalObjectHandler(
                                Path.Combine(PmGlobalConfiguration.PmInternalsFolder, pointer.ToString()));

                            var stringPmCSharpDefinedTypes = new PmCSharpDefinedTypes(pm.FileBasedStream);
                            return stringPmCSharpDefinedTypes.ReadString();
                        }
                        catch (FileNotFoundException)
                        {
                            return null;
                        }
                    }
                }
            }

            return null;
        }

        public void Lock()
        {
            _pm.Lock();
        }

        public void Release()
        {
            _pm.Release();
        }
    }
}
