using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace GardenSim
{
    /// <summary>
    /// A bin the player can drop dead/spent produce into to get rid of it. Anything whose name
    /// contains one of <see cref="disposableNameContains"/> (e.g. the "Dead"/"PestDead" props) is
    /// destroyed as soon as it comes to rest inside the trigger volume; live produce, seeds, and
    /// anything still being held are left alone.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class TrashCan : MonoBehaviour
    {
        [Tooltip("An object is disposable if its name contains any of these substrings (case-insensitive).")]
        [SerializeField] private string[] disposableNameContains = { "Dead" };

        [Tooltip("Optional sound played each time something is thrown away.")]
        [SerializeField] private AudioSource disposeSound;

        private void Reset()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }

        private void OnTriggerStay(Collider other)
        {
            var root = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
            if (!IsDisposable(root) || IsHeld(root)) return;

            if (disposeSound != null) disposeSound.Play();
            root.SetActive(false);
            Destroy(root);
        }

        private bool IsDisposable(GameObject go)
        {
            foreach (var token in disposableNameContains)
            {
                if (go.name.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            return false;
        }

        private static bool IsHeld(GameObject go)
        {
            var grab = go.GetComponent<XRGrabInteractable>();
            return grab != null && grab.isSelected;
        }
    }
}
