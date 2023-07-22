using Serilog;

namespace PM.Configs
{
    public class PmLogger
    {
        public PmLogTarget Target { get; private set; } = PmLogTarget.None;
        public string? Directory { get; private set; }

        public PmLogger()
        {
            SetTarget(PmLogTarget.None);
        }

        public void SetTarget(PmLogTarget target, string? directory = null)
        {
            if (target == PmLogTarget.File && string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException(
                    $"Parameter {nameof(directory)} cannot be empty if target equals {target}");
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
            Log.Information("Serilog setup finished, target={target}", Target);
        }
    }
}
