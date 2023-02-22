using PM.Core;
using PM.Factories;
using System.Reflection;
using PM.Proxies;
using PM.Configs;

namespace PM.Managers
{
    internal class PmManager : IInterceptorRedirect
    {
        public ObjectPropertiesInfoMapper ObjectMapper { get; }
        public PmMemoryMappedFileConfig PmMemoryMappedFile { get; }

        private readonly PmUserDefinedTypes _pm;

        internal readonly Dictionary<PropertyInfo, object> UserDefinedObjectsByProperty = new();
        private readonly Dictionary<ulong, PmCSharpDefinedTypes> _pmInnerObjectsByPointer = new();

        private readonly PointersToPersistentObjects _pointersToPersistentObjects = new();

        public PmManager(
            PmUserDefinedTypes pm,
            ObjectPropertiesInfoMapper objectPropertiesSizeMapper)
        {
            _pm = pm;
            PmMemoryMappedFile = pm.PmMemoryMappedFile;
            ObjectMapper = objectPropertiesSizeMapper;
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
                        var pointer = _pointersToPersistentObjects.GetNext();
                        var pm = PmFactory.CreatePm(new PmMemoryMappedFileConfig(Path.Combine(PmGlobalConfiguration.PmInternalsFolder, pointer.ToString())));
                        var pmCSharpDefinedTypes = new PmCSharpDefinedTypes(pm);
                        _pmInnerObjectsByPointer[pointer] = pmCSharpDefinedTypes;
                        pmCSharpDefinedTypes.WriteString(valuestr);

                        _pm.UpdateProperty(property, pointer);
                    }
                    else
                    {
                        // User defined objects
                        var pointer = _pointersToPersistentObjects.GetNext();
                        IPersistentFactory persistentFactory = new PersistentFactory();
                        var proxy = persistentFactory.CreateRootObjectByObject(
                            value,
                            pointer.ToString());
                        UserDefinedObjectsByProperty[property] = proxy;
                    }
                }
            }
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
                        if (_pmInnerObjectsByPointer.TryGetValue(pointer, out var innerPm))
                        {
                            return innerPm.ReadString();
                        }
                        else
                        {
                            var path = Path.Combine(PmGlobalConfiguration.PmInternalsFolder, pointer.ToString());
                            var filesize = TransactionFolderFactory.Create();
                            try
                            {
                                var pm = PmFactory.CreatePm(
                                    new PmMemoryMappedFileConfig(
                                        name: path,
                                        size: (int)filesize.GetFileSize(path)));
                                var stringPmCSharpDefinedTypes = new PmCSharpDefinedTypes(pm);
                                _pmInnerObjectsByPointer[pointer] = stringPmCSharpDefinedTypes;
                                return stringPmCSharpDefinedTypes.ReadString();
                            } 
                            catch(FileNotFoundException) 
                            {
                                return null;
                            }
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
