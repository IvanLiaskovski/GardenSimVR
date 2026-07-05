using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace GardenSim
{
    /// <summary>
    /// The harvest crate: the player places harvested produce inside, then presses the crate's
    /// "Deposit" button to add everything currently in the crate to the <see cref="Inventory"/>.
    ///
    /// Detection scans the trigger volume with a physics overlap at deposit time (rather than
    /// relying on OnTriggerEnter/Exit bookkeeping), so items that were spawned, teleported, or
    /// nudged into the crate are always found. Items still being held by the player are skipped.
    /// Attach to a trigger BoxCollider covering the crate's interior and wire a UI Button to
    /// <see cref="DepositAll"/>.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class CrateDeposit : MonoBehaviour
    {
        [Tooltip("Optional sound played when items are deposited.")]
        [SerializeField] private AudioSource depositSound;

        private BoxCollider _volume;

        private void Awake()
        {
            _volume = GetComponent<BoxCollider>();

            var button = GetComponentInChildren<Button>(true);
            if (button != null) button.onClick.AddListener(DepositAll);
        }

        private void Reset()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }

        /// <summary>Distinct produce items currently resting inside the crate (not held).</summary>
        public List<ProduceItemRef> ItemsInside()
        {
            var found = new List<ProduceItemRef>();
            if (_volume == null) _volume = GetComponent<BoxCollider>();

            Vector3 center = transform.TransformPoint(_volume.center);
            Vector3 halfExtents = Vector3.Scale(_volume.size * 0.5f, transform.lossyScale);
            var hits = Physics.OverlapBox(center, halfExtents, transform.rotation);

            foreach (var hit in hits)
            {
                var produce = hit.GetComponentInParent<ProduceItemRef>();
                if (produce == null || produce.definition == null || found.Contains(produce)) continue;
                if (IsHeld(produce)) continue;
                found.Add(produce);
            }
            return found;
        }

        /// <summary>Number of items currently resting in the crate.</summary>
        public int CountInside() => ItemsInside().Count;

        /// <summary>Deposits every item resting in the crate into the inventory. Wire to the crate button.</summary>
        public void DepositAll()
        {
            if (Inventory.Instance == null)
            {
                Debug.LogWarning("CrateDeposit: no Inventory in the scene.", this);
                return;
            }

            var items = ItemsInside();
            foreach (var produce in items)
            {
                Inventory.Instance.Add(produce.definition, produce.quantity);
                // Deactivate first so the (end-of-frame) deferred Destroy can't let a second button
                // press in the meantime deposit the same item twice.
                produce.gameObject.SetActive(false);
                Destroy(produce.gameObject);
            }

            if (items.Count > 0 && depositSound != null) depositSound.Play();
        }

        private static bool IsHeld(ProduceItemRef produce)
        {
            var grab = produce.GetComponent<XRGrabInteractable>();
            return grab != null && grab.isSelected;
        }
    }
}
