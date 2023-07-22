using PM.Core;

namespace PM.Configs
{
    [Flags]
    public enum PmLogTarget
    {
        None    = 1 << 0,
        Console = 1 << 1,
        File    = 1 << 2,
    }
}
