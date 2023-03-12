using PM.Examples.UserDefinedClassExampleDomainClasses;
using PM.Tests.Common;
using Xunit;

namespace PM.Examples
{
    public class UserDefinedClassExample : UnitTest
    {
        [Fact]
        public void BasicSetAndGet()
        {
            IPersistentFactory factory = new PersistentFactory();
            var obj = factory.CreateRootObject<BasicSetAndGetClass>(CreateFilePath("PmFilename"));

            // Call persitent memory (PM) and write
            // content into file called "PmFilename"
            obj.PropInt = int.MinValue;
            obj.PropString = "Hello PM!";

            Assert.Equal(int.MinValue, obj.PropInt);
            Assert.Equal("Hello PM!", obj.PropString);
        }
    }
}