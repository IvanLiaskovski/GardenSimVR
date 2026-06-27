using System.Collections.Generic;
using UnityEngine;

namespace GardenSim
{
    /// <summary>
    /// The set of items a vendor offers. Order here is the order shown in the shop panel.
    /// </summary>
    [CreateAssetMenu(fileName = "ShopCatalog", menuName = "GardenSim/Shop Catalog")]
    public class ShopCatalog : ScriptableObject
    {
        public List<ItemDefinition> items = new List<ItemDefinition>();
    }
}
