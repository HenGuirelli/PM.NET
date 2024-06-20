namespace PM.AutomaticManager.Tests.TestObjects
{
    // ====================================================
    // Test with many references. This test SHOULD be complex.
    // All classes must have at least 2 references, in graph this must be represent as a graph.
    // 
    // 
    //====================================================

    public class RootClass
    {
        public virtual InnerClass1 InnerObject1 { get; set; }
        public virtual InnerClass1 InnerObject2 { get; set; }

        public virtual int Val { get; set; }
    }

    public class InnerClass1
    {
        public virtual InnerClass2 InnerObject1 { get; set; }
        public virtual InnerClass2 InnerObject2 { get; set; }

        public virtual int Val { get; set; }
    }

    public class InnerClass2
    {
        public virtual InnerClass3 InnerObject1 { get; set; }
        public virtual InnerClass3 InnerObject2 { get; set; }

        public virtual int Val { get; set; }
    }

    public class InnerClass3
    {
        public virtual InnerClass1 InnerObject1 { get; set; }
        public virtual InnerClass1 InnerObject2 { get; set; }

        public virtual int Val { get; set; }
    }
}
