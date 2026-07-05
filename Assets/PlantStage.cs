using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PlantStageGrab : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private PlantGrowth plantGrowth;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        plantGrowth = GetComponentInParent<PlantGrowth>();
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrabbed);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        // Safe Unity-object null-check (compares the reference itself, not a member of it) - this
        // object stays grabbable/droppable like any normal item after harvest, so OnGrabbed fires
        // again on every later re-grab. Dereferencing plantGrowth.gameObject directly (the old check)
        // threw MissingReferenceException on the second grab, since the harvest below already
        // destroyed plantGrowth's GameObject the first time.
        if (plantGrowth != null)
        {
            plantGrowth.TakeOut();
            Transform grabbedObject = transform;
            // Detach from parent
            grabbedObject.SetParent(null);

            // PlantGrowth keeps this stage's Rigidbody kinematic while it's nested under the
            // (also kinematic) growth root, since a physics-simulated Rigidbody doesn't work
            // correctly nested under another one. Now that it's a standalone object, hand
            // physics control back so it behaves like a normal grabbed/dropped item.
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;

            // Destroy the parent (which has PlantGrowth) - stops the growth process entirely.
            Destroy(plantGrowth.gameObject);
        }

        // One-shot: harvesting is done, this is now just a normal droppable/re-grabbable prop.
        // Remove this component so future grabs don't try to run harvest logic again.
        Destroy(this);
    }
}