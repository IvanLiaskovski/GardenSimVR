using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace GardenSim
{
    /// <summary>
    /// Collect-on-grab: as soon as the player grabs this produce it is added to the
    /// <see cref="Inventory"/> and the physical object is removed. No basket / drop step needed.
    /// Pair with <see cref="ProduceItemRef"/> and an <c>XRGrabInteractable</c>.
    /// </summary>
    [RequireComponent(typeof(ProduceItemRef))]
    public class CollectOnGrab : MonoBehaviour
    {
        [Tooltip("Optional sound played when the crop is collected.")]
        [SerializeField] private AudioSource collectSound;

        private ProduceItemRef _produce;
        private XRGrabInteractable _grab;
        private bool _collected;

        private void Awake()
        {
            _produce = GetComponent<ProduceItemRef>();
            _grab = GetComponent<XRGrabInteractable>();
        }

        private void OnEnable()
        {
            if (_grab != null) _grab.selectEntered.AddListener(OnGrabbed);
        }

        private void OnDisable()
        {
            if (_grab != null) _grab.selectEntered.RemoveListener(OnGrabbed);
        }

        private void OnGrabbed(SelectEnterEventArgs _) => Collect();

        /// <summary>Adds this produce to the inventory and removes the object. Safe to call once.</summary>
        public void Collect()
        {
            if (_collected || _produce == null || _produce.definition == null) return;
            if (Inventory.Instance == null)
            {
                Debug.LogWarning("CollectOnGrab: no Inventory in scene; cannot collect.", this);
                return;
            }

            _collected = true;
            Inventory.Instance.Add(_produce.definition, _produce.quantity);
            if (collectSound != null) collectSound.Play();
            Destroy(gameObject); // deferred to end of frame; XRI handles the interactable removal
        }
    }
}
