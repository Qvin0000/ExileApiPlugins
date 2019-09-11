using System;

namespace Stashie
{
    public class FilterParameterCompare : IIFilter
    {
        public int CompareInt;
        public string CompareString;
        public Func<ItemData, bool> CompDeleg;
        public Func<ItemData, int> IntParameter;
        public Func<ItemData, string> StringParameter;

        public bool CompareItem(ItemData itemData)
        {
            return CompDeleg(itemData);
        }
    }
}
