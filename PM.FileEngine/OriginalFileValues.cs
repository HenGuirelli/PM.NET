using PM.FileEngine.FileFields;

namespace PM.FileEngine
{
    public class OriginalFileValues
    {
        public const CommitState HeaderCommitByte = CommitState.Commited;
        public const uint HeaderStartBlocksOffset = 9;
        public const uint HeaderVersionOffset = 1;
    }
}
