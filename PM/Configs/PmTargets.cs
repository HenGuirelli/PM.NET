namespace PM.Configs
{
    [Flags]
    public enum PmTargets
    {
        PM                          = 1 << 0,
        InVolatileMemory            = 1 << 1,
        TraditionalMemoryMappedFile = 1 << 2,

        FileBasedTarget = PM | TraditionalMemoryMappedFile
    }
}
