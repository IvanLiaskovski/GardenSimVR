using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GardenSim
{
    /// <summary>
    /// World-space view of the <see cref="Inventory"/>: a coin total plus one <see cref="InventorySlotUI"/>
    /// per owned item. Refreshes automatically whenever the inventory changes.
    /// </summary>
    public class InventoryPanelUI : MonoBehaviour
    {
        [SerializeField] private Inventory inventory;
        [SerializeField] private Transform slotContainer;
        [SerializeField] private InventorySlotUI slotPrefab;
        [SerializeField] private TMP_Text coinsLabel;
        [Tooltip("Optional 'inventory is empty' hint, shown only when nothing is owned.")]
        [SerializeField] private GameObject emptyHint;

        private readonly List<InventorySlotUI> _slots = new List<InventorySlotUI>();

        private void Start()
        {
            if (inventory == null) inventory = Inventory.Instance;
            if (inventory == null)
            {
                Debug.LogWarning("InventoryPanelUI: no Inventory found.", this);
                return;
            }

            inventory.OnChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (inventory != null) inventory.OnChanged -= Refresh;
        }

        private void Refresh()
        {
            if (inventory == null) return;

            if (coinsLabel != null) coinsLabel.text = inventory.Coins.ToString();

            var owned = new List<KeyValuePair<ItemDefinition, int>>();
            foreach (var kv in inventory.Counts)
                if (kv.Key != null && kv.Value > 0) owned.Add(kv);

            EnsureSlots(owned.Count);
            for (int i = 0; i < _slots.Count; i++)
            {
                bool active = i < owned.Count;
                _slots[i].gameObject.SetActive(active);
                if (active) _slots[i].Set(owned[i].Key, owned[i].Value);
            }

            if (emptyHint != null) emptyHint.SetActive(owned.Count == 0);
        }

        private void EnsureSlots(int count)
        {
            if (slotPrefab == null || slotContainer == null) return;
            while (_slots.Count < count)
                _slots.Add(Instantiate(slotPrefab, slotContainer));
        }
    }
}
