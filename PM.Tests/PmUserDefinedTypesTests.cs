using PM.Core;
using PM.Factories;
using PM.PmContent;
using PM.Tests.Common;
using System;
using Xunit;

namespace PM.Tests
{
    [Collection("PM.UnitTests")]
    public class PmUserDefinedTypesTests : UnitTest
    {
        [Fact]
        public void OnUpdateAndGetIntPropertyValue_ShouldExecWithoutException()
        {
            var pm = CreateObject(
                nameof(OnUpdateAndGetIntPropertyValue_ShouldExecWithoutException),
                typeof(ComplexClassWithPrimitiveProperties));

            var prop = typeof(ComplexClassWithPrimitiveProperties).GetProperty("Prop1");
            if (prop is null) throw new ApplicationException();

            pm.UpdateProperty(prop, int.MinValue);
            Assert.Equal(int.MinValue, pm.GetIntPropertValue(prop));
        }

        [Fact]
        public void OnUpdateAndGetFloatPropertyValue_ShouldExecWithoutException()
        {
            var pm = CreateObject(
                nameof(OnUpdateAndGetFloatPropertyValue_ShouldExecWithoutException),
                typeof(ComplexClassWithPrimitiveProperties));

            var prop = typeof(ComplexClassWithPrimitiveProperties).GetProperty("Prop2");
            if (prop is null) throw new ApplicationException();
            pm.UpdateProperty(prop, float.MaxValue);
            Assert.Equal(float.MaxValue, pm.GetFloatPropertValue(prop));
        }

        [Fact]
        public void OnUpdateAndGetDecimalPropertyValue_ShouldExecWithoutException()
        {
            var pm = CreateObject(
                nameof(OnUpdateAndGetDecimalPropertyValue_ShouldExecWithoutException),
                typeof(ComplexClassWithPrimitiveProperties));

            var prop = typeof(ComplexClassWithPrimitiveProperties).GetProperty("Prop3");
            if (prop is null) throw new ApplicationException();
            pm.UpdateProperty(prop, decimal.MaxValue);
            Assert.Equal(decimal.MaxValue, pm.GetDecimalPropertValue(prop));
        }

        [Fact]
        public void OnUpdateAndGetBytePropertyValue_ShouldExecWithoutException()
        {
            var pm = CreateObject(
                nameof(OnUpdateAndGetBytePropertyValue_ShouldExecWithoutException),
                typeof(ComplexClassWithPrimitiveProperties));

            var prop = typeof(ComplexClassWithPrimitiveProperties).GetProperty("Prop4");
            if (prop is null) throw new ApplicationException();
            pm.UpdateProperty(prop, byte.MaxValue);
            Assert.Equal(byte.MaxValue, pm.GetBytePropertValue(prop));
        }

        [Fact]
        public void OnUpdateAndGetSBytePropertyValue_ShouldExecWithoutException()
        {
            var pm = CreateObject(
                nameof(OnUpdateAndGetSBytePropertyValue_ShouldExecWithoutException),
                typeof(ComplexClassWithPrimitiveProperties));

            var prop = typeof(ComplexClassWithPrimitiveProperties).GetProperty("Prop5");
            if (prop is null) throw new ApplicationException();
            pm.UpdateProperty(prop, sbyte.MaxValue);
            Assert.Equal(sbyte.MaxValue, pm.GetSBytePropertValue(prop));
        }

        [Fact]
        public void OnUpdateAndGetShortPropertyValue_ShouldExecWithoutException()
        {
            var pm = CreateObject(
                nameof(OnUpdateAndGetShortPropertyValue_ShouldExecWithoutException),
                typeof(ComplexClassWithPrimitiveProperties));

            var prop = typeof(ComplexClassWithPrimitiveProperties).GetProperty("Prop6");
            if (prop is null) throw new ApplicationException();
            pm.UpdateProperty(prop, short.MaxValue);
            Assert.Equal(short.MaxValue, pm.GetShortPropertValue(prop));
        }

        [Fact]
        public void OnUpdateAndGetUShortPropertyValue_ShouldExecWithoutException()
        {
            var pm = CreateObject(
                nameof(OnUpdateAndGetUShortPropertyValue_ShouldExecWithoutException),
                typeof(ComplexClassWithPrimitiveProperties));

            var prop = typeof(ComplexClassWithPrimitiveProperties).GetProperty("Prop7");
            if (prop is null) throw new ApplicationException();
            pm.UpdateProperty(prop, ushort.MaxValue);
            Assert.Equal(ushort.MaxValue, pm.GetUShortPropertValue(prop));
        }

        [Fact]
        public void OnUpdateAndGetUIntPropertyValue_ShouldExecWithoutException()
        {
            var pm = CreateObject(
                nameof(OnUpdateAndGetUIntPropertyValue_ShouldExecWithoutException),
                typeof(ComplexClassWithPrimitiveProperties));

            var prop = typeof(ComplexClassWithPrimitiveProperties).GetProperty("Prop8");
            if (prop is null) throw new ApplicationException();
            pm.UpdateProperty(prop, uint.MaxValue);
            Assert.Equal(uint.MaxValue, pm.GetUIntPropertValue(prop));
        }

        [Fact]
        public void OnUpdateAndGetLongPropertyValue_ShouldExecWithoutException()
        {
            var pm = CreateObject(
                nameof(OnUpdateAndGetLongPropertyValue_ShouldExecWithoutException),
                typeof(ComplexClassWithPrimitiveProperties));

            var prop = typeof(ComplexClassWithPrimitiveProperties).GetProperty("Prop9");
            if (prop is null) throw new ApplicationException();
            pm.UpdateProperty(prop, long.MaxValue);
            Assert.Equal(long.MaxValue, pm.GetLongPropertValue(prop));
        }

        [Fact]
        public void OnUpdateAndGetULongPropertyValue_ShouldExecWithoutException()
        {
            var pm = CreateObject(
                nameof(OnUpdateAndGetULongPropertyValue_ShouldExecWithoutException),
                typeof(ComplexClassWithPrimitiveProperties));

            var prop = typeof(ComplexClassWithPrimitiveProperties).GetProperty("Prop10");
            if (prop is null) throw new ApplicationException();
            pm.UpdateProperty(prop, ulong.MaxValue);
            Assert.Equal(ulong.MaxValue, pm.GetULongPropertValue(prop));
        }

        [Fact]
        public void OnUpdateAndGetDoublePropertyValue_ShouldExecWithoutException()
        {
            var pm = CreateObject(
                nameof(OnUpdateAndGetDoublePropertyValue_ShouldExecWithoutException),
                typeof(ComplexClassWithPrimitiveProperties));

            var prop = typeof(ComplexClassWithPrimitiveProperties).GetProperty("Prop11");
            if (prop is null) throw new ApplicationException();
            pm.UpdateProperty(prop, double.MaxValue);
            Assert.Equal(double.MaxValue, pm.GetDoublePropertValue(prop));
        }

        [Fact]
        public void OnUpdateAndGetCharPropertyValue_ShouldExecWithoutException()
        {
            var pm = CreateObject(
                nameof(OnUpdateAndGetCharPropertyValue_ShouldExecWithoutException),
                typeof(ComplexClassWithPrimitiveProperties));

            var prop = typeof(ComplexClassWithPrimitiveProperties).GetProperty("Prop12");
            if (prop is null) throw new ApplicationException();
            pm.UpdateProperty(prop, char.MaxValue);
            Assert.Equal(char.MaxValue, pm.GetCharPropertValue(prop));
        }

        [Fact]
        public void OnUpdateAndGetBoolPropertyValue_ShouldExecWithoutException()
        {
            var pm = CreateObject(
                nameof(OnUpdateAndGetBoolPropertyValue_ShouldExecWithoutException),
                typeof(ComplexClassWithPrimitiveProperties));

            var prop = typeof(ComplexClassWithPrimitiveProperties).GetProperty("Prop13");
            if (prop is null) throw new ApplicationException();
            pm.UpdateProperty(prop, true);
            Assert.True(pm.GetBoolPropertValue(prop));
        }

        private static PmUserDefinedTypes CreateObject(string pmMappedFile, Type typeclasstest)
        {
            var pm = PmFactory.CreatePm(new PmMemoryMappedFileConfig(pmMappedFile));
            var objectPropertiesInfoMapper = new ObjectPropertiesInfoMapper(typeclasstest, new PmHeader(typeclasstest));
            return new PmUserDefinedTypes(pm, objectPropertiesInfoMapper);
        }
    }
}