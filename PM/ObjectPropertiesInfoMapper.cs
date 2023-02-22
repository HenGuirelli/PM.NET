using PM.Core;
using PM.PmContent;
using System.Reflection;

namespace PM
{
    public class ObjectPropertiesInfoMapper
    {
        private readonly Dictionary<PropertyInfo, int> _propetyOffsetByPropertyInfo = new();
        private readonly int _totalSize;
        public PmHeader Header { get; }

        public ObjectPropertiesInfoMapper(Type type, PmHeader header)
        {
            Header = header;
            foreach (var property in type.GetProperties())
            {
                var pmemType = SupportedTypesTable.Instance.GetPmType(property.PropertyType);
                _propetyOffsetByPropertyInfo[property] = _totalSize;
                _totalSize += pmemType.SizeBytes;
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
