namespace Stashie
{
    public class CustomFilter : BaseFilter
    {
        public string Name { get; set; }
        public ListIndexNode StashIndexNode { get; set; }
        public string SubmenuName { get; set; }
        public string Commands { get; set; }
        public int Index { get; set; }
        public bool AllowProcess => StashIndexNode.Index != -1;
    }
}
