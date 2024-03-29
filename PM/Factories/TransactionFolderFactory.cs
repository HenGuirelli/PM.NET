﻿using PM.Configs;
using PM.Core;
using PM.Transactions;

namespace PM.Factories
{
    internal class TransactionFolderFactory
    {
        public static IFileSystemHelper Create()
        {
            if (PmTargets.FileBasedTarget.HasFlag(PmGlobalConfiguration.PmTarget))
            {
                return new FileSystemHelper();
            }
            throw new ArgumentException(nameof(PmGlobalConfiguration.PmTarget));
        }
    }
}
