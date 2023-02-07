namespace PM.Tests.Common
{
    public class Constraints
    {
        /// <summary>
        /// Enable/disable the use of simulated PM in memory for environments without persistent memory.
        /// The real environment is always preferable, with this flag disabled.
        /// </summary>
        public static bool UseFakePm = true;
    }
}