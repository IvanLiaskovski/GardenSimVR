using UnityEngine;

namespace GardenSim
{
    /// <summary>
    /// Marks a grabbable world object as a piece of produce backed by an <see cref="ItemDefinition"/>.
    ///
    /// This is the integration seam for harvesting: when a crop is harvested, instantiate a prefab
    /// carrying this component (plus a Rigidbody, Collider and XRGrabInteractable). The player can then
    /// grab it and drop it into the basket, which deposits <see cref="quantity"/> of
    /// <see cref="definition"/> into the <see cref="Inventory"/>.
    /// </summary>
    public class ProduceItemRef : MonoBehaviour
    {
        public ItemDefinition definition;

        [Min(1)]
        public int quantity = 1;
    }
}
