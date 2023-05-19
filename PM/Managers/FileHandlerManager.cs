using PM.Core;
using System.Collections.Concurrent;
using System.Drawing;

namespace PM.Managers
{
    public class FileHandlerItem
    {
        public FileBasedStream FileBasedStream { get; }
        public bool HasMemoryReference { get; set; } = true;
        public ulong FilePointerReference { get; set; }

        public FileHandlerItem(FileBasedStream fileBasedStream)
        {
            FileBasedStream = fileBasedStream;
        }
    }

    public static class FileHandlerManager
    {
        public static readonly ConcurrentDictionary<string, FileHandlerItem> 
            _fileHandlersByFilename = new();
        public static readonly HashSet<FileBasedStream> _fileHandlers = new();

        public static FileHandlerItem CreateRootHandler(string filepath, int size = 4096)
        {
            return CreateHandler(filepath, "root", size);
        }

        public static FileHandlerItem CreateHashHandler(string filepath, int size = 4096)
        {
            return CreateHandler(filepath, "hash", size);
        }

        public static FileHandlerItem CreateInternalObjectHandler(string filepath, int size = 4096)
        {
            return CreateHandler(filepath, "pm", size);
        }

        public static FileHandlerItem CreateListHandler(string filepath, int size = 4096)
        {
            return CreateHandler(filepath, "pmlistitem", size);
        }

        public static FileHandlerItem CreateHandler(string filepath, string extension, int size = 4096)
        {
            var filename = filepath.EndsWith(extension) ? filepath : $"{filepath}.{extension}";
            return CreateHandler(filename, size);
        }

        public static FileHandlerItem CreateHandler(string filepath, int size = 4096)
        {
            if (_fileHandlersByFilename.TryGetValue(filepath, out var pmCached))
            {
                return pmCached;
            }
            var pm = new PmStream(filepath, size);
            return RegisterNewHandler(pm);
        }

        private static FileHandlerItem RegisterNewHandler(FileBasedStream fileBasedStream)
        {
            var fileHandlerItem = new FileHandlerItem(fileBasedStream);
            _fileHandlersByFilename.TryAdd(
                fileBasedStream.FilePath,
                fileHandlerItem);

            _fileHandlers.Add(fileBasedStream);

            return fileHandlerItem;
        }

        public static void ReleaseObjectFromMemory(FileBasedStream fileBasedStream)
        {
            ReleaseObjectFromMemory(fileBasedStream.FilePath);
        }

        public static void ReleaseObjectFromMemory(string filepath)
        {
            if (_fileHandlersByFilename.TryGetValue(filepath, out var fileHandlerItem))
            {
                fileHandlerItem.HasMemoryReference = false;
            }
        }

        public static bool CloseAndRemoveFile(FileBasedStream fileBasedStream)
        {
            return CloseAndRemoveFile(fileBasedStream.FilePath);
        }

        public static bool CloseAndRemoveFile(string filepath)
        {
            if (_fileHandlersByFilename.TryGetValue(filepath, out var fileHandlerItem))
            {
                // Only reference from this class
                if (!fileHandlerItem.HasMemoryReference && fileHandlerItem.FilePointerReference == 0)
                {
                    if (_fileHandlersByFilename.TryRemove(filepath, out var removedHandler) &&
                        _fileHandlers.Remove(removedHandler.FileBasedStream))
                    {
                        removedHandler.FileBasedStream.Close();
                        File.Delete(filepath);
                        return true;
                    }
                }
                return false;
            }
            else
            {
                try
                {
                    File.Delete(filepath);
                }
                catch
                {
                }
            }
            return true;
        }
    }
}
