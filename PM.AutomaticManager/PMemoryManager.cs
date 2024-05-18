using PM.AutomaticManager.MetaDatas;
using PM.AutomaticManager.Proxies;
using PM.Core.PMemory;
using PM.FileEngine;
using Serilog;
using System.Reflection;
using System.Text;

namespace PM.AutomaticManager
{
    public class PMemoryManager
    {
        internal PAllocator Allocator { get; }

        private readonly PMemoryMetaDataManager _metaDataManager;

        // Caches
        static readonly Dictionary<Type, ObjectPropertiesInfoMapper> _propertiesMapper = new();

        public PMemoryManager(PAllocator allocator)
        {
            Allocator = allocator;
            _metaDataManager = new PMemoryMetaDataManager(allocator);
        }

        internal TransactonRegionReturn CreateNewTransactionRegion(object obj, uint objectSize)
        {
            return _metaDataManager.CreateNewTransactionRegion(obj, objectSize);
        }

        internal PersistentRegion AllocRootObjectByType(Type type, string objectUserID)
        {
            ObjectPropertiesInfoMapper objectPropertiesInfoMapper = RegisterNewObjectPropertiesInfoMapper(type);
            return _metaDataManager.AllocRootObjectByType(type, objectUserID, objectPropertiesInfoMapper);
        }

        internal ObjectPropertiesInfoMapper RegisterNewObjectPropertiesInfoMapper(Type type)
        {
            ObjectPropertiesInfoMapper objectPropertiesInfoMapper;
            if (!_propertiesMapper.ContainsKey(type))
            {
                objectPropertiesInfoMapper = _propertiesMapper[type] = new ObjectPropertiesInfoMapper(type);
            }
            else
            {
                objectPropertiesInfoMapper = _propertiesMapper[type];
            }

            return objectPropertiesInfoMapper;
        }

        private byte[] GetObjectBuffer(object objeto)
        {
            // Cria um array de bytes para armazenar os bytes concatenados
            byte[] bytes = new byte[0];

            // Obtém todas as propriedades do objeto
            PropertyInfo[] propriedades = objeto.GetType().GetProperties();

            // Itera sobre as propriedades
            foreach (var propriedade in propriedades)
            {
                // Obtém o valor da propriedade
                object valorPropriedade = propriedade.GetValue(objeto);

                // Se for um valor primitivo, converte para bytes e concatena
                if (propriedade.PropertyType.IsPrimitive)
                {
                    byte[] bytesValor = GetBytesFromObject(valorPropriedade);
                    bytes = ConcatenarBytes(bytes, bytesValor);
                }
                // Se for uma string, converte para bytes UTF-8 e concatena
                else if (propriedade.PropertyType == typeof(string))
                {
                    byte[] bytesString = Encoding.UTF8.GetBytes((string)valorPropriedade);
                    bytes = ConcatenarBytes(bytes, bytesString);
                }
                // Se for um vetor, trata de acordo com a implementação definida
                else if (propriedade.PropertyType.IsArray)
                {
                    throw new NotImplementedException();
                    Array array = (Array)valorPropriedade;
                    // Para o array, pode-se concatenar os bytes dos elementos do array de alguma forma específica.
                    // Aqui, estamos convertendo cada elemento para uma string e concatenando.
                    foreach (var item in array)
                    {
                        byte[] bytesItem = Encoding.UTF8.GetBytes(item.ToString());
                        bytes = ConcatenarBytes(bytes, bytesItem);
                    }
                }
                // Se for um objeto complexo, chama recursivamente o método para obter os bytes
                else
                {
                    throw new NotImplementedException();
                    byte[] bytesObjeto = GetObjectBuffer(valorPropriedade);
                    bytes = ConcatenarBytes(bytes, bytesObjeto);
                }
            }

            return bytes;
        }

        static byte[] GetBytesFromObject(object valor)
        {
            Type type = valor.GetType();

            if (type == typeof(bool))
            {
                return BitConverter.GetBytes((bool)valor);
            }
            else if (type == typeof(char))
            {
                return BitConverter.GetBytes((char)valor);
            }
            else if (type == typeof(sbyte))
            {
                return new byte[] { (byte)((sbyte)valor) };
            }
            else if (type == typeof(byte))
            {
                return new byte[] { (byte)valor };
            }
            else if (type == typeof(short))
            {
                return BitConverter.GetBytes((short)valor);
            }
            else if (type == typeof(ushort))
            {
                return BitConverter.GetBytes((ushort)valor);
            }
            else if (type == typeof(int))
            {
                return BitConverter.GetBytes((int)valor);
            }
            else if (type == typeof(uint))
            {
                return BitConverter.GetBytes((uint)valor);
            }
            else if (type == typeof(long))
            {
                return BitConverter.GetBytes((long)valor);
            }
            else if (type == typeof(ulong))
            {
                return BitConverter.GetBytes((ulong)valor);
            }
            else if (type == typeof(float))
            {
                return BitConverter.GetBytes((float)valor);
            }
            else if (type == typeof(double))
            {
                return BitConverter.GetBytes((double)valor);
            }
            else if (type == typeof(decimal))
            {
                int[] bits = decimal.GetBits((decimal)valor);
                byte[] bytes = new byte[bits.Length * sizeof(int)];
                Buffer.BlockCopy(bits, 0, bytes, 0, bytes.Length);
                return bytes;
            }
            else if (type == typeof(string))
            {
                return Encoding.UTF8.GetBytes((string)valor);
            }

            throw new ArgumentException($"Object type not supported {type.Name}");
        }

        static byte[] ConcatenarBytes(byte[] bytes1, byte[] bytes2)
        {
            byte[] resultado = new byte[bytes1.Length + bytes2.Length];
            Buffer.BlockCopy(bytes1, 0, resultado, 0, bytes1.Length);
            Buffer.BlockCopy(bytes2, 0, resultado, bytes1.Length, bytes2.Length);
            return resultado;
        }

        internal void UpdateProperty(PersistentRegion persistentRegion, Type targetType, PropertyInfo property, object? value)
        {
            if (!_propertiesMapper.TryGetValue(targetType, out var mapper))
            {
                mapper = _propertiesMapper[targetType] = new ObjectPropertiesInfoMapper(targetType);
            }

            var propertyInternalOffset = mapper.GetPropertyOffset(property);
            if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(decimal))
            {
                persistentRegion.Write(GetBytesFromObject(value), offset: propertyInternalOffset);
            }
            else if (property.PropertyType == typeof(string))
            {
                // TODO: Add transaction
                var strValue = (string)value;
                var objectBytes = Encoding.UTF8.GetBytes(strValue);
                var region = Allocator.Alloc((uint)objectBytes.Length);
                region.Write(objectBytes, offset: 0);

                var blockIdBytes = GetBytesFromObject(region.BlockID);
                var regionIndexBytes = GetBytesFromObject(region.RegionIndex);
                persistentRegion.Write(
                    blockIdBytes
                    .Concat(regionIndexBytes)
                    .ToArray(),
                    offset: propertyInternalOffset);
            }
            else
            {
                if (value is null)
                {
                    // 1. Read pointer from region to remove;
                    var blockIDBytes = persistentRegion.Read(sizeof(uint), offset: propertyInternalOffset);
                    propertyInternalOffset += sizeof(uint);
                    var regionIndex = persistentRegion.Read(sizeof(byte), offset: propertyInternalOffset)[0];

                    var blockId = BitConverter.ToUInt32(blockIDBytes);
                    // Dont have pointer
                    if (blockId == 0)
                    {
                        // nothing to do, property already is null
                        return;
                    }
                    // 2. remove region pointer from parent
                    propertyInternalOffset -= sizeof(uint);
                    persistentRegion.Write(
                        GetBytesFromObject((UInt32)0)
                        .Concat(GetBytesFromObject((byte)0))
                        .ToArray(),
                        offset: propertyInternalOffset);
                    // 3. Get block to mark region as free region
                    var blockToRemove = Allocator.GetBlock(blockId);
                    // 4. Mark region as free
                    blockToRemove.MarkRegionAsFree(regionIndex);

                    return;
                }

                // Verify if the object already is a proxy object from PM.
                if (CastleManager.TryGetCastleProxyInterceptor(value, out var pmInterceptor))
                {
                    var proxyObjectRegion = pmInterceptor!.PersistentRegion;

                    persistentRegion.Write(
                        GetBytesFromObject(proxyObjectRegion.BlockID)
                        .Concat(GetBytesFromObject(proxyObjectRegion.RegionIndex))
                        .ToArray(),
                        offset: propertyInternalOffset);
                    return;
                }

                // TODO: Add transaction

                var objectPropertiesInfoMapper = new ObjectPropertiesInfoMapper(property.PropertyType);
                var objectLength = (uint)objectPropertiesInfoMapper.GetTypeSize();
                var region = Allocator.Alloc(objectLength);
                var objectBytes = new byte[objectLength];
                int i = 0;
                foreach (var innerProperty in property.PropertyType.GetProperties())
                {
                    if (innerProperty.PropertyType.IsPrimitive || innerProperty.PropertyType == typeof(decimal))
                    {
                        var propertyValueBytes = GetBytesFromObject(innerProperty.GetValue(value)!);
                        Buffer.BlockCopy(propertyValueBytes, 0, objectBytes, i, propertyValueBytes.Length);
                        i += propertyValueBytes.Length;
                    }
                    else // Complex class, do recursion
                    {
                        var objectValue = innerProperty.GetValue(value);
                        if (objectValue != null)
                        {
                            UpdateProperty(region, property.PropertyType, innerProperty, objectValue);
                        }
                        i += 5;
                    }
                }
                region.Write(objectBytes, offset: 0);

                var blockIdBytes = GetBytesFromObject(region.BlockID);
                var regionIndexBytes = GetBytesFromObject(region.RegionIndex);
                persistentRegion.Write(
                    blockIdBytes
                    .Concat(regionIndexBytes)
                    .ToArray(),
                    offset: propertyInternalOffset);
            }
        }

        internal object? GetPropertyValue(PersistentRegion persistentRegion, Type targetType, PropertyInfo property, out bool returnIsProxyObject)
        {
            returnIsProxyObject = false;
            if (!_propertiesMapper.TryGetValue(targetType, out var mapper))
            {
                mapper = _propertiesMapper[targetType] = new ObjectPropertiesInfoMapper(targetType);
            }

            var propertyInternalOffset = mapper.GetPropertyOffset(property);
            if (property.PropertyType.IsPrimitive)
            {
                if (property.PropertyType == typeof(int))
                {
                    var byteValue = persistentRegion.Read(sizeof(int), offset: propertyInternalOffset);
                    return BitConverter.ToInt32(byteValue);
                }
                else if (property.PropertyType == typeof(byte))
                {
                    var byteValue = persistentRegion.Read(sizeof(byte), offset: propertyInternalOffset);
                    return byteValue[0];
                }
                else if (property.PropertyType == typeof(short))
                {
                    var byteValue = persistentRegion.Read(sizeof(short), offset: propertyInternalOffset);
                    return BitConverter.ToInt16(byteValue);
                }
                else if (property.PropertyType == typeof(long))
                {
                    var byteValue = persistentRegion.Read(sizeof(long), offset: propertyInternalOffset);
                    return BitConverter.ToInt64(byteValue);
                }
                else if (property.PropertyType == typeof(float))
                {
                    var byteValue = persistentRegion.Read(sizeof(float), offset: propertyInternalOffset);
                    return BitConverter.ToSingle(byteValue, 0);
                }
                else if (property.PropertyType == typeof(double))
                {
                    var byteValue = persistentRegion.Read(sizeof(double), offset: propertyInternalOffset);
                    return BitConverter.ToDouble(byteValue, 0);
                }
                else if (property.PropertyType == typeof(bool))
                {
                    var byteValue = persistentRegion.Read(sizeof(bool), offset: propertyInternalOffset);
                    return BitConverter.ToBoolean(byteValue, 0);
                }
                else if (property.PropertyType == typeof(char))
                {
                    var byteValue = persistentRegion.Read(sizeof(char), offset: propertyInternalOffset);
                    return BitConverter.ToChar(byteValue, 0);
                }
                else if (property.PropertyType == typeof(sbyte))
                {
                    var byteValue = persistentRegion.Read(sizeof(sbyte), offset: propertyInternalOffset);
                    return (sbyte)byteValue[0];
                }
                else if (property.PropertyType == typeof(uint))
                {
                    var byteValue = persistentRegion.Read(sizeof(uint), offset: propertyInternalOffset);
                    return BitConverter.ToUInt32(byteValue, 0);
                }
                else if (property.PropertyType == typeof(ushort))
                {
                    var byteValue = persistentRegion.Read(sizeof(ushort), offset: propertyInternalOffset);
                    return BitConverter.ToUInt16(byteValue, 0);
                }
                else if (property.PropertyType == typeof(ulong))
                {
                    var byteValue = persistentRegion.Read(sizeof(ulong), offset: propertyInternalOffset);
                    return BitConverter.ToUInt64(byteValue, 0);
                }
            }
            else
            {
                if (property.PropertyType == typeof(decimal))
                {
                    var byteValue = persistentRegion.Read(sizeof(decimal), offset: propertyInternalOffset);
                    return new decimal(new int[] {
                        BitConverter.ToInt32(byteValue, 0),
                        BitConverter.ToInt32(byteValue, sizeof(int)),
                        BitConverter.ToInt32(byteValue, sizeof(int) * 2),
                        BitConverter.ToInt32(byteValue, sizeof(int) * 3)
                    });
                }
                else if (property.PropertyType == typeof(string))
                {
                    var blockIDBytes = persistentRegion.Read(sizeof(uint), offset: propertyInternalOffset);
                    propertyInternalOffset += sizeof(uint);
                    var regionIndex = persistentRegion.Read(sizeof(byte), offset: propertyInternalOffset)[0];

                    var blockId = BitConverter.ToUInt32(blockIDBytes);
                    // Dont have pointer
                    if (blockId == 0)
                    {
                        return null;
                    }

                    var strRegion = Allocator.GetRegion(blockId, regionIndex);

                    var stringBytes = new List<byte>();
                    var strRegionInternalOffset = 0;
                    while (true)
                    {
                        var @byte = strRegion.Read(count: 1, offset: strRegionInternalOffset)[0];

                        if (@byte == 0) break;

                        stringBytes.Add(@byte);
                        strRegionInternalOffset += 1;
                    }
                    return Encoding.UTF8.GetString(stringBytes.ToArray());
                }
                else
                {
                    // Complex types
                    var blockIDBytes = persistentRegion.Read(sizeof(uint), offset: propertyInternalOffset);
                    var blockId = BitConverter.ToUInt32(blockIDBytes);
                    if (blockId == 0)
                    {
                        Log.Information("Attempt to get a object that have null pointer.");
                        return null;
                    }
                    propertyInternalOffset += sizeof(uint);

                    var regionIndex = persistentRegion.Read(sizeof(byte), offset: propertyInternalOffset)[0];
                    var objRegion = Allocator.GetRegion(blockId, regionIndex);

                    // Create proxy and return
                    InnerObjectFactory innerObjectFactory = new(this);
                    var obj = innerObjectFactory.CreateInnerObject(objRegion, property.PropertyType);
                    returnIsProxyObject = true;
                    return obj;
                }
            }
            throw new NotImplementedException();
        }

        internal bool ObjectExists(string objectUserID)
        {
            return _metaDataManager.ObjectExists(objectUserID);
        }

        internal PersistentRegion GetRegionByObjectUserID(string objectUserID)
        {
            var metaDataStructure = _metaDataManager.GetByObjectUserID(objectUserID);
            if (metaDataStructure.MetadataType == MetadataType.Object)
            {
                if (metaDataStructure.ObjectMetaDataStructure is null)
                    throw new ApplicationException($"{nameof(metaDataStructure.ObjectMetaDataStructure)} cannot be null");

                var blockID = metaDataStructure.ObjectMetaDataStructure.BlockID;
                var regionIndex = metaDataStructure.ObjectMetaDataStructure.RegionIndex;

                return Allocator.GetRegion(blockID, regionIndex);
            }

            throw new NotImplementedException();
        }
    }
}
