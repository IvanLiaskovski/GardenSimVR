using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attaches to a <see cref="PlantGrowth"/> root. While the plant is actively growing (before it
/// reaches its harvestable stage, detected via the current stage prefab carrying a
/// <see cref="PlantStageGrab"/>), periodically rolls a chance to become infested with bugs. Once
/// infested, the player must press the swarm's "Clear Bugs" button before the death timer runs out,
/// or the plant is destroyed and replaced with <see cref="deadPrefab"/>.
/// </summary>
[RequireComponent(typeof(PlantGrowth))]
public class PestInfestation : MonoBehaviour
{
    [Header("Infestation odds")]
    [Tooltip("How often (seconds) to roll for a new infestation while the plant is growing.")]
    [SerializeField] private float checkInterval = 4f;
    [Tooltip("Chance [0-1] of becoming infested on each roll.")]
    [SerializeField] private float chancePerCheck = 0.15f;

    [Header("Consequence")]
    [Tooltip("Seconds the player has to clear the bugs before the plant dies.")]
    [SerializeField] private float timeToDeath = 12f;
    [Tooltip("Prop instantiated in place of the plant if the infestation isn't cleared in time.")]
    [SerializeField] private GameObject deadPrefab;

    [Header("Visual")]
    [SerializeField] private GameObject bugSwarmPrefab;
    [Tooltip("Local vertical offset from the plant's root position where the swarm/button appear.")]
    [SerializeField] private float swarmHeightOffset = 0.25f;

    private PlantGrowth _plantGrowth;
    private GameObject _activeSwarm;
    private Coroutine _deathCoroutine;
    private float _checkTimer;

    /// <summary>True while a bug swarm is currently active on this plant.</summary>
    public bool IsInfested => _activeSwarm != null;

    private void Awake()
    {
        _plantGrowth = GetComponent<PlantGrowth>();
    }

    private void Update()
    {
        if (IsInfested || IsHarvestableOrBeyond()) return;

        _checkTimer += Time.deltaTime;
        if (_checkTimer < checkInterval) return;
        _checkTimer = 0f;

        if (Random.value < chancePerCheck) Infest();
    }

    private bool IsHarvestableOrBeyond()
    {
        if (_plantGrowth.HasFinishedGrowing) return true;
        var current = _plantGrowth.CurrentPlantInstance;
        return current == null || current.GetComponent<PlantStageGrab>() != null;
    }

    private void Infest()
    {
        if (bugSwarmPrefab == null) return;

        Vector3 spawnPos = transform.position + Vector3.up * swarmHeightOffset;
        _activeSwarm = Instantiate(bugSwarmPrefab, spawnPos, Quaternion.identity);

        var button = _activeSwarm.GetComponentInChildren<Button>(true);
        if (button != null) button.onClick.AddListener(ClearBugs);

        _deathCoroutine = StartCoroutine(DeathTimer());
    }

    private IEnumerator DeathTimer()
    {
        yield return new WaitForSeconds(timeToDeath);
        Kill();
    }

    /// <summary>Removes the current infestation. Wired to the swarm's "Clear Bugs" button.</summary>
    public void ClearBugs()
    {
        if (!IsInfested) return;
        if (_deathCoroutine != null) StopCoroutine(_deathCoroutine);
        Destroy(_activeSwarm);
        _activeSwarm = null;
    }

    private void Kill()
    {
        if (deadPrefab != null)
            Instantiate(deadPrefab, transform.position, transform.rotation);

        if (_activeSwarm != null) Destroy(_activeSwarm);
        Destroy(gameObject); // stops PlantGrowth's coroutine too, matching the harvest-destroy pattern
    }
}
