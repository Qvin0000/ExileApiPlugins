namespace Stashie
{
    public class IdentifiedItemFilter : IIFilter
    {
        public bool BIdentified;

        public bool CompareItem(ItemData itemData) => itemData.BIdentified == BIdentified;
    }
}