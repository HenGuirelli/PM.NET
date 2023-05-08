using PM.Configs;
using PM.Core;
using PM.Factories;
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
            //LoadAllClasses();

            // 2. Identify all root objects
            var internalsFilenames = new HashSet<string>
            {
                Path.Combine(
                    PmGlobalConfiguration.PmInternalsFolder,
                    PointersToPersistentObjects.PmFileName)
            };
            var pmFiles = Directory
                .GetFiles(PmGlobalConfiguration.PmInternalsFolder)
                .Where(it => it.EndsWith(".root") || it.EndsWith(".pm"))
                .Except(internalsFilenames);

            var roots = pmFiles.Where(it => it.EndsWith(".root"));

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
                    if (FileHandlerManager.CloseAndDiscard(pmFile))
                    {
                        File.Delete(pmFile);
                    }
                }
            }

            return _pointerCount;
        }

        private void LoadAllClasses()
        {
            var classTypes = GetAllLoadedClasses();
            foreach (var classType in classTypes)
            {
                _classesWithHash[ClassHashCodeCalculator.GetHashCode(classType)] = classType;
            }
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
            var pmCSharpDefinedTypes = new PmCSharpDefinedTypes(pm);
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
