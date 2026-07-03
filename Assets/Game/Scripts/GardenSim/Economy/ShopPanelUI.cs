using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GardenSim
{
    /// <summary>
    /// World-space shop UI. Built from a <see cref="ShopCatalog"/> (one <see cref="ShopRowUI"/> per item)
    /// and driven by the <see cref="Inventory"/>. Shown/hidden by the <see cref="Vendor"/>.
    /// </summary>
    public class ShopPanelUI : MonoBehaviour
    {
        [Tooltip("The visual that is toggled on/off. If empty, this GameObject is used.")]
        [SerializeField] private GameObject root;
        [SerializeField] private Inventory inventory;
        [SerializeField] private Transform rowContainer;
        [SerializeField] private ShopRowUI rowPrefab;
        [SerializeField] private TMP_Text coinsLabel;

        private readonly List<ShopRowUI> _rows = new List<ShopRowUI>();
        private ShopCatalog _catalog;
        private Vendor _vendor;
        private bool _subscribed;

        public bool IsOpen => root != null && root.activeSelf;

        private void Awake()
        {
            if (root == null) root = gameObject;
        }

        /// <summary>Rebuilds the rows for the given catalog. Called by the vendor on start.</summary>
        public void Bind(ShopCatalog catalog, Vendor vendor = null)
        {
            _catalog = catalog;
            _vendor = vendor;
            if (inventory == null) inventory = Inventory.Instance;
            BuildRows();
        }

        public void Open()
        {
            if (inventory == null) inventory = Inventory.Instance;
            if (root != null) root.SetActive(true);
            Subscribe();
            RefreshRows();
        }

        public void Close()
        {
            Unsubscribe();
            if (root != null) root.SetActive(false);
        }

        private void OnDestroy() => Unsubscribe();

        private void BuildRows()
        {
            foreach (var row in _rows) if (row != null) Destroy(row.gameObject);
            _rows.Clear();

            if (_catalog == null || rowPrefab == null || rowContainer == null) return;

            foreach (var item in _catalog.items)
            {
                if (item == null) continue;
                var row = Instantiate(rowPrefab, rowContainer);
                row.Bind(item, OnBuy, OnSell);
                _rows.Add(row);
            }
        }

        private void OnBuy(ItemDefinition item)
        {
            // Prefer the vendor path so purchases can materialize on the counter.
            if (_vendor != null) _vendor.TryPurchase(item);
            else inventory?.Buy(item);
        }

        private void OnSell(ItemDefinition item) => inventory?.Sell(item);

        private void RefreshRows()
        {
            if (coinsLabel != null && inventory != null) coinsLabel.text = inventory.Coins.ToString();
            foreach (var row in _rows) row.Refresh(inventory);
        }

        private void Subscribe()
        {
            if (_subscribed || inventory == null) return;
            inventory.OnChanged += RefreshRows;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed || inventory == null) return;
            inventory.OnChanged -= RefreshRows;
            _subscribed = false;
        }
    }
}
