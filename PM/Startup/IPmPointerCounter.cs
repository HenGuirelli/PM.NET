namespace PM.Startup
{
    public interface IPmPointerCounter
    {
        IDictionary<ulong, ulong> MapPointers(string folder);
    }
}
