using System.Reflection;

namespace PM.AutomaticManager
{
    public class ObjectPropertiesInfoMapper
    {
        // Dictionary of property and its offset
        private readonly Dictionary<PropertyInfo, int> _offsetByPRoperty = new();
        private readonly Dictionary<Type, uint> _typeSizeByType = new();
        public Type ObjectType { get; }

        public ObjectPropertiesInfoMapper(Type type)
        {
            ObjectType = type;
            PropertyInfo[] properties = type.GetProperties();

            var offset = 0;
            foreach (var property in properties)
            {
                _offsetByPRoperty[property] = offset;
                switch (Type.GetTypeCode(property.PropertyType))
                {
                    case TypeCode.Boolean:
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                        offset += 1;
                        break;
                    case TypeCode.Char:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                        offset += 2;
                        break;
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Single:
                        offset += 4;
                        break;
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Double:
                        offset += 8;
                        break;
                    case TypeCode.Decimal:
                        offset += 16;
                        break;
                    default: // Complex type, we deal with it as a UInt32 (blockID) + byte (region index)
                        offset += sizeof(UInt32) + sizeof(byte);
                        break;
                }
            }
            _typeSizeByType[type] = (uint)offset;
        }

        /// <summary>
        /// Return property internal offset.
        /// Example: If property is the first in the classe, offset will be always 0.
        /// </summary>
        /// <param name="property">PropertyInfo from class</param>
        /// <returns></returns>
        public int GetPropertyOffset(PropertyInfo property)
        {
            return _offsetByPRoperty[property];
        }

        public uint GetTypeSize()
        {
            return _typeSizeByType[ObjectType];
        }
    }
}
