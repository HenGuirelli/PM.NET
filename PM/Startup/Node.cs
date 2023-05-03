namespace PM.Startup
{
    public class Node : IEquatable<Node>
    {
        public string Filename { get; set; }
        public string Filepath { get; set; }

        private readonly List<Node> _childrens = new List<Node>();

        public Node? GetChild(string file)
        {
            foreach (var children in _childrens)
            {
                if (children.Filename == file) return this;
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
            var child = GetChild(node.Filename);
            return child != null;
        }

        public bool Equals(Node? other)
        {
            if (other is null) return false;
            return Filename == other.Filename;
        }
    }
}
