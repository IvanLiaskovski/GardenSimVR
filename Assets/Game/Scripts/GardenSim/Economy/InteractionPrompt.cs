using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace GardenSim
{
    /// <summary>
    /// Shows a small world-space label that tells the player what to do with an object, e.g.
    /// "Trigger: Trade" or "Grip: Pick up". The label can appear on hover, while the player is
    /// holding produce, or always. It billboards (yaw only) toward the player so it stays readable.
    /// </summary>
    public class InteractionPrompt : MonoBehaviour
    {
        public enum Mode { OnHover, WhileHoldingProduce, WhileHoldingSeed, Always, HoverWithSeedInInventory }

        [SerializeField] private Mode mode = Mode.OnHover;

        [Tooltip("Interactable whose hover state drives the prompt (OnHover mode). Defaults to this object's interactable.")]
        [SerializeField] private XRBaseInteractable interactable;

        [Tooltip("Root object of the label to show/hide.")]
        [SerializeField] private GameObject label;
        [SerializeField] private TMP_Text labelText;

        [TextArea]
        [SerializeField] private string message = "Interact";

        [Tooltip("Rotate the label to face the player each frame.")]
        [SerializeField] private bool faceCamera = true;

        private Transform _cam;
        private bool _hovering;

        private void Awake()
        {
            if (interactable == null) interactable = GetComponent<XRBaseInteractable>();
            if (labelText != null) labelText.text = message;
            Show(mode == Mode.Always);
        }

        private void OnEnable()
        {
            if ((mode == Mode.OnHover || mode == Mode.HoverWithSeedInInventory) && interactable != null)
            {
                interactable.hoverEntered.AddListener(OnHoverEntered);
                interactable.hoverExited.AddListener(OnHoverExited);
            }
        }

        private void OnDisable()
        {
            if ((mode == Mode.OnHover || mode == Mode.HoverWithSeedInInventory) && interactable != null)
            {
                interactable.hoverEntered.RemoveListener(OnHoverEntered);
                interactable.hoverExited.RemoveListener(OnHoverExited);
            }
        }

        private void OnHoverEntered(HoverEnterEventArgs _)
        {
            _hovering = true;
            if (mode == Mode.OnHover) Show(true);
        }

        private void OnHoverExited(HoverExitEventArgs _)
        {
            _hovering = false;
            if (mode == Mode.OnHover) Show(false);
        }

        private void Update()
        {
            if (mode == Mode.WhileHoldingProduce)
                Show(AnyProduceHeld());
            else if (mode == Mode.WhileHoldingSeed)
                Show(AnyPlantSeedHeld());
            else if (mode == Mode.HoverWithSeedInInventory)
                Show(_hovering && AnySeedInInventory());

            if (faceCamera && label != null && label.activeSelf)
            {
                if (_cam == null && Camera.main != null) _cam = Camera.main.transform;
                if (_cam != null)
                {
                    Vector3 dir = label.transform.position - _cam.position; // readable (-forward) faces the player
                    dir.y = 0f;
                    if (dir.sqrMagnitude > 0.0001f)
                        label.transform.rotation = Quaternion.LookRotation(dir);
                }
            }
        }

        private static bool AnyProduceHeld()
        {
            foreach (var p in FindObjectsOfType<ProduceItemRef>())
            {
                var g = p.GetComponent<XRGrabInteractable>();
                if (g != null && g.isSelected) return true;
            }
            return false;
        }

        /// <summary>True while the player holds a plantable seed (a PlantGrowth root, e.g. Carrot/
        /// Tomato/MushroomCycle) that hasn't been placed into a GrowBox yet.</summary>
        private static bool AnyPlantSeedHeld()
        {
            foreach (var p in FindObjectsOfType<PlantGrowth>())
            {
                var g = p.GetComponent<XRGrabInteractable>();
                if (g != null && g.isSelected) return true;
            }
            return false;
        }

        /// <summary>True while the player's inventory holds at least one plantable seed item.</summary>
        private static bool AnySeedInInventory()
        {
            var inventory = Inventory.Instance;
            if (inventory == null) return false;
            foreach (var kvp in inventory.Counts)
                if (kvp.Key != null && kvp.Key.isSeed && kvp.Value > 0) return true;
            return false;
        }

        private void Show(bool visible)
        {
            if (label != null && label.activeSelf != visible) label.SetActive(visible);
        }
    }
}
