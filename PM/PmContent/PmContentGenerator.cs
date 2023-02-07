using PM.Core;

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

        public PmHeader CreateHeader()
        {
            var header = new PmHeader(_type);
            _pm.WriteInt(header.ClassHash, offset: header.ClassHashOffset);
            return header;
        }
    }
}
