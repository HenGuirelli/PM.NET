namespace PM.Tests.UserFinalTests
{
    public class ComplexClassInner2
    {
        public virtual int PropInt { get; set; }
        public virtual string PropStr { get; set; }
        public virtual ComplexClassInner2 PropSelfReferency { get; set; }
    }

    public class ComplexClassInner1
    {
        public virtual int PropInt { get; set; }
        public virtual string PropStr { get; set; }
        public virtual ComplexClassInner2 PropComplexClassInner2 { get; set; }
    }

    public class ComplexClassRoot
    {
        public virtual int PropInt { get; set; }
        public virtual string PropStr { get; set; }
        public virtual ComplexClassInner1 PropComplexClassInner1 { get; set; }
    }
}
