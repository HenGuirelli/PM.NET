namespace PM.Core.PMemory.FileFields
{
    public interface IField<out T>
    {
        int Offset { get; }
        T Value { get; }
    }

    public abstract class ByteFiled : IField<byte>
    {
        public const int Size = 1;
        public virtual int Offset { get; internal set; }
        public virtual byte Value { get; internal set; }
    }

    public abstract class UInt16Filed : IField<UInt16>
    {
        public virtual int Offset { get; internal set; }
        public virtual UInt16 Value { get; internal set; }
    }

    public abstract class UInt32Filed : IField<UInt32>
    {
        public virtual int Offset { get; internal set; }
        public virtual UInt32 Value { get; internal set; }
    }
}
