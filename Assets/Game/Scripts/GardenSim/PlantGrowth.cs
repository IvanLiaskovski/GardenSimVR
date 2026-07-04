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

    /// <summary>The currently active growth-stage instance (read-only; used by pest/infestation hooks).</summary>
    public GameObject CurrentPlantInstance => currentPlantInstance;

    /// <summary>True once the plant has reached its final configured stage (growth coroutine finished).</summary>
    public bool HasFinishedGrowing => currentStage >= growthStages.Length - 1;

    private void Start()
    {
        SpawnInitialStage();
        StartCoroutine(GrowPlant());
    }

    private void SpawnInitialStage()
    {
        if (growthStages[0].prefab != null)
        {
            currentPlantInstance = Instantiate(
                growthStages[0].prefab,
                transform.position,
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

            rb.isKinematic = true;
            rb.detectCollisions = false;
            GetComponent<Collider>().enabled = false;
        }



        Vector3 spawnPos = transform.position + Vector3.down * undergroundOffset;

        GameObject newPlant = Instantiate(
            growthStages[currentStage].prefab,
            spawnPos,
            transform.rotation,
            transform
        );

        Vector3 oldStartPos = oldPlant.transform.position;
        Vector3 oldEndPos = oldStartPos + Vector3.down * undergroundOffset;

        Vector3 newStartPos = newPlant.transform.position;
        Vector3 newEndPos = transform.position;

        BoxCollider newPlantBoxCollider = newPlant.GetComponent<BoxCollider>();

        if (newPlantBoxCollider)
            newPlantBoxCollider.enabled = false;

        float t = 0f;

        while (t < transitionDuration)
        {
            t += Time.deltaTime;

            float normalized = t / transitionDuration;

            // Smooth easing
            float eased = Mathf.SmoothStep(0f, 1f, normalized);

            oldPlant.transform.position =
                Vector3.Lerp(oldStartPos, oldEndPos, eased);

            newPlant.transform.position =
                Vector3.Lerp(newStartPos, newEndPos, eased);

            yield return null;
        }

        Destroy(oldPlant);

        if (newPlantBoxCollider)
            newPlantBoxCollider.enabled = true;

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
        currentStage = growthStages.Length;
        StopCoroutine(TransitionToNextStage());
    }
}