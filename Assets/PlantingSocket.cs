using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PlantingSocket : MonoBehaviour
{
    private XRSocketInteractor socket;

    private PlantGrowth currentPlant;

    [SerializeField] private Collider solidCollider;

    private void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
    }

    private void OnEnable()
    {
        socket.selectEntered.AddListener(OnSeedPlaced);
    }

    private void OnDisable()
    {
        socket.selectEntered.RemoveListener(OnSeedPlaced);
    }

    private void OnSeedPlaced(SelectEnterEventArgs args)
    {
        Debug.Log("Seed Placed");

        Transform seed = args.interactableObject.transform;

        PlantGrowth plant = args.interactableObject.transform.GetComponent<PlantGrowth>();
        currentPlant = plant;
        plant.SetConditionCommitted("planted");

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


        Debug.Log("Plant Watered!");
        currentPlant.SetConditionCommitted("watered");
        solidCollider.enabled = false;
        //currentPlant.SetConditionCommitted("watered");
    }
}