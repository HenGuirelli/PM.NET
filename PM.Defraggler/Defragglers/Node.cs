namespace PM.Defraggler.Defragglers
{
    internal class Node
    {
        public List<RegionsInUse> RegionsReference { get; } = new();
        public bool Visited { get; set; } = false;
        private readonly List<Node> _childrens = new();

        public Node? GetChild(RegionsInUse region)
        {
            foreach (var children in _childrens)
            {
                // Search region inner RegionsReference
                foreach (var regionReference in RegionsReference)
                {
                    if (region.Equals(regionReference)) return this;
                }
                return children.GetChild(region);
            }
            return null;
        }

        public void AddChild(Node node)
        {
            _childrens.Add(node);
        }
    }
}
