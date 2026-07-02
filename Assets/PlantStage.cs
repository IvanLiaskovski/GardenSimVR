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
        if (plantGrowth.gameObject != null)
        {
            Debug.Log("Grab");
            plantGrowth.TakeOut();
            Transform grabbedObject = transform;
            // Detach from parent
            grabbedObject.SetParent(null);

            // Destroy the parent (which has PlantGrowth)
            Destroy(plantGrowth.gameObject);
        }
    }
}