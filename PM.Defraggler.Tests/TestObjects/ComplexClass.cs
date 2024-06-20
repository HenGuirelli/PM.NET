namespace PM.Defraggler.Tests.TestObjects
{
    public class ComplexClass
    {
        public virtual PocoClass PocoObject { get; set; }
        public virtual ComplexClass SelfReferenceObject { get; set; }


        public virtual int IntVal1 { get; set; }
        public virtual int IntVal2 { get; set; }
    }
}
