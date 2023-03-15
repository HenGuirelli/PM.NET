using PM.Configs;
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

        public static void CreateSymbolicLinkInInternalsFolder(string symlink, string targetSymlink)
        {
            CreateSymbolicLink(
                symlink, 
                Path.Combine(PmGlobalConfiguration.PmInternalsFolder, targetSymlink));
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
    }
}
