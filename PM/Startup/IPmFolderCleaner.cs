namespace PM.Startup
{
    public interface IPmFolderCleaner
    {
        IDictionary<ulong, ulong> Collect(string folder);
    }
}
