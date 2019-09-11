namespace Stashie
{
    public class ElderItemFiler : IIFilter
    {
        public bool isElder;
        public bool CompareItem(ItemData itemData) => itemData.isElder == isElder;
    }
}