using System;

namespace Stashie
{
    public class FilterParameterCompare : IIFilter
    {
        public string CompareString;
        public int CompareInt;
        public Func<ItemData, string> StringParameter;
        public Func<ItemData, int> IntParameter;
        public Func<ItemData, bool> CompDeleg;

        public bool CompareItem(ItemData itemData) => CompDeleg(itemData);
    }
}