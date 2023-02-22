namespace PM.Core
{
    public class SupportedTypesTable
    {
        private static SupportedTypesTable? _instance;
        public static SupportedTypesTable Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SupportedTypesTable();
                }
                return _instance;
            }
        }

        private readonly Dictionary<int, PmType> _pmemTypetypesByInt = new();
        private readonly Dictionary<Type, PmType> _pmemTypeByTypes = new();
        private readonly static PmType ComplexObjectPmemType = new PmType(typeof(object), 14, 64); // 64 bits pointer

        private int _minID;
        private int _maxID;
        private SupportedTypesTable()
        {
            AddType(0, typeof(int), 32);
            AddType(1, typeof(byte), 8);
            AddType(2, typeof(sbyte), 8);
            AddType(3, typeof(short), 16);
            AddType(4, typeof(ushort), 16);
            AddType(5, typeof(uint), 32);
            AddType(6, typeof(long), 64);
            AddType(7, typeof(ulong), 64);
            AddType(8, typeof(float), 32);
            AddType(9, typeof(double), 64);
            AddType(10, typeof(decimal), 128);
            AddType(11, typeof(char), 16);
            AddType(12, typeof(bool), 8);
            AddType(13, typeof(string), 64);
        }

        public bool IsValidID(int id)
        {
            return id >= _minID && id <= _maxID;
        }

        private void AddType(int id, Type type, int size)
        {
            var pmemType = new PmType(type, id, size);
            _pmemTypetypesByInt.Add(id, pmemType);
            _pmemTypeByTypes.Add(type, pmemType);

            if (id < _minID) _minID = id;
            if (id > _maxID) _maxID = id;
        }

        public PmType GetPmemType(int id)
        {
            if (_pmemTypetypesByInt.TryGetValue(id, out var pmemType))
            {
                return pmemType;
            }
            throw new Exception($"type {id} not allowed");
        }

        public PmType GetPmType(Type type)
        {
            if (_pmemTypeByTypes.TryGetValue(type, out var pmemType))
            {
                return pmemType;
            }
            return ComplexObjectPmemType;
        }
    }
}
