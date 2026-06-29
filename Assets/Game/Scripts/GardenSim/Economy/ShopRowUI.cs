using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GardenSim
{
    /// <summary>
    /// One row in the shop panel for a single item: icon, name, owned count, and Buy / Sell buttons.
    /// Buttons are disabled (greyed) when the action is not currently possible.
    /// </summary>
    public class ShopRowUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text ownedLabel;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button sellButton;
        [SerializeField] private TMP_Text buyButtonLabel;
        [SerializeField] private TMP_Text sellButtonLabel;

        private ItemDefinition _item;
        private Action<ItemDefinition> _onBuy;
        private Action<ItemDefinition> _onSell;

        public void Bind(ItemDefinition item, Action<ItemDefinition> onBuy, Action<ItemDefinition> onSell)
        {
            _item = item;
            _onBuy = onBuy;
            _onSell = onSell;

            if (icon != null) { icon.sprite = item.icon; icon.enabled = item.icon != null; }
            if (nameLabel != null) nameLabel.text = item.DisplayName;
            if (buyButtonLabel != null) buyButtonLabel.text = $"Buy  {item.buyPrice}";
            if (sellButtonLabel != null) sellButtonLabel.text = $"Sell  {item.sellPrice}";

            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(() => _onBuy?.Invoke(_item));
                buyButton.gameObject.SetActive(item.canBuy);
            }

            if (sellButton != null)
            {
                sellButton.onClick.RemoveAllListeners();
                sellButton.onClick.AddListener(() => _onSell?.Invoke(_item));
                sellButton.gameObject.SetActive(item.canSell);
            }
        }

        public void Refresh(Inventory inventory)
        {
            int owned = inventory != null ? inventory.GetCount(_item) : 0;
            if (ownedLabel != null) ownedLabel.text = $"Owned: {owned}";
            if (buyButton != null) buyButton.interactable = inventory != null && inventory.CanBuy(_item);
            if (sellButton != null) sellButton.interactable = inventory != null && inventory.CanSell(_item);
        }
    }
}
