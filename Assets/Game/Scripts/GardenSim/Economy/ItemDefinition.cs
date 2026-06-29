using UnityEngine;

namespace GardenSim
{
    /// <summary>
    /// Designer-authored definition of a tradeable / collectable item (e.g. a Carrot, a Tomato, a seed).
    /// One asset per item type. Referenced by produce instances, the inventory store and the shop.
    /// </summary>
    [CreateAssetMenu(fileName = "Item", menuName = "GardenSim/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        [Tooltip("Stable unique id used for lookups and optional save data. Falls back to the asset name.")]
        public string id;

        public string displayName = "Item";
        public Sprite icon;

        [Header("Economy")]
        [Tooltip("Coins the player pays to buy one unit from the vendor.")]
        public int buyPrice = 10;
        [Tooltip("Coins the player receives when selling one unit to the vendor.")]
        public int sellPrice = 5;
        public bool canBuy = true;
        public bool canSell = true;

        [Header("Physical (optional)")]
        [Tooltip("Grabbable prefab that represents this item in the world. Used as an example for harvesting " +
                 "and (optionally) spawned when the item is bought.")]
        public GameObject worldPrefab;

        /// <summary>Human readable name, falling back to the asset name when blank.</summary>
        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;

        /// <summary>Stable id, falling back to the asset name when blank.</summary>
        public string Id => string.IsNullOrEmpty(id) ? name : id;
    }
}
