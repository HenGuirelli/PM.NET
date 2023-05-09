using PM.Core;
using PM.Factories;
using System.Collections.Concurrent;

namespace PM.Managers
{
    internal class FileHandlerItem
    {
        public FileBasedStream FileBasedStream { get; }
        public int PointerCounter { get; set; } = 1;

        public FileHandlerItem(FileBasedStream fileBasedStream)
        {
            FileBasedStream = fileBasedStream;
        }
    }

    internal static class FileHandlerManager
    {
        public static readonly ConcurrentDictionary<string, FileHandlerItem> 
            _fileHandlersByFilename = new();
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

        public static FileBasedStream CreateHandler(string filepath, string extension, int size = 4096)
        {
            var filename = filepath.EndsWith(extension) ? filepath : $"{filepath}.{extension}";
            return CreateHandler(filename, size);
        }

        public static FileBasedStream CreateHandler(string filepath, int size = 4096)
        {
            if (_fileHandlersByFilename.TryGetValue(filepath, out var pmCached))
            {
                pmCached.PointerCounter++;
                return pmCached.FileBasedStream;
            }
            var pm = PmFactory.CreatePm(filepath, size);
            RegisterNewHandler(pm);
            return pm;
        }

        private static void RegisterNewHandler(FileBasedStream fileBasedStream)
        {
            _fileHandlersByFilename.TryAdd(
                fileBasedStream.FilePath,
                new FileHandlerItem(fileBasedStream));

            _fileHandlers.Add(fileBasedStream);
        }

        public static bool CloseAndDiscard(FileBasedStream fileBasedStream)
        {
            return CloseAndDiscard(fileBasedStream.FilePath);
        }

        public static bool CloseAndDiscard(string filepath)
        {
            if (_fileHandlersByFilename.TryGetValue(filepath, out var fileHandlerItem))
            {
                if (fileHandlerItem.PointerCounter == 1)
                {
                    if (_fileHandlersByFilename.TryRemove(filepath, out var removedHandler) &&
                        _fileHandlers.Remove(removedHandler.FileBasedStream))
                    {
                        removedHandler.FileBasedStream.Dispose();
                        return true;
                    }
                }
                fileHandlerItem.PointerCounter--;
                return false;
            }
            return true;
        }
    }
}
