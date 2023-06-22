using PM.Collections;

namespace PM.Transactions.Tests
{
    public class InnerDomainObject2
    {
    }

    public class InnerDomainObject
    {
        public virtual int PropInt { get; set; }
        public PmList<InnerDomainObject2> InnerDomainObject2s { get; set; } = new();
    }

    public class DomainObject
    {
        public virtual string? PropStr { get; set; }
        public virtual byte PropByte { get; set; }
        public virtual sbyte PropSByte { get; set; }
        public virtual short PropShort { get; set; }
        public virtual ushort PropUShort { get; set; }
        public virtual uint PropUInt { get; set; }
        public virtual int PropInt { get; set; }
        public virtual long PropLong { get; set; }
        public virtual ulong PropULong { get; set; }
        public virtual float PropFloat { get; set; }
        public virtual double PropDouble { get; set; }
        public virtual decimal PropDecimal { get; set; }
        public virtual char PropChar { get; set; }
        public virtual bool PropBool { get; set; }

        public PmList<InnerDomainObject> PmList { get; set; }
    }
}
