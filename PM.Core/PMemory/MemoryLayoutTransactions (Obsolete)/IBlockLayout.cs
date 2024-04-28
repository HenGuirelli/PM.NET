﻿using PM.Core.PMemory.FileFields;

namespace PM.Core.PMemory.MemoryLayoutTransactions
{
    public interface IBlockLayout
    {
        OrderField Order { get; }
        void ApplyInOriginalFile(PmCSharpDefinedTypes transactionFile, PersistentAllocatorLayout originalFile);
    }
}
