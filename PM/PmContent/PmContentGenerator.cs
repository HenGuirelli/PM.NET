using PM.Common;

namespace PM.PmContent
{
    public class PmContentGenerator
    {
        private readonly PmCSharpDefinedTypes _pm;
        private readonly Type _type;

        public PmContentGenerator(PmCSharpDefinedTypes pm, Type type)
        {
            _pm = pm;
            _type = type;
        }

        public PmHeader CreateHeader(bool isRoot)
        {
            var header = new PmHeader(_type, isRoot);
            _pm.WriteInt(header.ClassHash, offset: header.ClassHashOffset);
            _pm.WriteBool(header.IsRootObject, offset: header.RootObjectOffset);
            return header;
        }
    }
}
