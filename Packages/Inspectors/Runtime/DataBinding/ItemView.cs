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

        [SerializeReference, SerializeField] private ItemVisuals visuals = default;
    }
}