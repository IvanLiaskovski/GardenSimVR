using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace GardenSim
{
    /// <summary>
    /// A merchant the player interacts with to open a shop. Attach to (or alongside) an
    /// <see cref="XRBaseInteractable"/> used as the "Trade" trigger — poking or grabbing it toggles the
    /// <see cref="ShopPanelUI"/>. Optionally auto-closes when the player walks away.
    /// </summary>
    public class Vendor : MonoBehaviour
    {
        [SerializeField] private ShopCatalog catalog;
        [SerializeField] private ShopPanelUI shopPanel;

        [Tooltip("Interactable the player pokes/grabs to toggle the shop. If empty, looks on this object.")]
        [SerializeField] private XRBaseInteractable tradeInteractable;

        [Tooltip("Where purchased goods (seeds) physically appear. If empty, purchases go straight to the inventory.")]
        [SerializeField] private Transform purchaseSpawnPoint;

        [Tooltip("Close the shop when the player gets further than this from the vendor. 0 = never auto-close.")]
        [SerializeField] private float autoCloseDistance = 0f;

        private Transform _player;

        private void Awake()
        {
            if (tradeInteractable == null) tradeInteractable = GetComponent<XRBaseInteractable>();
        }

        private void OnEnable()
        {
            if (tradeInteractable != null) tradeInteractable.selectEntered.AddListener(OnTradeSelected);
        }

        private void OnDisable()
        {
            if (tradeInteractable != null) tradeInteractable.selectEntered.RemoveListener(OnTradeSelected);
        }

        private void Start()
        {
            if (shopPanel != null)
            {
                shopPanel.Bind(catalog, this);
                shopPanel.Close();
            }

            var cam = Camera.main;
            if (cam != null) _player = cam.transform;
        }

        private void Update()
        {
            if (autoCloseDistance <= 0f || shopPanel == null || !shopPanel.IsOpen || _player == null)
                return;

            if (Vector3.Distance(_player.position, transform.position) > autoCloseDistance)
                shopPanel.Close();
        }

        private void OnTradeSelected(SelectEnterEventArgs _) => Toggle();

        /// <summary>Open the shop if closed, close it if open. Also hookable from UnityEvents/buttons.</summary>
        public void Toggle()
        {
            if (shopPanel == null) return;
            if (shopPanel.IsOpen) shopPanel.Close();
            else shopPanel.Open();
        }

        /// <summary>
        /// Buys one unit of the item. When the item has a physical prefab and this vendor has a
        /// purchase spawn point, the goods appear on the counter for the player to pick up instead of
        /// being added to the inventory directly (seeds collect themselves on grab).
        /// </summary>
        public bool TryPurchase(ItemDefinition item)
        {
            var inventory = Inventory.Instance;
            if (item == null || !item.canBuy || inventory == null) return false;

            if (item.worldPrefab != null && purchaseSpawnPoint != null)
            {
                if (!inventory.TrySpend(item.buyPrice)) return false;
                Vector3 jitter = new Vector3(Random.Range(-0.15f, 0.15f), 0.05f, Random.Range(-0.08f, 0.08f));
                Instantiate(item.worldPrefab, purchaseSpawnPoint.position + jitter, Quaternion.identity);
                return true;
            }

            return inventory.Buy(item);
        }
    }
}
