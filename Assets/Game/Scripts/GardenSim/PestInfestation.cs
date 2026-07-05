using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attaches to a <see cref="PlantGrowth"/> root. Once the plant has been watered (its 3rd growth
/// phase onward, before it reaches its harvestable stage, detected via the current stage prefab
/// carrying a <see cref="PlantStageGrab"/>), periodically rolls a chance to become infested with bugs.
/// Once infested, the player must press the swarm's "Clear Bugs" button before the death timer runs
/// out, or the plant is destroyed and replaced with <see cref="deadPrefab"/>.
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
    [Tooltip("How far in front of the plant (along its forward direction) the Clear Bugs button appears, so it isn't hidden behind/inside the plant's own foliage.")]
    [SerializeField] private float swarmForwardOffset = 0.3f;

    private PlantGrowth _plantGrowth;
    private GameObject _activeSwarm;
    private Image _timerRing;
    private TMP_Text _timerText;
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
        // Bugs only start showing up once the plant has been watered (3rd growth phase onward).
        if (IsInfested || _plantGrowth.CurrentStageIndex < 2 || IsHarvestableOrBeyond()) return;

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

        Vector3 spawnPos = transform.position + Vector3.up * swarmHeightOffset + transform.forward * swarmForwardOffset;
        _activeSwarm = Instantiate(bugSwarmPrefab, spawnPos, Quaternion.identity);

        var button = _activeSwarm.GetComponentInChildren<Button>(true);
        if (button != null) button.onClick.AddListener(ClearBugs);

        var timerDisplay = _activeSwarm.transform.Find("ClearButton/TimerDisplay");
        if (timerDisplay != null)
        {
            _timerRing = timerDisplay.Find("Ring")?.GetComponent<Image>();
            _timerText = timerDisplay.Find("Text")?.GetComponent<TMP_Text>();
        }

        _deathCoroutine = StartCoroutine(DeathTimer());
    }

    private IEnumerator DeathTimer()
    {
        float remaining = timeToDeath;
        UpdateTimerDisplay(remaining);

        while (remaining > 0f)
        {
            yield return null;
            remaining -= Time.deltaTime;
            UpdateTimerDisplay(remaining);
        }

        Kill();
    }

    /// <summary>Updates the red countdown ring/label on the active swarm, if present.</summary>
    private void UpdateTimerDisplay(float remaining)
    {
        remaining = Mathf.Max(0f, remaining);
        if (_timerRing != null) _timerRing.fillAmount = timeToDeath > 0f ? remaining / timeToDeath : 0f;
        if (_timerText != null) _timerText.text = Mathf.CeilToInt(remaining).ToString();
    }

    /// <summary>Removes the current infestation. Wired to the swarm's "Clear Bugs" button.</summary>
    public void ClearBugs()
    {
        if (!IsInfested) return;
        if (_deathCoroutine != null) StopCoroutine(_deathCoroutine);
        Destroy(_activeSwarm);
        _activeSwarm = null;
        _timerRing = null;
        _timerText = null;
    }

    private void Kill()
    {
        if (deadPrefab != null)
        {
            // Match PlantGrowth's own stage-spawning height (top of the root's collider), not the
            // root's raw transform.position - the visible plant sits well above that, so spawning at
            // the root position buried the dead prop in the ground/pot instead of showing it in place.
            Vector3 spawnPos = transform.position;
            var col = GetComponent<Collider>();
            if (col != null)
                spawnPos = new Vector3(col.bounds.center.x, col.bounds.max.y, col.bounds.center.z);

            Instantiate(deadPrefab, spawnPos, transform.rotation);
        }

        if (_activeSwarm != null) Destroy(_activeSwarm);
        Destroy(gameObject); // stops PlantGrowth's coroutine too, matching the harvest-destroy pattern
    }
}
