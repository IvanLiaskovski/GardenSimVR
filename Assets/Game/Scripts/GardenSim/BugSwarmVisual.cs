using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Animates the child "bug" transforms of a spawned pest swarm: each bug orbits the swarm's center
/// at its own radius/speed/height with a little jitter and tumble, giving a buzzing-flies look.
/// Direct children whose name starts with "Bug" are treated as bugs; everything else (e.g. the
/// "Clear Bugs" button canvas) is left alone.
/// </summary>
public class BugSwarmVisual : MonoBehaviour
{
    private struct BugMotion
    {
        public Transform bug;
        public float radius;
        public float speed;
        public float heightSpeed;
        public float angle;
        public float spinSpeed;
    }

    [SerializeField] private float baseRadius = 0.05f;
    [SerializeField] private float radiusJitter = 0.02f;
    [SerializeField] private float baseSpeed = 3f;
    [SerializeField] private float speedJitter = 1.5f;
    [SerializeField] private float restHeight = 0.05f;
    [SerializeField] private float bobHeight = 0.02f;

    [Tooltip("The 'Clear Bugs' button canvas; kept facing the player since plants sit at different " +
             "world positions/orientations. Auto-found by name if left empty.")]
    [SerializeField] private Transform billboardTarget;

    private BugMotion[] _bugs;
    private Transform _cam;

    private void Start()
    {
        if (billboardTarget == null) billboardTarget = transform.Find("ClearButton");
        if (Camera.main != null) _cam = Camera.main.transform;

        var list = new List<BugMotion>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (!child.name.StartsWith("Bug")) continue;
            list.Add(new BugMotion
            {
                bug = child,
                radius = baseRadius + Random.Range(-radiusJitter, radiusJitter),
                speed = baseSpeed + Random.Range(-speedJitter, speedJitter),
                heightSpeed = Random.Range(2f, 5f),
                angle = Random.Range(0f, Mathf.PI * 2f),
                spinSpeed = Random.Range(120f, 280f) * (Random.value < 0.5f ? -1f : 1f)
            });
        }
        _bugs = list.ToArray();
    }

    private void Update()
    {
        if (_bugs == null) return;

        float t = Time.time;
        for (int i = 0; i < _bugs.Length; i++)
        {
            var b = _bugs[i];
            if (b.bug == null) continue;

            float angle = b.angle + t * b.speed;
            float bob = Mathf.Sin(t * b.heightSpeed) * bobHeight;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * b.radius;

            b.bug.localPosition = offset + Vector3.up * (restHeight + bob);
            b.bug.Rotate(Vector3.up, b.spinSpeed * Time.deltaTime, Space.Self);
        }

        if (billboardTarget != null && _cam != null)
        {
            Vector3 dir = billboardTarget.position - _cam.position; // readable (-forward) faces the player
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                billboardTarget.rotation = Quaternion.LookRotation(dir);
        }
    }
}
