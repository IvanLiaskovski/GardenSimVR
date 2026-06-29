using System;
using System.Collections.Generic;
using UnityEngine;

namespace GardenSim
{
    /// <summary>
    /// Single source of truth for the player's owned items (counts per <see cref="ItemDefinition"/>)
    /// and coin balance. A scene singleton accessible via <see cref="Instance"/>. Fires
    /// <see cref="OnChanged"/> after every mutation so UI views can refresh.
    ///
    /// Integration seam for harvesting: call <c>Inventory.Instance.Add(itemDef, qty)</c>, or spawn a
    /// prefab carrying a <see cref="ProduceItemRef"/> for the player to deposit into the basket.
    /// </summary>
    public class Inventory : MonoBehaviour
    {
        public static Inventory Instance { get; private set; }

        [SerializeField] private int startingCoins = 50;

        /// <summary>Current coin balance.</summary>
        public int Coins { get; private set; }

        /// <summary>Raised after any change to items or coins.</summary>
        public event Action OnChanged;

        private readonly Dictionary<ItemDefinition, int> _counts = new Dictionary<ItemDefinition, int>();

        /// <summary>Read-only view of owned item counts.</summary>
        public IReadOnlyDictionary<ItemDefinition, int> Counts => _counts;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Inventory: a second instance was found and destroyed.", this);
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Coins = startingCoins;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ---- Queries ----------------------------------------------------------------

        public int GetCount(ItemDefinition item)
            => item != null && _counts.TryGetValue(item, out var n) ? n : 0;

        // ---- Item mutation ----------------------------------------------------------

        public void Add(ItemDefinition item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return;
            _counts[item] = GetCount(item) + quantity;
            OnChanged?.Invoke();
        }

        public bool Remove(ItemDefinition item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;
            int current = GetCount(item);
            if (current < quantity) return false;
            SetCount(item, current - quantity);
            OnChanged?.Invoke();
            return true;
        }

        // ---- Currency ---------------------------------------------------------------

        public void Earn(int amount)
        {
            if (amount <= 0) return;
            Coins += amount;
            OnChanged?.Invoke();
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0 || Coins < amount) return false;
            Coins -= amount;
            OnChanged?.Invoke();
            return true;
        }

        // ---- Trading ----------------------------------------------------------------

        public bool CanSell(ItemDefinition item, int quantity = 1)
            => item != null && item.canSell && GetCount(item) >= quantity;

        /// <summary>Removes stock and pays out, in a single change notification.</summary>
        public bool Sell(ItemDefinition item, int quantity = 1)
        {
            if (quantity <= 0 || !CanSell(item, quantity)) return false;
            SetCount(item, GetCount(item) - quantity);
            Coins += item.sellPrice * quantity;
            OnChanged?.Invoke();
            return true;
        }

        public bool CanBuy(ItemDefinition item, int quantity = 1)
            => item != null && item.canBuy && quantity > 0 && Coins >= item.buyPrice * quantity;

        /// <summary>Charges coins and grants stock, in a single change notification.</summary>
        public bool Buy(ItemDefinition item, int quantity = 1)
        {
            if (!CanBuy(item, quantity)) return false;
            Coins -= item.buyPrice * quantity;
            _counts[item] = GetCount(item) + quantity;
            OnChanged?.Invoke();
            return true;
        }

        // ---- Internal ---------------------------------------------------------------

        private void SetCount(ItemDefinition item, int value)
        {
            if (value <= 0) _counts.Remove(item);
            else _counts[item] = value;
        }
    }
}
