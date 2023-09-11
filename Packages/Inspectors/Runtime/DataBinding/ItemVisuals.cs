using System;

namespace Inspectors.DataBinding
{
    [Serializable]
    public abstract class ItemVisuals
    {
        /// <summary>
        /// The containing <see cref="ItemView"/>.
        /// </summary>
        [NonSerialized] public ItemView View = null;
    }
}