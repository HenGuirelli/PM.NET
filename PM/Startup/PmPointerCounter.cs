using PM.Configs;
using PM.Core;
using PM.Factories;
using PM.PmContent;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Xml.Linq;

namespace PM.Startup
{
    public class Node : IEquatable<Node>
    {
        public string File { get; set; }

        private readonly List<Node> _childrens = new List<Node>();

        public Node? GetChild(string file)
        {
            foreach (var children in _childrens)
            {
                if (children.File == file) return this;
                return children.GetChild(file);
            }
            return null;
        }

        public void AddChild(Node node)
        {
            _childrens.Add(node);
        }

        public bool HasChildren(string filename)
        {
            var child = GetChild(filename);
            return child != null;
        }

        public bool HasChildren(Node node)
        {
            var child = GetChild(node.File);
            return child != null;
        }

        public bool Equals(Node? other)
        {
            if (other is null) return false;
            return File == other.File;
        }
    }

    public class ReferenceTree
    {
        private readonly List<Node> _roots = new List<Node>();

        public bool Contains(string filename)
        {
            foreach(var root in _roots)
            {
                if (root.HasChildren(filename)) return true;
            }
            return false;
        }

        public void AddRoot(Node root)
        {
            _roots.Add(root);
        }
    }

    public class PmPointerCounter : IPmPointerCounter
    {
        private readonly ReferenceTree _referenceTree = new();
        private readonly Dictionary<int, Type> classesWithHash = new();
        private readonly Dictionary<ulong, ulong> _pointerCount = new();

        public IDictionary<ulong, ulong> MapPointers(string folder)
        {
            // 1. Get All Classes
            var classTypes = GetAllLoadedClasses();
            foreach (var classType in classTypes)
            {
                classesWithHash.Add(ClassHashCodeCalculator.GetHashCode(classType), classType);
            }

            // 2. Identify all root objects
            var roots = new List<string>();
            var pmFiles = Directory
                .GetFiles(PmGlobalConfiguration.PmInternalsFolder)
                .Where(it => !PmFileSystem.FileIsSymbolicLink(it));
            foreach (var pmFile in pmFiles)
            {
                var pm = PmFactory.CreatePm(pmFile, 4096);
                var pmCSharpDefinedTypes = new PmCSharpDefinedTypes(pm);
                if (pmCSharpDefinedTypes.ReadBool(offset: sizeof(int)))
                {
                    roots.Add(pmFile);
                }
            }

            // 3. Create reference tree
            foreach(var rootFile in roots)
            {
                var rootNode = new Node { File = rootFile };
                _referenceTree.AddRoot(rootNode);
                CreateTreeByNode(rootNode);
            }

            // 4. Verify the pm files that not in reference tree
            foreach (var pmFile in pmFiles)
            {
                if (!_referenceTree.Contains(pmFile))
                {
                    File.Delete(pmFile);
                }
            }

            return _pointerCount;
        }

        void CreateTreeByNode(Node node)
        {
            var pm = PmFactory.CreatePm(node.File, 4096);
            var pmCSharpDefinedTypes = new PmCSharpDefinedTypes(pm);
            var classHash = pmCSharpDefinedTypes.ReadInt();

            if (classesWithHash.TryGetValue(classHash, out var type))
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
                        var child = new Node { File = pointer.ToString() };
                        node.AddChild(child);
                        _pointerCount[pointer]++;
                        CreateTreeByNode(child);
                    }
                }
            }
        }

        static Type[] GetAllLoadedClasses()
        {
            List<Type> classes = new List<Type>();
            Process currentProcess = Process.GetCurrentProcess();
            ProcessModuleCollection modules = currentProcess.Modules;

            foreach (ProcessModule module in modules)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(module.FileName);
                    Type[] types = assembly.GetTypes();

                    classes.AddRange(types);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro ao carregar módulo: " + module.FileName);
                    Console.WriteLine(ex.Message);
                }
            }

            return classes.ToArray();
        }
    }
}
