using PM.Core;

namespace PM.Configs
{
    public static class PmGlobalConfiguration
    {
        public static PmTargets PmTarget { get; set; } = PmTargets.PM;
        public static int CollectFileInterval { get; set; } = 120000;
        public static string PmInternalsFolder { get; set; } = Path.Combine("pm", "internals");
        public static string PmTransactionFolder => Path.Combine(PmInternalsFolder, "transactions");

        public static PmLogger Logger { get; set; } = new PmLogger();
    }
}
