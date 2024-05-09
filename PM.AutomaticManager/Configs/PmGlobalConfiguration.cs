using PM.Core;

namespace PM.AutomaticManager.Configs
{
    public static class PmGlobalConfiguration
    {
        public static PmTargets PmTarget { get; set; } = PmTargets.PM;
        public static int CollectFileInterval { get; set; } = 120000;
        public static string PmMemoryFilePath { get; set; } = Path.Combine(PmInternalsFolder, "PM.NET.FileMemory");
        public static string PmMemoryFileTransactionPath { get; set; } = Path.Combine(PmInternalsFolder, "PM.NET.FileMemory.Transaction");

        #region PmInternalsFolder
        private static string _pmInternalsFolder = Path.Combine("pm", "internals");
        private static bool _pmInternalsFolderCreated = false;
        public static string PmInternalsFolder
        {
            get
            {
                if (!_pmInternalsFolderCreated && !Directory.Exists(_pmInternalsFolder))
                {
                    Directory.CreateDirectory(_pmInternalsFolder);
                    _pmInternalsFolderCreated = true;
                }
                return _pmInternalsFolder;
            }
            set
            {
                if (!_pmInternalsFolderCreated && !Directory.Exists(_pmInternalsFolder))
                {
                    Directory.CreateDirectory(_pmInternalsFolder);
                    _pmInternalsFolderCreated = true;
                }
                _pmInternalsFolder = value;
            }
        }
        #endregion PmInternalsFolder

        #region PmTransactionFolder
        public static string PmTransactionFolderName { get; set; } = "transactions";
        private static string _pmTransactionPathFolder => Path.Combine(PmInternalsFolder, PmTransactionFolderName);
        private static bool _pmTransactionFolderCreated = false;
        public static string PmTransactionFolder
        {
            get
            {
                if (!_pmTransactionFolderCreated && !Directory.Exists(_pmTransactionPathFolder))
                {
                    Directory.CreateDirectory(_pmTransactionPathFolder);
                    _pmTransactionFolderCreated = true;
                }
                return _pmTransactionPathFolder;
            }
            set
            {
                if (!_pmTransactionFolderCreated && !Directory.Exists(_pmTransactionPathFolder))
                {
                    Directory.CreateDirectory(_pmTransactionPathFolder);
                    _pmTransactionFolderCreated = true;
                }
                PmTransactionFolderName = value;
            }
        }
        #endregion PmTransactionFolder

        public static PmLogger Logger { get; set; } = new PmLogger();
        public static bool PersistentGCEnable { get; set; } = true;
        public static int ProxyCacheCount { get; set; } = 500;
    }
}
