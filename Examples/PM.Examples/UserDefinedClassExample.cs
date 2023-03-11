using PM.Configs;
using PM.Examples.UserDefinedClassExampleDomainClasses;
using PM.Tests.Common;
using Xunit;

namespace PM.Examples
{
    public class UserDefinedClassExample
    {
        public UserDefinedClassExample()
        {
            PmGlobalConfiguration.PmTarget = Constraints.PmTarget;
            PmGlobalConfiguration.PmInternalsFolder = Constraints.PmRootFolder;
        }

        [Fact]
        public void BasicSetAndGet()
        {
            IPersistentFactory factory = new PersistentFactory();
            var obj = factory.CreateRootObject<BasicSetAndGetClass>("PmFilename");

            // Call persitent memory (PM) and write
            // content into file called "PmFilename"
            obj.PropInt = int.MinValue;
            obj.PropString = "Hello PM!";

            Assert.Equal(int.MinValue, obj.PropInt);
            Assert.Equal("Hello PM!", obj.PropString);
        }
    }
}