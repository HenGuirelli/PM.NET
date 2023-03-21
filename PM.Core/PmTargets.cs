namespace PM.Core
{
    [Flags]
    public enum PmTargets
    {
        PM                          = 1 << 0,
        TraditionalMemoryMappedFile = 1 << 1,

        FileBasedTarget = PM | TraditionalMemoryMappedFile
    }
}
