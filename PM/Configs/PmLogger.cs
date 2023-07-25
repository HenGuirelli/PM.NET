using Serilog;
using Serilog.Events;

namespace PM.Configs
{
    public class PmLogger
    {
        public PmLogTarget Target { get; private set; } = PmLogTarget.None;
        public string? Directory { get; private set; }
        private LogEventLevel _logEventLevel = LogEventLevel.Verbose;
        private LoggerConfiguration _loggerConfiguration;

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

        public void SetTarget(
            PmLogTarget target,
            LogEventLevel logEventLevel = LogEventLevel.Information,
            string? directory = null)
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
            _logEventLevel = logEventLevel;

            UpdateLogger();
        }

        private void UpdateLogger()
        {
            _loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel);
            var writeTo = _loggerConfiguration.WriteTo;

            if ((Target & PmLogTarget.Console) != 0)
            {
                _loggerConfiguration = writeTo.Console();
            }
            if ((Target & PmLogTarget.File) != 0)
            {
                _loggerConfiguration = writeTo.File(
                    Path.Combine(Directory!, "PM.NET.log"),
                    fileSizeLimitBytes: null);
            }

            Log.Logger = _loggerConfiguration.CreateLogger();
            Log.Information("Serilog setup finished, target={target}", Target);
        }
    }
}
