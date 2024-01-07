﻿using PM.PmContent;
using PM.Startup;
using PM.Tests.Common;
using System;
using Xunit;
using Xunit.Abstractions;

namespace PM.Tests.Startup
{
    public class ClassHashManagerTest : UnitTest
    {
        private static readonly Random _random = new();

        public ClassHashManagerTest(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void OnAddHashFile_ShouldCreateAndWriteFile()
        {
            var classHashManager = ClassHashManager.Instance;

            var obj = new ComplexClassWithSelfReference();
            ulong randomPointer = (ulong)_random.Next();

            classHashManager.AddHashFile(obj.GetType());

            var hashFile = classHashManager.GetHashFile(obj.GetType());

            Assert.Equal(ClassHashCodeCalculator.GetHashCode(obj.GetType()), hashFile!.Hash);
        }
    }
}
