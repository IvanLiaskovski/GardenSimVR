using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlantGrowth : MonoBehaviour
{
    [Header("Growth Prefabs")]
    [SerializeField] private GameObject[] growthStagePrefabs;

    [Header("Timing")]
    [SerializeField] private float timePerStage = 10f;

    [Header("Animation")]
    [SerializeField] private float transitionDuration = 0.7f;
    [SerializeField] private float undergroundOffset = 0.3f;

    [Header("UI")]
    [SerializeField] private Image progressCircle;

    private int currentStage;
    private GameObject currentPlantInstance;

    private void Start()
    {
        SpawnInitialStage();
        StartCoroutine(GrowPlant());
    }

    private void SpawnInitialStage()
    {
        currentPlantInstance = Instantiate(
            growthStagePrefabs[0],
            transform.position,
            transform.rotation,
            transform
        );
    }

    private IEnumerator GrowPlant()
    {
        while (currentStage < growthStagePrefabs.Length - 1)
        {
            float timer = 0f;

            while (timer < timePerStage)
            {
                timer += Time.deltaTime;

                if (progressCircle != null)
                    progressCircle.fillAmount = timer / timePerStage;

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

        Vector3 spawnPos = transform.position + Vector3.down * undergroundOffset;

        GameObject newPlant = Instantiate(
            growthStagePrefabs[currentStage],
            spawnPos,
            transform.rotation,
            transform
        );

        Vector3 oldStartPos = oldPlant.transform.position;
        Vector3 oldEndPos = oldStartPos + Vector3.down * undergroundOffset;

        Vector3 newStartPos = newPlant.transform.position;
        Vector3 newEndPos = transform.position;

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

        currentPlantInstance = newPlant;
    }
}