using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// One fruit among several growing on a shared, immovable branch/trunk (see <see cref="FruitCluster"/>).
/// Detaching and going physical on release is already handled by <see cref="EnableGravityOnGrab"/>
/// (present on every fruit) - this just reports back to the cluster once that release happens, so it
/// can clean up the (now empty) branch once every fruit has been picked.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class FruitGrab : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private FruitCluster cluster;
    private bool _notified;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        cluster = GetComponentInParent<FruitCluster>();
    }

    private void OnEnable()
    {
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    private void OnDisable()
    {
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        // Only the first release matters - once detached, later re-grabs of the same free-standing
        // fruit shouldn't count again.
        if (_notified) return;
        _notified = true;
        if (cluster != null) cluster.NotifyFruitPicked();
    }
}
