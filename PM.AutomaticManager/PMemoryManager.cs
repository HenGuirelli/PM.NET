using PM.Core.PMemory;
using PM.FileEngine;
using System.Buffers.Binary;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PM.AutomaticManager
{
    public class PMemoryManager
    {
        private readonly PAllocator _allocator;
        private readonly PersistentRegion _metadataRegion;
        private volatile int _nextStructureOffset = 0;

        // Caches
        readonly Dictionary<string, MetaDataStructure> _metaDataStructure = new();
        static readonly Dictionary<Type, ObjectPropertiesInfoMapper> _propertiesMapper = new();

        public PMemoryManager(PAllocator allocator)
        {
            _allocator = allocator;

            if (!allocator.HasAnyBlocks)
            {
                // Reserve first block for metadata
                // 25000 pointers capacity
                _metadataRegion = allocator.Alloc(100_000);
            }
            else
            {
                _metadataRegion = _allocator.FirstPersistentBlockLayout!.Regions[0];
                _nextStructureOffset = 0;
                while (true)
                {
                    var metadataType = _metadataRegion.Read(count: 1, offset: _nextStructureOffset)[0]; // Read First metadataStructure
                    _nextStructureOffset += 1;
                    if (metadataType != 0) // Have value!!
                    {
                        if (metadataType == (byte)MetadataType.Object)
                        {
                            var blockId = BitConverter.ToUInt32(_metadataRegion.Read(count: 4, offset: _nextStructureOffset));
                            _nextStructureOffset += sizeof(UInt32);
                            var regionIndex = _metadataRegion.Read(count: 1, offset: _nextStructureOffset)[0];
                            _nextStructureOffset += sizeof(byte);
                            var offsetInnerRegion = BitConverter.ToUInt16(_metadataRegion.Read(count: 2, offset: _nextStructureOffset));
                            _nextStructureOffset += sizeof(UInt16);
                            var objectSize = BitConverter.ToUInt32(_metadataRegion.Read(count: 4, offset: _nextStructureOffset));
                            _nextStructureOffset += sizeof(UInt32);
                            var stringBytes = new List<byte>();
                            while (true)
                            {
                                var @byte = _metadataRegion.Read(count: 1, offset: _nextStructureOffset)[0];

                                if (@byte == 0) break;

                                stringBytes.Add(@byte);
                                _nextStructureOffset += 1;
                            }

                            var objectUserID = Encoding.UTF8.GetString(stringBytes.ToArray());
                            _metaDataStructure.Add(objectUserID,
                                new MetaDataStructure
                                {
                                    MetadataType = MetadataType.Object,
                                    ObjectMetaDataStructure = new ObjectMetaDataStructure
                                    {
                                        BlockID = blockId,
                                        RegionIndex = regionIndex,
                                        OffsetInnerRegion = offsetInnerRegion,
                                        ObjectSize = objectSize,
                                        ObjectUserID = objectUserID
                                    }
                                });
                        }
                    }
                    else { break; }
                }
            }
        }

        public void AddNewObject(string objectUserID, object obj)
        {
            var type = obj.GetType();
            if (!_propertiesMapper.ContainsKey(type))
            {
                _propertiesMapper[type] = new ObjectPropertiesInfoMapper(type);
            }

            var objectBuffer = GetObjectBuffer(obj);

            var objectRegion = _allocator.Alloc((uint)objectBuffer.Length);
            var blockId = BitConverter.GetBytes(objectRegion.BlockID);
            var regionIndex = objectRegion.RegionIndex;
            UInt16 offsetInnerRegion = 0;
            var offsetInnerRegionBytes = BitConverter.GetBytes(offsetInnerRegion); // Always zero
            var objectSizeBytes = BitConverter.GetBytes(objectRegion.RegionIndex);

            var buffer = new byte[12 + objectUserID.Length];
            var idBytes = Encoding.UTF8.GetBytes(objectUserID);
            buffer[0] = (byte)MetadataType.Object;
            Array.Copy(sourceArray: blockId, sourceIndex: 0, destinationArray: buffer, destinationIndex: 1, length: blockId.Length);
            buffer[5] = regionIndex;
            Array.Copy(sourceArray: offsetInnerRegionBytes, sourceIndex: 0, destinationArray: buffer, destinationIndex: 6, length: offsetInnerRegionBytes.Length);
            Array.Copy(sourceArray: objectSizeBytes, sourceIndex: 0, destinationArray: buffer, destinationIndex: 8, length: objectSizeBytes.Length);
            Array.Copy(sourceArray: idBytes, sourceIndex: 0, destinationArray: buffer, destinationIndex: 12, length: idBytes.Length);

            objectRegion.Write(objectBuffer);
            _metadataRegion.Write(buffer, _nextStructureOffset);
            _nextStructureOffset += buffer.Length;

            // Add to cache
            _metaDataStructure.Add(objectUserID, new MetaDataStructure
            {
                MetadataType = MetadataType.Object,
                ObjectMetaDataStructure = new ObjectMetaDataStructure
                {
                    BlockID = objectRegion.BlockID,
                    RegionIndex = regionIndex,
                    OffsetInnerRegion = offsetInnerRegion,
                    ObjectSize = (uint)objectBuffer.Length,
                    ObjectUserID = objectUserID
                }
            });
        }

        internal PersistentRegion AllocRootObjectByType(Type type, string objectUserID)
        {
            ObjectPropertiesInfoMapper objectPropertiesInfoMapper = RegisterNewObjectPropertiesInfoMapper(type);

            var objectLength = (uint)objectPropertiesInfoMapper.GetTypeSize();
            PersistentRegion objectRegion = _allocator.Alloc(objectLength);
            var blockId = BitConverter.GetBytes(objectRegion.BlockID);
            var regionIndex = objectRegion.RegionIndex;
            UInt16 offsetInnerRegion = 0;
            var offsetInnerRegionBytes = BitConverter.GetBytes(offsetInnerRegion); // Always zero
            var objectSizeBytes = BitConverter.GetBytes(objectLength);

            // 12 = metadata size + object user id length + \0 string byte
            var buffer = new byte[12 + objectUserID.Length + 1];
            var idBytes = Encoding.UTF8.GetBytes(objectUserID);
            buffer[0] = (byte)MetadataType.Object;
            Array.Copy(sourceArray: blockId, sourceIndex: 0, destinationArray: buffer, destinationIndex: 1, length: blockId.Length);
            buffer[5] = regionIndex;
            Array.Copy(sourceArray: offsetInnerRegionBytes, sourceIndex: 0, destinationArray: buffer, destinationIndex: 6, length: offsetInnerRegionBytes.Length);
            Array.Copy(sourceArray: objectSizeBytes, sourceIndex: 0, destinationArray: buffer, destinationIndex: 8, length: objectSizeBytes.Length);
            Array.Copy(sourceArray: idBytes, sourceIndex: 0, destinationArray: buffer, destinationIndex: 12, length: idBytes.Length);
            // But \0 character in end of string
            Array.Copy(sourceArray: new byte[] { (byte)'\0' }, sourceIndex: 0, destinationArray: buffer, destinationIndex: 12 + idBytes.Length, length: 1);

            _metadataRegion.Write(buffer, _nextStructureOffset);
            _nextStructureOffset += buffer.Length;

            // Add to cache
            _metaDataStructure.Add(objectUserID, new MetaDataStructure
            {
                MetadataType = MetadataType.Object,
                ObjectMetaDataStructure = new ObjectMetaDataStructure
                {
                    BlockID = objectRegion.BlockID,
                    RegionIndex = regionIndex,
                    OffsetInnerRegion = offsetInnerRegion,
                    ObjectSize = objectLength,
                    ObjectUserID = objectUserID
                }
            });

            return objectRegion;
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
                    byte[] bytesValor = ObterBytesValor(valorPropriedade);
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

        static byte[] ObterBytesValor(object valor)
        {
            Type tipo = valor.GetType();

            if (tipo == typeof(bool))
            {
                return BitConverter.GetBytes((bool)valor);
            }
            else if (tipo == typeof(char))
            {
                return BitConverter.GetBytes((char)valor);
            }
            else if (tipo == typeof(sbyte))
            {
                return new byte[] { (byte)((sbyte)valor) };
            }
            else if (tipo == typeof(byte))
            {
                return new byte[] { (byte)valor };
            }
            else if (tipo == typeof(short))
            {
                return BitConverter.GetBytes((short)valor);
            }
            else if (tipo == typeof(ushort))
            {
                return BitConverter.GetBytes((ushort)valor);
            }
            else if (tipo == typeof(int))
            {
                return BitConverter.GetBytes((int)valor);
            }
            else if (tipo == typeof(uint))
            {
                return BitConverter.GetBytes((uint)valor);
            }
            else if (tipo == typeof(long))
            {
                return BitConverter.GetBytes((long)valor);
            }
            else if (tipo == typeof(ulong))
            {
                return BitConverter.GetBytes((ulong)valor);
            }
            else if (tipo == typeof(float))
            {
                return BitConverter.GetBytes((float)valor);
            }
            else if (tipo == typeof(double))
            {
                return BitConverter.GetBytes((double)valor);
            }
            else if (tipo == typeof(decimal))
            {
                int[] bits = decimal.GetBits((decimal)valor);
                byte[] bytes = new byte[bits.Length * sizeof(int)];
                Buffer.BlockCopy(bits, 0, bytes, 0, bytes.Length);
                return bytes;
            }
            else if (tipo == typeof(string))
            {
                return Encoding.UTF8.GetBytes((string)valor);
            }

            throw new ArgumentException("Tipo de valor não suportado.");
        }

        static byte[] ConcatenarBytes(byte[] bytes1, byte[] bytes2)
        {
            byte[] resultado = new byte[bytes1.Length + bytes2.Length];
            Buffer.BlockCopy(bytes1, 0, resultado, 0, bytes1.Length);
            Buffer.BlockCopy(bytes2, 0, resultado, bytes1.Length, bytes2.Length);
            return resultado;
        }

        internal void UpdateProperty(PersistentRegion persistentRegion, Type targetType, PropertyInfo property, object value)
        {
            var mapper = _propertiesMapper[targetType];
            var propertyInternalOffset = mapper.GetPropertyOffset(property);
            if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(decimal))
            {
                persistentRegion.Write(ObterBytesValor(value), offset: propertyInternalOffset);
            }
            else
            {
                var objectPropertiesInfoMapper = new ObjectPropertiesInfoMapper(property.PropertyType);
                var objectLength = (uint)objectPropertiesInfoMapper.GetTypeSize();
                var region = _allocator.Alloc(objectLength);
                // TODO: Add transaction
                region.Write(ObterBytesValor(value), offset: 0);
                var blockIdBytes = ObterBytesValor(region.BlockID);
                var regionIndexBytes = ObterBytesValor(region.RegionIndex);
                persistentRegion.Write(
                    blockIdBytes
                    .Concat(regionIndexBytes)
                    .ToArray(),
                    offset: propertyInternalOffset);
            }
        }

        internal object GetPropertyValue(PersistentRegion persistentRegion, Type targetType, PropertyInfo property)
        {
            var mapper = _propertiesMapper[targetType];
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
                if (property.PropertyType == typeof(string))
                {
                    var blockID = persistentRegion.Read(sizeof(uint), offset: propertyInternalOffset);
                    propertyInternalOffset += sizeof(uint);
                    var regionIndex = persistentRegion.Read(sizeof(byte), offset: propertyInternalOffset)[0];

                    var strRegion = _allocator.GetRegion(BitConverter.ToUInt32(blockID), regionIndex);

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
            }
            // TODO: implement to non-primitive types
            throw new NotImplementedException();
        }

        internal bool ObjectExists(string objectID)
        {
            return _metaDataStructure.ContainsKey(objectID);
        }

        internal PersistentRegion GetRegionByObjectUserID(string objectUserID)
        {
            var metaDataStructure = _metaDataStructure[objectUserID];
            if (metaDataStructure.MetadataType == MetadataType.Object)
            {
                if (metaDataStructure.ObjectMetaDataStructure is null) 
                    throw new ApplicationException($"{nameof(metaDataStructure.ObjectMetaDataStructure)} cannot be null");

                var blockID = metaDataStructure.ObjectMetaDataStructure.BlockID;
                var regionIndex = metaDataStructure.ObjectMetaDataStructure.RegionIndex;

                return _allocator.GetRegion(blockID, regionIndex);
            }

            throw new NotImplementedException();
        }
    }
}
