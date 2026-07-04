using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class WateringCan : MonoBehaviour
{
    [SerializeField] private ParticleSystem waterParticles;
    private XRGrabInteractable grabInteractable;

    private bool isWatering = false;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    void OnEnable()
    {
        grabInteractable.activated.AddListener(OnActivate);
        grabInteractable.deactivated.AddListener(OnDeactivate);
    }

    void OnDisable()
    {
        grabInteractable.activated.RemoveListener(OnActivate);
        grabInteractable.deactivated.RemoveListener(OnDeactivate);
    }

    private void OnActivate(ActivateEventArgs args)
    {
        isWatering = true;
        waterParticles.Play();
    }

    private void OnDeactivate(DeactivateEventArgs args)
    {
        isWatering = false;
        waterParticles.Stop();
    }
}