using PM.Configs;
using PM.Core;
using PM.Managers;
using System.Reflection;

namespace PM
{
    internal class PmFileSystem
    {
        private static bool _useFileSystem => PmTargets.FileBasedTarget.HasFlag(PmGlobalConfiguration.PmTarget);

        public static string GetTargetOfSymbolicLink(string symlink)
        {
            var fi = new FileInfo(symlink);
            return fi.LinkTarget ?? throw new ApplicationException($"Null link target of symlink '{symlink}'");
        }

        public static string CreateSymbolicLinkInInternalsFolder(string symlink, string targetSymlink)
        {
            var pointer = Path.Combine(PmGlobalConfiguration.PmInternalsFolder, targetSymlink) + ".pm";
            CreateSymbolicLink(symlink, pointer);
            return pointer;
        }

        public static bool FileIsSymbolicLink(string symlink)
        {
            if (_useFileSystem)
            {
                return (File.GetAttributes(symlink) & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
            }
            throw new NotImplementedException();
        }

        public static void CreateSymbolicLink(string symlink, string targetSymlink)
        {
            if (_useFileSystem)
            {
                File.CreateSymbolicLink(symlink, targetSymlink);
                return;
            }
            throw new NotImplementedException();
        }

        public static bool FileExists(string filepath)
        {
            if (_useFileSystem)
            {
                return File.Exists(filepath);
            }
            throw new NotImplementedException();
        }

        internal static void FileMove(string fileOrigin, string fileDest)
        {
            if (_useFileSystem)
            {
                File.Move(fileOrigin, fileDest);
                return;
            }
            throw new NotImplementedException();
        }

        internal static void DeleteFile(string filepath)
        {
            if (_useFileSystem)
            {
                File.Delete(filepath);
                return;
            }
            throw new NotImplementedException();
        }

        internal static void ResizeFile(string filepath, long fileBytes)
        {
            using var fs = new FileStream(
                filepath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None);
            fs.SetLength(fileBytes);
        }

        internal static long GetFileSize(string filepath)
        {
            if (FileIsSymbolicLink(filepath))
            {
                var target = GetTargetOfSymbolicLink(filepath);
                return GetFileSize(target);
            }
            using var fs = new FileStream(
                filepath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);
            return fs.Length;
        }
    }
}
