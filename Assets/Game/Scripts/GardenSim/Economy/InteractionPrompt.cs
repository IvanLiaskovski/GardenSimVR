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
        public enum Mode { OnHover, WhileHoldingProduce, Always }

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

        private void Awake()
        {
            if (interactable == null) interactable = GetComponent<XRBaseInteractable>();
            if (labelText != null) labelText.text = message;
            Show(mode == Mode.Always);
        }

        private void OnEnable()
        {
            if (mode == Mode.OnHover && interactable != null)
            {
                interactable.hoverEntered.AddListener(OnHoverEntered);
                interactable.hoverExited.AddListener(OnHoverExited);
            }
        }

        private void OnDisable()
        {
            if (mode == Mode.OnHover && interactable != null)
            {
                interactable.hoverEntered.RemoveListener(OnHoverEntered);
                interactable.hoverExited.RemoveListener(OnHoverExited);
            }
        }

        private void OnHoverEntered(HoverEnterEventArgs _) => Show(true);
        private void OnHoverExited(HoverExitEventArgs _) => Show(false);

        private void Update()
        {
            if (mode == Mode.WhileHoldingProduce)
                Show(AnyProduceHeld());

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

        private void Show(bool visible)
        {
            if (label != null && label.activeSelf != visible) label.SetActive(visible);
        }
    }
}
