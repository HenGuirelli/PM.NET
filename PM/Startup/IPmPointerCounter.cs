namespace PM.Startup
{
    public interface IPmPointerCounter
    {
        IDictionary<ulong, ulong> Collect(string folder);
    }
}
