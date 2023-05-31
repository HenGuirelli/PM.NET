namespace PM
{
    public static class PmExtensions
    {
        public const string PmInternalFile = ".pm";
        public const string PmRootFile = ".root";
        public const string PmList = ".pmlist";
        public const string PmHash = ".hash";
        public const string PmListItem = ".pmlistitem";

        public static string AddExtension(string filename, string extension)
            => filename.EndsWith(extension) ? filename : filename + extension;
    }
}
