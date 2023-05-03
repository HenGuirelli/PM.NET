﻿using PM.Configs;
using PM.Startup;
using PM.Tests.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PM.Tests.Startup
{
    public class ComplexClassWithSelfReference
    {
        public virtual int Prop { get; set; }
        public virtual ComplexClassWithSelfReference SelfReference { get; set; }
    }

    public class PmPointerCounterTest : UnitTest
    {
        [Fact]
        public void OnCollect_ShouldDeleteUnusedFile()
        {
            DeleteAllFiles(PmGlobalConfiguration.PmInternalsFolder);

            var filepath = CreateFilePath(nameof(OnCollect_ShouldNotDeleteAnyFile));
            var testingObject = new PmPointerCounter();

            IPersistentFactory persistentFactory = new PersistentFactory();
            var obj = persistentFactory
                .CreateRootObject<ComplexClassWithSelfReference>(filepath);

            obj.SelfReference = new ComplexClassWithSelfReference
            {
                Prop = 1,
            };

            obj.SelfReference = null!;

            var pointers = testingObject.Collect(PmGlobalConfiguration.PmInternalsFolder);
        }
        
        [Fact]
        public void OnCollect_ShouldNotDeleteAnyFile()
        {
            DeleteAllFiles(PmGlobalConfiguration.PmInternalsFolder);

            var filepath = CreateFilePath(nameof(OnCollect_ShouldNotDeleteAnyFile));
            var testingObject = new PmPointerCounter();

            IPersistentFactory persistentFactory = new PersistentFactory();
            var obj = persistentFactory
                .CreateRootObject<ComplexClassWithSelfReference>(filepath);

            obj.SelfReference = new ComplexClassWithSelfReference
            {
                Prop = 1,
            };

            var pointers = testingObject.Collect(PmGlobalConfiguration.PmInternalsFolder);
        }

        static void DeleteAllFiles(string path)
        {
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                File.Delete(file);
            }
        }
    }
}
