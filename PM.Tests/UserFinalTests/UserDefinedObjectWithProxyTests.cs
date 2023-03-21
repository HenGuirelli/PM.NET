using PM.Configs;
using PM.Tests.Common;
using System;
using Xunit;

namespace PM.Tests.UserFinalTests
{
    public class UserDefinedObjectWithProxyTests : UnitTest
    {
        private static readonly Random _random = new();

        [Fact]
        public void ExampleWithPrimitives()
        {
            IPersistentFactory persistentFactory = new PersistentFactory();
            var obj = persistentFactory.CreateRootObject<ComplexClassWithPrimitiveProperties>(CreateFilePath(nameof(ExampleWithPrimitives)));

            obj.Prop1 = int.MaxValue;
            obj.Prop2 = float.MaxValue;
            obj.Prop3 = decimal.MaxValue;
            obj.Prop4 = byte.MaxValue;
            obj.Prop5 = sbyte.MaxValue;
            obj.Prop6 = short.MaxValue;
            obj.Prop7 = ushort.MaxValue;
            obj.Prop8 = uint.MaxValue;
            obj.Prop9 = int.MaxValue;
            obj.Prop10 = ulong.MaxValue;
            obj.Prop11 = long.MaxValue;
            obj.Prop12 = char.MaxValue;
            obj.Prop13 = true;

            Assert.Equal(int.MaxValue, obj.Prop1);
            Assert.Equal(float.MaxValue, obj.Prop2);
            Assert.Equal(decimal.MaxValue, obj.Prop3);
            Assert.Equal(byte.MaxValue, obj.Prop4);
            Assert.Equal(sbyte.MaxValue, obj.Prop5);
            Assert.Equal(short.MaxValue, obj.Prop6);
            Assert.Equal(ushort.MaxValue, obj.Prop7);
            Assert.Equal(uint.MaxValue, obj.Prop8);
            Assert.Equal(int.MaxValue, obj.Prop9);
            Assert.Equal(ulong.MaxValue, obj.Prop10);
            Assert.Equal(long.MaxValue, obj.Prop11);
            Assert.Equal(char.MaxValue, obj.Prop12);
            Assert.True(obj.Prop13);
        }

        [Fact]
        public void InterchangedPointers()
        {
            IPersistentFactory persistentFactory = new PersistentFactory();
            var obj = persistentFactory.CreateRootObject<ComplexClassRoot>(
                CreateFilePath(nameof(InterchangedPointers)));

            Console.WriteLine(obj.PropComplexClassInner1);

            obj.PropComplexClassInner1 = new ComplexClassInner1();
            obj.PropComplexClassInner1.PropStr = "Hello World from ComplexClassInner1!";

            var obj2 = persistentFactory.CreateRootObject<ComplexClassRoot>(
                CreateFilePath(nameof(InterchangedPointers) + "2"));

            obj2.PropComplexClassInner1 = obj.PropComplexClassInner1;

            Assert.Equal(obj.PropComplexClassInner1, obj2.PropComplexClassInner1);

        }

        [Fact]
        public void ExampleFullObject()
        {
            IPersistentFactory persistentFactory = new PersistentFactory();
            var obj = persistentFactory.CreateRootObject<ComplexClassRoot>(CreateFilePath(nameof(ExampleFullObject)));

            obj.PropStr = "Hello World!";
            obj.PropInt = int.MinValue;

            Assert.Equal("Hello World!", obj.PropStr);
            Assert.Equal(int.MinValue, obj.PropInt);
            Assert.Null(obj.PropComplexClassInner1);

            obj.PropComplexClassInner1 = new ComplexClassInner1();
            obj.PropComplexClassInner1.PropStr = "Hello World from ComplexClassInner1!";
            obj.PropComplexClassInner1.PropInt = int.MaxValue;
            Assert.NotNull(obj.PropComplexClassInner1);
            Assert.Equal("Hello World from ComplexClassInner1!", obj.PropComplexClassInner1.PropStr);
            Assert.Equal(int.MaxValue, obj.PropComplexClassInner1.PropInt);
            Assert.Null(obj.PropComplexClassInner1.PropComplexClassInner2);


            obj.PropComplexClassInner1.PropComplexClassInner2 = new ComplexClassInner2();
            obj.PropComplexClassInner1.PropComplexClassInner2.PropStr = "Hello World from ComplexClassInner2!";
            var randomInt = _random.Next();
            obj.PropComplexClassInner1.PropComplexClassInner2.PropInt = randomInt;
            Assert.NotNull(obj.PropComplexClassInner1.PropComplexClassInner2);
            Assert.Equal("Hello World from ComplexClassInner2!", obj.PropComplexClassInner1.PropComplexClassInner2.PropStr);
            Assert.Equal(randomInt, obj.PropComplexClassInner1.PropComplexClassInner2.PropInt);
            Assert.Null(obj.PropComplexClassInner1.PropComplexClassInner2.PropSelfReferency);


            obj.PropComplexClassInner1.PropComplexClassInner2.PropSelfReferency = new ComplexClassInner2();
            obj.PropComplexClassInner1.PropComplexClassInner2.PropSelfReferency.PropStr = "Hello World from self reference!";
            var newRandomInt = _random.Next();
            obj.PropComplexClassInner1.PropComplexClassInner2.PropSelfReferency.PropInt = newRandomInt;
            Assert.NotNull(obj.PropComplexClassInner1.PropComplexClassInner2.PropSelfReferency);
            Assert.Equal("Hello World from self reference!", obj.PropComplexClassInner1.PropComplexClassInner2.PropSelfReferency.PropStr);
            Assert.Equal(newRandomInt, obj.PropComplexClassInner1.PropComplexClassInner2.PropSelfReferency.PropInt);
        }

        [Fact]
        public void ExampleFullObjectSetInAPersistentObject()
        {
            IPersistentFactory persistentFactory = new PersistentFactory();
            var obj = persistentFactory.CreateRootObject<ComplexClassRoot>(CreateFilePath(nameof(ExampleFullObjectSetInAPersistentObject)));

            var inMemoryObject = new ComplexClassInner1
            {
                PropInt = int.MaxValue,
                PropStr = "InMemoryObject to PM 1",
                PropComplexClassInner2 = new ComplexClassInner2
                {
                    PropInt = int.MinValue,
                    PropStr = "InMemoryObject to PM 2",
                }
            };

            // At this point, the inMemoryObject go to persistent memory
            obj.PropComplexClassInner1 = inMemoryObject;

            Assert.Equal("InMemoryObject to PM 1", obj.PropComplexClassInner1.PropStr);
            Assert.Equal(int.MaxValue, obj.PropComplexClassInner1.PropInt);

            Assert.Equal("InMemoryObject to PM 2", obj.PropComplexClassInner1.PropComplexClassInner2.PropStr);
            Assert.Equal(int.MinValue, obj.PropComplexClassInner1.PropComplexClassInner2.PropInt);
        }

        [Fact]
        public void ExampleMergeTwoPersistentObjects()
        {
            IPersistentFactory persistentFactory = new PersistentFactory();
            var obj1 = persistentFactory.CreateRootObject<ComplexClassRoot>(CreateFilePath(nameof(ExampleMergeTwoPersistentObjects)));
            var obj2 = persistentFactory.CreateRootObject<ComplexClassInner1>(CreateFilePath(nameof(ExampleMergeTwoPersistentObjects) + "2"));

            obj1.PropComplexClassInner1 = obj2;

            Assert.Equal(obj1.PropComplexClassInner1, obj2);
        }

        [Fact]
        public void ExampleReadReferenceObjBeforeWrite_ShouldGetNull()
        {
            IPersistentFactory persistentFactory = new PersistentFactory();
            var obj1 = persistentFactory.CreateRootObject<ComplexClassRoot>(
                CreateFilePath(nameof(ExampleReadReferenceObjBeforeWrite_ShouldGetNull)));
            Assert.Null(obj1.PropStr);
            Assert.Null(obj1.PropComplexClassInner1);
        }
    }
}
