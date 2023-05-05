using PM.Core;

namespace PM.Tests.Common
{
    public class Constraints
    {
        /// <summary>
        /// Enable/disable the use of simulated PM in memory for environments without persistent memory.
        /// The real environment is always preferable.
        /// </summary>
        public const PmTargets PmTarget = PmTargets.TraditionalMemoryMappedFile;
        public const string PmRootFolder = @"D:\temp\pm_tests";
    }
}