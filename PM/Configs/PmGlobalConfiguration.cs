using PM.Core;

namespace PM.Configs
{
    public static class PmGlobalConfiguration
    {
        public static PmTargets PmTarget { get; set; } = PmTargets.PM;
        public static int CollectFileInterval { get; set; } = 120000;

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
        private static string _pmTransactionFolder = Path.Combine(PmInternalsFolder, "transactions");
        private static bool _pmTransactionFolderCreated = false;
        public static string PmTransactionFolder
        {
            get
            {
                if (!_pmTransactionFolderCreated && !Directory.Exists(_pmTransactionFolder))
                {
                    Directory.CreateDirectory(_pmTransactionFolder);
                    _pmTransactionFolderCreated = true;
                }
                return _pmTransactionFolder;
            }
            set
            {
                if (!_pmTransactionFolderCreated && !Directory.Exists(_pmTransactionFolder))
                {
                    Directory.CreateDirectory(_pmTransactionFolder);
                    _pmTransactionFolderCreated = true;
                }
                _pmTransactionFolder = value;
            }
        }
        #endregion PmTransactionFolder

        public static PmLogger Logger { get; set; } = new PmLogger();
    }
}
