namespace PM.Core
{
    /// <summary>
    /// Represents all objects that can be persisted
    /// </summary>
    public interface IPersistentObject
    {
        /// <summary>
        /// Initial load if file already exists
        /// </summary>
        void Load();
    }
}
