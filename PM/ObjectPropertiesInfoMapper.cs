using PM.Core;
using PM.PmContent;
using System.Reflection;

namespace PM
{
    public class ObjectPropertiesInfoMapper
    {
        private static readonly Dictionary<Type, ObjectPropertiesInfoMapper> _propertyInfoByType = new();

        private readonly Dictionary<PropertyInfo, int> _propetyOffsetByPropertyInfo = new();
        private readonly int _totalSize;
        public PmHeader Header { get; }

        public ObjectPropertiesInfoMapper(Type type, PmHeader header)
        {
            if (_propertyInfoByType.TryGetValue(type, out var propInfo))
            {
                Header = propInfo.Header;
                _propetyOffsetByPropertyInfo = propInfo._propetyOffsetByPropertyInfo;
                _totalSize = propInfo._totalSize;
            }
            else
            {
                Header = header;
                foreach (var property in type.GetProperties())
                {
                    var pmemType = SupportedTypesTable.Instance.GetPmType(property.PropertyType);
                    _propetyOffsetByPropertyInfo[property] = _totalSize;
                    _totalSize += pmemType.SizeBytes;
                }
                _propertyInfoByType[type] = this;
            }
        }

        public int GetPropetyID(PropertyInfo property)
        {
            var pmemType = SupportedTypesTable.Instance.GetPmType(property.PropertyType);
            return pmemType.ID;
        }

        public int GetOffSet(PropertyInfo property)
        {
            var propOffset = _propetyOffsetByPropertyInfo[property];
            return propOffset + Header.HeaderSize;
        }
    }
}
