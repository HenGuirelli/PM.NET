using PM.Collections.Internals;
using PM.Core;
using System.Collections.Concurrent;
using System.Drawing;
using System.Reflection;

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
        private static readonly FileHandlerTimedCollection
            _fileHandlersByFilename = new(5_000);

        public static FileHandlerItem CreateRootHandler(string filepath, int size = 4096)
        {
            return CreateHandler(filepath, PmExtensions.PmRootFile, size);
        }

        public static FileHandlerItem CreateHashHandler(string filepath, int size = 4096)
        {
            return CreateHandler(filepath, PmExtensions.PmHash, size);
        }

        public static FileHandlerItem CreateInternalObjectHandler(string filepath, int size = 4096)
        {
            return CreateHandler(filepath, PmExtensions.PmInternalFile, size);
        }

        public static FileHandlerItem CreateListHandler(string filepath, int size = 4096)
        {
            return CreateHandler(filepath, PmExtensions.PmListItem, size);
        }

        public static FileHandlerItem CreateHandler(string filepath, string extension, int size = 4096)
        {
            return CreateHandler(PmExtensions.AddExtension(filepath, extension), size);
        }

        public static FileHandlerItem CreateHandler(string filepath, int size = 4096)
        {
            if (_fileHandlersByFilename.TryGetValue(filepath, out var pmCached))
            {
                if (pmCached.FileBasedStream.IsDisposed)
                {
                    ReleaseObjectFromMemory(pmCached.FileBasedStream);
                    return CreateHandler(filepath, size);
                }
                return pmCached;
            }
            var pm = new PmStream(filepath, size);
            return RegisterNewHandler(pm);
        }

        private static FileHandlerItem RegisterNewHandler(FileBasedStream fileBasedStream)
        {
            var fileHandlerItem = new FileHandlerItem(fileBasedStream);
            try
            {
                _fileHandlersByFilename.TryAdd(
                    fileBasedStream.FilePath,
                    fileHandlerItem);
            }
            catch (ApplicationException ex)
            {
                _fileHandlersByFilename.CleanOldValues(_fileHandlersByFilename.Capacity / 2);
            }

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
                    if (_fileHandlersByFilename.TryRemove(filepath, out var removedHandler))
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
