namespace Stashie
{
    public class FracturedItemFiler : IIFilter
    {
        public bool isFractured;
        public bool CompareItem(ItemData itemData) => itemData.isFractured == isFractured;
    }
}