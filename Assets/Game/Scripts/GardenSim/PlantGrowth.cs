using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class GrowthCondition
{
    public string name;
    public bool isCommited;
}

[System.Serializable]
public class GrowthStage
{
    public GameObject prefab;
    public int growthTime;

    public GrowthCondition[] contitions;

    public Color stageColor = Color.white;
}
public class PlantGrowth : MonoBehaviour
{
    [Header("Growth Prefabs")]
    [SerializeField] private GrowthStage[] growthStages;

    [Header("Animation")]
    [SerializeField] private float transitionDuration = 0.7f;
    [SerializeField] private float undergroundOffset = 0.3f;

    [Header("UI")]
    [SerializeField] private Image progressCircle;



    private int currentStage;
    private GameObject currentPlantInstance;
    private Coroutine growCoroutine;

    /// <summary>The currently active growth-stage instance (read-only; used by pest/infestation hooks).</summary>
    public GameObject CurrentPlantInstance => currentPlantInstance;

    /// <summary>Index of the current growth stage (read-only; used by pest/infestation hooks).</summary>
    public int CurrentStageIndex => currentStage;

    /// <summary>True once the plant has reached its final configured stage (growth coroutine finished).</summary>
    public bool HasFinishedGrowing => currentStage >= growthStages.Length - 1;

    private void Start()
    {
        SpawnInitialStage();
        growCoroutine = StartCoroutine(GrowPlant());
    }

    private void SpawnInitialStage()
    {
        if (growthStages[0].prefab != null)
        {
            Collider col = GetComponent<Collider>();
            Vector3 spawnPos = new Vector3(
                col.bounds.center.x,
                col.bounds.max.y,
                col.bounds.center.z
            );

            currentPlantInstance = Instantiate(
               growthStages[0].prefab,
               spawnPos,
               transform.rotation,
               transform
           );
        }
    }

    private IEnumerator GrowPlant()
    {
        while (currentStage < growthStages.Length - 1)
        {
            // Wait until all conditions are fulfilled
            yield return new WaitUntil(() =>
    AreGrowthConditionsMet(growthStages[currentStage]));

            float timer = 0f;
            progressCircle.color = growthStages[currentStage].stageColor;

            while (timer < growthStages[currentStage].growthTime)
            {
                timer += Time.deltaTime;

                if (progressCircle != null)
                    progressCircle.fillAmount = timer / growthStages[currentStage].growthTime;

                yield return null;
            }

            progressCircle.fillAmount = 0f;
            yield return StartCoroutine(TransitionToNextStage());

            if (progressCircle != null)
                progressCircle.fillAmount = 0f;
        }
    }

    private IEnumerator TransitionToNextStage()
    {
        GameObject oldPlant = currentPlantInstance;

        currentStage++;

        if (currentStage == 1)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }

            // Collider parentCollider = GetComponent<Collider>();
            // if (parentCollider != null)
            //     parentCollider.enabled = false;
        }

        Collider col = GetComponent<Collider>();

        // Position at the top of the socket/collider
        Vector3 topPos = new Vector3(
            col.bounds.center.x,
            col.bounds.max.y,
            col.bounds.center.z
        );

        // Start slightly below the surface
        Vector3 spawnPos = topPos + Vector3.down * undergroundOffset;

        GameObject newPlant = Instantiate(
            growthStages[currentStage].prefab,
            spawnPos,
            Quaternion.identity,
            transform
        );

        BoxCollider newPlantBoxCollider = newPlant.GetComponent<BoxCollider>();
        if (newPlantBoxCollider != null)
            newPlantBoxCollider.enabled = false;

        // Freeze physics on both stage instances while their position is driven by script below.
        // Otherwise gravity keeps accumulating velocity on the non-kinematic Rigidbody underneath
        // the scripted position, and the moment the animation lets go the plant lurches/sinks into
        // the ground instead of resting on it.
        Rigidbody oldPlantRb = oldPlant.GetComponent<Rigidbody>();
        if (oldPlantRb != null)
        {
            oldPlantRb.velocity = Vector3.zero;
            oldPlantRb.angularVelocity = Vector3.zero;
            oldPlantRb.isKinematic = true;
        }

        Rigidbody newPlantRb = newPlant.GetComponent<Rigidbody>();
        if (newPlantRb != null)
        {
            newPlantRb.velocity = Vector3.zero;
            newPlantRb.angularVelocity = Vector3.zero;
            newPlantRb.isKinematic = true;
        }

        Vector3 oldStartPos = oldPlant.transform.position;
        Vector3 oldEndPos = oldStartPos + Vector3.down * undergroundOffset;

        float t = 0f;

        while (t < transitionDuration)
        {
            t += Time.deltaTime;

            float eased = Mathf.SmoothStep(0f, 1f, t / transitionDuration);

            oldPlant.transform.position = Vector3.Lerp(oldStartPos, oldEndPos, eased);
            newPlant.transform.position = Vector3.Lerp(spawnPos, topPos, eased);

            yield return null;
        }

        // Ensure exact final positions
        oldPlant.transform.position = oldEndPos;
        newPlant.transform.position = topPos;

        Destroy(oldPlant);

        if (newPlantBoxCollider != null)
            newPlantBoxCollider.enabled = true;

        // Deliberately left kinematic: this stage instance stays parented under this (also
        // kinematic, non-colliding) PlantGrowth root until it's harvested. A non-kinematic
        // Rigidbody nested under another Rigidbody's hierarchy is not something Unity's physics
        // supports properly — it stops colliding with the environment and free-falls forever.
        // PlantStageGrab.OnGrabbed() unparents the object AND flips this back to non-kinematic,
        // which is the point it actually needs physics (being carried/dropped by the player).
        currentPlantInstance = newPlant;
    }

    private bool AreGrowthConditionsMet(GrowthStage stage)
    {

        if (stage.contitions == null || stage.contitions.Length == 0)
            return true;

        foreach (GrowthCondition condition in stage.contitions)
        {
            if (!condition.isCommited)
                return false;
        }

        return true;
    }

    public void TakeOut()
    {
        // Stop the actual running coroutine (StopCoroutine(TransitionToNextStage()) previously just
        // built and stopped a brand-new, never-started enumerator - a no-op - so GrowPlant() kept
        // running and could still tick growthStages[currentStage] this same frame, before the
        // deferred Destroy() from PlantStageGrab actually took effect, indexing past the array once
        // currentStage was bumped below).
        if (growCoroutine != null) StopCoroutine(growCoroutine);

        // Valid last index, not one past it - keeps HasFinishedGrowing true without ever risking an
        // out-of-bounds read if anything still queries growthStages[currentStage] afterwards.
        currentStage = growthStages.Length - 1;
    }

    public void SetConditionCommitted(string targetName)
    {
        GrowthCondition[] conditions = growthStages[currentStage].contitions;

        for (int i = 0; i < conditions.Length; i++)
        {
            if (conditions[i].name == targetName)
            {
                conditions[i].isCommited = true;
            }
        }
    }
}