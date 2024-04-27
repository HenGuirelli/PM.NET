
namespace PM.Core.PMemory.FileFields
{
    public class CommitByteField : ByteFiled
    {
        public CommitState State
        {
            get
            {
                return _state;
            }
            set
            {
                Value = (byte)value;
                _state = value;
            }
        }
        private CommitState _state = CommitState.NotCommited;

        public CommitByteField(int offset, CommitState? commitState = null)
        {
            Offset = offset;
            if (commitState.HasValue) State = commitState.Value;
        }

        internal void UnCommit()
        {
            State = CommitState.NotCommited;
        }

        internal void Commit()
        {
            State = CommitState.Commited;
        }
    }
}
