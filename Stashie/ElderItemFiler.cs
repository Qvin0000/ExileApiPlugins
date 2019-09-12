namespace Stashie
{
    public class ElderItemFiler : IIFilter
    {
        public bool isElder;

        public bool CompareItem(ItemData itemData)
        {
            return itemData.isElder == isElder;
        }
    }
}
