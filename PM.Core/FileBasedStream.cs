﻿namespace PM.Core
{
    public abstract class FileBasedStream : Stream
    {
        public string FilePath { get; protected set; } = string.Empty;

        public virtual void Delete()
        {
            Dispose();
            File.Delete(FilePath);
        }

        public abstract void Resize(int size);
    }
}
