using System;
using System.Collections.Generic;
using System.Linq;

namespace Inspectors.DataBinding
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ItemVisualsAttribute : Attribute
    {
        public readonly List<Type> Types;

        public ItemVisualsAttribute(params Type[] types)
        {
            Types = types.Distinct().Where(typeof(ItemVisuals).IsAssignableFrom).ToList();
        }
    }
}