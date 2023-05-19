using PM.Configs;
using PM.Core;
using PM.Managers;
using PM.PmContent;
using System.Reflection;

namespace PM.Startup
{
    public class PmFolderCleaner : IPmFolderCleaner
    {
        private readonly ReferenceTree _referenceTree = new();
        private readonly Dictionary<int, Type> _classesWithHash = new();
        private readonly Dictionary<ulong, ulong> _pointerCount = new();

        public IDictionary<ulong, ulong> Collect(string folder)
        {
            // 1. Get All Classes
            LoadClassesAndHash();

            // 2. Identify all root objects
            var internalsFilenames = new HashSet<string>
            {
                Path.Combine(
                    PmGlobalConfiguration.PmInternalsFolder,
                    PointersToPersistentObjects.PmFileName)
            };
            var allPmFiles = Directory
                .GetFiles(PmGlobalConfiguration.PmInternalsFolder)
                .Where(it => 
                        it.EndsWith(".root")   || 
                        it.EndsWith(".pm")     ||
                        it.EndsWith(".pmlist") ||
                        it.EndsWith(".pmlistitem")
                )
                .Except(internalsFilenames);
            var listFiles = new List<string>();
            var listItemFiles = new List<string>();
            var roots = new List<string>();
            var pmFiles = new List<string>();

            foreach(var item in allPmFiles)
            {
                if (item.EndsWith(".root")) roots.Add(item);
                else if (item.EndsWith(".pm")) pmFiles.Add(item);
                else if (item.EndsWith(".pmlist")) listFiles.Add(item);
                else if (item.EndsWith(".pmlistitem")) listItemFiles.Add(item);
            }

            // 3. Create reference tree
            foreach (var rootFile in roots)
            {
                var rootNode = new Node
                {
                    Filename = Path.GetFileNameWithoutExtension(rootFile),
                    Filepath = rootFile
                };
                _referenceTree.AddRoot(rootNode);
                CreateTreeByNode(rootNode);
            }

            // 4. Verify the pm files that not in reference tree
            foreach (var pmFile in pmFiles)
            {
                if (!_referenceTree.Contains(Path.GetFileNameWithoutExtension(pmFile)))
                {
                    FileHandlerManager.CloseAndRemoveFile(pmFile);
                }
            }

            // 5. Clear .pmlistitem files
            var allPmListItemReferences = new HashSet<ulong>();
            foreach (var pmlist in listFiles)
            {
                var referenceFiles = GetReferenceFilesFromList(pmlist);
                foreach(var referenceFile in referenceFiles)
                {
                    allPmListItemReferences.Add(referenceFile);
                }
            }

            foreach (var listItemFile in listItemFiles)
            {
                var parsedListItemFile = ulong.Parse(Path.GetFileNameWithoutExtension(listItemFile));
                if (!allPmListItemReferences.Contains(parsedListItemFile))
                {
                    FileHandlerManager.ReleaseObjectFromMemory(listItemFile);
                    FileHandlerManager.CloseAndRemoveFile(listItemFile);
                }
            }

            return _pointerCount;
        }

        private List<ulong> GetReferenceFilesFromList(string pmlist)
        {
            var pm = FileHandlerManager.CreateHandler(pmlist);
            var pmCSharpDefinedTypes = new PmCSharpDefinedTypes(pm.FileBasedStream);

            var references = new List<ulong>();
            var offset = 0;
            ulong pointer = pmCSharpDefinedTypes.ReadULong(offset);
            while (pointer != 0)
            {
                offset += sizeof(ulong);
                pointer = pmCSharpDefinedTypes.ReadULong(offset);
                references.Add(pointer);
            }
            return references;
        }

        private void LoadClassesAndHash()
        {
            var allClassHashes = ClassHashManager.Instance.All;
            foreach (var item in allClassHashes)
            {
                if (string.IsNullOrWhiteSpace(item.AssemblyName) ||
                    string.IsNullOrWhiteSpace(item.SerializedType))
                {
                    continue;
                }

                var assembly = Assembly.Load(item.AssemblyName);
                var type = assembly.GetType(item.SerializedType);
                if (type != null)
                {
                    _classesWithHash[item.Hash] = type;
                }
            }
        }

        void CreateTreeByNode(Node node)
        {
            var pm = FileHandlerManager.CreateHandler(node.Filepath);
            var pmCSharpDefinedTypes = new PmCSharpDefinedTypes(pm.FileBasedStream);
            var classHash = pmCSharpDefinedTypes.ReadInt();

            if (_classesWithHash.TryGetValue(classHash, out var type))
            {
                var header = new PmHeader(
                    type,
                    pmCSharpDefinedTypes.ReadBool(offset: sizeof(int)));
                var objectPropertiesInfoMapper = new ObjectPropertiesInfoMapper(type, header);

                foreach (var prop in type.GetProperties())
                {
                    var pmType = SupportedTypesTable.Instance.GetPmType(prop.PropertyType);
                    if (!SupportedTypesTable.Instance.IsPrimitive(pmType.ID))
                    {
                        var offsetReferenceType = objectPropertiesInfoMapper.GetOffSet(prop);
                        var pointer = pmCSharpDefinedTypes.ReadULong(offsetReferenceType);
                        if (pointer == 0) return;

                        var child = new Node
                        {
                            Filename = pointer.ToString(),
                            Filepath = Path.Combine(PmGlobalConfiguration.PmInternalsFolder, pointer.ToString()) + ".pm"
                        };
                        node.AddChild(child);
                        AddPointerCount(pointer);

                        CreateTreeByNode(child);
                    }
                }
            }
        }

        private void AddPointerCount(ulong pointer)
        {
            if (!_pointerCount.ContainsKey(pointer))
            {
                _pointerCount[pointer] = 0;
            }
            _pointerCount[pointer]++;
        }

        static List<Type> GetAllLoadedClasses()
        {
            List<Type> types = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type[] assemblyTypes = assembly.GetTypes();
                foreach (Type type in assemblyTypes)
                {
                    if (type.Assembly == assembly && !type.IsAbstract && type.IsClass)
                    {
                        types.Add(type);
                    }
                }
            }
            return types;
        }
    }
}
