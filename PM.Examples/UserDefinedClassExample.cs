using PM.Configs;
using PM.Examples.UserDefinedClassExampleDomainClasses;
using Xunit;

namespace PM.Examples
{
    public class UserDefinedClassExample
    {
        public UserDefinedClassExample()
        {
            // Uses persistent memory so you don't need
            // the hardware for persistent memory
            PmGlobalConfiguration.PmTarget = PmTargets.InVolatileMemory;
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