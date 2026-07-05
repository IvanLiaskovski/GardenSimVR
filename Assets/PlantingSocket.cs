using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using GardenSim;

public class PlantingSocket : MonoBehaviour
{
    private XRSocketInteractor socket;

    private PlantGrowth currentPlant;

    [SerializeField] private Collider solidCollider;

    [Tooltip("Interactable whose select (Grip) triggers planting a seed from the player's inventory. Defaults to this object's XRSimpleInteractable.")]
    [SerializeField] private XRBaseInteractable plantTrigger;

    private void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
        if (plantTrigger == null) plantTrigger = GetComponent<XRSimpleInteractable>();
    }

    private void OnEnable()
    {
        socket.selectEntered.AddListener(OnSeedPlaced);
        if (plantTrigger != null) plantTrigger.selectEntered.AddListener(OnPlantTriggerSelected);
    }

    private void OnDisable()
    {
        socket.selectEntered.RemoveListener(OnSeedPlaced);
        if (plantTrigger != null) plantTrigger.selectEntered.RemoveListener(OnPlantTriggerSelected);
    }

    private void OnPlantTriggerSelected(SelectEnterEventArgs args) => PlantFromInventory();

    /// <summary>Consumes one matching seed from the player's inventory and plants it here, if the box is empty.</summary>
    public void PlantFromInventory()
    {
        if (currentPlant != null) return;

        var inventory = Inventory.Instance;
        if (inventory == null) return;

        foreach (var kvp in inventory.Counts)
        {
            var item = kvp.Key;
            if (item == null || !item.isSeed || kvp.Value <= 0 || item.plantPrefab == null) continue;
            if (!inventory.Remove(item, 1)) continue;

            Collider socketCollider = GetComponent<Collider>();
            Vector3 targetPos = new Vector3(
                socketCollider.bounds.center.x,
                socketCollider.bounds.max.y,
                socketCollider.bounds.center.z
            );

            GameObject instance = Instantiate(item.plantPrefab, targetPos, transform.rotation, transform);
            currentPlant = instance.GetComponent<PlantGrowth>();
            if (currentPlant != null) currentPlant.SetConditionCommitted("planted");
            break;
        }
    }

    private void OnSeedPlaced(SelectEnterEventArgs args)
    {
        Debug.Log("Seed Placed");

        Transform seed = args.interactableObject.transform;

        PlantGrowth plant = args.interactableObject.transform.GetComponent<PlantGrowth>();
        currentPlant = plant;
        if (plant != null) plant.SetConditionCommitted("planted");

        Collider socketCollider = GetComponent<Collider>();

        // top of the box in world space
        float topY = socketCollider.bounds.max.y;

        Vector3 targetPos = new Vector3(
            socketCollider.bounds.center.x,
            topY,
            socketCollider.bounds.center.z
        );

        seed.SetPositionAndRotation(
            targetPos,
            transform.rotation
        );
    }

    private void OnParticleCollision(GameObject other)
    {
        if (currentPlant == null) return;

        Debug.Log("Plant Watered!");
        currentPlant.SetConditionCommitted("watered");
        solidCollider.enabled = false;
    }
}