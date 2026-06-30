using UnityEngine;
using UnityEngine.InputSystem;

namespace GardenSim
{
    /// <summary>
    /// Opens/closes the inventory panel. Press "M" (XR Device Simulator / desktop) or the controller's
    /// Primary button (the face button under the thumb, e.g. "A"/"X") — a single, unambiguous binding
    /// rather than also listening for the system Menu button, which is reserved by the OS on some
    /// headsets. When opened, the panel is placed in front of the player and faces them, so it is
    /// always visible wherever they are looking.
    /// </summary>
    public class InventoryToggle : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private bool startOpen = false;

        [Header("Placement when opened")]
        [SerializeField] private float distance = 1.3f;
        [SerializeField] private float heightOffset = -0.05f;

        [Tooltip("Optional extra action (e.g. a specific VR button). Keyboard M is always polled.")]
        [SerializeField] private InputActionProperty toggleAction;

        private InputAction _controllerAction;

        private void Start()
        {
            if (panel != null) panel.SetActive(startOpen);
            if (startOpen) PlaceInFront();

            // Note: InputActionProperty.action is never null (an unconfigured field still returns an
            // empty placeholder InputAction), so "configured" must be judged by whether it has bindings.
            bool hasOverride = toggleAction.action != null && toggleAction.action.bindings.Count > 0;
            if (hasOverride)
            {
                _controllerAction = toggleAction.action;
            }
            else
            {
                _controllerAction = new InputAction("ToggleInventory", InputActionType.Button);
                _controllerAction.AddBinding("<XRController>/primaryButton");
            }
            _controllerAction.performed += OnAction;
            _controllerAction.Enable();
        }

        private void OnDestroy()
        {
            if (_controllerAction == null) return;
            _controllerAction.performed -= OnAction;
            _controllerAction.Disable();
            bool hasOverride = toggleAction.action != null && toggleAction.action.bindings.Count > 0;
            if (!hasOverride) _controllerAction.Dispose();
        }

        private void Update()
        {
            // Polling the keyboard is robust in the simulator/desktop (no action-asset wiring needed).
            var kb = Keyboard.current;
            if (kb != null && kb.mKey.wasPressedThisFrame) Toggle();
        }

        private void OnAction(InputAction.CallbackContext _) => Toggle();

        /// <summary>Show the panel if hidden (placing it in front of the player), hide it if shown.</summary>
        public void Toggle()
        {
            if (panel == null) return;
            bool show = !panel.activeSelf;
            panel.SetActive(show);
            if (show) PlaceInFront();
        }

        private void PlaceInFront()
        {
            var cam = Camera.main;
            if (cam == null || panel == null) return;

            Transform t = cam.transform;
            Vector3 flat = new Vector3(t.forward.x, 0f, t.forward.z);
            if (flat.sqrMagnitude < 0.0001f) flat = t.forward;
            flat.Normalize();

            panel.transform.position = t.position + flat * distance + Vector3.up * heightOffset;
            Vector3 face = panel.transform.position - t.position; // readable (-forward) toward the player
            face.y = 0f;
            if (face.sqrMagnitude > 0.0001f)
                panel.transform.rotation = Quaternion.LookRotation(face);
        }
    }
}
