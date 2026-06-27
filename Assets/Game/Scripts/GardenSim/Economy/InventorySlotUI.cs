using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GardenSim
{
    /// <summary>One row in the inventory info panel: an icon plus "Name xN".</summary>
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text label;

        public void Set(ItemDefinition item, int count)
        {
            if (icon != null)
            {
                icon.sprite = item != null ? item.icon : null;
                icon.enabled = icon.sprite != null;
            }

            if (label != null)
                label.text = item != null ? $"{item.DisplayName}   x{count}" : string.Empty;
        }
    }
}
