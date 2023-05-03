namespace PM.Startup
{
    public class ReferenceTree
    {
        private readonly List<Node> _roots = new List<Node>();

        public bool Contains(string filename)
        {
            foreach(var root in _roots)
            {
                if (root.Filename == filename) return true;
                if (root.HasChildren(filename)) return true;
            }
            return false;
        }

        public void AddRoot(Node root)
        {
            _roots.Add(root);
        }
    }
}
