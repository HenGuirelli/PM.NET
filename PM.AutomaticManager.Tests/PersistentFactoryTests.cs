using FileFormatExplain;
using PM.AutomaticManager.Configs;
using PM.Tests.Common;
using Xunit.Abstractions;

namespace PM.AutomaticManager.Tests
{
    public class ComplexClass
    {
        public virtual PocoClass PocoObject { get; set; }
        public virtual ComplexClass SelfReferenceObject { get; set; }


        public virtual int IntVal1 { get; set; }
        public virtual int IntVal2 { get; set; }
    }

    public class PocoClass
    {
        public virtual int IntVal1 { get; set; }
        public virtual int IntVal2 { get; set; }

        public virtual long LongVal1 { get; set; }
        public virtual long LongVal2 { get; set; }

        public virtual short ShortVal1 { get; set; }
        public virtual short ShortVal2 { get; set; }

        public virtual byte ByteVal1 { get; set; }
        public virtual byte ByteVal2 { get; set; }

        public virtual double DoubleVal1 { get; set; }
        public virtual double DoubleVal2 { get; set; }

        public virtual float FloatVal1 { get; set; }
        public virtual float FloatVal2 { get; set; }

        public virtual decimal DecimalVal1 { get; set; }
        public virtual decimal DecimalVal2 { get; set; }

        public virtual string StringVal1 { get; set; }
        public virtual string StringVal2 { get; set; }

        public virtual char CharVal1 { get; set; }
        public virtual char CharVal2 { get; set; }

        public virtual bool BoolVal1 { get; set; }
        public virtual bool BoolVal2 { get; set; }
    }


    public class PersistentFactoryTests : UnitTest
    {
        private ITestOutputHelper _output;

        public PersistentFactoryTests(ITestOutputHelper output)
            : base(output)
        {
            _output = output;
        }

        [Fact]
        public void OnCreateClassProxy()
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
        }

        [Fact]
        public void OnInterceptAllPrimitiveTypes_ShouldSaveAndGetCorrectValue()
        {
            PmGlobalConfiguration.PmTarget = Core.PmTargets.TraditionalMemoryMappedFile;

            var factory = new PersistentFactory();
            var proxyObj = factory.CreateRootObject<PocoClass>("All primitives types");

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
        public void OnInterceptComplexClass_ShouldSaveAndGetCorrectValue()
        {
            PmGlobalConfiguration.PmTarget = Core.PmTargets.TraditionalMemoryMappedFile;

            var factory = new PersistentFactory();
            var proxyObj = factory.CreateRootObject<ComplexClass>("All primitives types");

            Assert.Null(proxyObj.SelfReferenceObject);

            // Post Inner object update
            proxyObj.PocoObject = new PocoClass();
            Assert.NotNull(proxyObj.PocoObject);

            // Proxy should be createad and intercept
            proxyObj.PocoObject.IntVal1 = int.MaxValue;
            Assert.Equal(int.MaxValue, proxyObj.PocoObject.IntVal1);


            var decoded = PMemoryDecoder.DecodeHex(factory.Allocator.ReadOriginalFile(), dump: false);
            _output.WriteLine(decoded);


            // Pre Inner object update

            // Self reference
            proxyObj.SelfReferenceObject = new ComplexClass();
            proxyObj.SelfReferenceObject.IntVal1 = int.MaxValue;
            proxyObj.SelfReferenceObject.IntVal2 = int.MinValue;
            Assert.Equal(int.MaxValue, proxyObj.SelfReferenceObject.IntVal1);
            Assert.Equal(int.MinValue, proxyObj.SelfReferenceObject.IntVal2);

            //proxyObj.IntVal1 = int.MaxValue;
            //proxyObj.IntVal2 = int.MinValue;
            //Assert.Equal(int.MaxValue, proxyObj.IntVal1);
            //Assert.Equal(int.MinValue, proxyObj.IntVal2);

            //proxyObj.LongVal1 = long.MaxValue;
            //proxyObj.LongVal2 = long.MinValue;
            //Assert.Equal(long.MaxValue, proxyObj.LongVal1);
            //Assert.Equal(long.MinValue, proxyObj.LongVal2);

            //proxyObj.ShortVal1 = short.MaxValue;
            //proxyObj.ShortVal2 = short.MinValue;
            //Assert.Equal(short.MaxValue, proxyObj.ShortVal1);
            //Assert.Equal(short.MinValue, proxyObj.ShortVal2);

            //proxyObj.ByteVal1 = byte.MaxValue;
            //proxyObj.ByteVal2 = byte.MinValue;
            //Assert.Equal(byte.MaxValue, proxyObj.ByteVal1);
            //Assert.Equal(byte.MinValue, proxyObj.ByteVal2);

            //proxyObj.DoubleVal1 = double.MaxValue;
            //proxyObj.DoubleVal2 = double.MinValue;
            //Assert.Equal(double.MaxValue, proxyObj.DoubleVal1);
            //Assert.Equal(double.MinValue, proxyObj.DoubleVal2);

            //proxyObj.FloatVal1 = float.MaxValue;
            //proxyObj.FloatVal2 = float.MinValue;
            //Assert.Equal(float.MaxValue, proxyObj.FloatVal1);
            //Assert.Equal(float.MinValue, proxyObj.FloatVal2);

            //proxyObj.DecimalVal1 = decimal.MaxValue;
            //proxyObj.DecimalVal2 = decimal.MinValue;
            //Assert.Equal(decimal.MaxValue, proxyObj.DecimalVal1);
            //Assert.Equal(decimal.MinValue, proxyObj.DecimalVal2);

            //proxyObj.StringVal1 = "Hello";
            //proxyObj.StringVal2 = "World";
            //Assert.Equal("Hello", proxyObj.StringVal1);
            //Assert.Equal("World", proxyObj.StringVal2);

            //proxyObj.CharVal1 = 'A';
            //proxyObj.CharVal2 = 'Z';
            //Assert.Equal('A', proxyObj.CharVal1);
            //Assert.Equal('Z', proxyObj.CharVal2);

            //proxyObj.BoolVal1 = true;
            //proxyObj.BoolVal2 = false;
            //Assert.True(proxyObj.BoolVal1);
            //Assert.False(proxyObj.BoolVal2);
        }
    }
}
