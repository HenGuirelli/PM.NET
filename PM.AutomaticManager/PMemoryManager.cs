using PM.AutomaticManager.Proxies;
using PM.Core.PMemory;
using PM.FileEngine;
using System;
using System.Reflection;
using System.Text;

namespace PM.AutomaticManager
{
    public class PMemoryManager
    {
        private PAllocator _allocator;
        private PersistentRegion _region;
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
                _region = allocator.Alloc(100_000);
            }
            else
            {
                _region = _allocator.FirstPersistentBlockLayout!.Regions[0];
                _nextStructureOffset = 0;
                while (true)
                {
                    var metadataType = _region.Read(count: 1, offset: _nextStructureOffset)[0]; // Read First metadataStructure
                    _nextStructureOffset += 1;
                    if (metadataType != 0) // Have value!!
                    {
                        if (metadataType == (byte)MetadataType.Object)
                        {
                            var blockId = BitConverter.ToUInt32(_region.Read(count: 4, offset: _nextStructureOffset));
                            _nextStructureOffset += sizeof(UInt32);
                            var regionIndex = _region.Read(count: 1, offset: _nextStructureOffset)[0];
                            _nextStructureOffset += sizeof(byte);
                            var offsetInnerRegion = BitConverter.ToUInt16(_region.Read(count: 2, offset: _nextStructureOffset));
                            _nextStructureOffset += sizeof(UInt16);
                            var objectSize = BitConverter.ToUInt32(_region.Read(count: 4, offset: _nextStructureOffset));
                            _nextStructureOffset += sizeof(UInt32);
                            var str = new StringBuilder();
                            byte @char = 0;
                            while ((@char = _region.Read(count: 1, offset: _nextStructureOffset)[0]) != 0)
                            {
                                str.Append((char)@char);
                            }

                            var objectUserName = str.ToString();
                            _metaDataStructure.Add(objectUserName,
                                new MetaDataStructure
                                {
                                    MetadataType = MetadataType.Object,
                                    ObjectMetaDataStructure = new ObjectMetaDataStructure
                                    {
                                        BlockID = blockId,
                                        RegionIndex = regionIndex,
                                        OffsetInnerRegion = offsetInnerRegion,
                                        ObjectSize = objectSize,
                                        ObjectUserName = objectUserName
                                    }
                                });
                        }
                    }
                }
            }
        }

        public void AddNewObject(string id, object obj)
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

            var buffer = new byte[12 + id.Length];
            var idBytes = Encoding.UTF8.GetBytes(id);
            buffer[0] = (byte)MetadataType.Object;
            Array.Copy(sourceArray: blockId, sourceIndex: 0, destinationArray: buffer, destinationIndex: 1, length: blockId.Length);
            buffer[5] = regionIndex;
            Array.Copy(sourceArray: offsetInnerRegionBytes, sourceIndex: 0, destinationArray: buffer, destinationIndex: 6, length: offsetInnerRegionBytes.Length);
            Array.Copy(sourceArray: objectSizeBytes, sourceIndex: 0, destinationArray: buffer, destinationIndex: 8, length: objectSizeBytes.Length);
            Array.Copy(sourceArray: idBytes, sourceIndex: 0, destinationArray: buffer, destinationIndex: 12, length: idBytes.Length);

            objectRegion.Write(objectBuffer);
            _region.Write(buffer, _nextStructureOffset);
            _nextStructureOffset += buffer.Length;

            // Add to cache
            _metaDataStructure.Add(id, new MetaDataStructure
            {
                MetadataType = MetadataType.Object,
                ObjectMetaDataStructure = new ObjectMetaDataStructure
                {
                    BlockID = objectRegion.BlockID,
                    RegionIndex = regionIndex,
                    OffsetInnerRegion = offsetInnerRegion,
                    ObjectSize = (uint)objectBuffer.Length,
                    ObjectUserName = id
                }
            });
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

            throw new ArgumentException("Tipo de valor não suportado.");
        }

        static byte[] ConcatenarBytes(byte[] bytes1, byte[] bytes2)
        {
            byte[] resultado = new byte[bytes1.Length + bytes2.Length];
            Buffer.BlockCopy(bytes1, 0, resultado, 0, bytes1.Length);
            Buffer.BlockCopy(bytes2, 0, resultado, bytes1.Length, bytes2.Length);
            return resultado;
        }

        internal void UpdateProperty(PropertyInfo method, object value)
        {
            _propertiesMapper[]
        }

        internal object GetPropertyValue(PropertyInfo method)
        {
            throw new NotImplementedException();
        }
    }
}
