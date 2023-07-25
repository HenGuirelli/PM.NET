using Serilog;
using System.Diagnostics;
using System.Text;

namespace PM.Core
{
    public abstract class FileBasedStream : Stream
    {
        public virtual string FilePath { get; protected set; } = string.Empty;
        public bool IsDisposed { get; protected set; }

        public virtual void Delete()
        {
            Log.Verbose("Deleting file={file}, size={size}, stack={StackTrace}",
                FilePath,
                Length,
                GetStack());

            File.Delete(FilePath);
        }

        public virtual void Open()
        {
            Log.Verbose("Opening file={file}, size={size}, stack={StackTrace}",
                FilePath,
                Length,
                GetStack());
        }

        public override void Flush()
        {
            Log.Verbose("Flushing file={file}, size={size}, stack={StackTrace}",
                FilePath,
                Length,
                GetStack());
        }

        public override void SetLength(long value)
        {
            Log.Verbose("SetLength called on file={file}, size={size}, stack={StackTrace}",
                FilePath,
                Length,
                GetStack());
        }

        public virtual void Resize(int size)
        {
            Log.Verbose("Resizing file {file}. " +
                "Old size={oldSize}, new size={size}, stack={StackTrace}",
                FilePath, Length, size, GetStack());
        }

        public override void Close()
        {
            Log.Verbose("Closing file {file}, stack={StackTrace}", FilePath, GetStack());
        }

        protected override void Dispose(bool disposing)
        {
            Log.Verbose("Disposing file {file}, stack={StackTrace}", FilePath, GetStack());
            base.Dispose(disposing);
        }

        public static string GetStack()
        {
            if (Log.IsEnabled(Serilog.Events.LogEventLevel.Verbose))
            {
                var stackTrace = new StackTrace();
                return FormatStackTrace(stackTrace);
            }
            return string.Empty;
        }

        public static string FormatStackTrace(StackTrace stackTrace)
        {
            var frames = stackTrace.GetFrames();
            if (frames == null)
                return string.Empty;

            var formattedStack = new StringBuilder();
            foreach (var frame in frames)
            {
                var method = frame.GetMethod();
                var typeName = method?.DeclaringType?.FullName ?? "Unknown Type";
                var methodName = method?.Name ?? "Unknown Method";
                var lineNumber = frame.GetFileLineNumber();

                formattedStack.AppendLine($"{typeName}.{methodName}() (Line {lineNumber})");
            }

            return formattedStack.ToString();
        }
    }
}
