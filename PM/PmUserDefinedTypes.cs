using PM.Core;
using System.Reflection;

namespace PM
{
    public class PmUserDefinedTypes
    {
        public FileBasedStream PmMemoryMappedFile { get; }
        private readonly PmCSharpDefinedTypes _pmCSharpDefined;
        private readonly ObjectPropertiesInfoMapper _objectPropertiesSizeMapper;

        public PmUserDefinedTypes(FileBasedStream pm, ObjectPropertiesInfoMapper objectPropertiesInfoMapper)
        {
            _pmCSharpDefined = new PmCSharpDefinedTypes(pm);
            PmMemoryMappedFile = pm;
            _objectPropertiesSizeMapper = objectPropertiesInfoMapper;
        }

        public void UpdateProperty(PropertyInfo prop, float newValue)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            _pmCSharpDefined.WriteFloat(newValue, offset);
        }

        public void UpdateProperty(PropertyInfo prop, decimal newValue)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            _pmCSharpDefined.WriteDecimal(newValue, offset);
        }

        public void UpdateProperty(PropertyInfo prop, int newValue)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            _pmCSharpDefined.WriteInt(newValue, offset);
        }
        
        public void UpdateProperty(PropertyInfo prop, byte newValue)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            _pmCSharpDefined.WriteByte(newValue, offset);
        }

        public void UpdateProperty(PropertyInfo prop, sbyte newValue)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            _pmCSharpDefined.WriteSByte(newValue, offset);
        }

        public void UpdateProperty(PropertyInfo prop, short newValue)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            _pmCSharpDefined.WriteShort(newValue, offset);
        }

        public void UpdateProperty(PropertyInfo prop, ushort newValue)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            _pmCSharpDefined.WriteUShort(newValue, offset);
        }
        
        public void UpdateProperty(PropertyInfo prop, uint newValue)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            _pmCSharpDefined.WriteUInt(newValue, offset);
        }

        public void UpdateProperty(PropertyInfo prop, long newValue)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            _pmCSharpDefined.WriteLong(newValue, offset);
        }

        public void UpdateProperty(PropertyInfo prop, ulong newValue)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            _pmCSharpDefined.WriteULong(newValue, offset);
        }

        public void UpdateProperty(PropertyInfo prop, double newValue)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            _pmCSharpDefined.WriteDouble(newValue, offset);
        }

        public void UpdateProperty(PropertyInfo prop, char newValue)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            _pmCSharpDefined.WriteChar(newValue, offset);
        }

        public void UpdateProperty(PropertyInfo prop, bool newValue)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            _pmCSharpDefined.WriteBool(newValue, offset);
        }
        
        public int GetIntPropertValue(PropertyInfo prop)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            return _pmCSharpDefined.ReadInt(offset);
        } 
        
        public float GetFloatPropertValue(PropertyInfo prop)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            return _pmCSharpDefined.ReadFloat(offset);
        }

        public decimal GetDecimalPropertValue(PropertyInfo prop)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            return _pmCSharpDefined.ReadDecimal(offset);
        }

        public byte GetBytePropertValue(PropertyInfo prop)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            return _pmCSharpDefined.ReadByte(offset);
        }

        public sbyte GetSBytePropertValue(PropertyInfo prop)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            return _pmCSharpDefined.ReadSByte(offset);
        }

        public short GetShortPropertValue(PropertyInfo prop)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            return _pmCSharpDefined.ReadShort(offset);
        }

        public ushort GetUShortPropertValue(PropertyInfo prop)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            return _pmCSharpDefined.ReadUShort(offset);
        }

        public uint GetUIntPropertValue(PropertyInfo prop)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            return _pmCSharpDefined.ReadUInt(offset);
        }

        public long GetLongPropertValue(PropertyInfo prop)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            return _pmCSharpDefined.ReadLong(offset);
        }

        public ulong GetULongPropertValue(PropertyInfo prop)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            return _pmCSharpDefined.ReadULong(offset);
        }

        public double GetDoublePropertValue(PropertyInfo prop)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            return _pmCSharpDefined.ReadDouble(offset);
        }

        public char GetCharPropertValue(PropertyInfo prop)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            return _pmCSharpDefined.ReadChar(offset);
        }

        public bool GetBoolPropertValue(PropertyInfo prop)
        {
            var offset = _objectPropertiesSizeMapper.GetOffSet(prop);
            return _pmCSharpDefined.ReadBool(offset);
        }

        public void Lock()
        {
            //_pmCSharpDefined.Lock();
        }

        public void Release()
        {
            //_pmCSharpDefined.Release();
        }
    }
}
