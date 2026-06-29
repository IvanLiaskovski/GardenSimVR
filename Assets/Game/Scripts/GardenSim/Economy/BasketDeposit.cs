using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace GardenSim
{
    /// <summary>
    /// A worn "basket" that absorbs released produce dropped into its trigger volume, adding it to the
    /// <see cref="Inventory"/> and destroying the physical object. Produce that is still being held is
    /// ignored, so the player can reach through the basket without losing what they are carrying.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BasketDeposit : MonoBehaviour
    {
        [Tooltip("Optional sound played when an item is deposited.")]
        [SerializeField] private AudioSource depositSound;

        private void Reset()
        {
            // Make the basket a trigger by default so produce can pass into it.
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other) => TryDeposit(other);
        private void OnTriggerStay(Collider other) => TryDeposit(other);

        private void TryDeposit(Collider other)
        {
            var produce = other.GetComponentInParent<ProduceItemRef>();
            if (produce == null || produce.definition == null) return;

            // Only absorb produce the player has let go of.
            var grab = produce.GetComponent<XRGrabInteractable>();
            if (grab != null && grab.isSelected) return;

            if (Inventory.Instance == null)
            {
                Debug.LogWarning("BasketDeposit: no Inventory present in the scene; cannot deposit.", this);
                return;
            }

            Inventory.Instance.Add(produce.definition, produce.quantity);
            if (depositSound != null) depositSound.Play();
            Destroy(produce.gameObject);
        }
    }
}
