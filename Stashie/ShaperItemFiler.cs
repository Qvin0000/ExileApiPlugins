namespace Stashie
{
    public class ShaperItemFiler : IIFilter
    {
        public bool isShaper;

        public bool CompareItem(ItemData itemData) => itemData.isShaper == isShaper;
    }
}