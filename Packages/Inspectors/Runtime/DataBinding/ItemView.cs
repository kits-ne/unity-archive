using System;
using UnityEngine;

namespace Inspectors.DataBinding
{
    public abstract class ItemView : MonoBehaviour
    {
        public bool HasVisuals => Visuals != null;

        public ItemVisuals Visuals
        {
            get
            {
                if (visuals != null)
                {
                    visuals.View = this;
                }

                return visuals;
            }
        }
        public Type TypeOfVisuals => HasVisuals ? Visuals.GetType() : null;

        [SerializeReference, SerializeField] private ItemVisuals visuals = default;

        [ContextMenu(nameof(ClearVisuals))]
        private void ClearVisuals()
        {
            visuals = null;
        }
    }
}