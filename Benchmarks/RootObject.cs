namespace Benchmarks
{
    public class InnerClass
    {
        public virtual int MyProperty { get; set; }
    }

    public class RootObject
    {
        public virtual int IntVal { get; set; }

        public virtual long LongVal { get; set; }

        public virtual short ShortVal { get; set; }

        public virtual byte ByteVal { get; set; }

        public virtual double DoubleVal { get; set; }

        public virtual float FloatVal { get; set; }

        public virtual decimal DecimalVal { get; set; }

        public virtual char CharVal { get; set; }

        public virtual bool BoolVal { get; set; }

        public virtual string StringVal { get; set; }
        public virtual InnerClass InnerObject { get; set; }
    }
}
