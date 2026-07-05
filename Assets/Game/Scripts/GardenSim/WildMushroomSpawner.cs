using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keeps a steady population of wild mushrooms (built from the PlantCycles mushroom prefab)
/// scattered at random points across a rectangular meadow area. Unlike a planted crop, a
/// wild mushroom's "planted" growth condition is committed immediately on spawn so it grows
/// unattended — no GrowBox/watering required. Harvesting one (via <see cref="PlantStageGrab"/>)
/// frees up its slot so a replacement grows back in, giving the player a steady, low-effort income
/// source alongside the planted economy.
/// </summary>
public class WildMushroomSpawner : MonoBehaviour
{
    [Tooltip("PlantGrowth root prefab to spawn (e.g. MushroomCycle from PlantCycles).")]
    [SerializeField] private GameObject mushroomPrefab;

    [Header("Spawn area (world space, XZ rectangle)")]
    [SerializeField] private Vector2 areaMin = new Vector2(7f, -17f);
    [SerializeField] private Vector2 areaMax = new Vector2(17.5f, -1.5f);
    [Tooltip("The meadow isn't perfectly flat, so ground height is found per-spawn via a downward raycast rather than assumed.")]
    [SerializeField] private float raycastHeight = 50f;
    [SerializeField] private float spawnClearance = 0.05f;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Population")]
    [Tooltip("How many wild mushrooms should be growing/alive at once.")]
    [SerializeField] private int maxActive = 6;
    [Tooltip("Seconds between checks to top the population back up.")]
    [SerializeField] private float spawnCheckInterval = 15f;
    [Tooltip("Minimum distance between two wild mushrooms so they don't overlap.")]
    [SerializeField] private float minSpacing = 2.5f;
    [Tooltip("Seconds to wait between each mushroom while growing the initial population, so they trickle in instead of appearing all at once.")]
    [SerializeField] private float initialSpawnInterval = 8f;

    private readonly List<GameObject> _active = new List<GameObject>();
    private float _timer;

    private void Start()
    {
        StartCoroutine(SpawnInitialPopulation());
    }

    private IEnumerator SpawnInitialPopulation()
    {
        for (int i = 0; i < maxActive; i++)
        {
            if (!TrySpawnOne()) break;
            if (i < maxActive - 1) yield return new WaitForSeconds(initialSpawnInterval);
        }
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < spawnCheckInterval) return;
        _timer = 0f;

        _active.RemoveAll(go => go == null);
        if (_active.Count < maxActive) TrySpawnOne();
    }

    private bool TrySpawnOne()
    {
        if (mushroomPrefab == null) return false;

        const int maxAttempts = 20;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float x = Random.Range(areaMin.x, areaMax.x);
            float z = Random.Range(areaMin.y, areaMax.y);

            if (!TryFindGround(x, z, out float groundY)) continue;

            Vector3 pos = new Vector3(x, groundY + spawnClearance, z);
            if (IsTooCloseToExisting(pos)) continue;

            var instance = Instantiate(mushroomPrefab, pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            var growth = instance.GetComponent<PlantGrowth>();
            if (growth != null) growth.SetConditionCommitted("planted");

            _active.Add(instance);
            return true;
        }
        return false;
    }

    private bool TryFindGround(float x, float z, out float groundY)
    {
        Vector3 origin = new Vector3(x, raycastHeight, z);
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
        {
            groundY = hit.point.y;
            return true;
        }
        groundY = 0f;
        return false;
    }

    private bool IsTooCloseToExisting(Vector3 pos)
    {
        foreach (var go in _active)
        {
            if (go == null) continue;
            if (Vector3.Distance(go.transform.position, pos) < minSpacing) return true;
        }
        return false;
    }
}
