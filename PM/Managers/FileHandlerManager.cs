using PM.Collections.Internals;
using PM.Core;
using Serilog;

namespace PM.Managers
{
    public class FileHandlerItem
    {
        public FileBasedStream FileBasedStream { get; }
        private ulong _filePointerReference;
        public ulong FilePointerReference
        {
            get => _filePointerReference;
            set
            {
                Log.Verbose("file {filename} set PointerReference to {pointerReference}",
                    FileBasedStream.FilePath, value);
                _filePointerReference = value;
            }
        }

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
                if (pmCached.FileBasedStream.IsClosed)
                {
                    pmCached.FileBasedStream.Open();
                    return CreateHandler(filepath, size);
                }
                return pmCached;
            }
            var pm = new PmStream(filepath, size);
            return RegisterNewHandler(pm);
        }

        public static FileHandlerItem RegisterNewHandler(FileBasedStream fileBasedStream)
        {
            var fileHandlerItem = new FileHandlerItem(fileBasedStream);
            try
            {
                _fileHandlersByFilename.Add(
                    fileBasedStream.FilePath,
                    fileHandlerItem);
            }
            catch (CollectionLimitReachedException)
            {
                var streamsToClose =
                    //_fileHandlersByFilename.CleanOldValues((int)(_fileHandlersByFilename.Capacity * .2));
                    _fileHandlersByFilename.CleanOldValues(_fileHandlersByFilename.Capacity / 2);

                var itensCloseds = new List<string>(streamsToClose.Count());
                foreach (var item in streamsToClose)
                {
                    try
                    {
                        item.Close();
                        itensCloseds.Add(item.FilePath);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "File {FilePath} failed to closed", item.FilePath);
                    }
                    //Log.Verbose("Files closed: {files}", itensCloseds);
                }


                _fileHandlersByFilename.Add(
                    fileBasedStream.FilePath,
                    fileHandlerItem);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error on RegisterNewHandler");
            }

            return fileHandlerItem;
        }

#if DEBUG
        public static void CloseAllHandlers()
        {
            var fileHandlers = _fileHandlersByFilename.CleanOldValues(_fileHandlersByFilename.Count);
            foreach(var handler in fileHandlers)
            {
                CloseAndRemoveFile(handler);
            }
        }
#endif

        public static bool CloseAndRemoveFile(FileBasedStream fileBasedStream)
        {
            return CloseAndRemoveFile(fileBasedStream.FilePath);
        }

        public static bool CloseAndRemoveFile(string filepath)
        {
            if (CloseFile(filepath))
            {
                File.Delete(filepath);
                Log.Verbose("File {filepath} deleted successfully", filepath);
                return true;
            }
            else
            {
                try
                {
                    File.Delete(filepath);
                    Log.Verbose("File {filepath} deleted successfully (handler already closed)", filepath);
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error on delete file {filepath} on disk", filepath);
                }
            }
            return false;
        }

        public static bool CloseFile(string filepath)
        {
            if (_fileHandlersByFilename.TryGetValue(filepath, out var fileHandlerItem))
            {
                // Only reference from this class
                if (fileHandlerItem.FilePointerReference == 0)
                {
                    if (_fileHandlersByFilename.TryRemove(filepath, out var removedHandler))
                    {
                        removedHandler.FileBasedStream.Close();
                        Log.Verbose("File handler from {filename} closed successfully", filepath);
                        return true;
                    }
                }
                else
                {
                    Log.Debug(
                        "Method {method} called but file handler from {filename} " +
                        "yet have {reference} references",
                        nameof(CloseFile),
                        filepath,
                        fileHandlerItem.FilePointerReference);
                }
            }
            else
            {
                Log.Debug(
                    "Argument filepath {filepath} not found to close handler",
                    filepath);
            }
            return false;
        }
    }
}
