using FileFormatExplain;
using PM.AutomaticManager.Configs;
using PM.AutomaticManager.Tests.TestObjects;
using PM.Tests.Common;
using Xunit.Abstractions;

namespace PM.AutomaticManager.Tests
{
    public class PersistentFactoryTests : UnitTest
    {
        private readonly ITestOutputHelper _output;

        public PersistentFactoryTests(ITestOutputHelper output)
            : base(output)
        {
            _output = output;
        }

        [Fact]
        public void OnCreateClassProxy_ShouldIntercept()
        {
            PmGlobalConfiguration.PmTarget = Core.PmTargets.TraditionalMemoryMappedFile;

            var factory = new PersistentFactory();
            var proxyObj = factory.CreateRootObject<PocoClass>("My object id");

            proxyObj.IntVal1 = int.MaxValue;
            Assert.Equal(int.MaxValue, proxyObj.IntVal1);

            var decoded = PMemoryDecoder.DecodeHex(factory.Allocator.ReadOriginalFile(), dump: false);
            _output.WriteLine(decoded);

            proxyObj.IntVal1 = int.MinValue;
            Assert.Equal(int.MinValue, proxyObj.IntVal1);

            decoded = PMemoryDecoder.DecodeHex(factory.Allocator.ReadOriginalFile(), dump: false);
            _output.WriteLine(decoded);

            var secondObj = factory.CreateRootObject<PocoClass>("My second object");
            Assert.Equal(0, secondObj.IntVal1);
            secondObj.IntVal2 = int.MaxValue;
            Assert.Equal(int.MaxValue, secondObj.IntVal2);

            decoded = PMemoryDecoder.DecodeHex(factory.Allocator.ReadOriginalFile(), dump: false);
            _output.WriteLine(decoded);
        }

        [Fact]
        public void OnInterceptAllPrimitiveTypes_ShouldSaveAndGetCorrectValue()
        {
#if DEBUG
            PersistentFactory.Purge();
#endif

            PmGlobalConfiguration.PmTarget = Core.PmTargets.TraditionalMemoryMappedFile;

            var factory = new PersistentFactory();
            var proxyObj = factory.CreateRootObject<PocoClass>(nameof(OnInterceptAllPrimitiveTypes_ShouldSaveAndGetCorrectValue));

            proxyObj.IntVal1 = int.MaxValue;
            proxyObj.IntVal2 = int.MinValue;
            Assert.Equal(int.MaxValue, proxyObj.IntVal1);
            Assert.Equal(int.MinValue, proxyObj.IntVal2);

            proxyObj.LongVal1 = long.MaxValue;
            proxyObj.LongVal2 = long.MinValue;
            Assert.Equal(long.MaxValue, proxyObj.LongVal1);
            Assert.Equal(long.MinValue, proxyObj.LongVal2);

            proxyObj.ShortVal1 = short.MaxValue;
            proxyObj.ShortVal2 = short.MinValue;
            Assert.Equal(short.MaxValue, proxyObj.ShortVal1);
            Assert.Equal(short.MinValue, proxyObj.ShortVal2);

            proxyObj.ByteVal1 = byte.MaxValue;
            proxyObj.ByteVal2 = byte.MinValue;
            Assert.Equal(byte.MaxValue, proxyObj.ByteVal1);
            Assert.Equal(byte.MinValue, proxyObj.ByteVal2);

            proxyObj.DoubleVal1 = double.MaxValue;
            proxyObj.DoubleVal2 = double.MinValue;
            Assert.Equal(double.MaxValue, proxyObj.DoubleVal1);
            Assert.Equal(double.MinValue, proxyObj.DoubleVal2);

            proxyObj.FloatVal1 = float.MaxValue;
            proxyObj.FloatVal2 = float.MinValue;
            Assert.Equal(float.MaxValue, proxyObj.FloatVal1);
            Assert.Equal(float.MinValue, proxyObj.FloatVal2);

            proxyObj.DecimalVal1 = decimal.MaxValue;
            proxyObj.DecimalVal2 = decimal.MinValue;
            Assert.Equal(decimal.MaxValue, proxyObj.DecimalVal1);
            Assert.Equal(decimal.MinValue, proxyObj.DecimalVal2);

            Assert.Null(proxyObj.StringVal1);
            proxyObj.StringVal1 = "Hello";
            proxyObj.StringVal2 = "World";
            Assert.Equal("Hello", proxyObj.StringVal1);
            Assert.Equal("World", proxyObj.StringVal2);

            proxyObj.CharVal1 = 'A';
            proxyObj.CharVal2 = 'Z';
            Assert.Equal('A', proxyObj.CharVal1);
            Assert.Equal('Z', proxyObj.CharVal2);

            proxyObj.BoolVal1 = true;
            proxyObj.BoolVal2 = false;
            Assert.True(proxyObj.BoolVal1);
            Assert.False(proxyObj.BoolVal2);
        }

        [Fact]
        public void OnInterceptComplexClass_WithNewObjects_ShouldSaveAndGetCorrectValue()
        {
#if DEBUG
            PersistentFactory.Purge();
#endif

            PmGlobalConfiguration.PmTarget = Core.PmTargets.TraditionalMemoryMappedFile;

            var factory = new PersistentFactory();
            var proxyObj = factory.CreateRootObject<ComplexClass>(nameof(OnInterceptComplexClass_WithNewObjects_ShouldSaveAndGetCorrectValue));

            Assert.Null(proxyObj.SelfReferenceObject);

            // We Will do 2 principal test:
            // 1. Post inner object update
            //      * update inner object AFTER include him in root object
            // 2. Pre inner object update
            //      * update inner object BEFORE include him in root object,
            //      * in this case PM.NET should add proxy and serialize all
            //      * properties however be the depth in reference tree

            // =========== 1. Post Inner object updatE ===========
            proxyObj.PocoObject = new PocoClass();
            Assert.NotNull(proxyObj.PocoObject);

            // Proxy should be createad and intercept
            proxyObj.PocoObject.IntVal1 = int.MaxValue;
            Assert.Equal(int.MaxValue, proxyObj.PocoObject.IntVal1);

            // Self reference
            proxyObj.SelfReferenceObject = new ComplexClass();
            proxyObj.SelfReferenceObject.IntVal1 = int.MaxValue;
            proxyObj.SelfReferenceObject.IntVal2 = int.MinValue;
            Assert.Equal(int.MaxValue, proxyObj.SelfReferenceObject.IntVal1);
            Assert.Equal(int.MinValue, proxyObj.SelfReferenceObject.IntVal2);

            var decoded = PMemoryDecoder.DecodeHex(factory.Allocator.ReadOriginalFile(), dump: false);
            _output.WriteLine(decoded);

            // Pre Inner object update
            // see test OnInterceptComplexClass_WithObjectWithValues_ShouldSaveAndGetCorrectValue()
        }

        [Fact]
        public void OnInterceptComplexClass_WithObjectWithValues_ShouldSaveAndGetCorrectValue()
        {
#if DEBUG
            PersistentFactory.Purge();
#endif

            PmGlobalConfiguration.PmTarget = Core.PmTargets.TraditionalMemoryMappedFile;

            var factory = new PersistentFactory();
            var proxyObj = factory.CreateRootObject<ComplexClass>("All primitives types");

            Assert.Null(proxyObj.SelfReferenceObject);

            // We Will do 2 principal test:
            // 1. Post inner object update
            //      * update inner object AFTER include him in root object
            // 2. Pre inner object update
            //      * update inner object BEFORE include him in root object,
            //      * in this case PM.NET should add proxy and serialize all
            //      * properties however be the depth in reference tree

            // =========== 2. Pre inner object update ===========
            proxyObj.PocoObject = new PocoClass
            {
                IntVal1 = int.MaxValue,
                BoolVal1 = true,
            };
            Assert.NotNull(proxyObj.PocoObject);
            Assert.Equal(int.MaxValue, proxyObj.PocoObject.IntVal1);
            Assert.True(proxyObj.PocoObject.BoolVal1);

            // Self reference
            proxyObj.SelfReferenceObject = new ComplexClass
            {
                IntVal1 = int.MaxValue,
                IntVal2 = int.MinValue
            };
            Assert.Equal(int.MaxValue, proxyObj.SelfReferenceObject.IntVal1);
            Assert.Equal(int.MinValue, proxyObj.SelfReferenceObject.IntVal2);

            var decoded = PMemoryDecoder.DecodeHex(factory.Allocator.ReadOriginalFile(), dump: false);
            _output.WriteLine(decoded);
        }

        [Fact]
        public void OnInterceptComplexClass_HellReferenceTest()
        {
#if DEBUG
            PersistentFactory.Purge();
#endif

            PmGlobalConfiguration.PmTarget = Core.PmTargets.TraditionalMemoryMappedFile;

            var factory = new PersistentFactory();
            var proxyObj = factory.CreateRootObject<RootClass>(nameof(OnInterceptComplexClass_HellReferenceTest));

            // ========== Level 1 ==========

            Assert.Null(proxyObj.InnerObject1);
            Assert.Null(proxyObj.InnerObject2);
            proxyObj.InnerObject1 = new InnerClass1 { Val = 10 };
            proxyObj.InnerObject2 = new InnerClass1 { Val = 11 };
            Assert.Equal(10, proxyObj.InnerObject1.Val);
            Assert.Equal(11, proxyObj.InnerObject2.Val);

            // ========== Level 2 ==========

            proxyObj.InnerObject1.InnerObject1 = new InnerClass2 { Val = 20 };
            proxyObj.InnerObject1.InnerObject2 = new InnerClass2 { Val = 21 };
            proxyObj.InnerObject2.InnerObject1 = new InnerClass2 { Val = 22 };
            proxyObj.InnerObject2.InnerObject2 = new InnerClass2 { Val = 23 };
            Assert.Equal(20, proxyObj.InnerObject1.InnerObject1.Val);
            Assert.Equal(21, proxyObj.InnerObject1.InnerObject2.Val);
            Assert.Equal(22, proxyObj.InnerObject2.InnerObject1.Val);
            Assert.Equal(23, proxyObj.InnerObject2.InnerObject2.Val);
            // Older values still working?
            Assert.Equal(10, proxyObj.InnerObject1.Val);
            Assert.Equal(11, proxyObj.InnerObject2.Val);

            // ========== Level 3 ==========

            proxyObj.InnerObject1.InnerObject1.InnerObject1 = new InnerClass3 { Val = 20 };
            proxyObj.InnerObject1.InnerObject1.InnerObject2 = new InnerClass3 { Val = 21 };
            proxyObj.InnerObject1.InnerObject2.InnerObject1 = new InnerClass3 { Val = 22 };
            proxyObj.InnerObject1.InnerObject2.InnerObject2 = new InnerClass3 { Val = 23 };
            proxyObj.InnerObject2.InnerObject1.InnerObject1 = new InnerClass3 { Val = 24 };
            proxyObj.InnerObject2.InnerObject1.InnerObject2 = new InnerClass3 { Val = 25 };
            proxyObj.InnerObject2.InnerObject2.InnerObject1 = new InnerClass3 { Val = 26 };
            proxyObj.InnerObject2.InnerObject2.InnerObject2 = new InnerClass3 { Val = 27 };
            Assert.Equal(20, proxyObj.InnerObject1.InnerObject1.InnerObject1.Val);
            Assert.Equal(21, proxyObj.InnerObject1.InnerObject1.InnerObject2.Val);
            Assert.Equal(22, proxyObj.InnerObject1.InnerObject2.InnerObject1.Val);
            Assert.Equal(23, proxyObj.InnerObject1.InnerObject2.InnerObject2.Val);
            Assert.Equal(24, proxyObj.InnerObject2.InnerObject1.InnerObject1.Val);
            Assert.Equal(25, proxyObj.InnerObject2.InnerObject1.InnerObject2.Val);
            Assert.Equal(26, proxyObj.InnerObject2.InnerObject2.InnerObject1.Val);
            Assert.Equal(27, proxyObj.InnerObject2.InnerObject2.InnerObject2.Val);
            // Older values still working?
            Assert.Equal(20, proxyObj.InnerObject1.InnerObject1.Val);
            Assert.Equal(21, proxyObj.InnerObject1.InnerObject2.Val);
            Assert.Equal(22, proxyObj.InnerObject2.InnerObject1.Val);
            Assert.Equal(23, proxyObj.InnerObject2.InnerObject2.Val);
            Assert.Equal(10, proxyObj.InnerObject1.Val);
            Assert.Equal(11, proxyObj.InnerObject2.Val);

            var decoded = PMemoryDecoder.DecodeHex(factory.Allocator.ReadOriginalFile(), dump: false);
            _output.WriteLine(decoded);
        }


        [Fact]
        public void OnInterceptComplexClass_CircularReference()
        {
#if DEBUG
            PersistentFactory.Purge();
#endif

            PmGlobalConfiguration.PmTarget = Core.PmTargets.TraditionalMemoryMappedFile;

            var factory = new PersistentFactory();
            var proxyObj = factory.CreateRootObject<ComplexClass>(nameof(OnInterceptComplexClass_CircularReference));

            proxyObj.IntVal1 = int.MinValue;
            proxyObj.SelfReferenceObject = proxyObj;

            Assert.Equal(int.MinValue, proxyObj.IntVal1);
            Assert.Equal(int.MinValue, proxyObj.SelfReferenceObject.IntVal1);
            Assert.Equal(int.MinValue, proxyObj.SelfReferenceObject.SelfReferenceObject.IntVal1);
            Assert.Equal(int.MinValue, proxyObj.SelfReferenceObject.SelfReferenceObject.SelfReferenceObject.IntVal1);
            Assert.Equal(int.MinValue, proxyObj.SelfReferenceObject.SelfReferenceObject.SelfReferenceObject.SelfReferenceObject.IntVal1);
            Assert.Equal(int.MinValue, proxyObj.SelfReferenceObject.SelfReferenceObject.SelfReferenceObject.SelfReferenceObject.SelfReferenceObject.IntVal1);

            var decoded = PMemoryDecoder.DecodeHex(factory.Allocator.ReadOriginalFile(), dump: false);
            _output.WriteLine(decoded);
        }


        [Fact]
        public void OnInterceptComplexClass_ShouldRemoveReferences()
        {
#if DEBUG
            PersistentFactory.Purge();
#endif

            PmGlobalConfiguration.PmTarget = Core.PmTargets.TraditionalMemoryMappedFile;

            var factory = new PersistentFactory();
            var proxyObj = factory.CreateRootObject<ComplexClass>(nameof(OnInterceptComplexClass_CircularReference));

            proxyObj.SelfReferenceObject = new ComplexClass { IntVal1 = int.MaxValue };
            Assert.Equal(int.MaxValue, proxyObj.SelfReferenceObject.IntVal1);
            proxyObj.SelfReferenceObject = null!;

            Assert.Null(proxyObj.SelfReferenceObject);

            var decoded = PMemoryDecoder.DecodeHex(factory.Allocator.ReadOriginalFile(), dump: false);
            _output.WriteLine(decoded);
        }
    }
}
