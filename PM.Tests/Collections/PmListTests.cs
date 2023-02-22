﻿using PM.Collections;
using PM.Configs;
using PM.Tests.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PM.Tests.Collections
{
    public class Foo
    {
        public virtual int Bar { get; set; }
    }

    public class PmListTests
    {
        public PmListTests()
        {
            if (Constraints.UseFakePm)
            {
                PmGlobalConfiguration.PmTarget = PmTargets.TraditionalMemoryMappedFile;
                PmGlobalConfiguration.PmInternalsFolder = "D:\\temp\\pm_tests";
            }
        }

        [Fact]
        public void OnAddPersistent_ShouldAdd()
        {
            var list = new PmList<Foo>(Path.Combine(PmGlobalConfiguration.PmInternalsFolder, nameof(OnAddPersistent_ShouldAdd)));

            list.AddPersistent(new Foo { Bar = 1 });
            list.AddPersistent(new Foo { Bar = 2 });

            Assert.Equal(2, list.Count);
            Assert.Equal(1, list[0].Bar);
            Assert.Equal(2, list[1].Bar);
            Assert.Throws<IndexOutOfRangeException>(() => list[2].Bar);
        }

        [Fact]
        public void OnAddPersistentWhenOverflowDefaultCapacity_ShouldAdd()
        {
            var count = 300;
            var list = new PmList<Foo>(nameof(OnAddPersistentWhenOverflowDefaultCapacity_ShouldAdd));

            for (int i = 0; i < count; i++)
            {
                list.AddPersistent(new Foo { Bar = i });
            }

            Assert.Equal(count, list.Count);
            // Verify first and last item in list
            Assert.Equal(0, list[0].Bar);
            Assert.Equal(count - 1, list[list.Count - 1].Bar);
        }

        [Fact]
        public void OnClear_ShouldClearEntireList()
        {
            var count = 3;
            var list = new PmList<Foo>(nameof(OnClear_ShouldClearEntireList));

            for (int i = 0; i < count; i++)
            {
                list.AddPersistent(new Foo { Bar = i });
            }

            list.Clear();
            Assert.Empty(list);
        }


        [Fact]
        public void OnSetsAndGets_ShouldRunCorretly()
        {
            var list = new PmList<Foo>(nameof(OnSetsAndGets_ShouldRunCorretly));

            list.AddPersistent(new Foo { Bar = 1 });
            list.AddPersistent(new Foo { Bar = 2 });

            Assert.Equal(1, list[0].Bar);    
            Assert.Equal(2, list[1].Bar);
        }

        [Fact]
        public void OnEnumerator_ShouldRunCorretly()
        {
            var count = 300;
            var list = new PmList<Foo>(nameof(OnEnumerator_ShouldRunCorretly));

            for (int i = 0; i < count; i++)
            {
                list.AddPersistent(new Foo { Bar = i });
            }

            int j = 0;
            foreach(var item in list)
            {
                Assert.Equal(list[j].Bar, item.Bar);
                j++;
            }
        }

        [Fact]
        public void OnCopyTo_ShouldCopyEntireList()
        {
            var count = 300;
            var list = new PmList<Foo>(nameof(OnCopyTo_ShouldCopyEntireList));
            
            for (int i = 0; i < count / 2; i++)
            {
                list.AddPersistent(new Foo { Bar = i });
            }

            Foo[] vet = new Foo[count];
            list.CopyTo(vet, count);

            for (int i = 0; i < count / 2; i++)
                Assert.Equal(i, vet[i].Bar);
        }

        [Fact]
        public void OnRemove_ShouldCopyEntireList()
        {
            var list = new PmList<Foo>(nameof(OnRemove_ShouldCopyEntireList));

            list.AddPersistent(new Foo { Bar = 1 });
            var item = list.AddPersistent(new Foo { Bar = 2 });
            list.AddPersistent(new Foo { Bar = 3 });

            Assert.Equal(3, list.Count);
            Assert.True(list.Remove(item));
            Assert.Equal(2, list.Count);
        }
    }
}
