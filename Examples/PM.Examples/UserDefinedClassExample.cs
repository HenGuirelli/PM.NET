using PM.Examples.UserDefinedClassExampleDomainClasses;
using PM.Tests.Common;
using System;
using System.Linq;
using Xunit;

namespace PM.Examples
{
    public class UserDefinedClassExample : UnitTest
    {
        [Fact]
        public void BasicSetAndGet()
        {
            IPersistentFactory factory = new PersistentFactory();
            var obj = factory.CreateRootObject<BasicSetAndGetClass>(CreateFilePath(nameof(BasicSetAndGet)));

            // Call persitent memory (PM) and write
            // content into file called "BasicSetAndGet.pm"
            obj.PropInt = int.MinValue;
            obj.PropString = "Hello PM!";

            Assert.Equal(int.MinValue, obj.PropInt);
            Assert.Equal("Hello PM!", obj.PropString);
        }

        [Fact]
        public void ClassWithPersistentList()
        {
            var val = Guid.NewGuid().ToString();
            IPersistentFactory factory = new PersistentFactory();
            var obj = factory.CreateRootObject<ClassWithPersistentList>(
                CreateFilePath(nameof(ClassWithPersistentList)));

            obj.ItemList = new Collections.PmList<ListItem>(
                CreateFilePath(nameof(ClassWithPersistentList) + "_List"));
            obj.ItemList.Clear();
            obj.ItemList.AddPersistent(new ListItem
            {
                Val = val
            });

            Assert.Equal(val, obj.ItemList.Single().Val);
        }
    }
}