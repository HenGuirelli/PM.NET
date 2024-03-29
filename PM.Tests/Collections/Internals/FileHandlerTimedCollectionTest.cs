﻿using PM.Collections.Internals;
using PM.Core;
using System.IO;
using Xunit;

namespace PM.Tests.Collections.Internals
{
    public class FileHandlerTimedCollectionTest
    {
        [Fact]
        public void OnFullCapacity_ShouldThrowException()
        {
            var capacity = 10;
            var fileHandlerTimedCollection = new FileHandlerTimedCollection(capacity);

            for (int i = 0; i < capacity; i++)
            {
                fileHandlerTimedCollection.Add(i.ToString(), null);
            }

            Assert.Equal(capacity, fileHandlerTimedCollection.Count);

            Assert.Throws<CollectionLimitReachedException>(() => fileHandlerTimedCollection.Add("11", null));
        }

        [Fact]
        public void OnCleanOldValues_ShouldremoveHalfItems()
        {
            var capacity = 10;
            var fileHandlerTimedCollection = new FileHandlerTimedCollection(capacity);

            var dir = new DirectoryInfo(".");

            foreach (var file in dir.EnumerateFiles("*.logtest"))
            {
                file.Delete();
            }

            for (int i = 0; i < capacity; i++)
            {
                //fileHandlerTimedCollection.Add(i.ToString(), null);
                fileHandlerTimedCollection.Add(i.ToString(), 
                    new PM.Managers.FileHandlerItem(
                        new TraditionalMemoryMappedStream($"./{i}.logtest", 4096)));
            }

            Assert.Equal(capacity, fileHandlerTimedCollection.Count);

            Assert.Throws<CollectionLimitReachedException>(() => fileHandlerTimedCollection.Add("11", null));
            fileHandlerTimedCollection.CleanOldValues(capacity / 2);

            Assert.Equal(5, fileHandlerTimedCollection.Count);

        }
    }
}
