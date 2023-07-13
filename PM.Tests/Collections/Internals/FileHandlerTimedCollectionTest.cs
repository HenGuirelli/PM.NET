using PM.Collections.Internals;
using System;
using Xunit;

namespace PM.Tests.Collections.Internals
{
    public class FileHandlerTimedCollectionTest
    {
        [Fact]
        public void OnFullCapcity_ShouldThrowException()
        {
            var capacity = 10;
            var fileHandlerTimedCollection = new FileHandlerTimedCollection(capacity);

            for (int i = 0; i < capacity; i++)
            {
                fileHandlerTimedCollection.Add(i.ToString(), null);
            }

            Assert.Equal(capacity, fileHandlerTimedCollection.Count);

            Assert.Throws<ApplicationException>(() => fileHandlerTimedCollection.Add("11", null));
        }

        [Fact]
        public void OnCleanOldValues_ShouldremoveHalfItems()
        {
            var capacity = 10;
            var fileHandlerTimedCollection = new FileHandlerTimedCollection(capacity);

            for (int i = 0; i < capacity; i++)
            {
                fileHandlerTimedCollection.Add(i.ToString(), null);
            }

            Assert.Equal(capacity, fileHandlerTimedCollection.Count);

            Assert.Throws<ApplicationException>(() => fileHandlerTimedCollection.Add("11", null));
            fileHandlerTimedCollection.CleanOldValues(capacity / 2);

            Assert.Equal(5, fileHandlerTimedCollection.Count);

        }
    }
}
