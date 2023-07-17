using PM.Core;
using Serilog;

namespace PM.Configs
{
    [Flags]
    public enum PmLogTarget
    {
        None    = 0,
        Console = 1 << 1,
        File    = 1 << 2,
    }

    public static class PmLogTargetExtensions
    {
        public static bool IsValid(this PmLogTarget target)
        {
            if (target.HasFlag(PmLogTarget.None) && target != PmLogTarget.None)
            {
                return false;
            }
            return true;
        }
    }

    public class PmLogger
    {
        public PmLogTarget Target { get; private set; } = PmLogTarget.None;
        public string? Directory { get; private set; }

        public void SetTarget(PmLogTarget target, string? directory)
        {
            if (target == PmLogTarget.File && string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException(
                    $"Parameter {nameof(directory)} cannot be empty if target equals {target}");
            }

            if (!target.IsValid())
            {
                throw new ArgumentException($"Target value of {target} is not valid");
            }

            if (target == PmLogTarget.File && !string.IsNullOrWhiteSpace(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            Target = target;
            Directory = directory;

            UpdateLogger();
        }

        private void UpdateLogger()
        {
            var loggerConfiguration = new LoggerConfiguration();
            var writeTo = loggerConfiguration.WriteTo;

            if ((Target & PmLogTarget.Console) != 0)
            {
                loggerConfiguration = writeTo.Console();
            }
            if ((Target & PmLogTarget.File) != 0)
            {
                loggerConfiguration = writeTo.File(Directory!);
            }

            Log.Logger = loggerConfiguration.CreateLogger();
            Log.Information("Usando Serilog...");
        }
    }

    public static class PmGlobalConfiguration
    {
        public static PmTargets PmTarget { get; set; } = PmTargets.PM;
        public static int CollectFileInterval { get; set; } = 120000;
        public static string PmInternalsFolder { get; set; } = Path.Combine("pm", "internals");
        public static string PmTransactionFolder => Path.Combine(PmInternalsFolder, "transactions");

        public static PmLogger Logger { get; } = new PmLogger();
    }
}
