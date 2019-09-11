using System.Collections.Generic;
using JM.LinqFaster;

namespace Stashie
{
    public class BaseFilter : IIFilter
    {
        public List<IIFilter> Filters { get; } = new List<IIFilter>();
        public bool BAny { get; set; }

        public bool CompareItem(ItemData itemData)
        {
            return BAny ? Filters.AnyF(x => x.CompareItem(itemData)) : Filters.AllF(x => x.CompareItem(itemData));
        }
    }
}
