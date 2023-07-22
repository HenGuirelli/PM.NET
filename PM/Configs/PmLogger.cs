using Serilog;
using Serilog.Events;

namespace PM.Configs
{
    public class PmLogger
    {
        public PmLogTarget Target { get; private set; } = PmLogTarget.None;
        public string? Directory { get; private set; }
        private LogEventLevel _logEventLevel = LogEventLevel.Verbose;
        public LogEventLevel LogEventLevel
        {
            get => _logEventLevel;
            set
            {
                _logEventLevel = value;
                UpdateLogger();
            }
        }

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
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel);
            var writeTo = loggerConfiguration.WriteTo;

            if ((Target & PmLogTarget.Console) != 0)
            {
                loggerConfiguration = writeTo.Console();
            }
            if ((Target & PmLogTarget.File) != 0)
            {
                loggerConfiguration = writeTo.File(Path.Combine(Directory!, "PM.NET.log"));
            }

            Log.Logger = loggerConfiguration.CreateLogger();
            Log.Information("Serilog setup finished, target={target}", Target);
        }
    }
}
