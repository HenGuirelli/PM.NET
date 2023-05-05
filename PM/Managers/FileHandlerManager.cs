using PM.Core;
using PM.Factories;
using System.Collections.Concurrent;

namespace PM.Managers
{
    internal static class FileHandlerManager
    {
        public static readonly ConcurrentDictionary<string, FileBasedStream> _fileHandlersByFilename = new();
        public static readonly HashSet<FileBasedStream> _fileHandlers = new();

        public static FileBasedStream CreateRootHandler(string filepath, int size = 4096)
        {
            return CreateHandler(filepath, "root", size);
        }

        public static FileBasedStream CreateHashHandler(string filepath, int size = 4096)
        {
            return CreateHandler(filepath, "hash", size);
        }

        public static FileBasedStream CreateInternalObjectHandler(string filepath, int size = 4096)
        {
            return CreateHandler(filepath, "pm", size);
        }

        public static FileBasedStream CreateHandler(string filepath, int size = 4096)
        {
            if (_fileHandlersByFilename.TryGetValue(filepath, out var pmCached))
            {
                return pmCached;
            }
            var pm = PmFactory.CreatePm(filepath, size);
            RegisterNewHandler(pm);
            return pm;
        }

        public static FileBasedStream CreateHandler(string filepath, string extension, int size = 4096)
        {
            var filename = filepath.EndsWith(extension) ? filepath : $"{filepath}.{extension}";
            return CreateHandler(filename, size);
        }

        private static void RegisterNewHandler(FileBasedStream fileBasedStream)
        {
            _fileHandlersByFilename.TryAdd(fileBasedStream.FilePath, fileBasedStream);
            _fileHandlers.Add(fileBasedStream);
        }

        public static void CloseAndDiscard(FileBasedStream fileBasedStream)
        {
            CloseAndDiscard(fileBasedStream.FilePath);
        }

        public static void CloseAndDiscard(string filepath)
        {
            if (_fileHandlersByFilename.TryRemove(filepath, out var removedHandler) &&
                _fileHandlers.Remove(removedHandler))
            {
                removedHandler.Dispose();
            }
        }
    }
}
